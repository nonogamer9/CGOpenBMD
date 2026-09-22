using System.Collections.Generic;
using UnityEngine;

namespace DG.DemiLib
{
	public interface IEditorGUINode
	{
		string id { get; set; }

		Vector2 guiPosition { get; set; }

		List<string> connectedNodesIds { get; }
	}
}
