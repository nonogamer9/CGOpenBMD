namespace KDTree
{
	public class SquareEuclideanDistanceFunction : DistanceFunctions
	{
		public double Distance(double[] p1, double[] p2)
		{
			double num = 0.0;
			for (int i = 0; i < p1.Length; i++)
			{
				double num2 = p1[i] - p2[i];
				num += num2 * num2;
			}
			return num;
		}

		public double DistanceToRectangle(double[] point, double[] min, double[] max)
		{
			double num = 0.0;
			double num2 = 0.0;
			for (int i = 0; i < point.Length; i++)
			{
				num2 = 0.0;
				if (point[i] > max[i])
				{
					num2 = point[i] - max[i];
				}
				else if (point[i] < min[i])
				{
					num2 = point[i] - min[i];
				}
				num += num2 * num2;
			}
			return num;
		}
	}
}
