using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
	internal class JSONSerializer
	{
		public static string SerializeProductDef(ProductDefinition product)
		{
			return MiniJson.JsonEncode(EncodeProductDef(product));
		}

		public static string SerializeProductDefs(IEnumerable<ProductDefinition> products)
		{
			List<object> list = new List<object>();
			foreach (ProductDefinition product in products)
			{
				list.Add(EncodeProductDef(product));
			}
			return MiniJson.JsonEncode(list);
		}

		public static string SerializeProductDescs(ProductDescription product)
		{
			return MiniJson.JsonEncode(EncodeProductDesc(product));
		}

		public static string SerializeProductDescs(IEnumerable<ProductDescription> products)
		{
			List<object> list = new List<object>();
			foreach (ProductDescription product in products)
			{
				list.Add(EncodeProductDesc(product));
			}
			return MiniJson.JsonEncode(list);
		}

		public static List<ProductDescription> DeserializeProductDescriptions(string json)
		{
			List<ProductDescription> list = new List<ProductDescription>();
			try
			{
				Type typeFromHandle = typeof(ProductDescription);
				ConstructorInfo constructor = typeFromHandle.GetConstructor(new Type[5]
				{
					typeof(string),
					typeof(ProductMetadata),
					typeof(string),
					typeof(string),
					typeof(ProductType)
				});
				List<object> list2 = (List<object>)MiniJson.JsonDecode(json);
				foreach (Dictionary<string, object> item2 in list2)
				{
					ProductMetadata productMetadata = DeserializeMetadata((Dictionary<string, object>)item2["metadata"]);
					if (constructor == null)
					{
						ProductDescription item = new ProductDescription((string)item2["storeSpecificId"], productMetadata, item2.TryGetString("receipt"), item2.TryGetString("transactionId"));
						list.Add(item);
						continue;
					}
					object[] parameters = new object[5]
					{
						(string)item2["storeSpecificId"],
						productMetadata,
						item2.TryGetString("receipt"),
						item2.TryGetString("transactionId"),
						ProductType.NonConsumable
					};
					object obj = constructor.Invoke(parameters);
					list.Add((ProductDescription)obj);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return list;
		}

		public static Dictionary<string, string> DeserializeSubscriptionDescriptions(string json)
		{
			List<object> list = (List<object>)MiniJson.JsonDecode(json);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (Dictionary<string, object> item in list)
			{
				Dictionary<string, object> dic = (Dictionary<string, object>)item["metadata"];
				string key = (string)item["storeSpecificId"];
				Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
				dictionary3["introductoryPrice"] = dic.TryGetString("introductoryPrice");
				dictionary3["introductoryPriceLocale"] = dic.TryGetString("introductoryPriceLocale");
				dictionary3["introductoryPriceNumberOfPeriods"] = dic.TryGetString("introductoryPriceNumberOfPeriods");
				dictionary3["numberOfUnits"] = dic.TryGetString("numberOfUnits");
				dictionary3["unit"] = dic.TryGetString("unit");
				dictionary.Add(key, MiniJson.JsonEncode(dictionary3));
			}
			return dictionary;
		}

		public static PurchaseFailureDescription DeserializeFailureReason(string json)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(json);
			PurchaseFailureReason reason = PurchaseFailureReason.Unknown;
			if (Enum.IsDefined(typeof(PurchaseFailureReason), (string)dictionary["reason"]))
			{
				reason = (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), (string)dictionary["reason"]);
			}
			return new PurchaseFailureDescription((string)dictionary["productId"], reason, dictionary.TryGetString("message"));
		}

		private static ProductMetadata DeserializeMetadata(Dictionary<string, object> data)
		{
			decimal num = 0.0m;
			try
			{
				num = Convert.ToDecimal(data["localizedPrice"]);
			}
			catch
			{
				num = 0.0m;
			}
			return new ProductMetadata(data.TryGetString("localizedPriceString"), data.TryGetString("localizedTitle"), data.TryGetString("localizedDescription"), data.TryGetString("isoCurrencyCode"), num);
		}

		private static Dictionary<string, object> EncodeProductDef(ProductDefinition product)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("id", product.id);
			dictionary.Add("storeSpecificId", product.storeSpecificId);
			dictionary.Add("type", product.type.ToString());
			bool flag = true;
			PropertyInfo property = typeof(ProductDefinition).GetProperty("enabled");
			if (property != null)
			{
				try
				{
					flag = Convert.ToBoolean(property.GetValue(product, null));
				}
				catch
				{
					flag = true;
				}
			}
			dictionary.Add("enabled", flag);
			List<object> list = new List<object>();
			PropertyInfo property2 = typeof(ProductDefinition).GetProperty("payouts");
			if (property2 != null)
			{
				object value = property2.GetValue(product, null);
				Array array = value as Array;
				if (array != null)
				{
					foreach (object item in array)
					{
						Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
						Type type = item.GetType();
						dictionary2["t"] = type.GetField("typeString").GetValue(item);
						dictionary2["st"] = type.GetField("subtype").GetValue(item);
						dictionary2["q"] = type.GetField("quantity").GetValue(item);
						dictionary2["d"] = type.GetField("data").GetValue(item);
						list.Add(dictionary2);
					}
				}
			}
			dictionary.Add("payouts", list);
			return dictionary;
		}

		private static Dictionary<string, object> EncodeProductDesc(ProductDescription product)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("storeSpecificId", product.storeSpecificId);
			Type typeFromHandle = typeof(ProductDescription);
			FieldInfo field = typeFromHandle.GetField("type");
			if (field != null)
			{
				object value = field.GetValue(product);
				dictionary.Add("type", value.ToString());
			}
			dictionary.Add("metadata", EncodeProductMeta(product.metadata));
			dictionary.Add("receipt", product.receipt);
			dictionary.Add("transactionId", product.transactionId);
			return dictionary;
		}

		private static Dictionary<string, object> EncodeProductMeta(ProductMetadata product)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("localizedPriceString", product.localizedPriceString);
			dictionary.Add("localizedTitle", product.localizedTitle);
			dictionary.Add("localizedDescription", product.localizedDescription);
			dictionary.Add("isoCurrencyCode", product.isoCurrencyCode);
			dictionary.Add("localizedPrice", Convert.ToDouble(product.localizedPrice));
			return dictionary;
		}
	}
}
