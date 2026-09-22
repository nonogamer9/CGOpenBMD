using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine.Purchasing
{
	internal static class QueryHelper
	{
		internal static string ToQueryString(this Dictionary<string, object> parameters)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string key in parameters.Keys)
			{
				string text = parameters[key].ToString();
				if (text != null)
				{
					stringBuilder.Append((stringBuilder.Length == 0) ? "?" : "&");
					stringBuilder.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(text));
				}
			}
			return stringBuilder.ToString();
		}
	}
}
