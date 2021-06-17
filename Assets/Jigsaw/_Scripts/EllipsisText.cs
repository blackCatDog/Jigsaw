using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EllipsisText : MonoBehaviour
{
    public int lengthLimit = 15;

	public void UpdateText()
    {
        string text = GetComponent<Text>().text;
        if (text.Length > lengthLimit)
        {
            text = text.Substring(0, lengthLimit - 1) + "..";
            GetComponent<Text>().text = text;
        }
    }
}
