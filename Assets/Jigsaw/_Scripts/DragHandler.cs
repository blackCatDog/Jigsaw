using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IPointerExitHandler
{
    public static GameObject itemBeingDragged;

    public static bool isCustomerDragged;

    public ScrollRect scrollRect;
    public Transform dragParent;
    private GameObject emptyTile;
    private Rect screenRect;
    private Tile tile;

    public float holdTime;
    public float maxScrollVelocityInDrag;
    private int siblingIndex;


    private float timer;

    private bool isHolding;
    public bool canDrag;
    private bool isPointerOverGameObject;

    private Vector3 startMousePos, lastPosition, dragDelta;

    void Start()
    {
        timer = holdTime;
        screenRect = RectTransformToScreenSpace(scrollRect.GetComponent<RectTransform>());
        tile = GetComponent<Tile>();

        emptyTile = MainController.emptyTile;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                if (tile.piece != null)
                {
                    if (!Piece.isMoving && !Tile.isMoving)
                    {
                        tile.piece.OnBeginDrag();
                        canDrag = true;
                    }
                }
                else
                {
                    if (!Piece.isMoving && !Tile.isMoving)
                    {
                        isPointerOverGameObject = true;
                        isHolding = true;

                        lastPosition = Input.mousePosition;
                        startMousePos = Input.mousePosition;
                        dragDelta = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        StartCoroutine(Holding());
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isHolding = false;
            if (canDrag)
            {
                if (tile.piece != null)
                {
                    canDrag = false;
                    tile.piece.OnEndDrag();
                }
                else
                {
                    isHolding = false;
                    scrollRect.vertical = true;

                    itemBeingDragged = null;
                    isCustomerDragged = false;

                    canDrag = false;
                    timer = holdTime;

                    bool isInside = IsInsideScrollRect();

                    if (isInside && emptyTile.activeSelf)
                    {
                        Tile.isMoving = true;
                        iTween.MoveTo(gameObject, iTween.Hash("position", emptyTile.transform.position, "time", 0.12f, "oncomplete", "OnMoveTileToEmptyComplete"));
                    }

                    if (!isInside)
                    {
                        transform.SetParent(MonoUtils.instance.tileRegion);
                        Board.instance.AddTile2Board(tile);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                if (canDrag)
                {
                    if (tile.piece != null)
                    {
                        tile.piece.OnDrag();
                    }
                    else
                    {
                        if (Input.mousePosition != lastPosition)
                        {
                            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                            transform.position = new Vector3(pos.x + dragDelta.x, pos.y + dragDelta.y, transform.position.z);
                            lastPosition = Input.mousePosition;

                            bool inside = IsInsideScrollRect();
                            transform.SetParent(inside ? MonoUtils.instance.scrollDragRegion : dragParent);
                            transform.localScale = Vector3.one * (inside ? GameState.tileScaleInScrollView : ZoomManager.instance.zoom);
                            tile.UpdateShadow(inside ? GameState.shadowOffset : GameState.shadowOffsetInBoard);

                            if (!inside)
                            {
                                if (emptyTile.activeSelf)
                                {
                                    emptyTile.transform.SetSiblingIndex(scrollRect.content.childCount - 1);
                                    emptyTile.SetActive(false);
                                }
                            }
                            else
                            {
                                if (!emptyTile.activeSelf)
                                {
                                    emptyTile.SetActive(true);
                                }

                                int index = 0;
                                foreach(Transform child in scrollRect.content)
                                {
                                    if (child.name == "Empty Tile") continue;

                                    if (child.gameObject.activeSelf && transform.localPosition.y > child.localPosition.y)
                                    {
                                        break;
                                    }

                                    index++;
                                }
                                emptyTile.transform.SetSiblingIndex(index);
                            }
                        }
                    }
                }
                else
                {
                    if (!isPointerOverGameObject)
                    {
                        isHolding = false;
                    }
                }
            }
        }
    }

    private void OnMoveTileToEmptyComplete()
    {
        transform.SetParent(scrollRect.content);
        transform.SetSiblingIndex(emptyTile.transform.GetSiblingIndex());

        emptyTile.transform.SetSiblingIndex(scrollRect.content.childCount - 1);
        emptyTile.SetActive(false);

        Tile.isMoving = false;
        Board.instance.TileMoveBackToScrollview(tile);
        Sound.instance.Play(Sound.Others.PieceIn);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverGameObject = false;
    }

    private IEnumerator Holding()
    {
        while (timer > 0)
        {
            if (scrollRect.velocity.x >= maxScrollVelocityInDrag)
            {
                isHolding = false;
            }

            if (!isHolding)
            {
                timer = holdTime;
                yield break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        while (startMousePos == Input.mousePosition)
        {
            if (isHolding)
                yield return null;
            else
                yield break;
        }

        if (isHolding)
        {
            if (IsInsideScrollRect())
            {
                Vector3 direction = Input.mousePosition - startMousePos;
                float angle = Vector3.Angle(direction, Vector3.up);
                if (angle > 20 && angle < 160)
                {
                    isCustomerDragged = true;
                    itemBeingDragged = gameObject;
                    canDrag = true;
                    scrollRect.vertical = false;

                    siblingIndex = transform.GetSiblingIndex();
                    transform.SetParent(MonoUtils.instance.scrollDragRegion);

                    emptyTile.SetActive(true);
                    emptyTile.transform.SetSiblingIndex(siblingIndex);

                    Sound.instance.Play(Sound.Others.PieceOut);
                }
                else
                {
                    isHolding = false;
                }
            }
            else
            {
                isCustomerDragged = true;
                itemBeingDragged = gameObject;
                canDrag = true;
            }
        }
    }

    

    private bool IsInsideScrollRect()
    {
        return screenRect.Contains(transform.position);
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }

    public void Reset()
    {
        isHolding = false;
        canDrag = false;
        isPointerOverGameObject = false;
    }
}