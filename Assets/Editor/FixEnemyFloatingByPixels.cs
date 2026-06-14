using UnityEngine;
using UnityEditor;
using System.IO;

public class FixEnemyFloatingByPixels : EditorWindow
{
    [MenuItem("Tools/Fix Enemy Floating (Pixel Perfect)")]
    public static void FixEnemies()
    {
        string[] prefabPaths = {
            "Assets/Prefabs/Enemy_Scout.prefab",
            "Assets/Prefabs/Enemy_Rifleman.prefab",
            "Assets/Prefabs/Enemy_Heavy.prefab",
            "Assets/Prefabs/Enemy_Grenadier.prefab",
            "Assets/Prefabs/Enemy.prefab"
        };

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) continue;

            Sprite sprite = sr.sprite;
            Texture2D tex = sprite.texture;
            
            // Make texture readable temporarily
            string texPath = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            bool wasReadable = importer.isReadable;
            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            // Find lowest non-transparent pixel
            Rect rect = sprite.rect;
            int minY = (int)rect.height;
            int maxY = 0;
            int minX = (int)rect.width;
            int maxX = 0;

            Color[] pixels = tex.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            for (int y = 0; y < rect.height; y++)
            {
                for (int x = 0; x < rect.width; x++)
                {
                    Color c = pixels[y * (int)rect.width + x];
                    if (c.a > 0.05f)
                    {
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                    }
                }
            }

            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            // Calculate exact tight bounds in local space
            float ppu = sprite.pixelsPerUnit;
            float actualWidth = (maxX - minX + 1) / ppu;
            float actualHeight = (maxY - minY + 1) / ppu;
            
            // Pivot offset
            Vector2 pivotOffset = sprite.pivot / ppu;
            
            // Center of the tight bounds relative to the pivot
            float centerX = (minX + (maxX - minX) / 2f) / ppu - pivotOffset.x;
            float centerY = (minY + (maxY - minY) / 2f) / ppu - pivotOffset.y;

            Vector2 tightCenter = new Vector2(centerX, centerY);
            Vector2 tightSize = new Vector2(actualWidth, actualHeight);

            BoxCollider2D box = prefab.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = tightSize;
                box.offset = tightCenter;
                EditorUtility.SetDirty(prefab);
            }

            CapsuleCollider2D cap = prefab.GetComponent<CapsuleCollider2D>();
            if (cap != null)
            {
                cap.size = tightSize;
                cap.offset = tightCenter;
                EditorUtility.SetDirty(prefab);
            }
            
            Debug.Log($"[Pixel Perfect] Fixed {prefab.name}. Size: {tightSize}, Offset: {tightCenter}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Pixel Perfect Collider Fix Complete!");
    }
}
