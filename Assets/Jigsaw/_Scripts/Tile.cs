using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[System.Serializable]
public class TileData
{
    public int tileIndex;
    public float positionX;
    public float positionY;
}

public class Tile : MonoBehaviour
{
    public Image mask;
    public RectTransform shadowTr;
    public RectTransform border;
    public Action onRepositionComplete;
    public Vector2 position;

    [HideInInspector]
    public int tileIndex;
    public Vector2 finalPosition;
    public Piece piece;

    public static bool isMoving = false;
    private Vector3 shadowOffset;

    public void GetFinalPosition()
    {
        finalPosition = new Vector2(tileIndex % Board.col, Board.row - 1 - tileIndex / Board.col);
    }

    public void UpdateShadow(float shadowOffset)
    {
        shadowTr.anchoredPosition = new Vector2(1, -1) * shadowOffset;
    }

    public void UpdateBorder()
    {
        int column = (int)finalPosition.x;
        int row = (int)finalPosition.y;

        int top = -10, left = -10, bottom = -10, right = -10;
        bool hasBorder = false;

        if (row == 0) { bottom = 0; hasBorder = true; }
        if (row == Board.row - 1) { top = 0; hasBorder = true; }
        if (column == 0) { left = 0; hasBorder = true; }
        if (column == Board.col - 1) { right = 0; hasBorder = true; }

        if (hasBorder)
        {
            border.gameObject.SetActive(true);
            border.offsetMin = new Vector2(left, bottom);
            border.offsetMax = new Vector2(-right, -top);
        }
    }

    private void LateUpdate()
    {
        if (piece != null)
        {
            shadowTr.position = transform.position + shadowOffset;
        }
    }

    public void SetPiece(Piece piece)
    {
        if (this.piece == null)
        {
            shadowTr.SetParent(MonoUtils.instance.shadowRegion);
            shadowOffset = shadowTr.position - transform.position;
        }
        this.piece = piece;
    }

    public bool IsEdgeTile()
    {
        return finalPosition.x == 0 || finalPosition.x == Board.col - 1 || finalPosition.y == 0 || finalPosition.y == Board.row - 1;
    }

    public void MoveTo(Vector3 localPosition)
    {
        isMoving = true;
        iTween.MoveTo(gameObject, iTween.Hash("position", localPosition, "isLocal", true, "time", 0.12f, "oncomplete", "OnRepositionComplete"));
    }

    private void OnRepositionComplete()
    {
        Board.instance.OnTileRepositionComplete(this);
        isMoving = false;
    }

    public IEnumerator DoMatchAnimation()
    {
        const float time = 0.3f;
        mask.gameObject.SetActive(true);
        mask.canvasRenderer.SetAlpha(0.2f);
        mask.CrossFadeAlpha(0.3f, 0.15f, true);
        yield return new WaitForSeconds(0.15f);
        mask.CrossFadeAlpha(0, time, true);
        yield return new WaitForSeconds(time);
        mask.gameObject.SetActive(false);
    }
}
