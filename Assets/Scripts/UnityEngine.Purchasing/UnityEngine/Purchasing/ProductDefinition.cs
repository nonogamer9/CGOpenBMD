using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
	public class ProductDefinition
	{
		private List<PayoutDefinition> m_Payouts = new List<PayoutDefinition>();

		public string id { get; private set; }

		public string storeSpecificId { get; private set; }

		public ProductType type { get; private set; }

		public bool enabled { get; private set; }

		public IEnumerable<PayoutDefinition> payouts
		{
			get
			{
				return m_Payouts;
			}
		}

		public PayoutDefinition payout
		{
			get
			{
				return (m_Payouts.Count <= 0) ? null : m_Payouts[0];
			}
		}

		private ProductDefinition()
		{
		}

		public ProductDefinition(string id, string storeSpecificId, ProductType type)
			: this(id, storeSpecificId, type, true)
		{
		}

		public ProductDefinition(string id, string storeSpecificId, ProductType type, bool enabled)
			: this(id, storeSpecificId, type, enabled, (IEnumerable<PayoutDefinition>)null)
		{
		}

		public ProductDefinition(string id, string storeSpecificId, ProductType type, bool enabled, PayoutDefinition payout)
			: this(id, storeSpecificId, type, enabled, new List<PayoutDefinition> { payout })
		{
		}

		public ProductDefinition(string id, string storeSpecificId, ProductType type, bool enabled, IEnumerable<PayoutDefinition> payouts)
		{
			this.id = id;
			this.storeSpecificId = storeSpecificId;
			this.type = type;
			this.enabled = enabled;
			SetPayouts(payouts);
		}

		public ProductDefinition(string id, ProductType type)
			: this(id, id, type)
		{
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			ProductDefinition productDefinition = obj as ProductDefinition;
			if (productDefinition == null)
			{
				return false;
			}
			return id == productDefinition.id;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		internal void SetPayouts(IEnumerable<PayoutDefinition> newPayouts)
		{
			if (newPayouts != null)
			{
				m_Payouts.Clear();
				m_Payouts.AddRange(newPayouts);
			}
		}
	}
}
