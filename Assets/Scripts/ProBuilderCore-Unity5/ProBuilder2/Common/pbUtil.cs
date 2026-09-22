using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProBuilder2.Common
{
	public static class pbUtil
	{
		private struct SearchRange
		{
			public int begin;

			public int end;

			public SearchRange(int begin, int end)
			{
				this.begin = begin;
				this.end = end;
			}

			public bool Valid()
			{
				return end - begin > 1;
			}

			public int Center()
			{
				return begin + (end - begin) / 2;
			}

			public override string ToString()
			{
				return "{" + begin + ", " + end + "} : " + Center();
			}
		}

		public static T[] GetComponents<T>(this IEnumerable<GameObject> gameObjects) where T : Component
		{
			List<T> list = new List<T>();
			foreach (GameObject gameObject in gameObjects)
			{
				list.AddRange(gameObject.transform.GetComponentsInChildren<T>());
			}
			return list.ToArray();
		}

		public static T[] GetComponents<T>(GameObject go) where T : Component
		{
			return go.transform.GetComponentsInChildren<T>();
		}

		public static T[] GetComponents<T>(this IEnumerable<Transform> transforms) where T : Component
		{
			List<T> list = new List<T>();
			foreach (Transform transform in transforms)
			{
				list.AddRange(transform.GetComponentsInChildren<T>());
			}
			return list.ToArray();
		}

		public static Vector3[] ToWorldSpace(this Transform t, Vector3[] v)
		{
			Vector3[] array = new Vector3[v.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = t.TransformPoint(v[i]);
			}
			return array;
		}

		public static GameObject EmptyGameObjectWithTransform(Transform t)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.position = t.position;
			gameObject.transform.localRotation = t.localRotation;
			gameObject.transform.localScale = t.localScale;
			return gameObject;
		}

		public static T[] ValuesWithIndices<T>(this T[] arr, int[] indices)
		{
			T[] array = new T[indices.Length];
			for (int i = 0; i < indices.Length; i++)
			{
				array[i] = arr[indices[i]];
			}
			return array;
		}

		public static List<T> ValuesWithIndices<T>(this IList<T> arr, IList<int> indices)
		{
			List<T> list = new List<T>();
			foreach (int index in indices)
			{
				list.Add(arr[index]);
			}
			return list;
		}

		public static int[] AllIndexesOf<T>(T[] arr, T instance)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < arr.Length; i++)
			{
				if (arr[i].Equals(instance))
				{
					list.Add(i);
				}
			}
			return list.ToArray();
		}

		public static bool IsEqual<T>(T[] a, T[] b)
		{
			if (a == null && b == null)
			{
				return true;
			}
			if ((a == null && b != null) || (a != null && b == null))
			{
				return false;
			}
			if (a.Length != b.Length)
			{
				return false;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (!a[i].Equals(b[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static T[] Add<T>(this T[] arr, T val)
		{
			T[] array = new T[arr.Length + 1];
			Array.ConstrainedCopy(arr, 0, array, 0, arr.Length);
			array[arr.Length] = val;
			return array;
		}

		public static T[] AddRange<T>(this T[] arr, T[] val)
		{
			T[] array = new T[arr.Length + val.Length];
			Array.ConstrainedCopy(arr, 0, array, 0, arr.Length);
			Array.ConstrainedCopy(val, 0, array, arr.Length, val.Length);
			return array;
		}

		public static T[] Remove<T>(this T[] arr, T val)
		{
			List<T> list = new List<T>(arr);
			list.Remove(val);
			return list.ToArray();
		}

		public static T[] Remove<T>(this T[] arr, IEnumerable<T> val)
		{
			return arr.Except(val).ToArray();
		}

		public static T[] RemoveAt<T>(this T[] arr, int index)
		{
			T[] array = new T[arr.Length - 1];
			int num = 0;
			for (int i = 0; i < arr.Length; i++)
			{
				if (i != index)
				{
					array[num] = arr[i];
					num++;
				}
			}
			return array;
		}

		public static T[] RemoveAt<T>(this IList<T> list, IEnumerable<int> indices)
		{
			List<int> list2 = new List<int>(indices);
			list2.Sort();
			return list.SortedRemoveAt(list2);
		}

		public static T[] SortedRemoveAt<T>(this IList<T> list, IList<int> sorted_indices)
		{
			int count = sorted_indices.Count;
			int count2 = list.Count;
			T[] array = new T[count2 - count];
			int i = 0;
			for (int j = 0; j < count2; j++)
			{
				if (i < count && sorted_indices[i] == j)
				{
					for (; i < count && sorted_indices[i] == j; i++)
					{
					}
				}
				else
				{
					array[j - i] = list[j];
				}
			}
			return array;
		}

		public static int NearestIndexPriorToValue<T>(IList<T> sorted_list, T value) where T : IComparable<T>
		{
			int count = sorted_list.Count;
			if (count < 1)
			{
				return -1;
			}
			SearchRange searchRange = new SearchRange(0, count - 1);
			if (value.CompareTo(sorted_list[0]) < 0)
			{
				return -1;
			}
			if (value.CompareTo(sorted_list[count - 1]) > 0)
			{
				return count - 1;
			}
			while (searchRange.Valid())
			{
				if (sorted_list[searchRange.Center()].CompareTo(value) > 0)
				{
					searchRange.end = searchRange.Center();
					continue;
				}
				searchRange.begin = searchRange.Center();
				if (sorted_list[searchRange.begin + 1].CompareTo(value) < 0)
				{
					continue;
				}
				return searchRange.begin;
			}
			return 0;
		}

		public static T[] Fill<T>(T val, int length)
		{
			return FilledArray(val, length);
		}

		public static List<T> Fill<T>(Func<int, T> ctor, int length)
		{
			List<T> list = new List<T>(length);
			for (int i = 0; i < length; i++)
			{
				list.Add(ctor(i));
			}
			return list;
		}

		public static T[] FilledArray<T>(T val, int length)
		{
			T[] array = new T[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = val;
			}
			return array;
		}

		public static bool ContainsMatch<T>(this T[] a, T[] b)
		{
			int num = -1;
			for (int i = 0; i < a.Length; i++)
			{
				num = Array.IndexOf(b, a[i]);
				if (num > -1)
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsMatch<T>(this T[] a, T[] b, out int index_a, out int index_b)
		{
			index_b = -1;
			for (index_a = 0; index_a < a.Length; index_a++)
			{
				index_b = Array.IndexOf(b, a[index_a]);
				if (index_b > -1)
				{
					return true;
				}
			}
			return false;
		}

		public static T[] Concat<T>(this T[] x, T[] y)
		{
			if (x == null)
			{
				throw new ArgumentNullException("x");
			}
			if (y == null)
			{
				throw new ArgumentNullException("y");
			}
			int destinationIndex = x.Length;
			Array.Resize(ref x, x.Length + y.Length);
			Array.Copy(y, 0, x, destinationIndex, y.Length);
			return x;
		}

		public static int IndexOf<T>(this List<List<T>> InList, T InValue)
		{
			for (int i = 0; i < InList.Count; i++)
			{
				for (int j = 0; j < InList[i].Count; j++)
				{
					if (InList[i][j].Equals(InValue))
					{
						return i;
					}
				}
			}
			return -1;
		}

		public static T[] Fill<T>(int count, Func<int, T> ctor)
		{
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ctor(i);
			}
			return array;
		}

		public static void AddOrAppend<T, K>(this Dictionary<T, List<K>> dictionary, T key, K value)
		{
			List<K> value2;
			if (dictionary.TryGetValue(key, out value2))
			{
				value2.Add(value);
				return;
			}
			dictionary.Add(key, new List<K> { value });
		}

		public static void AddOrAppendRange<T, K>(this Dictionary<T, List<K>> dictionary, T key, List<K> value)
		{
			List<K> value2;
			if (dictionary.TryGetValue(key, out value2))
			{
				value2.AddRange(value);
			}
			else
			{
				dictionary.Add(key, value);
			}
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> knownKeys = new HashSet<TKey>();
			return source.Where((TSource x) => knownKeys.Add(keySelector(x)));
		}

		[Obsolete]
		public static string ToFormattedString<T>(this T[] t, string _delimiter)
		{
			return t.ToFormattedString(_delimiter, 0, -1);
		}

		[Obsolete]
		public static string ToFormattedString<T>(this T[] t, string _delimiter, int entriesPerLine, int maxEntries)
		{
			int num = ((maxEntries <= 0) ? t.Length : Mathf.Min(t.Length, maxEntries));
			if (t == null || num < 1)
			{
				return "Empty Array.";
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < num - 1; i++)
			{
				if (entriesPerLine > 0 && (i + 1) % entriesPerLine == 0)
				{
					stringBuilder.AppendLine(((t[i] != null) ? t[i].ToString() : "null") + _delimiter);
				}
				else
				{
					stringBuilder.Append(((t[i] != null) ? t[i].ToString() : "null") + _delimiter);
				}
			}
			stringBuilder.Append((t[num - 1] != null) ? t[num - 1].ToString() : "null");
			return stringBuilder.ToString();
		}

		[Obsolete]
		public static string ToFormattedString<T>(this List<T> t, string _delimiter)
		{
			return t.ToArray().ToFormattedString(_delimiter);
		}

		[Obsolete]
		public static string ToFormattedString<T>(this HashSet<T> t, string _delimiter)
		{
			return t.ToArray().ToFormattedString(_delimiter);
		}

		public static string ToString<TKey, TValue>(this Dictionary<TKey, TValue> dict)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<TKey, TValue> item in dict)
			{
				stringBuilder.AppendLine(string.Format("Key: {0}  Value: {1}", item.Key, item.Value));
			}
			return stringBuilder.ToString();
		}

		public static string ToString<T>(this IEnumerable<T> arr, string separator = ", ")
		{
			return string.Join(separator, arr.Select((T x) => x.ToString()).ToArray());
		}

		public static string ControlKeyString(char character)
		{
			switch (character)
			{
			case '⌘':
				return "Control";
			case '⇧':
				return "Shift";
			case '⌥':
				return "Alt";
			case '⎇':
				return "Alt";
			case '⌫':
				return "Delete";
			default:
				return character.ToString();
			}
		}

		[Obsolete("ColorWithString is deprecated. Use TryParseColor.")]
		public static bool ColorWithString(string value, out Color col)
		{
			col = Color.white;
			return TryParseColor(value, ref col);
		}

		public static bool TryParseColor(string value, ref Color col)
		{
			string valid = "01234567890.,";
			value = new string(value.Where((char c) => valid.Contains(c)).ToArray());
			string[] array = value.Split(',');
			if (array.Length < 4)
			{
				return false;
			}
			try
			{
				float r = float.Parse(array[0]);
				float g = float.Parse(array[1]);
				float b = float.Parse(array[2]);
				float a = float.Parse(array[3]);
				col.r = r;
				col.g = g;
				col.b = b;
				col.a = a;
			}
			catch
			{
				return false;
			}
			return true;
		}

		public static Vector3[] StringToVector3Array(string str)
		{
			List<Vector3> list = new List<Vector3>();
			str = str.Replace(" ", string.Empty);
			string[] array = str.Split('\n');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Contains("//"))
				{
					string[] array3 = text.Split(',');
					float result;
					float result2;
					float result3;
					if (array3.Length >= 3 && float.TryParse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture, out result) && float.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out result2) && float.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out result3))
					{
						list.Add(new Vector3(result, result2, result3));
					}
				}
			}
			return list.ToArray();
		}

		public static Vector2 DivideBy(this Vector2 v, Vector2 o)
		{
			return new Vector2(v.x / o.x, v.y / o.y);
		}

		public static Vector3 DivideBy(this Vector3 v, Vector3 o)
		{
			return new Vector3(v.x / o.x, v.y / o.y, v.z / o.z);
		}
	}
}
