using System.ComponentModel;

namespace Ionic
{
	internal enum ComparisonOperator
	{
		[Description(">")]
		GreaterThan = 0,
		[Description(">=")]
		GreaterThanOrEqualTo = 1,
		[Description("<")]
		LesserThan = 2,
		[Description("<=")]
		LesserThanOrEqualTo = 3,
		[Description("=")]
		EqualTo = 4,
		[Description("!=")]
		NotEqualTo = 5
	}
}
