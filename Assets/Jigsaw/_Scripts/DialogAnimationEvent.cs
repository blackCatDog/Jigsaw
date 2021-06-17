using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogAnimationEvent : MonoBehaviour {

	public void OnDialogShowComplete()
    {
        transform.parent.GetComponent<Dialog>().OnShowComplete();
    }

    public void OnDialogCloseComplete()
    {
        transform.parent.GetComponent<Dialog>().OnCloseComplete();
    }
}
