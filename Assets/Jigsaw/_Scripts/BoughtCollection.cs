using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoughtCollection : MonoBehaviour
{
    public string packName;
    public string bannerUrl;
    public Image iconImage;

	void OnEnable()
    {
        var iconName = "pack_banner.png";
        StartCoroutine(CUtils.LoadPicture(bannerUrl, packName, iconName, OnIconLoadComplete));
    }

    private void OnIconLoadComplete(Texture2D texture)
    {
        if (texture != null)
        {
            var iconSprite = CUtils.CreateSprite(texture, texture.width, texture.height);
            iconImage.sprite = iconSprite;
        }
    }
}
