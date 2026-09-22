using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	internal static class SerializationExtensions
	{
		public static string TryGetString(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key) && dic[key] != null)
			{
				return dic[key].ToString();
			}
			return null;
		}
	}
}
