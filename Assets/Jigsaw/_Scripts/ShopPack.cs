using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPack : MonoBehaviour {

    public string packName;
    public string bannerUrl;
    public int price;

    public Image bannerImage;
    public Text packNameText, packPriceText;

    private bool enableLoading;

    private void Start()
    {
        packNameText.text = packName;
        packPriceText.text = price == 0 ? "FREE" : price.ToString();
    }

    private void Update()
    {
        if (!enableLoading && transform.position.y > -20f)
        {
            enableLoading = true;

            var bannerName = "pack_banner.png";
            StartCoroutine(CUtils.LoadPicture(bannerUrl, packName, bannerName, OnBannerLoadComplete));
        }
    }

    private void OnBannerLoadComplete(Texture2D texture)
    {
        if (texture != null)
        {
            var bannerSprite = CUtils.CreateSprite(texture, texture.width, texture.height);
            bannerImage.sprite = bannerSprite;
        }
    }

    public void OnButtonClick()
    {
        ShopController.instance.OnPackButtonClick(this);
    }
}
