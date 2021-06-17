using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragBoardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform[] targets;
    public ZoomManager zoomManager;
    private Vector3 delta;


    public void OnBeginDrag(PointerEventData eventData)
    {
        delta = targets[0].position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        foreach(var target in targets)
        {
            target.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + delta;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }
}
