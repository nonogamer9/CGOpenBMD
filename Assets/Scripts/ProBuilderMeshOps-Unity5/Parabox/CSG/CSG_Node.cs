using System.Collections.Generic;

namespace Parabox.CSG
{
	internal class CSG_Node
	{
		public List<CSG_Polygon> polygons;

		public CSG_Node front;

		public CSG_Node back;

		public CSG_Plane plane;

		public CSG_Node()
		{
			front = null;
			back = null;
		}

		public CSG_Node(List<CSG_Polygon> list)
		{
			Build(list);
		}

		public CSG_Node(List<CSG_Polygon> list, CSG_Plane plane, CSG_Node front, CSG_Node back)
		{
			polygons = list;
			this.plane = plane;
			this.front = front;
			this.back = back;
		}

		public CSG_Node Clone()
		{
			return new CSG_Node(polygons, plane, front, back);
		}

		public void ClipTo(CSG_Node other)
		{
			polygons = other.ClipPolygons(polygons);
			if (front != null)
			{
				front.ClipTo(other);
			}
			if (back != null)
			{
				back.ClipTo(other);
			}
		}

		public void Invert()
		{
			for (int i = 0; i < polygons.Count; i++)
			{
				polygons[i].Flip();
			}
			plane.Flip();
			if (front != null)
			{
				front.Invert();
			}
			if (back != null)
			{
				back.Invert();
			}
			CSG_Node cSG_Node = front;
			front = back;
			back = cSG_Node;
		}

		public void Build(List<CSG_Polygon> list)
		{
			if (list.Count < 1)
			{
				return;
			}
			if (plane == null || !plane.Valid())
			{
				plane = new CSG_Plane();
				plane.normal = list[0].plane.normal;
				plane.w = list[0].plane.w;
			}
			if (polygons == null)
			{
				polygons = new List<CSG_Polygon>();
			}
			List<CSG_Polygon> list2 = new List<CSG_Polygon>();
			List<CSG_Polygon> list3 = new List<CSG_Polygon>();
			for (int i = 0; i < list.Count; i++)
			{
				plane.SplitPolygon(list[i], polygons, polygons, list2, list3);
			}
			if (list2.Count > 0)
			{
				if (front == null)
				{
					front = new CSG_Node();
				}
				front.Build(list2);
			}
			if (list3.Count > 0)
			{
				if (back == null)
				{
					back = new CSG_Node();
				}
				back.Build(list3);
			}
		}

		public List<CSG_Polygon> ClipPolygons(List<CSG_Polygon> list)
		{
			if (!plane.Valid())
			{
				return list;
			}
			List<CSG_Polygon> list2 = new List<CSG_Polygon>();
			List<CSG_Polygon> list3 = new List<CSG_Polygon>();
			for (int i = 0; i < list.Count; i++)
			{
				plane.SplitPolygon(list[i], list2, list3, list2, list3);
			}
			if (front != null)
			{
				list2 = front.ClipPolygons(list2);
			}
			if (back != null)
			{
				list3 = back.ClipPolygons(list3);
			}
			else
			{
				list3.Clear();
			}
			list2.AddRange(list3);
			return list2;
		}

		public List<CSG_Polygon> AllPolygons()
		{
			List<CSG_Polygon> list = polygons;
			List<CSG_Polygon> collection = new List<CSG_Polygon>();
			List<CSG_Polygon> collection2 = new List<CSG_Polygon>();
			if (front != null)
			{
				collection = front.AllPolygons();
			}
			if (back != null)
			{
				collection2 = back.AllPolygons();
			}
			list.AddRange(collection);
			list.AddRange(collection2);
			return list;
		}

		public static CSG_Node Union(CSG_Node a1, CSG_Node b1)
		{
			CSG_Node cSG_Node = a1.Clone();
			CSG_Node cSG_Node2 = b1.Clone();
			cSG_Node.ClipTo(cSG_Node2);
			cSG_Node2.ClipTo(cSG_Node);
			cSG_Node2.Invert();
			cSG_Node2.ClipTo(cSG_Node);
			cSG_Node2.Invert();
			cSG_Node.Build(cSG_Node2.AllPolygons());
			return new CSG_Node(cSG_Node.AllPolygons());
		}

		public static CSG_Node Subtract(CSG_Node a1, CSG_Node b1)
		{
			CSG_Node cSG_Node = a1.Clone();
			CSG_Node cSG_Node2 = b1.Clone();
			cSG_Node.Invert();
			cSG_Node.ClipTo(cSG_Node2);
			cSG_Node2.ClipTo(cSG_Node);
			cSG_Node2.Invert();
			cSG_Node2.ClipTo(cSG_Node);
			cSG_Node2.Invert();
			cSG_Node.Build(cSG_Node2.AllPolygons());
			cSG_Node.Invert();
			return new CSG_Node(cSG_Node.AllPolygons());
		}

		public static CSG_Node Intersect(CSG_Node a1, CSG_Node b1)
		{
			CSG_Node cSG_Node = a1.Clone();
			CSG_Node cSG_Node2 = b1.Clone();
			cSG_Node.Invert();
			cSG_Node2.ClipTo(cSG_Node);
			cSG_Node2.Invert();
			cSG_Node.ClipTo(cSG_Node2);
			cSG_Node2.ClipTo(cSG_Node);
			cSG_Node.Build(cSG_Node2.AllPolygons());
			cSG_Node.Invert();
			return new CSG_Node(cSG_Node.AllPolygons());
		}
	}
}
