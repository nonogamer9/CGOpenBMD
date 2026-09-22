using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uniject;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.Purchasing
{
	internal class UIFakeStore : FakeStore
	{
		protected class DialogRequest
		{
			public string QueryText;

			public string OkayButtonText;

			public string CancelButtonText;

			public List<string> Options;

			public Action<bool, int> Callback;
		}

		protected class LifecycleNotifier : MonoBehaviour
		{
			public Action OnDestroyCallback;

			private void OnDestroy()
			{
				if (OnDestroyCallback != null)
				{
					OnDestroyCallback();
				}
			}
		}

		private const string EnvironmentDescriptionPostfix = "\n\n[Environment: FakeStore]";

		private const string SuccessString = "Success";

		private const int RetrieveProductsDescriptionCount = 2;

		private DialogRequest m_CurrentDialog;

		private int m_LastSelectedDropdownIndex;

		private GameObject UIFakeStoreCanvasPrefab;

		private Canvas m_Canvas;

		private GameObject m_EventSystem;

		private string m_ParentGameObjectPath;

		private IUtil m_Util;

		public UIFakeStore()
		{
		}

		public UIFakeStore(IUtil util)
		{
			m_Util = util;
		}

		protected override bool StartUI<T>(object model, DialogType dialogType, Action<bool, T> callback)
		{
			List<string> list = new List<string>();
			list.Add("Success");
			foreach (T value in Enum.GetValues(typeof(T)))
			{
				list.Add(value.ToString());
			}
			Action<bool, int> callback2 = (bool result, int codeValue) =>
			{
				T arg = (T)(object)codeValue;
				callback(result, arg);
			};
			string queryText = null;
			string okayButtonText = null;
			string text = null;
			switch (dialogType)
			{
			case DialogType.Purchase:
				queryText = CreatePurchaseQuestion((ProductDefinition)model);
				okayButtonText = ((UIMode != FakeStoreUIMode.DeveloperUser) ? "Buy" : "OK");
				break;
			case DialogType.RetrieveProducts:
				queryText = CreateRetrieveProductsQuestion((ReadOnlyCollection<ProductDefinition>)model);
				okayButtonText = "OK";
				break;
			default:
				Debug.LogError("Unrecognized DialogType " + dialogType);
				break;
			}
			text = "Cancel";
			return StartUI(queryText, okayButtonText, text, list, callback2);
		}

		private bool StartUI(string queryText, string okayButtonText, string cancelButtonText, List<string> options, Action<bool, int> callback)
		{
			if (IsShowingDialog())
			{
				return false;
			}
			DialogRequest dialogRequest = new DialogRequest();
			dialogRequest.QueryText = queryText;
			dialogRequest.OkayButtonText = okayButtonText;
			dialogRequest.CancelButtonText = cancelButtonText;
			dialogRequest.Options = options;
			dialogRequest.Callback = callback;
			m_CurrentDialog = dialogRequest;
			InstantiateDialog();
			return true;
		}

		private void InstantiateDialog()
		{
			if (m_CurrentDialog == null)
			{
				Debug.LogError(string.Concat(this, " requires m_CurrentDialog. Not showing dialog."));
				return;
			}
			if (UIFakeStoreCanvasPrefab == null)
			{
				UIFakeStoreCanvasPrefab = Resources.Load("UIFakeStoreCanvas") as GameObject;
			}
			Canvas component = UIFakeStoreCanvasPrefab.GetComponent<Canvas>();
			m_Canvas = Object.Instantiate(component);
			LifecycleNotifier lifecycleNotifier = m_Canvas.gameObject.AddComponent<LifecycleNotifier>();
			lifecycleNotifier.OnDestroyCallback = () =>
			{
				m_CurrentDialog = null;
			};
			m_ParentGameObjectPath = m_Canvas.name + "/Panel/";
			if (Object.FindObjectOfType<EventSystem>() == null)
			{
				m_EventSystem = new GameObject("EventSystem", typeof(EventSystem));
				m_EventSystem.AddComponent<StandaloneInputModule>();
				m_EventSystem.transform.parent = m_Canvas.transform;
			}
			GameObject gameObject = GameObject.Find(m_ParentGameObjectPath + "HeaderText");
			Text component2 = gameObject.GetComponent<Text>();
			component2.text = m_CurrentDialog.QueryText;
			Text okayButtonText = GetOkayButtonText();
			okayButtonText.text = m_CurrentDialog.OkayButtonText;
			Text cancelButtonText = GetCancelButtonText();
			cancelButtonText.text = m_CurrentDialog.CancelButtonText;
			GetDropdown().options.Clear();
			foreach (string option in m_CurrentDialog.Options)
			{
				GetDropdown().options.Add(new Dropdown.OptionData(option));
			}
			if (m_CurrentDialog.Options.Count > 0)
			{
				m_LastSelectedDropdownIndex = 0;
			}
			GetDropdown().RefreshShownValue();
			GetOkayButton().onClick.AddListener(() =>
			{
				OkayButtonClicked();
			});
			GetCancelButton().onClick.AddListener(() =>
			{
				CancelButtonClicked();
			});
			GetDropdown().onValueChanged.AddListener((int selectedItem) =>
			{
				DropdownValueChanged(selectedItem);
			});
			if (UIMode == FakeStoreUIMode.StandardUser)
			{
				GetDropdown().onValueChanged.RemoveAllListeners();
				Object.Destroy(GetDropdownContainerGameObject());
			}
			else if (UIMode == FakeStoreUIMode.DeveloperUser)
			{
				GetCancelButton().onClick.RemoveAllListeners();
				Object.Destroy(GetCancelButtonGameObject());
			}
		}

		private string CreatePurchaseQuestion(ProductDefinition definition)
		{
			return "Do you want to Purchase " + definition.id + "?\n\n[Environment: FakeStore]";
		}

		private string CreateRetrieveProductsQuestion(ReadOnlyCollection<ProductDefinition> definitions)
		{
			string text = "Do you want to initialize purchasing for products {";
			text += string.Join(", ", (from pid in definitions.Take(2)
				select pid.id).ToArray());
			if (definitions.Count > 2)
			{
				text += ", ...";
			}
			return text + "}?\n\n[Environment: FakeStore]";
		}

		private Button GetOkayButton()
		{
			return GameObject.Find(m_ParentGameObjectPath + "Button1").GetComponent<Button>();
		}

		private Button GetCancelButton()
		{
			GameObject gameObject = GameObject.Find(m_ParentGameObjectPath + "Button2");
			if (gameObject != null)
			{
				return gameObject.GetComponent<Button>();
			}
			return null;
		}

		private GameObject GetCancelButtonGameObject()
		{
			return GameObject.Find(m_ParentGameObjectPath + "Button2");
		}

		private Text GetOkayButtonText()
		{
			return GameObject.Find(m_ParentGameObjectPath + "Button1/Text").GetComponent<Text>();
		}

		private Text GetCancelButtonText()
		{
			return GameObject.Find(m_ParentGameObjectPath + "Button2/Text").GetComponent<Text>();
		}

		private Dropdown GetDropdown()
		{
			GameObject gameObject = GameObject.Find(m_ParentGameObjectPath + "Panel2/Panel3/Dropdown");
			if (gameObject != null)
			{
				return gameObject.GetComponent<Dropdown>();
			}
			return null;
		}

		private GameObject GetDropdownContainerGameObject()
		{
			return GameObject.Find(m_ParentGameObjectPath + "Panel2");
		}

		private void OkayButtonClicked()
		{
			bool arg = false;
			if (m_LastSelectedDropdownIndex == 0 || UIMode != FakeStoreUIMode.DeveloperUser)
			{
				arg = true;
			}
			int arg2 = Math.Max(0, m_LastSelectedDropdownIndex - 1);
			m_CurrentDialog.Callback(arg, arg2);
			CloseDialog();
		}

		private void CancelButtonClicked()
		{
			int arg = Math.Max(0, m_LastSelectedDropdownIndex - 1);
			m_CurrentDialog.Callback(false, arg);
			CloseDialog();
		}

		private void DropdownValueChanged(int selectedItem)
		{
			m_LastSelectedDropdownIndex = selectedItem;
		}

		private void CloseDialog()
		{
			m_CurrentDialog = null;
			GetOkayButton().onClick.RemoveAllListeners();
			if ((bool)GetCancelButton())
			{
				GetCancelButton().onClick.RemoveAllListeners();
			}
			if (GetDropdown() != null)
			{
				GetDropdown().onValueChanged.RemoveAllListeners();
			}
			Object.Destroy(m_Canvas.gameObject);
		}

		public bool IsShowingDialog()
		{
			return m_CurrentDialog != null;
		}
	}
}
