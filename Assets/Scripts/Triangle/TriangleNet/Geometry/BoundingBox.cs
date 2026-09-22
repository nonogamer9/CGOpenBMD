using System;

namespace TriangleNet.Geometry
{
	public class BoundingBox
	{
		private double xmin;

		private double ymin;

		private double xmax;

		private double ymax;

		public double MinX
		{
			get
			{
				return xmin;
			}
		}

		public double MaxX
		{
			get
			{
				return xmax;
			}
		}

		public double MinY
		{
			get
			{
				return ymin;
			}
		}

		public double MaxY
		{
			get
			{
				return ymax;
			}
		}

		public double Width
		{
			get
			{
				return xmax - xmin;
			}
		}

		public double Height
		{
			get
			{
				return ymax - ymin;
			}
		}

		public BoundingBox()
			: this(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue)
		{
		}

		public BoundingBox(BoundingBox other)
			: this(other.MinX, other.MinY, other.MaxX, other.MaxY)
		{
		}

		public BoundingBox(double xmin, double ymin, double xmax, double ymax)
		{
			this.xmin = xmin;
			this.xmax = xmax;
			this.ymin = ymin;
			this.ymax = ymax;
		}

		public void Resize(double dx, double dy)
		{
			xmin -= dx;
			xmax += dx;
			ymin -= dy;
			ymax += dy;
		}

		public void Expand(double x, double y)
		{
			xmin = Math.Min(xmin, x);
			ymin = Math.Min(ymin, y);
			xmax = Math.Max(xmax, x);
			ymax = Math.Max(ymax, y);
		}

		public void Expand(BoundingBox other)
		{
			xmin = Math.Min(xmin, other.xmin);
			ymin = Math.Min(ymin, other.ymin);
			xmax = Math.Max(xmax, other.xmax);
			ymax = Math.Max(ymax, other.ymax);
		}

		public bool Contains(Point pt)
		{
			if (pt.x >= xmin && pt.x <= xmax && pt.y >= ymin)
			{
				return pt.y <= ymax;
			}
			return false;
		}

		public bool Contains(BoundingBox other)
		{
			if (xmin <= other.MinX && other.MaxX <= xmax && ymin <= other.MinY)
			{
				return other.MaxY <= ymax;
			}
			return false;
		}

		public bool Intersects(BoundingBox other)
		{
			if (other.MinX < xmax && xmin < other.MaxX && other.MinY < ymax)
			{
				return ymin < other.MaxY;
			}
			return false;
		}
	}
}
