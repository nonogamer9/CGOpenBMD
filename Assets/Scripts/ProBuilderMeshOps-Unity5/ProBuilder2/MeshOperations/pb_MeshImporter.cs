using System;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using UnityEngine;

namespace ProBuilder2.MeshOperations
{
	public class pb_MeshImporter
	{
		public class Settings
		{
			public bool quads = true;

			public bool smoothing = true;

			public float smoothingThreshold = 1f;

			public static Settings Default
			{
				get
				{
					Settings settings = new Settings();
					settings.quads = true;
					settings.smoothing = true;
					settings.smoothingThreshold = 1f;
					return settings;
				}
			}

			public override string ToString()
			{
				return string.Format("quads: {0}\nsmoothing: {1}\nthreshold: {2}", quads, smoothing, smoothingThreshold);
			}
		}

		private static readonly Settings DEFAULT_IMPORT_SETTINGS = new Settings
		{
			quads = true,
			smoothing = true,
			smoothingThreshold = 1f
		};

		private pb_Object m_Mesh;

		private pb_Vertex[] m_Vertices;

		public pb_MeshImporter(pb_Object target)
		{
			m_Mesh = target;
		}

		public bool Import(GameObject go, Settings importSettings = null)
		{
			MeshFilter component = go.GetComponent<MeshFilter>();
			MeshRenderer component2 = go.GetComponent<MeshRenderer>();
			if (component == null)
			{
				return false;
			}
			return Import(component.sharedMesh, (!component2) ? null : component2.sharedMaterials, importSettings);
		}

		public bool Import(Mesh originalMesh, Material[] materials, Settings importSettings = null)
		{
			if (importSettings == null)
			{
				importSettings = DEFAULT_IMPORT_SETTINGS;
			}
			pb_Vertex[] vertices = pb_Vertex.GetVertices(originalMesh);
			List<pb_Vertex> list = new List<pb_Vertex>();
			List<pb_Face> list2 = new List<pb_Face>();
			int num = 0;
			int num2 = ((materials != null) ? materials.Length : 0);
			for (int i = 0; i < originalMesh.subMeshCount; i++)
			{
				Material m = ((num2 <= 0) ? pb_Constant.DefaultMaterial : materials[i % num2]);
				switch (originalMesh.GetTopology(i))
				{
				case MeshTopology.Triangles:
				{
					int[] indices2 = originalMesh.GetIndices(i);
					for (int k = 0; k < indices2.Length; k += 3)
					{
						list2.Add(new pb_Face(new int[3]
						{
							num,
							num + 1,
							num + 2
						}, m, new pb_UV(), 0, -1, -1, true));
						list.Add(vertices[indices2[k]]);
						list.Add(vertices[indices2[k + 1]]);
						list.Add(vertices[indices2[k + 2]]);
						num += 3;
					}
					break;
				}
				case MeshTopology.Quads:
				{
					int[] indices = originalMesh.GetIndices(i);
					for (int j = 0; j < indices.Length; j += 4)
					{
						list2.Add(new pb_Face(new int[6]
						{
							num,
							num + 1,
							num + 2,
							num + 1,
							num + 2,
							num + 3
						}, m, new pb_UV(), 0, -1, -1, true));
						list.Add(vertices[indices[j]]);
						list.Add(vertices[indices[j + 1]]);
						list.Add(vertices[indices[j + 2]]);
						list.Add(vertices[indices[j + 3]]);
						num += 4;
					}
					break;
				}
				default:
					throw new NotImplementedException("ProBuilder only supports importing triangle and quad meshes.");
				}
			}
			m_Vertices = list.ToArray();
			m_Mesh.Clear();
			m_Mesh.SetVertices(m_Vertices);
			m_Mesh.SetFaces(list2);
			m_Mesh.SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(m_Mesh.vertices));
			m_Mesh.SetSharedIndicesUV(new pb_IntArray[0]);
			HashSet<pb_Face> hashSet = new HashSet<pb_Face>();
			if (importSettings.quads)
			{
				List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GetWingedEdges(m_Mesh, m_Mesh.faces, true);
				Dictionary<pb_EdgeLookup, float> dictionary = new Dictionary<pb_EdgeLookup, float>();
				for (int l = 0; l < wingedEdges.Count; l++)
				{
					foreach (pb_WingedEdge item in wingedEdges[l])
					{
						if (item.opposite != null && !dictionary.ContainsKey(item.edge))
						{
							float quadScore = GetQuadScore(item, item.opposite);
							dictionary.Add(item.edge, quadScore);
						}
					}
				}
				List<pb_Tuple<pb_Face, pb_Face>> list3 = new List<pb_Tuple<pb_Face, pb_Face>>();
				foreach (pb_WingedEdge item2 in wingedEdges)
				{
					if (!hashSet.Add(item2.face))
					{
						continue;
					}
					float num3 = 0f;
					pb_Face pb_Face2 = null;
					foreach (pb_WingedEdge item3 in item2)
					{
						float value;
						if ((item3.opposite == null || !hashSet.Contains(item3.opposite.face)) && dictionary.TryGetValue(item3.edge, out value) && value > num3 && item2.face == GetBestQuadConnection(item3.opposite, dictionary))
						{
							num3 = value;
							pb_Face2 = item3.opposite.face;
						}
					}
					if (pb_Face2 != null)
					{
						hashSet.Add(pb_Face2);
						list3.Add(new pb_Tuple<pb_Face, pb_Face>(item2.face, pb_Face2));
					}
				}
				pb_MergeFaces.MergePairs(m_Mesh, list3, !importSettings.smoothing);
			}
			if (importSettings.smoothing)
			{
				pb_Smoothing.ApplySmoothingGroups(m_Mesh, m_Mesh.faces, importSettings.smoothingThreshold, m_Vertices.Select((pb_Vertex x) => x.normal).ToArray());
				pb_MergeFaces.CollapseCoincidentVertices(m_Mesh, m_Mesh.faces);
			}
			return false;
		}

		private pb_Face GetBestQuadConnection(pb_WingedEdge wing, Dictionary<pb_EdgeLookup, float> connections)
		{
			float num = 0f;
			pb_Face result = null;
			foreach (pb_WingedEdge item in wing)
			{
				float value = 0f;
				if (connections.TryGetValue(item.edge, out value) && value > num)
				{
					num = connections[item.edge];
					result = item.opposite.face;
				}
			}
			return result;
		}

		private float GetQuadScore(pb_WingedEdge left, pb_WingedEdge right, float normalThreshold = 0.9f)
		{
			int[] array = pb_WingedEdge.MakeQuad(left, right);
			if (array == null)
			{
				return 0f;
			}
			Vector3 lhs = pb_Math.Normal(m_Vertices[array[0]].position, m_Vertices[array[1]].position, m_Vertices[array[2]].position);
			Vector3 rhs = pb_Math.Normal(m_Vertices[array[2]].position, m_Vertices[array[3]].position, m_Vertices[array[0]].position);
			float num = Vector3.Dot(lhs, rhs);
			if (num < normalThreshold)
			{
				return 0f;
			}
			Vector3 vector = m_Vertices[array[1]].position - m_Vertices[array[0]].position;
			Vector3 vector2 = m_Vertices[array[2]].position - m_Vertices[array[1]].position;
			Vector3 vector3 = m_Vertices[array[3]].position - m_Vertices[array[2]].position;
			Vector3 vector4 = m_Vertices[array[0]].position - m_Vertices[array[3]].position;
			vector.Normalize();
			vector2.Normalize();
			vector3.Normalize();
			vector4.Normalize();
			float num2 = Mathf.Abs(Vector3.Dot(vector, vector2));
			float num3 = Mathf.Abs(Vector3.Dot(vector2, vector3));
			float num4 = Mathf.Abs(Vector3.Dot(vector3, vector4));
			float num5 = Mathf.Abs(Vector3.Dot(vector4, vector));
			num += 1f - (num2 + num3 + num4 + num5) * 0.25f;
			num += Mathf.Abs(Vector3.Dot(vector, vector3)) * 0.5f;
			num += Mathf.Abs(Vector3.Dot(vector2, vector4)) * 0.5f;
			return num * 0.33f;
		}
	}
}
