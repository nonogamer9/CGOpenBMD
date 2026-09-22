using System;

namespace KdTree
{
	public class DuplicateNodeError : Exception
	{
		public DuplicateNodeError()
			: base("Cannot Add Node With Duplicate Coordinates")
		{
		}
	}
}
