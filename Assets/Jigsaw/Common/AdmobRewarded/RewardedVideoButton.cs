using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardedVideoButton : MonoBehaviour
{

    public void OnClick()
    {
        if (IsAvailableToShow())
        {
#if UNITY_EDITOR
            Superpow.Utils.RewardVideoAd();
#else
            AdmobController.instance.ShowRewardBasedVideo();
#endif
        }
        else
        {
            Toast.instance.ShowMessage("Ad is not available at the moment, please wait..");
        }
        
        Sound.instance.PlayButton();
    }

    public bool IsAvailableToShow()
    {
        return IsActionAvailable() && IsAdAvailable();
    }

    private bool IsActionAvailable()
    {
        return CUtils.IsActionAvailable("rewarded_video", ConfigController.Config.rewardedVideoPeriod);
    }

    private bool IsAdAvailable()
    {
        if (AdmobController.instance.rewardBasedVideo == null) return false;
        bool isLoaded = AdmobController.instance.rewardBasedVideo.IsLoaded();
        return isLoaded;
    }
}
