using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class clickable : Singleton<clickable>
{
    [Header("Cursor")]
    public Image selectcursor;
    public Sprite clickablesprite, clickablespritegreen, clickablespritered;
    public Sprite idlesprite, enemysprite, unknownsprite;
    public Sprite doorsprite, doorspritegreen;
    public Sprite drawersprite, drawerspritegreen, lockeddrawersprite;
    public Sprite lockeddoorsprite;
    public Sprite pickupsprite, pickupcloseenoughsprite, evidencesprite;

    [Header("Info Text")]
    public TextMeshProUGUI infotext, infotextbg;
    public Transform infotextbgimg;

    [Header("Input")]
    public InputActionReference pointerPosition;
    public InputActionReference rightClick;

    int doorlayer, drawerlayer, clickablelayer, enemylayer;
    int pickuplayer, evidencelayer, staticevidencelayer, slidingdoorlayer;

    private RaycastHit currentHit;
    private bool hasHit;

    private Sprite currentSprite;
    private string currentText, currentTextColor;
    private Transform currentTarget;
    
    void Start()
    {
        selectcursor = GetComponent<Image>();

        doorlayer = LayerMask.NameToLayer("door");
        drawerlayer = LayerMask.NameToLayer("drawer");
        clickablelayer = LayerMask.NameToLayer("clickable");
        enemylayer = LayerMask.NameToLayer("enemy");
        pickuplayer = LayerMask.NameToLayer("pickupable");
        evidencelayer = LayerMask.NameToLayer("evidence");
        staticevidencelayer = LayerMask.NameToLayer("staticevidence");
        slidingdoorlayer = LayerMask.NameToLayer("slidingdoor");

        SetCursor(idlesprite, "");
        
        
        pointerPosition?.action.Enable();
        rightClick?.action.Enable();
        rightClick.action.performed += HandleClick;
    }
    


    void FixedUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null || Player.Instance == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * Player.Instance.RayCastDistance, Color.red);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            Player.Instance.RayCastDistance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0)
        {
            ClearHit();
            return;
        }

        hits = System.Array.FindAll(hits, h =>
            h.collider != null &&
            !h.transform.IsChildOf(Player.Instance.transform)
        );

        if (hits.Length == 0)
        {
            ClearHit();
            return;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Find the first hit that resolves to an interactable target.
        // This keeps LOS blocking: we still walk hits in distance order.
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];

            // Anything in front blocks LOS unless it's "non-blocking" by your design.
            // If this hit is NOT interactable (even after parent checks), we stop here.
            Transform target = ResolveInteractableTarget(hit);
            if (target == null)
            {
                // blocked by something non-interactable
                ClearHit();
                return;
            }

            // Interactable found
            currentHit = hit;
            currentTarget = target;
            hasHit = true;

            ApplyHoverToTarget(currentHit, currentTarget);
            return;
        }

        ClearHit();
    }


    // Helper: decide what the interactable "root" is for a hit
    private Transform ResolveInteractableTarget(RaycastHit hit)
    {
        if (hit.collider == null) return null;

        // First: direct collider object
        Transform t = hit.collider.transform;

        // PICKUPS: allow tag-based (your Pickup script uses COLLECTABLE)
        if (t.CompareTag("COLLECTABLE")) return t;

        // Or if the pickup layer is on a parent, walk up to find it
        Transform p = t;
        while (p != null)
        {
            int layer = p.gameObject.layer;

            bool isInteractableLayer =
                layer == pickuplayer || layer == drawerlayer || layer == doorlayer || layer == slidingdoorlayer ||
                layer == clickablelayer || layer == enemylayer || layer == evidencelayer || layer == staticevidencelayer;

            if (isInteractableLayer)
                return p;

            // Also treat COLLECTABLE on a parent as pickup
            if (p.CompareTag("COLLECTABLE"))
                return p;

            p = p.parent;
        }

        // Not interactable (therefore it blocks LOS)
        return null;
    }

    private void ClearHit()
    {
        if (hasHit)
        {
            hasHit = false;
            currentTarget = null;
            SetCursor(idlesprite, "");
        }
    }

    // NEW: hover logic should use the resolved target, not the raw collider layer
    private void ApplyHoverToTarget(RaycastHit hit, Transform target)
    {
        int layer = target.gameObject.layer;

        // Pickup: either pickup layer OR COLLECTABLE tag
        if (layer == pickuplayer || target.CompareTag("COLLECTABLE"))
        {
            SetCursor(IsCloseEnough(hit) ? pickupcloseenoughsprite : pickupsprite);
            return;
        }

        if (layer == drawerlayer)
        {
            var drawer = target.GetComponentInParent<Drawers>();
            bool locked = drawer != null && drawer.isLocked;
            SetCursor(locked ? lockeddrawersprite : drawerspritegreen, locked ? "locked" : "", locked ? "red" : "white");
            return;
        }

        if (layer == doorlayer || layer == slidingdoorlayer)
        {
            var door = target.GetComponentInParent<Door>();
            bool locked = door != null && door.isLocked;
            SetCursor(locked ? lockeddoorsprite : doorspritegreen, locked ? "locked" : "", locked ? "red" : "white");
            return;
        }

        if (layer == clickablelayer)
        {
            var switchComp = target.GetComponentInParent<LightSwitch>();
            if (switchComp != null)
            {
                bool powerOn = GameMaster.Instance.POWER_SUPPLY_ENABLED;
                SetCursor(powerOn ? clickablespritegreen : clickablespritered,
                          powerOn ? "Lights" : "Lights (Power Disabled)",
                          powerOn ? "green" : "red");
                return;
            }
            SetCursor(clickablespritegreen);
            return;
        }

        if (layer == evidencelayer || layer == staticevidencelayer)
        {
            SetCursor(evidencesprite);
            return;
        }

        if (layer == enemylayer)
        {
            SetCursor(enemysprite, "", "red");
            return;
        }

        SetCursor(idlesprite, "");
    }


    private void HandleClick(InputAction.CallbackContext callbackContext)
    {
        if (!hasHit) return;

        if (currentHit.transform.GetComponentInParent<Drawers>() is Drawers drawer)
        {
            drawer.Interact();
            return;
        }

        if (currentHit.transform.GetComponentInParent<Door>() is Door door)
        {
            Debug.Log("door clicked");
            
            door.TryUseDoor(currentHit.collider);
            return;
        }

        if (currentHit.transform.GetComponentInParent<LightSwitch>() is LightSwitch sw)
        {
            sw.ToggleLightswitch();
        }
    }

    private bool IsCloseEnough(RaycastHit hit)
    {
        float dist = Vector3.Distance(Player.Instance.transform.position, hit.point);
        return dist <= Player.Instance.RayCastDistance;
    }

    private void SetCursor(Sprite sprite, string text = "", string color = "white")
    {
        if (currentSprite != sprite)
        {
            selectcursor.sprite = sprite;
            currentSprite = sprite;
        }

        if (currentText != text || currentTextColor != color)
        {
            INFOTEXT(text, color);
            currentText = text;
            currentTextColor = color;
        }
    }
    public bool IsHoveringPickup()
    {
        if (!hasHit || currentTarget == null) return false;
        return currentTarget.CompareTag("COLLECTABLE") || currentTarget.gameObject.layer == pickuplayer;
    }

    public RaycastHit GetCurrentHit() => currentHit;

// Optional: if you want the actual interactable Transform:
    public Transform GetCurrentTarget() => currentTarget;
    
    private void INFOTEXT(string text, string textcolor = "white")
    {
        infotext.text = text;
        infotextbg.text = text;

        infotext.color = textcolor switch
        {
            "red" => new Color(1f, 0.2f, 0.2f, 1f),
            "green" => new Color(0f, 0.9f, 0f, 1f),
            _ => Color.white
        };

        if (infotextbgimg is RectTransform rect)
            rect.sizeDelta = new Vector2(text.Length * 18, 35);
    }
}
