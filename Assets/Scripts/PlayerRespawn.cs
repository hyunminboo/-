using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 lastSafePosition;

    void Start()
    {
        // Initialize with starting position
        lastSafePosition = transform.position;
    }

    public void UpdateSafePosition(Vector3 safePos)
    {
        lastSafePosition = safePos;
    }

    public void Respawn()
    {
        // Optionally add damage or effects here
        Debug.Log("Respawning to: " + lastSafePosition);
        
        // Reset velocity if rigid body exists
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        transform.position = lastSafePosition;
    }
}
