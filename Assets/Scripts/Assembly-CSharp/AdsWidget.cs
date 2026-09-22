using System;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class AdsWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject AdsBtnPrefab;

	[SerializeField]
	private Transform container;

	[SerializeField]
	private GameObject window;

	private static bool dontShowOnThisSession;

	private void Awake()
	{
		if (dontShowOnThisSession)
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			NetworkManager.ConfigsLoaded = (Action<string>)Delegate.Combine(NetworkManager.ConfigsLoaded, new Action<string>(OnConfigsLoaded));
		}
	}

	private void OnDestroy()
	{
		NetworkManager.ConfigsLoaded = (Action<string>)Delegate.Remove(NetworkManager.ConfigsLoaded, new Action<string>(OnConfigsLoaded));
	}

	private void OnConfigsLoaded(string jsonAppData)
	{
		JSONNode jSONNode = JSON.Parse(jsonAppData);
		if (!(jSONNode != null))
		{
			return;
		}
		foreach (JSONNode item in jSONNode["Apps"].AsArray)
		{
			if (!Application.identifier.Equals(item["Id"].Value))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(AdsBtnPrefab);
				gameObject.transform.SetParent(container);
				gameObject.transform.localScale = Vector3.one;
				StartCoroutine(HandleApp(item, gameObject));
			}
		}
		window.SetActive(true);
	}

	private IEnumerator HandleApp(JSONNode appNode, GameObject button)
	{
		button.GetComponent<Button>().onClick.AddListener(() =>
		{
			string value = appNode["UrlAndroid"].Value;
			if (DataModel.isIOS)
			{
				value = appNode["UrlIOS"].Value;
			}
			if (value != string.Empty)
			{
				Application.OpenURL(value);
			}
		});
		button.GetComponentInChildren<Text>().text = appNode["Name"];
		WWW www = new WWW(appNode["Icon"]);
		yield return www;
		Sprite sprite = Sprite.Create(www.texture, new Rect(0f, 0f, www.texture.width, www.texture.height), new Vector2(0f, 0f), 100f);
		button.GetComponentInChildren<Image>().sprite = sprite;
	}

	public void Close()
	{
		dontShowOnThisSession = true;
		base.gameObject.SetActive(false);
	}
}
