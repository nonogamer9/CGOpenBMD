using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	internal class ConnectFaceRebuildData
	{
		public pb_FaceRebuildData faceRebuildData;

		public List<int> newVertexIndices;

		public ConnectFaceRebuildData(pb_FaceRebuildData faceRebuildData, List<int> newVertexIndices)
		{
			this.faceRebuildData = faceRebuildData;
			this.newVertexIndices = newVertexIndices;
		}
	}
}
