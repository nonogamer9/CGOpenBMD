using System.Collections.Generic;

namespace Parabox.CSG
{
	internal class CSG_Polygon
	{
		public List<CSG_Vertex> vertices;

		public CSG_Plane plane;

		public CSG_Polygon(List<CSG_Vertex> list)
		{
			vertices = list;
			plane = new CSG_Plane(list[0].position, list[1].position, list[2].position);
		}

		public void Flip()
		{
			vertices.Reverse();
			for (int i = 0; i < vertices.Count; i++)
			{
				vertices[i].Flip();
			}
			plane.Flip();
		}

		public override string ToString()
		{
			return "N: " + plane.normal;
		}
	}
}
