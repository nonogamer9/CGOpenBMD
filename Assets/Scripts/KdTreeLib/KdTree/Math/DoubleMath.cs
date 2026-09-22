using System;

namespace KdTree.Math
{
	[Serializable]
	public class DoubleMath : TypeMath<double>
	{
		public override double MinValue
		{
			get
			{
				return double.MinValue;
			}
		}

		public override double MaxValue
		{
			get
			{
				return double.MaxValue;
			}
		}

		public override double Zero
		{
			get
			{
				return 0.0;
			}
		}

		public override double NegativeInfinity
		{
			get
			{
				return double.NegativeInfinity;
			}
		}

		public override double PositiveInfinity
		{
			get
			{
				return double.PositiveInfinity;
			}
		}

		public override int Compare(double a, double b)
		{
			return a.CompareTo(b);
		}

		public override bool AreEqual(double a, double b)
		{
			return a == b;
		}

		public override double Add(double a, double b)
		{
			return a + b;
		}

		public override double Subtract(double a, double b)
		{
			return a - b;
		}

		public override double Multiply(double a, double b)
		{
			return a * b;
		}

		public override double DistanceSquaredBetweenPoints(double[] a, double[] b)
		{
			double num = Zero;
			int num2 = a.Length;
			for (int i = 0; i < num2; i++)
			{
				double num3 = Subtract(a[i], b[i]);
				double b2 = Multiply(num3, num3);
				num = Add(num, b2);
			}
			return num;
		}
	}
}
