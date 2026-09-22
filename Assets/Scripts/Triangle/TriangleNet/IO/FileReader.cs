using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TriangleNet.Geometry;
using TriangleNet.Log;

namespace TriangleNet.IO
{
	public static class FileReader
	{
		private static NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;

		private static int startIndex = 0;

		private static bool TryReadLine(StreamReader reader, out string[] token)
		{
			token = null;
			if (reader.EndOfStream)
			{
				return false;
			}
			string text = reader.ReadLine().Trim();
			while (string.IsNullOrEmpty(text.Trim()) || text.StartsWith("#"))
			{
				if (reader.EndOfStream)
				{
					return false;
				}
				text = reader.ReadLine().Trim();
			}
			token = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			return true;
		}

		private static void ReadVertex(InputGeometry data, int index, string[] line, int attributes, int marks)
		{
			double x = double.Parse(line[1], nfi);
			double y = double.Parse(line[2], nfi);
			int boundary = 0;
			double[] array = ((attributes == 0) ? null : new double[attributes]);
			for (int i = 0; i < attributes; i++)
			{
				if (line.Length > 3 + i)
				{
					array[i] = double.Parse(line[3 + i], nfi);
				}
			}
			if (marks > 0 && line.Length > 3 + attributes)
			{
				boundary = int.Parse(line[3 + attributes]);
			}
			data.AddPoint(x, y, boundary, array);
		}

		public static void Read(string filename, out InputGeometry geometry)
		{
			geometry = null;
			string text = Path.ChangeExtension(filename, ".poly");
			if (File.Exists(text))
			{
				geometry = ReadPolyFile(text);
				return;
			}
			text = Path.ChangeExtension(filename, ".node");
			geometry = ReadNodeFile(text);
		}

		public static void Read(string filename, out InputGeometry geometry, out List<ITriangle> triangles)
		{
			triangles = null;
			Read(filename, out geometry);
			string text = Path.ChangeExtension(filename, ".ele");
			if (File.Exists(text) && geometry != null)
			{
				triangles = ReadEleFile(text);
			}
		}

		public static InputGeometry Read(string filename)
		{
			InputGeometry geometry = null;
			Read(filename, out geometry);
			return geometry;
		}

		public static InputGeometry ReadNodeFile(string nodefilename)
		{
			return ReadNodeFile(nodefilename, false);
		}

		public static InputGeometry ReadNodeFile(string nodefilename, bool readElements)
		{
			startIndex = 0;
			int num = 0;
			int attributes = 0;
			int marks = 0;
			InputGeometry inputGeometry;
			using (StreamReader reader = new StreamReader(nodefilename))
			{
				string[] token;
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file.");
				}
				num = int.Parse(token[0]);
				if (num < 3)
				{
					throw new Exception("Input must have at least three input vertices.");
				}
				if (token.Length > 1 && int.Parse(token[1]) != 2)
				{
					throw new Exception("Triangle only works with two-dimensional meshes.");
				}
				if (token.Length > 2)
				{
					attributes = int.Parse(token[2]);
				}
				if (token.Length > 3)
				{
					marks = int.Parse(token[3]);
				}
				inputGeometry = new InputGeometry(num);
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						if (!TryReadLine(reader, out token))
						{
							throw new Exception("Can't read input file (vertices).");
						}
						if (token.Length < 3)
						{
							throw new Exception("Invalid vertex.");
						}
						if (i == 0)
						{
							startIndex = int.Parse(token[0], nfi);
						}
						ReadVertex(inputGeometry, i, token, attributes, marks);
					}
				}
			}
			if (readElements)
			{
				string text = Path.ChangeExtension(nodefilename, ".ele");
				if (File.Exists(text))
				{
					ReadEleFile(text, true);
				}
			}
			return inputGeometry;
		}

		public static InputGeometry ReadPolyFile(string polyfilename)
		{
			return ReadPolyFile(polyfilename, false, false);
		}

		public static InputGeometry ReadPolyFile(string polyfilename, bool readElements)
		{
			return ReadPolyFile(polyfilename, readElements, false);
		}

		public static InputGeometry ReadPolyFile(string polyfilename, bool readElements, bool readArea)
		{
			startIndex = 0;
			int num = 0;
			int attributes = 0;
			int marks = 0;
			InputGeometry inputGeometry;
			using (StreamReader reader = new StreamReader(polyfilename))
			{
				string[] token;
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file.");
				}
				num = int.Parse(token[0]);
				if (token.Length > 1 && int.Parse(token[1]) != 2)
				{
					throw new Exception("Triangle only works with two-dimensional meshes.");
				}
				if (token.Length > 2)
				{
					attributes = int.Parse(token[2]);
				}
				if (token.Length > 3)
				{
					marks = int.Parse(token[3]);
				}
				if (num > 0)
				{
					inputGeometry = new InputGeometry(num);
					for (int i = 0; i < num; i++)
					{
						if (!TryReadLine(reader, out token))
						{
							throw new Exception("Can't read input file (vertices).");
						}
						if (token.Length < 3)
						{
							throw new Exception("Invalid vertex.");
						}
						if (i == 0)
						{
							startIndex = int.Parse(token[0], nfi);
						}
						ReadVertex(inputGeometry, i, token, attributes, marks);
					}
				}
				else
				{
					inputGeometry = ReadNodeFile(Path.ChangeExtension(polyfilename, ".node"));
					num = inputGeometry.Count;
				}
				if (inputGeometry.Points == null)
				{
					throw new Exception("No nodes available.");
				}
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file (segments).");
				}
				int num2 = int.Parse(token[0]);
				int num3 = 0;
				if (token.Length > 1)
				{
					num3 = int.Parse(token[1]);
				}
				for (int j = 0; j < num2; j++)
				{
					if (!TryReadLine(reader, out token))
					{
						throw new Exception("Can't read input file (segments).");
					}
					if (token.Length < 3)
					{
						throw new Exception("Segment has no endpoints.");
					}
					int num4 = int.Parse(token[1]) - startIndex;
					int num5 = int.Parse(token[2]) - startIndex;
					int boundary = 0;
					if (num3 > 0 && token.Length > 3)
					{
						boundary = int.Parse(token[3]);
					}
					if (num4 < 0 || num4 >= num)
					{
						if (Behavior.Verbose)
						{
							SimpleLog.Instance.Warning("Invalid first endpoint of segment.", "MeshReader.ReadPolyfile()");
						}
					}
					else if (num5 < 0 || num5 >= num)
					{
						if (Behavior.Verbose)
						{
							SimpleLog.Instance.Warning("Invalid second endpoint of segment.", "MeshReader.ReadPolyfile()");
						}
					}
					else
					{
						inputGeometry.AddSegment(num4, num5, boundary);
					}
				}
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file (holes).");
				}
				int num6 = int.Parse(token[0]);
				if (num6 > 0)
				{
					for (int k = 0; k < num6; k++)
					{
						if (!TryReadLine(reader, out token))
						{
							throw new Exception("Can't read input file (holes).");
						}
						if (token.Length < 3)
						{
							throw new Exception("Invalid hole.");
						}
						inputGeometry.AddHole(double.Parse(token[1], nfi), double.Parse(token[2], nfi));
					}
				}
				if (TryReadLine(reader, out token))
				{
					int num7 = int.Parse(token[0]);
					if (num7 > 0)
					{
						for (int l = 0; l < num7; l++)
						{
							if (!TryReadLine(reader, out token))
							{
								throw new Exception("Can't read input file (region).");
							}
							if (token.Length < 4)
							{
								throw new Exception("Invalid region attributes.");
							}
							inputGeometry.AddRegion(double.Parse(token[1], nfi), double.Parse(token[2], nfi), int.Parse(token[3]));
						}
					}
				}
			}
			if (readElements)
			{
				string text = Path.ChangeExtension(polyfilename, ".ele");
				if (File.Exists(text))
				{
					ReadEleFile(text, readArea);
				}
			}
			return inputGeometry;
		}

		public static List<ITriangle> ReadEleFile(string elefilename)
		{
			return ReadEleFile(elefilename, false);
		}

		private static List<ITriangle> ReadEleFile(string elefilename, bool readArea)
		{
			int num = 0;
			int num2 = 0;
			List<ITriangle> list;
			using (StreamReader reader = new StreamReader(elefilename))
			{
				bool flag = false;
				string[] token;
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file (elements).");
				}
				num = int.Parse(token[0]);
				num2 = 0;
				if (token.Length > 2)
				{
					num2 = int.Parse(token[2]);
					flag = true;
				}
				if (num2 > 1)
				{
					SimpleLog.Instance.Warning("Triangle attributes not supported.", "FileReader.Read");
				}
				list = new List<ITriangle>(num);
				for (int i = 0; i < num; i++)
				{
					if (!TryReadLine(reader, out token))
					{
						throw new Exception("Can't read input file (elements).");
					}
					if (token.Length < 4)
					{
						throw new Exception("Triangle has no nodes.");
					}
					InputTriangle inputTriangle = new InputTriangle(int.Parse(token[1]) - startIndex, int.Parse(token[2]) - startIndex, int.Parse(token[3]) - startIndex);
					if ((num2 > 0) & flag)
					{
						int result = 0;
						flag = int.TryParse(token[4], out result);
						inputTriangle.region = result;
					}
					list.Add(inputTriangle);
				}
			}
			if (readArea)
			{
				string text = Path.ChangeExtension(elefilename, ".area");
				if (File.Exists(text))
				{
					ReadAreaFile(text, num);
				}
			}
			return list;
		}

		private static double[] ReadAreaFile(string areafilename, int intriangles)
		{
			double[] array = null;
			using (StreamReader reader = new StreamReader(areafilename))
			{
				string[] token;
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file (area).");
				}
				if (int.Parse(token[0]) != intriangles)
				{
					SimpleLog.Instance.Warning("Number of area constraints doesn't match number of triangles.", "ReadAreaFile()");
					return null;
				}
				array = new double[intriangles];
				for (int i = 0; i < intriangles; i++)
				{
					if (!TryReadLine(reader, out token))
					{
						throw new Exception("Can't read input file (area).");
					}
					if (token.Length != 2)
					{
						throw new Exception("Triangle has no nodes.");
					}
					array[i] = double.Parse(token[1], nfi);
				}
				return array;
			}
		}

		public static List<Edge> ReadEdgeFile(string edgeFile, int invertices)
		{
			List<Edge> list = null;
			startIndex = 0;
			using (StreamReader reader = new StreamReader(edgeFile))
			{
				string[] token;
				if (!TryReadLine(reader, out token))
				{
					throw new Exception("Can't read input file (segments).");
				}
				int num = int.Parse(token[0]);
				int num2 = 0;
				if (token.Length > 1)
				{
					num2 = int.Parse(token[1]);
				}
				if (num > 0)
				{
					list = new List<Edge>(num);
				}
				for (int i = 0; i < num; i++)
				{
					if (!TryReadLine(reader, out token))
					{
						throw new Exception("Can't read input file (segments).");
					}
					if (token.Length < 3)
					{
						throw new Exception("Segment has no endpoints.");
					}
					int num3 = int.Parse(token[1]) - startIndex;
					int num4 = int.Parse(token[2]) - startIndex;
					int boundary = 0;
					if (num2 > 0 && token.Length > 3)
					{
						boundary = int.Parse(token[3]);
					}
					if (num3 < 0 || num3 >= invertices)
					{
						if (Behavior.Verbose)
						{
							SimpleLog.Instance.Warning("Invalid first endpoint of segment.", "MeshReader.ReadPolyfile()");
						}
					}
					else if (num4 < 0 || num4 >= invertices)
					{
						if (Behavior.Verbose)
						{
							SimpleLog.Instance.Warning("Invalid second endpoint of segment.", "MeshReader.ReadPolyfile()");
						}
					}
					else
					{
						list.Add(new Edge(num3, num4, boundary));
					}
				}
				return list;
			}
		}
	}
}
