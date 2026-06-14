
using UnityEngine;
public class TeleportLogger : MonoBehaviour {
    Vector3 lastPos;
    void Start() { lastPos = transform.position; }
    void LateUpdate() {
        if (Vector3.Distance(transform.position, lastPos) > 5f) {
            Debug.Log("[TELEPORT LOGGER] Player teleported from " + lastPos + " to " + transform.position);
            Debug.Log("Stack Trace: " + StackTraceUtility.ExtractStackTrace());
        }
        lastPos = transform.position;
    }
}
