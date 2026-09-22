using System.Collections.Generic;
using UnityEngine;

namespace Parabox.CSG
{
	internal class CSG_Model
	{
		public List<CSG_Vertex> vertices;

		public List<int> indices;

		public CSG_Model()
		{
			vertices = new List<CSG_Vertex>();
			indices = new List<int>();
		}

		public CSG_Model(GameObject go)
		{
			vertices = new List<CSG_Vertex>();
			Mesh sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
			Transform component = go.GetComponent<Transform>();
			Vector3[] array = sharedMesh.vertices;
			Vector3[] normals = sharedMesh.normals;
			Vector2[] uv = sharedMesh.uv;
			Color[] colors = sharedMesh.colors;
			for (int i = 0; i < array.Length; i++)
			{
				vertices.Add(new CSG_Vertex(component.TransformPoint(array[i]), component.TransformDirection(normals[i]), uv[i], colors[i]));
			}
			indices = new List<int>(sharedMesh.triangles);
		}

		public CSG_Model(List<CSG_Polygon> list)
		{
			vertices = new List<CSG_Vertex>();
			indices = new List<int>();
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				CSG_Polygon cSG_Polygon = list[i];
				for (int j = 2; j < cSG_Polygon.vertices.Count; j++)
				{
					vertices.Add(cSG_Polygon.vertices[0]);
					indices.Add(num++);
					vertices.Add(cSG_Polygon.vertices[j - 1]);
					indices.Add(num++);
					vertices.Add(cSG_Polygon.vertices[j]);
					indices.Add(num++);
				}
			}
		}

		public List<CSG_Polygon> ToPolygons()
		{
			List<CSG_Polygon> list = new List<CSG_Polygon>();
			for (int i = 0; i < indices.Count; i += 3)
			{
				List<CSG_Vertex> list2 = new List<CSG_Vertex>();
				list2.Add(vertices[indices[i]]);
				list2.Add(vertices[indices[i + 1]]);
				list2.Add(vertices[indices[i + 2]]);
				List<CSG_Vertex> list3 = list2;
				list.Add(new CSG_Polygon(list3));
			}
			return list;
		}

		public Mesh ToMesh()
		{
			Mesh mesh = new Mesh();
			int count = vertices.Count;
			Vector3[] array = new Vector3[count];
			Vector3[] array2 = new Vector3[count];
			Vector2[] array3 = new Vector2[count];
			Color[] array4 = new Color[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = vertices[i].position;
				array2[i] = vertices[i].normal;
				array3[i] = vertices[i].uv;
				array4[i] = vertices[i].color;
			}
			mesh.vertices = array;
			mesh.normals = array2;
			mesh.colors = array4;
			mesh.uv = array3;
			mesh.triangles = indices.ToArray();
			return mesh;
		}
	}
}
