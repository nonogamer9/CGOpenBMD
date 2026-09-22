using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Parabox.STL
{
	public static class pb_Stl_Exporter
	{
		public static bool Export(string path, GameObject[] gameObjects, FileType type)
		{
			Mesh[] array = CreateWorldSpaceMeshesWithTransforms(gameObjects.Select((GameObject x) => x.transform).ToArray());
			bool result = false;
			if (array != null && array.Length != 0 && !string.IsNullOrEmpty(path))
			{
				result = pb_Stl.WriteFile(path, array, type);
			}
			int num = 0;
			while (array != null && num < array.Length)
			{
				Object.DestroyImmediate(array[num]);
				num++;
			}
			return result;
		}

		private static Mesh[] CreateWorldSpaceMeshesWithTransforms(IList<Transform> transforms)
		{
			if (transforms == null || transforms.Count < 1)
			{
				return null;
			}
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < transforms.Count; i++)
			{
				zero += transforms[i].position;
			}
			Vector3 position = zero / transforms.Count;
			GameObject gameObject = new GameObject();
			gameObject.name = "ROOT";
			gameObject.transform.position = position;
			foreach (Transform transform2 in transforms)
			{
				GameObject obj = (GameObject)Object.Instantiate((Object)transform2.gameObject);
				obj.transform.SetParent(transform2.parent, false);
				obj.transform.SetParent(gameObject.transform, true);
			}
			gameObject.transform.position = Vector3.zero;
			List<MeshFilter> list = (from x in gameObject.GetComponentsInChildren<MeshFilter>()
				where x.sharedMesh != null
				select x).ToList();
			int count = list.Count;
			Mesh[] array = new Mesh[count];
			for (int num = 0; num < count; num++)
			{
				Transform transform = list[num].transform;
				Vector3[] vertices = list[num].sharedMesh.vertices;
				Vector3[] normals = list[num].sharedMesh.normals;
				for (int num2 = 0; num2 < vertices.Length; num2++)
				{
					vertices[num2] = transform.TransformPoint(vertices[num2]);
					normals[num2] = transform.TransformDirection(normals[num2]);
				}
				Mesh mesh = new Mesh();
				mesh.name = list[num].name;
				mesh.vertices = vertices;
				mesh.normals = normals;
				mesh.triangles = list[num].sharedMesh.triangles;
				array[num] = mesh;
			}
			Object.DestroyImmediate(gameObject);
			return array;
		}
	}
}
