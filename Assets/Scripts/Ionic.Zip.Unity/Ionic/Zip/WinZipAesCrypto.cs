using System;
using System.IO;
using System.Security.Cryptography;

namespace Ionic.Zip
{
	internal class WinZipAesCrypto
	{
		internal byte[] _Salt;

		internal byte[] _providedPv;

		internal byte[] _generatedPv;

		internal int _KeyStrengthInBits;

		private byte[] _MacInitializationVector;

		private byte[] _StoredMac;

		private byte[] _keyBytes;

		private short PasswordVerificationStored;

		private short PasswordVerificationGenerated;

		private int Rfc2898KeygenIterations = 1000;

		private string _Password;

		private bool _cryptoGenerated;

		public byte[] CalculatedMac;

		public byte[] GeneratedPV
		{
			get
			{
				if (!_cryptoGenerated)
				{
					_GenerateCryptoBytes();
				}
				return _generatedPv;
			}
		}

		public byte[] Salt
		{
			get
			{
				return _Salt;
			}
		}

		private int _KeyStrengthInBytes
		{
			get
			{
				return _KeyStrengthInBits / 8;
			}
		}

		public int SizeOfEncryptionMetadata
		{
			get
			{
				return _KeyStrengthInBytes / 2 + 10 + 2;
			}
		}

		public string Password
		{
			private get
			{
				return _Password;
			}
			set
			{
				_Password = value;
				if (_Password != null)
				{
					PasswordVerificationGenerated = (short)(GeneratedPV[0] + GeneratedPV[1] * 256);
					if (PasswordVerificationGenerated != PasswordVerificationStored)
					{
						throw new BadPasswordException();
					}
				}
			}
		}

		public byte[] KeyBytes
		{
			get
			{
				if (!_cryptoGenerated)
				{
					_GenerateCryptoBytes();
				}
				return _keyBytes;
			}
		}

		public byte[] MacIv
		{
			get
			{
				if (!_cryptoGenerated)
				{
					_GenerateCryptoBytes();
				}
				return _MacInitializationVector;
			}
		}

		private WinZipAesCrypto(string password, int KeyStrengthInBits)
		{
			_Password = password;
			_KeyStrengthInBits = KeyStrengthInBits;
		}

		public static WinZipAesCrypto Generate(string password, int KeyStrengthInBits)
		{
			WinZipAesCrypto winZipAesCrypto = new WinZipAesCrypto(password, KeyStrengthInBits);
			int num = winZipAesCrypto._KeyStrengthInBytes / 2;
			winZipAesCrypto._Salt = new byte[num];
			Random random = new Random();
			random.NextBytes(winZipAesCrypto._Salt);
			return winZipAesCrypto;
		}

		public static WinZipAesCrypto ReadFromStream(string password, int KeyStrengthInBits, Stream s)
		{
			WinZipAesCrypto winZipAesCrypto = new WinZipAesCrypto(password, KeyStrengthInBits);
			int num = winZipAesCrypto._KeyStrengthInBytes / 2;
			winZipAesCrypto._Salt = new byte[num];
			winZipAesCrypto._providedPv = new byte[2];
			s.Read(winZipAesCrypto._Salt, 0, winZipAesCrypto._Salt.Length);
			s.Read(winZipAesCrypto._providedPv, 0, winZipAesCrypto._providedPv.Length);
			winZipAesCrypto.PasswordVerificationStored = (short)(winZipAesCrypto._providedPv[0] + winZipAesCrypto._providedPv[1] * 256);
			if (password != null)
			{
				winZipAesCrypto.PasswordVerificationGenerated = (short)(winZipAesCrypto.GeneratedPV[0] + winZipAesCrypto.GeneratedPV[1] * 256);
				if (winZipAesCrypto.PasswordVerificationGenerated != winZipAesCrypto.PasswordVerificationStored)
				{
					throw new BadPasswordException("bad password");
				}
			}
			return winZipAesCrypto;
		}

		private void _GenerateCryptoBytes()
		{
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(_Password, Salt, Rfc2898KeygenIterations);
			_keyBytes = rfc2898DeriveBytes.GetBytes(_KeyStrengthInBytes);
			_MacInitializationVector = rfc2898DeriveBytes.GetBytes(_KeyStrengthInBytes);
			_generatedPv = rfc2898DeriveBytes.GetBytes(2);
			_cryptoGenerated = true;
		}

		public void ReadAndVerifyMac(Stream s)
		{
			bool flag = false;
			_StoredMac = new byte[10];
			s.Read(_StoredMac, 0, _StoredMac.Length);
			if (_StoredMac.Length != CalculatedMac.Length)
			{
				flag = true;
			}
			if (!flag)
			{
				for (int i = 0; i < _StoredMac.Length; i++)
				{
					if (_StoredMac[i] != CalculatedMac[i])
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				throw new BadStateException("The MAC does not match.");
			}
		}
	}
}
