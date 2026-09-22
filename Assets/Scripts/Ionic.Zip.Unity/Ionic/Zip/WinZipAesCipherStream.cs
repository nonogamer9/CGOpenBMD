using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Ionic.Zip
{
	internal class WinZipAesCipherStream : Stream
	{
		private const int BLOCK_SIZE_IN_BYTES = 16;

		private WinZipAesCrypto _params;

		private Stream _s;

		private CryptoMode _mode;

		private int _nonce;

		private bool _finalBlock;

		internal HMACSHA1 _mac;

		internal RijndaelManaged _aesCipher;

		internal ICryptoTransform _xform;

		private byte[] counter = new byte[16];

		private byte[] counterOut = new byte[16];

		private long _length;

		private long _totalBytesXferred;

		private byte[] _PendingWriteBlock;

		private int _pendingCount;

		private byte[] _iobuf;

		private object _outputLock = new object();

		public byte[] FinalAuthentication
		{
			get
			{
				if (!_finalBlock)
				{
					if (_totalBytesXferred != 0)
					{
						throw new BadStateException("The final hash has not been computed.");
					}
					byte[] buffer = new byte[0];
					_mac.ComputeHash(buffer);
				}
				byte[] array = new byte[10];
				Array.Copy(_mac.Hash, 0, array, 0, 10);
				return array;
			}
		}

		public override bool CanRead
		{
			get
			{
				if (_mode != CryptoMode.Decrypt)
				{
					return false;
				}
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _mode == CryptoMode.Encrypt;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		internal WinZipAesCipherStream(Stream s, WinZipAesCrypto cryptoParams, long length, CryptoMode mode)
			: this(s, cryptoParams, mode)
		{
			_length = length;
		}

		internal WinZipAesCipherStream(Stream s, WinZipAesCrypto cryptoParams, CryptoMode mode)
		{
			_params = cryptoParams;
			_s = s;
			_mode = mode;
			_nonce = 1;
			if (_params == null)
			{
				throw new BadPasswordException("Supply a password to use AES encryption.");
			}
			int num = _params.KeyBytes.Length * 8;
			if (num != 256 && num != 128 && num != 192)
			{
				throw new ArgumentOutOfRangeException("keysize", "size of key must be 128, 192, or 256");
			}
			_mac = new HMACSHA1(_params.MacIv);
			_aesCipher = new RijndaelManaged();
			_aesCipher.BlockSize = 128;
			_aesCipher.KeySize = num;
			_aesCipher.Mode = CipherMode.ECB;
			_aesCipher.Padding = PaddingMode.None;
			byte[] rgbIV = new byte[16];
			_xform = _aesCipher.CreateEncryptor(_params.KeyBytes, rgbIV);
			if (_mode == CryptoMode.Encrypt)
			{
				_iobuf = new byte[2048];
				_PendingWriteBlock = new byte[16];
			}
		}

		private void XorInPlace(byte[] buffer, int offset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				buffer[offset + i] = (byte)(counterOut[i] ^ buffer[offset + i]);
			}
		}

		private void WriteTransformOneBlock(byte[] buffer, int offset)
		{
			Array.Copy(BitConverter.GetBytes(_nonce++), 0, counter, 0, 4);
			_xform.TransformBlock(counter, 0, 16, counterOut, 0);
			XorInPlace(buffer, offset, 16);
			_mac.TransformBlock(buffer, offset, 16, null, 0);
		}

		private void WriteTransformBlocks(byte[] buffer, int offset, int count)
		{
			int i = offset;
			for (int num = count + offset; i < buffer.Length && i < num; i += 16)
			{
				WriteTransformOneBlock(buffer, i);
			}
		}

		private void WriteTransformFinalBlock()
		{
			if (_pendingCount == 0)
			{
				throw new InvalidOperationException("No bytes available.");
			}
			if (_finalBlock)
			{
				throw new InvalidOperationException("The final block has already been transformed.");
			}
			Array.Copy(BitConverter.GetBytes(_nonce++), 0, counter, 0, 4);
			counterOut = _xform.TransformFinalBlock(counter, 0, 16);
			XorInPlace(_PendingWriteBlock, 0, _pendingCount);
			_mac.TransformFinalBlock(_PendingWriteBlock, 0, _pendingCount);
			_finalBlock = true;
		}

		private int ReadTransformOneBlock(byte[] buffer, int offset, int last)
		{
			if (_finalBlock)
			{
				throw new NotSupportedException();
			}
			int num = last - offset;
			int num2 = ((num > 16) ? 16 : num);
			Array.Copy(BitConverter.GetBytes(_nonce++), 0, counter, 0, 4);
			if (num2 == num && _length > 0 && _totalBytesXferred + last == _length)
			{
				_mac.TransformFinalBlock(buffer, offset, num2);
				counterOut = _xform.TransformFinalBlock(counter, 0, 16);
				_finalBlock = true;
			}
			else
			{
				_mac.TransformBlock(buffer, offset, num2, null, 0);
				_xform.TransformBlock(counter, 0, 16, counterOut, 0);
			}
			XorInPlace(buffer, offset, num2);
			return num2;
		}

		private void ReadTransformBlocks(byte[] buffer, int offset, int count)
		{
			int i = offset;
			int num2;
			for (int num = count + offset; i < buffer.Length && i < num; i += num2)
			{
				num2 = ReadTransformOneBlock(buffer, i, num);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_mode == CryptoMode.Encrypt)
			{
				throw new NotSupportedException();
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Must not be less than zero.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "Must not be less than zero.");
			}
			if (buffer.Length < offset + count)
			{
				throw new ArgumentException("The buffer is too small");
			}
			int count2 = count;
			if (_totalBytesXferred >= _length)
			{
				return 0;
			}
			long num = _length - _totalBytesXferred;
			if (num < count)
			{
				count2 = (int)num;
			}
			int num2 = _s.Read(buffer, offset, count2);
			ReadTransformBlocks(buffer, offset, count2);
			_totalBytesXferred += num2;
			return num2;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_finalBlock)
			{
				throw new InvalidOperationException("The final block has already been transformed.");
			}
			if (_mode == CryptoMode.Decrypt)
			{
				throw new NotSupportedException();
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Must not be less than zero.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "Must not be less than zero.");
			}
			if (buffer.Length < offset + count)
			{
				throw new ArgumentException("The offset and count are too large");
			}
			if (count == 0)
			{
				return;
			}
			if (count + _pendingCount <= 16)
			{
				Buffer.BlockCopy(buffer, offset, _PendingWriteBlock, _pendingCount, count);
				_pendingCount += count;
				return;
			}
			int num = count;
			int num2 = offset;
			if (_pendingCount != 0)
			{
				int num3 = 16 - _pendingCount;
				if (num3 > 0)
				{
					Buffer.BlockCopy(buffer, offset, _PendingWriteBlock, _pendingCount, num3);
					num -= num3;
					num2 += num3;
				}
				WriteTransformOneBlock(_PendingWriteBlock, 0);
				_s.Write(_PendingWriteBlock, 0, 16);
				_totalBytesXferred += 16L;
				_pendingCount = 0;
			}
			int num4 = (num - 1) / 16;
			_pendingCount = num - num4 * 16;
			Buffer.BlockCopy(buffer, num2 + num - _pendingCount, _PendingWriteBlock, 0, _pendingCount);
			num -= _pendingCount;
			_totalBytesXferred += num;
			if (num4 <= 0)
			{
				return;
			}
			do
			{
				int num5 = _iobuf.Length;
				if (num5 > num)
				{
					num5 = num;
				}
				Buffer.BlockCopy(buffer, num2, _iobuf, 0, num5);
				WriteTransformBlocks(_iobuf, 0, num5);
				_s.Write(_iobuf, 0, num5);
				num -= num5;
				num2 += num5;
			}
			while (num > 0);
		}

		public override void Close()
		{
			if (_pendingCount > 0)
			{
				WriteTransformFinalBlock();
				_s.Write(_PendingWriteBlock, 0, _pendingCount);
				_totalBytesXferred += _pendingCount;
				_pendingCount = 0;
			}
			_s.Close();
		}

		public override void Flush()
		{
			_s.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		[Conditional("Trace")]
		private void TraceOutput(string format, params object[] varParams)
		{
			lock (_outputLock)
			{
				int hashCode = Thread.CurrentThread.GetHashCode();
				Console.ForegroundColor = (ConsoleColor)(hashCode % 8 + 8);
				Console.Write("{0:000} WZACS ", hashCode);
				Console.WriteLine(format, varParams);
				Console.ResetColor();
			}
		}
	}
}
