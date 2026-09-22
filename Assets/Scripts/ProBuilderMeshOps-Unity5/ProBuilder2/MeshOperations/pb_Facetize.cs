using System;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	[Obsolete("See pb_MeshTopology")]
	public static class pb_Facetize
	{
		[Obsolete("Use pb_MeshTopology.ToTriangles")]
		public static pb_ActionResult Facetize(this pb_Object pb, IList<pb_Face> faces, out pb_Face[] newFaces)
		{
			return pb.ToTriangles(faces, out newFaces);
		}
	}
}
