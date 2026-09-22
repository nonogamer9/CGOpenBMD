using System;

namespace TriangleNet.Geometry
{
	public class Point : IComparable<Point>, IEquatable<Point>
	{
		internal int id;

		internal double x;

		internal double y;

		internal int mark;

		internal double[] attributes;

		public int ID
		{
			get
			{
				return id;
			}
		}

		public double X
		{
			get
			{
				return x;
			}
		}

		public double Y
		{
			get
			{
				return y;
			}
		}

		public int Boundary
		{
			get
			{
				return mark;
			}
		}

		public double[] Attributes
		{
			get
			{
				return attributes;
			}
		}

		public Point()
			: this(0.0, 0.0, 0)
		{
		}

		public Point(double x, double y)
			: this(x, y, 0)
		{
		}

		public Point(double x, double y, int mark)
		{
			this.x = x;
			this.y = y;
			this.mark = mark;
		}

		public static bool operator ==(Point a, Point b)
		{
			if ((object)a == b)
			{
				return true;
			}
			if ((object)a == null || (object)b == null)
			{
				return false;
			}
			return a.Equals(b);
		}

		public static bool operator !=(Point a, Point b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Point point = obj as Point;
			if ((object)point == null)
			{
				return false;
			}
			if (x == point.x)
			{
				return y == point.y;
			}
			return false;
		}

		public bool Equals(Point p)
		{
			if ((object)p == null)
			{
				return false;
			}
			if (x == p.x)
			{
				return y == p.y;
			}
			return false;
		}

		public int CompareTo(Point other)
		{
			if (x == other.x && y == other.y)
			{
				return 0;
			}
			if (!(x < other.x) && (x != other.x || !(y < other.y)))
			{
				return 1;
			}
			return -1;
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("[{0},{1}]", x, y);
		}
	}
}
