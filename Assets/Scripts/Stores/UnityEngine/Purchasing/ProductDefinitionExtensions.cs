using System;
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	internal static class ProductDefinitionExtensions
	{
		internal static List<ProductDefinition> DecodeJSON(this List<object> productsList, string storeName)
		{
			List<ProductDefinition> list = new List<ProductDefinition>();
			try
			{
				foreach (object products in productsList)
				{
					Dictionary<string, object> dictionary = (Dictionary<string, object>)products;
					object value;
					dictionary.TryGetValue("id", out value);
					object value2;
					dictionary.TryGetValue("store_ids", out value2);
					object value3;
					dictionary.TryGetValue("type", out value3);
					Dictionary<string, object> dictionary2 = value2 as Dictionary<string, object>;
					string storeSpecificId = (string)value;
					if (dictionary2 != null)
					{
						foreach (KeyValuePair<string, object> item2 in dictionary2)
						{
							string text = item2.Key.ToLower();
							string text2 = (string)item2.Value;
							if (!string.IsNullOrEmpty(text2) && storeName.ToLower() == text)
							{
								storeSpecificId = text2;
							}
						}
					}
					else
					{
						object value4;
						dictionary.TryGetValue("storeSpecificId", out value4);
						string text3 = (string)value4;
						if (text3 != null)
						{
							storeSpecificId = text3;
						}
					}
					ProductType type = (ProductType)Enum.Parse(typeof(ProductType), (string)value3);
					ProductDefinition item = new ProductDefinition((string)value, storeSpecificId, type);
					list.Add(item);
				}
				return list;
			}
			catch
			{
				return null;
			}
		}
	}
}
