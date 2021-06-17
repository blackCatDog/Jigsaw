using UnityEngine;

namespace Superpow
{
    public class Utils
    {
        public static int GetCatIndex(string catName)
        {
            for(int i = 0; i < GameData.instance.categories.Count; i++)
            {
                if (GameData.instance.categories[i].name == catName) return i;
            }

            return -1;
        }

        public static string GetTimeString(float t)
        {
            int h = Mathf.FloorToInt(t / 3600f);
            int m = Mathf.FloorToInt(t / 60f) % 60;
            int s = Mathf.FloorToInt(t) % 60;

            if (h == 0 && m == 0)
                return s.ToString("00") + (s == 1 ? " second" : " seconds");

            if (h == 0)
                return m.ToString("00") + ":" + s.ToString("00") + (m == 1 ? " min" : " mins");

            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00") + (h == 1 ? " hour" : " hours");
        }

        public static void RewardVideoAd()
        {
            int amount = ConfigController.Config.rewardedVideoAmount;
            CurrencyController.CreditBalance(amount);
            Toast.instance.ShowMessage(string.Format("You got {0} free rubies", amount), 2.5f);
            CUtils.SetActionTime("rewarded_video");
        }
    }
}