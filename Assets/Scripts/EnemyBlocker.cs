using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyBlocker : MonoBehaviour
{
    private Collider2D myCol;
    private bool playerIgnored = false;

    void Start()
    {
        myCol = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!playerIgnored && myCol != null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null && player.activeInHierarchy)
            {
                Collider2D[] playerCols = player.GetComponentsInChildren<Collider2D>();
                foreach (var pcol in playerCols)
                {
                    Physics2D.IgnoreCollision(myCol, pcol, true);
                }
                playerIgnored = true;
            }
        }
    }
}
