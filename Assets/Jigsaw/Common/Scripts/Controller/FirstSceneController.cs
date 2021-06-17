using UnityEngine;
using System.Collections;

public class FirstSceneController : MonoBehaviour
{
	public static FirstSceneController instance;

	private void Awake()
	{
		instance = this;
		Application.targetFrameRate = 60;
        CPlayerPrefs.useRijndael(CommonConst.ENCRYPTION_PREFS);
	}

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !DialogController.instance.IsDialogShowing())
        {
            Application.Quit();
        }
    }
}
