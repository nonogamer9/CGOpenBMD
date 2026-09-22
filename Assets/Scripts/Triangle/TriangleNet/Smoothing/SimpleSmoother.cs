using TriangleNet.Data;
using TriangleNet.Geometry;
using TriangleNet.Tools;

namespace TriangleNet.Smoothing
{
	public class SimpleSmoother : ISmoother
	{
		private Mesh mesh;

		public SimpleSmoother(Mesh mesh)
		{
			this.mesh = mesh;
		}

		public void Smooth()
		{
			mesh.behavior.Quality = false;
			for (int i = 0; i < 5; i++)
			{
				Step();
				mesh.Triangulate(Rebuild());
			}
		}

		private void Step()
		{
			foreach (VoronoiRegion region in new BoundedVoronoi(mesh, false).Regions)
			{
				int num = 0;
				double num3;
				double num2 = (num3 = 0.0);
				foreach (Point vertex in region.Vertices)
				{
					num++;
					num2 += vertex.x;
					num3 += vertex.y;
				}
				region.Generator.x = num2 / (double)num;
				region.Generator.y = num3 / (double)num;
			}
		}

		private InputGeometry Rebuild()
		{
			InputGeometry inputGeometry = new InputGeometry(mesh.vertices.Count);
			foreach (Vertex value in mesh.vertices.Values)
			{
				inputGeometry.AddPoint(value.x, value.y, value.mark);
			}
			foreach (Segment value2 in mesh.subsegs.Values)
			{
				inputGeometry.AddSegment(value2.P0, value2.P1, value2.Boundary);
			}
			foreach (Point hole in mesh.holes)
			{
				inputGeometry.AddHole(hole.x, hole.y);
			}
			foreach (RegionPointer region in mesh.regions)
			{
				inputGeometry.AddRegion(region.point.x, region.point.y, region.id);
			}
			return inputGeometry;
		}
	}
}
