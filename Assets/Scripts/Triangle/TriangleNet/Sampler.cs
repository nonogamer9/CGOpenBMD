using System;
using System.Collections.Generic;
using System.Linq;

namespace TriangleNet
{
	internal class Sampler
	{
		private static Random rand = new Random(DateTime.Now.Millisecond);

		private int samples = 1;

		private int triangleCount;

		private static int samplefactor = 11;

		private int[] keys;

		public void Reset()
		{
			samples = 1;
			triangleCount = 0;
		}

		public void Update(Mesh mesh)
		{
			Update(mesh, false);
		}

		public void Update(Mesh mesh, bool forceUpdate)
		{
			int count = mesh.triangles.Count;
			if ((triangleCount != count) | forceUpdate)
			{
				triangleCount = count;
				while (samplefactor * samples * samples * samples < count)
				{
					samples++;
				}
				keys = mesh.triangles.Keys.ToArray();
			}
		}

		public int[] GetSamples(Mesh mesh)
		{
			List<int> list = new List<int>(samples);
			int num = triangleCount / samples;
			for (int i = 0; i < samples; i++)
			{
				int num2 = rand.Next(i * num, (i + 1) * num - 1);
				if (!mesh.triangles.Keys.Contains(keys[num2]))
				{
					Update(mesh, true);
					i--;
				}
				else
				{
					list.Add(keys[num2]);
				}
			}
			return list.ToArray();
		}
	}
}
