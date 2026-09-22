using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdsController : MonoBehaviour
{
	public static Action RewardedVideoFinished;

	public static Action RewardedVideoFailed;

	public static void ShowAd()
	{
		Debug.Log("UnityAds call  isReady = " + Advertisement.IsReady());
		if (Advertisement.IsReady())
		{
			AdsController.instance.isShowingAdsNow = true;
			AdsController.lastAdsShownTime = Time.time;
			Advertisement.Show();
		}
	}

	public static void ShowRewardedAd()
	{
		if (Advertisement.IsReady("rewardedVideo"))
		{
			AdsController.instance.isShowingAdsNow = true;
			AdsController.lastAdsShownTime = Time.time;
			ShowOptions showOptions = new ShowOptions();
			showOptions.resultCallback = HandleShowResult;
			ShowOptions showOptions2 = showOptions;
			Advertisement.Show("rewardedVideo", showOptions2);
		}
		else if (RewardedVideoFailed != null)
		{
			RewardedVideoFailed();
		}
	}

	private static void HandleShowResult(ShowResult result)
	{
		switch (result)
		{
		case ShowResult.Finished:
			Debug.Log("The ad was successfully shown.");
			AdsController.lastAdsShownTime = Time.time;
			if (RewardedVideoFinished != null)
			{
				RewardedVideoFinished();
			}
			break;
		case ShowResult.Skipped:
			Debug.Log("The ad was skipped before reaching the end.");
			break;
		case ShowResult.Failed:
			Debug.LogError("The ad failed to be shown.");
			if (RewardedVideoFailed != null)
			{
				RewardedVideoFailed();
			}
			break;
		}
		AdsController.instance.isShowingAdsNow = false;
	}

	public static bool IsRewardedReady()
	{
		return Advertisement.IsReady("rewardedVideo");
	}

	public static bool IsSimpleVideoReady()
	{
		return Advertisement.IsReady();
	}
}
