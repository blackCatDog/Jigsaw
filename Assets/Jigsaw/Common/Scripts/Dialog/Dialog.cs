using UnityEngine;
using System;

public class Dialog : MonoBehaviour
{
    public Animator anim;
    public Action<Dialog> onDialogOpened;
    public Action<Dialog> onDialogClosed;
    public DialogType dialogType;
    public bool enableAd = true;
    public bool enableEscape = true;
    public GameObject background;

    private AnimatorStateInfo info;
    private bool isShowing;

    protected virtual void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void Update()
    {
        if (enableEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        if (Input.GetMouseButtonDown(0) && isShowing && !CUtils.IsPointerOverUIObject(background))
        {
            Timer.Schedule(this, 0, Close);
        }
    }

    public virtual void Show()
    {
        anim.SetTrigger("show");
        onDialogOpened(this);
    }

    public void OnShowComplete()
    {
        isShowing = true;
        if (enableAd)
        {
            CUtils.ShowInterstitialAd();
        }
    }

    public void OnCloseComplete()
    {
        isShowing = false;
        onDialogClosed(this);
    }

    public virtual void Close()
    {
        anim.SetTrigger("hide");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        isShowing = false;
    }
}
