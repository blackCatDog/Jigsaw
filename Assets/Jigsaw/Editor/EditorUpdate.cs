using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class EditorUpdate
{
    static EditorUpdate()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        EditorApplication.update -= Update;

#if UNITY_ANDROID
        string from = "Assets/UnrollBall/Plugins/Android/AndroidManifest.xml";
        string to = "Assets/Plugins/Android/AndroidManifest.xml";

        if (File.Exists(from) && !File.Exists(to))
        {
            if (!Directory.Exists("Assets/Plugins")) AssetDatabase.CreateFolder("Assets", "Plugins");
            if (!Directory.Exists("Assets/Plugins/Android")) AssetDatabase.CreateFolder("Assets/Plugins", "Android");
            AssetDatabase.MoveAsset(from, to);
        }
#endif

        if (!Directory.Exists("Assets/Plugins/UnityPurchasing/Bin"))
        {
            IAPChecker.CheckItNow();
            return;
        }

        IAPChecker.CheckItNow();
    }
}