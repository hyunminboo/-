using UnityEngine;
using UnityEditor;

public class CheckImportedImages : MonoBehaviour
{
    [MenuItem("Tools/Check Imported Images")]
    public static void Execute()
    {
        string[] files = {
            "Assets/ImportedBackground/background2.png",
            "Assets/ImportedBackground/ground2-1.png",
            "Assets/ImportedBackground/ground2-2.png",
            "Assets/ImportedBackground/water.png"
        };
        foreach (var file in files)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
            if (tex != null)
            {
                Debug.Log($"Found image: {file} ({tex.width}x{tex.height})");
            }
            else
            {
                Debug.Log($"Not found: {file}");
            }
        }
    }
}
