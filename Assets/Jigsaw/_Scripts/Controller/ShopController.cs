using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ShopController : BaseController {

    public ShopPack shopPackPrefab;
    public ShopPhoto shopPhotoPrefab;
    public RectTransform gridPacks;
    public GameObject packList, packDetail;
    public Text packDetailName, packDetailNumPicture, packDetailPrice;
    public GameObject buyBtn, groupBtn, downloadBtn, playBtn, loading;

    public ScrollRect packScrollRect, packDetailSrollRect;

    public static ShopController instance;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }

    protected override void Start()
    {
        base.Start();

        if (ShopData.instance.packList != null && ShopData.instance.packList.Count != 0) UpdateShopList();
        else
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Toast.instance.ShowMessage("No internet connection");
            }
            StartCoroutine(CheckPackJson());
        }
    }

    private IEnumerator CheckPackJson()
    {
        while (true)
        {
            if (ShopData.instance.packList != null && ShopData.instance.packList.Count != 0)
            {
                UpdateShopList();
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateShopList()
    {
        foreach (var packData in ShopData.instance.packList)
        {
            ShopPack pack = Instantiate(shopPackPrefab, gridPacks);
            pack.transform.localScale = Vector3.one;
            pack.packName = packData.name;
            pack.price = packData.price;
            pack.bannerUrl = packData.bannerUrl;
        }

        Timer.Schedule(this, 0, () =>
        {
            var contentTr = packScrollRect.content.GetComponent<RectTransform>();
            float height = gridPacks.rect.height + 112.2f;
            contentTr.sizeDelta = new Vector2(contentTr.rect.width, height);
        });
    }

    private PackData packData;
    public void OnPackButtonClick(ShopPack shopPack)
    {
        packList.SetActive(false);
        packDetail.SetActive(true);

        foreach (Transform child in packDetailSrollRect.content)
        {
            Destroy(child.gameObject);
        }

        var index = shopPack.transform.GetSiblingIndex();
        packData = ShopData.instance.packList[index];
        foreach(var photoData in packData.photoDatas)
        {
            ShopPhoto photo = Instantiate(shopPhotoPrefab, packDetailSrollRect.content);
            photo.transform.localScale = Vector3.one;
            photo.packName = packData.name;
            photo.iconUrl = photoData.iconUrl;
        }

        packDetailName.text = packData.name;
        packDetailNumPicture.text = packData.photoDatas.Count + " pictures";
        packDetailPrice.text = packData.price.ToString();

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var isBought = Prefs.IsPackBought(packData.name) || packData.price == 0;
        buyBtn.SetActive(!isBought);
        groupBtn.SetActive(isBought);

        int numDownloaded = 0;
        foreach (var photoData in packData.photoDatas)
        {
            string fileName = Path.GetFileName(photoData.imageUrl);
            string filePath = CUtils.GetLocalPath(packData.name, fileName);
            if (File.Exists(filePath))
            {
                numDownloaded++;
            }
        }

        if (numDownloaded > 0 && !Prefs.IsPackBought(packData.name))
        {
            Prefs.BuyPack(packData);
        }

        bool needUpdate = false;
        if (isBought)
        {
            var boughtPackJson = JsonUtility.ToJson(Prefs.GetBoughtPack(packData.name));
            var packJson = JsonUtility.ToJson(packData);
            if (boughtPackJson != packJson)
            {
                needUpdate = true;
            }
        }

        playBtn.SetActive(numDownloaded > 0);
        downloadBtn.SetActive(numDownloaded < packData.photoDatas.Count || needUpdate);
        downloadBtn.GetComponentInChildren<Text>().text = needUpdate ? "Update" : "Download";

        
    }

    public void OnPackBuyClick()
    {
        if (CurrencyController.DebitBalance(packData.price))
        {
            Prefs.BuyPack(packData);
            UpdateButtons();
        }
        else
        {
            DialogController.instance.ShowDialog(DialogType.Shop);
        }
    }

    int numComplete, numDownload;
    public void DownloadPack()
    {
        Prefs.BuyPack(packData);
        downloadBtn.GetComponent<Button>().interactable = false;
        loading.SetActive(true);
        numDownload = numComplete = 0;
        foreach(var photoData in packData.photoDatas)
        {
            string imageName = Path.GetFileName(photoData.imageUrl);
            string iconName = Path.GetFileName(photoData.iconUrl);
            StartCoroutine(CUtils.LoadPicture(photoData.imageUrl, packData.name, imageName, OnCachePictureComplete, Const.imageSize, true));
            StartCoroutine(CUtils.LoadPicture(photoData.iconUrl, packData.name, iconName, null, Const.iconSize, true));
        }
    }

    private void OnCachePictureComplete(Texture2D texture)
    {
        numDownload++;
        if (texture != null) numComplete++;

        if (numDownload == packData.photoDatas.Count)
        {
            if (numDownload == numComplete)
            {
                Toast.instance.ShowMessage("The pack is downloaded");
            }
            else
            {
                Toast.instance.ShowMessage(numComplete + "/" + packData.photoDatas.Count + " pictures are downloaded");
            }

            downloadBtn.GetComponent<Button>().interactable = true;
            loading.SetActive(false);
            UpdateButtons();
        }
    }

    public void PlayDownloadedPack()
    {
        Prefs.CurrentCategory = packData.name;
        CUtils.LoadScene(0, true);
    }

    public void OnBackButtonClick()
    {
        if (packList.activeSelf)
        {
            CUtils.LoadScene(0, true);
        }
        else
        {
            packList.SetActive(true);
            packDetail.SetActive(false);
        }
    }
}

[System.Serializable]
public class PackDatas
{
    public List<PackData> packDatas;
}

[System.Serializable]
public class PackData
{
    public string name;
    public string bannerUrl;
    public int price;
    public List<PhotoData> photoDatas;
}

[System.Serializable]
public class PhotoData
{
    public string iconUrl;
    public string imageUrl;
    public string bannerUrl;
}