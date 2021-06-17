using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MonoUtils : MonoBehaviour {

    public Tile tile;
    public Transform dragRegion;
    public Transform scrollDragRegion;
    public Transform tileRegion, shadowRegion;
    public Piece piece;
    public ScrollRect scrollRect;

    public static MonoUtils instance;

    private void Awake()
    {
        instance = this;
    }
}
