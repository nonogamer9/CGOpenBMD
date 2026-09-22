using System;
using System.IO;
using System.Text;

namespace UnityEngine.Purchasing
{
	internal class TransactionLog
	{
		private readonly ILogger logger;

		private readonly string persistentDataPath;

		public TransactionLog(ILogger logger, string persistentDataPath)
		{
			this.logger = logger;
			if (!string.IsNullOrEmpty(persistentDataPath))
			{
				this.persistentDataPath = Path.Combine(Path.Combine(persistentDataPath, "Unity"), "UnityPurchasing");
			}
		}

		public void Clear()
		{
			Directory.Delete(persistentDataPath, true);
		}

		public bool HasRecordOf(string transactionID)
		{
			if (string.IsNullOrEmpty(transactionID) || string.IsNullOrEmpty(persistentDataPath))
			{
				return false;
			}
			return Directory.Exists(GetRecordPath(transactionID));
		}

		public void Record(string transactionID)
		{
			if (!string.IsNullOrEmpty(transactionID) && !string.IsNullOrEmpty(persistentDataPath))
			{
				string recordPath = GetRecordPath(transactionID);
				try
				{
					Directory.CreateDirectory(recordPath);
				}
				catch (Exception ex)
				{
					logger.Log(ex.Message);
				}
			}
		}

		private string GetRecordPath(string transactionID)
		{
			return Path.Combine(persistentDataPath, ComputeHash(transactionID));
		}

		internal static string ComputeHash(string transactionID)
		{
			ulong num = 3074457345618258791uL;
			for (int i = 0; i < transactionID.Length; i++)
			{
				num += transactionID[i];
				num *= 3074457345618258799L;
			}
			StringBuilder stringBuilder = new StringBuilder(16);
			byte[] bytes = BitConverter.GetBytes(num);
			foreach (byte b in bytes)
			{
				stringBuilder.AppendFormat("{0:X2}", b);
			}
			return stringBuilder.ToString();
		}
	}
}
