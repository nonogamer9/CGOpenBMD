using System;
using System.CodeDom.Compiler;

namespace UnityEngine.Advertisements.SimpleJson
{
	[GeneratedCode("simple-json", "1.0.0")]
	internal interface IJsonSerializerStrategy
	{
		bool TrySerializeNonPrimitiveObject(object input, out object output);

		object DeserializeObject(object value, Type type);
	}
}
