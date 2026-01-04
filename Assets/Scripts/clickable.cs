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

    int doorlayer, cupboardlayer, drawerlayer, clickablelayer, enemylayer;
    int idlelayer, floorlayer, unknownlayer, pickuplayer;
    int evidencelayer, staticevidencelayer, slidingdoorlayer;

    private RaycastHit currentHit;
    private bool hasHit;

    private Sprite currentSprite;
    private string currentText, currentTextColor;

    void Awake()
    {
        selectcursor = GetComponent<Image>();

        doorlayer = LayerMask.NameToLayer("door");
        cupboardlayer = LayerMask.NameToLayer("cupboard");
        drawerlayer = LayerMask.NameToLayer("drawer");
        clickablelayer = LayerMask.NameToLayer("clickable");
        enemylayer = LayerMask.NameToLayer("enemy");
        idlelayer = LayerMask.NameToLayer("Default");
        floorlayer = LayerMask.NameToLayer("ground");
        unknownlayer = LayerMask.NameToLayer("unknown");
        pickuplayer = LayerMask.NameToLayer("pickupable");
        evidencelayer = LayerMask.NameToLayer("evidence");
        staticevidencelayer = LayerMask.NameToLayer("staticevidence");
        slidingdoorlayer = LayerMask.NameToLayer("slidingdoor");

        SetCursor(idlesprite, "");
    }

    void OnEnable()
    {
        pointerPosition?.action.Enable();
        rightClick?.action.Enable();
        
        rightClick.action.performed += HandleClick;
    }

    void OnDisable()
    {
        pointerPosition?.action.Disable();
        rightClick?.action.Disable();
        
        rightClick.action.performed -= HandleClick;
        
    }

    void FixedUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * Player.Instance.RayCastDistance, Color.red);

        RaycastHit[] hits = Physics.RaycastAll(ray, Player.Instance.RayCastDistance, ~0, QueryTriggerInteraction.Ignore);

        // Filter only interactable layers
        hits = System.Array.FindAll(hits, h =>
        {
            int l = h.transform.gameObject.layer;
            return l == pickuplayer || l == drawerlayer || l == doorlayer || l == slidingdoorlayer
                   || l == clickablelayer || l == enemylayer || l == evidencelayer || l == staticevidencelayer;
        });

        if (hits.Length == 0)
        {
            if (hasHit)
            {
                hasHit = false;
                SetCursor(idlesprite, "");
            }
            return;
        }

        // Sort hits: pickups first, then by distance
        System.Array.Sort(hits, (a, b) =>
        {
            bool aPickup = a.transform.gameObject.layer == pickuplayer;
            bool bPickup = b.transform.gameObject.layer == pickuplayer;

            if (aPickup && !bPickup) return -1;
            if (!aPickup && bPickup) return 1;

            return a.distance.CompareTo(b.distance);
        });


        // Make sure there is at least one hit
        if (hits.Length > 0)
        {
            currentHit = hits[0];
            hasHit = true;

            ApplyHover(currentHit);

        }
        else
        {
            hasHit = false;
            SetCursor(idlesprite, "");
        }
    }


    private void ApplyHover(RaycastHit hit)
    {
        int layer = hit.transform.gameObject.layer;

        if (layer == pickuplayer)
        {
            SetCursor(IsCloseEnough(hit) ? pickupcloseenoughsprite : pickupsprite);
            return;
        }

        if (layer == drawerlayer)
        {
            var drawer = hit.transform.GetComponentInParent<Drawers>();
            bool locked = drawer != null && drawer.isLocked;
            SetCursor(locked ? lockeddrawersprite : drawerspritegreen, locked ? "locked" : "", locked ? "red" : "white");
            return;
        }

        if (layer == doorlayer || layer == slidingdoorlayer)
        {
            var door = hit.transform.GetComponentInParent<Door>();
            bool locked = door != null && door.isLocked;
            SetCursor(locked ? lockeddoorsprite : doorspritegreen, locked ? "locked" : "", locked ? "red" : "white");
            return;
        }

        if (layer == clickablelayer)
        {
            var switchComp = hit.transform.GetComponentInParent<LightSwitch>();
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

        // 1. Prioritize pickup first
        if (currentHit.transform.gameObject.layer == pickuplayer)
        {
            GameMaster.Instance.Pickup.PickupItem(currentHit.transform);
            return;
        }


        // 2. Then drawers
        if (currentHit.transform.GetComponentInParent<Drawers>() is Drawers drawer)
        {
            drawer.Interact();
            return;
        }

        // 3. Then doors
        if (currentHit.transform.GetComponentInParent<Door>() is Door door)
        {
            door.TryUseDoor(currentHit.collider);
            return;
        }

        // 4. Then light switches
        if (currentHit.transform.GetComponentInParent<LightSwitch>() is LightSwitch sw)
        {
            sw.ToggleLightswitch();
        }
    }



    public bool IsHoveringPickup()
    {
        if (!hasHit) return false;
        return currentHit.transform.gameObject.layer == pickuplayer;
    }


    public RaycastHit GetCurrentHit() => currentHit;

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

        var rect = infotextbgimg as RectTransform;
        rect.sizeDelta = new Vector2(text.Length * 18, 35);
    }
}
