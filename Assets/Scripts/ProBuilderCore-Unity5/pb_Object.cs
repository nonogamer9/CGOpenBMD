using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ProBuilder2.Common;
using UnityEngine;

[AddComponentMenu("")]
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(pb_Entity))]
[ExecuteInEditMode]
public class pb_Object : MonoBehaviour
{
	[SerializeField]
	private pb_Face[] _quads;

	[SerializeField]
	private pb_IntArray[] _sharedIndices;

	[SerializeField]
	private Vector3[] _vertices;

	[SerializeField]
	private Vector2[] _uv;

	[SerializeField]
	private List<Vector4> _uv3;

	[SerializeField]
	private List<Vector4> _uv4;

	[SerializeField]
	private Vector4[] _tangents;

	[SerializeField]
	private pb_IntArray[] _sharedIndicesUV = new pb_IntArray[0];

	[SerializeField]
	private Color[] _colors;

	public bool userCollisions;

	public bool isSelectable = true;

	public pb_UnwrapParameters unwrapParameters = new pb_UnwrapParameters();

	public string asset_guid;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static Action<pb_Object> onDestroyObject__BackingField;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private static Action<pb_Object> onElementSelectionChanged__BackingField;

	public bool dontDestroyMeshOnDelete;

	[SerializeField]
	private int[] m_selectedFaces = new int[0];

	[SerializeField]
	private pb_Edge[] m_SelectedEdges = new pb_Edge[0];

	[SerializeField]
	private int[] m_selectedTriangles = new int[0];

	private pb_Face[] _faces
	{
		get
		{
			return _quads;
		}
	}

	public Mesh msh
	{
		get
		{
			return GetComponent<MeshFilter>().sharedMesh;
		}
		set
		{
			base.gameObject.GetComponent<MeshFilter>().sharedMesh = value;
		}
	}

	public pb_Face[] faces
	{
		get
		{
			return _quads;
		}
	}

	public pb_Face[] quads
	{
		get
		{
			pb_Log.Warning("pb_Quad is deprecated.  Please use pb_Face instead.");
			return _quads;
		}
	}

	public pb_IntArray[] sharedIndices
	{
		get
		{
			return _sharedIndices;
		}
	}

	public pb_IntArray[] sharedIndicesUV
	{
		get
		{
			return _sharedIndicesUV;
		}
	}

	public int id
	{
		get
		{
			return base.gameObject.GetInstanceID();
		}
	}

	public Vector3[] vertices
	{
		get
		{
			return _vertices;
		}
	}

	public Color[] colors
	{
		get
		{
			return _colors;
		}
	}

	public Vector2[] uv
	{
		get
		{
			return _uv;
		}
	}

	public bool hasUv3
	{
		get
		{
			return _uv3 != null && _uv3.Count == vertexCount;
		}
	}

	public bool hasUv4
	{
		get
		{
			return _uv4 != null && _uv4.Count == vertexCount;
		}
	}

	public List<Vector4> uv3
	{
		get
		{
			return _uv3;
		}
	}

	public List<Vector4> uv4
	{
		get
		{
			return _uv4;
		}
	}

	public int faceCount
	{
		get
		{
			return (_faces != null) ? _faces.Length : 0;
		}
	}

	public int vertexCount
	{
		get
		{
			return (_vertices != null) ? _vertices.Length : 0;
		}
	}

	public int triangleCount
	{
		get
		{
			return (_faces != null) ? _faces.Sum((pb_Face x) => x.indices.Length) : 0;
		}
	}

	public pb_Face[] SelectedFaces
	{
		get
		{
			return faces.ValuesWithIndices(m_selectedFaces);
		}
	}

	public int SelectedFaceCount
	{
		get
		{
			return m_selectedFaces.Length;
		}
	}

	public int[] SelectedTriangles
	{
		get
		{
			return m_selectedTriangles;
		}
	}

	public int SelectedTriangleCount
	{
		get
		{
			return m_selectedTriangles.Length;
		}
	}

	public pb_Edge[] SelectedEdges
	{
		get
		{
			return m_SelectedEdges;
		}
	}

	public int SelectedEdgeCount
	{
		get
		{
			return m_SelectedEdges.Length;
		}
	}

	public static event Action<pb_Object> onDestroyObject
	{
		add
		{
			Action<pb_Object> action = onDestroyObject__BackingField;
			Action<pb_Object> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref onDestroyObject__BackingField, (Action<pb_Object>)Delegate.Combine(action2, value), action);
			}
			while ((object)action != action2);
		}
		remove
		{
			Action<pb_Object> action = onDestroyObject__BackingField;
			Action<pb_Object> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref onDestroyObject__BackingField, (Action<pb_Object>)Delegate.Remove(action2, value), action);
			}
			while ((object)action != action2);
		}
	}

	internal static event Action<pb_Object> onElementSelectionChanged
	{
		add
		{
			Action<pb_Object> action = onElementSelectionChanged__BackingField;
			Action<pb_Object> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref onElementSelectionChanged__BackingField, (Action<pb_Object>)Delegate.Combine(action2, value), action);
			}
			while ((object)action != action2);
		}
		remove
		{
			Action<pb_Object> action = onElementSelectionChanged__BackingField;
			Action<pb_Object> action2;
			do
			{
				action2 = action;
				action = Interlocked.CompareExchange(ref onElementSelectionChanged__BackingField, (Action<pb_Object>)Delegate.Remove(action2, value), action);
			}
			while ((object)action != action2);
		}
	}

	private void Awake()
	{
		if (!GetComponent<MeshRenderer>().isPartOfStaticBatch)
		{
			Vector3[] array = ((!(msh != null)) ? null : msh.normals);
			if ((array == null || array.Length != msh.vertexCount || (array.Length > 0 && array[0] == Vector3.zero)) && _vertices != null)
			{
				ToMesh();
				Refresh();
			}
		}
	}

	public static pb_Object InitWithObject(pb_Object pb)
	{
		Vector3[] array = new Vector3[pb.vertexCount];
		Array.Copy(pb.vertices, array, pb.vertexCount);
		Vector2[] array2 = new Vector2[pb.vertexCount];
		Array.Copy(pb.uv, array2, pb.vertexCount);
		Color[] array3 = new Color[pb.vertexCount];
		Array.Copy(pb.colors, array3, pb.vertexCount);
		pb_Face[] array4 = new pb_Face[pb.faces.Length];
		for (int i = 0; i < array4.Length; i++)
		{
			array4[i] = new pb_Face(pb.faces[i]);
		}
		pb_Object pb_Object2 = CreateInstanceWithElements(array, array2, array3, array4, pb.GetSharedIndices(), pb.GetSharedIndicesUV());
		pb_Object2.gameObject.name = pb.gameObject.name + "-clone";
		return pb_Object2;
	}

	public static pb_Object CreateInstanceWithPoints(Vector3[] vertices)
	{
		if (vertices.Length % 4 != 0)
		{
			pb_Log.Warning("Invalid Geometry.  Make sure vertices in are pairs of 4 (faces).");
			return null;
		}
		GameObject gameObject = new GameObject();
		pb_Object pb_Object2 = gameObject.AddComponent<pb_Object>();
		gameObject.name = "ProBuilder Mesh";
		pb_Object2.GeometryWithPoints(vertices);
		pb_Object2.GetComponent<pb_Entity>().SetEntity(EntityType.Detail);
		return pb_Object2;
	}

	public static pb_Object CreateInstanceWithVerticesFaces(Vector3[] v, pb_Face[] f)
	{
		GameObject gameObject = new GameObject();
		pb_Object pb_Object2 = gameObject.AddComponent<pb_Object>();
		gameObject.name = "ProBuilder Mesh";
		pb_Object2.GeometryWithVerticesFaces(v, f);
		return pb_Object2;
	}

	public static pb_Object CreateInstanceWithElements(Vector3[] v, Vector2[] u, Color[] c, pb_Face[] f, pb_IntArray[] si, pb_IntArray[] si_uv)
	{
		GameObject gameObject = new GameObject();
		pb_Object pb_Object2 = gameObject.AddComponent<pb_Object>();
		pb_Object2.SetVertices(v);
		pb_Object2.SetUV(u);
		pb_Object2.SetColors(c);
		pb_Object2.SetSharedIndices(si ?? pb_IntArrayUtility.ExtractSharedIndices(v));
		pb_Object2.SetSharedIndicesUV(si_uv ?? new pb_IntArray[0]);
		pb_Object2.SetFaces(f);
		pb_Object2.ToMesh();
		pb_Object2.Refresh();
		pb_Object2.GetComponent<pb_Entity>().SetEntity(EntityType.Detail);
		return pb_Object2;
	}

	public static pb_Object CreateInstanceWithElements(pb_Vertex[] vertices, pb_Face[] faces, pb_IntArray[] si, pb_IntArray[] si_uv)
	{
		GameObject gameObject = new GameObject();
		pb_Object pb_Object2 = gameObject.AddComponent<pb_Object>();
		Vector3[] position;
		Color[] color;
		Vector2[] uv;
		Vector3[] normal;
		Vector4[] tangent;
		Vector2[] uv2;
		List<Vector4> list;
		List<Vector4> list2;
		pb_Vertex.GetArrays(vertices, out position, out color, out uv, out normal, out tangent, out uv2, out list, out list2);
		pb_Object2.SetVertices(position);
		pb_Object2.SetColors(color);
		pb_Object2.SetUV(uv);
		if (list != null)
		{
			pb_Object2._uv3 = list;
		}
		if (list2 != null)
		{
			pb_Object2._uv4 = list2;
		}
		pb_Object2.SetSharedIndices(si ?? pb_IntArrayUtility.ExtractSharedIndices(position));
		pb_Object2.SetSharedIndicesUV(si_uv ?? new pb_IntArray[0]);
		pb_Object2.SetFaces(faces);
		pb_Object2.ToMesh();
		pb_Object2.Refresh();
		pb_Object2.GetComponent<pb_Entity>().SetEntity(EntityType.Detail);
		return pb_Object2;
	}

	public void Clear()
	{
		_quads = new pb_Face[0];
		_vertices = new Vector3[0];
		_uv = new Vector2[0];
		_uv3 = null;
		_uv4 = null;
		_tangents = null;
		_sharedIndices = new pb_IntArray[0];
		_sharedIndicesUV = null;
		_colors = null;
		SetSelectedTriangles(null);
	}

	public Vector3[] GetNormals()
	{
		Vector3[] normals = null;
		if (msh.vertexCount == vertexCount)
		{
			normals = msh.normals;
		}
		if (normals == null || normals.Length != vertexCount)
		{
			normals = pb_MeshUtility.GenerateNormals(this);
			pb_MeshUtility.SmoothNormals(this, ref normals);
		}
		return normals;
	}

	public pb_IntArray[] GetSharedIndices()
	{
		int num = _sharedIndices.Length;
		pb_IntArray[] array = new pb_IntArray[num];
		for (int i = 0; i < num; i++)
		{
			int[] array2 = new int[_sharedIndices[i].Length];
			Array.Copy(_sharedIndices[i].array, array2, array2.Length);
			array[i] = new pb_IntArray(array2);
		}
		return array;
	}

	public pb_IntArray[] GetSharedIndicesUV()
	{
		int num = _sharedIndicesUV.Length;
		pb_IntArray[] array = new pb_IntArray[num];
		for (int i = 0; i < num; i++)
		{
			int[] array2 = new int[_sharedIndicesUV[i].Length];
			Array.Copy(_sharedIndicesUV[i].array, array2, array2.Length);
			array[i] = new pb_IntArray(array2);
		}
		return array;
	}

	public void AddToFaceSelection(pb_Face face)
	{
		int num = Array.IndexOf(faces, face);
		if (num > -1)
		{
			SetSelectedFaces(m_selectedFaces.Add(num));
		}
	}

	public void SetSelectedFaces(IEnumerable<pb_Face> selected)
	{
		List<int> list = new List<int>();
		foreach (pb_Face item in selected)
		{
			int num = Array.IndexOf(faces, item);
			if (num > -1)
			{
				list.Add(num);
			}
		}
		SetSelectedFaces(list);
	}

	public void SetSelectedFaces(IEnumerable<int> selected)
	{
		m_selectedFaces = selected.ToArray();
		m_selectedTriangles = pb_Face.AllTriangles(SelectedFaces);
		pb_Edge[] array = pb_Edge.AllEdges(SelectedFaces);
		int num = array.Length;
		m_SelectedEdges = new pb_Edge[num];
		for (int i = 0; i < num; i++)
		{
			m_SelectedEdges[i] = new pb_Edge(array[i]);
		}
		if (onElementSelectionChanged__BackingField != null)
		{
			onElementSelectionChanged__BackingField(this);
		}
	}

	public void SetSelectedEdges(IEnumerable<pb_Edge> edges)
	{
		m_selectedFaces = new int[0];
		m_SelectedEdges = edges.Select((pb_Edge x) => new pb_Edge(x)).ToArray();
		m_selectedTriangles = m_SelectedEdges.AllTriangles();
		if (onElementSelectionChanged__BackingField != null)
		{
			onElementSelectionChanged__BackingField(this);
		}
	}

	public void SetSelectedTriangles(int[] tris)
	{
		m_selectedFaces = new int[0];
		m_SelectedEdges = new pb_Edge[0];
		m_selectedTriangles = tris ?? new int[0];
		if (onElementSelectionChanged__BackingField != null)
		{
			onElementSelectionChanged__BackingField(this);
		}
	}

	public void RemoveFromFaceSelectionAtIndex(int index)
	{
		SetSelectedFaces(m_selectedFaces.RemoveAt(index));
	}

	public void RemoveFromFaceSelection(pb_Face face)
	{
		int num = Array.IndexOf(faces, face);
		if (num > -1)
		{
			SetSelectedFaces(m_selectedFaces.Remove(num));
		}
	}

	public void ClearSelection()
	{
		m_selectedFaces = new int[0];
		m_SelectedEdges = new pb_Edge[0];
		m_selectedTriangles = new int[0];
	}

	public void SetVertices(Vector3[] v)
	{
		_vertices = v;
	}

	public void SetVertices(IList<pb_Vertex> vertices, bool applyMesh = false)
	{
		Vector3[] position;
		Color[] color;
		Vector2[] uv;
		Vector3[] normal;
		Vector4[] tangent;
		Vector2[] uv2;
		List<Vector4> list;
		List<Vector4> list2;
		pb_Vertex.GetArrays(vertices, out position, out color, out uv, out normal, out tangent, out uv2, out list, out list2);
		SetVertices(position);
		SetColors(color);
		SetUV(uv);
		if (list != null)
		{
			_uv3 = list;
		}
		if (list2 != null)
		{
			_uv4 = list2;
		}
		if (applyMesh)
		{
			Mesh mesh = msh;
			pb_Vertex pb_Vertex2 = vertices[0];
			if (pb_Vertex2.hasPosition)
			{
				mesh.vertices = position;
			}
			if (pb_Vertex2.hasColor)
			{
				mesh.colors = color;
			}
			if (pb_Vertex2.hasUv0)
			{
				mesh.uv = uv;
			}
			if (pb_Vertex2.hasNormal)
			{
				mesh.normals = normal;
			}
			if (pb_Vertex2.hasTangent)
			{
				mesh.tangents = tangent;
			}
			if (pb_Vertex2.hasUv2)
			{
				mesh.uv2 = uv2;
			}
			if (pb_Vertex2.hasUv3 && list != null)
			{
				mesh.SetUVs(2, list);
			}
			if (pb_Vertex2.hasUv4 && list2 != null)
			{
				mesh.SetUVs(3, list2);
			}
		}
	}

	public void SetUV(Vector2[] uvs)
	{
		_uv = uvs;
	}

	public void SetFaces(IEnumerable<pb_Face> faces)
	{
		_quads = faces.Where((pb_Face x) => x != null).ToArray();
		if (_quads.Length != faces.Count())
		{
			pb_Log.Warning("SetFaces() pruned " + (faces.Count() - _quads.Length) + " null faces from this object.");
		}
	}

	public void SetSharedIndices(pb_IntArray[] si)
	{
		_sharedIndices = si;
	}

	public void SetSharedIndices(IEnumerable<KeyValuePair<int, int>> si)
	{
		_sharedIndices = si.ToSharedIndices();
	}

	public void SetSharedIndicesUV(pb_IntArray[] si)
	{
		_sharedIndicesUV = si;
	}

	public void SetSharedIndicesUV(IEnumerable<KeyValuePair<int, int>> si)
	{
		_sharedIndicesUV = si.ToSharedIndices();
	}

	private void GeometryWithPoints(Vector3[] v)
	{
		pb_Face[] array = new pb_Face[v.Length / 4];
		for (int i = 0; i < v.Length; i += 4)
		{
			array[i / 4] = new pb_Face(new int[6]
			{
				i,
				i + 1,
				i + 2,
				i + 1,
				i + 3,
				i + 2
			}, pb_Constant.DefaultMaterial, new pb_UV(), 0, -1, -1, false);
		}
		SetVertices(v);
		SetUV(new Vector2[v.Length]);
		SetColors(pbUtil.FilledArray(Color.white, v.Length));
		SetFaces(array);
		SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(v));
		ToMesh();
		Refresh();
	}

	public void GeometryWithVerticesFaces(Vector3[] v, pb_Face[] f)
	{
		SetVertices(v);
		SetUV(new Vector2[v.Length]);
		SetFaces(f);
		SetSharedIndices(pb_IntArrayUtility.ExtractSharedIndices(v));
		ToMesh();
		Refresh();
	}

	private void GeometryWithVerticesFacesIndices(Vector3[] v, pb_Face[] f, pb_IntArray[] s)
	{
		SetFaces(f);
		SetVertices(v);
		SetUV(new Vector2[v.Length]);
		SetSharedIndices(s);
		if (msh != null)
		{
			UnityEngine.Object.DestroyImmediate(msh);
		}
		ToMesh();
		Refresh();
	}

	public MeshRebuildReason Verify()
	{
		if (msh == null)
		{
			try
			{
				ToMesh();
				Refresh();
			}
			catch (Exception ex)
			{
				pb_Log.Error("Failed rebuilding null pb_Object. Cached mesh attributes are invalid or missing.\n" + ex.ToString());
			}
			return MeshRebuildReason.Null;
		}
		int result;
		int.TryParse(msh.name.Replace("pb_Mesh", string.Empty), out result);
		if (result != id)
		{
			return MeshRebuildReason.InstanceIDMismatch;
		}
		return (msh.uv2 != null) ? MeshRebuildReason.None : MeshRebuildReason.Lightmap;
	}

	public void ToMesh()
	{
		ToMesh(MeshTopology.Triangles);
	}

	public void ToMesh(MeshTopology preferredTopology)
	{
		Mesh mesh = msh;
		if (mesh != null && mesh.vertexCount == _vertices.Length)
		{
			mesh = msh;
		}
		else if (mesh == null)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = _vertices;
		if (_uv != null)
		{
			mesh.uv = _uv;
		}
		mesh.uv2 = null;
		pb_Submesh[] submeshes;
		mesh.subMeshCount = pb_Face.GetMeshIndices(faces, out submeshes, preferredTopology);
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			mesh.SetIndices(submeshes[i].indices, submeshes[i].topology, i, false);
		}
		mesh.name = string.Format("pb_Mesh{0}", id);
		GetComponent<MeshFilter>().sharedMesh = mesh;
		GetComponent<MeshRenderer>().sharedMaterials = submeshes.Select((pb_Submesh x) => x.material).ToArray();
	}

	public void MakeUnique()
	{
		pb_Face[] array = new pb_Face[_faces.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new pb_Face(_faces[i]);
		}
		pb_IntArray[] array2 = new pb_IntArray[_sharedIndices.Length];
		Array.Copy(_sharedIndices, array2, array2.Length);
		SetSharedIndices(array2);
		SetFaces(array);
		Vector3[] destinationArray = new Vector3[vertexCount];
		Array.Copy(_vertices, destinationArray, vertexCount);
		SetVertices(destinationArray);
		if (_uv != null && _uv.Length == vertexCount)
		{
			Vector2[] array3 = new Vector2[vertexCount];
			Array.Copy(_uv, array3, vertexCount);
			SetUV(array3);
		}
		msh = new Mesh();
		ToMesh();
		Refresh();
	}

	public void Refresh(RefreshMask mask = RefreshMask.All)
	{
		if ((int)(mask & RefreshMask.UV) > 0)
		{
			RefreshUV();
		}
		if ((int)(mask & RefreshMask.Colors) > 0)
		{
			RefreshColors();
		}
		if ((int)(mask & RefreshMask.Normals) > 0)
		{
			RefreshNormals();
		}
		if ((int)(mask & RefreshMask.Tangents) > 0)
		{
			RefreshTangents();
		}
		if ((int)(mask & RefreshMask.Collisions) > 0)
		{
			RefreshCollisions();
		}
	}

	public void RefreshCollisions()
	{
		Mesh mesh = msh;
		mesh.RecalculateBounds();
		if (userCollisions || !GetComponent<Collider>())
		{
			return;
		}
		Collider[] components = base.gameObject.GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			Type type = collider.GetType();
			if (type == typeof(BoxCollider))
			{
				((BoxCollider)collider).center = mesh.bounds.center;
				((BoxCollider)collider).size = mesh.bounds.size;
			}
			else if (type == typeof(SphereCollider))
			{
				((SphereCollider)collider).center = mesh.bounds.center;
				((SphereCollider)collider).radius = pb_Math.LargestValue(mesh.bounds.extents);
			}
			else if (type == typeof(CapsuleCollider))
			{
				((CapsuleCollider)collider).center = mesh.bounds.center;
				Vector2 v = new Vector2(mesh.bounds.extents.x, mesh.bounds.extents.z);
				((CapsuleCollider)collider).radius = pb_Math.LargestValue(v);
				((CapsuleCollider)collider).height = mesh.bounds.size.y;
			}
			else if (type == typeof(WheelCollider))
			{
				((WheelCollider)collider).center = mesh.bounds.center;
				((WheelCollider)collider).radius = pb_Math.LargestValue(mesh.bounds.extents);
			}
			else if (type == typeof(MeshCollider))
			{
				base.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
				base.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
			}
		}
	}

	public int GetUnusedTextureGroup(int i = 1)
	{
		while (Array.Exists(faces, (pb_Face element) => element.textureGroup == i))
		{
			i++;
		}
		return i;
	}

	public int UnusedElementGroup(int i = 1)
	{
		while (Array.Exists(faces, (pb_Face element) => element.elementGroup == i))
		{
			i++;
		}
		return i;
	}

	public void RefreshUV()
	{
		RefreshUV(faces);
	}

	public void GetUVs(int channel, List<Vector4> uvs)
	{
		uvs.Clear();
		switch (channel)
		{
		default:
		{
			for (int j = 0; j < vertexCount; j++)
			{
				uvs.Add(_uv[j]);
			}
			break;
		}
		case 1:
			if (msh != null && msh.uv2 != null)
			{
				Vector2[] uv = msh.uv2;
				for (int i = 0; i < uv.Length; i++)
				{
					uvs.Add(uv[i]);
				}
			}
			break;
		case 2:
			if (_uv3 != null)
			{
				uvs.AddRange(_uv3);
			}
			break;
		case 3:
			if (_uv4 != null)
			{
				uvs.AddRange(_uv4);
			}
			break;
		}
	}

	public void SetUVs(int channel, List<Vector4> uvs)
	{
		switch (channel)
		{
		case 1:
			msh.uv2 = uvs.Cast<Vector2>().ToArray();
			break;
		case 2:
			_uv3 = uvs;
			break;
		case 3:
			_uv4 = uvs;
			break;
		default:
			_uv = uvs.Cast<Vector2>().ToArray();
			break;
		}
	}

	public void RefreshUV(IEnumerable<pb_Face> facesToRefresh)
	{
		Vector2[] array = msh.uv;
		Vector2[] uvs;
		if (_uv != null && _uv.Length == vertexCount)
		{
			uvs = _uv;
		}
		else if (array != null && array.Length == vertexCount)
		{
			uvs = array;
		}
		else
		{
			pb_Face[] array2 = faces;
			foreach (pb_Face pb_Face2 in array2)
			{
				pb_Face2.manualUV = false;
			}
			facesToRefresh = faces;
			uvs = new Vector2[vertexCount];
		}
		int num = -2;
		Dictionary<int, List<pb_Face>> dictionary = new Dictionary<int, List<pb_Face>>();
		bool flag = false;
		foreach (pb_Face item in facesToRefresh)
		{
			if (item.uv.useWorldSpace)
			{
				flag = true;
			}
			if (item != null && !item.manualUV)
			{
				List<pb_Face> value;
				if (item.textureGroup > 0 && dictionary.TryGetValue(item.textureGroup, out value))
				{
					value.Add(item);
					continue;
				}
				dictionary.Add((item.textureGroup <= 0) ? num-- : item.textureGroup, new List<pb_Face> { item });
			}
		}
		if (faces.Length != facesToRefresh.Count())
		{
			pb_Face[] array3 = faces;
			foreach (pb_Face pb_Face3 in array3)
			{
				if (!pb_Face3.manualUV && dictionary.ContainsKey(pb_Face3.textureGroup) && !dictionary[pb_Face3.textureGroup].Contains(pb_Face3))
				{
					dictionary[pb_Face3.textureGroup].Add(pb_Face3);
				}
			}
		}
		num = 0;
		Vector3[] verts = ((!flag) ? null : base.transform.ToWorldSpace(vertices));
		foreach (KeyValuePair<int, List<pb_Face>> item2 in dictionary)
		{
			int[] indices = pb_Face.AllTrianglesDistinct(item2.Value).ToArray();
			Vector3 vector = ((item2.Value.Count <= 1) ? pb_Math.Normal(this, item2.Value[0]) : pb_Projection.FindBestPlane(_vertices, indices).normal);
			if (item2.Value[0].uv.useWorldSpace)
			{
				pb_UVUtility.PlanarMap2(verts, uvs, indices, item2.Value[0].uv, base.transform.TransformDirection(vector));
			}
			else
			{
				pb_UVUtility.PlanarMap2(vertices, uvs, indices, item2.Value[0].uv, vector);
			}
			Vector2 localPivot = item2.Value[0].uv.localPivot;
			foreach (pb_Face item3 in item2.Value)
			{
				item3.uv.localPivot = localPivot;
			}
		}
		_uv = uvs;
		msh.uv = uvs;
		if (hasUv3)
		{
			msh.SetUVs(2, uv3);
		}
		if (hasUv4)
		{
			msh.SetUVs(3, uv4);
		}
	}

	public void SetFaceMaterial(pb_Face[] quad, Material mat)
	{
		for (int i = 0; i < quad.Length; i++)
		{
			quad[i].material = mat;
		}
	}

	public void SetUV2(Vector2[] v)
	{
		GetComponent<MeshFilter>().sharedMesh.uv2 = v;
	}

	public void RefreshColors()
	{
		Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		if (_colors == null || _colors.Length != vertexCount)
		{
			_colors = pbUtil.FilledArray(Color.white, vertexCount);
		}
		sharedMesh.colors = _colors;
	}

	public void SetColors(Color[] InColors)
	{
		_colors = ((InColors.Length != vertexCount) ? pbUtil.FilledArray(Color.white, vertexCount) : InColors);
	}

	public void SetFaceColor(pb_Face face, Color color)
	{
		if (_colors == null)
		{
			_colors = pbUtil.FilledArray(Color.white, vertexCount);
		}
		int[] distinctIndices = face.distinctIndices;
		foreach (int num in distinctIndices)
		{
			_colors[num] = color;
		}
	}

	public void SetTangents(Vector4[] tangents)
	{
		_tangents = tangents;
	}

	public void RefreshNormals()
	{
		msh.RecalculateNormals();
		Vector3[] normals = msh.normals;
		pb_MeshUtility.SmoothNormals(this, ref normals);
		GetComponent<MeshFilter>().sharedMesh.normals = normals;
	}

	public void RefreshTangents()
	{
		Mesh InMesh = GetComponent<MeshFilter>().sharedMesh;
		if (_tangents != null && _tangents.Length == vertexCount)
		{
			InMesh.tangents = _tangents;
		}
		else
		{
			pb_MeshUtility.GenerateTangent(ref InMesh);
		}
	}

	public void OnDestroy()
	{
		if (!dontDestroyMeshOnDelete && Application.isEditor && !Application.isPlaying && Time.frameCount > 0)
		{
			if (onDestroyObject__BackingField != null)
			{
				onDestroyObject__BackingField(this);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject.GetComponent<MeshFilter>().sharedMesh, true);
			}
		}
	}
}
