using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Superpow;
using SwipeMenu;
using UnityEngine.EventSystems;

public class DifficultyDialog : Dialog {

    public Transform selector;
    public Transform groupTr;
    public Sprite[] inactiveTexts;
    public Sprite[] activeTexts;
    public Transform[] progressTrs;
    public Text buttonText;
    public Text[] diffStatus;
    public Text unlockPriceText;
    public Text unlockMessageText;
    public GameObject selectDiffContent, unlockContent;

    private int diffIndex;
    private float[] progress;
    private string unlockMessage;

    public void OnStart()
    {
        unlockMessage = unlockMessageText.text;
        int photoIndex = Prefs.CurrentPhoto;
        int catIndex = Utils.GetCatIndex(Prefs.CurrentCategory);
        var catData = GameData.instance.categories[catIndex];

        bool isLocked = photoIndex < catData.isLocked.Count && catData.isLocked[photoIndex] && Prefs.IsLocked(Prefs.CurrentCategory, photoIndex);
        
        UpdateUI(isLocked);
    }

    private void UpdateUI(bool isLocked)
    {
        unlockContent.SetActive(isLocked);
        selectDiffContent.SetActive(!isLocked);

        if (isLocked)
        {
            int price = ConfigController.Config.unlockPicturePrice;
            unlockMessageText.text = string.Format(unlockMessage, price);
            unlockPriceText.text = "x " + price;
        }
        else
        {
            progress = new float[5];
            for (int i = 0; i < progress.Length; i++)
            {
                string status = Prefs.GetStatus(Prefs.CurrentCategory, Prefs.CurrentPhoto, i);
                if (status == Const.STATUS_NOTSTARTED) progress[i] = -1;
                else if (status == Const.STATUS_COMPLETE) progress[i] = 1;
                else
                {
                    var levelData = JsonUtility.FromJson<LevelData>(Prefs.GetProgress(Prefs.CurrentCategory, Prefs.CurrentPhoto, i));
                    progress[i] = levelData.progressDone;
                }
            }

            for (int i = 0; i < progress.Length; i++)
            {
                if (progress[i] != -1)
                {
                    var maskRt = progressTrs[i].Find("ProgressMask").GetComponent<RectTransform>();
                    maskRt.sizeDelta = new Vector2(maskRt.sizeDelta.x * progress[i], maskRt.sizeDelta.y);
                }

                diffStatus[i].gameObject.SetActive(true);
                diffStatus[i].text = progress[i] == 1 ? "Completed" : progress[i] == -1 ? "" : "In progress";
            }

            UpdateButtonText(0);
            Timer.Schedule(this, 0, () =>
            {
                UpdateIndex(0);
            });
        }
    }

    public void SelectDiff(int index)
    {
        if (diffIndex == index) return;

        diffIndex = index;
        UpdateIndex(index);
        Sound.instance.PlayButton();
    }

    private void UpdateIndex(int index)
    {
        var element = groupTr.GetChild(index);
        selector.position = element.position;
        element.GetComponent<Image>().canvasRenderer.SetAlpha(0);
        element.GetComponent<Image>().CrossFadeAlpha(1, 0.35f, true);

        for(int i = 0; i < activeTexts.Length; i++)
        {
            groupTr.GetChild(i).GetComponent<Image>().sprite = i == index ? activeTexts[i] : inactiveTexts[i];
        }

        for(int i = 0; i < progressTrs.Length; i++)
        {
            progressTrs[i].gameObject.SetActive(i == index && progress[i] != -1);
        }

        UpdateButtonText(index);
    }

    private void UpdateButtonText(int index)
    {
        buttonText.text = progress[index] == -1 ? "New game" : progress[index] == 1 ? "Play again" : "Continue";
    }

    public void OnButtonClick()
    {
        Prefs.CurrentDiff = diffIndex;

        CUtils.LoadScene(1, true);
        Sound.instance.PlayButton();
    }

    public void OnUnlockClick()
    {
        int price = ConfigController.Config.unlockPicturePrice;
        if (CurrencyController.DebitBalance(price))
        {
            UpdateUI(false);
            var frame = FindObjectOfType<Menu>().transform.GetChild((Prefs.CurrentPhoto));
            frame.transform.Find("Locked").gameObject.SetActive(false);

            Prefs.SetUnlocked(Prefs.CurrentCategory, Prefs.CurrentPhoto);
            Toast.instance.ShowMessage("Successful");
        }
        else
        {
            Toast.instance.ShowMessage("You don't have enough rubies");
        }
        Sound.instance.PlayButton();
    }
}
