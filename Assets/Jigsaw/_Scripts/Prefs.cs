using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefs {

    public static string CurrentCategory
    {
        get { return PlayerPrefs.GetString("current_category", GameData.instance.categories[0].name); }
        set { PlayerPrefs.SetString("current_category", value); }
    }

    public static int CurrentPhoto
    {
        get { return PlayerPrefs.GetInt("current_photo"); }
        set { PlayerPrefs.SetInt("current_photo", value); }
    }

    public static int CurrentDiff
    {
        get { return PlayerPrefs.GetInt("current_diff"); }
        set { PlayerPrefs.SetInt("current_diff", value); }
    }

    public static int LastPhoto
    {
        get { return PlayerPrefs.GetInt("last_photo_" + CurrentCategory); }
        set { PlayerPrefs.SetInt("last_photo_" + CurrentCategory, value); }
    }

    public static string GetPrefKey(string category, int photo, int diff)
    {
        return category + "_" + photo + "_" + diff;
    }

    public static string GetStatus(string category, int photo, int diff)
    {
        string key = "status_" + GetPrefKey(category, photo, diff);
        if (!PlayerPrefs.HasKey(key)) return Const.STATUS_NOTSTARTED;

        return PlayerPrefs.GetString(key);
    }

    public static void SetStatus(string category, int photo, int diff, string status)
    {
        PlayerPrefs.SetString("status_" + GetPrefKey(category, photo, diff), status);
    }

    public static string GetCurrentStatus()
    {
        return GetStatus(CurrentCategory, CurrentPhoto, CurrentDiff);
    }

    public static void SetCurrentStatus(string status)
    {
        SetStatus(CurrentCategory, CurrentPhoto, CurrentDiff, status);
    }

    public static string GetProgress(string category, int photo, int diff)
    {
        string key = "progress_" + GetPrefKey(category, photo, diff);
        return PlayerPrefs.GetString(key);
    }

    public static void SetProgress(string category, int photo, int diff, string progress)
    {
        PlayerPrefs.SetString("progress_" + GetPrefKey(category, photo, diff), progress);
    }

    public static string GetCurrentProgress()
    {
        return GetProgress(CurrentCategory, CurrentPhoto, CurrentDiff);
    }

    public static void SetCurrentProgress(string progress)
    {
        SetProgress(CurrentCategory, CurrentPhoto, CurrentDiff, progress);
    }

    public static bool IsLocked(string category, int photo)
    {
        string key = "isLocked_" + category + "_" + photo;
        return !PlayerPrefs.HasKey(key);
    }

    public static void SetUnlocked(string category, int photo)
    {
        string key = "isLocked_" + category + "_" + photo;
        PlayerPrefs.SetString(key, "false");
    }

    public static bool IsPhotoCompleted(string category, int photo)
    {
        for(int i = 0; i < 4; i++)
        {
            var status = GetStatus(category, photo, i);
            if (status != Const.STATUS_COMPLETE) return false;
        }
        return true;
    }

    public static int GameTime
    {
        get { return PlayerPrefs.GetInt(GetPrefKey(CurrentCategory, CurrentPhoto, CurrentDiff)); }
        set { PlayerPrefs.SetInt(GetPrefKey(CurrentCategory, CurrentPhoto, CurrentDiff), value); }
    }

    public static bool IsRewarded
    {
        get { return PlayerPrefs.GetInt("is_rewarded" + GetPrefKey(CurrentCategory, CurrentPhoto, CurrentDiff)) == 1; }
        set { PlayerPrefs.SetInt("is_rewarded" + GetPrefKey(CurrentCategory, CurrentPhoto, CurrentDiff), value ? 1 : 0); }
    }

    public static void BuyPack(PackData packData)
    {
        var boughtPacks = GetBoughtPacks();
        boughtPacks.RemoveAll(x => x.name == packData.name);

        boughtPacks.Add(packData);
        var json = JsonUtility.ToJson(new PackDatas() { packDatas = boughtPacks });
        PlayerPrefs.SetString("pack_bought", json);
    }

    public static bool IsPackBought(string packName)
    {
        return GetBoughtPack(packName) != null;
    }

    public static List<PackData> GetBoughtPacks()
    {
        if (!PlayerPrefs.HasKey("pack_bought")) return new List<PackData>();
        return JsonUtility.FromJson<PackDatas>(PlayerPrefs.GetString("pack_bought")).packDatas;
    }

    public static PackData GetBoughtPack(string packName)
    {
        var boughtPacks = GetBoughtPacks();
        return boughtPacks.Find(x => x.name == packName);
    }
}
