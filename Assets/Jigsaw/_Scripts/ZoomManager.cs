using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomManager : MonoBehaviour {
    public RectTransform[] transforms;
    public RectTransform boardTr;
    public Board board;
    public Button zoomButton, zoomButton2;
    public Sprite zoomInSprite, zoomOutSprite;
    public MainController mainController;
    public bool enableTouchZoom;

    [Range(1,2)]
    public float zoom = 1;

    private float lastZoom = 1;
    private bool zoomIn = true;
    private float contentWidth, contentHeight;

    public static ZoomManager instance;

    private void Awake()
    {
        instance = this;
        UpdateZoomButton();
    }

    private void Update()
    {
        // If there are two touches on the device...
        if (Input.touchCount == 2 && enableTouchZoom)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // ... change the orthographic size based on the change in distance between the touches.
            zoom -= deltaMagnitudeDiff * Time.deltaTime * 0.05f;

            zoom = Mathf.Clamp(zoom, 1, 2);
        }

        if (lastZoom != zoom)
        {
            lastZoom = zoom;
            OnZoomChanged();
        }
    }

    private void UpdateZoomButton()
    {
        if (zoom == 2) zoomIn = false;
        if (zoom == 1) zoomIn = true;

        var interactable = (Prefs.CurrentDiff == 2 || Prefs.CurrentDiff == 3) && !mainController.isGameComplete;
        zoomButton.interactable = interactable;
        zoomButton2.interactable = interactable;
        zoomButton.GetComponent<Image>().sprite = zoomIn ? zoomInSprite : zoomOutSprite;
        zoomButton2.GetComponent<Image>().sprite = zoomIn ? zoomInSprite : zoomOutSprite;
    }

    public void OnZoomButtonClick()
    {
        if (zoomIn)
        {
            var distance = 2 - zoom;
            zoom += distance > 0.5f ? (distance / 2f) : distance; 
        }
        else
        {
            var distance = zoom - 1;
            zoom -= distance > 0.5f ? (distance / 2f) : distance;
        }
    }

    private void OnZoomChanged()
    {
        foreach(var tr in transforms)
        {
            tr.localScale = Vector2.one * zoom;
        }

        UpdateContentSize();
        UpdateZoomButton();
    }

    public void UpdateContentSize()
    {
        Vector3[] corners = new Vector3[4];
        transforms[0].GetWorldCorners(corners);
        contentWidth = corners[3].x - corners[0].x;
        contentHeight = corners[1].y - corners[0].y;
    }

    public void ZoomOut()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", zoom, "to", 1, "time", 0.3f, "onupdate", "OnZoomOutUpdate"));
    }

    private void OnZoomOutUpdate(float value)
    {
        zoom = value;
    }

    public void LateUpdate()
    {
        foreach (var tr in transforms)
        {
            float newX = Mathf.Clamp(tr.position.x, board.bottomLeftCorner.x - contentWidth + board.worldWidth, board.bottomLeftCorner.x);
            float newY = Mathf.Clamp(tr.position.y, board.bottomLeftCorner.y - contentHeight + board.worldHeight, board.bottomLeftCorner.y);
            tr.position = new Vector3(newX, newY, tr.position.z);
        }
    }
}
