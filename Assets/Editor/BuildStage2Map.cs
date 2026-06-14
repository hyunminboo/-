using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildStage2Map : MonoBehaviour
{
    [MenuItem("Tools/Build Stage 2 Map")]
    public static void Execute()
    {
        // 1. Setup Parent
        GameObject mapParent = GameObject.Find("ImportedMapParent");
        if (mapParent == null)
        {
            mapParent = new GameObject("ImportedMapParent");
            mapParent.transform.position = Vector3.zero;
        }

        // Add PlayerRespawn to Player if it doesn't exist
        GameObject player = GameObject.Find("Player");
        if (player != null && player.GetComponent<PlayerRespawn>() == null)
        {
            player.AddComponent<PlayerRespawn>();
        }

        // Clean up old elements
        GameObject oldBg2 = GameObject.Find("Background_2");
        if (oldBg2 != null) DestroyImmediate(oldBg2);
        
        Transform oldGrp = mapParent.transform.Find("Stage2Group");
        if (oldGrp != null) DestroyImmediate(oldGrp.gameObject);

        // Group
        GameObject stage2Group = new GameObject("Stage2Group");
        stage2Group.transform.SetParent(mapParent.transform);

        // Assets
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ImportedBackground/background2.png");
        Sprite waterSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ImportedBackground/water.png");
        Sprite ground1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ImportedBackground/ground2-1.png");
        Sprite ground2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ImportedBackground/ground2-2.png");

        if (bgSprite == null) { Debug.LogError("Assets/ImportedBackground/background2.png not found!"); return; }
        
        float bgWidth = bgSprite.bounds.size.x;
        float bgHeight = bgSprite.bounds.size.y;
        
        float startX = 95f; // Where Stage 1 ends
        float startY = -30f; // Below Stage 1

        // 2. Build Background Grid (6 cols, 2 rows)
        GameObject bgGroup = new GameObject("Backgrounds");
        bgGroup.transform.SetParent(stage2Group.transform);
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                GameObject bg = new GameObject($"CaveBG_{row}_{col}");
                bg.transform.SetParent(bgGroup.transform);
                SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
                sr.sprite = bgSprite;
                sr.sortingOrder = -10; // Behind everything
                bg.transform.position = new Vector3(startX + col * bgWidth, startY + row * bgHeight, 0f);
            }
        }

        // 3. Build Water (Bottom of the cave)
        if (waterSprite != null)
        {
            GameObject waterGroup = new GameObject("Water");
            waterGroup.transform.SetParent(stage2Group.transform);
            float waterWidth = waterSprite.bounds.size.x;
            for (int i = 0; i < 20; i++) // Enough to cover 6 backgrounds
            {
                GameObject water = new GameObject($"Water_{i}");
                water.transform.SetParent(waterGroup.transform);
                SpriteRenderer sr = water.AddComponent<SpriteRenderer>();
                sr.sprite = waterSprite;
                sr.sortingOrder = 5; // Foreground
                
                // Position at the bottom of the cave
                Vector3 pos = new Vector3(startX - 10f + i * waterWidth, startY - bgHeight/2f + waterSprite.bounds.size.y/2f, 0f);
                water.transform.position = pos;

                BoxCollider2D col = water.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                
                water.AddComponent<WaterHazard>();
            }
        }

        // 4. Place Platforms
        GameObject platformGroup = new GameObject("Platforms");
        platformGroup.transform.SetParent(stage2Group.transform);
        
        // Let's create an initial Safe Ground platform exactly where the player falls
        CreatePlatform(platformGroup, ground1, new Vector3(startX, startY + 5f, 0f), true);

        // Procedurally place some platforms forming a path
        System.Random rnd = new System.Random(42);
        float currentX = startX + 15f;
        float currentY = startY + 5f;
        for (int i = 0; i < 15; i++)
        {
            float gapX = (float)rnd.NextDouble() * 5f + 5f;
            float gapY = (float)rnd.NextDouble() * 8f - 4f; // -4 to +4
            
            currentX += gapX;
            currentY = Mathf.Clamp(currentY + gapY, startY, startY + 15f); // Stay within cave
            
            Sprite pSprite = rnd.Next(2) == 0 ? ground1 : ground2;
            if (pSprite != null)
            {
                CreatePlatform(platformGroup, pSprite, new Vector3(currentX, currentY, 0f), false);
            }
        }

        // Invisible walls
        CreateInvisibleWall("Stage2_Wall_Left", stage2Group.transform, new Vector3(startX - 10f, startY + 5f, 0f));
        CreateInvisibleWall("Stage2_Wall_Right", stage2Group.transform, new Vector3(currentX + 20f, startY + 5f, 0f));

        Debug.Log("Stage 2 Map Generated Successfully!");
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    private static void CreatePlatform(GameObject parent, Sprite sprite, Vector3 position, bool isStart)
    {
        GameObject p = new GameObject(isStart ? "StartPlatform" : "Platform");
        p.transform.SetParent(parent.transform);
        p.transform.position = position;
        
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 0;

        BoxCollider2D col = p.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        // Adjust collider to top of the platform
        col.size = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y * 0.3f);
        col.offset = new Vector2(0f, sprite.bounds.size.y * 0.3f);

        p.AddComponent<SafeGround>();
        
        // Optional: Platform Effector so player can jump up through it
        // p.AddComponent<PlatformEffector2D>();
        // col.usedByEffector = true;
    }

    private static void CreateInvisibleWall(string name, Transform parent, Vector3 position)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent);
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = new Vector2(2f, 40f);
        wall.transform.position = position;
    }
}
