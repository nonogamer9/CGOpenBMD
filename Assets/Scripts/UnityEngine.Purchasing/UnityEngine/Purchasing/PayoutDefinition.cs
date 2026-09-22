using System;

namespace UnityEngine.Purchasing
{
	[Serializable]
	public class PayoutDefinition
	{
		[SerializeField]
		private PayoutType m_Type;

		[SerializeField]
		private string m_Subtype = string.Empty;

		[SerializeField]
		private double m_Quantity;

		[SerializeField]
		private string m_Data = string.Empty;

		public const int MaxSubtypeLength = 64;

		public const int MaxDataLength = 1024;

		public PayoutType type
		{
			get
			{
				return m_Type;
			}
			private set
			{
				m_Type = value;
			}
		}

		public string typeString
		{
			get
			{
				return m_Type.ToString();
			}
		}

		public string subtype
		{
			get
			{
				return m_Subtype;
			}
			private set
			{
				if (value.Length > 64)
				{
					throw new ArgumentException(string.Format("subtype connot be longer than {0}", 64));
				}
				m_Subtype = value;
			}
		}

		public double quantity
		{
			get
			{
				return m_Quantity;
			}
			private set
			{
				m_Quantity = value;
			}
		}

		public string data
		{
			get
			{
				return m_Data;
			}
			private set
			{
				if (value.Length > 1024)
				{
					throw new ArgumentException(string.Format("data cannot be longer than {0}", 1024));
				}
				m_Data = value;
			}
		}

		public PayoutDefinition()
		{
		}

		public PayoutDefinition(string typeString, string subtype, double quantity)
			: this(typeString, subtype, quantity, string.Empty)
		{
		}

		public PayoutDefinition(string typeString, string subtype, double quantity, string data)
		{
			PayoutType payoutType = PayoutType.Other;
			if (Enum.IsDefined(typeof(PayoutType), typeString))
			{
				payoutType = (PayoutType)Enum.Parse(typeof(PayoutType), typeString);
			}
			type = payoutType;
			this.subtype = subtype;
			this.quantity = quantity;
			this.data = data;
		}

		public PayoutDefinition(string subtype, double quantity)
			: this(PayoutType.Other, subtype, quantity, string.Empty)
		{
		}

		public PayoutDefinition(string subtype, double quantity, string data)
			: this(PayoutType.Other, subtype, quantity, data)
		{
		}

		public PayoutDefinition(PayoutType type, string subtype, double quantity)
			: this(type, subtype, quantity, string.Empty)
		{
		}

		public PayoutDefinition(PayoutType type, string subtype, double quantity, string data)
		{
			this.type = type;
			this.subtype = subtype;
			this.quantity = quantity;
			this.data = data;
		}
	}
}
