using System;
using System.Collections.Generic;
using UnityEngine;

namespace DG.DemiLib.External
{
	public class DeHierarchyComponent : MonoBehaviour
	{
		public enum HColor
		{
			None = 0,
			Blue = 1,
			Green = 2,
			Orange = 3,
			Purple = 4,
			Red = 5,
			Yellow = 6,
			BrightGrey = 7,
			DarkGrey = 8,
			Black = 9,
			White = 10
		}

		public enum IcoType
		{
			Dot = 0,
			Star = 1,
			Cog = 2,
			Comment = 3,
			UI = 4,
			Play = 5,
			Heart = 6,
			Skull = 7,
			Camera = 8
		}

		[Serializable]
		public class CustomizedItem
		{
			public GameObject gameObject;

			public HColor hColor = HColor.BrightGrey;

			public IcoType icoType;

			public CustomizedItem(GameObject gameObject, HColor hColor)
			{
				this.gameObject = gameObject;
				this.hColor = hColor;
			}

			public CustomizedItem(GameObject gameObject, IcoType icoType)
			{
				this.gameObject = gameObject;
				this.icoType = icoType;
			}

			public Color GetColor()
			{
				switch (hColor)
				{
				case HColor.Blue:
					return new Color(0.2145329f, 0.4501492f, 31f / 34f, 1f);
				case HColor.Green:
					return new Color(0.05060553f, 117f / 136f, 0.2237113f, 1f);
				case HColor.Orange:
					return new Color(65f / 68f, 0.4471125f, 0.05622837f, 1f);
				case HColor.Purple:
					return new Color(0.907186f, 0.05406574f, 125f / 136f, 1f);
				case HColor.Red:
					return new Color(125f / 136f, 0.1617312f, 0.07434041f, 1f);
				case HColor.Yellow:
					return new Color(1f, 0.853854f, 0.03676468f, 1f);
				case HColor.BrightGrey:
					return new Color(0.6470588f, 0.6470588f, 0.6470588f, 1f);
				case HColor.DarkGrey:
					return new Color(0.3308824f, 0.3308824f, 0.3308824f, 1f);
				case HColor.Black:
					return Color.black;
				case HColor.White:
					return Color.white;
				default:
					return Color.white;
				}
			}
		}

		public List<CustomizedItem> customizedItems = new List<CustomizedItem>();

		public List<int> MissingItemsIndexes()
		{
			List<int> list = null;
			for (int num = customizedItems.Count - 1; num > -1; num--)
			{
				if (customizedItems[num].gameObject == null)
				{
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(num);
				}
			}
			return list;
		}

		public void StoreItemColor(GameObject go, HColor hColor)
		{
			for (int i = 0; i < customizedItems.Count; i++)
			{
				if (!(customizedItems[i].gameObject != go))
				{
					customizedItems[i].hColor = hColor;
					return;
				}
			}
			customizedItems.Add(new CustomizedItem(go, hColor));
		}

		public void StoreItemIcon(GameObject go, IcoType icoType)
		{
			for (int i = 0; i < customizedItems.Count; i++)
			{
				if (!(customizedItems[i].gameObject != go))
				{
					customizedItems[i].icoType = icoType;
					return;
				}
			}
			customizedItems.Add(new CustomizedItem(go, icoType));
		}

		public bool RemoveItemData(GameObject go)
		{
			int num = -1;
			for (int i = 0; i < customizedItems.Count; i++)
			{
				if (customizedItems[i].gameObject == go)
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				customizedItems.RemoveAt(num);
				return true;
			}
			return false;
		}

		public CustomizedItem GetItem(GameObject go)
		{
			for (int i = 0; i < customizedItems.Count; i++)
			{
				if (customizedItems[i].gameObject == go)
				{
					return customizedItems[i];
				}
			}
			return null;
		}
	}
}
