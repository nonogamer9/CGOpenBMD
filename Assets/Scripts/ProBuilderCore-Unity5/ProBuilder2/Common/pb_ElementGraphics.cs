using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProBuilder2.Common
{
	[Serializable]
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class pb_ElementGraphics : pb_MonoBehaviourSingleton<pb_ElementGraphics>
	{
		private const string FACE_SHADER = "Hidden/ProBuilder/FaceHighlight";

		private const string EDGE_SHADER = "Hidden/ProBuilder/FaceHighlight";

		private const string VERT_SHADER = "Hidden/ProBuilder/pb_VertexShader";

		private const string PREVIEW_OBJECT_NAME = "ProBuilderSelectionGameObject";

		private const string WIREFRAME_OBJECT_NAME = "ProBuilderWireframeGameObject";

		private const string SELECTION_MESH_NAME = "ProBuilderEditorSelectionMesh";

		private const string WIREFRAME_MESH_NAME = "ProBuilderEditorWireframeMesh";

		private static float vertexHandleSize = 0.03f;

		[SerializeField]
		private Material faceMaterial;

		[SerializeField]
		private Material vertexMaterial;

		[SerializeField]
		private Material wireframeMaterial;

		[SerializeField]
		private Color faceSelectionColor = new Color(0f, 1f, 1f, 0.275f);

		[SerializeField]
		private Color edgeSelectionColor = new Color(0f, 0.6f, 0.7f, 1f);

		[SerializeField]
		private Color vertSelectionColor = new Color(1f, 0.2f, 0.2f, 1f);

		[SerializeField]
		private Color wireframeColor = new Color(0.53f, 0.65f, 0.84f, 1f);

		[SerializeField]
		private Color vertexDotColor = new Color(0.8f, 0.8f, 0.8f, 1f);

		private static readonly HideFlags PB_EDITOR_GRAPHIC_HIDE_FLAGS = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

		private pb_ObjectPool<pb_Renderable> pool;

		private List<pb_Renderable> activeRenderables = new List<pb_Renderable>();

		public override void Awake()
		{
			base.Awake();
			base.gameObject.hideFlags = HideFlags.HideAndDontSave;
			if (pb_MonoBehaviourSingleton<pb_MeshRenderer>.nullableInstance == null)
			{
				base.gameObject.AddComponent<pb_MeshRenderer>();
			}
			wireframeMaterial = CreateMaterial(Shader.Find("Hidden/ProBuilder/FaceHighlight"), "WIREFRAME_MATERIAL");
			wireframeMaterial.SetColor("_Color", wireframeColor);
			faceMaterial = CreateMaterial(Shader.Find("Hidden/ProBuilder/FaceHighlight"), "FACE_SELECTION_MATERIAL");
			faceMaterial.SetColor("_Color", faceSelectionColor);
			vertexMaterial = CreateMaterial(Shader.Find("Hidden/ProBuilder/pb_VertexShader"), "VERTEX_BILLBOARD_MATERIAL");
			vertexMaterial.SetColor("_Color", vertexDotColor);
			vertexMaterial.SetFloat("_Scale", vertexHandleSize * 4f);
		}

		private void OnDestroy()
		{
			UnityEngine.Object.DestroyImmediate(faceMaterial);
			UnityEngine.Object.DestroyImmediate(vertexMaterial);
			UnityEngine.Object.DestroyImmediate(wireframeMaterial);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			pool = new pb_ObjectPool<pb_Renderable>(0, 8, pb_Renderable.CreateInstance, pb_Renderable.DestroyInstance);
		}

		private void OnDisable()
		{
			pool.Empty();
		}

		private Material CreateMaterial(Shader shader, string materialName)
		{
			Material material = new Material(shader);
			material.name = materialName;
			material.hideFlags = PB_EDITOR_GRAPHIC_HIDE_FLAGS;
			return material;
		}

		public void LoadPrefs(Color in_faceSelectionColor, Color in_edgeSelectionColor, Color in_vertSelectionColor, Color in_vertexDotColor, float in_vertexHandleSize)
		{
			faceSelectionColor = in_faceSelectionColor;
			edgeSelectionColor = in_edgeSelectionColor;
			vertSelectionColor = in_vertSelectionColor;
			vertexDotColor = in_vertexDotColor;
			vertexHandleSize = in_vertexHandleSize;
			wireframeMaterial.SetColor("_Color", wireframeColor);
			faceMaterial.SetColor("_Color", faceSelectionColor);
			vertexMaterial.SetColor("_Color", vertexDotColor);
			vertexMaterial.SetFloat("_Scale", vertexHandleSize * 4f);
		}

		private void AddRenderable(pb_Renderable ren)
		{
			activeRenderables.Add(ren);
			pb_MeshRenderer.Add(ren);
		}

		public void RebuildGraphics(pb_Object[] selection, pb_Edge[][] universalEdgesDistinct, EditLevel editLevel, SelectMode selectionMode)
		{
			if (pool == null)
			{
				return;
			}
			foreach (pb_Renderable activeRenderable in activeRenderables)
			{
				pool.Put(activeRenderable);
				pb_MeshRenderer.Remove(activeRenderable);
			}
			wireframeMaterial.SetColor("_Color", (selectionMode != SelectMode.Edge || editLevel != EditLevel.Geometry) ? wireframeColor : edgeSelectionColor);
			for (int i = 0; i < selection.Length; i++)
			{
				AddRenderable(BuildEdgeMesh(selection[i], universalEdgesDistinct[i]));
			}
			if (editLevel != EditLevel.Geometry)
			{
				return;
			}
			switch (selectionMode)
			{
			case SelectMode.Face:
				foreach (pb_Object pb2 in selection)
				{
					AddRenderable(BuildFaceMesh(pb2));
				}
				break;
			case SelectMode.Vertex:
				foreach (pb_Object pb in selection)
				{
					AddRenderable(BuildVertexMesh(pb));
				}
				break;
			}
		}

		private pb_Renderable BuildFaceMesh(pb_Object pb)
		{
			pb_Renderable pb_Renderable2 = pool.Get();
			pb_Renderable2.name = "Faces Renderable";
			pb_Renderable2.transform = pb.transform;
			pb_Renderable2.materials = new Material[1] { faceMaterial };
			pb_Renderable2.mesh.Clear();
			pb_Renderable2.mesh.vertices = pb.vertices;
			pb_Renderable2.mesh.triangles = pb_Face.AllTriangles(pb.SelectedFaces);
			return pb_Renderable2;
		}

		private pb_Renderable BuildVertexMesh(pb_Object pb)
		{
			ushort num = 16383;
			int num2 = pb.sharedIndices.Length;
			if (num2 > num)
			{
				num2 = num;
			}
			Vector3[] array = new Vector3[pb.sharedIndices.Length];
			HashSet<int> hashSet = new HashSet<int>(pb.sharedIndices.GetCommonIndices(pb.SelectedTriangles));
			for (int i = 0; i < num2; i++)
			{
				array[i] = pb.vertices[pb.sharedIndices[i][0]];
			}
			Vector3[] array2 = new Vector3[num2 * 4];
			Vector3[] array3 = new Vector3[num2 * 4];
			Vector2[] array4 = new Vector2[num2 * 4];
			Vector2[] array5 = new Vector2[num2 * 4];
			Color[] array6 = new Color[num2 * 4];
			int[] array7 = new int[num2 * 6];
			int num3 = 0;
			int num4 = 0;
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;
			for (int j = 0; j < num2; j++)
			{
				array2[num4] = array[j];
				array2[num4 + 1] = array[j];
				array2[num4 + 2] = array[j];
				array2[num4 + 3] = array[j];
				array4[num4] = Vector3.zero;
				array4[num4 + 1] = Vector3.right;
				array4[num4 + 2] = Vector3.up;
				array4[num4 + 3] = Vector3.one;
				array5[num4] = -up - right;
				array5[num4 + 1] = -up + right;
				array5[num4 + 2] = up - right;
				array5[num4 + 3] = up + right;
				array3[num4] = Vector3.forward;
				array3[num4 + 1] = Vector3.forward;
				array3[num4 + 2] = Vector3.forward;
				array3[num4 + 3] = Vector3.forward;
				array7[num3] = num4;
				array7[num3 + 1] = num4 + 1;
				array7[num3 + 2] = num4 + 2;
				array7[num3 + 3] = num4 + 1;
				array7[num3 + 4] = num4 + 3;
				array7[num3 + 5] = num4 + 2;
				if (hashSet.Contains(j))
				{
					array6[num4] = vertSelectionColor;
					array6[num4 + 1] = vertSelectionColor;
					array6[num4 + 2] = vertSelectionColor;
					array6[num4 + 3] = vertSelectionColor;
				}
				else
				{
					array6[num4] = vertexDotColor;
					array6[num4 + 1] = vertexDotColor;
					array6[num4 + 2] = vertexDotColor;
					array6[num4 + 3] = vertexDotColor;
				}
				num4 += 4;
				num3 += 6;
			}
			pb_Renderable pb_Renderable2 = pool.Get();
			pb_Renderable2.name = "Vertex Renderable";
			pb_Renderable2.transform = pb.transform;
			pb_Renderable2.materials = new Material[1] { vertexMaterial };
			pb_Renderable2.mesh.Clear();
			pb_Renderable2.mesh.vertices = array2;
			pb_Renderable2.mesh.normals = array3;
			pb_Renderable2.mesh.uv = array4;
			pb_Renderable2.mesh.uv2 = array5;
			pb_Renderable2.mesh.colors = array6;
			pb_Renderable2.mesh.triangles = array7;
			return pb_Renderable2;
		}

		private pb_Renderable BuildEdgeMesh(pb_Object pb, pb_Edge[] universalEdgesDistinct)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			int num = universalEdgesDistinct.Length;
			int[] array = new int[num * 2];
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				array[num2++] = sharedIndices[universalEdgesDistinct[i].x][0];
				array[num2++] = sharedIndices[universalEdgesDistinct[i].y][0];
			}
			pb_Renderable pb_Renderable2 = pool.Get();
			pb_Renderable2.name = "Wireframe Renderable";
			pb_Renderable2.materials = new Material[1] { wireframeMaterial };
			pb_Renderable2.transform = pb.transform;
			pb_Renderable2.mesh.name = "Wireframe Mesh";
			pb_Renderable2.mesh.Clear();
			pb_Renderable2.mesh.vertices = pb.vertices;
			pb_Renderable2.mesh.normals = pb.vertices;
			pb_Renderable2.mesh.uv = new Vector2[pb.vertexCount];
			pb_Renderable2.mesh.subMeshCount = 1;
			pb_Renderable2.mesh.SetIndices(array, MeshTopology.Lines, 0);
			return pb_Renderable2;
		}
	}
}
