using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Ionic
{
	internal sealed class EnumUtil
	{
		private EnumUtil()
		{
		}

		internal static string GetDescription(Enum value)
		{
			FieldInfo field = value.GetType().GetField(value.ToString());
			DescriptionAttribute[] array = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (array.Length > 0)
			{
				return array[0].Description;
			}
			return value.ToString();
		}

		internal static object Parse(Type enumType, string stringRepresentation)
		{
			return Parse(enumType, stringRepresentation, false);
		}

		public static Enum[] GetEnumValues(Type type)
		{
			if (!type.IsEnum)
			{
				throw new ArgumentException("not an enum");
			}
			List<Enum> list = new List<Enum>();
			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.IsLiteral)
				{
					list.Add((Enum)fieldInfo.GetValue(null));
				}
			}
			return list.ToArray();
		}

		public static string[] GetEnumStrings<T>()
		{
			Type typeFromHandle = typeof(T);
			if (!typeFromHandle.IsEnum)
			{
				throw new ArgumentException("not an enum");
			}
			List<string> list = new List<string>();
			FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.IsLiteral)
				{
					list.Add(fieldInfo.Name);
				}
			}
			return list.ToArray();
		}

		internal static object Parse(Type enumType, string stringRepresentation, bool ignoreCase)
		{
			if (ignoreCase)
			{
				stringRepresentation = stringRepresentation.ToLower();
			}
			Enum[] enumValues = GetEnumValues(enumType);
			foreach (Enum obj in enumValues)
			{
				string text = GetDescription(obj);
				if (ignoreCase)
				{
					text = text.ToLower();
				}
				if (text == stringRepresentation)
				{
					return obj;
				}
			}
			return Enum.Parse(enumType, stringRepresentation, ignoreCase);
		}
	}
}
