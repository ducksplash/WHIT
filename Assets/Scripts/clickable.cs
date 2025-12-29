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
    }

    void OnDisable()
    {
        pointerPosition?.action.Disable();
        rightClick?.action.Disable();
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * Player.Instance.RayCastDistance, Color.red);

        // 👇 THE IMPORTANT FIX
        if (Physics.Raycast(ray, out RaycastHit hit, Player.Instance.RayCastDistance))
        {
            hasHit = true;
            currentHit = hit;

            ApplyHover(hit);

            if (rightClick != null && rightClick.action.WasReleasedThisFrame())
                HandleClick();
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

        if (layer == drawerlayer && IsCloseEnough(hit))
        {
            var drawer = hit.transform.GetComponentInParent<Drawers>();

            SetCursor(
                drawer != null && drawer.isLocked ? lockeddrawersprite : drawerspritegreen,
                drawer != null && drawer.isLocked ? "locked" : "",
                drawer != null && drawer.isLocked ? "red" : "white"
            );
            return;
    }

        if (layer == doorlayer || layer == slidingdoorlayer)
        {
            Door door = hit.transform.GetComponentInParent<Door>();

            if (door != null && door.isLocked)
                SetCursor(lockeddoorsprite, "locked", "red");
            else
                SetCursor(doorspritegreen, "", "white");

            return;
        }

        if (layer == clickablelayer)
        {
            var switchComp = hit.transform.GetComponentInParent<LightSwitch>();

            if (switchComp != null)
            {
                bool powerOn = GameMaster.POWER_SUPPLY_ENABLED;

                SetCursor(
                    powerOn ? clickablespritegreen : clickablespritered,
                    powerOn ? "Lights" : "Lights (Power Disabled)",
                    powerOn ? "green" : "red"
                );

                return;
            }

            SetCursor(clickablespritegreen);
            return;
        }

        if (layer == pickuplayer)
        {
            SetCursor(IsCloseEnough(hit) ? pickupcloseenoughsprite : pickupsprite);
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
        }
        else
        {
            SetCursor(idlesprite, "");
        }
    }

    private void HandleClick()
    {
        if (!hasHit) return;

        if (currentHit.transform.gameObject.layer == drawerlayer)
        {
            var drawer = currentHit.transform.GetComponentInParent<Drawers>();
            if (drawer != null) drawer.Interact();
            return;
        }

        if (currentHit.transform.gameObject.layer == doorlayer ||
            currentHit.transform.gameObject.layer == slidingdoorlayer)
        {
            Door door = currentHit.transform.GetComponentInParent<Door>();
            if (door != null)
                door.TryUseDoor(currentHit.collider);

            return;
        }

        if (currentHit.transform.gameObject.layer == clickablelayer)
        {
            var switchComp = currentHit.transform.GetComponentInParent<LightSwitch>();
            if (switchComp != null)
                switchComp.ToggleLightswitch();
        }
    }

    public bool IsHoveringInteractive()
    {
        if (!hasHit) return false;

        int l = currentHit.transform.gameObject.layer;
        return l == drawerlayer || l == doorlayer || l == clickablelayer;
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

    private bool IsCloseEnough(RaycastHit hit) =>
        hit.distance <= Player.Instance.RayCastDistance;

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
