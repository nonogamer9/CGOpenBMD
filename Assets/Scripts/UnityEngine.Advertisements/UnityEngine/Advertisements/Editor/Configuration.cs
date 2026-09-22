using System.Collections.Generic;
using UnityEngine.Advertisements.SimpleJson;

namespace UnityEngine.Advertisements.Editor
{
	internal sealed class Configuration
	{
		public bool enabled { get; private set; }

		public string defaultPlacement { get; private set; }

		public Dictionary<string, bool> placements { get; private set; }

		public Configuration(string configurationResponse)
		{
			IDictionary<string, object> dictionary = (IDictionary<string, object>)UnityEngine.Advertisements.SimpleJson.SimpleJson.DeserializeObject(configurationResponse);
			enabled = (bool)dictionary["enabled"];
			placements = new Dictionary<string, bool>();
			foreach (IDictionary<string, object> item in (IList<object>)dictionary["placements"])
			{
				string key = (string)item["id"];
				bool value = (bool)item["allowSkip"];
				if ((bool)item["default"])
				{
					defaultPlacement = key;
				}
				placements.Add(key, value);
			}
		}
	}
}
