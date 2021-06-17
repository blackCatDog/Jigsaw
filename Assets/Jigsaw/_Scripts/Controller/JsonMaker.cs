using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class JsonMaker : MonoBehaviour {

    public InputField packName, packPrice, packBannerUrl, iconUrls, imageUrls, bannerUrls;
    public ScrollRect scrollRect;
    public Text packNamePrefab;
    public Button saveEdit, removePack;

    public List<PackData> packList = new List<PackData>();

    public PackData GetPack()
    {
        if (string.IsNullOrEmpty(packName.text.Trim()) || string.IsNullOrEmpty(packPrice.text.Trim()) || string.IsNullOrEmpty(packBannerUrl.text.Trim())
            || string.IsNullOrEmpty(imageUrls.text.Trim()) || string.IsNullOrEmpty(iconUrls.text.Trim()) || string.IsNullOrEmpty(bannerUrls.text.Trim()))
        {
            Toast.instance.ShowMessage("You need to enter all the input field");
            return null;
        }

        var name = packName.text.Trim();
        var price = int.Parse(packPrice.text.Trim());

        if (price < 0)
        {
            Toast.instance.ShowMessage("Price can't be negative");
            return null;
        }

        var imageLinks = imageUrls.text.Split(new [] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var iconLinks = iconUrls.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var bannerLinks = bannerUrls.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (imageLinks.Length != iconLinks.Length || imageLinks.Length != bannerLinks.Length)
        {
            Toast.instance.ShowMessage("Number of links are not equal");
            return null;
        }

        List<PhotoData> photoDatas = new List<PhotoData>();
        for(int i = 0; i < imageLinks.Length; i++)
        {
            PhotoData photoData = new PhotoData()
            {
                bannerUrl = bannerLinks[i],
                iconUrl = iconLinks[i],
                imageUrl = imageLinks[i]
            };

            photoDatas.Add(photoData);
        }

        PackData packData = new PackData()
        {
            name = name,
            price = price,
            bannerUrl = packBannerUrl.text.Trim(),
            photoDatas = photoDatas
        };

        return packData;
    }

    public void AddPack()
    {
        saveEdit.interactable = false;
        removePack.interactable = false;
        var packData = GetPack();
        if (packData != null)
        {
            var pack = packList.FirstOrDefault(x => x.name == packData.name);
            if (pack == null)
            {
                packList.Add(packData);
                UpdatePackNameScrollView();

                EmptyFields();
                Toast.instance.ShowMessage("Done");
            }
            else
            {
                Toast.instance.ShowMessage("The pack name exists. Please choose another name");
            }
        }
    }

    private void EmptyFields()
    {
        packBannerUrl.text = "";
        iconUrls.text = "";
        imageUrls.text = "";
        bannerUrls.text = "";
    }

    public void UpdatePackNameScrollView()
    {
        foreach (Transform child in scrollRect.content)
        {
            Destroy(child.gameObject);
        }

        int i = 0;
        foreach (var packData in packList)
        {
            var text = Instantiate(packNamePrefab, scrollRect.content);
            text.transform.localScale = Vector3.one;
            text.text = i + ". " + packData.name;

            int _tempInt = i;
            text.GetComponent<Button>().onClick.AddListener(() => OnEditPack(_tempInt));
            i++;
        }

        Timer.Schedule(this, 0, () =>
        {
            CUtils.ShowChildInScrollView(scrollRect, packList.Count - 1);
        });
    }

    private int editIndex;
    public void OnEditPack(int index)
    {
        editIndex = index;
        var pack = packList[index];
        packName.text = pack.name;
        packPrice.text = pack.price.ToString();
        packBannerUrl.text = pack.bannerUrl;

        string[] imageLinks = pack.photoDatas.Select(x => x.imageUrl).ToArray();
        string[] bannerLinks = pack.photoDatas.Select(x => x.bannerUrl).ToArray();
        string[] iconLinks = pack.photoDatas.Select(x => x.iconUrl).ToArray();

        imageUrls.text = string.Join("\n", imageLinks);
        bannerUrls.text = string.Join("\n", bannerLinks);
        iconUrls.text = string.Join("\n", iconLinks);

        saveEdit.interactable = true;
        removePack.interactable = true;
    }

    public void OnSaveEdit()
    {
        var packData = GetPack();
        if (packData != null)
        {
            var pack = packList.FirstOrDefault(x => x.name == packData.name);
            if (pack == null || packList.IndexOf(pack) == editIndex)
            {
                packList[editIndex] = packData;
                Toast.instance.ShowMessage("Done");
                UpdatePackNameScrollView();
                saveEdit.interactable = false;
                removePack.interactable = false;
                EmptyFields();
            }
            else
            {
                Toast.instance.ShowMessage("The pack name exists. Please choose another name");
            }
        }
    }

    public void OnRemovePack()
    {
        packList.RemoveAt(editIndex);
        UpdatePackNameScrollView();
    }

    public void DeleteLastPack()
    {
        saveEdit.interactable = false;
        removePack.interactable = false;
        if (packList.Count == 0) return;
        packList.RemoveAt(packList.Count - 1);
        UpdatePackNameScrollView();
    }

    public void SortByNames()
    {
        if (packList == null || packList.Count == 0) return;
        packList = packList.OrderBy(x => x.name).ToList();
        UpdatePackNameScrollView();
    }

    public void LoadJson()
    {
#if UNITY_EDITOR
        saveEdit.interactable = false;
        removePack.interactable = false;
        var path = EditorUtility.OpenFilePanel(
                "Load a json file",
                "",
                "json");

        if (path.Length != 0)
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<PackDatas>(json);
            if (data != null)
            {
                packList = data.packDatas;
                UpdatePackNameScrollView();
            }
        }
#endif
    }

    public void SaveJson()
    {
#if UNITY_EDITOR
        saveEdit.interactable = false;
        removePack.interactable = false;
        var path = EditorUtility.SaveFilePanel(
                "Save pack as json",
                "",
                "packs",
                "json");

        if (path.Length != 0)
        {
            PackDatas data = new PackDatas() { packDatas = packList };
            File.WriteAllText(path, JsonUtility.ToJson(data));
        }
#endif
    }
}
