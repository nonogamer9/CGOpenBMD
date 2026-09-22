using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Parabox.STL
{
	public static class pb_Stl
	{
		public static bool WriteFile(string path, Mesh mesh, FileType type = FileType.Ascii, bool convertToRightHandedCoordinates = true)
		{
			return WriteFile(path, new Mesh[1] { mesh }, type, convertToRightHandedCoordinates);
		}

		public static bool WriteFile(string path, IList<Mesh> meshes, FileType type = FileType.Ascii, bool convertToRightHandedCoordinates = true)
		{
			try
			{
				if (type == FileType.Binary)
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create), new ASCIIEncoding()))
					{
						binaryWriter.Write(new byte[80]);
						uint value = (uint)(meshes.Sum((Mesh x) => x.triangles.Length) / 3);
						binaryWriter.Write(value);
						foreach (Mesh mesh in meshes)
						{
							Vector3[] array = (convertToRightHandedCoordinates ? Left2Right(mesh.vertices) : mesh.vertices);
							Vector3[] array2 = (convertToRightHandedCoordinates ? Left2Right(mesh.normals) : mesh.normals);
							int[] triangles = mesh.triangles;
							int num = triangles.Length;
							if (convertToRightHandedCoordinates)
							{
								Array.Reverse(triangles);
							}
							for (int num2 = 0; num2 < num; num2 += 3)
							{
								int num3 = triangles[num2];
								int num4 = triangles[num2 + 1];
								int num5 = triangles[num2 + 2];
								Vector3 vector = AvgNrm(array2[num3], array2[num4], array2[num5]);
								binaryWriter.Write(vector.x);
								binaryWriter.Write(vector.y);
								binaryWriter.Write(vector.z);
								binaryWriter.Write(array[num3].x);
								binaryWriter.Write(array[num3].y);
								binaryWriter.Write(array[num3].z);
								binaryWriter.Write(array[num4].x);
								binaryWriter.Write(array[num4].y);
								binaryWriter.Write(array[num4].z);
								binaryWriter.Write(array[num5].x);
								binaryWriter.Write(array[num5].y);
								binaryWriter.Write(array[num5].z);
								binaryWriter.Write((ushort)0);
							}
						}
					}
				}
				else
				{
					string contents = WriteString(meshes);
					File.WriteAllText(path, contents);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
				return false;
			}
			return true;
		}

		public static string WriteString(Mesh mesh, bool convertToRightHandedCoordinates = true)
		{
			return WriteString(new Mesh[1] { mesh }, convertToRightHandedCoordinates);
		}

		public static string WriteString(IList<Mesh> meshes, bool convertToRightHandedCoordinates = true)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string arg = ((meshes.Count == 1) ? meshes[0].name : "Composite Mesh");
			stringBuilder.AppendLine(string.Format("solid {0}", arg));
			foreach (Mesh mesh in meshes)
			{
				Vector3[] array = (convertToRightHandedCoordinates ? Left2Right(mesh.vertices) : mesh.vertices);
				Vector3[] array2 = (convertToRightHandedCoordinates ? Left2Right(mesh.normals) : mesh.normals);
				int[] triangles = mesh.triangles;
				if (convertToRightHandedCoordinates)
				{
					Array.Reverse(triangles);
				}
				int num = triangles.Length;
				for (int i = 0; i < num; i += 3)
				{
					int num2 = triangles[i];
					int num3 = triangles[i + 1];
					int num4 = triangles[i + 2];
					Vector3 vector = AvgNrm(array2[num2], array2[num3], array2[num4]);
					stringBuilder.AppendLine(string.Format("facet normal {0} {1} {2}", vector.x, vector.y, vector.z));
					stringBuilder.AppendLine("outer loop");
					stringBuilder.AppendLine(string.Format("\tvertex {0} {1} {2}", array[num2].x, array[num2].y, array[num2].z));
					stringBuilder.AppendLine(string.Format("\tvertex {0} {1} {2}", array[num3].x, array[num3].y, array[num3].z));
					stringBuilder.AppendLine(string.Format("\tvertex {0} {1} {2}", array[num4].x, array[num4].y, array[num4].z));
					stringBuilder.AppendLine("endloop");
					stringBuilder.AppendLine("endfacet");
				}
			}
			stringBuilder.AppendLine(string.Format("endsolid {0}", arg));
			return stringBuilder.ToString();
		}

		private static Vector3[] Left2Right(Vector3[] v)
		{
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, 1f, -1f));
			Vector3[] array = new Vector3[v.Length];
			for (int i = 0; i < v.Length; i++)
			{
				array[i] = matrix4x.MultiplyPoint3x4(v[i]);
			}
			return array;
		}

		private static Vector3 AvgNrm(Vector3 a, Vector3 b, Vector3 c)
		{
			return new Vector3((a.x + b.x + c.x) / 3f, (a.y + b.y + c.y) / 3f, (a.z + b.z + c.z) / 3f);
		}
	}
}
