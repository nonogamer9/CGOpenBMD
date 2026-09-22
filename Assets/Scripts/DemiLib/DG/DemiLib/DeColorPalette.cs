using System;
using System.Globalization;
using UnityEngine;

namespace DG.DemiLib
{
	[Serializable]
	public class DeColorPalette
	{
		public DeColorGlobal global = new DeColorGlobal();

		public DeColorBG bg = new DeColorBG();

		public DeColorContent content = new DeColorContent();

		public static Color HexToColor(string hex)
		{
			if (hex[0] == '#')
			{
				hex = hex.Substring(1);
			}
			int length = hex.Length;
			if (length < 6)
			{
				float r = ((float)HexToInt(hex[0]) + (float)HexToInt(hex[0]) * 16f) / 255f;
				float g = ((float)HexToInt(hex[1]) + (float)HexToInt(hex[1]) * 16f) / 255f;
				float b = ((float)HexToInt(hex[2]) + (float)HexToInt(hex[2]) * 16f) / 255f;
				float a = ((length == 4) ? (((float)HexToInt(hex[3]) + (float)HexToInt(hex[3]) * 16f) / 255f) : 1f);
				return new Color(r, g, b, a);
			}
			float r2 = ((float)HexToInt(hex[1]) + (float)HexToInt(hex[0]) * 16f) / 255f;
			float g2 = ((float)HexToInt(hex[3]) + (float)HexToInt(hex[2]) * 16f) / 255f;
			float b2 = ((float)HexToInt(hex[5]) + (float)HexToInt(hex[4]) * 16f) / 255f;
			float a2 = ((length == 8) ? (((float)HexToInt(hex[7]) + (float)HexToInt(hex[6]) * 16f) / 255f) : 1f);
			return new Color(r2, g2, b2, a2);
		}

		private static int HexToInt(char hexVal)
		{
			return int.Parse(hexVal.ToString(), NumberStyles.HexNumber);
		}
	}
}
