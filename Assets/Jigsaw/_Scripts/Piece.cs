using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class PieceData
{
    public List<TileData> tiles;
    public bool isFixed;
}

public class Piece : MonoBehaviour {

    public List<Tile> tiles = new List<Tile>();
    private Vector2 moveDelta;
    public CanvasGroup canvasGroup;
    public static bool isMoving = false;
    public bool isFixed;
    private DragBoardHandler dragBoardHandler;

    private void Start()
    {
        dragBoardHandler = FindObjectOfType<DragBoardHandler>();
    }

    public void AddTile(Tile tile)
    {
        tile.transform.SetParent(transform);
        tile.SetPiece(this);
        tiles.Add(tile);
    }

    public void AddTiles(List<Tile> tiles)
    {
        foreach(var tile in tiles)
        {
            AddTile(tile);
        }
    }

    private Vector3 lastPosition;
    private Vector3 dragDelta;
    private Vector3 lastLocalPosition;

    public void OnBeginDrag()
    {
        if (isFixed)
        {
            if (ZoomManager.instance.zoom == 1)
                iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0.3f, "time", 0.1f, "onupdate", "OnUpdate"));
            else
                dragBoardHandler.OnBeginDrag(null);
            return;
        }

        lastPosition = Input.mousePosition;
        dragDelta = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastLocalPosition = transform.localPosition;
        transform.SetSiblingIndex(transform.parent.childCount - 1);
    }

    private void OnUpdate(float value)
    {
        canvasGroup.alpha = value;
    }

    public void OnDrag()
    {
        if (isFixed)
        {
            if (ZoomManager.instance.zoom != 1)
                dragBoardHandler.OnDrag(null);
            return;
        }

        if (Input.mousePosition != lastPosition)
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            transform.position = new Vector3(pos.x + dragDelta.x, pos.y + dragDelta.y, transform.position.z);
            lastPosition = Input.mousePosition;
        }
    }

    public void OnEndDrag()
    {
        if (isFixed)
        {
            if (ZoomManager.instance.zoom == 1)
                iTween.ValueTo(gameObject, iTween.Hash("from", 0.3f, "to", 1, "time", 0.1f, "onupdate", "OnUpdate"));
            return;
        }

        float x = Mathf.Floor((tiles[0].transform.localPosition.x + transform.localPosition.x) / Board.tileSize);
        float y = Mathf.Floor((tiles[0].transform.localPosition.y + transform.localPosition.y) / Board.tileSize);

        var position = new Vector2(x, y);
        moveDelta = position - tiles[0].position;

        float left, right, up, down;
        GetBorder(out left, out right, out up, out down);

        moveDelta.x = Mathf.Clamp(moveDelta.x, -right, Board.col - 1 - left);
        moveDelta.y = Mathf.Clamp(moveDelta.y, -up, Board.row - 1 - down);

        var localDeltaX = Board.tileSize * moveDelta.x;
        var localDeltaY = Board.tileSize * moveDelta.y;

        var newLocalPosition = lastLocalPosition + new Vector3(localDeltaX, localDeltaY);

        isMoving = true;
        iTween.MoveTo(gameObject, iTween.Hash("position", newLocalPosition, "isLocal", true, "time", 0.12f, "oncomplete", "OnPieceRepositionComplete"));
    }

    private void OnPieceRepositionComplete()
    {
        foreach(var tile in tiles)
        {
            tile.position += moveDelta;
        }

        CheckFixed();
        ReOrderSiblingIndex();

        Board.instance.OnTilesRepositionComplete(tiles);
        isMoving = false;
    }

    private void GetBorder(out float left, out float right, out float up, out float down)
    {
        left = down = int.MaxValue;
        right = up = int.MinValue;

        foreach(var tile in tiles)
        {
            if (tile.position.x < left) left = tile.position.x;
            if (tile.position.x > right) right = tile.position.x;
            if (tile.position.y < down) down = tile.position.y;
            if (tile.position.y > up) up = tile.position.y;
        }
    }


    public void CheckFixed()
    {
        if (isFixed) return;
        isFixed = IsFixed();
    }

    private bool IsFixed()
    {
        Vector2[] checkList = { Vector2.zero, Vector2.right * (Board.col - 1), Vector2.up * (Board.row - 1), new Vector2(Board.col - 1, Board.row - 1) };
        
        int numCorner = 0;
        List<Vector2> cornerPositions = new List<Vector2>();
        foreach(var tile in tiles)
        {
            foreach (var aCorner in checkList)
            {
                if (tile.position == tile.finalPosition && tile.position == aCorner)
                {
                    cornerPositions.Add(aCorner);
                    numCorner++;
                }
            }
        }

        if (numCorner == 0 || numCorner == 1) return false;
        if (numCorner >= 3) return true;
        if (numCorner == 2 &&   cornerPositions[0].x + cornerPositions[0].y + 
                                cornerPositions[1].x + cornerPositions[1].y == Board.col + Board.row - 2)
            return true;

        Vector2 vector = cornerPositions[1] - cornerPositions[0];
        vector.Normalize();

        for (Vector2 i = cornerPositions[0]; i == cornerPositions[1]; i += vector)
        {
            if (tiles.Find(x=>x.position == i) == null)
            {
                return false;
            }
        }

        if (Mathf.Abs(vector.y) == 1) return true;

        var perpendicular = new Vector2(-vector.y, vector.x).normalized;

        Vector2[] cases = { cornerPositions[0] + perpendicular, cornerPositions[0] - perpendicular,
                            cornerPositions[1] + perpendicular, cornerPositions[1] - perpendicular};

        foreach(var aCase in cases)
        {
            if (tiles.Find(x=>x.position == aCase) != null)
            {
                return true;
            }
        }

        return false;
    }

    public void ReOrderSiblingIndex()
    {
        Transform parent = transform.parent;
        for(int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child != transform)
            {
                var tile = child.GetComponent<Tile>();
                Piece piece = null;
                if (tile == null) piece = parent.GetChild(i).GetComponent<Piece>();

                int count = tile != null ? 1 : piece.tiles.Count;

                if (tiles.Count <= count)
                {
                    transform.SetSiblingIndex(i + 1);
                    return;
                }
            }
        }

        transform.SetSiblingIndex(0);
    }
}
