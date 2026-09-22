using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	public class IDs : IEnumerable<KeyValuePair<string, string>>, IEnumerable
	{
		private Dictionary<string, string> m_Dic = new Dictionary<string, string>();

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_Dic.GetEnumerator();
		}

		public void Add(string id, params string[] stores)
		{
			foreach (string key in stores)
			{
				m_Dic[key] = id;
			}
		}

		public void Add(string id, params object[] stores)
		{
			foreach (object obj in stores)
			{
				m_Dic[obj.ToString()] = id;
			}
		}

		internal string SpecificIDForStore(string store, string defaultValue)
		{
			if (m_Dic.ContainsKey(store))
			{
				return m_Dic[store];
			}
			return defaultValue;
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return m_Dic.GetEnumerator();
		}
	}
}
