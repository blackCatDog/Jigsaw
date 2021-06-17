using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using Superpow;
using SoftMasking;
#if EASY_MOBILE_LITE
using EasyMobile;
#endif

public class MainController : BaseController {
    public ScrollRect scrollRect;
    public Board board;
    public ZoomManager zoomManager;
    public Image largePreview, popupPreview;
    public GameObject gameEndView;
    public GameObject boardOverlay;
    public Text totalPieceText, completeTimeText, rubyRewardText;
    public Preview preview;

    public static GameObject emptyTile;
    public float[] tileScaleInScrollviewList;
    public float[] shadowOffsets;
    public float[] shadowOffsetsInBoard;
    public Category[] categories;

    [HideInInspector]
    public Sprite completeSprite;
    [HideInInspector]
    public Texture2D iconTexture;
    [HideInInspector]
    public Sprite[] maskes;
    [HideInInspector]
    public Pooler tilePooler;
    [HideInInspector]
    public bool isGameComplete;

    protected override void Start()
    {
        base.Start();

        int catIndex = Utils.GetCatIndex(Prefs.CurrentCategory);
        int diffIndex = Prefs.CurrentDiff;

        if (catIndex == -1)
        {
            catIndex = 0;
            diffIndex = Prefs.CurrentDiff = 0;
            Prefs.CurrentPhoto = 0;
            Prefs.CurrentCategory = GameData.instance.categories[catIndex].name;
        }

        completeSprite = GameData.instance.categories[catIndex].images[Prefs.CurrentPhoto];
        iconTexture = GameData.instance.categories[catIndex].icons[Prefs.CurrentPhoto];

        largePreview.sprite = completeSprite;
        popupPreview.sprite = iconTexture ? completeSprite : CUtils.CreateSprite(iconTexture, iconTexture.width, iconTexture.height);

        maskes = GameData.instance.allMask[diffIndex].maskes;
        tilePooler = FindObjectOfType<Pooler>();
        GameState.tileScaleInScrollView = tileScaleInScrollviewList[diffIndex];
        GameState.shadowOffset = shadowOffsets[diffIndex];
        GameState.shadowOffsetInBoard = shadowOffsetsInBoard[diffIndex];

        var textAsset = Resources.Load("tile_rects_" + diffIndex) as TextAsset;
        List<Rect> tileRects = JsonUtility.FromJson<GridTile>(textAsset.text).tileRects;

        switch (diffIndex)
        {
            case 0:
                board.LoadSetting(5, 7);
                break;
            case 1:
                board.LoadSetting(7, 10);
                break;
            case 2:
                board.LoadSetting(10, 14);
                break;
            case 3:
                board.LoadSetting(14, 20);
                break;
            case 4:
                board.LoadSetting(3, 4);
                break;
        }

        board.tileRects = tileRects;
        board.LoadSize(Const.imageSize);
        zoomManager.UpdateContentSize();

        LevelData levelData = null;
        string currentStatus = Prefs.GetCurrentStatus();

        if (currentStatus == Const.STATUS_INPROGRESS)
        {
            levelData = JsonUtility.FromJson<LevelData>(Prefs.GetCurrentProgress());
        }
        else if (currentStatus == Const.STATUS_COMPLETE)
        {
            Prefs.GameTime = 0;
        }

        if (levelData != null)
        {
            board.LoadPieces(levelData.pieces);
            board.LoadTiles(levelData.tiles);
        }

        List<int> boardTileIndexes = new List<int>();
        if (levelData != null)
        {
            foreach(var tile in board.boardTiles)
            {
                boardTileIndexes.Add(tile.tileIndex);
            }
        }

        int[] shuffledInts = new int[tileRects.Count];
        for (int i = 0; i < tileRects.Count; i++) shuffledInts[i] = i;
        shuffledInts.Shuffle();

        for(int k = 0; k < tileRects.Count; k++)
        {
            int i = shuffledInts[k];
            if (!boardTileIndexes.Contains(i))
            {
                var tileObj = Instantiate(MonoUtils.instance.tile, scrollRect.content);
                tileObj.tileIndex = i;
                tileObj.GetFinalPosition();
                tileObj.UpdateBorder();
                tileObj.transform.localScale = Vector3.one * GameState.tileScaleInScrollView;

                var mask = maskes[i];
                tileObj.shadowTr.GetComponent<Image>().sprite = mask;
                tileObj.shadowTr.sizeDelta = mask.rect.size;
                tileObj.UpdateShadow(GameState.shadowOffset);

                Transform imageMaskTr = tileObj.transform.Find("Image");
                imageMaskTr.GetComponent<SoftMask>().sprite = mask;
                imageMaskTr.GetComponent<RectTransform>().sizeDelta = mask.rect.size;

                Transform pictureTr = imageMaskTr.Find("Picture");
                pictureTr.GetComponent<Image>().sprite = Sprite.Create(completeSprite.texture, tileRects[i], Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);

                tileObj.mask.sprite = mask;
                tileObj.mask.GetComponent<RectTransform>().sizeDelta = mask.rect.size;

                tileObj.GetComponent<DragHandler>().scrollRect = scrollRect;
                tileObj.GetComponent<DragHandler>().dragParent = MonoUtils.instance.dragRegion;
            }

            i++;
        }

        AddEmptyTile();
        emptyTile.SetActive(false);
        StartCoroutine(UpdateGameTime());

        CUtils.ShowInterstitialAd();
    }

    private void AddEmptyTile()
    {
        emptyTile = new GameObject("Empty Tile");
        emptyTile.AddComponent<RectTransform>();
        emptyTile.transform.SetParent(scrollRect.content);
        emptyTile.transform.localScale = Vector3.one;
        emptyTile.transform.localPosition = Vector3.zero;
        emptyTile.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
    }

    public void OnComplete()
    {
        if (isGameComplete) return;

        isGameComplete = true;
        StopCoroutine(UpdateGameTime());

        Timer.Schedule(this, 1f, () =>
        {
            gameEndView.SetActive(true);
            boardOverlay.SetActive(true);
            gameEndView.transform.position += Vector3.down * 600;

            gameEndView.GetComponent<Animator>().SetTrigger("show");

            completeTimeText.text = string.Format(completeTimeText.text, Utils.GetTimeString(Prefs.GameTime));
            totalPieceText.text = string.Format(totalPieceText.text, Board.row * Board.col);

            bool isRewarded = Prefs.IsRewarded;
            int rewardValue = ConfigController.Config.rewardRubyOnComplete[Prefs.CurrentDiff];

            if (isRewarded) rewardValue /= 2;
            else Prefs.IsRewarded = true;

            rubyRewardText.text = "x" + rewardValue;
            CurrencyController.CreditBalance(rewardValue);
        });

        Timer.Schedule(this, 1.5f, () =>
        {
            CUtils.ShowInterstitialAd();
        });
    }

    public void EndView_Hide()
    {
        boardOverlay.SetActive(false);
        gameEndView.GetComponent<Animator>().SetTrigger("hide");
        Timer.Schedule(this, 0.3f, () => { gameEndView.SetActive(false); });
    }

    public void EndView_Share()
    {
#if !EASY_MOBILE_LITE
        Toast.instance.ShowMessage("Please install Easy Mobile Lite. \nSee Console for more details", 7);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("You need to import Easy Mobile Lite (free) to use this function. This is how to import:(Click this log to see full instruction)").AppendLine();
        sb.Append("1. Open the asset store: Window -> General -> Asset Store or Window -> Asset Store").AppendLine();
        sb.Append("2. Search: Easy Mobile Lite").AppendLine();
        sb.Append("3. Download and import it");

        Debug.LogError(sb.ToString());
#else
        StartCoroutine(DoShare());
#endif
    }

    public IEnumerator DoShare()
    {
        yield return new WaitForSeconds(0.3f);
        yield return new WaitForEndOfFrame();

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && EASY_MOBILE_LITE
        Sharing.ShareScreenshot("screenshot", "");
#else
        Toast.instance.ShowMessage("This feature only works on Android and iOS");
#endif
    }

    public void EndView_Replay()
    {
        boardOverlay.SetActive(false);
        gameEndView.GetComponent<Animator>().SetTrigger("hide");
        Prefs.GameTime = 0;

        Timer.Schedule(this, 0.3f, ()=> { CUtils.LoadScene(1, true); });
    }

    public void EndView_Next()
    {
        boardOverlay.SetActive(false);
        gameEndView.GetComponent<Animator>().SetTrigger("hide");
        Timer.Schedule(this, 0.3f, () => { CUtils.LoadScene(0, true); });
    }

    private IEnumerator UpdateGameTime()
    {
        while (!isGameComplete)
        {
            Prefs.GameTime += 1;
            yield return new WaitForSeconds(1);
        }
    }

    public void OnExitGame()
    {
        CUtils.LoadScene(0, true);
    }

    [HideInInspector]
    public int previewMode;
    private bool enableEdgeTile;
    public void PreviewClick()
    {
        if (isGameComplete) return;

        previewMode = (previewMode + 1) % 3;
        if (previewMode == 0)
        {
            largePreview.gameObject.SetActive(false);
        }
        else if (previewMode == 1)
        {
            preview.Show();
        }
        else if (previewMode == 2)
        {
            preview.Hide();
            largePreview.gameObject.SetActive(true);
        }
        Sound.instance.PlayButton();
    }

    public void ToggleEdgeTile()
    {
        if (isGameComplete) return;

        enableEdgeTile = !enableEdgeTile;
        foreach (Transform child in scrollRect.content)
        {
            if (child.name == "Empty Tile") continue;
            if (!enableEdgeTile)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                Tile tile = child.GetComponent<Tile>();
                if (tile != null && tile.IsEdgeTile() == false)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        Sound.instance.PlayButton();
    }
}

[System.Serializable]
public class GridTile
{
    public List<Rect> tileRects;
}

[System.Serializable]
public class DiffMask
{
    public Sprite[] maskes;
}

[System.Serializable]
public class Category
{
    public string name;
    public Sprite banner;
    public List<Sprite> images;
    public List<Texture2D> icons;
    public List<bool> isLocked;
    public bool isDownloaded; 
}