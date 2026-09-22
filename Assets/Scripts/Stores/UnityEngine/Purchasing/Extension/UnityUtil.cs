using System;
using System.Collections;
using System.Collections.Generic;
using Uniject;

namespace UnityEngine.Purchasing.Extension
{
	[HideInInspector]
	[AddComponentMenu("")]
	internal class UnityUtil : MonoBehaviour, IUtil
	{
		private static List<Action> s_Callbacks = new List<Action>();

		private static volatile bool s_CallbacksPending;

		private static List<RuntimePlatform> s_PcControlledPlatforms = new List<RuntimePlatform>
		{
			RuntimePlatform.LinuxPlayer,
			RuntimePlatform.OSXEditor,
			RuntimePlatform.OSXPlayer,
			RuntimePlatform.WindowsEditor,
			RuntimePlatform.WindowsPlayer
		};

		private List<Action<bool>> pauseListeners = new List<Action<bool>>();

		public DateTime currentTime
		{
			get
			{
				return DateTime.Now;
			}
		}

		public string persistentDataPath
		{
			get
			{
				return Application.persistentDataPath;
			}
		}

		public string deviceUniqueIdentifier
		{
			get
			{
				return SystemInfo.deviceUniqueIdentifier;
			}
		}

		public string unityVersion
		{
			get
			{
				return Application.unityVersion;
			}
		}

		public string cloudProjectId
		{
			get
			{
				return Application.cloudProjectId;
			}
		}

		public string userId
		{
			get
			{
				return PlayerPrefs.GetString("unity.cloud_userid", string.Empty);
			}
		}

		public string gameVersion
		{
			get
			{
				return Application.version;
			}
		}

		public ulong sessionId
		{
			get
			{
				return ulong.Parse(PlayerPrefs.GetString("unity.player_sessionid", "0"));
			}
		}

		public RuntimePlatform platform
		{
			get
			{
				return Application.platform;
			}
		}

		public bool isEditor
		{
			get
			{
				return Application.isEditor;
			}
		}

		public string deviceModel
		{
			get
			{
				return SystemInfo.deviceModel;
			}
		}

		public string deviceName
		{
			get
			{
				return SystemInfo.deviceName;
			}
		}

		public DeviceType deviceType
		{
			get
			{
				return SystemInfo.deviceType;
			}
		}

		public string operatingSystem
		{
			get
			{
				return SystemInfo.operatingSystem;
			}
		}

		public int screenWidth
		{
			get
			{
				return Screen.width;
			}
		}

		public int screenHeight
		{
			get
			{
				return Screen.height;
			}
		}

		public float screenDpi
		{
			get
			{
				return Screen.dpi;
			}
		}

		public string screenOrientation
		{
			get
			{
				return Screen.orientation.ToString();
			}
		}

		public T[] GetAnyComponentsOfType<T>() where T : class
		{
			GameObject[] array = (GameObject[])Object.FindObjectsOfType(typeof(GameObject));
			List<T> list = new List<T>();
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
				foreach (MonoBehaviour monoBehaviour in components)
				{
					if (monoBehaviour is T)
					{
						list.Add(monoBehaviour as T);
					}
				}
			}
			return list.ToArray();
		}

		object IUtil.InitiateCoroutine(IEnumerator start)
		{
			return StartCoroutine(start);
		}

		void IUtil.InitiateCoroutine(IEnumerator start, int delay)
		{
			DelayedCoroutine(start, delay);
		}

		public void RunOnMainThread(Action runnable)
		{
			lock (s_Callbacks)
			{
				s_Callbacks.Add(runnable);
				s_CallbacksPending = true;
			}
		}

		public object GetWaitForSeconds(int seconds)
		{
			return new WaitForSeconds(seconds);
		}

		private void Start()
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}

		public static T FindInstanceOfType<T>() where T : MonoBehaviour
		{
			return (T)Object.FindObjectOfType(typeof(T));
		}

		public static T LoadResourceInstanceOfType<T>() where T : MonoBehaviour
		{
			return ((GameObject)Object.Instantiate(Resources.Load(typeof(T).ToString()))).GetComponent<T>();
		}

		public static bool PcPlatform()
		{
			return s_PcControlledPlatforms.Contains(Application.platform);
		}

		public static void DebugLog(string message, params object[] args)
		{
			try
			{
				Debug.Log(string.Format("com.ballatergames.debug - {0}", string.Format(message, args)));
			}
			catch (ArgumentNullException message2)
			{
				Debug.Log(message2);
			}
			catch (FormatException message3)
			{
				Debug.Log(message3);
			}
		}

		private IEnumerator DelayedCoroutine(IEnumerator coroutine, int delay)
		{
			yield return new WaitForSeconds(delay);
			StartCoroutine(coroutine);
		}

		private void Update()
		{
			if (!s_CallbacksPending)
			{
				return;
			}
			Action[] array;
			lock (s_Callbacks)
			{
				if (s_Callbacks.Count == 0)
				{
					return;
				}
				array = new Action[s_Callbacks.Count];
				s_Callbacks.CopyTo(array);
				s_Callbacks.Clear();
				s_CallbacksPending = false;
			}
			Action[] array2 = array;
			foreach (Action action in array2)
			{
				action();
			}
		}

		public void AddPauseListener(Action<bool> runnable)
		{
			pauseListeners.Add(runnable);
		}

		public void OnApplicationPause(bool paused)
		{
			foreach (Action<bool> pauseListener in pauseListeners)
			{
				pauseListener(paused);
			}
		}

		public bool IsClassOrSubclass(Type potentialBase, Type potentialDescendant)
		{
			return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
		}
	}
}
