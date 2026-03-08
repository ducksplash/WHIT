using UnityEngine;

public class Ladder : MonoBehaviour
{
    [Header("Ladder Anchors")]
    public Transform bottomMountPoint;
    public Transform topMountPoint;
    public Transform bottomExitPoint;
    public Transform topExitPoint;
    public Transform topHoistStartPoint;
    public Transform ladderMeshTransform;
    
    [Header("Level Routing")]
    public int bottomLevel = 0;
    public int topLevel = 1;
    public bool bidirectional = true;

    [Header("Gizmos")]
    public bool drawGizmos = true;
    public float gizmoSphereRadius = 0.14f;
    public float gizmoLineOffset = 0.04f;
    public float gizmoArrowHeadLength = 0.22f;
    public float gizmoArrowHeadAngle = 25f;

    public Vector3 FacingForward
    {
        get
        {
            Vector3 f = transform.forward;
            f.y = 0f;
            return f.sqrMagnitude > 0.0001f ? f.normalized : Vector3.forward;
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        DrawAnchor(bottomMountPoint, Color.green, "Bottom Mount");
        DrawAnchor(topMountPoint, Color.cyan, "Top Mount");
        DrawAnchor(bottomExitPoint, new Color(1f, 0.7f, 0.2f), "Bottom Exit");
        DrawAnchor(topExitPoint, Color.magenta, "Top Exit");
        DrawAnchor(topHoistStartPoint, Color.red, "Top Hoist Start");

        if (bottomMountPoint != null && topMountPoint != null)
        {
            Vector3 a = bottomMountPoint.position;
            Vector3 b = topMountPoint.position;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(a, b);

            DrawArrow(a, b, Color.white);

            if (bidirectional)
                DrawArrow(b, a, Color.gray);
        }

        if (bottomMountPoint != null && bottomExitPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bottomMountPoint.position, bottomExitPoint.position);
        }

        if (topMountPoint != null && topExitPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(topMountPoint.position, topExitPoint.position);
        }

        if (topHoistStartPoint != null && topExitPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(topHoistStartPoint.position, topExitPoint.position);
        }

        DrawFacing();
    }

    private void DrawAnchor(Transform t, Color color, string label)
    {
        if (t == null) return;

        Gizmos.color = color;
        Gizmos.DrawSphere(t.position, gizmoSphereRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.Label(t.position + Vector3.up * 0.08f, label);
#endif
    }

    private void DrawFacing()
    {
        Vector3 origin = transform.position + Vector3.up * gizmoLineOffset;
        Vector3 dir = FacingForward;
        if (dir.sqrMagnitude < 0.0001f) return;

        Vector3 end = origin + dir * 0.8f;
        DrawArrow(origin, end, Color.blue);
    }

    private void DrawArrow(Vector3 from, Vector3 to, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(from, to);

        Vector3 dir = (to - from);
        if (dir.sqrMagnitude < 0.0001f) return;

        dir.Normalize();

        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        Vector3 rightHead = look * Quaternion.Euler(0f, 180f - gizmoArrowHeadAngle, 0f) * Vector3.forward;
        Vector3 leftHead  = look * Quaternion.Euler(0f, 180f + gizmoArrowHeadAngle, 0f) * Vector3.forward;

        Gizmos.DrawLine(to, to + rightHead * gizmoArrowHeadLength);
        Gizmos.DrawLine(to, to + leftHead * gizmoArrowHeadLength);
    }
}