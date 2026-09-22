using System;

namespace UnityEngine.Purchasing
{
	[Serializable]
	public class ProductCatalogPayout
	{
		public enum ProductCatalogPayoutType
		{
			Other = 0,
			Currency = 1,
			Item = 2,
			Resource = 3
		}

		[SerializeField]
		private string t = ProductCatalogPayoutType.Other.ToString();

		public const int MaxSubtypeLength = 64;

		[SerializeField]
		private string st = string.Empty;

		[SerializeField]
		private double q;

		public const int MaxDataLength = 1024;

		[SerializeField]
		private string d = string.Empty;

		public ProductCatalogPayoutType type
		{
			get
			{
				ProductCatalogPayoutType result = ProductCatalogPayoutType.Other;
				if (Enum.IsDefined(typeof(ProductCatalogPayoutType), t))
				{
					result = (ProductCatalogPayoutType)Enum.Parse(typeof(ProductCatalogPayoutType), t);
				}
				return result;
			}
			set
			{
				t = value.ToString();
			}
		}

		public string typeString
		{
			get
			{
				return t;
			}
		}

		public string subtype
		{
			get
			{
				return st;
			}
			set
			{
				if (value.Length > 64)
				{
					throw new ArgumentException(string.Format("subtype should be no longer than {0} characters", 64));
				}
				st = value;
			}
		}

		public double quantity
		{
			get
			{
				return q;
			}
			set
			{
				q = value;
			}
		}

		public string data
		{
			get
			{
				return d;
			}
			set
			{
				if (value.Length > 1024)
				{
					throw new ArgumentException(string.Format("data should be no longer than {0} characters", 1024));
				}
				d = value;
			}
		}
	}
}
