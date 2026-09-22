using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	public static class pb_MeshTopology
	{
		public static bool ToQuads(pb_Object target)
		{
			return false;
		}

		public static pb_ActionResult ToTriangles(this pb_Object pb, IList<pb_Face> faces, out pb_Face[] newFaces)
		{
			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			List<pb_FaceRebuildData> list = new List<pb_FaceRebuildData>();
			foreach (pb_Face face in faces)
			{
				List<pb_FaceRebuildData> collection = BreakFaceIntoTris(face, vertices, lookup);
				list.AddRange(collection);
			}
			pb_FaceRebuildData.Apply(list, pb, vertices, null, lookup);
			pb.DeleteFaces(faces);
			pb.ToMesh();
			newFaces = list.Select((pb_FaceRebuildData x) => x.face).ToArray();
			return new pb_ActionResult(Status.Success, string.Format("Triangulated {0} {1}", faces.Count, (faces.Count >= 2) ? "Faces" : "Face"));
		}

		private static List<pb_FaceRebuildData> BreakFaceIntoTris(pb_Face face, List<pb_Vertex> vertices, Dictionary<int, int> lookup)
		{
			int[] indices = face.indices;
			int num = indices.Length;
			List<pb_FaceRebuildData> list = new List<pb_FaceRebuildData>(num / 3);
			for (int i = 0; i < num; i += 3)
			{
				pb_FaceRebuildData pb_FaceRebuildData2 = new pb_FaceRebuildData();
				pb_FaceRebuildData2.face = new pb_Face(face);
				pb_FaceRebuildData2.face.SetIndices(new int[3] { 0, 1, 2 });
				pb_FaceRebuildData2.vertices = new List<pb_Vertex>
				{
					vertices[indices[i]],
					vertices[indices[i + 1]],
					vertices[indices[i + 2]]
				};
				pb_FaceRebuildData2.sharedIndices = new List<int>
				{
					lookup[indices[i]],
					lookup[indices[i + 1]],
					lookup[indices[i + 2]]
				};
				list.Add(pb_FaceRebuildData2);
			}
			return list;
		}
	}
}
