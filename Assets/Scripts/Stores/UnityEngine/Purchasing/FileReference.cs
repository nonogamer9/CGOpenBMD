using System;
using System.IO;
using Uniject;

namespace UnityEngine.Purchasing
{
	internal class FileReference
	{
		private string m_FilePath;

		private ILogger m_Logger;

		internal static FileReference CreateInstance(string filename, ILogger logger, IUtil util)
		{
			try
			{
				string path = Path.Combine(util.persistentDataPath, "Unity");
				string path2 = Path.Combine(util.cloudProjectId, "IAP");
				string text = Path.Combine(path, path2);
				Directory.CreateDirectory(text);
				string filePath = Path.Combine(text, filename);
				return new FileReference(filePath, logger);
			}
			catch
			{
				return null;
			}
		}

		internal FileReference(string filePath, ILogger logger)
		{
			m_FilePath = filePath;
			m_Logger = logger;
		}

		internal void Save(string payload)
		{
			try
			{
				File.WriteAllText(m_FilePath, payload);
			}
			catch (Exception message)
			{
				m_Logger.LogError("Failed persisting content", message);
			}
		}

		internal string Load()
		{
			try
			{
				return File.ReadAllText(m_FilePath);
			}
			catch
			{
				return null;
			}
		}

		internal void Delete()
		{
			try
			{
				File.Delete(m_FilePath);
			}
			catch (Exception message)
			{
				m_Logger.Log("Failed deleting cached content", message);
			}
		}
	}
}
