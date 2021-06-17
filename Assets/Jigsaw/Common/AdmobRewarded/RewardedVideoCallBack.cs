using GoogleMobileAds.Api;
using UnityEngine;

public class RewardedVideoCallBack : MonoBehaviour {

    private void Start()
    {
        Timer.Schedule(this, 0.5f, AddEvents);
    }

    private void AddEvents()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (AdmobController.instance.rewardBasedVideo != null)
        {
            AdmobController.instance.rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        }
#endif
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        Superpow.Utils.RewardVideoAd();
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (AdmobController.instance.rewardBasedVideo != null)
        {
            AdmobController.instance.rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
        }
#endif
    }
}
