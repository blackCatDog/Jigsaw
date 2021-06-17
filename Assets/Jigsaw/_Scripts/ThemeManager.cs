using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour {
    public ScrollRect scrollRect;
    public GameObject themeButtonPrefab;
    public Transform selectBorder;
    public Image mainBackround, scrollBackground;
    public Theme[] themes;

    private void Start()
    {
        int themeIndex = PlayerPrefs.GetInt("theme_index");

        foreach(Theme theme in themes)
        {
            var themeObj = Instantiate(themeButtonPrefab);
            themeObj.transform.SetParent(scrollRect.content);
            themeObj.transform.localScale = Vector3.one;
            themeObj.transform.localPosition = Vector3.zero;
            themeObj.GetComponent<Image>().sprite = theme.mainBackground;
        }

        OnChangeThemed(themeIndex);
    }

    public void OnChangeThemed(int themeIndex)
    {
        mainBackround.sprite = themes[themeIndex].mainBackground;
        scrollBackground.sprite = themes[themeIndex].scrollBackground;
        selectBorder.SetParent(scrollRect.content.GetChild(themeIndex));
        selectBorder.localPosition = Vector3.zero;

        PlayerPrefs.SetInt("theme_index", themeIndex);
    }
}

[System.Serializable]
public class Theme
{
    public Sprite mainBackground;
    public Sprite scrollBackground;
}