using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SlicePhotoEditor
{

    static readonly string[] textureExtensions = {
        ".png",
        ".jpg",
        ".jpeg",
        ".tiff",
        ".tga",
        ".bmp"
    };

    public static bool IsTexture(string path)
    {
        foreach (var extension in textureExtensions)
        {
            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    [MenuItem("Assets/Slice Photo", true)]
    public static bool CheckSlicePhoto()
    {
        return IsTexture(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    [MenuItem("Assets/Slice Photo")]
    public static void SlicePhoto()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.maxTextureSize = 2048;

        List<SpriteMetaData> metaData = new List<SpriteMetaData>();

        int i = 0;
        for (int index = 0; index < 4; index++)
        {
            TextAsset textAsset = Resources.Load("slice_photo_" + index) as TextAsset;
            List<Rect> tileRects = JsonUtility.FromJson<GridTile>(textAsset.text).tileRects;

            foreach (var rect in tileRects)
            {
                SpriteMetaData smd = new SpriteMetaData
                {
                    name = Selection.activeObject.name + "_" + i,
                    rect = rect
                };

                metaData.Add(smd);
                i++;
            }
        }

        importer.spritesheet = metaData.ToArray();
        importer.SaveAndReimport();
    }

    static void UpdateSpriteMetaData(TextureImporter importer, string pathToData)
    {

    }
}
