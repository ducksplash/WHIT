using UnityEngine;

public class Ladder : MonoBehaviour
{
    public Transform bottomMountPoint;
    public Transform topMountPoint;
    public Transform bottomExitPoint;
    public Transform topExitPoint;

    public int bottomLevel = 0;
    public int topLevel = 1;

    public bool bidirectional = true;

    public Vector3 FacingForward
    {
        get
        {
            Vector3 f = transform.forward;
            f.y = 0f;
            return f.sqrMagnitude > 0.0001f ? f.normalized : Vector3.forward;
        }
    }
}