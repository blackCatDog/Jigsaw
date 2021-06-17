using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using SoftMasking;

[ExecuteInEditMode]
public class Board : MonoBehaviour
{
    public List<Tile> boardTiles = new List<Tile>();
    public List<Piece> pieces = new List<Piece>();
    public GameObject completeView;
    public MainController mainController;
    public ZoomManager zoomManager;
    public RectTransform rootCanvas, rightPanel;
    public Transform dragRegion;
    public RectTransform gameEndView;
    public GameObject menu1, menu2;

    public static int row, col;
    public static float tileSize;

    [HideInInspector]
    public List<Rect> tileRects;
    [HideInInspector]
    public float worldWidth, worldHeight;
    [HideInInspector]
    public Vector3 bottomLeftCorner;

    public static Board instance;

    private void Awake()
    {
        instance = this;
    }
    
    public void LoadSetting(int _row, int _col)
    {
        row = _row;
        col = _col;
        tileSize = GetComponent<RectTransform>().sizeDelta.y / row;
    }

    private void Update()
    {
#if UNITY_EDITOR
        LoadSize(Const.imageSize);
        zoomManager.UpdateContentSize();
#endif
    }

    public void LoadSize(Vector2Int size)
    {
        bool leftMenu = rootCanvas.rect.width >= 1050;
        menu1.SetActive(leftMenu);
        menu2.SetActive(!leftMenu);

        float maxWidth, maxHeight;
        if (leftMenu)
        {
            maxWidth = rootCanvas.rect.width - rightPanel.rect.width - 90 - 10;
            maxHeight = rootCanvas.rect.height - 10;
            transform.localPosition = new Vector3((90 - rightPanel.rect.width) / 2f, 0);
        }
        else
        {
            maxWidth = rootCanvas.rect.width - rightPanel.rect.width;
            maxHeight = rootCanvas.rect.height - 90 - 10;
            transform.localPosition = new Vector3(-rightPanel.rect.width / 2f, 90 / 2f);
        }

        var ratio = size.x / (float)size.y;

        GetComponent<RectTransform>().sizeDelta = size;
        if (ratio <= maxWidth / maxHeight)
        {
            transform.localScale = Vector3.one * (maxHeight / size.y);
        }
        else
        {
            transform.localScale = Vector3.one * (maxWidth / size.x);
        }
        dragRegion.localScale = transform.localScale;
        gameEndView.sizeDelta = new Vector2(size.x * transform.localScale.x, gameEndView.sizeDelta.y);
        gameEndView.localPosition = new Vector3(transform.localPosition.x, gameEndView.localPosition.y);

        Vector3[] v = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(v);
        worldWidth = v[3].x - v[0].x;
        worldHeight = v[1].y - v[0].y;
        bottomLeftCorner = v[0];
    }

    public void LoadPieces(List<PieceData> pieceDatas)
    {
        foreach(var pieceData in pieceDatas)
        {
            Piece piece = Instantiate(MonoUtils.instance.piece);
            piece.transform.SetParent(MonoUtils.instance.tileRegion);
            piece.transform.localScale = Vector3.one;
            piece.transform.localPosition = Vector3.zero;

            var tiles = LoadTiles(pieceData.tiles);
            piece.AddTiles(tiles);
            piece.ReOrderSiblingIndex();
            piece.isFixed = pieceData.isFixed;

            pieces.Add(piece);
        }
    }

    public List<Tile> LoadTiles(List<TileData> tileDatas)
    {
        Sprite[] maskes = mainController.maskes;
        List<Tile> tiles = new List<Tile>();
        foreach(var tileData in tileDatas)
        {
            var i = tileData.tileIndex;
            var tilePosition = tileRects[i].position;

            var tileObj = Instantiate(MonoUtils.instance.tile, MonoUtils.instance.tileRegion);
            tileObj.tileIndex = i;
            tileObj.GetFinalPosition();
            tileObj.UpdateBorder();
            tileObj.transform.SetParent(MonoUtils.instance.tileRegion);
            tileObj.transform.localScale = Vector3.one;
            tileObj.position = new Vector2(tileData.positionX, tileData.positionY);
            tileObj.transform.localPosition = GetLocalPosition(tileObj);

            var mask = maskes[i];
            tileObj.shadowTr.GetComponent<Image>().sprite = mask;
            tileObj.shadowTr.sizeDelta = mask.rect.size;
            tileObj.UpdateShadow(GameState.shadowOffsetInBoard);

            Transform imageMaskTr = tileObj.transform.Find("Image");
            imageMaskTr.GetComponent<SoftMask>().sprite = mask;
            imageMaskTr.GetComponent<RectTransform>().sizeDelta = mask.rect.size;

            Transform pictureTr = imageMaskTr.Find("Picture");
            pictureTr.GetComponent<Image>().sprite = Sprite.Create(mainController.completeSprite.texture, tileRects[i], Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);

            tileObj.mask.sprite = mask;
            tileObj.mask.GetComponent<RectTransform>().sizeDelta = mask.rect.size;

            tileObj.GetComponent<DragHandler>().scrollRect = MonoUtils.instance.scrollRect;
            tileObj.GetComponent<DragHandler>().dragParent = MonoUtils.instance.dragRegion;

            boardTiles.Add(tileObj);
            tiles.Add(tileObj);
        }
        return tiles;
    }

    public void AddTile2Board(Tile tile)
    {
        tile.position = FindPosition(tile);
        //tile.position = tile.finalPosition;
        tile.MoveTo(GetLocalPosition(tile));
    }

    private Vector2 GetLocalPosition(Tile tile)
    {
        if (tile.position == tile.finalPosition) return tileRects[tile.tileIndex].position + tileRects[tile.tileIndex].size * 0.5f;
        return tileRects[tile.tileIndex].position + tileRects[tile.tileIndex].size * 0.5f + tileSize * new Vector2(tile.position.x - tile.finalPosition.x, tile.position.y - tile.finalPosition.y);
    }

    public Vector2 FindPosition(Tile tile)
    {
        float minDistance = int.MaxValue;
        int index = 0;

        int i = 0;
        foreach(var tileRect in tileRects)
        {
            float distance = Vector2.Distance(tile.transform.localPosition, tileRect.position + tileRect.size * 0.5f);
            if (distance < minDistance)
            {
                minDistance = distance;
                index = i;
            }
            i++;
        }

        int y = row - 1 - index / col;
        int x = index % col;

        return new Vector2(x, y);
    }

    public void OnTileRepositionComplete(Tile tile)
    {
        if (!boardTiles.Contains(tile))
        {
            boardTiles.Add(tile);
        }

        List<Tile> tiles = new List<Tile>() { tile };
        OnTilesRepositionComplete(tiles);
    }

    public void OnTilesRepositionComplete(List<Tile> tiles)
    {
        var otherTiles = boardTiles.Except(tiles).ToList();
        List<Tile> linkedTiles = new List<Tile>();

        foreach (var tile in tiles)
        {
            Vector2 leftPos = tile.position + Vector2.left;
            Vector2 rightPos = tile.position + Vector2.right;
            Vector2 upPos = tile.position + Vector2.up;
            Vector2 downPos = tile.position + Vector2.down;

            Vector2 finalLeftPos = tile.finalPosition + Vector2.left;
            Vector2 finalRightPos = tile.finalPosition + Vector2.right;
            Vector2 finalUpPos = tile.finalPosition + Vector2.up;
            Vector2 finalDownPos = tile.finalPosition + Vector2.down;

            foreach (Tile t in otherTiles)
            {
                if (t.position == leftPos && t.finalPosition == finalLeftPos ||
                    t.position == rightPos && t.finalPosition == finalRightPos ||
                    t.position == upPos && t.finalPosition == finalUpPos ||
                    t.position == downPos && t.finalPosition == finalDownPos)
                {
                    if (!linkedTiles.Contains(t))
                    {
                        linkedTiles.Add(t);
                    }
                }
            }
        }

        if (linkedTiles.Count > 0)
        {
            List<Piece> releventPieces = new List<Piece>();
            foreach(var linkedTile in linkedTiles)
            {
                if (linkedTile.piece != null && !releventPieces.Contains(linkedTile.piece))
                {
                    releventPieces.Add(linkedTile.piece);
                }
            }

            if (tiles.Count > 1) releventPieces.Add(tiles[0].piece);

            if (releventPieces.Count > 0)
            {
                int maxCount = -1;
                Piece pieceWithMaxCount = null;

                foreach(Piece piece in releventPieces)
                {
                    if (piece.tiles.Count > maxCount)
                    {
                        maxCount = piece.tiles.Count;
                        pieceWithMaxCount = piece;
                    }
                }

                foreach(Piece piece in releventPieces)
                {
                    if (pieceWithMaxCount != piece)
                    {
                        pieceWithMaxCount.AddTiles(piece.tiles);
                        pieces.Remove(piece);
                        DestroyImmediate(piece.gameObject);
                    }
                }

                if (tiles.Count == 1) linkedTiles.Add(tiles[0]);

                foreach (var linkedTile in linkedTiles)
                {
                    if (linkedTile.piece == null)
                    {
                        pieceWithMaxCount.AddTile(linkedTile);
                    }
                }

                pieceWithMaxCount.CheckFixed();
                pieceWithMaxCount.ReOrderSiblingIndex();

                StartCoroutine(DoMatchAnimation(tiles[0], pieceWithMaxCount));
                StartCoroutine(CheckGameComplete());

                SoundPieceConnect(tiles.Count);
            }
            else
            {
                Piece piece = Instantiate(MonoUtils.instance.piece);
                piece.transform.SetParent(MonoUtils.instance.tileRegion);
                piece.transform.localScale = Vector3.one;
                piece.transform.localPosition = Vector3.zero;

                linkedTiles.Add(tiles[0]);
                piece.AddTiles(linkedTiles);
                piece.ReOrderSiblingIndex();

                pieces.Add(piece);

                StartCoroutine(DoMatchAnimation(tiles[0], piece));
                StartCoroutine(CheckGameComplete());
                SoundPieceConnect(1);
            }
        }

        SaveProgress();
    }

    private void SoundPieceConnect(int numTiles)
    {
        var type = Sound.Others.PieceConnect;
        int totalPieces = row * col;
        int numDone = GetNumDone();
        if (numDone == totalPieces) type = Sound.Others.PieceConnectLast;
        else if (numDone == totalPieces - 1) type = Sound.Others.PieceConnectPreLast;
        else
        {
            type = numTiles == 1 ? Sound.Others.PieceConnect : (numTiles == 2 ? Sound.Others.PieceConnect2 : Sound.Others.PieceConnect3);
        }

        Sound.instance.Play(type);
    }

    public void TileMoveBackToScrollview(Tile tile)
    {
        boardTiles.Remove(tile);
    }

    private List<Tile> fadedTiles;
    private IEnumerator DoMatchAnimation(Tile centerTile, Piece piece)
    {
        TileComparer comparer = new TileComparer { center = centerTile };
        fadedTiles = piece.tiles.OrderBy(x => x, comparer).ToList();

        List<Tile> group = new List<Tile>();
        int begin = 0;

        int times = 0;
        for(int i = 0; i < fadedTiles.Count; i++)
        {
            if (Vector3.Distance(fadedTiles[i].position, centerTile.position) != Vector3.Distance(fadedTiles[begin].position, centerTile.position))
            {
                begin = i;
                times++;
                DoGroupAnimation(group);

                yield return new WaitForSeconds(0.08f);
                group.Clear();

                if (times == 4) break;
            }
            group.Add(fadedTiles[i]);
        }

        if (times < 4)
        DoGroupAnimation(group);
    }

    private void DoGroupAnimation(List<Tile> group)
    {
        foreach(Tile tile in group)
        {
            tile.StartCoroutine(tile.DoMatchAnimation());
        }
    }

    public class TileComparer : IComparer<Tile>
    {
        public Tile center;
        public int Compare(Tile x, Tile y)
        {
            var fx = F(x);
            var fy = F(y);

            if (fx < fy) return -1;
            if (fx == fy) return 0;
            return 1;
        }

        float F(Tile a)
        {
            return Vector3.Distance(a.position, center.position);
        }
    }

    private IEnumerator CheckGameComplete()
    {
        if (boardTiles.Count != col * row) yield break;

        foreach (var tile in boardTiles)
        {
            if (tile.position != tile.finalPosition) yield break;
        }

        yield return new WaitForSeconds(0.5f);

        if (zoomManager.zoom == 1)
        {
            DoGameComplete();
        }
        else
        {
            zoomManager.ZoomOut();
            Timer.Schedule(this, 0.3f, DoGameComplete);
        }
    }

    private void DoGameComplete()
    {
        completeView.SetActive(true);
        Prefs.SetCurrentStatus(Const.STATUS_COMPLETE);
        mainController.OnComplete();
    }

    private int GetNumDone()
    {
        int numDone = 0;
        foreach (var tile in boardTiles)
        {
            if (tile.position == tile.finalPosition)
            {
                numDone++;
            }
        }
        return numDone;
    }

    private void SaveProgress()
    {
        Prefs.SetCurrentStatus(Const.STATUS_INPROGRESS);
        List<PieceData> pieceDatas = new List<PieceData>();
        List<TileData> noPieceTileDatas = new List<TileData>();

        foreach(var piece in pieces)
        {
            List<TileData> tileDatas = new List<TileData>();
            foreach(var tile in piece.tiles)
            {
                TileData tileData = new TileData()
                {
                    tileIndex = tile.tileIndex,
                    positionX = tile.position.x,
                    positionY = tile.position.y
                };

                tileDatas.Add(tileData);
            }
            PieceData pieceData = new PieceData() { tiles = tileDatas, isFixed = piece.isFixed };
            pieceDatas.Add(pieceData);
        }

        foreach(var tile in boardTiles)
        {
            if (tile.piece == null)
            {
                TileData tileData = new TileData()
                {
                    tileIndex = tile.tileIndex,
                    positionX = tile.position.x,
                    positionY = tile.position.y
                };

                noPieceTileDatas.Add(tileData);
            }
        }

        int numDone = GetNumDone();

        LevelData levelData = new LevelData()
        {
            pieces = pieceDatas,
            tiles = noPieceTileDatas,
            progressDone = numDone / ((float)row * col)
        };

        string json = JsonUtility.ToJson(levelData);
        Prefs.SetCurrentProgress(json);
    }

    [ContextMenu("Debug Game Complete")]
    private void DebugGameComplete()
    {
        zoomManager.ZoomOut();
        Timer.Schedule(this, 0.3f, DoGameComplete);
    }
}
