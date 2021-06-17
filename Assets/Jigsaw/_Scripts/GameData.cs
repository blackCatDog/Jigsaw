using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour {

    public List<Category> categories;
    public DiffMask[] allMask;

    public static GameData instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
