using System;
using System.Collections;
using UnityEngine;

public class AdsController : MonoBehaviour
{
	public static AdsController instance;

	public bool isShowingAdsNow;

	[SerializeField]
	private AppodealAdsController appodeal;

	[SerializeField]
	private AdMobController admob;

	public static float lastAdsShownTime = -1f;

	public static Action RewardedVideoFinished;

	public static Action RewardedVideoFailed;

	private static int _AdsPauseTime = 140;

	public static int AdsPauseTime
	{
		get
		{
			return _AdsPauseTime;
		}
		set
		{
			_AdsPauseTime = value;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void Start()
	{
		UnityAdsController.RewardedVideoFinished = (Action)Delegate.Combine(UnityAdsController.RewardedVideoFinished, new Action(OnRewardedVideoFinished));
		UnityAdsController.RewardedVideoFailed = (Action)Delegate.Combine(UnityAdsController.RewardedVideoFailed, new Action(OnRewardedVideoFailed));
	}

	private void OnDestroy()
	{
		UnityAdsController.RewardedVideoFinished = (Action)Delegate.Remove(UnityAdsController.RewardedVideoFinished, new Action(OnRewardedVideoFinished));
		UnityAdsController.RewardedVideoFailed = (Action)Delegate.Remove(UnityAdsController.RewardedVideoFailed, new Action(OnRewardedVideoFailed));
	}

	public void ShowTestUnityAds()
	{
		UnityAdsController.ShowAd();
	}

	public void ShowAdsOnGameFinished(float delay = 0f)
	{
		if (!LocalStore.isAdsDisabled && !(Time.time - lastAdsShownTime < (float)AdsPauseTime))
		{
			UnityAdsController.ShowAd();
		}
	}

	public void ShowAdsOnMenuBtnsClick()
	{
		ShowAdsOnGameFinished();
	}

	public void ShowInterstitialAd(float delay = 0f)
	{
		if (!LocalStore.isAdsDisabled)
		{
			StartCoroutine(ShowInterstitialAdCRT(delay));
		}
	}

	private IEnumerator ShowInterstitialAdCRT(float delay)
	{
		yield return new WaitForSeconds(delay);
		UnityAdsController.ShowAd();
	}

	public void ShowRewardedVideo(float delay = 0f)
	{
		StartCoroutine(ShowNonSkipVideoCRT(delay));
	}

	private IEnumerator ShowNonSkipVideoCRT(float delay)
	{
		yield return new WaitForSeconds(delay);
		UnityAdsController.ShowRewardedAd();
	}

	private void OnRewardedVideoFinished()
	{
		if (RewardedVideoFinished != null)
		{
			RewardedVideoFinished();
		}
	}

	private void OnRewardedVideoFailed()
	{
		if (RewardedVideoFailed != null)
		{
			RewardedVideoFailed();
		}
	}
}
