using System;
using System.IO;
using System.Text;
using Ionic.Zip;

namespace Ionic
{
	internal class SizeCriterion : SelectionCriterion
	{
		internal ComparisonOperator Operator;

		internal long Size;

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("size ").Append(EnumUtil.GetDescription(Operator)).Append(" ")
				.Append(Size.ToString());
			return stringBuilder.ToString();
		}

		internal override bool Evaluate(string filename)
		{
			FileInfo fileInfo = new FileInfo(filename);
			return _Evaluate(fileInfo.Length);
		}

		private bool _Evaluate(long Length)
		{
			bool flag = false;
			switch (Operator)
			{
			case ComparisonOperator.GreaterThanOrEqualTo:
				return Length >= Size;
			case ComparisonOperator.GreaterThan:
				return Length > Size;
			case ComparisonOperator.LesserThanOrEqualTo:
				return Length <= Size;
			case ComparisonOperator.LesserThan:
				return Length < Size;
			case ComparisonOperator.EqualTo:
				return Length == Size;
			case ComparisonOperator.NotEqualTo:
				return Length != Size;
			default:
				throw new ArgumentException("Operator");
			}
		}

		internal override bool Evaluate(ZipEntry entry)
		{
			return _Evaluate(entry.UncompressedSize);
		}
	}
}
