using UnityEngine;
using UnityEditor;

public class SuperpowWindowEditor
{
    [MenuItem("Superpow/Clear all playerprefs")]
    static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Superpow/Credit balance (ruby, coin..)")]
    static void AddRuby()
    {
        CurrencyController.CreditBalance(1000);
    }

    [MenuItem("Superpow/Set balance to 0")]
    static void SetBalanceZero()
    {
        CurrencyController.SetBalance(0);
    }
}