using System;
using System.Collections.Generic;
using UnityEngine;

namespace Parabox.CSG
{
	internal class CSG_Plane
	{
		[Flags]
		private enum EPolygonType
		{
			Coplanar = 0,
			Front = 1,
			Back = 2,
			Spanning = Front | Back
		}

		public Vector3 normal;

		public float w;

		public CSG_Plane()
		{
			normal = Vector3.zero;
			w = 0f;
		}

		public CSG_Plane(Vector3 a, Vector3 b, Vector3 c)
		{
			normal = Vector3.Cross(b - a, c - a);
			w = Vector3.Dot(normal, a);
		}

		public bool Valid()
		{
			return normal.magnitude > 0f;
		}

		public void Flip()
		{
			normal *= -1f;
			w *= -1f;
		}

		public void SplitPolygon(CSG_Polygon polygon, List<CSG_Polygon> coplanarFront, List<CSG_Polygon> coplanarBack, List<CSG_Polygon> front, List<CSG_Polygon> back)
		{
			EPolygonType ePolygonType = EPolygonType.Coplanar;
			List<EPolygonType> list = new List<EPolygonType>();
			for (int i = 0; i < polygon.vertices.Count; i++)
			{
				float num = Vector3.Dot(normal, polygon.vertices[i].position) - w;
				EPolygonType ePolygonType2 = ((!(num < -1E-05f)) ? ((num > 1E-05f) ? EPolygonType.Front : EPolygonType.Coplanar) : EPolygonType.Back);
				ePolygonType |= ePolygonType2;
				list.Add(ePolygonType2);
			}
			switch (ePolygonType)
			{
			case EPolygonType.Coplanar:
				if (Vector3.Dot(normal, polygon.plane.normal) > 0f)
				{
					coplanarFront.Add(polygon);
				}
				else
				{
					coplanarBack.Add(polygon);
				}
				break;
			case EPolygonType.Front:
				front.Add(polygon);
				break;
			case EPolygonType.Back:
				back.Add(polygon);
				break;
			case EPolygonType.Spanning:
			{
				List<CSG_Vertex> list2 = new List<CSG_Vertex>();
				List<CSG_Vertex> list3 = new List<CSG_Vertex>();
				for (int j = 0; j < polygon.vertices.Count; j++)
				{
					int index = (j + 1) % polygon.vertices.Count;
					EPolygonType ePolygonType3 = list[j];
					EPolygonType ePolygonType4 = list[index];
					CSG_Vertex cSG_Vertex = polygon.vertices[j];
					CSG_Vertex b = polygon.vertices[index];
					if (ePolygonType3 != EPolygonType.Back)
					{
						list2.Add(cSG_Vertex);
					}
					if (ePolygonType3 != EPolygonType.Front)
					{
						list3.Add(cSG_Vertex);
					}
					if ((ePolygonType3 | ePolygonType4) == EPolygonType.Spanning)
					{
						float t = (w - Vector3.Dot(normal, cSG_Vertex.position)) / Vector3.Dot(normal, b.position - cSG_Vertex.position);
						CSG_Vertex item = CSG_Vertex.Interpolate(cSG_Vertex, b, t);
						list2.Add(item);
						list3.Add(item);
					}
				}
				if (list2.Count >= 3)
				{
					front.Add(new CSG_Polygon(list2));
				}
				if (list3.Count >= 3)
				{
					back.Add(new CSG_Polygon(list3));
				}
				break;
			}
			}
		}
	}
}
