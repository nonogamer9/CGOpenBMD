using System;
using TriangleNet.Geometry;
using TriangleNet.Tools;

namespace TriangleNet
{
	public static class Primitives
	{
		private static double epsilon;

		private static double splitter;

		private static double resulterrbound;

		private static double ccwerrboundA;

		private static double ccwerrboundB;

		private static double ccwerrboundC;

		private static double iccerrboundA;

		private static double iccerrboundB;

		private static double iccerrboundC;

		private static double o3derrboundA;

		private static double o3derrboundB;

		private static double o3derrboundC;

		public static void ExactInit()
		{
			bool flag = true;
			double num = 0.5;
			epsilon = 1.0;
			splitter = 1.0;
			double num2 = 1.0;
			double num3;
			do
			{
				num3 = num2;
				epsilon *= num;
				if (flag)
				{
					splitter *= 2.0;
				}
				flag = !flag;
				num2 = 1.0 + epsilon;
			}
			while (num2 != 1.0 && num2 != num3);
			splitter++;
			resulterrbound = (3.0 + 8.0 * epsilon) * epsilon;
			ccwerrboundA = (3.0 + 16.0 * epsilon) * epsilon;
			ccwerrboundB = (2.0 + 12.0 * epsilon) * epsilon;
			ccwerrboundC = (9.0 + 64.0 * epsilon) * epsilon * epsilon;
			iccerrboundA = (10.0 + 96.0 * epsilon) * epsilon;
			iccerrboundB = (4.0 + 48.0 * epsilon) * epsilon;
			iccerrboundC = (44.0 + 576.0 * epsilon) * epsilon * epsilon;
			o3derrboundA = (7.0 + 56.0 * epsilon) * epsilon;
			o3derrboundB = (3.0 + 28.0 * epsilon) * epsilon;
			o3derrboundC = (26.0 + 288.0 * epsilon) * epsilon * epsilon;
		}

		public static double CounterClockwise(Point pa, Point pb, Point pc)
		{
			Statistic.CounterClockwiseCount++;
			double num = (pa.x - pc.x) * (pb.y - pc.y);
			double num2 = (pa.y - pc.y) * (pb.x - pc.x);
			double num3 = num - num2;
			if (Behavior.NoExact)
			{
				return num3;
			}
			double num4;
			if (num > 0.0)
			{
				if (num2 <= 0.0)
				{
					return num3;
				}
				num4 = num + num2;
			}
			else
			{
				if (!(num < 0.0))
				{
					return num3;
				}
				if (num2 >= 0.0)
				{
					return num3;
				}
				num4 = 0.0 - num - num2;
			}
			double num5 = ccwerrboundA * num4;
			if (num3 >= num5 || 0.0 - num3 >= num5)
			{
				return num3;
			}
			Statistic.CounterClockwiseAdaptCount++;
			return CounterClockwiseAdapt(pa, pb, pc, num4);
		}

		public static double InCircle(Point pa, Point pb, Point pc, Point pd)
		{
			Statistic.InCircleCount++;
			double num = pa.x - pd.x;
			double num2 = pb.x - pd.x;
			double num3 = pc.x - pd.x;
			double num4 = pa.y - pd.y;
			double num5 = pb.y - pd.y;
			double num6 = pc.y - pd.y;
			double num7 = num2 * num6;
			double num8 = num3 * num5;
			double num9 = num * num + num4 * num4;
			double num10 = num3 * num4;
			double num11 = num * num6;
			double num12 = num2 * num2 + num5 * num5;
			double num13 = num * num5;
			double num14 = num2 * num4;
			double num15 = num3 * num3 + num6 * num6;
			double num16 = num9 * (num7 - num8) + num12 * (num10 - num11) + num15 * (num13 - num14);
			if (Behavior.NoExact)
			{
				return num16;
			}
			double num17 = (Math.Abs(num7) + Math.Abs(num8)) * num9 + (Math.Abs(num10) + Math.Abs(num11)) * num12 + (Math.Abs(num13) + Math.Abs(num14)) * num15;
			double num18 = iccerrboundA * num17;
			if (num16 > num18 || 0.0 - num16 > num18)
			{
				return num16;
			}
			Statistic.InCircleAdaptCount++;
			return InCircleAdapt(pa, pb, pc, pd, num17);
		}

		public static double NonRegular(Point pa, Point pb, Point pc, Point pd)
		{
			return InCircle(pa, pb, pc, pd);
		}

		public static Point FindCircumcenter(Point torg, Point tdest, Point tapex, ref double xi, ref double eta, double offconstant)
		{
			Statistic.CircumcenterCount++;
			double num = tdest.x - torg.x;
			double num2 = tdest.y - torg.y;
			double num3 = tapex.x - torg.x;
			double num4 = tapex.y - torg.y;
			double num5 = num * num + num2 * num2;
			double num6 = num3 * num3 + num4 * num4;
			double num7 = (tdest.x - tapex.x) * (tdest.x - tapex.x) + (tdest.y - tapex.y) * (tdest.y - tapex.y);
			double num8;
			if (Behavior.NoExact)
			{
				num8 = 0.5 / (num * num4 - num3 * num2);
			}
			else
			{
				num8 = 0.5 / CounterClockwise(tdest, tapex, torg);
				Statistic.CounterClockwiseCount--;
			}
			double num9 = (num4 * num5 - num2 * num6) * num8;
			double num10 = (num * num6 - num3 * num5) * num8;
			if (num5 < num6 && num5 < num7)
			{
				if (offconstant > 0.0)
				{
					double num11 = 0.5 * num - offconstant * num2;
					double num12 = 0.5 * num2 + offconstant * num;
					if (num11 * num11 + num12 * num12 < num9 * num9 + num10 * num10)
					{
						num9 = num11;
						num10 = num12;
					}
				}
			}
			else if (num6 < num7)
			{
				if (offconstant > 0.0)
				{
					double num11 = 0.5 * num3 + offconstant * num4;
					double num12 = 0.5 * num4 - offconstant * num3;
					if (num11 * num11 + num12 * num12 < num9 * num9 + num10 * num10)
					{
						num9 = num11;
						num10 = num12;
					}
				}
			}
			else if (offconstant > 0.0)
			{
				double num11 = 0.5 * (tapex.x - tdest.x) - offconstant * (tapex.y - tdest.y);
				double num12 = 0.5 * (tapex.y - tdest.y) + offconstant * (tapex.x - tdest.x);
				if (num11 * num11 + num12 * num12 < (num9 - num) * (num9 - num) + (num10 - num2) * (num10 - num2))
				{
					num9 = num + num11;
					num10 = num2 + num12;
				}
			}
			xi = (num4 * num9 - num3 * num10) * (2.0 * num8);
			eta = (num * num10 - num2 * num9) * (2.0 * num8);
			return new Point(torg.x + num9, torg.y + num10);
		}

		public static Point FindCircumcenter(Point torg, Point tdest, Point tapex, ref double xi, ref double eta)
		{
			Statistic.CircumcenterCount++;
			double num = tdest.x - torg.x;
			double num2 = tdest.y - torg.y;
			double num3 = tapex.x - torg.x;
			double num4 = tapex.y - torg.y;
			double num5 = num * num + num2 * num2;
			double num6 = num3 * num3 + num4 * num4;
			double num7;
			if (Behavior.NoExact)
			{
				num7 = 0.5 / (num * num4 - num3 * num2);
			}
			else
			{
				num7 = 0.5 / CounterClockwise(tdest, tapex, torg);
				Statistic.CounterClockwiseCount--;
			}
			double num8 = (num4 * num5 - num2 * num6) * num7;
			double num9 = (num * num6 - num3 * num5) * num7;
			xi = (num4 * num8 - num3 * num9) * (2.0 * num7);
			eta = (num * num9 - num2 * num8) * (2.0 * num7);
			return new Point(torg.x + num8, torg.y + num9);
		}

		private static int FastExpansionSumZeroElim(int elen, double[] e, int flen, double[] f, double[] h)
		{
			double num = e[0];
			double num2 = f[0];
			int num4;
			int num3 = (num4 = 0);
			double num5;
			if (num2 > num == num2 > 0.0 - num)
			{
				num5 = num;
				num = e[++num3];
			}
			else
			{
				num5 = num2;
				num2 = f[++num4];
			}
			int num6 = 0;
			if (num3 < elen && num4 < flen)
			{
				double num7;
				double num9;
				if (num2 > num == num2 > 0.0 - num)
				{
					num7 = num + num5;
					double num8 = num7 - num;
					num9 = num5 - num8;
					num = e[++num3];
				}
				else
				{
					num7 = num2 + num5;
					double num8 = num7 - num2;
					num9 = num5 - num8;
					num2 = f[++num4];
				}
				num5 = num7;
				if (num9 != 0.0)
				{
					h[num6++] = num9;
				}
				while (num3 < elen && num4 < flen)
				{
					if (num2 > num == num2 > 0.0 - num)
					{
						num7 = num5 + num;
						double num8 = num7 - num5;
						double num10 = num7 - num8;
						double num11 = num - num8;
						num9 = num5 - num10 + num11;
						num = e[++num3];
					}
					else
					{
						num7 = num5 + num2;
						double num8 = num7 - num5;
						double num10 = num7 - num8;
						double num11 = num2 - num8;
						num9 = num5 - num10 + num11;
						num2 = f[++num4];
					}
					num5 = num7;
					if (num9 != 0.0)
					{
						h[num6++] = num9;
					}
				}
			}
			while (num3 < elen)
			{
				double num7 = num5 + num;
				double num8 = num7 - num5;
				double num10 = num7 - num8;
				double num11 = num - num8;
				double num9 = num5 - num10 + num11;
				num = e[++num3];
				num5 = num7;
				if (num9 != 0.0)
				{
					h[num6++] = num9;
				}
			}
			while (num4 < flen)
			{
				double num7 = num5 + num2;
				double num8 = num7 - num5;
				double num10 = num7 - num8;
				double num11 = num2 - num8;
				double num9 = num5 - num10 + num11;
				num2 = f[++num4];
				num5 = num7;
				if (num9 != 0.0)
				{
					h[num6++] = num9;
				}
			}
			if (num5 != 0.0 || num6 == 0)
			{
				h[num6++] = num5;
			}
			return num6;
		}

		private static int ScaleExpansionZeroElim(int elen, double[] e, double b, double[] h)
		{
			double num = splitter * b;
			double num2 = num - b;
			double num3 = num - num2;
			double num4 = b - num3;
			double num5 = e[0] * b;
			double num6 = splitter * e[0];
			num2 = num6 - e[0];
			double num7 = num6 - num2;
			double num8 = e[0] - num7;
			double num9 = num5 - num7 * num3 - num8 * num3 - num7 * num4;
			double num10 = num8 * num4 - num9;
			int num11 = 0;
			if (num10 != 0.0)
			{
				h[num11++] = num10;
			}
			for (int i = 1; i < elen; i++)
			{
				double num12 = e[i];
				double num13 = num12 * b;
				double num14 = splitter * num12;
				num2 = num14 - num12;
				num7 = num14 - num2;
				num8 = num12 - num7;
				num9 = num13 - num7 * num3 - num8 * num3 - num7 * num4;
				double num15 = num8 * num4 - num9;
				double num16 = num5 + num15;
				double num17 = num16 - num5;
				double num18 = num16 - num17;
				double num19 = num15 - num17;
				num10 = num5 - num18 + num19;
				if (num10 != 0.0)
				{
					h[num11++] = num10;
				}
				num5 = num13 + num16;
				num17 = num5 - num13;
				num10 = num16 - num17;
				if (num10 != 0.0)
				{
					h[num11++] = num10;
				}
			}
			if (num5 != 0.0 || num11 == 0)
			{
				h[num11++] = num5;
			}
			return num11;
		}

		private static double Estimate(int elen, double[] e)
		{
			double num = e[0];
			for (int i = 1; i < elen; i++)
			{
				num += e[i];
			}
			return num;
		}

		private static double CounterClockwiseAdapt(Point pa, Point pb, Point pc, double detsum)
		{
			double[] array = new double[5];
			double[] array2 = new double[5];
			double[] array3 = new double[8];
			double[] array4 = new double[12];
			double[] array5 = new double[16];
			double num = pa.X - pc.X;
			double num2 = pb.X - pc.X;
			double num3 = pa.Y - pc.Y;
			double num4 = pb.Y - pc.Y;
			double num5 = num * num4;
			double num6 = splitter * num;
			double num7 = num6 - num;
			double num8 = num6 - num7;
			double num9 = num - num8;
			double num10 = splitter * num4;
			num7 = num10 - num4;
			double num11 = num10 - num7;
			double num12 = num4 - num11;
			double num13 = num5 - num8 * num11 - num9 * num11 - num8 * num12;
			double num14 = num9 * num12 - num13;
			double num15 = num3 * num2;
			double num16 = splitter * num3;
			num7 = num16 - num3;
			num8 = num16 - num7;
			num9 = num3 - num8;
			double num17 = splitter * num2;
			num7 = num17 - num2;
			num11 = num17 - num7;
			num12 = num2 - num11;
			num13 = num15 - num8 * num11 - num9 * num11 - num8 * num12;
			double num18 = num9 * num12 - num13;
			double num19 = num14 - num18;
			double num20 = num14 - num19;
			double num21 = num19 + num20;
			double num22 = num20 - num18;
			double num23 = num14 - num21;
			array[0] = num23 + num22;
			double num24 = num5 + num19;
			num20 = num24 - num5;
			num21 = num24 - num20;
			num22 = num19 - num20;
			num23 = num5 - num21;
			double num25 = num23 + num22;
			num19 = num25 - num15;
			num20 = num25 - num19;
			num21 = num19 + num20;
			num22 = num20 - num15;
			num23 = num25 - num21;
			array[1] = num23 + num22;
			double num26 = num24 + num19;
			num20 = num26 - num24;
			num21 = num26 - num20;
			num22 = num19 - num20;
			num23 = num24 - num21;
			array[2] = num23 + num22;
			array[3] = num26;
			double num27 = Estimate(4, array);
			double num28 = ccwerrboundB * detsum;
			if (num27 >= num28 || 0.0 - num27 >= num28)
			{
				return num27;
			}
			num20 = pa.X - num;
			num21 = num + num20;
			num22 = num20 - pc.X;
			num23 = pa.X - num21;
			double num29 = num23 + num22;
			num20 = pb.X - num2;
			num21 = num2 + num20;
			num22 = num20 - pc.X;
			num23 = pb.X - num21;
			double num30 = num23 + num22;
			num20 = pa.Y - num3;
			num21 = num3 + num20;
			num22 = num20 - pc.Y;
			num23 = pa.Y - num21;
			double num31 = num23 + num22;
			num20 = pb.Y - num4;
			num21 = num4 + num20;
			num22 = num20 - pc.Y;
			num23 = pb.Y - num21;
			double num32 = num23 + num22;
			if (num29 == 0.0 && num31 == 0.0 && num30 == 0.0 && num32 == 0.0)
			{
				return num27;
			}
			num28 = ccwerrboundC * detsum + resulterrbound * ((num27 >= 0.0) ? num27 : (0.0 - num27));
			num27 += num * num32 + num4 * num29 - (num3 * num30 + num2 * num31);
			if (num27 >= num28 || 0.0 - num27 >= num28)
			{
				return num27;
			}
			double num33 = num29 * num4;
			double num34 = splitter * num29;
			num7 = num34 - num29;
			num8 = num34 - num7;
			num9 = num29 - num8;
			double num35 = splitter * num4;
			num7 = num35 - num4;
			num11 = num35 - num7;
			num12 = num4 - num11;
			num13 = num33 - num8 * num11 - num9 * num11 - num8 * num12;
			double num36 = num9 * num12 - num13;
			double num37 = num31 * num2;
			double num38 = splitter * num31;
			num7 = num38 - num31;
			num8 = num38 - num7;
			num9 = num31 - num8;
			double num39 = splitter * num2;
			num7 = num39 - num2;
			num11 = num39 - num7;
			num12 = num2 - num11;
			num13 = num37 - num8 * num11 - num9 * num11 - num8 * num12;
			double num40 = num9 * num12 - num13;
			num19 = num36 - num40;
			num20 = num36 - num19;
			num21 = num19 + num20;
			num22 = num20 - num40;
			num23 = num36 - num21;
			array2[0] = num23 + num22;
			num24 = num33 + num19;
			num20 = num24 - num33;
			num21 = num24 - num20;
			num22 = num19 - num20;
			num23 = num33 - num21;
			double num41 = num23 + num22;
			num19 = num41 - num37;
			num20 = num41 - num19;
			num21 = num19 + num20;
			num22 = num20 - num37;
			num23 = num41 - num21;
			array2[1] = num23 + num22;
			double num42 = num24 + num19;
			num20 = num42 - num24;
			num21 = num42 - num20;
			num22 = num19 - num20;
			num23 = num24 - num21;
			array2[2] = num23 + num22;
			array2[3] = num42;
			int elen = FastExpansionSumZeroElim(4, array, 4, array2, array3);
			num33 = num * num32;
			double num43 = splitter * num;
			num7 = num43 - num;
			num8 = num43 - num7;
			num9 = num - num8;
			double num44 = splitter * num32;
			num7 = num44 - num32;
			num11 = num44 - num7;
			num12 = num32 - num11;
			num13 = num33 - num8 * num11 - num9 * num11 - num8 * num12;
			double num45 = num9 * num12 - num13;
			num37 = num3 * num30;
			double num46 = splitter * num3;
			num7 = num46 - num3;
			num8 = num46 - num7;
			num9 = num3 - num8;
			double num47 = splitter * num30;
			num7 = num47 - num30;
			num11 = num47 - num7;
			num12 = num30 - num11;
			num13 = num37 - num8 * num11 - num9 * num11 - num8 * num12;
			num40 = num9 * num12 - num13;
			num19 = num45 - num40;
			num20 = num45 - num19;
			num21 = num19 + num20;
			num22 = num20 - num40;
			num23 = num45 - num21;
			array2[0] = num23 + num22;
			num24 = num33 + num19;
			num20 = num24 - num33;
			num21 = num24 - num20;
			num22 = num19 - num20;
			num23 = num33 - num21;
			double num48 = num23 + num22;
			num19 = num48 - num37;
			num20 = num48 - num19;
			num21 = num19 + num20;
			num22 = num20 - num37;
			num23 = num48 - num21;
			array2[1] = num23 + num22;
			num42 = num24 + num19;
			num20 = num42 - num24;
			num21 = num42 - num20;
			num22 = num19 - num20;
			num23 = num24 - num21;
			array2[2] = num23 + num22;
			array2[3] = num42;
			int elen2 = FastExpansionSumZeroElim(elen, array3, 4, array2, array4);
			num33 = num29 * num32;
			double num49 = splitter * num29;
			num7 = num49 - num29;
			num8 = num49 - num7;
			num9 = num29 - num8;
			double num50 = splitter * num32;
			num7 = num50 - num32;
			num11 = num50 - num7;
			num12 = num32 - num11;
			num13 = num33 - num8 * num11 - num9 * num11 - num8 * num12;
			double num51 = num9 * num12 - num13;
			num37 = num31 * num30;
			double num52 = splitter * num31;
			num7 = num52 - num31;
			num8 = num52 - num7;
			num9 = num31 - num8;
			double num53 = splitter * num30;
			num7 = num53 - num30;
			num11 = num53 - num7;
			num12 = num30 - num11;
			num13 = num37 - num8 * num11 - num9 * num11 - num8 * num12;
			num40 = num9 * num12 - num13;
			num19 = num51 - num40;
			num20 = num51 - num19;
			num21 = num19 + num20;
			num22 = num20 - num40;
			num23 = num51 - num21;
			array2[0] = num23 + num22;
			num24 = num33 + num19;
			num20 = num24 - num33;
			num21 = num24 - num20;
			num22 = num19 - num20;
			num23 = num33 - num21;
			double num54 = num23 + num22;
			num19 = num54 - num37;
			num20 = num54 - num19;
			num21 = num19 + num20;
			num22 = num20 - num37;
			num23 = num54 - num21;
			array2[1] = num23 + num22;
			num42 = num24 + num19;
			num20 = num42 - num24;
			num21 = num42 - num20;
			num22 = num19 - num20;
			num23 = num24 - num21;
			array2[2] = num23 + num22;
			array2[3] = num42;
			int num55 = FastExpansionSumZeroElim(elen2, array4, 4, array2, array5);
			return array5[num55 - 1];
		}

		private static double InCircleAdapt(Point pa, Point pb, Point pc, Point pd, double permanent)
		{
			double[] array = new double[4];
			double[] array2 = new double[4];
			double[] array3 = new double[4];
			double[] array4 = new double[8];
			double[] array5 = new double[16];
			double[] array6 = new double[8];
			double[] array7 = new double[16];
			double[] array8 = new double[32];
			double[] array9 = new double[8];
			double[] array10 = new double[16];
			double[] array11 = new double[8];
			double[] array12 = new double[16];
			double[] array13 = new double[32];
			double[] array14 = new double[8];
			double[] array15 = new double[16];
			double[] array16 = new double[8];
			double[] array17 = new double[16];
			double[] array18 = new double[32];
			double[] array19 = new double[64];
			double[] array20 = new double[1152];
			double[] array21 = new double[1152];
			double[] array22 = new double[4];
			double[] array23 = new double[4];
			double[] array24 = new double[4];
			double[] array25 = new double[5];
			double[] array26 = new double[5];
			double[] array27 = new double[8];
			double[] array28 = new double[16];
			double[] array29 = new double[16];
			double[] array30 = new double[16];
			double[] array31 = new double[32];
			double[] array32 = new double[32];
			double[] array33 = new double[48];
			double[] array34 = new double[64];
			double[] array35 = new double[8];
			double[] array36 = new double[8];
			double[] array37 = new double[8];
			double[] array38 = new double[8];
			double[] array39 = new double[8];
			double[] array40 = new double[8];
			double[] array41 = new double[8];
			double[] array42 = new double[8];
			double[] array43 = new double[8];
			double[] array44 = new double[8];
			double[] array45 = new double[8];
			double[] array46 = new double[8];
			double[] array47 = new double[8];
			double[] array48 = new double[8];
			double[] array49 = new double[8];
			double[] array50 = new double[8];
			double[] array51 = new double[8];
			double[] array52 = new double[8];
			int elen = 0;
			int elen2 = 0;
			int elen3 = 0;
			int elen4 = 0;
			int elen5 = 0;
			int elen6 = 0;
			double[] array53 = new double[16];
			double[] array54 = new double[16];
			double[] array55 = new double[16];
			double[] array56 = new double[16];
			double[] array57 = new double[16];
			double[] array58 = new double[16];
			double[] array59 = new double[8];
			double[] array60 = new double[8];
			double[] array61 = new double[8];
			double[] array62 = new double[8];
			double[] array63 = new double[8];
			double[] array64 = new double[8];
			double[] array65 = new double[8];
			double[] array66 = new double[8];
			double[] array67 = new double[8];
			double[] array68 = new double[4];
			double[] array69 = new double[4];
			double[] array70 = new double[4];
			double num = pa.X - pd.X;
			double num2 = pb.X - pd.X;
			double num3 = pc.X - pd.X;
			double num4 = pa.Y - pd.Y;
			double num5 = pb.Y - pd.Y;
			double num6 = pc.Y - pd.Y;
			num = pa.X - pd.X;
			num2 = pb.X - pd.X;
			num3 = pc.X - pd.X;
			num4 = pa.Y - pd.Y;
			num5 = pb.Y - pd.Y;
			num6 = pc.Y - pd.Y;
			double num7 = num2 * num6;
			double num8 = splitter * num2;
			double num9 = num8 - num2;
			double num10 = num8 - num9;
			double num11 = num2 - num10;
			double num12 = splitter * num6;
			num9 = num12 - num6;
			double num13 = num12 - num9;
			double num14 = num6 - num13;
			double num15 = num7 - num10 * num13 - num11 * num13 - num10 * num14;
			double num16 = num11 * num14 - num15;
			double num17 = num3 * num5;
			double num18 = splitter * num3;
			num9 = num18 - num3;
			num10 = num18 - num9;
			num11 = num3 - num10;
			double num19 = splitter * num5;
			num9 = num19 - num5;
			num13 = num19 - num9;
			num14 = num5 - num13;
			num15 = num17 - num10 * num13 - num11 * num13 - num10 * num14;
			double num20 = num11 * num14 - num15;
			double num21 = num16 - num20;
			double num22 = num16 - num21;
			double num23 = num21 + num22;
			double num24 = num22 - num20;
			double num25 = num16 - num23;
			array[0] = num25 + num24;
			double num26 = num7 + num21;
			num22 = num26 - num7;
			num23 = num26 - num22;
			num24 = num21 - num22;
			num25 = num7 - num23;
			double num27 = num25 + num24;
			num21 = num27 - num17;
			num22 = num27 - num21;
			num23 = num21 + num22;
			num24 = num22 - num17;
			num25 = num27 - num23;
			array[1] = num25 + num24;
			double num28 = num26 + num21;
			num22 = num28 - num26;
			num23 = num28 - num22;
			num24 = num21 - num22;
			num25 = num26 - num23;
			array[2] = num25 + num24;
			array[3] = num28;
			int elen7 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array, num, array4), array4, num, array5);
			int flen = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array, num4, array6), array6, num4, array7);
			int elen8 = FastExpansionSumZeroElim(elen7, array5, flen, array7, array8);
			double num29 = num3 * num4;
			double num30 = splitter * num3;
			num9 = num30 - num3;
			num10 = num30 - num9;
			num11 = num3 - num10;
			double num31 = splitter * num4;
			num9 = num31 - num4;
			num13 = num31 - num9;
			num14 = num4 - num13;
			num15 = num29 - num10 * num13 - num11 * num13 - num10 * num14;
			double num32 = num11 * num14 - num15;
			double num33 = num * num6;
			double num34 = splitter * num;
			num9 = num34 - num;
			num10 = num34 - num9;
			num11 = num - num10;
			double num35 = splitter * num6;
			num9 = num35 - num6;
			num13 = num35 - num9;
			num14 = num6 - num13;
			num15 = num33 - num10 * num13 - num11 * num13 - num10 * num14;
			double num36 = num11 * num14 - num15;
			num21 = num32 - num36;
			num22 = num32 - num21;
			num23 = num21 + num22;
			num24 = num22 - num36;
			num25 = num32 - num23;
			array2[0] = num25 + num24;
			num26 = num29 + num21;
			num22 = num26 - num29;
			num23 = num26 - num22;
			num24 = num21 - num22;
			num25 = num29 - num23;
			num27 = num25 + num24;
			num21 = num27 - num33;
			num22 = num27 - num21;
			num23 = num21 + num22;
			num24 = num22 - num33;
			num25 = num27 - num23;
			array2[1] = num25 + num24;
			double num37 = num26 + num21;
			num22 = num37 - num26;
			num23 = num37 - num22;
			num24 = num21 - num22;
			num25 = num26 - num23;
			array2[2] = num25 + num24;
			array2[3] = num37;
			int elen9 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array2, num2, array9), array9, num2, array10);
			int flen2 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array2, num5, array11), array11, num5, array12);
			int flen3 = FastExpansionSumZeroElim(elen9, array10, flen2, array12, array13);
			double num38 = num * num5;
			double num39 = splitter * num;
			num9 = num39 - num;
			num10 = num39 - num9;
			num11 = num - num10;
			double num40 = splitter * num5;
			num9 = num40 - num5;
			num13 = num40 - num9;
			num14 = num5 - num13;
			num15 = num38 - num10 * num13 - num11 * num13 - num10 * num14;
			double num41 = num11 * num14 - num15;
			double num42 = num2 * num4;
			double num43 = splitter * num2;
			num9 = num43 - num2;
			num10 = num43 - num9;
			num11 = num2 - num10;
			double num44 = splitter * num4;
			num9 = num44 - num4;
			num13 = num44 - num9;
			num14 = num4 - num13;
			num15 = num42 - num10 * num13 - num11 * num13 - num10 * num14;
			double num45 = num11 * num14 - num15;
			num21 = num41 - num45;
			num22 = num41 - num21;
			num23 = num21 + num22;
			num24 = num22 - num45;
			num25 = num41 - num23;
			array3[0] = num25 + num24;
			num26 = num38 + num21;
			num22 = num26 - num38;
			num23 = num26 - num22;
			num24 = num21 - num22;
			num25 = num38 - num23;
			num27 = num25 + num24;
			num21 = num27 - num42;
			num22 = num27 - num21;
			num23 = num21 + num22;
			num24 = num22 - num42;
			num25 = num27 - num23;
			array3[1] = num25 + num24;
			double num46 = num26 + num21;
			num22 = num46 - num26;
			num23 = num46 - num22;
			num24 = num21 - num22;
			num25 = num26 - num23;
			array3[2] = num25 + num24;
			array3[3] = num46;
			int elen10 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array3, num3, array14), array14, num3, array15);
			int flen4 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array3, num6, array16), array16, num6, array17);
			int num47 = FastExpansionSumZeroElim(flen: FastExpansionSumZeroElim(elen10, array15, flen4, array17, array18), elen: FastExpansionSumZeroElim(elen8, array8, flen3, array13, array19), e: array19, f: array18, h: array20);
			double num48 = Estimate(num47, array20);
			double num49 = iccerrboundB * permanent;
			if (num48 >= num49 || 0.0 - num48 >= num49)
			{
				return num48;
			}
			num22 = pa.X - num;
			num23 = num + num22;
			num24 = num22 - pd.X;
			num25 = pa.X - num23;
			double num50 = num25 + num24;
			num22 = pa.Y - num4;
			num23 = num4 + num22;
			num24 = num22 - pd.Y;
			num25 = pa.Y - num23;
			double num51 = num25 + num24;
			num22 = pb.X - num2;
			num23 = num2 + num22;
			num24 = num22 - pd.X;
			num25 = pb.X - num23;
			double num52 = num25 + num24;
			num22 = pb.Y - num5;
			num23 = num5 + num22;
			num24 = num22 - pd.Y;
			num25 = pb.Y - num23;
			double num53 = num25 + num24;
			num22 = pc.X - num3;
			num23 = num3 + num22;
			num24 = num22 - pd.X;
			num25 = pc.X - num23;
			double num54 = num25 + num24;
			num22 = pc.Y - num6;
			num23 = num6 + num22;
			num24 = num22 - pd.Y;
			num25 = pc.Y - num23;
			double num55 = num25 + num24;
			if (num50 == 0.0 && num52 == 0.0 && num54 == 0.0 && num51 == 0.0 && num53 == 0.0 && num55 == 0.0)
			{
				return num48;
			}
			num49 = iccerrboundC * permanent + resulterrbound * ((num48 >= 0.0) ? num48 : (0.0 - num48));
			num48 += (num * num + num4 * num4) * (num2 * num55 + num6 * num52 - (num5 * num54 + num3 * num53)) + 2.0 * (num * num50 + num4 * num51) * (num2 * num6 - num5 * num3) + ((num2 * num2 + num5 * num5) * (num3 * num51 + num4 * num54 - (num6 * num50 + num * num55)) + 2.0 * (num2 * num52 + num5 * num53) * (num3 * num4 - num6 * num)) + ((num3 * num3 + num6 * num6) * (num * num53 + num5 * num50 - (num4 * num52 + num2 * num51)) + 2.0 * (num3 * num54 + num6 * num55) * (num * num5 - num4 * num2));
			if (num48 >= num49 || 0.0 - num48 >= num49)
			{
				return num48;
			}
			double[] array71 = array20;
			double[] array72 = array21;
			if (num52 != 0.0 || num53 != 0.0 || num54 != 0.0 || num55 != 0.0)
			{
				double num56 = num * num;
				double num57 = splitter * num;
				num9 = num57 - num;
				num10 = num57 - num9;
				num11 = num - num10;
				num15 = num56 - num10 * num10 - (num10 + num10) * num11;
				double num58 = num11 * num11 - num15;
				double num59 = num4 * num4;
				double num60 = splitter * num4;
				num9 = num60 - num4;
				num10 = num60 - num9;
				num11 = num4 - num10;
				num15 = num59 - num10 * num10 - (num10 + num10) * num11;
				double num61 = num11 * num11 - num15;
				num21 = num58 + num61;
				num22 = num21 - num58;
				num23 = num21 - num22;
				num24 = num61 - num22;
				num25 = num58 - num23;
				array22[0] = num25 + num24;
				num26 = num56 + num21;
				num22 = num26 - num56;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num56 - num23;
				num27 = num25 + num24;
				num21 = num27 + num59;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num59 - num22;
				num25 = num27 - num23;
				array22[1] = num25 + num24;
				double num62 = num26 + num21;
				num22 = num62 - num26;
				num23 = num62 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array22[2] = num25 + num24;
				array22[3] = num62;
			}
			if (num54 != 0.0 || num55 != 0.0 || num50 != 0.0 || num51 != 0.0)
			{
				double num63 = num2 * num2;
				double num64 = splitter * num2;
				num9 = num64 - num2;
				num10 = num64 - num9;
				num11 = num2 - num10;
				num15 = num63 - num10 * num10 - (num10 + num10) * num11;
				double num65 = num11 * num11 - num15;
				double num66 = num5 * num5;
				double num67 = splitter * num5;
				num9 = num67 - num5;
				num10 = num67 - num9;
				num11 = num5 - num10;
				num15 = num66 - num10 * num10 - (num10 + num10) * num11;
				double num68 = num11 * num11 - num15;
				num21 = num65 + num68;
				num22 = num21 - num65;
				num23 = num21 - num22;
				num24 = num68 - num22;
				num25 = num65 - num23;
				array23[0] = num25 + num24;
				num26 = num63 + num21;
				num22 = num26 - num63;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num63 - num23;
				num27 = num25 + num24;
				num21 = num27 + num66;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num66 - num22;
				num25 = num27 - num23;
				array23[1] = num25 + num24;
				double num69 = num26 + num21;
				num22 = num69 - num26;
				num23 = num69 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array23[2] = num25 + num24;
				array23[3] = num69;
			}
			if (num50 != 0.0 || num51 != 0.0 || num52 != 0.0 || num53 != 0.0)
			{
				double num70 = num3 * num3;
				double num71 = splitter * num3;
				num9 = num71 - num3;
				num10 = num71 - num9;
				num11 = num3 - num10;
				num15 = num70 - num10 * num10 - (num10 + num10) * num11;
				double num72 = num11 * num11 - num15;
				double num73 = num6 * num6;
				double num74 = splitter * num6;
				num9 = num74 - num6;
				num10 = num74 - num9;
				num11 = num6 - num10;
				num15 = num73 - num10 * num10 - (num10 + num10) * num11;
				double num75 = num11 * num11 - num15;
				num21 = num72 + num75;
				num22 = num21 - num72;
				num23 = num21 - num22;
				num24 = num75 - num22;
				num25 = num72 - num23;
				array24[0] = num25 + num24;
				num26 = num70 + num21;
				num22 = num26 - num70;
				num23 = num26 - num22;
				num24 = num21 - num22;
				num25 = num70 - num23;
				num27 = num25 + num24;
				num21 = num27 + num73;
				num22 = num21 - num27;
				num23 = num21 - num22;
				num24 = num73 - num22;
				num25 = num27 - num23;
				array24[1] = num25 + num24;
				double num76 = num26 + num21;
				num22 = num76 - num26;
				num23 = num76 - num22;
				num24 = num21 - num22;
				num25 = num26 - num23;
				array24[2] = num25 + num24;
				array24[3] = num76;
			}
			if (num50 != 0.0)
			{
				elen = ScaleExpansionZeroElim(4, array, num50, array47);
				int elen11 = ScaleExpansionZeroElim(elen, array47, 2.0 * num, array28);
				int flen5 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array24, num50, array36), array36, num5, array29);
				int elen12 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array23, num50, array35), array35, 0.0 - num6, array30);
				int flen6 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array31);
				int flen7 = FastExpansionSumZeroElim(elen12, array30, flen6, array31, array33);
				num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
				double[] array73 = array71;
				array71 = array72;
				array72 = array73;
			}
			if (num51 != 0.0)
			{
				elen2 = ScaleExpansionZeroElim(4, array, num51, array48);
				int elen11 = ScaleExpansionZeroElim(elen2, array48, 2.0 * num4, array28);
				int flen5 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array23, num51, array37), array37, num3, array29);
				int elen13 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array24, num51, array38), array38, 0.0 - num2, array30);
				int flen6 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array31);
				int flen7 = FastExpansionSumZeroElim(elen13, array30, flen6, array31, array33);
				num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
				double[] array74 = array71;
				array71 = array72;
				array72 = array74;
			}
			if (num52 != 0.0)
			{
				elen3 = ScaleExpansionZeroElim(4, array2, num52, array49);
				int elen11 = ScaleExpansionZeroElim(elen3, array49, 2.0 * num2, array28);
				int flen5 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array22, num52, array39), array39, num6, array29);
				int elen14 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array24, num52, array40), array40, 0.0 - num4, array30);
				int flen6 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array31);
				int flen7 = FastExpansionSumZeroElim(elen14, array30, flen6, array31, array33);
				num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
				double[] array75 = array71;
				array71 = array72;
				array72 = array75;
			}
			if (num53 != 0.0)
			{
				elen4 = ScaleExpansionZeroElim(4, array2, num53, array50);
				int elen11 = ScaleExpansionZeroElim(elen4, array50, 2.0 * num5, array28);
				int flen5 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array24, num53, array42), array42, num, array29);
				int elen15 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array22, num53, array41), array41, 0.0 - num3, array30);
				int flen6 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array31);
				int flen7 = FastExpansionSumZeroElim(elen15, array30, flen6, array31, array33);
				num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
				double[] array76 = array71;
				array71 = array72;
				array72 = array76;
			}
			if (num54 != 0.0)
			{
				elen5 = ScaleExpansionZeroElim(4, array3, num54, array51);
				int elen11 = ScaleExpansionZeroElim(elen5, array51, 2.0 * num3, array28);
				int flen5 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array23, num54, array44), array44, num4, array29);
				int elen16 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array22, num54, array43), array43, 0.0 - num5, array30);
				int flen6 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array31);
				int flen7 = FastExpansionSumZeroElim(elen16, array30, flen6, array31, array33);
				num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
				double[] array77 = array71;
				array71 = array72;
				array72 = array77;
			}
			if (num55 != 0.0)
			{
				elen6 = ScaleExpansionZeroElim(4, array3, num55, array52);
				int elen11 = ScaleExpansionZeroElim(elen6, array52, 2.0 * num6, array28);
				int flen5 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array22, num55, array45), array45, num2, array29);
				int elen17 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array23, num55, array46), array46, 0.0 - num, array30);
				int flen6 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array31);
				int flen7 = FastExpansionSumZeroElim(elen17, array30, flen6, array31, array33);
				num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
				double[] array78 = array71;
				array71 = array72;
				array72 = array78;
			}
			if (num50 != 0.0 || num51 != 0.0)
			{
				int elen18;
				int elen19;
				if (num52 != 0.0 || num53 != 0.0 || num54 != 0.0 || num55 != 0.0)
				{
					double num77 = num52 * num6;
					double num78 = splitter * num52;
					num9 = num78 - num52;
					num10 = num78 - num9;
					num11 = num52 - num10;
					double num79 = splitter * num6;
					num9 = num79 - num6;
					num13 = num79 - num9;
					num14 = num6 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					double num80 = num11 * num14 - num15;
					double num81 = num2 * num55;
					double num82 = splitter * num2;
					num9 = num82 - num2;
					num10 = num82 - num9;
					num11 = num2 - num10;
					double num83 = splitter * num55;
					num9 = num83 - num55;
					num13 = num83 - num9;
					num14 = num55 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					double num84 = num11 * num14 - num15;
					num21 = num80 + num84;
					num22 = num21 - num80;
					num23 = num21 - num22;
					num24 = num84 - num22;
					num25 = num80 - num23;
					array25[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 + num81;
					num22 = num21 - num27;
					num23 = num21 - num22;
					num24 = num81 - num22;
					num25 = num27 - num23;
					array25[1] = num25 + num24;
					double num85 = num26 + num21;
					num22 = num85 - num26;
					num23 = num85 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array25[2] = num25 + num24;
					array25[3] = num85;
					double num86 = 0.0 - num5;
					num77 = num54 * num86;
					double num87 = splitter * num54;
					num9 = num87 - num54;
					num10 = num87 - num9;
					num11 = num54 - num10;
					double num88 = splitter * num86;
					num9 = num88 - num86;
					num13 = num88 - num9;
					num14 = num86 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					num80 = num11 * num14 - num15;
					num86 = 0.0 - num53;
					num81 = num3 * num86;
					double num89 = splitter * num3;
					num9 = num89 - num3;
					num10 = num89 - num9;
					num11 = num3 - num10;
					double num90 = splitter * num86;
					num9 = num90 - num86;
					num13 = num90 - num9;
					num14 = num86 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					num84 = num11 * num14 - num15;
					num21 = num80 + num84;
					num22 = num21 - num80;
					num23 = num21 - num22;
					num24 = num84 - num22;
					num25 = num80 - num23;
					array26[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 + num81;
					num22 = num21 - num27;
					num23 = num21 - num22;
					num24 = num81 - num22;
					num25 = num27 - num23;
					array26[1] = num25 + num24;
					double num91 = num26 + num21;
					num22 = num91 - num26;
					num23 = num91 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array26[2] = num25 + num24;
					array26[3] = num91;
					elen18 = FastExpansionSumZeroElim(4, array25, 4, array26, array66);
					num77 = num52 * num55;
					double num92 = splitter * num52;
					num9 = num92 - num52;
					num10 = num92 - num9;
					num11 = num52 - num10;
					double num93 = splitter * num55;
					num9 = num93 - num55;
					num13 = num93 - num9;
					num14 = num55 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					num80 = num11 * num14 - num15;
					num81 = num54 * num53;
					double num94 = splitter * num54;
					num9 = num94 - num54;
					num10 = num94 - num9;
					num11 = num54 - num10;
					double num95 = splitter * num53;
					num9 = num95 - num53;
					num13 = num95 - num9;
					num14 = num53 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					num84 = num11 * num14 - num15;
					num21 = num80 - num84;
					num22 = num80 - num21;
					num23 = num21 + num22;
					num24 = num22 - num84;
					num25 = num80 - num23;
					array69[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 - num81;
					num22 = num27 - num21;
					num23 = num21 + num22;
					num24 = num22 - num81;
					num25 = num27 - num23;
					array69[1] = num25 + num24;
					double num96 = num26 + num21;
					num22 = num96 - num26;
					num23 = num96 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array69[2] = num25 + num24;
					array69[3] = num96;
					elen19 = 4;
				}
				else
				{
					array66[0] = 0.0;
					elen18 = 1;
					array69[0] = 0.0;
					elen19 = 1;
				}
				if (num50 != 0.0)
				{
					int elen11 = ScaleExpansionZeroElim(elen, array47, num50, array28);
					int elen20 = ScaleExpansionZeroElim(elen18, array66, num50, array53);
					int flen6 = ScaleExpansionZeroElim(elen20, array53, 2.0 * num, array31);
					int flen7 = FastExpansionSumZeroElim(elen11, array28, flen6, array31, array33);
					num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
					double[] array79 = array71;
					array71 = array72;
					array72 = array79;
					if (num53 != 0.0)
					{
						elen11 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array24, num50, array27), array27, num53, array28);
						num47 = FastExpansionSumZeroElim(num47, array71, elen11, array28, array72);
						double[] array80 = array71;
						array71 = array72;
						array72 = array80;
					}
					if (num55 != 0.0)
					{
						elen11 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array23, 0.0 - num50, array27), array27, num55, array28);
						num47 = FastExpansionSumZeroElim(num47, array71, elen11, array28, array72);
						double[] array81 = array71;
						array71 = array72;
						array72 = array81;
					}
					flen6 = ScaleExpansionZeroElim(elen20, array53, num50, array31);
					int elen21 = ScaleExpansionZeroElim(elen19, array69, num50, array59);
					elen11 = ScaleExpansionZeroElim(elen21, array59, 2.0 * num, array28);
					int flen5 = ScaleExpansionZeroElim(elen21, array59, num50, array29);
					int flen8 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array32);
					int flen9 = FastExpansionSumZeroElim(flen6, array31, flen8, array32, array34);
					num47 = FastExpansionSumZeroElim(num47, array71, flen9, array34, array72);
					double[] array82 = array71;
					array71 = array72;
					array72 = array82;
				}
				if (num51 != 0.0)
				{
					int elen11 = ScaleExpansionZeroElim(elen2, array48, num51, array28);
					int elen22 = ScaleExpansionZeroElim(elen18, array66, num51, array54);
					int flen6 = ScaleExpansionZeroElim(elen22, array54, 2.0 * num4, array31);
					int flen7 = FastExpansionSumZeroElim(elen11, array28, flen6, array31, array33);
					num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
					double[] array83 = array71;
					array71 = array72;
					array72 = array83;
					flen6 = ScaleExpansionZeroElim(elen22, array54, num51, array31);
					int elen23 = ScaleExpansionZeroElim(elen19, array69, num51, array60);
					elen11 = ScaleExpansionZeroElim(elen23, array60, 2.0 * num4, array28);
					int flen5 = ScaleExpansionZeroElim(elen23, array60, num51, array29);
					int flen8 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array32);
					int flen9 = FastExpansionSumZeroElim(flen6, array31, flen8, array32, array34);
					num47 = FastExpansionSumZeroElim(num47, array71, flen9, array34, array72);
					double[] array84 = array71;
					array71 = array72;
					array72 = array84;
				}
			}
			if (num52 != 0.0 || num53 != 0.0)
			{
				int elen24;
				int elen25;
				if (num54 != 0.0 || num55 != 0.0 || num50 != 0.0 || num51 != 0.0)
				{
					double num77 = num54 * num4;
					double num97 = splitter * num54;
					num9 = num97 - num54;
					num10 = num97 - num9;
					num11 = num54 - num10;
					double num98 = splitter * num4;
					num9 = num98 - num4;
					num13 = num98 - num9;
					num14 = num4 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					double num80 = num11 * num14 - num15;
					double num81 = num3 * num51;
					double num99 = splitter * num3;
					num9 = num99 - num3;
					num10 = num99 - num9;
					num11 = num3 - num10;
					double num100 = splitter * num51;
					num9 = num100 - num51;
					num13 = num100 - num9;
					num14 = num51 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					double num84 = num11 * num14 - num15;
					num21 = num80 + num84;
					num22 = num21 - num80;
					num23 = num21 - num22;
					num24 = num84 - num22;
					num25 = num80 - num23;
					array25[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 + num81;
					num22 = num21 - num27;
					num23 = num21 - num22;
					num24 = num81 - num22;
					num25 = num27 - num23;
					array25[1] = num25 + num24;
					double num85 = num26 + num21;
					num22 = num85 - num26;
					num23 = num85 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array25[2] = num25 + num24;
					array25[3] = num85;
					double num86 = 0.0 - num6;
					num77 = num50 * num86;
					double num101 = splitter * num50;
					num9 = num101 - num50;
					num10 = num101 - num9;
					num11 = num50 - num10;
					double num102 = splitter * num86;
					num9 = num102 - num86;
					num13 = num102 - num9;
					num14 = num86 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					num80 = num11 * num14 - num15;
					num86 = 0.0 - num55;
					num81 = num * num86;
					double num103 = splitter * num;
					num9 = num103 - num;
					num10 = num103 - num9;
					num11 = num - num10;
					double num104 = splitter * num86;
					num9 = num104 - num86;
					num13 = num104 - num9;
					num14 = num86 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					num84 = num11 * num14 - num15;
					num21 = num80 + num84;
					num22 = num21 - num80;
					num23 = num21 - num22;
					num24 = num84 - num22;
					num25 = num80 - num23;
					array26[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 + num81;
					num22 = num21 - num27;
					num23 = num21 - num22;
					num24 = num81 - num22;
					num25 = num27 - num23;
					array26[1] = num25 + num24;
					double num91 = num26 + num21;
					num22 = num91 - num26;
					num23 = num91 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array26[2] = num25 + num24;
					array26[3] = num91;
					elen24 = FastExpansionSumZeroElim(4, array25, 4, array26, array67);
					num77 = num54 * num51;
					double num105 = splitter * num54;
					num9 = num105 - num54;
					num10 = num105 - num9;
					num11 = num54 - num10;
					double num106 = splitter * num51;
					num9 = num106 - num51;
					num13 = num106 - num9;
					num14 = num51 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					num80 = num11 * num14 - num15;
					num81 = num50 * num55;
					double num107 = splitter * num50;
					num9 = num107 - num50;
					num10 = num107 - num9;
					num11 = num50 - num10;
					double num108 = splitter * num55;
					num9 = num108 - num55;
					num13 = num108 - num9;
					num14 = num55 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					num84 = num11 * num14 - num15;
					num21 = num80 - num84;
					num22 = num80 - num21;
					num23 = num21 + num22;
					num24 = num22 - num84;
					num25 = num80 - num23;
					array70[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 - num81;
					num22 = num27 - num21;
					num23 = num21 + num22;
					num24 = num22 - num81;
					num25 = num27 - num23;
					array70[1] = num25 + num24;
					double num109 = num26 + num21;
					num22 = num109 - num26;
					num23 = num109 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array70[2] = num25 + num24;
					array70[3] = num109;
					elen25 = 4;
				}
				else
				{
					array67[0] = 0.0;
					elen24 = 1;
					array70[0] = 0.0;
					elen25 = 1;
				}
				if (num52 != 0.0)
				{
					int elen11 = ScaleExpansionZeroElim(elen3, array49, num52, array28);
					int elen26 = ScaleExpansionZeroElim(elen24, array67, num52, array55);
					int flen6 = ScaleExpansionZeroElim(elen26, array55, 2.0 * num2, array31);
					int flen7 = FastExpansionSumZeroElim(elen11, array28, flen6, array31, array33);
					num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
					double[] array85 = array71;
					array71 = array72;
					array72 = array85;
					if (num55 != 0.0)
					{
						elen11 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array22, num52, array27), array27, num55, array28);
						num47 = FastExpansionSumZeroElim(num47, array71, elen11, array28, array72);
						double[] array86 = array71;
						array71 = array72;
						array72 = array86;
					}
					if (num51 != 0.0)
					{
						elen11 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array24, 0.0 - num52, array27), array27, num51, array28);
						num47 = FastExpansionSumZeroElim(num47, array71, elen11, array28, array72);
						double[] array87 = array71;
						array71 = array72;
						array72 = array87;
					}
					flen6 = ScaleExpansionZeroElim(elen26, array55, num52, array31);
					int elen27 = ScaleExpansionZeroElim(elen25, array70, num52, array61);
					elen11 = ScaleExpansionZeroElim(elen27, array61, 2.0 * num2, array28);
					int flen5 = ScaleExpansionZeroElim(elen27, array61, num52, array29);
					int flen8 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array32);
					int flen9 = FastExpansionSumZeroElim(flen6, array31, flen8, array32, array34);
					num47 = FastExpansionSumZeroElim(num47, array71, flen9, array34, array72);
					double[] array88 = array71;
					array71 = array72;
					array72 = array88;
				}
				if (num53 != 0.0)
				{
					int elen11 = ScaleExpansionZeroElim(elen4, array50, num53, array28);
					int elen28 = ScaleExpansionZeroElim(elen24, array67, num53, array56);
					int flen6 = ScaleExpansionZeroElim(elen28, array56, 2.0 * num5, array31);
					int flen7 = FastExpansionSumZeroElim(elen11, array28, flen6, array31, array33);
					num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
					double[] array89 = array71;
					array71 = array72;
					array72 = array89;
					flen6 = ScaleExpansionZeroElim(elen28, array56, num53, array31);
					int elen29 = ScaleExpansionZeroElim(elen25, array70, num53, array62);
					elen11 = ScaleExpansionZeroElim(elen29, array62, 2.0 * num5, array28);
					int flen5 = ScaleExpansionZeroElim(elen29, array62, num53, array29);
					int flen8 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array32);
					int flen9 = FastExpansionSumZeroElim(flen6, array31, flen8, array32, array34);
					num47 = FastExpansionSumZeroElim(num47, array71, flen9, array34, array72);
					double[] array90 = array71;
					array71 = array72;
					array72 = array90;
				}
			}
			if (num54 != 0.0 || num55 != 0.0)
			{
				int elen30;
				int elen31;
				if (num50 != 0.0 || num51 != 0.0 || num52 != 0.0 || num53 != 0.0)
				{
					double num77 = num50 * num5;
					double num110 = splitter * num50;
					num9 = num110 - num50;
					num10 = num110 - num9;
					num11 = num50 - num10;
					double num111 = splitter * num5;
					num9 = num111 - num5;
					num13 = num111 - num9;
					num14 = num5 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					double num80 = num11 * num14 - num15;
					double num81 = num * num53;
					double num112 = splitter * num;
					num9 = num112 - num;
					num10 = num112 - num9;
					num11 = num - num10;
					double num113 = splitter * num53;
					num9 = num113 - num53;
					num13 = num113 - num9;
					num14 = num53 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					double num84 = num11 * num14 - num15;
					num21 = num80 + num84;
					num22 = num21 - num80;
					num23 = num21 - num22;
					num24 = num84 - num22;
					num25 = num80 - num23;
					array25[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 + num81;
					num22 = num21 - num27;
					num23 = num21 - num22;
					num24 = num81 - num22;
					num25 = num27 - num23;
					array25[1] = num25 + num24;
					double num85 = num26 + num21;
					num22 = num85 - num26;
					num23 = num85 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array25[2] = num25 + num24;
					array25[3] = num85;
					double num86 = 0.0 - num4;
					num77 = num52 * num86;
					double num114 = splitter * num52;
					num9 = num114 - num52;
					num10 = num114 - num9;
					num11 = num52 - num10;
					double num115 = splitter * num86;
					num9 = num115 - num86;
					num13 = num115 - num9;
					num14 = num86 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					num80 = num11 * num14 - num15;
					num86 = 0.0 - num51;
					num81 = num2 * num86;
					double num116 = splitter * num2;
					num9 = num116 - num2;
					num10 = num116 - num9;
					num11 = num2 - num10;
					double num117 = splitter * num86;
					num9 = num117 - num86;
					num13 = num117 - num9;
					num14 = num86 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					num84 = num11 * num14 - num15;
					num21 = num80 + num84;
					num22 = num21 - num80;
					num23 = num21 - num22;
					num24 = num84 - num22;
					num25 = num80 - num23;
					array26[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 + num81;
					num22 = num21 - num27;
					num23 = num21 - num22;
					num24 = num81 - num22;
					num25 = num27 - num23;
					array26[1] = num25 + num24;
					double num91 = num26 + num21;
					num22 = num91 - num26;
					num23 = num91 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array26[2] = num25 + num24;
					array26[3] = num91;
					elen30 = FastExpansionSumZeroElim(4, array25, 4, array26, array65);
					num77 = num50 * num53;
					double num118 = splitter * num50;
					num9 = num118 - num50;
					num10 = num118 - num9;
					num11 = num50 - num10;
					double num119 = splitter * num53;
					num9 = num119 - num53;
					num13 = num119 - num9;
					num14 = num53 - num13;
					num15 = num77 - num10 * num13 - num11 * num13 - num10 * num14;
					num80 = num11 * num14 - num15;
					num81 = num52 * num51;
					double num120 = splitter * num52;
					num9 = num120 - num52;
					num10 = num120 - num9;
					num11 = num52 - num10;
					double num121 = splitter * num51;
					num9 = num121 - num51;
					num13 = num121 - num9;
					num14 = num51 - num13;
					num15 = num81 - num10 * num13 - num11 * num13 - num10 * num14;
					num84 = num11 * num14 - num15;
					num21 = num80 - num84;
					num22 = num80 - num21;
					num23 = num21 + num22;
					num24 = num22 - num84;
					num25 = num80 - num23;
					array68[0] = num25 + num24;
					num26 = num77 + num21;
					num22 = num26 - num77;
					num23 = num26 - num22;
					num24 = num21 - num22;
					num25 = num77 - num23;
					num27 = num25 + num24;
					num21 = num27 - num81;
					num22 = num27 - num21;
					num23 = num21 + num22;
					num24 = num22 - num81;
					num25 = num27 - num23;
					array68[1] = num25 + num24;
					double num122 = num26 + num21;
					num22 = num122 - num26;
					num23 = num122 - num22;
					num24 = num21 - num22;
					num25 = num26 - num23;
					array68[2] = num25 + num24;
					array68[3] = num122;
					elen31 = 4;
				}
				else
				{
					array65[0] = 0.0;
					elen30 = 1;
					array68[0] = 0.0;
					elen31 = 1;
				}
				if (num54 != 0.0)
				{
					int elen11 = ScaleExpansionZeroElim(elen5, array51, num54, array28);
					int elen32 = ScaleExpansionZeroElim(elen30, array65, num54, array57);
					int flen6 = ScaleExpansionZeroElim(elen32, array57, 2.0 * num3, array31);
					int flen7 = FastExpansionSumZeroElim(elen11, array28, flen6, array31, array33);
					num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
					double[] array91 = array71;
					array71 = array72;
					array72 = array91;
					if (num51 != 0.0)
					{
						elen11 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array23, num54, array27), array27, num51, array28);
						num47 = FastExpansionSumZeroElim(num47, array71, elen11, array28, array72);
						double[] array92 = array71;
						array71 = array72;
						array72 = array92;
					}
					if (num53 != 0.0)
					{
						elen11 = ScaleExpansionZeroElim(ScaleExpansionZeroElim(4, array22, 0.0 - num54, array27), array27, num53, array28);
						num47 = FastExpansionSumZeroElim(num47, array71, elen11, array28, array72);
						double[] array93 = array71;
						array71 = array72;
						array72 = array93;
					}
					flen6 = ScaleExpansionZeroElim(elen32, array57, num54, array31);
					int elen33 = ScaleExpansionZeroElim(elen31, array68, num54, array63);
					elen11 = ScaleExpansionZeroElim(elen33, array63, 2.0 * num3, array28);
					int flen5 = ScaleExpansionZeroElim(elen33, array63, num54, array29);
					int flen8 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array32);
					int flen9 = FastExpansionSumZeroElim(flen6, array31, flen8, array32, array34);
					num47 = FastExpansionSumZeroElim(num47, array71, flen9, array34, array72);
					double[] array94 = array71;
					array71 = array72;
					array72 = array94;
				}
				if (num55 != 0.0)
				{
					int elen11 = ScaleExpansionZeroElim(elen6, array52, num55, array28);
					int elen34 = ScaleExpansionZeroElim(elen30, array65, num55, array58);
					int flen6 = ScaleExpansionZeroElim(elen34, array58, 2.0 * num6, array31);
					int flen7 = FastExpansionSumZeroElim(elen11, array28, flen6, array31, array33);
					num47 = FastExpansionSumZeroElim(num47, array71, flen7, array33, array72);
					double[] array95 = array71;
					array71 = array72;
					array72 = array95;
					flen6 = ScaleExpansionZeroElim(elen34, array58, num55, array31);
					int elen35 = ScaleExpansionZeroElim(elen31, array68, num55, array64);
					elen11 = ScaleExpansionZeroElim(elen35, array64, 2.0 * num6, array28);
					int flen5 = ScaleExpansionZeroElim(elen35, array64, num55, array29);
					int flen8 = FastExpansionSumZeroElim(elen11, array28, flen5, array29, array32);
					int flen9 = FastExpansionSumZeroElim(flen6, array31, flen8, array32, array34);
					num47 = FastExpansionSumZeroElim(num47, array71, flen9, array34, array72);
					double[] array96 = array71;
					array71 = array72;
					array72 = array96;
				}
			}
			return array71[num47 - 1];
		}
	}
}
