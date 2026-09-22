using System;

namespace UnityEngine.Purchasing
{
	[Serializable]
	public class Price : ISerializationCallbackReceiver
	{
		public decimal value;

		[SerializeField]
		private int[] data;

		[SerializeField]
		private double num;

		public void OnBeforeSerialize()
		{
			data = decimal.GetBits(value);
			num = decimal.ToDouble(value);
		}

		public void OnAfterDeserialize()
		{
			if (data != null && data.Length == 4)
			{
				value = new decimal(data);
			}
		}
	}
}
