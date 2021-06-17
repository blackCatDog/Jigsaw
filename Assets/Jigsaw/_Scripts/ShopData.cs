#pragma warning disable 0618
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopData : MonoBehaviour
{
    public string packJsonLink;
    public List<PackData> packList;
    public static ShopData instance;

    int numTry = 1;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        var url = packJsonLink.Trim();
        if (string.IsNullOrEmpty(url)) return;

        StartCoroutine(GetTextFromServer(url));
    }

    private IEnumerator GetTextFromServer(string url)
    {
        WWW www = new WWW(url);
        yield return www;

        if (www.isDone && string.IsNullOrEmpty(www.error))
        {
            PackDatas data = null;
            try
            {
                data = JsonUtility.FromJson<PackDatas>(www.text);
            }catch(Exception e)
            {
                Debug.Log(e.Message);
            }

            if (data == null || data.packDatas.Count == 0)
            {
                Debug.LogWarning("Json content is empty or incorrect format");
                yield break;
            }

            packList = data.packDatas;
        }
        else
        {
            if (numTry == 1)
                Debug.LogError("Can't get json content from server");

            numTry++;
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(GetTextFromServer(url));
        }
    }
}
