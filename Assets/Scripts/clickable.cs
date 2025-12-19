using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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

    // Layer cache
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

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast all hits
        var hits = Physics.RaycastAll(ray, 4f);

        // Select closest interactive target by priority
        RaycastHit? targetHit = SelectTarget(hits);

        if (targetHit.HasValue)
        {
            hasHit = true;
            currentHit = targetHit.Value;
            ApplyHover(currentHit);

            if (Input.GetMouseButtonUp(1))
                HandleClick();
        }
        else
        {
            hasHit = false;
            SetCursor(idlesprite, "");
        }
    }

    // -------------------------
    // Target selection by priority
    // -------------------------
    private RaycastHit? SelectTarget(RaycastHit[] hits)
    {
        if (hits.Length == 0) return null;

        // Assign a priority value: lower = higher priority
        int Priority(RaycastHit h)
        {
            int l = h.transform.gameObject.layer;

            if (l == clickablelayer) return 0;
            if (l == doorlayer || l == slidingdoorlayer) return 1;
            if (l == drawerlayer) return 2;
            if (l == pickuplayer) return 3;
            if (l == evidencelayer || l == staticevidencelayer) return 4;
            if (l == enemylayer) return 5;
            if (l == unknownlayer) return 6;
            return 10; // default / idle
        }

        return hits
            .OrderBy(Priority)
            .ThenBy(h => h.distance) // closest
            .FirstOrDefault();
    }

    private void ApplyHover(RaycastHit hit)
    {
        int layer = hit.transform.gameObject.layer;

        if (layer == clickablelayer && IsCloseEnough(hit))
        {
            if (hit.transform.CompareTag("LIGHTSWITCHES"))
                SetCursor(GameMaster.POWER_SUPPLY_ENABLED ? clickablespritegreen : clickablespritered,
                          GameMaster.POWER_SUPPLY_ENABLED ? "Lights" : "Lights (Power Disabled)",
                          GameMaster.POWER_SUPPLY_ENABLED ? "green" : "red");
            else if (hit.transform.CompareTag("POWERSWITCH"))
                SetCursor(GameMaster.POWER_SUPPLY_ENABLED ? clickablespritegreen : clickablespritered,
                          GameMaster.POWER_SUPPLY_ENABLED ? "Power (on)" : "Power (off)",
                          GameMaster.POWER_SUPPLY_ENABLED ? "green" : "red");
            else
                SetCursor(clickablespritegreen);
            return;
        }

        if (layer == doorlayer && IsCloseEnough(hit))
        {
            Door door = hit.transform.GetComponentInParent<Door>();
            if (door == null)
            {
                SetCursor(doorsprite);
                return;
            }

            bool locked = door.IsLocked();
            

            if (door.transform.CompareTag("ExteriorDoor"))
            {
                SetCursor(doorspritegreen, "Move to a new location");
            }
            else
            {
                SetCursor(
                    locked ? lockeddoorsprite : doorspritegreen,
                    locked ? "Locked" : "",
                    locked ? "red" : "green"
                );
                
            }
            return;
        }


        if (layer == drawerlayer && IsCloseEnough(hit))
        {
            var drawer = hit.transform.GetComponent<Drawers>();
            SetCursor(drawer != null && drawer.isLocked ? lockeddrawersprite : drawerspritegreen,
                      drawer != null && drawer.isLocked ? "locked" : "",
                      drawer != null && drawer.isLocked ? "red" : "white");
            return;
        }
        

        if (layer == pickuplayer)
            SetCursor(IsCloseEnough(hit) ? pickupcloseenoughsprite : pickupsprite);
        else if (layer == evidencelayer || layer == staticevidencelayer)
            SetCursor(evidencesprite);
        else if (layer == enemylayer)
            SetCursor(enemysprite);
        else if (layer == unknownlayer)
            SetCursor(unknownsprite);
        else
            SetCursor(idlesprite);
    }

    private void HandleClick()
    {
        if (!hasHit) return;

        // Light switches
        if (currentHit.transform.CompareTag("LIGHTSWITCHES"))
        {
            var light = currentHit.transform.GetComponent<LightSwitch>();
            if (light != null) light.ToggleLightswitch();
            return;
        }

        // Doors
        if (currentHit.transform.gameObject.layer == doorlayer)
        {
            Door door = currentHit.transform.GetComponentInParent<Door>();
            if (door != null)
                door.TryUseDoor(currentHit.collider);
        }

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

    private bool IsCloseEnough(RaycastHit hit) => hit.distance <= 4f;

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
