using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ShopPhoto : MonoBehaviour {
    public string packName;
    public string iconUrl;
    public Image iconImage;
    public GameObject loading;

    private int numTry = 0;
    private string iconName;

    private void Start()
    {
        iconName = Path.GetFileName(iconUrl);

        string filePath = CUtils.GetLocalPath(packName, iconName);
        loading.SetActive(!File.Exists(filePath));

        StartCoroutine(CUtils.LoadPicture(iconUrl, packName, iconName, OnIconLoadComplete));
    }

    private void OnIconLoadComplete(Texture2D texture)
    {
        if (texture == null)
        {
            if (numTry < 3)
            {
                StartCoroutine(CUtils.LoadPicture(iconUrl, packName, iconName, OnIconLoadComplete));
            }
            else
            {
                loading.SetActive(false);
            }
            numTry++;
            return;
        }
        var iconSprite = CUtils.CreateSprite(texture, texture.width, texture.height);
        iconImage.sprite = iconSprite;
        loading.SetActive(false);
    }
}
