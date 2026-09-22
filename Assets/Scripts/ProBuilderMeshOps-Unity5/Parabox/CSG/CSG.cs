using System.Collections.Generic;
using UnityEngine;

namespace Parabox.CSG
{
	public class CSG
	{
		public const float EPSILON = 1E-05f;

		public static Mesh Union(GameObject lhs, GameObject rhs)
		{
			CSG_Model cSG_Model = new CSG_Model(lhs);
			CSG_Model cSG_Model2 = new CSG_Model(rhs);
			CSG_Node a = new CSG_Node(cSG_Model.ToPolygons());
			CSG_Node b = new CSG_Node(cSG_Model2.ToPolygons());
			List<CSG_Polygon> list = CSG_Node.Union(a, b).AllPolygons();
			CSG_Model cSG_Model3 = new CSG_Model(list);
			return cSG_Model3.ToMesh();
		}

		public static Mesh Subtract(GameObject lhs, GameObject rhs)
		{
			CSG_Model cSG_Model = new CSG_Model(lhs);
			CSG_Model cSG_Model2 = new CSG_Model(rhs);
			CSG_Node a = new CSG_Node(cSG_Model.ToPolygons());
			CSG_Node b = new CSG_Node(cSG_Model2.ToPolygons());
			List<CSG_Polygon> list = CSG_Node.Subtract(a, b).AllPolygons();
			CSG_Model cSG_Model3 = new CSG_Model(list);
			return cSG_Model3.ToMesh();
		}

		public static Mesh Intersect(GameObject lhs, GameObject rhs)
		{
			CSG_Model cSG_Model = new CSG_Model(lhs);
			CSG_Model cSG_Model2 = new CSG_Model(rhs);
			CSG_Node a = new CSG_Node(cSG_Model.ToPolygons());
			CSG_Node b = new CSG_Node(cSG_Model2.ToPolygons());
			List<CSG_Polygon> list = CSG_Node.Intersect(a, b).AllPolygons();
			CSG_Model cSG_Model3 = new CSG_Model(list);
			return cSG_Model3.ToMesh();
		}
	}
}
