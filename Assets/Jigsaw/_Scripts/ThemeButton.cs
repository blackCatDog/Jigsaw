using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeButton : MonoBehaviour {

	public void OnClick()
    {
        FindObjectOfType<ThemeManager>().OnChangeThemed(transform.GetSiblingIndex());
    }
}
