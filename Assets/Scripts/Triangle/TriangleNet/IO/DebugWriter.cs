using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace TriangleNet.IO
{
	internal class DebugWriter
	{
		private static NumberFormatInfo nfi;

		private int iteration;

		private string session;

		private StreamWriter stream;

		private string tmpFile;

		private int[] vertices;

		private int triangles;

		private static readonly DebugWriter instance;

		public static DebugWriter Session
		{
			get
			{
				return instance;
			}
		}

		static DebugWriter()
		{
			nfi = CultureInfo.InvariantCulture.NumberFormat;
			instance = new DebugWriter();
		}

		private DebugWriter()
		{
		}

		public void Start(string session)
		{
			iteration = 0;
			this.session = session;
			if (stream != null)
			{
				throw new Exception("A session is active. Finish before starting a new.");
			}
			tmpFile = Path.GetTempFileName();
			stream = new StreamWriter(tmpFile);
		}

		public void Write(Mesh mesh, bool skip)
		{
			WriteMesh(mesh, skip);
			triangles = mesh.Triangles.Count;
		}

		public void Write(Mesh mesh)
		{
			Write(mesh, false);
		}

		public void Finish()
		{
			Finish(session + ".mshx");
		}

		private void Finish(string path)
		{
			if (stream == null)
			{
				return;
			}
			stream.Flush();
			stream.Dispose();
			stream = null;
			string s = "#!N" + iteration + Environment.NewLine;
			using (FileStream compressedStream = new FileStream(path, FileMode.Create))
			{
				using (GZipStream gZipStream = new GZipStream(compressedStream, CompressionMode.Compress, false))
				{
					byte[] bytes = Encoding.UTF8.GetBytes(s);
					gZipStream.Write(bytes, 0, bytes.Length);
					bytes = File.ReadAllBytes(tmpFile);
					gZipStream.Write(bytes, 0, bytes.Length);
				}
			}
			File.Delete(tmpFile);
		}

		private void WriteGeometry(InputGeometry geometry)
		{
			stream.WriteLine("#!G{0}", iteration++);
		}

		private void WriteMesh(Mesh mesh, bool skip)
		{
			if ((triangles == mesh.triangles.Count) & skip)
			{
				return;
			}
			stream.WriteLine("#!M{0}", iteration++);
			if (VerticesChanged(mesh))
			{
				HashVertices(mesh);
				stream.WriteLine("{0}", mesh.vertices.Count);
				foreach (Vertex value in mesh.vertices.Values)
				{
					stream.WriteLine("{0} {1} {2} {3}", value.hash, value.x.ToString(nfi), value.y.ToString(nfi), value.mark);
				}
			}
			else
			{
				stream.WriteLine("0");
			}
			stream.WriteLine("{0}", mesh.subsegs.Count);
			Osub osub = new Osub
			{
				orient = 0
			};
			foreach (Segment value2 in mesh.subsegs.Values)
			{
				if (value2.hash > 0)
				{
					osub.seg = value2;
					Vertex vertex = osub.Org();
					Vertex vertex2 = osub.Dest();
					stream.WriteLine("{0} {1} {2} {3}", osub.seg.hash, vertex.hash, vertex2.hash, osub.seg.boundary);
				}
			}
			Otri otri = default(Otri);
			Otri o = default(Otri);
			otri.orient = 0;
			stream.WriteLine("{0}", mesh.triangles.Count);
			foreach (Triangle value3 in mesh.triangles.Values)
			{
				otri.triangle = value3;
				Vertex vertex = otri.Org();
				Vertex vertex2 = otri.Dest();
				Vertex vertex3 = otri.Apex();
				int num = ((vertex == null) ? (-1) : vertex.hash);
				int num2 = ((vertex2 == null) ? (-1) : vertex2.hash);
				int num3 = ((vertex3 == null) ? (-1) : vertex3.hash);
				stream.Write("{0} {1} {2} {3}", otri.triangle.hash, num, num2, num3);
				otri.orient = 1;
				otri.Sym(ref o);
				int hash = o.triangle.hash;
				otri.orient = 2;
				otri.Sym(ref o);
				int hash2 = o.triangle.hash;
				otri.orient = 0;
				otri.Sym(ref o);
				int hash3 = o.triangle.hash;
				stream.WriteLine(" {0} {1} {2}", hash, hash2, hash3);
			}
		}

		private bool VerticesChanged(Mesh mesh)
		{
			if (vertices == null || mesh.Vertices.Count != vertices.Length)
			{
				return true;
			}
			int num = 0;
			foreach (Vertex vertex in mesh.Vertices)
			{
				if (vertex.id != vertices[num++])
				{
					return true;
				}
			}
			return false;
		}

		private void HashVertices(Mesh mesh)
		{
			if (vertices == null || mesh.Vertices.Count != vertices.Length)
			{
				vertices = new int[mesh.Vertices.Count];
			}
			int num = 0;
			foreach (Vertex vertex in mesh.Vertices)
			{
				vertices[num++] = vertex.id;
			}
		}
	}
}
