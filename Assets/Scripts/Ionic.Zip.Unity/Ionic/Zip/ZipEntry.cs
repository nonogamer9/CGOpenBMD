using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ionic.BZip2;
using Ionic.Crc;
using Ionic.Zlib;

namespace Ionic.Zip
{
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	[Guid("ebc25cf6-9120-4283-b972-0e5520d00004")]
	public class ZipEntry
	{
		private class CopyHelper
		{
			private static Regex re = new Regex(" \\(copy (\\d+)\\)$");

			private static int callCount = 0;

			internal static string AppendCopyToFileName(string f)
			{
				callCount++;
				if (callCount > 25)
				{
					throw new OverflowException("overflow while creating filename");
				}
				int num = 1;
				int num2 = f.LastIndexOf(".");
				if (num2 == -1)
				{
					Match match = re.Match(f);
					if (match.Success)
					{
						num = int.Parse(match.Groups[1].Value) + 1;
						string text = string.Format(" (copy {0})", num);
						f = f.Substring(0, match.Index) + text;
					}
					else
					{
						string text2 = string.Format(" (copy {0})", num);
						f += text2;
					}
				}
				else
				{
					Match match2 = re.Match(f.Substring(0, num2));
					if (match2.Success)
					{
						num = int.Parse(match2.Groups[1].Value) + 1;
						string text3 = string.Format(" (copy {0})", num);
						f = f.Substring(0, match2.Index) + text3 + f.Substring(num2);
					}
					else
					{
						string text4 = string.Format(" (copy {0})", num);
						f = f.Substring(0, num2) + text4 + f.Substring(num2);
					}
				}
				return f;
			}
		}

		private delegate T Func<T>();

		private short _VersionMadeBy;

		private short _InternalFileAttrs;

		private int _ExternalFileAttrs;

		private short _filenameLength;

		private short _extraFieldLength;

		private short _commentLength;

		private ZipCrypto _zipCrypto_forExtract;

		private ZipCrypto _zipCrypto_forWrite;

		private WinZipAesCrypto _aesCrypto_forExtract;

		private WinZipAesCrypto _aesCrypto_forWrite;

		private short _WinZipAesMethod;

		internal DateTime _LastModified;

		private DateTime _Mtime;

		private DateTime _Atime;

		private DateTime _Ctime;

		private bool _ntfsTimesAreSet;

		private bool _emitNtfsTimes = true;

		private bool _emitUnixTimes;

		private bool _TrimVolumeFromFullyQualifiedPaths = true;

		internal string _LocalFileName;

		private string _FileNameInArchive;

		internal short _VersionNeeded;

		internal short _BitField;

		internal short _CompressionMethod;

		private short _CompressionMethod_FromZipFile;

		private CompressionLevel _CompressionLevel;

		internal string _Comment;

		private bool _IsDirectory;

		private byte[] _CommentBytes;

		internal long _CompressedSize;

		internal long _CompressedFileDataSize;

		internal long _UncompressedSize;

		internal int _TimeBlob;

		private bool _crcCalculated;

		internal int _Crc32;

		internal byte[] _Extra;

		private bool _metadataChanged;

		private bool _restreamRequiredOnSave;

		private bool _sourceIsEncrypted;

		private bool _skippedDuringSave;

		private uint _diskNumber;

		private static Encoding ibm437 = Encoding.UTF8;

		private Encoding _actualEncoding;

		internal ZipContainer _container;

		private long __FileDataPosition = -1L;

		private byte[] _EntryHeader;

		internal long _RelativeOffsetOfLocalHeader;

		private long _future_ROLH;

		private long _TotalEntrySize;

		private int _LengthOfHeader;

		private int _LengthOfTrailer;

		internal bool _InputUsesZip64;

		private uint _UnsupportedAlgorithmId;

		internal string _Password;

		internal ZipEntrySource _Source;

		internal EncryptionAlgorithm _Encryption;

		internal EncryptionAlgorithm _Encryption_FromZipFile;

		internal byte[] _WeakEncryptionHeader;

		internal Stream _archiveStream;

		private Stream _sourceStream;

		private long? _sourceStreamOriginalPosition;

		private bool _sourceWasJitProvided;

		private bool _ioOperationCanceled;

		private bool _presumeZip64;

		private bool? _entryRequiresZip64;

		private bool? _OutputUsesZip64;

		private bool _IsText;

		private ZipEntryTimestamp _timestamp;

		private static DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private static DateTime _win32Epoch = DateTime.FromFileTimeUtc(0L);

		private static DateTime _zeroHour = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private WriteDelegate _WriteDelegate;

		private OpenDelegate _OpenDelegate;

		private CloseDelegate _CloseDelegate;

		private Stream _inputDecryptorStream;

		private int _readExtraDepth;

		private object _outputLock = new object();

		internal bool AttributesIndicateDirectory
		{
			get
			{
				if (_InternalFileAttrs == 0)
				{
					return (_ExternalFileAttrs & 0x10) == 16;
				}
				return false;
			}
		}

		public string Info
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(string.Format("          ZipEntry: {0}\n", FileName)).Append(string.Format("   Version Made By: {0}\n", _VersionMadeBy)).Append(string.Format(" Needed to extract: {0}\n", VersionNeeded));
				if (_IsDirectory)
				{
					stringBuilder.Append("        Entry type: directory\n");
				}
				else
				{
					stringBuilder.Append(string.Format("         File type: {0}\n", _IsText ? "text" : "binary")).Append(string.Format("       Compression: {0}\n", CompressionMethod)).Append(string.Format("        Compressed: 0x{0:X}\n", CompressedSize))
						.Append(string.Format("      Uncompressed: 0x{0:X}\n", UncompressedSize))
						.Append(string.Format("             CRC32: 0x{0:X8}\n", _Crc32));
				}
				stringBuilder.Append(string.Format("       Disk Number: {0}\n", _diskNumber));
				if (_RelativeOffsetOfLocalHeader > uint.MaxValue)
				{
					stringBuilder.Append(string.Format("   Relative Offset: 0x{0:X16}\n", _RelativeOffsetOfLocalHeader));
				}
				else
				{
					stringBuilder.Append(string.Format("   Relative Offset: 0x{0:X8}\n", _RelativeOffsetOfLocalHeader));
				}
				stringBuilder.Append(string.Format("         Bit Field: 0x{0:X4}\n", _BitField)).Append(string.Format("        Encrypted?: {0}\n", _sourceIsEncrypted)).Append(string.Format("          Timeblob: 0x{0:X8}\n", _TimeBlob))
					.Append(string.Format("              Time: {0}\n", SharedUtilities.PackedToDateTime(_TimeBlob)));
				stringBuilder.Append(string.Format("         Is Zip64?: {0}\n", _InputUsesZip64));
				if (!string.IsNullOrEmpty(_Comment))
				{
					stringBuilder.Append(string.Format("           Comment: {0}\n", _Comment));
				}
				stringBuilder.Append("\n");
				return stringBuilder.ToString();
			}
		}

		public DateTime LastModified
		{
			get
			{
				return _LastModified.ToLocalTime();
			}
			set
			{
				_LastModified = ((value.Kind == DateTimeKind.Unspecified) ? DateTime.SpecifyKind(value, DateTimeKind.Local) : value.ToLocalTime());
				_Mtime = SharedUtilities.AdjustTime_Reverse(_LastModified).ToUniversalTime();
				_metadataChanged = true;
			}
		}

		private int BufferSize
		{
			get
			{
				return _container.BufferSize;
			}
		}

		public DateTime ModifiedTime
		{
			get
			{
				return _Mtime;
			}
			set
			{
				SetEntryTimes(_Ctime, _Atime, value);
			}
		}

		public DateTime AccessedTime
		{
			get
			{
				return _Atime;
			}
			set
			{
				SetEntryTimes(_Ctime, value, _Mtime);
			}
		}

		public DateTime CreationTime
		{
			get
			{
				return _Ctime;
			}
			set
			{
				SetEntryTimes(value, _Atime, _Mtime);
			}
		}

		public bool EmitTimesInWindowsFormatWhenSaving
		{
			get
			{
				return _emitNtfsTimes;
			}
			set
			{
				_emitNtfsTimes = value;
				_metadataChanged = true;
			}
		}

		public bool EmitTimesInUnixFormatWhenSaving
		{
			get
			{
				return _emitUnixTimes;
			}
			set
			{
				_emitUnixTimes = value;
				_metadataChanged = true;
			}
		}

		public ZipEntryTimestamp Timestamp
		{
			get
			{
				return _timestamp;
			}
		}

		public FileAttributes Attributes
		{
			get
			{
				return (FileAttributes)_ExternalFileAttrs;
			}
			set
			{
				_ExternalFileAttrs = (int)value;
				_VersionMadeBy = 45;
				_metadataChanged = true;
			}
		}

		internal string LocalFileName
		{
			get
			{
				return _LocalFileName;
			}
		}

		public string FileName
		{
			get
			{
				return _FileNameInArchive;
			}
			set
			{
				if (_container.ZipFile == null)
				{
					throw new ZipException("Cannot rename; this is not supported in ZipOutputStream/ZipInputStream.");
				}
				if (string.IsNullOrEmpty(value))
				{
					throw new ZipException("The FileName must be non empty and non-null.");
				}
				string text = NameInArchive(value, null);
				if (!(_FileNameInArchive == text))
				{
					_container.ZipFile.RemoveEntry(this);
					_container.ZipFile.InternalAddEntry(text, this);
					_FileNameInArchive = text;
					_container.ZipFile.NotifyEntryChanged();
					_metadataChanged = true;
				}
			}
		}

		public Stream InputStream
		{
			get
			{
				return _sourceStream;
			}
			set
			{
				if (_Source != ZipEntrySource.Stream)
				{
					throw new ZipException("You must not set the input stream for this entry.");
				}
				_sourceWasJitProvided = true;
				_sourceStream = value;
			}
		}

		public bool InputStreamWasJitProvided
		{
			get
			{
				return _sourceWasJitProvided;
			}
		}

		public ZipEntrySource Source
		{
			get
			{
				return _Source;
			}
		}

		public short VersionNeeded
		{
			get
			{
				return _VersionNeeded;
			}
		}

		public string Comment
		{
			get
			{
				return _Comment;
			}
			set
			{
				_Comment = value;
				_metadataChanged = true;
			}
		}

		public bool? RequiresZip64
		{
			get
			{
				return _entryRequiresZip64;
			}
		}

		public bool? OutputUsedZip64
		{
			get
			{
				return _OutputUsesZip64;
			}
		}

		public short BitField
		{
			get
			{
				return _BitField;
			}
		}

		public CompressionMethod CompressionMethod
		{
			get
			{
				return (CompressionMethod)_CompressionMethod;
			}
			set
			{
				if (value != (CompressionMethod)_CompressionMethod)
				{
					if (value != CompressionMethod.None && value != CompressionMethod.Deflate && value != CompressionMethod.BZip2)
					{
						throw new InvalidOperationException("Unsupported compression method.");
					}
					_CompressionMethod = (short)value;
					if (_CompressionMethod == 0)
					{
						_CompressionLevel = CompressionLevel.None;
					}
					else if (CompressionLevel == CompressionLevel.None)
					{
						_CompressionLevel = CompressionLevel.Default;
					}
					if (_container.ZipFile != null)
					{
						_container.ZipFile.NotifyEntryChanged();
					}
					_restreamRequiredOnSave = true;
				}
			}
		}

		public CompressionLevel CompressionLevel
		{
			get
			{
				return _CompressionLevel;
			}
			set
			{
				if ((_CompressionMethod != 8 && _CompressionMethod != 0) || (value == CompressionLevel.Default && _CompressionMethod == 8))
				{
					return;
				}
				_CompressionLevel = value;
				if (value != CompressionLevel.None || _CompressionMethod != 0)
				{
					if (_CompressionLevel == CompressionLevel.None)
					{
						_CompressionMethod = 0;
					}
					else
					{
						_CompressionMethod = 8;
					}
					if (_container.ZipFile != null)
					{
						_container.ZipFile.NotifyEntryChanged();
					}
					_restreamRequiredOnSave = true;
				}
			}
		}

		public long CompressedSize
		{
			get
			{
				return _CompressedSize;
			}
		}

		public long UncompressedSize
		{
			get
			{
				return _UncompressedSize;
			}
		}

		public double CompressionRatio
		{
			get
			{
				if (UncompressedSize == 0)
				{
					return 0.0;
				}
				return 100.0 * (1.0 - 1.0 * (double)CompressedSize / (1.0 * (double)UncompressedSize));
			}
		}

		public int Crc
		{
			get
			{
				return _Crc32;
			}
		}

		public bool IsDirectory
		{
			get
			{
				return _IsDirectory;
			}
		}

		public bool UsesEncryption
		{
			get
			{
				return _Encryption_FromZipFile != EncryptionAlgorithm.None;
			}
		}

		public EncryptionAlgorithm Encryption
		{
			get
			{
				return _Encryption;
			}
			set
			{
				if (value != _Encryption)
				{
					if (value == EncryptionAlgorithm.Unsupported)
					{
						throw new InvalidOperationException("You may not set Encryption to that value.");
					}
					_Encryption = value;
					_restreamRequiredOnSave = true;
					if (_container.ZipFile != null)
					{
						_container.ZipFile.NotifyEntryChanged();
					}
				}
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
				if (_Password == null)
				{
					_Encryption = EncryptionAlgorithm.None;
					return;
				}
				if (_Source == ZipEntrySource.ZipFile && !_sourceIsEncrypted)
				{
					_restreamRequiredOnSave = true;
				}
				if (Encryption == EncryptionAlgorithm.None)
				{
					_Encryption = EncryptionAlgorithm.PkzipWeak;
				}
			}
		}

		internal bool IsChanged
		{
			get
			{
				return _restreamRequiredOnSave | _metadataChanged;
			}
		}

		public ExtractExistingFileAction ExtractExistingFile { get; set; }

		public ZipErrorAction ZipErrorAction { get; set; }

		public bool IncludedInMostRecentSave
		{
			get
			{
				return !_skippedDuringSave;
			}
		}

		public SetCompressionCallback SetCompression { get; set; }

		[Obsolete("Beginning with v1.9.1.6 of DotNetZip, this property is obsolete.  It will be removed in a future version of the library. Your applications should  use AlternateEncoding and AlternateEncodingUsage instead.")]
		public bool UseUnicodeAsNecessary
		{
			get
			{
				if (AlternateEncoding == Encoding.GetEncoding("UTF-8"))
				{
					return AlternateEncodingUsage == ZipOption.AsNecessary;
				}
				return false;
			}
			set
			{
				if (value)
				{
					AlternateEncoding = Encoding.GetEncoding("UTF-8");
					AlternateEncodingUsage = ZipOption.AsNecessary;
				}
				else
				{
					AlternateEncoding = ZipFile.DefaultEncoding;
					AlternateEncodingUsage = ZipOption.Default;
				}
			}
		}

		[Obsolete("This property is obsolete since v1.9.1.6. Use AlternateEncoding and AlternateEncodingUsage instead.", true)]
		public Encoding ProvisionalAlternateEncoding { get; set; }

		public Encoding AlternateEncoding { get; set; }

		public ZipOption AlternateEncodingUsage { get; set; }

		public bool IsText
		{
			get
			{
				return _IsText;
			}
			set
			{
				_IsText = value;
			}
		}

		internal Stream ArchiveStream
		{
			get
			{
				if (_archiveStream == null)
				{
					if (_container.ZipFile != null)
					{
						ZipFile zipFile = _container.ZipFile;
						zipFile.Reset(false);
						_archiveStream = zipFile.StreamForDiskNumber(_diskNumber);
					}
					else
					{
						_archiveStream = _container.ZipOutputStream.OutputStream;
					}
				}
				return _archiveStream;
			}
		}

		internal long FileDataPosition
		{
			get
			{
				if (__FileDataPosition == -1)
				{
					SetFdpLoh();
				}
				return __FileDataPosition;
			}
		}

		private int LengthOfHeader
		{
			get
			{
				if (_LengthOfHeader == 0)
				{
					SetFdpLoh();
				}
				return _LengthOfHeader;
			}
		}

		private string UnsupportedAlgorithm
		{
			get
			{
				string empty = string.Empty;
				switch (_UnsupportedAlgorithmId)
				{
				case 0u:
					return "--";
				case 26113u:
					return "DES";
				case 26114u:
					return "RC2";
				case 26115u:
					return "3DES-168";
				case 26121u:
					return "3DES-112";
				case 26126u:
					return "PKWare AES128";
				case 26127u:
					return "PKWare AES192";
				case 26128u:
					return "PKWare AES256";
				case 26370u:
					return "RC2";
				case 26400u:
					return "Blowfish";
				case 26401u:
					return "Twofish";
				case 26625u:
					return "RC4";
				default:
					return string.Format("Unknown (0x{0:X4})", _UnsupportedAlgorithmId);
				}
			}
		}

		private string UnsupportedCompressionMethod
		{
			get
			{
				string empty = string.Empty;
				switch ((int)_CompressionMethod)
				{
				case 0:
					return "Store";
				case 1:
					return "Shrink";
				case 8:
					return "DEFLATE";
				case 9:
					return "Deflate64";
				case 12:
					return "BZIP2";
				case 14:
					return "LZMA";
				case 19:
					return "LZ77";
				case 98:
					return "PPMd";
				default:
					return string.Format("Unknown (0x{0:X4})", _CompressionMethod);
				}
			}
		}

		internal void ResetDirEntry()
		{
			__FileDataPosition = -1L;
			_LengthOfHeader = 0;
		}

		internal static ZipEntry ReadDirEntry(ZipFile zf, Dictionary<string, object> previouslySeen)
		{
			Stream readStream = zf.ReadStream;
			Encoding encoding = ((zf.AlternateEncodingUsage == ZipOption.Always) ? zf.AlternateEncoding : ZipFile.DefaultEncoding);
			int num = SharedUtilities.ReadSignature(readStream);
			if (IsNotValidZipDirEntrySig(num))
			{
				readStream.Seek(-4L, SeekOrigin.Current);
				if ((long)num != 101010256 && (long)num != 101075792 && num != 67324752)
				{
					throw new BadReadException(string.Format("  Bad signature (0x{0:X8}) at position 0x{1:X8}", num, readStream.Position));
				}
				return null;
			}
			int num2 = 46;
			byte[] array = new byte[42];
			int num3 = readStream.Read(array, 0, array.Length);
			if (num3 != array.Length)
			{
				return null;
			}
			int num4 = 0;
			ZipEntry zipEntry = new ZipEntry();
			zipEntry.AlternateEncoding = encoding;
			zipEntry._Source = ZipEntrySource.ZipFile;
			zipEntry._container = new ZipContainer(zf);
			zipEntry._VersionMadeBy = (short)(array[num4++] + array[num4++] * 256);
			zipEntry._VersionNeeded = (short)(array[num4++] + array[num4++] * 256);
			zipEntry._BitField = (short)(array[num4++] + array[num4++] * 256);
			zipEntry._CompressionMethod = (short)(array[num4++] + array[num4++] * 256);
			zipEntry._TimeBlob = array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256;
			zipEntry._LastModified = SharedUtilities.PackedToDateTime(zipEntry._TimeBlob);
			zipEntry._timestamp |= ZipEntryTimestamp.DOS;
			zipEntry._Crc32 = array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256;
			zipEntry._CompressedSize = (uint)(array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256);
			zipEntry._UncompressedSize = (uint)(array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256);
			zipEntry._CompressionMethod_FromZipFile = zipEntry._CompressionMethod;
			zipEntry._filenameLength = (short)(array[num4++] + array[num4++] * 256);
			zipEntry._extraFieldLength = (short)(array[num4++] + array[num4++] * 256);
			zipEntry._commentLength = (short)(array[num4++] + array[num4++] * 256);
			zipEntry._diskNumber = (uint)(array[num4++] + array[num4++] * 256);
			zipEntry._InternalFileAttrs = (short)(array[num4++] + array[num4++] * 256);
			zipEntry._ExternalFileAttrs = array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256;
			zipEntry._RelativeOffsetOfLocalHeader = (uint)(array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256);
			zipEntry.IsText = (zipEntry._InternalFileAttrs & 1) == 1;
			array = new byte[zipEntry._filenameLength];
			num3 = readStream.Read(array, 0, array.Length);
			num2 += num3;
			if ((zipEntry._BitField & 0x800) == 2048)
			{
				zipEntry._FileNameInArchive = SharedUtilities.Utf8StringFromBuffer(array);
			}
			else
			{
				zipEntry._FileNameInArchive = SharedUtilities.StringFromBuffer(array, encoding);
			}
			while (previouslySeen.ContainsKey(zipEntry._FileNameInArchive))
			{
				zipEntry._FileNameInArchive = CopyHelper.AppendCopyToFileName(zipEntry._FileNameInArchive);
				zipEntry._metadataChanged = true;
			}
			if (zipEntry.AttributesIndicateDirectory)
			{
				zipEntry.MarkAsDirectory();
			}
			else if (zipEntry._FileNameInArchive.EndsWith("/"))
			{
				zipEntry.MarkAsDirectory();
			}
			zipEntry._CompressedFileDataSize = zipEntry._CompressedSize;
			if ((zipEntry._BitField & 1) == 1)
			{
				zipEntry._Encryption_FromZipFile = (zipEntry._Encryption = EncryptionAlgorithm.PkzipWeak);
				zipEntry._sourceIsEncrypted = true;
			}
			if (zipEntry._extraFieldLength > 0)
			{
				zipEntry._InputUsesZip64 = zipEntry._CompressedSize == uint.MaxValue || zipEntry._UncompressedSize == uint.MaxValue || zipEntry._RelativeOffsetOfLocalHeader == uint.MaxValue;
				num2 += zipEntry.ProcessExtraField(readStream, zipEntry._extraFieldLength);
				zipEntry._CompressedFileDataSize = zipEntry._CompressedSize;
			}
			if (zipEntry._Encryption == EncryptionAlgorithm.PkzipWeak)
			{
				zipEntry._CompressedFileDataSize -= 12L;
			}
			else if (zipEntry.Encryption == EncryptionAlgorithm.WinZipAes128 || zipEntry.Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				zipEntry._CompressedFileDataSize = zipEntry.CompressedSize - (GetLengthOfCryptoHeaderBytes(zipEntry.Encryption) + 10);
				zipEntry._LengthOfTrailer = 10;
			}
			if ((zipEntry._BitField & 8) == 8)
			{
				if (zipEntry._InputUsesZip64)
				{
					zipEntry._LengthOfTrailer += 24;
				}
				else
				{
					zipEntry._LengthOfTrailer += 16;
				}
			}
			zipEntry.AlternateEncoding = (((zipEntry._BitField & 0x800) == 2048) ? Encoding.UTF8 : encoding);
			zipEntry.AlternateEncodingUsage = ZipOption.Always;
			if (zipEntry._commentLength > 0)
			{
				array = new byte[zipEntry._commentLength];
				num3 = readStream.Read(array, 0, array.Length);
				num2 += num3;
				if ((zipEntry._BitField & 0x800) == 2048)
				{
					zipEntry._Comment = SharedUtilities.Utf8StringFromBuffer(array);
				}
				else
				{
					zipEntry._Comment = SharedUtilities.StringFromBuffer(array, encoding);
				}
			}
			return zipEntry;
		}

		internal static bool IsNotValidZipDirEntrySig(int signature)
		{
			return signature != 33639248;
		}

		public ZipEntry()
		{
			_CompressionMethod = 8;
			_CompressionLevel = CompressionLevel.Default;
			_Encryption = EncryptionAlgorithm.None;
			_Source = ZipEntrySource.None;
			AlternateEncoding = Encoding.UTF8;
			AlternateEncodingUsage = ZipOption.Default;
		}

		public void SetEntryTimes(DateTime created, DateTime accessed, DateTime modified)
		{
			_ntfsTimesAreSet = true;
			if (created == _zeroHour && created.Kind == _zeroHour.Kind)
			{
				created = _win32Epoch;
			}
			if (accessed == _zeroHour && accessed.Kind == _zeroHour.Kind)
			{
				accessed = _win32Epoch;
			}
			if (modified == _zeroHour && modified.Kind == _zeroHour.Kind)
			{
				modified = _win32Epoch;
			}
			_Ctime = created.ToUniversalTime();
			_Atime = accessed.ToUniversalTime();
			_Mtime = modified.ToUniversalTime();
			_LastModified = _Mtime;
			if (!_emitUnixTimes && !_emitNtfsTimes)
			{
				_emitNtfsTimes = true;
			}
			_metadataChanged = true;
		}

		internal static string NameInArchive(string filename, string directoryPathInArchive)
		{
			string text = null;
			text = ((directoryPathInArchive == null) ? filename : ((!string.IsNullOrEmpty(directoryPathInArchive)) ? Path.Combine(directoryPathInArchive, Path.GetFileName(filename)) : Path.GetFileName(filename)));
			return SharedUtilities.NormalizePathForUseInZipFile(text);
		}

		internal static ZipEntry CreateFromNothing(string nameInArchive)
		{
			return Create(nameInArchive, ZipEntrySource.None, null, null);
		}

		internal static ZipEntry CreateFromFile(string filename, string nameInArchive)
		{
			return Create(nameInArchive, ZipEntrySource.FileSystem, filename, null);
		}

		internal static ZipEntry CreateForStream(string entryName, Stream s)
		{
			return Create(entryName, ZipEntrySource.Stream, s, null);
		}

		internal static ZipEntry CreateForWriter(string entryName, WriteDelegate d)
		{
			return Create(entryName, ZipEntrySource.WriteDelegate, d, null);
		}

		internal static ZipEntry CreateForJitStreamProvider(string nameInArchive, OpenDelegate opener, CloseDelegate closer)
		{
			return Create(nameInArchive, ZipEntrySource.JitStream, opener, closer);
		}

		internal static ZipEntry CreateForZipOutputStream(string nameInArchive)
		{
			return Create(nameInArchive, ZipEntrySource.ZipOutputStream, null, null);
		}

		private static ZipEntry Create(string nameInArchive, ZipEntrySource source, object arg1, object arg2)
		{
			if (string.IsNullOrEmpty(nameInArchive))
			{
				throw new ZipException("The entry name must be non-null and non-empty.");
			}
			ZipEntry zipEntry = new ZipEntry();
			zipEntry._VersionMadeBy = 45;
			zipEntry._Source = source;
			zipEntry._Mtime = (zipEntry._Atime = (zipEntry._Ctime = DateTime.UtcNow));
			switch (source)
			{
			case ZipEntrySource.Stream:
				zipEntry._sourceStream = arg1 as Stream;
				break;
			case ZipEntrySource.WriteDelegate:
				zipEntry._WriteDelegate = arg1 as WriteDelegate;
				break;
			case ZipEntrySource.JitStream:
				zipEntry._OpenDelegate = arg1 as OpenDelegate;
				zipEntry._CloseDelegate = arg2 as CloseDelegate;
				break;
			case ZipEntrySource.None:
				zipEntry._Source = ZipEntrySource.FileSystem;
				break;
			default:
			{
				string text = arg1 as string;
				if (string.IsNullOrEmpty(text))
				{
					throw new ZipException("The filename must be non-null and non-empty.");
				}
				try
				{
					zipEntry._Mtime = (zipEntry._Ctime = (zipEntry._Atime = DateTime.UtcNow));
					zipEntry._ExternalFileAttrs = 0;
					zipEntry._ntfsTimesAreSet = true;
					zipEntry._LocalFileName = Path.GetFullPath(text);
				}
				catch (PathTooLongException innerException)
				{
					string message = string.Format("The path is too long, filename={0}", text);
					throw new ZipException(message, innerException);
				}
				break;
			}
			case ZipEntrySource.ZipOutputStream:
				break;
			}
			zipEntry._LastModified = zipEntry._Mtime;
			zipEntry._FileNameInArchive = SharedUtilities.NormalizePathForUseInZipFile(nameInArchive);
			return zipEntry;
		}

		internal void MarkAsDirectory()
		{
			_IsDirectory = true;
			if (!_FileNameInArchive.EndsWith("/"))
			{
				_FileNameInArchive += "/";
			}
		}

		public override string ToString()
		{
			return string.Format("ZipEntry::{0}", FileName);
		}

		private void SetFdpLoh()
		{
			long position = ArchiveStream.Position;
			try
			{
				ArchiveStream.Seek(_RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
			}
			catch (IOException innerException)
			{
				string message = string.Format("Exception seeking  entry({0}) offset(0x{1:X8}) len(0x{2:X8})", FileName, _RelativeOffsetOfLocalHeader, ArchiveStream.Length);
				throw new BadStateException(message, innerException);
			}
			byte[] array = new byte[30];
			ArchiveStream.Read(array, 0, array.Length);
			short num = (short)(array[26] + array[27] * 256);
			short num2 = (short)(array[28] + array[29] * 256);
			ArchiveStream.Seek(num + num2, SeekOrigin.Current);
			_LengthOfHeader = 30 + num2 + num + GetLengthOfCryptoHeaderBytes(_Encryption_FromZipFile);
			__FileDataPosition = _RelativeOffsetOfLocalHeader + _LengthOfHeader;
			ArchiveStream.Seek(position, SeekOrigin.Begin);
		}

		private static int GetKeyStrengthInBits(EncryptionAlgorithm a)
		{
			switch (a)
			{
			case EncryptionAlgorithm.WinZipAes256:
				return 256;
			case EncryptionAlgorithm.WinZipAes128:
				return 128;
			default:
				return -1;
			}
		}

		internal static int GetLengthOfCryptoHeaderBytes(EncryptionAlgorithm a)
		{
			switch (a)
			{
			case EncryptionAlgorithm.None:
				return 0;
			case EncryptionAlgorithm.WinZipAes128:
			case EncryptionAlgorithm.WinZipAes256:
			{
				int keyStrengthInBits = GetKeyStrengthInBits(a);
				return keyStrengthInBits / 8 / 2 + 2;
			}
			case EncryptionAlgorithm.PkzipWeak:
				return 12;
			default:
				throw new ZipException("internal error");
			}
		}

		public void Extract()
		{
			InternalExtract(".", null, null);
		}

		public void Extract(ExtractExistingFileAction extractExistingFile)
		{
			ExtractExistingFile = extractExistingFile;
			InternalExtract(".", null, null);
		}

		public void Extract(Stream stream)
		{
			InternalExtract(null, stream, null);
		}

		public void Extract(string baseDirectory)
		{
			InternalExtract(baseDirectory, null, null);
		}

		public void Extract(string baseDirectory, ExtractExistingFileAction extractExistingFile)
		{
			ExtractExistingFile = extractExistingFile;
			InternalExtract(baseDirectory, null, null);
		}

		public void ExtractWithPassword(string password)
		{
			InternalExtract(".", null, password);
		}

		public void ExtractWithPassword(string baseDirectory, string password)
		{
			InternalExtract(baseDirectory, null, password);
		}

		public void ExtractWithPassword(ExtractExistingFileAction extractExistingFile, string password)
		{
			ExtractExistingFile = extractExistingFile;
			InternalExtract(".", null, password);
		}

		public void ExtractWithPassword(string baseDirectory, ExtractExistingFileAction extractExistingFile, string password)
		{
			ExtractExistingFile = extractExistingFile;
			InternalExtract(baseDirectory, null, password);
		}

		public void ExtractWithPassword(Stream stream, string password)
		{
			InternalExtract(null, stream, password);
		}

		public CrcCalculatorStream OpenReader()
		{
			if (_container.ZipFile == null)
			{
				throw new InvalidOperationException("Use OpenReader() only with ZipFile.");
			}
			return InternalOpenReader(_Password ?? _container.Password);
		}

		public CrcCalculatorStream OpenReader(string password)
		{
			if (_container.ZipFile == null)
			{
				throw new InvalidOperationException("Use OpenReader() only with ZipFile.");
			}
			return InternalOpenReader(password);
		}

		internal CrcCalculatorStream InternalOpenReader(string password)
		{
			ValidateCompression();
			ValidateEncryption();
			SetupCryptoForExtract(password);
			if (_Source != ZipEntrySource.ZipFile)
			{
				throw new BadStateException("You must call ZipFile.Save before calling OpenReader");
			}
			long length = ((_CompressionMethod_FromZipFile == 0) ? _CompressedFileDataSize : UncompressedSize);
			Stream archiveStream = ArchiveStream;
			ArchiveStream.Seek(FileDataPosition, SeekOrigin.Begin);
			_inputDecryptorStream = GetExtractDecryptor(archiveStream);
			Stream extractDecompressor = GetExtractDecompressor(_inputDecryptorStream);
			return new CrcCalculatorStream(extractDecompressor, length);
		}

		private void OnExtractProgress(long bytesWritten, long totalBytesToWrite)
		{
			if (_container.ZipFile != null)
			{
				_ioOperationCanceled = _container.ZipFile.OnExtractBlock(this, bytesWritten, totalBytesToWrite);
			}
		}

		private void OnBeforeExtract(string path)
		{
			if (_container.ZipFile != null && !_container.ZipFile._inExtractAll)
			{
				_ioOperationCanceled = _container.ZipFile.OnSingleEntryExtract(this, path, true);
			}
		}

		private void OnAfterExtract(string path)
		{
			if (_container.ZipFile != null && !_container.ZipFile._inExtractAll)
			{
				_container.ZipFile.OnSingleEntryExtract(this, path, false);
			}
		}

		private void OnExtractExisting(string path)
		{
			if (_container.ZipFile != null)
			{
				_ioOperationCanceled = _container.ZipFile.OnExtractExisting(this, path);
			}
		}

		private static void ReallyDelete(string fileName)
		{
			File.Delete(fileName);
		}

		private void WriteStatus(string format, params object[] args)
		{
			if (_container.ZipFile != null && _container.ZipFile.Verbose)
			{
				_container.ZipFile.StatusMessageTextWriter.WriteLine(format, args);
			}
		}

		private void InternalExtract(string baseDir, Stream outstream, string password)
		{
			if (_container == null)
			{
				throw new BadStateException("This entry is an orphan");
			}
			if (_container.ZipFile == null)
			{
				throw new InvalidOperationException("Use Extract() only with ZipFile.");
			}
			_container.ZipFile.Reset(false);
			if (_Source != ZipEntrySource.ZipFile)
			{
				throw new BadStateException("You must call ZipFile.Save before calling any Extract method");
			}
			OnBeforeExtract(baseDir);
			_ioOperationCanceled = false;
			string outFileName = null;
			Stream stream = null;
			bool flag = false;
			bool flag2 = false;
			try
			{
				ValidateCompression();
				ValidateEncryption();
				if (ValidateOutput(baseDir, outstream, out outFileName))
				{
					WriteStatus("extract dir {0}...", outFileName);
					OnAfterExtract(baseDir);
					return;
				}
				if (outFileName != null && File.Exists(outFileName))
				{
					flag = true;
					int num = CheckExtractExistingFile(baseDir, outFileName);
					if (num == 2 || num == 1)
					{
						return;
					}
				}
				string text = password ?? _Password ?? _container.Password;
				if (_Encryption_FromZipFile != EncryptionAlgorithm.None)
				{
					if (text == null)
					{
						throw new BadPasswordException();
					}
					SetupCryptoForExtract(text);
				}
				if (outFileName != null)
				{
					WriteStatus("extract file {0}...", outFileName);
					outFileName += ".tmp";
					string directoryName = Path.GetDirectoryName(outFileName);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					else if (_container.ZipFile != null)
					{
						flag2 = _container.ZipFile._inExtractAll;
					}
					stream = new FileStream(outFileName, FileMode.CreateNew);
				}
				else
				{
					WriteStatus("extract entry {0} to stream...", FileName);
					stream = outstream;
				}
				if (_ioOperationCanceled)
				{
					return;
				}
				int actualCrc = ExtractOne(stream);
				if (_ioOperationCanceled)
				{
					return;
				}
				VerifyCrcAfterExtract(actualCrc);
				if (outFileName != null)
				{
					stream.Close();
					stream = null;
					string text2 = outFileName;
					string text3 = null;
					outFileName = text2.Substring(0, text2.Length - 4);
					if (flag)
					{
						text3 = outFileName + ".PendingOverwrite";
						File.Move(outFileName, text3);
					}
					File.Move(text2, outFileName);
					_SetTimes(outFileName, true);
					if (text3 != null && File.Exists(text3))
					{
						ReallyDelete(text3);
					}
					if (flag2 && FileName.IndexOf('/') != -1)
					{
						string directoryName2 = Path.GetDirectoryName(FileName);
						if (_container.ZipFile[directoryName2] == null)
						{
							_SetTimes(Path.GetDirectoryName(outFileName), false);
						}
					}
					if ((_VersionMadeBy & 0xFF00) == 2560 || (_VersionMadeBy & 0xFF00) == 0)
					{
						File.SetAttributes(outFileName, (FileAttributes)_ExternalFileAttrs);
					}
				}
				OnAfterExtract(baseDir);
			}
			catch (Exception)
			{
				_ioOperationCanceled = true;
				throw;
			}
			finally
			{
				if (_ioOperationCanceled && outFileName != null)
				{
					if (stream != null)
					{
						stream.Close();
					}
					if (File.Exists(outFileName) && !flag)
					{
						File.Delete(outFileName);
					}
				}
			}
		}

		internal void VerifyCrcAfterExtract(int actualCrc32)
		{
			if (actualCrc32 != _Crc32 && ((Encryption != EncryptionAlgorithm.WinZipAes128 && Encryption != EncryptionAlgorithm.WinZipAes256) || _WinZipAesMethod != 2))
			{
				throw new BadCrcException("CRC error: the file being extracted appears to be corrupted. " + string.Format("Expected 0x{0:X8}, Actual 0x{1:X8}", _Crc32, actualCrc32));
			}
			if (UncompressedSize != 0 && (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256))
			{
				WinZipAesCipherStream winZipAesCipherStream = _inputDecryptorStream as WinZipAesCipherStream;
				_aesCrypto_forExtract.CalculatedMac = winZipAesCipherStream.FinalAuthentication;
				_aesCrypto_forExtract.ReadAndVerifyMac(ArchiveStream);
			}
		}

		private int CheckExtractExistingFile(string baseDir, string targetFileName)
		{
			int num = 0;
			while (true)
			{
				switch (ExtractExistingFile)
				{
				case ExtractExistingFileAction.OverwriteSilently:
					WriteStatus("the file {0} exists; will overwrite it...", targetFileName);
					return 0;
				case ExtractExistingFileAction.DoNotOverwrite:
					WriteStatus("the file {0} exists; not extracting entry...", FileName);
					OnAfterExtract(baseDir);
					return 1;
				case ExtractExistingFileAction.InvokeExtractProgressEvent:
					if (num > 0)
					{
						throw new ZipException(string.Format("The file {0} already exists.", targetFileName));
					}
					OnExtractExisting(baseDir);
					if (_ioOperationCanceled)
					{
						return 2;
					}
					break;
				default:
					throw new ZipException(string.Format("The file {0} already exists.", targetFileName));
				}
				num++;
			}
		}

		private void _CheckRead(int nbytes)
		{
			if (nbytes == 0)
			{
				throw new BadReadException(string.Format("bad read of entry {0} from compressed archive.", FileName));
			}
		}

		private int ExtractOne(Stream output)
		{
			int num = 0;
			Stream archiveStream = ArchiveStream;
			try
			{
				archiveStream.Seek(FileDataPosition, SeekOrigin.Begin);
				byte[] array = new byte[BufferSize];
				long num2 = ((_CompressionMethod_FromZipFile != 0) ? UncompressedSize : _CompressedFileDataSize);
				_inputDecryptorStream = GetExtractDecryptor(archiveStream);
				Stream extractDecompressor = GetExtractDecompressor(_inputDecryptorStream);
				long num3 = 0L;
				using (CrcCalculatorStream crcCalculatorStream = new CrcCalculatorStream(extractDecompressor))
				{
					while (num2 > 0)
					{
						int count = (int)((num2 > array.Length) ? array.Length : num2);
						int num4 = crcCalculatorStream.Read(array, 0, count);
						_CheckRead(num4);
						output.Write(array, 0, num4);
						num2 -= num4;
						num3 += num4;
						OnExtractProgress(num3, UncompressedSize);
						if (_ioOperationCanceled)
						{
							break;
						}
					}
					return crcCalculatorStream.Crc;
				}
			}
			finally
			{
				ZipSegmentedStream zipSegmentedStream = archiveStream as ZipSegmentedStream;
				if (zipSegmentedStream != null)
				{
					zipSegmentedStream.Dispose();
					_archiveStream = null;
				}
			}
		}

		internal Stream GetExtractDecompressor(Stream input2)
		{
			switch (_CompressionMethod_FromZipFile)
			{
			case 0:
				return input2;
			case 8:
				return new DeflateStream(input2, CompressionMode.Decompress, true);
			case 12:
				return new BZip2InputStream(input2, true);
			default:
				return null;
			}
		}

		internal Stream GetExtractDecryptor(Stream input)
		{
			Stream stream = null;
			if (_Encryption_FromZipFile == EncryptionAlgorithm.PkzipWeak)
			{
				return new ZipCipherStream(input, _zipCrypto_forExtract, CryptoMode.Decrypt);
			}
			if (_Encryption_FromZipFile == EncryptionAlgorithm.WinZipAes128 || _Encryption_FromZipFile == EncryptionAlgorithm.WinZipAes256)
			{
				return new WinZipAesCipherStream(input, _aesCrypto_forExtract, _CompressedFileDataSize, CryptoMode.Decrypt);
			}
			return input;
		}

		internal void _SetTimes(string fileOrDirectory, bool isFile)
		{
		}

		internal void ValidateEncryption()
		{
			if (Encryption != EncryptionAlgorithm.PkzipWeak && Encryption != EncryptionAlgorithm.WinZipAes128 && Encryption != EncryptionAlgorithm.WinZipAes256 && Encryption != EncryptionAlgorithm.None)
			{
				if (_UnsupportedAlgorithmId != 0)
				{
					throw new ZipException(string.Format("Cannot extract: Entry {0} is encrypted with an algorithm not supported by DotNetZip: {1}", FileName, UnsupportedAlgorithm));
				}
				throw new ZipException(string.Format("Cannot extract: Entry {0} uses an unsupported encryption algorithm ({1:X2})", FileName, (int)Encryption));
			}
		}

		private void ValidateCompression()
		{
			if (_CompressionMethod_FromZipFile != 0 && _CompressionMethod_FromZipFile != 8 && _CompressionMethod_FromZipFile != 12)
			{
				throw new ZipException(string.Format("Entry {0} uses an unsupported compression method (0x{1:X2}, {2})", FileName, _CompressionMethod_FromZipFile, UnsupportedCompressionMethod));
			}
		}

		private void SetupCryptoForExtract(string password)
		{
			if (_Encryption_FromZipFile == EncryptionAlgorithm.None)
			{
				return;
			}
			if (_Encryption_FromZipFile == EncryptionAlgorithm.PkzipWeak)
			{
				if (password == null)
				{
					throw new ZipException("Missing password.");
				}
				ArchiveStream.Seek(FileDataPosition - 12, SeekOrigin.Begin);
				_zipCrypto_forExtract = ZipCrypto.ForRead(password, this);
			}
			else if (_Encryption_FromZipFile == EncryptionAlgorithm.WinZipAes128 || _Encryption_FromZipFile == EncryptionAlgorithm.WinZipAes256)
			{
				if (password == null)
				{
					throw new ZipException("Missing password.");
				}
				if (_aesCrypto_forExtract != null)
				{
					_aesCrypto_forExtract.Password = password;
					return;
				}
				int lengthOfCryptoHeaderBytes = GetLengthOfCryptoHeaderBytes(_Encryption_FromZipFile);
				ArchiveStream.Seek(FileDataPosition - lengthOfCryptoHeaderBytes, SeekOrigin.Begin);
				int keyStrengthInBits = GetKeyStrengthInBits(_Encryption_FromZipFile);
				_aesCrypto_forExtract = WinZipAesCrypto.ReadFromStream(password, keyStrengthInBits, ArchiveStream);
			}
		}

		private bool ValidateOutput(string basedir, Stream outstream, out string outFileName)
		{
			if (basedir != null)
			{
				string text = FileName.Replace(Path.DirectorySeparatorChar, '/');
				if (text.IndexOf(':') == 1)
				{
					text = text.Substring(2);
				}
				if (text.StartsWith("/"))
				{
					text = text.Substring(1);
				}
				if (_container.ZipFile.FlattenFoldersOnExtract)
				{
					outFileName = Path.Combine(basedir, (text.IndexOf('/') != -1) ? Path.GetFileName(text) : text);
				}
				else
				{
					outFileName = Path.Combine(basedir, text);
				}
				outFileName = outFileName.Replace('/', Path.DirectorySeparatorChar);
				if (IsDirectory || FileName.EndsWith("/"))
				{
					if (!Directory.Exists(outFileName))
					{
						Directory.CreateDirectory(outFileName);
						_SetTimes(outFileName, false);
					}
					else if (ExtractExistingFile == ExtractExistingFileAction.OverwriteSilently)
					{
						_SetTimes(outFileName, false);
					}
					return true;
				}
				return false;
			}
			if (outstream != null)
			{
				outFileName = null;
				if (IsDirectory || FileName.EndsWith("/"))
				{
					return true;
				}
				return false;
			}
			throw new ArgumentNullException("outstream");
		}

		private void ReadExtraField()
		{
			_readExtraDepth++;
			long position = ArchiveStream.Position;
			ArchiveStream.Seek(_RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
			byte[] array = new byte[30];
			ArchiveStream.Read(array, 0, array.Length);
			int num = 26;
			short num2 = (short)(array[num++] + array[num++] * 256);
			short extraFieldLength = (short)(array[num++] + array[num++] * 256);
			ArchiveStream.Seek(num2, SeekOrigin.Current);
			ProcessExtraField(ArchiveStream, extraFieldLength);
			ArchiveStream.Seek(position, SeekOrigin.Begin);
			_readExtraDepth--;
		}

		private static bool ReadHeader(ZipEntry ze, Encoding defaultEncoding)
		{
			int num = 0;
			ze._RelativeOffsetOfLocalHeader = ze.ArchiveStream.Position;
			int num2 = SharedUtilities.ReadEntrySignature(ze.ArchiveStream);
			num += 4;
			if (IsNotValidSig(num2))
			{
				ze.ArchiveStream.Seek(-4L, SeekOrigin.Current);
				if (IsNotValidZipDirEntrySig(num2) && (long)num2 != 101010256)
				{
					throw new BadReadException(string.Format("  Bad signature (0x{0:X8}) at position  0x{1:X8}", num2, ze.ArchiveStream.Position));
				}
				return false;
			}
			byte[] array = new byte[26];
			int num3 = ze.ArchiveStream.Read(array, 0, array.Length);
			if (num3 != array.Length)
			{
				return false;
			}
			num += num3;
			int num4 = 0;
			ze._VersionNeeded = (short)(array[num4++] + array[num4++] * 256);
			ze._BitField = (short)(array[num4++] + array[num4++] * 256);
			ze._CompressionMethod_FromZipFile = (ze._CompressionMethod = (short)(array[num4++] + array[num4++] * 256));
			ze._TimeBlob = array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256;
			ze._LastModified = SharedUtilities.PackedToDateTime(ze._TimeBlob);
			ze._timestamp |= ZipEntryTimestamp.DOS;
			if ((ze._BitField & 1) == 1)
			{
				ze._Encryption_FromZipFile = (ze._Encryption = EncryptionAlgorithm.PkzipWeak);
				ze._sourceIsEncrypted = true;
			}
			ze._Crc32 = array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256;
			ze._CompressedSize = (uint)(array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256);
			ze._UncompressedSize = (uint)(array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256);
			if ((int)ze._CompressedSize == -1 || (int)ze._UncompressedSize == -1)
			{
				ze._InputUsesZip64 = true;
			}
			short num5 = (short)(array[num4++] + array[num4++] * 256);
			short extraFieldLength = (short)(array[num4++] + array[num4++] * 256);
			array = new byte[num5];
			num3 = ze.ArchiveStream.Read(array, 0, array.Length);
			num += num3;
			if ((ze._BitField & 0x800) == 2048)
			{
				ze.AlternateEncoding = Encoding.UTF8;
				ze.AlternateEncodingUsage = ZipOption.Always;
			}
			ze._FileNameInArchive = ze.AlternateEncoding.GetString(array, 0, array.Length);
			if (ze._FileNameInArchive.EndsWith("/"))
			{
				ze.MarkAsDirectory();
			}
			num += ze.ProcessExtraField(ze.ArchiveStream, extraFieldLength);
			ze._LengthOfTrailer = 0;
			if (!ze._FileNameInArchive.EndsWith("/") && (ze._BitField & 8) == 8)
			{
				long position = ze.ArchiveStream.Position;
				bool flag = true;
				long num6 = 0L;
				int num7 = 0;
				while (flag)
				{
					num7++;
					if (ze._container.ZipFile != null)
					{
						ze._container.ZipFile.OnReadBytes(ze);
					}
					long num8 = SharedUtilities.FindSignature(ze.ArchiveStream, 134695760);
					if (num8 == -1)
					{
						return false;
					}
					num6 += num8;
					if (ze._InputUsesZip64)
					{
						array = new byte[20];
						num3 = ze.ArchiveStream.Read(array, 0, array.Length);
						if (num3 != 20)
						{
							return false;
						}
						num4 = 0;
						ze._Crc32 = array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256;
						ze._CompressedSize = BitConverter.ToInt64(array, num4);
						num4 += 8;
						ze._UncompressedSize = BitConverter.ToInt64(array, num4);
						num4 += 8;
						ze._LengthOfTrailer += 24;
					}
					else
					{
						array = new byte[12];
						num3 = ze.ArchiveStream.Read(array, 0, array.Length);
						if (num3 != 12)
						{
							return false;
						}
						num4 = 0;
						ze._Crc32 = array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256;
						ze._CompressedSize = (uint)(array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256);
						ze._UncompressedSize = (uint)(array[num4++] + array[num4++] * 256 + array[num4++] * 256 * 256 + array[num4++] * 256 * 256 * 256);
						ze._LengthOfTrailer += 16;
					}
					flag = num6 != ze._CompressedSize;
					if (flag)
					{
						ze.ArchiveStream.Seek(-12L, SeekOrigin.Current);
						num6 += 4;
					}
				}
				ze.ArchiveStream.Seek(position, SeekOrigin.Begin);
			}
			ze._CompressedFileDataSize = ze._CompressedSize;
			if ((ze._BitField & 1) == 1)
			{
				if (ze.Encryption == EncryptionAlgorithm.WinZipAes128 || ze.Encryption == EncryptionAlgorithm.WinZipAes256)
				{
					int keyStrengthInBits = GetKeyStrengthInBits(ze._Encryption_FromZipFile);
					ze._aesCrypto_forExtract = WinZipAesCrypto.ReadFromStream(null, keyStrengthInBits, ze.ArchiveStream);
					num += ze._aesCrypto_forExtract.SizeOfEncryptionMetadata - 10;
					ze._CompressedFileDataSize -= ze._aesCrypto_forExtract.SizeOfEncryptionMetadata;
					ze._LengthOfTrailer += 10;
				}
				else
				{
					ze._WeakEncryptionHeader = new byte[12];
					num += ReadWeakEncryptionHeader(ze._archiveStream, ze._WeakEncryptionHeader);
					ze._CompressedFileDataSize -= 12L;
				}
			}
			ze._LengthOfHeader = num;
			ze._TotalEntrySize = ze._LengthOfHeader + ze._CompressedFileDataSize + ze._LengthOfTrailer;
			return true;
		}

		internal static int ReadWeakEncryptionHeader(Stream s, byte[] buffer)
		{
			int num = s.Read(buffer, 0, 12);
			if (num != 12)
			{
				throw new ZipException(string.Format("Unexpected end of data at position 0x{0:X8}", s.Position));
			}
			return num;
		}

		private static bool IsNotValidSig(int signature)
		{
			return signature != 67324752;
		}

		internal static ZipEntry ReadEntry(ZipContainer zc, bool first)
		{
			ZipFile zipFile = zc.ZipFile;
			Stream readStream = zc.ReadStream;
			Encoding alternateEncoding = zc.AlternateEncoding;
			ZipEntry zipEntry = new ZipEntry();
			zipEntry._Source = ZipEntrySource.ZipFile;
			zipEntry._container = zc;
			zipEntry._archiveStream = readStream;
			if (zipFile != null)
			{
				zipFile.OnReadEntry(true, null);
			}
			if (first)
			{
				HandlePK00Prefix(readStream);
			}
			if (!ReadHeader(zipEntry, alternateEncoding))
			{
				return null;
			}
			zipEntry.__FileDataPosition = zipEntry.ArchiveStream.Position;
			readStream.Seek(zipEntry._CompressedFileDataSize + zipEntry._LengthOfTrailer, SeekOrigin.Current);
			HandleUnexpectedDataDescriptor(zipEntry);
			if (zipFile != null)
			{
				zipFile.OnReadBytes(zipEntry);
				zipFile.OnReadEntry(false, zipEntry);
			}
			return zipEntry;
		}

		internal static void HandlePK00Prefix(Stream s)
		{
			uint num = (uint)SharedUtilities.ReadInt(s);
			if (num != 808471376)
			{
				s.Seek(-4L, SeekOrigin.Current);
			}
		}

		private static void HandleUnexpectedDataDescriptor(ZipEntry entry)
		{
			Stream archiveStream = entry.ArchiveStream;
			uint num = (uint)SharedUtilities.ReadInt(archiveStream);
			if (num == entry._Crc32)
			{
				int num2 = SharedUtilities.ReadInt(archiveStream);
				if (num2 == entry._CompressedSize)
				{
					num2 = SharedUtilities.ReadInt(archiveStream);
					if (num2 != entry._UncompressedSize)
					{
						archiveStream.Seek(-12L, SeekOrigin.Current);
					}
				}
				else
				{
					archiveStream.Seek(-8L, SeekOrigin.Current);
				}
			}
			else
			{
				archiveStream.Seek(-4L, SeekOrigin.Current);
			}
		}

		internal static int FindExtraFieldSegment(byte[] extra, int offx, ushort targetHeaderId)
		{
			short num2;
			for (int i = offx; i + 3 < extra.Length; i += num2)
			{
				ushort num = (ushort)(extra[i++] + extra[i++] * 256);
				if (num == targetHeaderId)
				{
					return i - 2;
				}
				num2 = (short)(extra[i++] + extra[i++] * 256);
			}
			return -1;
		}

		internal int ProcessExtraField(Stream s, short extraFieldLength)
		{
			int num = 0;
			if (extraFieldLength > 0)
			{
				byte[] array = (_Extra = new byte[extraFieldLength]);
				num = s.Read(array, 0, array.Length);
				long posn = s.Position - num;
				int num2 = 0;
				while (num2 + 3 < array.Length)
				{
					int num3 = num2;
					ushort num4 = (ushort)(array[num2++] + array[num2++] * 256);
					short num5 = (short)(array[num2++] + array[num2++] * 256);
					switch (num4)
					{
					case 10:
						num2 = ProcessExtraFieldWindowsTimes(array, num2, num5, posn);
						break;
					case 21589:
						num2 = ProcessExtraFieldUnixTimes(array, num2, num5, posn);
						break;
					case 22613:
						num2 = ProcessExtraFieldInfoZipTimes(array, num2, num5, posn);
						break;
					case 1:
						num2 = ProcessExtraFieldZip64(array, num2, num5, posn);
						break;
					case 39169:
						num2 = ProcessExtraFieldWinZipAes(array, num2, num5, posn);
						break;
					case 23:
						num2 = ProcessExtraFieldPkwareStrongEncryption(array, num2);
						break;
					}
					num2 = num3 + num5 + 4;
				}
			}
			return num;
		}

		private int ProcessExtraFieldPkwareStrongEncryption(byte[] Buffer, int j)
		{
			j += 2;
			_UnsupportedAlgorithmId = (ushort)(Buffer[j++] + Buffer[j++] * 256);
			_Encryption_FromZipFile = (_Encryption = EncryptionAlgorithm.Unsupported);
			return j;
		}

		private int ProcessExtraFieldWinZipAes(byte[] buffer, int j, short dataSize, long posn)
		{
			if (_CompressionMethod == 99)
			{
				if ((_BitField & 1) != 1)
				{
					throw new BadReadException(string.Format("  Inconsistent metadata at position 0x{0:X16}", posn));
				}
				_sourceIsEncrypted = true;
				if (dataSize != 7)
				{
					throw new BadReadException(string.Format("  Inconsistent size (0x{0:X4}) in WinZip AES field at position 0x{1:X16}", dataSize, posn));
				}
				_WinZipAesMethod = BitConverter.ToInt16(buffer, j);
				j += 2;
				if (_WinZipAesMethod != 1 && _WinZipAesMethod != 2)
				{
					throw new BadReadException(string.Format("  Unexpected vendor version number (0x{0:X4}) for WinZip AES metadata at position 0x{1:X16}", _WinZipAesMethod, posn));
				}
				short num = BitConverter.ToInt16(buffer, j);
				j += 2;
				if (num != 17729)
				{
					throw new BadReadException(string.Format("  Unexpected vendor ID (0x{0:X4}) for WinZip AES metadata at position 0x{1:X16}", num, posn));
				}
				int num2 = ((buffer[j] == 1) ? 128 : ((buffer[j] == 3) ? 256 : (-1)));
				if (num2 < 0)
				{
					throw new BadReadException(string.Format("Invalid key strength ({0})", num2));
				}
				_Encryption_FromZipFile = (_Encryption = ((num2 == 128) ? EncryptionAlgorithm.WinZipAes128 : EncryptionAlgorithm.WinZipAes256));
				j++;
				_CompressionMethod_FromZipFile = (_CompressionMethod = BitConverter.ToInt16(buffer, j));
				j += 2;
			}
			return j;
		}

		private int ProcessExtraFieldZip64(byte[] buffer, int j, short dataSize, long posn)
		{
			_InputUsesZip64 = true;
			if (dataSize > 28)
			{
				throw new BadReadException(string.Format("  Inconsistent size (0x{0:X4}) for ZIP64 extra field at position 0x{1:X16}", dataSize, posn));
			}
			return j;
		}

		private int ProcessExtraFieldInfoZipTimes(byte[] buffer, int j, short dataSize, long posn)
		{
			if (dataSize != 12 && dataSize != 8)
			{
				throw new BadReadException(string.Format("  Unexpected size (0x{0:X4}) for InfoZip v1 extra field at position 0x{1:X16}", dataSize, posn));
			}
			int num = BitConverter.ToInt32(buffer, j);
			_Mtime = _unixEpoch.AddSeconds(num);
			j += 4;
			num = BitConverter.ToInt32(buffer, j);
			_Atime = _unixEpoch.AddSeconds(num);
			j += 4;
			_Ctime = DateTime.UtcNow;
			_ntfsTimesAreSet = true;
			_timestamp |= ZipEntryTimestamp.InfoZip1;
			return j;
		}

		private int ProcessExtraFieldUnixTimes(byte[] buffer, int j, short dataSize, long posn)
		{
			if (dataSize != 13 && dataSize != 9 && dataSize != 5)
			{
				throw new BadReadException(string.Format("  Unexpected size (0x{0:X4}) for Extended Timestamp extra field at position 0x{1:X16}", dataSize, posn));
			}
			return j;
		}

		private int ProcessExtraFieldWindowsTimes(byte[] buffer, int j, short dataSize, long posn)
		{
			if (dataSize != 32)
			{
				throw new BadReadException(string.Format("  Unexpected size (0x{0:X4}) for NTFS times extra field at position 0x{1:X16}", dataSize, posn));
			}
			j += 4;
			short num = (short)(buffer[j] + buffer[j + 1] * 256);
			short num2 = (short)(buffer[j + 2] + buffer[j + 3] * 256);
			j += 4;
			if (num == 1 && num2 == 24)
			{
				long fileTime = BitConverter.ToInt64(buffer, j);
				_Mtime = DateTime.FromFileTimeUtc(fileTime);
				j += 8;
				fileTime = BitConverter.ToInt64(buffer, j);
				_Atime = DateTime.FromFileTimeUtc(fileTime);
				j += 8;
				fileTime = BitConverter.ToInt64(buffer, j);
				_Ctime = DateTime.FromFileTimeUtc(fileTime);
				j += 8;
				_ntfsTimesAreSet = true;
				_timestamp |= ZipEntryTimestamp.Windows;
				_emitNtfsTimes = true;
			}
			return j;
		}

		internal void WriteCentralDirectoryEntry(Stream s)
		{
			byte[] array = new byte[4096];
			int num = 0;
			array[num++] = 80;
			array[num++] = 75;
			array[num++] = 1;
			array[num++] = 2;
			array[num++] = (byte)(_VersionMadeBy & 0xFF);
			array[num++] = (byte)((_VersionMadeBy & 0xFF00) >> 8);
			short num2 = (short)((VersionNeeded != 0) ? VersionNeeded : 20);
			if (!_OutputUsesZip64.HasValue)
			{
				_OutputUsesZip64 = _container.Zip64 == Zip64Option.Always;
			}
			short num3 = (short)(_OutputUsesZip64.Value ? 45 : num2);
			if (CompressionMethod == CompressionMethod.BZip2)
			{
				num3 = 46;
			}
			array[num++] = (byte)(num3 & 0xFF);
			array[num++] = (byte)((num3 & 0xFF00) >> 8);
			array[num++] = (byte)(_BitField & 0xFF);
			array[num++] = (byte)((_BitField & 0xFF00) >> 8);
			array[num++] = (byte)(_CompressionMethod & 0xFF);
			array[num++] = (byte)((_CompressionMethod & 0xFF00) >> 8);
			if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				num -= 2;
				array[num++] = 99;
				array[num++] = 0;
			}
			array[num++] = (byte)(_TimeBlob & 0xFF);
			array[num++] = (byte)((_TimeBlob & 0xFF00) >> 8);
			array[num++] = (byte)((_TimeBlob & 0xFF0000) >> 16);
			array[num++] = (byte)((_TimeBlob & 0xFF000000u) >> 24);
			array[num++] = (byte)(_Crc32 & 0xFF);
			array[num++] = (byte)((_Crc32 & 0xFF00) >> 8);
			array[num++] = (byte)((_Crc32 & 0xFF0000) >> 16);
			array[num++] = (byte)((_Crc32 & 0xFF000000u) >> 24);
			int num4 = 0;
			if (_OutputUsesZip64.Value)
			{
				for (num4 = 0; num4 < 8; num4++)
				{
					array[num++] = byte.MaxValue;
				}
			}
			else
			{
				array[num++] = (byte)(_CompressedSize & 0xFF);
				array[num++] = (byte)((_CompressedSize & 0xFF00) >> 8);
				array[num++] = (byte)((_CompressedSize & 0xFF0000) >> 16);
				array[num++] = (byte)((_CompressedSize & 0xFF000000u) >> 24);
				array[num++] = (byte)(_UncompressedSize & 0xFF);
				array[num++] = (byte)((_UncompressedSize & 0xFF00) >> 8);
				array[num++] = (byte)((_UncompressedSize & 0xFF0000) >> 16);
				array[num++] = (byte)((_UncompressedSize & 0xFF000000u) >> 24);
			}
			byte[] encodedFileNameBytes = GetEncodedFileNameBytes();
			short num5 = (short)encodedFileNameBytes.Length;
			array[num++] = (byte)(num5 & 0xFF);
			array[num++] = (byte)((num5 & 0xFF00) >> 8);
			_presumeZip64 = _OutputUsesZip64.Value;
			_Extra = ConstructExtraField(true);
			short num6 = (short)((_Extra != null) ? _Extra.Length : 0);
			array[num++] = (byte)(num6 & 0xFF);
			array[num++] = (byte)((num6 & 0xFF00) >> 8);
			int num7 = ((_CommentBytes != null) ? _CommentBytes.Length : 0);
			if (num7 + num > array.Length)
			{
				num7 = array.Length - num;
			}
			array[num++] = (byte)(num7 & 0xFF);
			array[num++] = (byte)((num7 & 0xFF00) >> 8);
			if (_container.ZipFile != null && _container.ZipFile.MaxOutputSegmentSize != 0)
			{
				array[num++] = (byte)(_diskNumber & 0xFF);
				array[num++] = (byte)((_diskNumber & 0xFF00) >> 8);
			}
			else
			{
				array[num++] = 0;
				array[num++] = 0;
			}
			array[num++] = (byte)(_IsText ? 1u : 0u);
			array[num++] = 0;
			array[num++] = (byte)(_ExternalFileAttrs & 0xFF);
			array[num++] = (byte)((_ExternalFileAttrs & 0xFF00) >> 8);
			array[num++] = (byte)((_ExternalFileAttrs & 0xFF0000) >> 16);
			array[num++] = (byte)((_ExternalFileAttrs & 0xFF000000u) >> 24);
			if (_RelativeOffsetOfLocalHeader > uint.MaxValue)
			{
				array[num++] = byte.MaxValue;
				array[num++] = byte.MaxValue;
				array[num++] = byte.MaxValue;
				array[num++] = byte.MaxValue;
			}
			else
			{
				array[num++] = (byte)(_RelativeOffsetOfLocalHeader & 0xFF);
				array[num++] = (byte)((_RelativeOffsetOfLocalHeader & 0xFF00) >> 8);
				array[num++] = (byte)((_RelativeOffsetOfLocalHeader & 0xFF0000) >> 16);
				array[num++] = (byte)((_RelativeOffsetOfLocalHeader & 0xFF000000u) >> 24);
			}
			Buffer.BlockCopy(encodedFileNameBytes, 0, array, num, num5);
			num += num5;
			if (_Extra != null)
			{
				byte[] extra = _Extra;
				int srcOffset = 0;
				Buffer.BlockCopy(extra, srcOffset, array, num, num6);
				num += num6;
			}
			if (num7 != 0)
			{
				Buffer.BlockCopy(_CommentBytes, 0, array, num, num7);
				num += num7;
			}
			s.Write(array, 0, num);
		}

		private byte[] ConstructExtraField(bool forCentralDirectory)
		{
			List<byte[]> list = new List<byte[]>();
			if (_container.Zip64 == Zip64Option.Always || (_container.Zip64 == Zip64Option.AsNecessary && (!forCentralDirectory || _entryRequiresZip64.Value)))
			{
				int num = 4 + (forCentralDirectory ? 28 : 16);
				byte[] array = new byte[num];
				int num2 = 0;
				if (_presumeZip64 || forCentralDirectory)
				{
					array[num2++] = 1;
					array[num2++] = 0;
				}
				else
				{
					array[num2++] = 153;
					array[num2++] = 153;
				}
				array[num2++] = (byte)(num - 4);
				array[num2++] = 0;
				Array.Copy(BitConverter.GetBytes(_UncompressedSize), 0, array, num2, 8);
				num2 += 8;
				Array.Copy(BitConverter.GetBytes(_CompressedSize), 0, array, num2, 8);
				num2 += 8;
				if (forCentralDirectory)
				{
					Array.Copy(BitConverter.GetBytes(_RelativeOffsetOfLocalHeader), 0, array, num2, 8);
					num2 += 8;
					Array.Copy(BitConverter.GetBytes(0), 0, array, num2, 4);
				}
				list.Add(array);
			}
			if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				byte[] array = new byte[11];
				int num3 = 0;
				array[num3++] = 1;
				array[num3++] = 153;
				array[num3++] = 7;
				array[num3++] = 0;
				array[num3++] = 1;
				array[num3++] = 0;
				array[num3++] = 65;
				array[num3++] = 69;
				switch (GetKeyStrengthInBits(Encryption))
				{
				case 128:
					array[num3] = 1;
					break;
				case 256:
					array[num3] = 3;
					break;
				default:
					array[num3] = byte.MaxValue;
					break;
				}
				num3++;
				array[num3++] = (byte)(_CompressionMethod & 0xFF);
				array[num3++] = (byte)(_CompressionMethod & 0xFF00);
				list.Add(array);
			}
			if (_ntfsTimesAreSet && _emitNtfsTimes)
			{
				byte[] array = new byte[36];
				int num4 = 0;
				array[num4++] = 10;
				array[num4++] = 0;
				array[num4++] = 32;
				array[num4++] = 0;
				num4 += 4;
				array[num4++] = 1;
				array[num4++] = 0;
				array[num4++] = 24;
				array[num4++] = 0;
				long value = _Mtime.ToFileTime();
				Array.Copy(BitConverter.GetBytes(value), 0, array, num4, 8);
				num4 += 8;
				value = _Atime.ToFileTime();
				Array.Copy(BitConverter.GetBytes(value), 0, array, num4, 8);
				num4 += 8;
				value = _Ctime.ToFileTime();
				Array.Copy(BitConverter.GetBytes(value), 0, array, num4, 8);
				num4 += 8;
				list.Add(array);
			}
			if (_ntfsTimesAreSet && _emitUnixTimes)
			{
				int num5 = 9;
				if (!forCentralDirectory)
				{
					num5 += 8;
				}
				byte[] array = new byte[num5];
				int num6 = 0;
				array[num6++] = 85;
				array[num6++] = 84;
				array[num6++] = (byte)(num5 - 4);
				array[num6++] = 0;
				array[num6++] = 7;
				int value2 = (int)(_Mtime - _unixEpoch).TotalSeconds;
				Array.Copy(BitConverter.GetBytes(value2), 0, array, num6, 4);
				num6 += 4;
				if (!forCentralDirectory)
				{
					value2 = (int)(_Atime - _unixEpoch).TotalSeconds;
					Array.Copy(BitConverter.GetBytes(value2), 0, array, num6, 4);
					num6 += 4;
					value2 = (int)(_Ctime - _unixEpoch).TotalSeconds;
					Array.Copy(BitConverter.GetBytes(value2), 0, array, num6, 4);
					num6 += 4;
				}
				list.Add(array);
			}
			byte[] array2 = null;
			if (list.Count > 0)
			{
				int num7 = 0;
				int num8 = 0;
				for (int i = 0; i < list.Count; i++)
				{
					num7 += list[i].Length;
				}
				array2 = new byte[num7];
				for (int i = 0; i < list.Count; i++)
				{
					Array.Copy(list[i], 0, array2, num8, list[i].Length);
					num8 += list[i].Length;
				}
			}
			return array2;
		}

		private string NormalizeFileName()
		{
			string text = FileName.Replace("\\", "/");
			string text2 = null;
			if (_TrimVolumeFromFullyQualifiedPaths && FileName.Length >= 3 && FileName[1] == ':' && text[2] == '/')
			{
				return text.Substring(3);
			}
			if (FileName.Length >= 4 && text[0] == '/' && text[1] == '/')
			{
				int num = text.IndexOf('/', 2);
				if (num == -1)
				{
					throw new ArgumentException("The path for that entry appears to be badly formatted");
				}
				return text.Substring(num + 1);
			}
			if (FileName.Length >= 3 && text[0] == '.' && text[1] == '/')
			{
				return text.Substring(2);
			}
			return text;
		}

		private byte[] GetEncodedFileNameBytes()
		{
			string text = NormalizeFileName();
			switch (AlternateEncodingUsage)
			{
			case ZipOption.Always:
				if (_Comment != null && _Comment.Length != 0)
				{
					_CommentBytes = AlternateEncoding.GetBytes(_Comment);
				}
				_actualEncoding = AlternateEncoding;
				return AlternateEncoding.GetBytes(text);
			case ZipOption.Default:
				if (_Comment != null && _Comment.Length != 0)
				{
					_CommentBytes = ibm437.GetBytes(_Comment);
				}
				_actualEncoding = ibm437;
				return ibm437.GetBytes(text);
			default:
			{
				byte[] bytes = ibm437.GetBytes(text);
				string text2 = ibm437.GetString(bytes, 0, bytes.Length);
				_CommentBytes = null;
				if (text2 != text)
				{
					bytes = AlternateEncoding.GetBytes(text);
					if (_Comment != null && _Comment.Length != 0)
					{
						_CommentBytes = AlternateEncoding.GetBytes(_Comment);
					}
					_actualEncoding = AlternateEncoding;
					return bytes;
				}
				_actualEncoding = ibm437;
				if (_Comment == null || _Comment.Length == 0)
				{
					return bytes;
				}
				byte[] bytes2 = ibm437.GetBytes(_Comment);
				string text3 = ibm437.GetString(bytes2, 0, bytes2.Length);
				if (text3 != Comment)
				{
					bytes = AlternateEncoding.GetBytes(text);
					_CommentBytes = AlternateEncoding.GetBytes(_Comment);
					_actualEncoding = AlternateEncoding;
					return bytes;
				}
				_CommentBytes = bytes2;
				return bytes;
			}
			}
		}

		private bool WantReadAgain()
		{
			if (_UncompressedSize < 16)
			{
				return false;
			}
			if (_CompressionMethod == 0)
			{
				return false;
			}
			if (CompressionLevel == CompressionLevel.None)
			{
				return false;
			}
			if (_CompressedSize < _UncompressedSize)
			{
				return false;
			}
			if (_Source == ZipEntrySource.Stream && !_sourceStream.CanSeek)
			{
				return false;
			}
			if (_aesCrypto_forWrite != null && CompressedSize - _aesCrypto_forWrite.SizeOfEncryptionMetadata <= UncompressedSize + 16)
			{
				return false;
			}
			if (_zipCrypto_forWrite != null && CompressedSize - 12 <= UncompressedSize)
			{
				return false;
			}
			return true;
		}

		private void MaybeUnsetCompressionMethodForWriting(int cycle)
		{
			if (cycle > 1)
			{
				_CompressionMethod = 0;
			}
			else if (IsDirectory)
			{
				_CompressionMethod = 0;
			}
			else
			{
				if (_Source == ZipEntrySource.ZipFile)
				{
					return;
				}
				if (_Source == ZipEntrySource.Stream)
				{
					if (_sourceStream != null && _sourceStream.CanSeek)
					{
						long length = _sourceStream.Length;
						if (length == 0)
						{
							_CompressionMethod = 0;
							return;
						}
					}
				}
				else if (_Source == ZipEntrySource.FileSystem && SharedUtilities.GetFileLength(LocalFileName) == 0)
				{
					_CompressionMethod = 0;
					return;
				}
				if (SetCompression != null)
				{
					CompressionLevel = SetCompression(LocalFileName, _FileNameInArchive);
				}
				if (CompressionLevel == CompressionLevel.None && CompressionMethod == CompressionMethod.Deflate)
				{
					_CompressionMethod = 0;
				}
			}
		}

		internal void WriteHeader(Stream s, int cycle)
		{
			CountingStream countingStream = s as CountingStream;
			_future_ROLH = ((countingStream != null) ? countingStream.ComputedPosition : s.Position);
			int num = 0;
			int num2 = 0;
			byte[] array = new byte[30];
			array[num2++] = 80;
			array[num2++] = 75;
			array[num2++] = 3;
			array[num2++] = 4;
			_presumeZip64 = _container.Zip64 == Zip64Option.Always || (_container.Zip64 == Zip64Option.AsNecessary && !s.CanSeek);
			short num3 = (short)(_presumeZip64 ? 45 : 20);
			if (CompressionMethod == CompressionMethod.BZip2)
			{
				num3 = 46;
			}
			array[num2++] = (byte)(num3 & 0xFF);
			array[num2++] = (byte)((num3 & 0xFF00) >> 8);
			byte[] encodedFileNameBytes = GetEncodedFileNameBytes();
			short num4 = (short)encodedFileNameBytes.Length;
			if (_Encryption == EncryptionAlgorithm.None)
			{
				_BitField &= -2;
			}
			else
			{
				_BitField |= 1;
			}
			if (_actualEncoding.WebName == "utf-8")
			{
				_BitField |= 2048;
			}
			if (IsDirectory || cycle == 99)
			{
				_BitField &= -9;
				_BitField &= -2;
				Encryption = EncryptionAlgorithm.None;
				Password = null;
			}
			else if (!s.CanSeek)
			{
				_BitField |= 8;
			}
			array[num2++] = (byte)(_BitField & 0xFF);
			array[num2++] = (byte)((_BitField & 0xFF00) >> 8);
			if (__FileDataPosition == -1)
			{
				_CompressedSize = 0L;
				_crcCalculated = false;
			}
			MaybeUnsetCompressionMethodForWriting(cycle);
			array[num2++] = (byte)(_CompressionMethod & 0xFF);
			array[num2++] = (byte)((_CompressionMethod & 0xFF00) >> 8);
			if (cycle == 99)
			{
				SetZip64Flags();
			}
			else if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				num2 -= 2;
				array[num2++] = 99;
				array[num2++] = 0;
			}
			_TimeBlob = SharedUtilities.DateTimeToPacked(LastModified);
			array[num2++] = (byte)(_TimeBlob & 0xFF);
			array[num2++] = (byte)((_TimeBlob & 0xFF00) >> 8);
			array[num2++] = (byte)((_TimeBlob & 0xFF0000) >> 16);
			array[num2++] = (byte)((_TimeBlob & 0xFF000000u) >> 24);
			array[num2++] = (byte)(_Crc32 & 0xFF);
			array[num2++] = (byte)((_Crc32 & 0xFF00) >> 8);
			array[num2++] = (byte)((_Crc32 & 0xFF0000) >> 16);
			array[num2++] = (byte)((_Crc32 & 0xFF000000u) >> 24);
			if (_presumeZip64)
			{
				for (num = 0; num < 8; num++)
				{
					array[num2++] = byte.MaxValue;
				}
			}
			else
			{
				array[num2++] = (byte)(_CompressedSize & 0xFF);
				array[num2++] = (byte)((_CompressedSize & 0xFF00) >> 8);
				array[num2++] = (byte)((_CompressedSize & 0xFF0000) >> 16);
				array[num2++] = (byte)((_CompressedSize & 0xFF000000u) >> 24);
				array[num2++] = (byte)(_UncompressedSize & 0xFF);
				array[num2++] = (byte)((_UncompressedSize & 0xFF00) >> 8);
				array[num2++] = (byte)((_UncompressedSize & 0xFF0000) >> 16);
				array[num2++] = (byte)((_UncompressedSize & 0xFF000000u) >> 24);
			}
			array[num2++] = (byte)(num4 & 0xFF);
			array[num2++] = (byte)((num4 & 0xFF00) >> 8);
			_Extra = ConstructExtraField(false);
			short num5 = (short)((_Extra != null) ? _Extra.Length : 0);
			array[num2++] = (byte)(num5 & 0xFF);
			array[num2++] = (byte)((num5 & 0xFF00) >> 8);
			byte[] array2 = new byte[num2 + num4 + num5];
			Buffer.BlockCopy(array, 0, array2, 0, num2);
			Buffer.BlockCopy(encodedFileNameBytes, 0, array2, num2, encodedFileNameBytes.Length);
			num2 += encodedFileNameBytes.Length;
			if (_Extra != null)
			{
				Buffer.BlockCopy(_Extra, 0, array2, num2, _Extra.Length);
				num2 += _Extra.Length;
			}
			_LengthOfHeader = num2;
			ZipSegmentedStream zipSegmentedStream = s as ZipSegmentedStream;
			if (zipSegmentedStream != null)
			{
				zipSegmentedStream.ContiguousWrite = true;
				uint num6 = zipSegmentedStream.ComputeSegment(num2);
				if (num6 != zipSegmentedStream.CurrentSegment)
				{
					_future_ROLH = 0L;
				}
				else
				{
					_future_ROLH = zipSegmentedStream.Position;
				}
				_diskNumber = num6;
			}
			if (_container.Zip64 == Zip64Option.Default && (uint)_RelativeOffsetOfLocalHeader >= uint.MaxValue)
			{
				throw new ZipException("Offset within the zip archive exceeds 0xFFFFFFFF. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");
			}
			s.Write(array2, 0, num2);
			if (zipSegmentedStream != null)
			{
				zipSegmentedStream.ContiguousWrite = false;
			}
			_EntryHeader = array2;
		}

		private int FigureCrc32()
		{
			if (!_crcCalculated)
			{
				Stream stream = null;
				if (_Source == ZipEntrySource.WriteDelegate)
				{
					CrcCalculatorStream crcCalculatorStream = new CrcCalculatorStream(Stream.Null);
					_WriteDelegate(FileName, crcCalculatorStream);
					_Crc32 = crcCalculatorStream.Crc;
				}
				else if (_Source != ZipEntrySource.ZipFile)
				{
					if (_Source == ZipEntrySource.Stream)
					{
						PrepSourceStream();
						stream = _sourceStream;
					}
					else if (_Source == ZipEntrySource.JitStream)
					{
						if (_sourceStream == null)
						{
							_sourceStream = _OpenDelegate(FileName);
						}
						PrepSourceStream();
						stream = _sourceStream;
					}
					else if (_Source != ZipEntrySource.ZipOutputStream)
					{
						stream = File.Open(LocalFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					}
					CRC32 cRC = new CRC32();
					_Crc32 = cRC.GetCrc32(stream);
					if (_sourceStream == null)
					{
						stream.Dispose();
					}
				}
				_crcCalculated = true;
			}
			return _Crc32;
		}

		private void PrepSourceStream()
		{
			if (_sourceStream == null)
			{
				throw new ZipException(string.Format("The input stream is null for entry '{0}'.", FileName));
			}
			if (_sourceStreamOriginalPosition.HasValue)
			{
				_sourceStream.Position = _sourceStreamOriginalPosition.Value;
			}
			else if (_sourceStream.CanSeek)
			{
				_sourceStreamOriginalPosition = _sourceStream.Position;
			}
			else if (Encryption == EncryptionAlgorithm.PkzipWeak && _Source != ZipEntrySource.ZipFile && (_BitField & 8) != 8)
			{
				throw new ZipException("It is not possible to use PKZIP encryption on a non-seekable input stream");
			}
		}

		internal void CopyMetaData(ZipEntry source)
		{
			__FileDataPosition = source.__FileDataPosition;
			CompressionMethod = source.CompressionMethod;
			_CompressionMethod_FromZipFile = source._CompressionMethod_FromZipFile;
			_CompressedFileDataSize = source._CompressedFileDataSize;
			_UncompressedSize = source._UncompressedSize;
			_BitField = source._BitField;
			_Source = source._Source;
			_LastModified = source._LastModified;
			_Mtime = source._Mtime;
			_Atime = source._Atime;
			_Ctime = source._Ctime;
			_ntfsTimesAreSet = source._ntfsTimesAreSet;
			_emitUnixTimes = source._emitUnixTimes;
			_emitNtfsTimes = source._emitNtfsTimes;
		}

		private void OnWriteBlock(long bytesXferred, long totalBytesToXfer)
		{
			if (_container.ZipFile != null)
			{
				_ioOperationCanceled = _container.ZipFile.OnSaveBlock(this, bytesXferred, totalBytesToXfer);
			}
		}

		private void _WriteEntryData(Stream s)
		{
			Stream input = null;
			long _FileDataPosition = -1L;
			try
			{
				_FileDataPosition = s.Position;
			}
			catch (Exception)
			{
			}
			try
			{
				long num = SetInputAndFigureFileLength(ref input);
				CountingStream countingStream = new CountingStream(s);
				Stream stream;
				Stream stream2;
				if (num != 0)
				{
					stream = MaybeApplyEncryption(countingStream);
					stream2 = MaybeApplyCompression(stream, num);
				}
				else
				{
					stream = (stream2 = countingStream);
				}
				CrcCalculatorStream crcCalculatorStream = new CrcCalculatorStream(stream2, true);
				if (_Source == ZipEntrySource.WriteDelegate)
				{
					_WriteDelegate(FileName, crcCalculatorStream);
				}
				else
				{
					byte[] array = new byte[BufferSize];
					int count;
					while ((count = SharedUtilities.ReadWithRetry(input, array, 0, array.Length, FileName)) != 0)
					{
						crcCalculatorStream.Write(array, 0, count);
						OnWriteBlock(crcCalculatorStream.TotalBytesSlurped, num);
						if (_ioOperationCanceled)
						{
							break;
						}
					}
				}
				FinishOutputStream(s, countingStream, stream, stream2, crcCalculatorStream);
			}
			finally
			{
				if (_Source == ZipEntrySource.JitStream)
				{
					if (_CloseDelegate != null)
					{
						_CloseDelegate(FileName, input);
					}
				}
				else if (input is FileStream)
				{
					input.Dispose();
				}
			}
			if (!_ioOperationCanceled)
			{
				__FileDataPosition = _FileDataPosition;
				PostProcessOutput(s);
			}
		}

		private long SetInputAndFigureFileLength(ref Stream input)
		{
			long result = -1L;
			if (_Source == ZipEntrySource.Stream)
			{
				PrepSourceStream();
				input = _sourceStream;
				try
				{
					result = _sourceStream.Length;
				}
				catch (NotSupportedException)
				{
				}
			}
			else if (_Source == ZipEntrySource.ZipFile)
			{
				string password = ((_Encryption_FromZipFile == EncryptionAlgorithm.None) ? null : (_Password ?? _container.Password));
				_sourceStream = InternalOpenReader(password);
				PrepSourceStream();
				input = _sourceStream;
				result = _sourceStream.Length;
			}
			else if (_Source == ZipEntrySource.JitStream)
			{
				if (_sourceStream == null)
				{
					_sourceStream = _OpenDelegate(FileName);
				}
				PrepSourceStream();
				input = _sourceStream;
				try
				{
					result = _sourceStream.Length;
				}
				catch (NotSupportedException)
				{
				}
			}
			else if (_Source == ZipEntrySource.FileSystem)
			{
				FileShare fileShare = FileShare.ReadWrite;
				fileShare |= FileShare.Delete;
				input = File.Open(LocalFileName, FileMode.Open, FileAccess.Read, fileShare);
				result = input.Length;
			}
			return result;
		}

		internal void FinishOutputStream(Stream s, CountingStream entryCounter, Stream encryptor, Stream compressor, CrcCalculatorStream output)
		{
			if (output != null)
			{
				output.Close();
				if (compressor is DeflateStream)
				{
					compressor.Close();
				}
				else if (compressor is BZip2OutputStream)
				{
					compressor.Close();
				}
				else if (compressor is ParallelBZip2OutputStream)
				{
					compressor.Close();
				}
				else if (compressor is ParallelDeflateOutputStream)
				{
					compressor.Close();
				}
				encryptor.Flush();
				encryptor.Close();
				_LengthOfTrailer = 0;
				_UncompressedSize = output.TotalBytesSlurped;
				WinZipAesCipherStream winZipAesCipherStream = encryptor as WinZipAesCipherStream;
				if (winZipAesCipherStream != null && _UncompressedSize > 0)
				{
					s.Write(winZipAesCipherStream.FinalAuthentication, 0, 10);
					_LengthOfTrailer += 10;
				}
				_CompressedFileDataSize = entryCounter.BytesWritten;
				_CompressedSize = _CompressedFileDataSize;
				_Crc32 = output.Crc;
				StoreRelativeOffset();
			}
		}

		internal void PostProcessOutput(Stream s)
		{
			CountingStream countingStream = s as CountingStream;
			if (_UncompressedSize == 0 && _CompressedSize == 0)
			{
				if (_Source == ZipEntrySource.ZipOutputStream)
				{
					return;
				}
				if (_Password != null)
				{
					int num = 0;
					if (Encryption == EncryptionAlgorithm.PkzipWeak)
					{
						num = 12;
					}
					else if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
					{
						num = _aesCrypto_forWrite._Salt.Length + _aesCrypto_forWrite.GeneratedPV.Length;
					}
					if (_Source == ZipEntrySource.ZipOutputStream && !s.CanSeek)
					{
						throw new ZipException("Zero bytes written, encryption in use, and non-seekable output.");
					}
					if (Encryption != EncryptionAlgorithm.None)
					{
						s.Seek(-1 * num, SeekOrigin.Current);
						s.SetLength(s.Position);
						if (countingStream != null)
						{
							countingStream.Adjust(num);
						}
						_LengthOfHeader -= num;
						__FileDataPosition -= num;
					}
					_Password = null;
					_BitField &= -2;
					int num2 = 6;
					_EntryHeader[num2++] = (byte)(_BitField & 0xFF);
					_EntryHeader[num2++] = (byte)((_BitField & 0xFF00) >> 8);
					if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
					{
						short num3 = (short)(_EntryHeader[26] + _EntryHeader[27] * 256);
						int offx = 30 + num3;
						int num4 = FindExtraFieldSegment(_EntryHeader, offx, 39169);
						if (num4 >= 0)
						{
							_EntryHeader[num4++] = 153;
							_EntryHeader[num4++] = 153;
						}
					}
				}
				CompressionMethod = CompressionMethod.None;
				Encryption = EncryptionAlgorithm.None;
			}
			else if (_zipCrypto_forWrite != null || _aesCrypto_forWrite != null)
			{
				if (Encryption == EncryptionAlgorithm.PkzipWeak)
				{
					_CompressedSize += 12L;
				}
				else if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
				{
					_CompressedSize += _aesCrypto_forWrite.SizeOfEncryptionMetadata;
				}
			}
			int num5 = 8;
			_EntryHeader[num5++] = (byte)(_CompressionMethod & 0xFF);
			_EntryHeader[num5++] = (byte)((_CompressionMethod & 0xFF00) >> 8);
			num5 = 14;
			_EntryHeader[num5++] = (byte)(_Crc32 & 0xFF);
			_EntryHeader[num5++] = (byte)((_Crc32 & 0xFF00) >> 8);
			_EntryHeader[num5++] = (byte)((_Crc32 & 0xFF0000) >> 16);
			_EntryHeader[num5++] = (byte)((_Crc32 & 0xFF000000u) >> 24);
			SetZip64Flags();
			short num6 = (short)(_EntryHeader[26] + _EntryHeader[27] * 256);
			short num7 = (short)(_EntryHeader[28] + _EntryHeader[29] * 256);
			if (_OutputUsesZip64.Value)
			{
				_EntryHeader[4] = 45;
				_EntryHeader[5] = 0;
				for (int i = 0; i < 8; i++)
				{
					_EntryHeader[num5++] = byte.MaxValue;
				}
				num5 = 30 + num6;
				_EntryHeader[num5++] = 1;
				_EntryHeader[num5++] = 0;
				num5 += 2;
				Array.Copy(BitConverter.GetBytes(_UncompressedSize), 0, _EntryHeader, num5, 8);
				num5 += 8;
				Array.Copy(BitConverter.GetBytes(_CompressedSize), 0, _EntryHeader, num5, 8);
			}
			else
			{
				_EntryHeader[4] = 20;
				_EntryHeader[5] = 0;
				num5 = 18;
				_EntryHeader[num5++] = (byte)(_CompressedSize & 0xFF);
				_EntryHeader[num5++] = (byte)((_CompressedSize & 0xFF00) >> 8);
				_EntryHeader[num5++] = (byte)((_CompressedSize & 0xFF0000) >> 16);
				_EntryHeader[num5++] = (byte)((_CompressedSize & 0xFF000000u) >> 24);
				_EntryHeader[num5++] = (byte)(_UncompressedSize & 0xFF);
				_EntryHeader[num5++] = (byte)((_UncompressedSize & 0xFF00) >> 8);
				_EntryHeader[num5++] = (byte)((_UncompressedSize & 0xFF0000) >> 16);
				_EntryHeader[num5++] = (byte)((_UncompressedSize & 0xFF000000u) >> 24);
				if (num7 != 0)
				{
					num5 = 30 + num6;
					short num8 = (short)(_EntryHeader[num5 + 2] + _EntryHeader[num5 + 3] * 256);
					if (num8 == 16)
					{
						_EntryHeader[num5++] = 153;
						_EntryHeader[num5++] = 153;
					}
				}
			}
			if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				num5 = 8;
				_EntryHeader[num5++] = 99;
				_EntryHeader[num5++] = 0;
				num5 = 30 + num6;
				do
				{
					ushort num9 = (ushort)(_EntryHeader[num5] + _EntryHeader[num5 + 1] * 256);
					short num10 = (short)(_EntryHeader[num5 + 2] + _EntryHeader[num5 + 3] * 256);
					if (num9 != 39169)
					{
						num5 += num10 + 4;
						continue;
					}
					num5 += 9;
					_EntryHeader[num5++] = (byte)(_CompressionMethod & 0xFF);
					_EntryHeader[num5++] = (byte)(_CompressionMethod & 0xFF00);
				}
				while (num5 < num7 - 30 - num6);
			}
			if ((_BitField & 8) != 8 || (_Source == ZipEntrySource.ZipOutputStream && s.CanSeek))
			{
				ZipSegmentedStream zipSegmentedStream = s as ZipSegmentedStream;
				if (zipSegmentedStream != null && _diskNumber != zipSegmentedStream.CurrentSegment)
				{
					using (Stream stream = ZipSegmentedStream.ForUpdate(_container.ZipFile.Name, _diskNumber))
					{
						stream.Seek(_RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
						stream.Write(_EntryHeader, 0, _EntryHeader.Length);
					}
				}
				else
				{
					s.Seek(_RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
					s.Write(_EntryHeader, 0, _EntryHeader.Length);
					if (countingStream != null)
					{
						countingStream.Adjust(_EntryHeader.Length);
					}
					s.Seek(_CompressedSize, SeekOrigin.Current);
				}
			}
			if ((_BitField & 8) == 8 && !IsDirectory)
			{
				byte[] array = new byte[16 + (_OutputUsesZip64.Value ? 8 : 0)];
				num5 = 0;
				Array.Copy(BitConverter.GetBytes(134695760), 0, array, num5, 4);
				num5 += 4;
				Array.Copy(BitConverter.GetBytes(_Crc32), 0, array, num5, 4);
				num5 += 4;
				if (_OutputUsesZip64.Value)
				{
					Array.Copy(BitConverter.GetBytes(_CompressedSize), 0, array, num5, 8);
					num5 += 8;
					Array.Copy(BitConverter.GetBytes(_UncompressedSize), 0, array, num5, 8);
					num5 += 8;
				}
				else
				{
					array[num5++] = (byte)(_CompressedSize & 0xFF);
					array[num5++] = (byte)((_CompressedSize & 0xFF00) >> 8);
					array[num5++] = (byte)((_CompressedSize & 0xFF0000) >> 16);
					array[num5++] = (byte)((_CompressedSize & 0xFF000000u) >> 24);
					array[num5++] = (byte)(_UncompressedSize & 0xFF);
					array[num5++] = (byte)((_UncompressedSize & 0xFF00) >> 8);
					array[num5++] = (byte)((_UncompressedSize & 0xFF0000) >> 16);
					array[num5++] = (byte)((_UncompressedSize & 0xFF000000u) >> 24);
				}
				s.Write(array, 0, array.Length);
				_LengthOfTrailer += array.Length;
			}
		}

		private void SetZip64Flags()
		{
			_entryRequiresZip64 = _CompressedSize >= uint.MaxValue || _UncompressedSize >= uint.MaxValue || _RelativeOffsetOfLocalHeader >= uint.MaxValue;
			if (_container.Zip64 == Zip64Option.Default && _entryRequiresZip64.Value)
			{
				throw new ZipException("Compressed or Uncompressed size, or offset exceeds the maximum value. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");
			}
			_OutputUsesZip64 = _container.Zip64 == Zip64Option.Always || _entryRequiresZip64.Value;
		}

		internal void PrepOutputStream(Stream s, long streamLength, out CountingStream outputCounter, out Stream encryptor, out Stream compressor, out CrcCalculatorStream output)
		{
			outputCounter = new CountingStream(s);
			if (streamLength != 0)
			{
				encryptor = MaybeApplyEncryption(outputCounter);
				compressor = MaybeApplyCompression(encryptor, streamLength);
			}
			else
			{
				encryptor = (compressor = outputCounter);
			}
			output = new CrcCalculatorStream(compressor, true);
		}

		private Stream MaybeApplyCompression(Stream s, long streamLength)
		{
			if (_CompressionMethod == 8 && CompressionLevel != CompressionLevel.None)
			{
				if (_container.ParallelDeflateThreshold == 0 || (streamLength > _container.ParallelDeflateThreshold && _container.ParallelDeflateThreshold > 0))
				{
					if (_container.ParallelDeflater == null)
					{
						_container.ParallelDeflater = new ParallelDeflateOutputStream(s, CompressionLevel, _container.Strategy, true);
						if (_container.CodecBufferSize > 0)
						{
							_container.ParallelDeflater.BufferSize = _container.CodecBufferSize;
						}
						if (_container.ParallelDeflateMaxBufferPairs > 0)
						{
							_container.ParallelDeflater.MaxBufferPairs = _container.ParallelDeflateMaxBufferPairs;
						}
					}
					ParallelDeflateOutputStream parallelDeflater = _container.ParallelDeflater;
					parallelDeflater.Reset(s);
					return parallelDeflater;
				}
				DeflateStream deflateStream = new DeflateStream(s, CompressionMode.Compress, CompressionLevel, true);
				if (_container.CodecBufferSize > 0)
				{
					deflateStream.BufferSize = _container.CodecBufferSize;
				}
				deflateStream.Strategy = _container.Strategy;
				return deflateStream;
			}
			if (_CompressionMethod == 12)
			{
				if (_container.ParallelDeflateThreshold == 0 || (streamLength > _container.ParallelDeflateThreshold && _container.ParallelDeflateThreshold > 0))
				{
					return new ParallelBZip2OutputStream(s, true);
				}
				return new BZip2OutputStream(s, true);
			}
			return s;
		}

		private Stream MaybeApplyEncryption(Stream s)
		{
			if (Encryption == EncryptionAlgorithm.PkzipWeak)
			{
				return new ZipCipherStream(s, _zipCrypto_forWrite, CryptoMode.Encrypt);
			}
			if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				return new WinZipAesCipherStream(s, _aesCrypto_forWrite, CryptoMode.Encrypt);
			}
			return s;
		}

		private void OnZipErrorWhileSaving(Exception e)
		{
			if (_container.ZipFile != null)
			{
				_ioOperationCanceled = _container.ZipFile.OnZipErrorSaving(this, e);
			}
		}

		internal void Write(Stream s)
		{
			CountingStream countingStream = s as CountingStream;
			ZipSegmentedStream zipSegmentedStream = s as ZipSegmentedStream;
			bool flag = false;
			do
			{
				try
				{
					if (_Source == ZipEntrySource.ZipFile && !_restreamRequiredOnSave)
					{
						CopyThroughOneEntry(s);
						break;
					}
					if (IsDirectory)
					{
						WriteHeader(s, 1);
						StoreRelativeOffset();
						_entryRequiresZip64 = _RelativeOffsetOfLocalHeader >= uint.MaxValue;
						_OutputUsesZip64 = _container.Zip64 == Zip64Option.Always || _entryRequiresZip64.Value;
						if (zipSegmentedStream != null)
						{
							_diskNumber = zipSegmentedStream.CurrentSegment;
						}
						break;
					}
					bool flag2 = true;
					int num = 0;
					do
					{
						num++;
						WriteHeader(s, num);
						WriteSecurityMetadata(s);
						_WriteEntryData(s);
						_TotalEntrySize = _LengthOfHeader + _CompressedFileDataSize + _LengthOfTrailer;
						flag2 = num <= 1 && s.CanSeek && WantReadAgain();
						if (flag2)
						{
							if (zipSegmentedStream != null)
							{
								zipSegmentedStream.TruncateBackward(_diskNumber, _RelativeOffsetOfLocalHeader);
							}
							else
							{
								s.Seek(_RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
							}
							s.SetLength(s.Position);
							if (countingStream != null)
							{
								countingStream.Adjust(_TotalEntrySize);
							}
						}
					}
					while (flag2);
					_skippedDuringSave = false;
					flag = true;
				}
				catch (Exception ex)
				{
					ZipErrorAction zipErrorAction = ZipErrorAction;
					int num2 = 0;
					while (true)
					{
						if (ZipErrorAction == ZipErrorAction.Throw)
						{
							throw;
						}
						if (ZipErrorAction == ZipErrorAction.Skip || ZipErrorAction == ZipErrorAction.Retry)
						{
							long num3 = ((countingStream != null) ? countingStream.ComputedPosition : s.Position);
							long num4 = num3 - _future_ROLH;
							if (num4 > 0)
							{
								s.Seek(num4, SeekOrigin.Current);
								long position = s.Position;
								s.SetLength(s.Position);
								if (countingStream != null)
								{
									countingStream.Adjust(num3 - position);
								}
							}
							if (ZipErrorAction == ZipErrorAction.Skip)
							{
								WriteStatus("Skipping file {0} (exception: {1})", LocalFileName, ex.ToString());
								_skippedDuringSave = true;
								flag = true;
							}
							else
							{
								ZipErrorAction = zipErrorAction;
							}
							break;
						}
						if (num2 > 0)
						{
							throw;
						}
						if (ZipErrorAction == ZipErrorAction.InvokeErrorEvent)
						{
							OnZipErrorWhileSaving(ex);
							if (_ioOperationCanceled)
							{
								flag = true;
								break;
							}
						}
						num2++;
					}
				}
			}
			while (!flag);
		}

		internal void StoreRelativeOffset()
		{
			_RelativeOffsetOfLocalHeader = _future_ROLH;
		}

		internal void NotifySaveComplete()
		{
			_Encryption_FromZipFile = _Encryption;
			_CompressionMethod_FromZipFile = _CompressionMethod;
			_restreamRequiredOnSave = false;
			_metadataChanged = false;
			_Source = ZipEntrySource.ZipFile;
		}

		internal void WriteSecurityMetadata(Stream outstream)
		{
			if (Encryption == EncryptionAlgorithm.None)
			{
				return;
			}
			string password = _Password;
			if (_Source == ZipEntrySource.ZipFile && password == null)
			{
				password = _container.Password;
			}
			if (password == null)
			{
				_zipCrypto_forWrite = null;
				_aesCrypto_forWrite = null;
			}
			else if (Encryption == EncryptionAlgorithm.PkzipWeak)
			{
				_zipCrypto_forWrite = ZipCrypto.ForWrite(password);
				Random random = new Random();
				byte[] array = new byte[12];
				random.NextBytes(array);
				if ((_BitField & 8) == 8)
				{
					_TimeBlob = SharedUtilities.DateTimeToPacked(LastModified);
					array[11] = (byte)((_TimeBlob >> 8) & 0xFF);
				}
				else
				{
					FigureCrc32();
					array[11] = (byte)((_Crc32 >> 24) & 0xFF);
				}
				byte[] array2 = _zipCrypto_forWrite.EncryptMessage(array, array.Length);
				outstream.Write(array2, 0, array2.Length);
				_LengthOfHeader += array2.Length;
			}
			else if (Encryption == EncryptionAlgorithm.WinZipAes128 || Encryption == EncryptionAlgorithm.WinZipAes256)
			{
				int keyStrengthInBits = GetKeyStrengthInBits(Encryption);
				_aesCrypto_forWrite = WinZipAesCrypto.Generate(password, keyStrengthInBits);
				outstream.Write(_aesCrypto_forWrite.Salt, 0, _aesCrypto_forWrite._Salt.Length);
				outstream.Write(_aesCrypto_forWrite.GeneratedPV, 0, _aesCrypto_forWrite.GeneratedPV.Length);
				_LengthOfHeader += _aesCrypto_forWrite._Salt.Length + _aesCrypto_forWrite.GeneratedPV.Length;
			}
		}

		private void CopyThroughOneEntry(Stream outStream)
		{
			if (LengthOfHeader == 0)
			{
				throw new BadStateException("Bad header length.");
			}
			if (_metadataChanged || ArchiveStream is ZipSegmentedStream || outStream is ZipSegmentedStream || (_InputUsesZip64 && _container.UseZip64WhenSaving == Zip64Option.Default) || (!_InputUsesZip64 && _container.UseZip64WhenSaving == Zip64Option.Always))
			{
				CopyThroughWithRecompute(outStream);
			}
			else
			{
				CopyThroughWithNoChange(outStream);
			}
			_entryRequiresZip64 = _CompressedSize >= uint.MaxValue || _UncompressedSize >= uint.MaxValue || _RelativeOffsetOfLocalHeader >= uint.MaxValue;
			_OutputUsesZip64 = _container.Zip64 == Zip64Option.Always || _entryRequiresZip64.Value;
		}

		private void CopyThroughWithRecompute(Stream outstream)
		{
			byte[] array = new byte[BufferSize];
			CountingStream countingStream = new CountingStream(ArchiveStream);
			long relativeOffsetOfLocalHeader = _RelativeOffsetOfLocalHeader;
			int lengthOfHeader = LengthOfHeader;
			WriteHeader(outstream, 0);
			StoreRelativeOffset();
			if (!FileName.EndsWith("/"))
			{
				long num = relativeOffsetOfLocalHeader + lengthOfHeader;
				int lengthOfCryptoHeaderBytes = GetLengthOfCryptoHeaderBytes(_Encryption_FromZipFile);
				num -= lengthOfCryptoHeaderBytes;
				_LengthOfHeader += lengthOfCryptoHeaderBytes;
				countingStream.Seek(num, SeekOrigin.Begin);
				long num2 = _CompressedSize;
				while (num2 > 0)
				{
					lengthOfCryptoHeaderBytes = (int)((num2 > array.Length) ? array.Length : num2);
					int num3 = countingStream.Read(array, 0, lengthOfCryptoHeaderBytes);
					outstream.Write(array, 0, num3);
					num2 -= num3;
					OnWriteBlock(countingStream.BytesRead, _CompressedSize);
					if (_ioOperationCanceled)
					{
						break;
					}
				}
				if ((_BitField & 8) == 8)
				{
					int num4 = 16;
					if (_InputUsesZip64)
					{
						num4 += 8;
					}
					byte[] buffer = new byte[num4];
					countingStream.Read(buffer, 0, num4);
					if (_InputUsesZip64 && _container.UseZip64WhenSaving == Zip64Option.Default)
					{
						outstream.Write(buffer, 0, 8);
						if (_CompressedSize > uint.MaxValue)
						{
							throw new InvalidOperationException("ZIP64 is required");
						}
						outstream.Write(buffer, 8, 4);
						if (_UncompressedSize > uint.MaxValue)
						{
							throw new InvalidOperationException("ZIP64 is required");
						}
						outstream.Write(buffer, 16, 4);
						_LengthOfTrailer -= 8;
					}
					else if (!_InputUsesZip64 && _container.UseZip64WhenSaving == Zip64Option.Always)
					{
						byte[] buffer2 = new byte[4];
						outstream.Write(buffer, 0, 8);
						outstream.Write(buffer, 8, 4);
						outstream.Write(buffer2, 0, 4);
						outstream.Write(buffer, 12, 4);
						outstream.Write(buffer2, 0, 4);
						_LengthOfTrailer += 8;
					}
					else
					{
						outstream.Write(buffer, 0, num4);
					}
				}
			}
			_TotalEntrySize = _LengthOfHeader + _CompressedFileDataSize + _LengthOfTrailer;
		}

		private void CopyThroughWithNoChange(Stream outstream)
		{
			byte[] array = new byte[BufferSize];
			CountingStream countingStream = new CountingStream(ArchiveStream);
			countingStream.Seek(_RelativeOffsetOfLocalHeader, SeekOrigin.Begin);
			if (_TotalEntrySize == 0)
			{
				_TotalEntrySize = _LengthOfHeader + _CompressedFileDataSize + _LengthOfTrailer;
			}
			CountingStream countingStream2 = outstream as CountingStream;
			_RelativeOffsetOfLocalHeader = ((countingStream2 != null) ? countingStream2.ComputedPosition : outstream.Position);
			long num = _TotalEntrySize;
			while (num > 0)
			{
				int count = (int)((num > array.Length) ? array.Length : num);
				int num2 = countingStream.Read(array, 0, count);
				outstream.Write(array, 0, num2);
				num -= num2;
				OnWriteBlock(countingStream.BytesRead, _TotalEntrySize);
				if (_ioOperationCanceled)
				{
					break;
				}
			}
		}

		[Conditional("Trace")]
		private void TraceWriteLine(string format, params object[] varParams)
		{
			lock (_outputLock)
			{
				int hashCode = Thread.CurrentThread.GetHashCode();
				Console.Write("{0:000} ZipEntry.Write ", hashCode);
				Console.WriteLine(format, varParams);
			}
		}
	}
}
