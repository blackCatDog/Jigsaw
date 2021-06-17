#pragma warning disable 0618
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CheckServer : MonoBehaviour {

    public InputField packJsonLink, fromIndexInput;
    public Text resultText, progress;
    public Button checkBtn, checkBtn2;

    List<PackData> packList;
    private string jsonFileContent = null;

    private void Awake()
    {
        if (Application.isEditor)
            Application.runInBackground = true;
    }

    private void Start()
    {
        packJsonLink.text = PlayerPrefs.GetString("packJsonLink", "");
    }

    public void Check()
    {
        if (string.IsNullOrEmpty(packJsonLink.text.Trim()))
        {
            resultText.text = "Please put the json link in the field";
            return;
        }

        checkBtn.interactable = false;
        ClearConsole();
        resultText.text = "";
        progress.text = "Loading json content ...";
        StartCoroutine(GetTextFromServer());
    }

    public void Check2()
    {
        ClearConsole();
        if (string.IsNullOrEmpty(jsonFileContent))
        {
            resultText.text = "You need to load json file first";
        }
        else
        {
            checkBtn2.interactable = false;
            CheckContent(jsonFileContent);
        }
    }

    public void LoadJsonFile()
    {
#if UNITY_EDITOR
        var path = EditorUtility.OpenFilePanel(
                "Load a json file",
                "",
                "json");

        if (path.Length != 0)
        {
            jsonFileContent = File.ReadAllText(path);
        }
#endif
    }

    private IEnumerator GetTextFromServer()
    {
        var url = packJsonLink.text.Trim();
        WWW www = new WWW(url);
        yield return www;

        progress.text = "";

        if (www.isDone && string.IsNullOrEmpty(www.error))
        {
            PlayerPrefs.SetString("packJsonLink", url);
            CheckContent(www.text);
        }
        else
        {
            resultText.text = "Can't get json content from server";

            checkBtn.interactable = true;
        }
    }

    private void CheckContent(string json)
    {
        PackDatas data = null;
        try
        {
            data = JsonUtility.FromJson<PackDatas>(json);
        }
        catch (Exception)
        {
            checkBtn.interactable = true;
            checkBtn2.interactable = true;
            resultText.text = "Json content is incorrect format";
            return;
        }

        if (data.packDatas.Count == 0)
        {
            resultText.text = "Pack list is empty or incorrect format";
            checkBtn.interactable = true;
            checkBtn2.interactable = true;
            return;
        }

        packList = data.packDatas;

        if (CheckDuplicatePack())
        {
            resultText.text = "Duplicated pack names. Check Window->Console for more details";
            checkBtn.interactable = true;
            checkBtn2.interactable = true;
            return;
        }

        string from = fromIndexInput.text;
        if (string.IsNullOrEmpty(from))
        {
            packIndex = 0;
        }
        else
        {
            int fromInt = int.Parse(from);
            packIndex = Mathf.Max(0, fromInt);

            if (packIndex >= packList.Count)
            {
                resultText.text = "From index can't be larger than " + (packList.Count - 1);
                checkBtn.interactable = true;
                checkBtn2.interactable = true;
                return;
            }
        }
        
        CheckPackData();
    }

    private bool CheckDuplicatePack()
    {
        bool duplicate = false;
        for(int i = 0; i < packList.Count; i++)
        {
            var pack = packList[i];
            for(int j = i + 1; j < packList.Count; j++)
            {
                var pack2 = packList[j];
                if (pack.name == pack2.name)
                {
                    duplicate = true;
                    Debug.Log("Duplicated name:" + pack.name);
                    break;
                }
            }
        }
        return duplicate;
    }

    public int packIndex, numBanners, numIcons, numImages;
    public int bannerDownloaded, iconDownloaded, imageDownloaded;

    private void CheckPackData()
    {
        numBanners = numIcons = numImages = 0;
        bannerDownloaded = iconDownloaded = imageDownloaded = 0;

        var packData = packList[packIndex];
        progress.text = "Checking pack " + packIndex + ": " + packData.name;
        print("Pack " + packIndex + ": " + packData.name + " ---------------------------------------------");
        resultText.text = "";

        int index = 0;
        foreach(var photoData in packData.photoDatas)
        {
            StartCoroutine(DownloadPicture(photoData.bannerUrl, index, OnDownloadBannerComplete, 1));
            StartCoroutine(DownloadPicture(photoData.iconUrl, index, OnDownloadIconComplete, 1));
            StartCoroutine(DownloadPicture(photoData.imageUrl, index, OnDownloadImageComplete, 1));
            index++;
        }
    }

    private void CheckComplete()
    {
        int total = packList[packIndex].photoDatas.Count;
        if (numBanners == total && numIcons == total && numImages == total)
        {
            if (bannerDownloaded != total || iconDownloaded != total || imageDownloaded != total)
            {
                resultText.text = "FAILED: Check Window->Console to see more details";
                checkBtn.interactable = true;
                checkBtn2.interactable = true;
            }
            else
            {
                packIndex++;
                if (packIndex < packList.Count)
                {
                    CheckPackData();
                }
                else
                {
                    resultText.text = "PASSED";
                    progress.text = "";
                    checkBtn.interactable = true;
                    checkBtn2.interactable = true;
                }
            }
        }
    }

    private void OnDownloadBannerComplete(string url, int index, Texture2D texture, int numTry)
    {
        if (texture ==  null)
        {
            if (numTry == 2)
            {
                print("Failed to download banner at position " + index + ": " + url);
            }
            else
            {
                StartCoroutine(DownloadPicture(url, index, OnDownloadBannerComplete, ++numTry));
                return;
            }
        }
        else
        {
            if (texture.width == Const.bannerSize.x && texture.height == Const.bannerSize.y)
                bannerDownloaded++;
            else
                print("Banner incorrect size at position " + index + ": " + url);

            Destroy(texture);
        }
        numBanners++;

        if (numBanners == packList[packIndex].photoDatas.Count)
        {
            if (numBanners != bannerDownloaded)
            {
                print("Banners: " + bannerDownloaded + "/" + numBanners + " are downloaded --> NOT PASS");
            }
            else
            {
                print("Banners: PASSED");
            }

            CheckComplete();
        }
    }

    private void OnDownloadIconComplete(string url, int index, Texture2D texture, int numTry)
    {
        if (texture == null)
        {
            if (numTry == 2)
            {
                print("Failed to download icon at position " + index + ": " + url);
            }
            else
            {
                StartCoroutine(DownloadPicture(url, index, OnDownloadIconComplete, ++numTry));
                return;
            }
        }
        else
        {
            if (texture.width == Const.iconSize.x && texture.height == Const.iconSize.y)
                iconDownloaded++;
            else
                print("Icon incorrect size at position " + index + ": " + url);

            Destroy(texture);
        }
        numIcons++;

        if (numIcons == packList[packIndex].photoDatas.Count)
        {
            if (numIcons != iconDownloaded)
            {
                print("Icons: " + iconDownloaded + "/" + numIcons + " are downloaded --> NOT PASS");
            }
            else
            {
                print("Icons: PASSED");
            }

            CheckComplete();
        }
    }

    private void OnDownloadImageComplete(string url, int index, Texture2D texture, int numTry)
    {
        if (texture == null)
        {
            if (numTry == 2)
            {
                print("Failed to download image at position " + index + ": " + url);
            }
            else
            {
                StartCoroutine(DownloadPicture(url, index, OnDownloadImageComplete, ++numTry));
                return;
            }
        }
        else
        {
            if (texture.width == Const.imageSize.x && texture.height == Const.imageSize.y)
                imageDownloaded++;
            else
                print("Image incorrect size at position " + index + ": " + url);

            Destroy(texture);
        }
        numImages++;

        if (numImages == packList[packIndex].photoDatas.Count)
        {
            if (numImages != imageDownloaded)
            {
                print("Images: " + imageDownloaded + "/" + numImages + " are downloaded --> NOT PASS");
            }
            else
            {
                print("Images: PASSED");
            }

            CheckComplete();
        }
    }

    public static IEnumerator DownloadPicture(string url, int index, Action<string, int, Texture2D, int> result, int numTry)
    {
        if (url == null)
        {
            result(url, index, null, numTry);
            yield break;
        }

        WWW www = new WWW(url);
        yield return www;
        if (www.isDone && string.IsNullOrEmpty(www.error))
        {
            Texture2D texture = www.texture;
            result(url, index, texture, numTry);
        }
        else
        {
            result(url, index, null, numTry);
        }
    }

    public static void ClearConsole()
    {
#if UNITY_EDITOR
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
#endif
    }
}
