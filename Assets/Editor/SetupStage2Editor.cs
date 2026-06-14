using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupStage2Editor : MonoBehaviour
{
    [MenuItem("Tools/Setup Stage 2 Underground")]
    public static void Execute()
    {
        // 1. Create or Find ImportedMapParent
        GameObject mapParent = GameObject.Find("ImportedMapParent");
        if (mapParent == null)
        {
            mapParent = new GameObject("ImportedMapParent");
            mapParent.transform.position = Vector3.zero;
        }

        // 2. Locate Background_2
        GameObject bg2 = GameObject.Find("Background_2");
        if (bg2 == null)
        {
            Debug.LogError("Background_2 not found!");
            return;
        }

        // 3. Move Background_2 under ImportedMapParent and position it underground
        bg2.transform.SetParent(mapParent.transform);
        
        // Stage2_Trigger is at X=95. Let's make underground floor at Y=-25
        bg2.transform.position = new Vector3(95f, -25f, 0f);

        // Make it large enough for a stage
        bg2.transform.localScale = new Vector3(5f, 2f, 1f); 

        // 4. Set up Floor Collider for Stage 2
        BoxCollider2D floorCollider = bg2.GetComponent<BoxCollider2D>();
        if (floorCollider == null)
        {
            floorCollider = bg2.AddComponent<BoxCollider2D>();
        }
        
        // Disable trigger so it's a solid floor
        floorCollider.isTrigger = false;

        // Ensure the sprite bounds provide a good floor, or manually adjust offset/size
        SpriteRenderer sr = bg2.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float w = sr.sprite.bounds.size.x;
            float h = sr.sprite.bounds.size.y;
            // Floor is at the bottom of the sprite
            floorCollider.size = new Vector2(w, h * 0.2f); // bottom 20% acts as floor
            floorCollider.offset = new Vector2(0f, -h * 0.4f); 
        }

        // 5. Create Invisible Walls for Stage 2 bounds
        CreateInvisibleWall("Stage2_Wall_Left", mapParent.transform, new Vector3(60f, -20f, 0f));
        CreateInvisibleWall("Stage2_Wall_Right", mapParent.transform, new Vector3(130f, -20f, 0f));

        Debug.Log("Stage 2 Underground Setup Complete!");
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    private static void CreateInvisibleWall(string name, Transform parent, Vector3 position)
    {
        GameObject wall = GameObject.Find(name);
        if (wall == null)
        {
            wall = new GameObject(name);
            wall.transform.SetParent(parent);
            
            BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = new Vector2(1f, 30f); // Tall wall
        }
        wall.transform.position = position;
    }
}
