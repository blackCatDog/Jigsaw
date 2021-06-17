using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadPixels : MonoBehaviour {
    public Sprite sprite;
    public Transform root;
    public Sprite[] maskSprites;
    public Tile imagePrefab;
    public Text timeText;

    List<Rect> tileRects;
    List<Color[]> listMaskColors = new List<Color[]>();

    void Start()
    {
        Application.targetFrameRate = 60;

        var textAsset = Resources.Load("tile_rects_" + 3) as TextAsset;
        tileRects = JsonUtility.FromJson<GridTile>(textAsset.text).tileRects;
        for (int i = 0; i < maskSprites.Length; i++)
        {
            var tile = Instantiate(imagePrefab, root);
            tile.transform.localScale = Vector3.one;
            tile.GetComponent<RectTransform>().anchoredPosition = tileRects[i].position;
            tile.GetComponent<RectTransform>().sizeDelta = tileRects[i].size;
        }

        for (int i = 0; i < maskSprites.Length; i++)
        {
            var mask = maskSprites[i];
            var maskColors = mask.texture.GetPixels((int)mask.rect.x, (int)mask.rect.y, (int)mask.rect.width, (int)mask.rect.height);
            listMaskColors.Add(maskColors);
        }
    }
	
    public void DoTask()
    {
        StartCoroutine(IEDoTask());
    }

	public IEnumerator IEDoTask()
    {
        float beginTime = Time.realtimeSinceStartup;

        for (int i = 0; i < maskSprites.Length; i++)
        {
            var maskColors = listMaskColors[i];

            int width = (int)tileRects[i].width;
            int height = (int)tileRects[i].height;

            var colors = sprite.texture.GetPixels((int)tileRects[i].position.x, (int)tileRects[i].position.y, width, height);
            int index = 0;
            foreach (var color in maskColors)
            {
                colors[index].a = color.a;
                index++;
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels(colors);
            texture.Apply();
            //var newSprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);

            //var tile = Instantiate(imagePrefab, root);
            //tile.transform.localScale = Vector3.one;
            //tile.GetComponent<RectTransform>().anchoredPosition = tileRects[i].position;
            //tile.GetComponent<RectTransform>().sizeDelta = tileRects[i].size;

            var tile = root.GetChild(i);

            var image = tile.Find("Image").GetComponent<Image>();
            image.sprite = maskSprites[i];
            image.SetNativeSize();
        }

        yield return 0;
        timeText.text = Time.realtimeSinceStartup - beginTime + "";
    }
}
