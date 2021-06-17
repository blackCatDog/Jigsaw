using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Preview : MonoBehaviour, IBeginDragHandler, IDragHandler {
    public CanvasGroup canvasGroup;
    public MainController mainController;

    public float widthSize, heightSize;

    private void Start()
    {
        heightSize = Camera.main.orthographicSize;
        widthSize = heightSize * Camera.main.aspect;
    }

    public void Show()
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", 0.15f, "onupdate", "OnUpdate", "oncomplete", "OnComplete"));
    }

    public void Hide()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", 0.15f, "onupdate", "OnUpdate", "oncomplete", "OnComplete"));
    }

    public void Close()
    {
        mainController.previewMode = 0;
        Hide();
    }

    private void OnUpdate(float value)
    {
        canvasGroup.alpha = value;
    }

    private void OnComplete()
    {

    }

    private Vector3 delta;
    public void OnBeginDrag(PointerEventData eventData)
    {
        delta = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + delta;
    }

    private void LateUpdate()
    {
        float x = Mathf.Clamp(transform.position.x, -widthSize, widthSize);
        float y = Mathf.Clamp(transform.position.y, -heightSize, heightSize);

        transform.position = new Vector3(x, y, transform.position.z);
    }
}
