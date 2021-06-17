using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using SwipeMenu;
using Superpow;
using System;
using System.IO;

public class HomeController : BaseController
{
    public ScrollRect categoryMenuScrollRect;
    public Transform frameParentTr;
    public GameObject categoryMenuPrefab, categoryShopPrefab;
    public GameObject framePrefab;
    public Material framePhotoMaterial;
    public GameObject loading, removeAdButton, restoreButton;

    private List<PackData> boughtPacks;
    private int numDefaultCategory;

    protected override void Start()
    {
        base.Start();

        GameData.instance.categories.RemoveAll(x => x.isDownloaded);
        numDefaultCategory = GameData.instance.categories.Count;

        for (int i = 0; i < numDefaultCategory; i++)
        {
            GameObject cat = Instantiate(categoryMenuPrefab, categoryMenuScrollRect.content);
            cat.transform.localScale = Vector3.one;
            cat.GetComponent<Image>().sprite = GameData.instance.categories[i].banner;
            cat.GetComponentInChildren<Text>().text = GameData.instance.categories[i].name;
            cat.GetComponentInChildren<EllipsisText>().UpdateText();

            int _tempInt = i;
            cat.GetComponent<Button>().onClick.AddListener(() => OnCategoryClick(_tempInt));
        }

        AddBoughtPacks();
        AddCategoryShop();

        int catIndex = Utils.GetCatIndex(Prefs.CurrentCategory);
        if (catIndex == -1) catIndex = 0;

        UpdateFrames(catIndex);

        Timer.Schedule(this, 0, () =>
        {
            CUtils.ShowChildInScrollView(categoryMenuScrollRect, catIndex);
        });

        Music.instance.ChangeGameMusic();

        if (CUtils.IsAdsRemoved())
        {
            removeAdButton.SetActive(false);
            restoreButton.SetActive(false);
        }
        else
        {
#if !UNITY_IOS
            restoreButton.SetActive(false);
#endif
        }
    }

    private void UpdateFrames(int category)
    {
        for(int i = frameParentTr.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(frameParentTr.GetChild(i).gameObject);
        }

        List<MenuItem> menuItems = new List<MenuItem>();
        var catData = GameData.instance.categories[category];

        if (Prefs.LastPhoto >= catData.icons.Count) Prefs.LastPhoto = 0;

        for (int i = 0; i < catData.icons.Count; i++)
        {
            GameObject frame = Instantiate(framePrefab, frameParentTr);
            Material newMat = new Material(framePhotoMaterial);

            newMat.SetTexture("_MainTex", catData.icons[i]);
            newMat.SetTexture("_EmissionMap", catData.icons[i]);
            newMat.SetColor("_EmissionColor", catData.icons[i] != null ? Color.white : Color.black);

            frame.transform.Find("Photo").GetComponent<MeshRenderer>().material = newMat;

            if (catData.icons[i] == null)
            {
                int index = category - numDefaultCategory;
                var packData = boughtPacks[index];
                var frameShopPhoto = frame.transform.Find("Photo").GetComponent<FrameShopPhoto>();
                frameShopPhoto.catIndex = category;
                frameShopPhoto.photoIndex = i;
                frameShopPhoto.packName = packData.name;
                frameShopPhoto.iconUrl = packData.photoDatas[i].iconUrl;
                frameShopPhoto.enabled = true;
            }

            bool isLocked = i < catData.isLocked.Count && catData.isLocked[i] && Prefs.IsLocked(Prefs.CurrentCategory, i);
            bool isCompleted = Prefs.IsPhotoCompleted(Prefs.CurrentCategory, i);
            frame.transform.Find("Locked").gameObject.SetActive(isLocked);
            frame.transform.Find("Completed").gameObject.SetActive(isCompleted);

            int _tempInt = i;
            frame.GetComponent<MenuItem>().OnClick.AddListener(() => OnFrameClick(_tempInt));
            menuItems.Add(frame.GetComponent<MenuItem>());
        }

        Menu.Instance.menuItems = menuItems.ToArray();
        Menu.Instance.UpdateMenuItems(Prefs.LastPhoto + 1);

        for (int i = 0; i < GameData.instance.categories.Count; i++)
        {
            var borderImage = categoryMenuScrollRect.content.GetChild(i).GetChild(0);
            borderImage.GetComponent<Image>().color = i == category ? Color.yellow : Color.white;
        }
    }

    private void OnCategoryClick(int index)
    {
        if (Prefs.CurrentCategory == GameData.instance.categories[index].name) return;

        Prefs.CurrentCategory = GameData.instance.categories[index].name;
        UpdateFrames(index);
        Sound.instance.PlayButton();
    }

    private void OnFrameClick(int index)
    {
        if (loading.activeSelf) return;

        Prefs.CurrentPhoto = index;
        Prefs.LastPhoto = index;

        var catIndex = Utils.GetCatIndex(Prefs.CurrentCategory);
        var cat = GameData.instance.categories[catIndex];

        if (cat.images[index] != null)
        {
            OpenSelectDifficulty();
        }
        else
        {
            int packIndex = catIndex - numDefaultCategory;
            var packData = boughtPacks[packIndex];
            var url = packData.photoDatas[index].imageUrl;
            string fileName = Path.GetFileName(url);

            var localPath = CUtils.GetLocalPath(packData.name, fileName);
            if (!File.Exists(localPath))
            {
                loading.SetActive(true);
            }

            StartCoroutine(CUtils.LoadPicture(url, packData.name, fileName, (texture)=>
            {
                if (texture != null)
                {
                    if (texture.width != Const.imageSize.x || texture.height != Const.imageSize.y)
                    {
                        Toast.instance.ShowMessage(string.Format("The image size is not correct: {0}x{1}", texture.width, texture.height));
                    }
                    else
                    {
                        cat.images[index] = CUtils.CreateSprite(texture, texture.width, texture.height);
                        OpenSelectDifficulty();
                    }
                }
                else
                {
                    Toast.instance.ShowMessage("Failure to download the image");
                }

                loading.SetActive(false);
            }, Const.imageSize));
        }
    }

    private void OpenSelectDifficulty()
    {
        var dialog = (DifficultyDialog)DialogController.instance.GetDialog(DialogType.Difficulty);
        dialog.OnStart();
        DialogController.instance.ShowDialog(dialog);

        Sound.instance.PlayButton();
    }

    public int GetIconIndex(string name)
    {
        var arr = name.Split(null);
        if (arr.Length != 2) return 0;

        int result;
        bool success = int.TryParse(arr[1], out result);
        if (success) return result;

        return 0;
    }

    private void AddCategoryShop()
    {
        GameObject cat = Instantiate(categoryShopPrefab, categoryMenuScrollRect.content);
        cat.transform.localScale = Vector3.one;

        cat.GetComponent<Button>().onClick.AddListener(OnCategoryShopClick);
    }

    private void OnCategoryShopClick()
    {
        CUtils.LoadScene(2, true);
    }

    private void AddBoughtPacks()
    {
        boughtPacks = Prefs.GetBoughtPacks();

        for (int i = 0; i < boughtPacks.Count; i++)
        {
            var catIndex = Utils.GetCatIndex(boughtPacks[i].name);
            if (catIndex != -1) continue;

            GameObject cat = Instantiate(categoryMenuPrefab, categoryMenuScrollRect.content);
            cat.transform.localScale = Vector3.one;

            var boughtCollection = cat.GetComponent<BoughtCollection>();
            boughtCollection.packName = boughtPacks[i].name;
            boughtCollection.bannerUrl = boughtPacks[i].bannerUrl;
            boughtCollection.enabled = true;
            cat.GetComponentInChildren<Text>().text = boughtPacks[i].name;

            int _tempInt = i + numDefaultCategory;
            cat.GetComponent<Button>().onClick.AddListener(() => OnCategoryClick(_tempInt));

            List<Texture2D> iconTextures = new List<Texture2D>();
            List<Sprite> images = new List<Sprite>();
            List<bool> isLocked = new List<bool>();
            for (int j = 0; j < boughtPacks[i].photoDatas.Count; j++)
            {
                iconTextures.Add(null);
                images.Add(null);
                isLocked.Add(false);
            }

            Category newCat = new Category
            {
                name = boughtPacks[i].name,
                icons = iconTextures,
                images = images,
                isLocked = isLocked,
                isDownloaded = true
            };

            GameData.instance.categories.Add(newCat);
        }
    }

    public void RemoveAd()
    {
#if IAP && UNITY_PURCHASING
        Sound.instance.PlayButton();
        Purchaser.instance.BuyRemoveAd();
#else
        Debug.LogError("Please enable, import and install Unity IAP to use this function");
#endif
    }

    public void RestorePurchases()
    {
#if IAP && UNITY_PURCHASING
        Sound.instance.PlayButton();
        Purchaser.instance.RestorePurchases();
#else
        Debug.LogError("Please enable, import and install Unity IAP to use this function");
#endif
    }
}
