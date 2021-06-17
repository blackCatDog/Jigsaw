using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingMenu : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public ThemePanel themePanel;
    public GameObject gameEndView;

    public bool isShowing;

    public void Show()
    {
        if (iTween.Count(gameObject) > 0) return;
        isShowing = true;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", 0.15f, "onupdate", "OnUpdate"));
    }

    public void Hide()
    {
        if (iTween.Count(gameObject) > 0) return;
        isShowing = false;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", 0.15f, "onupdate", "OnUpdate"));
    }

    private void OnUpdate(float value)
    {
        canvasGroup.alpha = value;
    }

    public void OnThemeClick()
    {
        if (gameEndView.activeSelf) return;

        if (themePanel.isShowing) themePanel.Hide();
        else themePanel.Show();

        Sound.instance.PlayButton();
    }

    public void OnExitClick()
    {
        CUtils.LoadScene(0, true);
    }
}
