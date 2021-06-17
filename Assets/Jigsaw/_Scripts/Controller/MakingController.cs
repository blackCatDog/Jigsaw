using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SoftMasking;
using System.IO;

public class MakingController : MonoBehaviour {

    public Transform[] gridImages;
    public GameObject picturePrefab;

    private void Start()
    {
    }

    private void Function1()
    {
        foreach (Transform child in gridImages)
        {
            if (child.gameObject.activeSelf)
            {
                foreach(Transform item in child)
                {
                    item.GetComponent<Image>().enabled = false;
                    var picture = Instantiate(picturePrefab, item);
                    picture.transform.localScale = Vector3.one;
                    picture.GetComponent<RectTransform>().sizeDelta = item.GetComponent<RectTransform>().sizeDelta;
                }
            }
        }
    }

    [ContextMenu("Round tile positions")]
    public void RoundPosition()
    {
        foreach (var gridImage in gridImages)
        {
            if (gridImage.gameObject.activeSelf)
            {
                foreach(Transform child in gridImage)
                {
                    var rt = child.GetComponent<RectTransform>();
                    Vector2 position = new Vector2(Mathf.RoundToInt(rt.anchoredPosition.x), Mathf.RoundToInt(rt.anchoredPosition.y));
                    rt.anchoredPosition = position;
                }
            }
        }
    }

    [ContextMenu("Generate Json")]
    public void GenerateJson()
    {
        int i = 0;
        foreach (var gridImage in gridImages)
        {
            if (gridImage.gameObject.activeSelf)
            {
                List<Rect> tileRects = new List<Rect>();

                foreach (Transform child in gridImage)
                {
                    var rt = child.GetComponent<RectTransform>();
                    Vector2 position = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y);
                    tileRects.Add(new Rect(position, rt.sizeDelta));
                }

                GridTile gridTile = new GridTile() { tileRects = tileRects };

                StreamWriter writer = new StreamWriter("Assets/Resources/tile_rects_" + i + ".json", false);
                writer.Write(JsonUtility.ToJson(gridTile));
                writer.Close();
            }
            i++;
        }
    }

    [ContextMenu("Refine tiles")]
    public void RefineTiles()
    {
        var gridImage = gridImages[0];
        float deltaX = 2000 - gridImage.GetComponent<RectTransform>().sizeDelta.x;
        float deltaY = 1440 - gridImage.GetComponent<RectTransform>().sizeDelta.y;

        int row, column;
        if (gridImage.childCount == 35)
        {
            row = 5; column = 7;
        }
        else if (gridImage.childCount == 70)
        {
            row = 7; column = 10;
        }
        else if (gridImage.childCount == 140)
        {
            row = 10; column = 14;
        }
        else 
        {
            row = 14; column = 20;
        }

        Vector2 delta = new Vector2(deltaX / (column - 1), deltaY / (row - 1));

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                int index = i * column + j;
                var rt = gridImage.GetChild(index).GetComponent<RectTransform>();
                if (i != row - 1)
                {
                    rt.anchoredPosition += delta.y * (row - 1 - i) * Vector2.up;
                }
                if (j != 0)
                {
                    rt.anchoredPosition += delta.x * j * Vector2.right;
                }
            }
        }
    }
}

