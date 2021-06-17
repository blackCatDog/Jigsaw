using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSlider : MonoBehaviour {
    public Transform inTr, outTr;
    public Preview preview;
    public GameObject largePreview;
    public GameObject arrow;
    public GameObject[] buttons;
    public SettingMenu settingMenu;
    public ScrollRect scrollRect;
    public MainController mainController;

    public bool isShowing;

    private int previewMode;
    private bool enableEdgeTile;

    public void ButtonClick()
    {
        if (isShowing) Hide();
        else Show();

        Sound.instance.PlayButton();
    }

	public void Show()
    {
        if (iTween.Count(gameObject) > 0) return;
        isShowing = true;

        StartCoroutine(ShowAnimation());
    }

    private IEnumerator ShowAnimation()
    {
        arrow.SetActive(false);
        iTween.MoveTo(gameObject, inTr.position, 0.3f);
        yield return new WaitForSeconds(0.2f);
        for (int i = buttons.Length - 1; i >= 0; i--)
        {
            iTween.ScaleTo(buttons[i], Vector3.one, 0.15f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void Hide()
    {
        if (iTween.Count(gameObject) > 0) return;
        isShowing = false;

        StartCoroutine(HideAnimation());
    }

    private IEnumerator HideAnimation()
    {
        if (settingMenu.isShowing) settingMenu.Hide();

        for (int i = 0; i < buttons.Length; i++)
        {
            iTween.ScaleTo(buttons[i], Vector3.zero, 0.15f);
            yield return new WaitForSeconds(0.05f);
        }

        iTween.MoveTo(gameObject, outTr.position, 0.3f);
        yield return new WaitForSeconds(0.3f);
        arrow.SetActive(true);
    }

    public void PreviewClick()
    {
        if (mainController.isGameComplete) return;

        previewMode = (previewMode + 1) % 3;
        if (previewMode == 0)
        {
            largePreview.SetActive(false);
        }
        else if (previewMode == 1)
        {
            preview.Show();
        }
        else if (previewMode == 2)
        {
            preview.Hide();
            largePreview.SetActive(true);
        }
        Sound.instance.PlayButton();
    }

    public void SettingClick()
    {
        if (settingMenu.isShowing) settingMenu.Hide();
        else settingMenu.Show();
        Sound.instance.PlayButton();
    }

    public void ToggleEdgeTile()
    {
        if (mainController.isGameComplete) return;

        enableEdgeTile = !enableEdgeTile;
        foreach (Transform child in scrollRect.content)
        {
            if (child.name == "Empty Tile") continue;
            if (!enableEdgeTile)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                Tile tile = child.GetComponent<Tile>();
                if (tile != null && tile.IsEdgeTile() == false)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        Sound.instance.PlayButton();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isShowing && !CUtils.IsPointerOverUIObject(gameObject) && !CUtils.IsPointerOverUIObject(settingMenu.gameObject))
            {
                Hide();
            }
        }
    }
}
