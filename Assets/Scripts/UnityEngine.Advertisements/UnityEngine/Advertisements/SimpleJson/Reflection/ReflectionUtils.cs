using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace UnityEngine.Advertisements.SimpleJson.Reflection
{
	[GeneratedCode("reflection-utils", "1.0.0")]
	internal class ReflectionUtils
	{
		public delegate object GetDelegate(object source);

		public delegate void SetDelegate(object source, object value);

		public delegate object ConstructorDelegate(params object[] args);

		public delegate TValue ThreadSafeDictionaryValueFactory<TKey, TValue>(TKey key);

		public sealed class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
		{
			private readonly object _lock = new object();

			private readonly ThreadSafeDictionaryValueFactory<TKey, TValue> _valueFactory;

			private Dictionary<TKey, TValue> _dictionary;

			public ICollection<TKey> Keys
			{
				get
				{
					return _dictionary.Keys;
				}
			}

			public ICollection<TValue> Values
			{
				get
				{
					return _dictionary.Values;
				}
			}

			public TValue this[TKey key]
			{
				get
				{
					return Get(key);
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public int Count
			{
				get
				{
					return _dictionary.Count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public ThreadSafeDictionary(ThreadSafeDictionaryValueFactory<TKey, TValue> valueFactory)
			{
				_valueFactory = valueFactory;
			}

			private TValue Get(TKey key)
			{
				if (_dictionary == null)
				{
					return AddValue(key);
				}
				TValue value;
				if (!_dictionary.TryGetValue(key, out value))
				{
					return AddValue(key);
				}
				return value;
			}

			private TValue AddValue(TKey key)
			{
				TValue val = _valueFactory(key);
				lock (_lock)
				{
					if (_dictionary == null)
					{
						_dictionary = new Dictionary<TKey, TValue>();
						_dictionary[key] = val;
					}
					else
					{
						TValue value;
						if (_dictionary.TryGetValue(key, out value))
						{
							return value;
						}
						Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(_dictionary);
						dictionary[key] = val;
						_dictionary = dictionary;
					}
				}
				return val;
			}

			public void Add(TKey key, TValue value)
			{
				throw new NotImplementedException();
			}

			public bool ContainsKey(TKey key)
			{
				return _dictionary.ContainsKey(key);
			}

			public bool Remove(TKey key)
			{
				throw new NotImplementedException();
			}

			public bool TryGetValue(TKey key, out TValue value)
			{
				value = this[key];
				return true;
			}

			public void Add(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public bool Contains(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public bool Remove(KeyValuePair<TKey, TValue> item)
			{
				throw new NotImplementedException();
			}

			public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				return _dictionary.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _dictionary.GetEnumerator();
			}
		}

		private static readonly object[] EmptyObjects = new object[0];

		public static Attribute GetAttribute(MemberInfo info, Type type)
		{
			if (info == null || type == null || !Attribute.IsDefined(info, type))
			{
				return null;
			}
			return Attribute.GetCustomAttribute(info, type);
		}

		public static Attribute GetAttribute(Type objectType, Type attributeType)
		{
			if (objectType == null || attributeType == null || !Attribute.IsDefined(objectType, attributeType))
			{
				return null;
			}
			return Attribute.GetCustomAttribute(objectType, attributeType);
		}

		public static Type[] GetGenericTypeArguments(Type type)
		{
			return type.GetGenericArguments();
		}

		public static bool IsTypeGenericeCollectionInterface(Type type)
		{
			if (!type.IsGenericType)
			{
				return false;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			return genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(ICollection<>) || genericTypeDefinition == typeof(IEnumerable<>);
		}

		public static bool IsAssignableFrom(Type type1, Type type2)
		{
			return type1.IsAssignableFrom(type2);
		}

		public static bool IsTypeDictionary(Type type)
		{
			if (typeof(IDictionary).IsAssignableFrom(type))
			{
				return true;
			}
			if (!type.IsGenericType)
			{
				return false;
			}
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			return genericTypeDefinition == typeof(IDictionary<, >);
		}

		public static bool IsNullableType(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public static object ToNullableType(object obj, Type nullableType)
		{
			return (obj != null) ? Convert.ChangeType(obj, Nullable.GetUnderlyingType(nullableType), CultureInfo.InvariantCulture) : null;
		}

		public static bool IsValueType(Type type)
		{
			return type.IsValueType;
		}

		public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
		{
			return type.GetConstructors();
		}

		public static ConstructorInfo GetConstructorInfo(Type type, params Type[] argsType)
		{
			IEnumerable<ConstructorInfo> constructors = GetConstructors(type);
			foreach (ConstructorInfo item in constructors)
			{
				ParameterInfo[] parameters = item.GetParameters();
				if (argsType.Length != parameters.Length)
				{
					continue;
				}
				int num = 0;
				bool flag = true;
				ParameterInfo[] parameters2 = item.GetParameters();
				foreach (ParameterInfo parameterInfo in parameters2)
				{
					if (parameterInfo.ParameterType != argsType[num])
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				return item;
			}
			return null;
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static IEnumerable<FieldInfo> GetFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static MethodInfo GetGetterMethodInfo(PropertyInfo propertyInfo)
		{
			return propertyInfo.GetGetMethod(true);
		}

		public static MethodInfo GetSetterMethodInfo(PropertyInfo propertyInfo)
		{
			return propertyInfo.GetSetMethod(true);
		}

		public static ConstructorDelegate GetContructor(ConstructorInfo constructorInfo)
		{
			return GetConstructorByReflection(constructorInfo);
		}

		public static ConstructorDelegate GetContructor(Type type, params Type[] argsType)
		{
			return GetConstructorByReflection(type, argsType);
		}

		public static ConstructorDelegate GetConstructorByReflection(ConstructorInfo constructorInfo)
		{
			return (object[] args) => constructorInfo.Invoke(args);
		}

		public static ConstructorDelegate GetConstructorByReflection(Type type, params Type[] argsType)
		{
			ConstructorInfo constructorInfo = GetConstructorInfo(type, argsType);
			return (constructorInfo != null) ? GetConstructorByReflection(constructorInfo) : null;
		}

		public static GetDelegate GetGetMethod(PropertyInfo propertyInfo)
		{
			return GetGetMethodByReflection(propertyInfo);
		}

		public static GetDelegate GetGetMethod(FieldInfo fieldInfo)
		{
			return GetGetMethodByReflection(fieldInfo);
		}

		public static GetDelegate GetGetMethodByReflection(PropertyInfo propertyInfo)
		{
			MethodInfo methodInfo = GetGetterMethodInfo(propertyInfo);
			return (object source) => methodInfo.Invoke(source, EmptyObjects);
		}

		public static GetDelegate GetGetMethodByReflection(FieldInfo fieldInfo)
		{
			return (object source) => fieldInfo.GetValue(source);
		}

		public static SetDelegate GetSetMethod(PropertyInfo propertyInfo)
		{
			return GetSetMethodByReflection(propertyInfo);
		}

		public static SetDelegate GetSetMethod(FieldInfo fieldInfo)
		{
			return GetSetMethodByReflection(fieldInfo);
		}

		public static SetDelegate GetSetMethodByReflection(PropertyInfo propertyInfo)
		{
			MethodInfo methodInfo = GetSetterMethodInfo(propertyInfo);
			return (object source, object value) =>
			{
				methodInfo.Invoke(source, new object[1] { value });
			};
		}

		public static SetDelegate GetSetMethodByReflection(FieldInfo fieldInfo)
		{
			return (object source, object value) =>
			{
				fieldInfo.SetValue(source, value);
			};
		}
	}
}
