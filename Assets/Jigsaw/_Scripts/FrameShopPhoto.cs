using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FrameShopPhoto : MonoBehaviour {

    public string packName;
    public string iconUrl;
    public Material material;
    [HideInInspector]
    public int catIndex, photoIndex;

    void OnEnable()
    {
        material = GetComponent<MeshRenderer>().material;
        var iconName = Path.GetFileName(iconUrl);
        StartCoroutine(CUtils.LoadPicture(iconUrl, packName, iconName, OnIconLoadComplete));
    }

    private void OnIconLoadComplete(Texture2D texture)
    {
        if (texture != null)
        {
            material.SetTexture("_MainTex", texture);
            material.SetTexture("_EmissionMap", texture);
            material.SetColor("_EmissionColor", Color.white);

            GameData.instance.categories[catIndex].icons[photoIndex] = texture;
        }
    }
}
