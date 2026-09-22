using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace UnityEngine.Advertisements.Editor
{
	[AddComponentMenu("")]
	internal sealed class Placeholder : MonoBehaviour
	{
		private Texture2D m_LandscapeTexture;

		private Texture2D m_PortraitTexture;

		private bool m_Showing;

		private string m_PlacementId;

		private bool m_AllowSkip;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private EventHandler<FinishEventArgs> OnFinish__BackingField;

		public event EventHandler<FinishEventArgs> OnFinish
		{
			add
			{
				EventHandler<FinishEventArgs> eventHandler = OnFinish__BackingField;
				EventHandler<FinishEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnFinish__BackingField, (EventHandler<FinishEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<FinishEventArgs> eventHandler = OnFinish__BackingField;
				EventHandler<FinishEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref OnFinish__BackingField, (EventHandler<FinishEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while ((object)eventHandler != eventHandler2);
			}
		}

		private static Texture2D TextureFromFile(string filePath)
		{
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(File.ReadAllBytes(filePath));
			return texture2D;
		}

		public void Load(string extensionPath)
		{
			m_LandscapeTexture = TextureFromFile(Path.Combine(extensionPath, "Editor/Resources/Editor/landscape.jpg"));
			m_PortraitTexture = TextureFromFile(Path.Combine(extensionPath, "Editor/Resources/Editor/portrait.jpg"));
		}

		public void Show(string placementId, bool allowSkip)
		{
			m_PlacementId = placementId;
			m_AllowSkip = allowSkip;
			m_Showing = true;
		}

		public void OnGUI()
		{
			if (m_Showing)
			{
				GUI.ModalWindow(0, new Rect(0f, 0f, Screen.width, Screen.height), ModalWindowFunction, string.Empty);
			}
		}

		private void ModalWindowFunction(int id)
		{
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), (Screen.width <= Screen.height) ? m_PortraitTexture : m_LandscapeTexture, ScaleMode.ScaleAndCrop);
			if (m_AllowSkip && GUI.Button(new Rect(20f, 20f, 150f, 50f), "Skip"))
			{
				m_Showing = false;
				EventHandler<FinishEventArgs> onFinish__BackingField = OnFinish__BackingField;
				if (onFinish__BackingField != null)
				{
					onFinish__BackingField(this, new FinishEventArgs(m_PlacementId, ShowResult.Skipped));
				}
			}
			if (GUI.Button(new Rect(Screen.width - 170, 20f, 150f, 50f), "Close"))
			{
				m_Showing = false;
				EventHandler<FinishEventArgs> onFinish__BackingField2 = OnFinish__BackingField;
				if (onFinish__BackingField2 != null)
				{
					onFinish__BackingField2(this, new FinishEventArgs(m_PlacementId, ShowResult.Finished));
				}
			}
		}
	}
}
