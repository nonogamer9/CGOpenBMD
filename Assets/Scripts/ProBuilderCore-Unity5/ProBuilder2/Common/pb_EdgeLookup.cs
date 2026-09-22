using System;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Common
{
	public class pb_EdgeLookup : IEquatable<pb_EdgeLookup>
	{
		public pb_Edge local;

		public pb_Edge common;

		public pb_EdgeLookup(pb_Edge common, pb_Edge local)
		{
			this.common = common;
			this.local = local;
		}

		public pb_EdgeLookup(int cx, int cy, int x, int y)
		{
			common = new pb_Edge(cx, cy);
			local = new pb_Edge(x, y);
		}

		public bool Equals(pb_EdgeLookup b)
		{
			return common.Equals((!object.ReferenceEquals(b, null)) ? b.common : pb_Edge.Empty);
		}

		public override bool Equals(object b)
		{
			pb_EdgeLookup pb_EdgeLookup2 = b as pb_EdgeLookup;
			return pb_EdgeLookup2 != null && common.Equals(pb_EdgeLookup2.common);
		}

		public override int GetHashCode()
		{
			return common.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", common.x, common.y);
		}

		public static IEnumerable<pb_EdgeLookup> GetEdgeLookup(IEnumerable<pb_Edge> edges, Dictionary<int, int> lookup)
		{
			return edges.Select((pb_Edge x) => new pb_EdgeLookup(new pb_Edge(lookup[x.x], lookup[x.y]), x));
		}
	}
}
