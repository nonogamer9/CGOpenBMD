using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProBuilder2.Common
{
	public static class pb_SelectionPicker
	{
		private static bool m_Initialized = false;

		private static RenderTextureFormat m_RenderTextureFormat = RenderTextureFormat.Default;

		private static RenderTextureFormat[] m_PreferredFormats = new RenderTextureFormat[2]
		{
			RenderTextureFormat.ARGB32,
			RenderTextureFormat.ARGBFloat
		};

		private static RenderTextureFormat renderTextureFormat
		{
			get
			{
				if (m_Initialized)
				{
					return m_RenderTextureFormat;
				}
				m_Initialized = true;
				for (int i = 0; i < m_PreferredFormats.Length; i++)
				{
					if (SystemInfo.SupportsRenderTextureFormat(m_PreferredFormats[i]))
					{
						m_RenderTextureFormat = m_PreferredFormats[i];
						break;
					}
				}
				return m_RenderTextureFormat;
			}
		}

		private static TextureFormat textureFormat
		{
			get
			{
				return TextureFormat.ARGB32;
			}
		}

		public static Dictionary<pb_Object, HashSet<pb_Face>> PickFacesInRect(Camera camera, Rect pickerRect, IEnumerable<pb_Object> selection, int renderTextureWidth = -1, int renderTextureHeight = -1)
		{
			Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map;
			Texture2D texture2D = RenderSelectionPickerTexture(camera, selection, out map, renderTextureWidth, renderTextureHeight);
			Color32[] pixels = texture2D.GetPixels32();
			int num = Math.Max(0, Mathf.FloorToInt(pickerRect.x));
			int num2 = Math.Max(0, Mathf.FloorToInt((float)texture2D.height - pickerRect.y - pickerRect.height));
			int width = texture2D.width;
			int height = texture2D.height;
			int num3 = Mathf.FloorToInt(pickerRect.width);
			int num4 = Mathf.FloorToInt(pickerRect.height);
			UnityEngine.Object.DestroyImmediate(texture2D);
			Dictionary<pb_Object, HashSet<pb_Face>> dictionary = new Dictionary<pb_Object, HashSet<pb_Face>>();
			HashSet<pb_Face> value = null;
			HashSet<uint> hashSet = new HashSet<uint>();
			for (int i = num2; i < Math.Min(num2 + num4, height); i++)
			{
				for (int j = num; j < Math.Min(num + num3, width); j++)
				{
					uint num5 = DecodeRGBA(pixels[i * width + j]);
					pb_Tuple<pb_Object, pb_Face> value2;
					if (hashSet.Add(num5) && map.TryGetValue(num5, out value2))
					{
						if (dictionary.TryGetValue(value2.Item1, out value))
						{
							value.Add(value2.Item2);
							continue;
						}
						dictionary.Add(value2.Item1, new HashSet<pb_Face> { value2.Item2 });
					}
				}
			}
			return dictionary;
		}

		public static Dictionary<pb_Object, HashSet<int>> PickVerticesInRect(Camera camera, Rect pickerRect, IEnumerable<pb_Object> selection, int renderTextureWidth = -1, int renderTextureHeight = -1)
		{
			Dictionary<pb_Object, HashSet<int>> dictionary = new Dictionary<pb_Object, HashSet<int>>();
			Dictionary<uint, pb_Tuple<pb_Object, int>> map;
			Texture2D texture2D = RenderSelectionPickerTexture(camera, selection, out map, renderTextureWidth, renderTextureHeight);
			Color32[] pixels = texture2D.GetPixels32();
			int num = Math.Max(0, Mathf.FloorToInt(pickerRect.x));
			int num2 = Math.Max(0, Mathf.FloorToInt((float)texture2D.height - pickerRect.y - pickerRect.height));
			int width = texture2D.width;
			int height = texture2D.height;
			int num3 = Mathf.FloorToInt(pickerRect.width);
			int num4 = Mathf.FloorToInt(pickerRect.height);
			UnityEngine.Object.DestroyImmediate(texture2D);
			HashSet<int> value = null;
			HashSet<uint> hashSet = new HashSet<uint>();
			for (int i = num2; i < Math.Min(num2 + num4, height); i++)
			{
				for (int j = num; j < Math.Min(num + num3, width); j++)
				{
					uint num5 = DecodeRGBA(pixels[i * width + j]);
					pb_Tuple<pb_Object, int> value2;
					if (hashSet.Add(num5) && map.TryGetValue(num5, out value2))
					{
						if (dictionary.TryGetValue(value2.Item1, out value))
						{
							value.Add(value2.Item2);
							continue;
						}
						dictionary.Add(value2.Item1, new HashSet<int> { value2.Item2 });
					}
				}
			}
			return dictionary;
		}

		public static Texture2D RenderSelectionPickerTexture(Camera camera, IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map, int width = -1, int height = -1)
		{
			List<GameObject> list = GenerateFaceDepthTestMeshes(selection, out map);
			Texture2D result = RenderWithReplacementShader(camera, pb_Constant.SelectionPickerShader, "ProBuilderPicker", width, height);
			foreach (GameObject item in list)
			{
				UnityEngine.Object.DestroyImmediate(item.GetComponent<MeshFilter>().sharedMesh);
				UnityEngine.Object.DestroyImmediate(item);
			}
			return result;
		}

		public static Texture2D RenderSelectionPickerTexture(Camera camera, IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, int>> map, int width = -1, int height = -1)
		{
			List<GameObject> list = GenerateVertexDepthTestMeshes(selection, out map);
			Texture2D result = RenderWithReplacementShader(camera, pb_Constant.SelectionPickerShader, "ProBuilderPicker", width, height);
			foreach (GameObject item in list)
			{
				UnityEngine.Object.DestroyImmediate(item.GetComponent<MeshFilter>().sharedMesh);
				UnityEngine.Object.DestroyImmediate(item);
			}
			return result;
		}

		public static List<GameObject> GenerateFaceDepthTestMeshes(IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, pb_Face>> map)
		{
			List<GameObject> list = new List<GameObject>();
			map = new Dictionary<uint, pb_Tuple<pb_Object, pb_Face>>();
			uint num = 0u;
			foreach (pb_Object item in selection)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = item.name + " (Face Depth Test)";
				gameObject.transform.position = item.transform.position;
				gameObject.transform.localRotation = item.transform.localRotation;
				gameObject.transform.localScale = item.transform.localScale;
				Mesh mesh = new Mesh();
				mesh.vertices = item.vertices;
				mesh.triangles = item.faces.SelectMany((pb_Face x) => x.indices).ToArray();
				Color32[] array = new Color32[mesh.vertexCount];
				pb_Face[] faces = item.faces;
				foreach (pb_Face pb_Face2 in faces)
				{
					Color32 color = EncodeRGBA(num++);
					map.Add(DecodeRGBA(color), new pb_Tuple<pb_Object, pb_Face>(item, pb_Face2));
					for (int num3 = 0; num3 < pb_Face2.distinctIndices.Length; num3++)
					{
						array[pb_Face2.distinctIndices[num3]] = color;
					}
				}
				mesh.colors32 = array;
				try
				{
					gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
					gameObject.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.FacePickerMaterial;
				}
				catch
				{
					Debug.LogWarning("Could not find shader `pb_FacePicker.shader`.  Please re-import ProBuilder to fix!");
				}
				list.Add(gameObject);
			}
			return list;
		}

		private static List<GameObject> GenerateVertexDepthTestMeshes(IEnumerable<pb_Object> selection, out Dictionary<uint, pb_Tuple<pb_Object, int>> map)
		{
			List<GameObject> list = new List<GameObject>();
			map = new Dictionary<uint, pb_Tuple<pb_Object, int>>();
			Color32 val = new Color32(0, 0, 0, byte.MaxValue);
			uint index = 2u;
			foreach (pb_Object item in selection)
			{
				GameObject gameObject = pbUtil.EmptyGameObjectWithTransform(item.transform);
				gameObject.name = item.name + "  (Depth Mask)";
				Mesh mesh = new Mesh();
				mesh.vertices = item.vertices;
				mesh.triangles = item.faces.SelectMany((pb_Face x) => x.indices).ToArray();
				mesh.colors32 = pbUtil.Fill(val, item.vertexCount);
				gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
				gameObject.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.FacePickerMaterial;
				list.Add(gameObject);
				GameObject gameObject2 = pbUtil.EmptyGameObjectWithTransform(item.transform);
				gameObject2.name = item.name + "  (Vertex Billboards)";
				gameObject2.AddComponent<MeshFilter>().sharedMesh = BuildVertexMesh(item, map, ref index);
				gameObject2.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.VertexPickerMaterial;
				list.Add(gameObject2);
			}
			return list;
		}

		private static Mesh BuildVertexMesh(pb_Object pb, Dictionary<uint, pb_Tuple<pb_Object, int>> map, ref uint index)
		{
			int num = Math.Min(pb.sharedIndices.Length, 16382);
			Vector3[] array = new Vector3[num * 4];
			Vector2[] array2 = new Vector2[num * 4];
			Vector2[] array3 = new Vector2[num * 4];
			Color[] array4 = new Color[num * 4];
			int[] array5 = new int[num * 6];
			int num2 = 0;
			int num3 = 0;
			Vector3 up = Vector3.up;
			Vector3 right = Vector3.right;
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = pb.vertices[pb.sharedIndices[i][0]];
				array[num3] = vector;
				array[num3 + 1] = vector;
				array[num3 + 2] = vector;
				array[num3 + 3] = vector;
				array2[num3] = Vector3.zero;
				array2[num3 + 1] = Vector3.right;
				array2[num3 + 2] = Vector3.up;
				array2[num3 + 3] = Vector3.one;
				array3[num3] = -up - right;
				array3[num3 + 1] = -up + right;
				array3[num3 + 2] = up - right;
				array3[num3 + 3] = up + right;
				array5[num2] = num3;
				array5[num2 + 1] = num3 + 1;
				array5[num2 + 2] = num3 + 2;
				array5[num2 + 3] = num3 + 1;
				array5[num2 + 4] = num3 + 3;
				array5[num2 + 5] = num3 + 2;
				Color32 color = EncodeRGBA(index);
				map.Add(index++, new pb_Tuple<pb_Object, int>(pb, i));
				array4[num3] = color;
				array4[num3 + 1] = color;
				array4[num3 + 2] = color;
				array4[num3 + 3] = color;
				num3 += 4;
				num2 += 6;
			}
			Mesh mesh = new Mesh();
			mesh.name = "Vertex Billboard";
			mesh.vertices = array;
			mesh.uv = array2;
			mesh.uv2 = array3;
			mesh.colors = array4;
			mesh.triangles = array5;
			return mesh;
		}

		public static uint DecodeRGBA(Color32 color)
		{
			uint r = color.r;
			uint g = color.g;
			uint b = color.b;
			if (BitConverter.IsLittleEndian)
			{
				return (r << 16) | (g << 8) | b;
			}
			return (r << 24) | (g << 16) | (b << 8);
		}

		public static Color32 EncodeRGBA(uint hash)
		{
			if (BitConverter.IsLittleEndian)
			{
				return new Color32((byte)((hash >> 16) & 0xFF), (byte)((hash >> 8) & 0xFF), (byte)(hash & 0xFF), byte.MaxValue);
			}
			return new Color32((byte)((hash >> 24) & 0xFF), (byte)((hash >> 16) & 0xFF), (byte)((hash >> 8) & 0xFF), byte.MaxValue);
		}

		private static Texture2D RenderWithReplacementShader(Camera camera, Shader shader, string tag, int width = -1, int height = -1)
		{
			bool flag = width < 0 || height < 0;
			int num = ((!flag) ? width : ((int)camera.pixelRect.width));
			int num2 = ((!flag) ? height : ((int)camera.pixelRect.height));
			GameObject gameObject = new GameObject();
			Camera camera2 = gameObject.AddComponent<Camera>();
			camera2.CopyFrom(camera);
			camera2.renderingPath = RenderingPath.Forward;
			camera2.enabled = false;
			camera2.clearFlags = CameraClearFlags.Color;
			camera2.backgroundColor = Color.white;
			camera2.allowHDR = false;
			camera2.allowMSAA = false;
			camera2.forceIntoRenderTexture = true;
			RenderTextureDescriptor desc = new RenderTextureDescriptor
			{
				width = num,
				height = num2,
				colorFormat = renderTextureFormat,
				autoGenerateMips = false,
				depthBufferBits = 16,
				dimension = TextureDimension.Tex2D,
				enableRandomWrite = false,
				memoryless = RenderTextureMemoryless.None,
				sRGB = false,
				useMipMap = false,
				volumeDepth = 1,
				msaaSamples = 1
			};
			RenderTexture temporary = RenderTexture.GetTemporary(desc);
			RenderTexture active = RenderTexture.active;
			camera2.targetTexture = temporary;
			RenderTexture.active = temporary;
			camera2.RenderWithShader(shader, tag);
			Texture2D texture2D = new Texture2D(num, num2, textureFormat, false, false);
			texture2D.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			UnityEngine.Object.DestroyImmediate(gameObject);
			return texture2D;
		}
	}
}
