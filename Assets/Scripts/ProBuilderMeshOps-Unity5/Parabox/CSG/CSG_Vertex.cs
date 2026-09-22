using UnityEngine;

namespace Parabox.CSG
{
	internal struct CSG_Vertex
	{
		public Vector3 position;

		public Color color;

		public Vector3 normal;

		public Vector2 uv;

		public CSG_Vertex(Vector3 position, Vector3 normal, Vector2 uv, Color color)
		{
			this.position = position;
			this.normal = normal;
			this.uv = uv;
			this.color = color;
		}

		public void Flip()
		{
			normal *= -1f;
		}

		public static CSG_Vertex Interpolate(CSG_Vertex a, CSG_Vertex b, float t)
		{
			return new CSG_Vertex
			{
				position = Vector3.Lerp(a.position, b.position, t),
				normal = Vector3.Lerp(a.normal, b.normal, t),
				uv = Vector2.Lerp(a.uv, b.uv, t),
				color = (a.color + b.color) / 2f
			};
		}
	}
}
