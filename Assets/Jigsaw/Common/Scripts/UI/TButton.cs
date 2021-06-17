using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TButton : MyButton {
    [HideInInspector]
    public bool isOn;
    public Image image;
    public Sprite on, off;

    public bool IsOn
    {
        get { return isOn; }
        set
        {
            isOn = value;
            UpdateButtons();
        }
    }

    public override void OnButtonClick()
    {
        base.OnButtonClick();
        IsOn = !IsOn;
    }

    public void UpdateButtons()
    {
        image.sprite = isOn ? on : off;
    }
}
