using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemePanel : MonoBehaviour {
    public Transform inTr, outTr;
    public GameObject content;
    public GameObject background;
    public bool isShowing;
    public ScrollRect scrollRect;

    public void Show()
    {
        if (iTween.Count(content) > 0) return;
        isShowing = true;

        iTween.MoveTo(content, inTr.position, 0.3f);

        int themeIndex = PlayerPrefs.GetInt("theme_index");
        CUtils.ShowChildInScrollView(scrollRect, themeIndex);
    }

    public void Hide()
    {
        if (iTween.Count(content) > 0) return;
        isShowing = false;

        iTween.MoveTo(content, outTr.position, 0.3f);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isShowing && !CUtils.IsPointerOverUIObject(background))
            {
                Hide();
            }
        }
    }
}
