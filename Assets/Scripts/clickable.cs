using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class clickable : Singleton<clickable>
{
    [Header("Cursor")]
    public Image selectcursor;
    public Sprite clickablesprite, clickablespritegreen, clickablespritered;
    public Sprite terminalspritegreen, terminalspritered;
    public Sprite idlesprite, enemysprite, unknownsprite;
    public Sprite doorsprite, doorspritegreen;
    public Sprite drawersprite, drawerspritegreen, lockeddrawersprite;
    public Sprite lockeddoorsprite;
    public Sprite seatsprite;
    public Sprite pickupsprite, pickupcloseenoughsprite, evidencesprite;

    [Header("Info Text")]
    public TextMeshProUGUI infotext, infotextbg;
    public Transform infotextbgimg;

    [Header("Input")]
    public InputActionReference pointerPosition;
    public InputActionReference rightClick;

    [Header("Aim Assist (Steam Deck)")]
    public bool enableAimAssistOnSteamOS = true;

    [Tooltip("Max yaw angle difference (degrees) to trigger assist.")]
    public float aimAssistMaxAngle = 30f;

    [Tooltip("How long to disable look input after snapping.")]
    public float aimAssistLockSeconds = 0.5f;

    [Tooltip("Cooldown after an assist triggers (prevents repeated locks).")]
    public float aimAssistCooldown = 0.35f;

    [Header("Aim Assist - Only when aiming slowly")]
    [Tooltip("Optional: set this to your Look/Camera input (mouse delta / right stick). If null, we fallback to camera angular velocity.")]
    public InputActionReference lookDeltaAction;

    [Tooltip("If using lookDeltaAction: max magnitude considered 'slow'. (Tune: gamepad ~0.15-0.35, mouse delta depends on your scaling)")]
    public float lookDeltaSlowThreshold = 0.25f;

    [Tooltip("How long the aim must stay under the slow threshold before assist can snap.")]
    public float slowAimingHoldSeconds = 0.08f;

    [Tooltip("Fallback if lookDeltaAction isn't assigned: max camera angular speed (deg/sec) considered slow.")]
    public float cameraAngularSlowThreshold = 45f;

    int doorlayer, drawerlayer, clickablelayer, enemylayer;
    int pickuplayer, evidencelayer, staticevidencelayer, slidingdoorlayer, seatlayer;
    int terminallayer;

    private RaycastHit currentHit;
    private bool hasHit;

    private Sprite currentSprite;
    private string currentText, currentTextColor;
    private Transform currentTarget;

    private Transform _lastAssistTarget;
    private float _nextAssistTime;

    // --- Slow-aim gating state ---
    private float _slowAimTimer;
    private Vector3 _lastCamForward;

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
        terminallayer = LayerMask.NameToLayer("terminal");
        seatlayer = LayerMask.NameToLayer("SEAT");

        SetCursor(idlesprite, "");

        pointerPosition?.action.Enable();
        rightClick?.action.Enable();
        rightClick.action.performed += HandleClick;

        lookDeltaAction?.action.Enable();

        Camera cam = Camera.main;
        if (cam != null) _lastCamForward = cam.transform.forward;
    }

    void FixedUpdate()
    {
        if (GameMaster.Instance == null) return;
        
        if (GameMaster.Instance.PLAYERBUSY) return;
        
        Camera cam = Camera.main;
        if (cam == null || Player.Instance == null) return;

        // Update slow-aim timer (used to gate aim assist)
        UpdateSlowAimGate(cam);

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

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];

            Transform target = ResolveInteractableTarget(hit);
            if (target == null)
            {
                ClearHit();
                return;
            }

            currentHit = hit;
            currentTarget = target;
            hasHit = true;

            ApplyHoverToTarget(currentHit, currentTarget);

            // Aim assist: now gated by "aiming slowly"
            TryAimAssistSnapAndLock(cam, currentHit, currentTarget);

            return;
        }

        ClearHit();
    }

    private void UpdateSlowAimGate(Camera cam)
    {
        bool isSlow = IsAimingSlow(cam);

        if (isSlow)
            _slowAimTimer += Time.fixedDeltaTime;
        else
            _slowAimTimer = 0f;

        _lastCamForward = cam.transform.forward;
    }

    private bool IsAimingSlow(Camera cam)
    {
        // Prefer actual look input if provided (best signal for "cursor moving slowly")
        if (lookDeltaAction != null && lookDeltaAction.action != null)
        {
            Vector2 delta = lookDeltaAction.action.ReadValue<Vector2>();
            return delta.magnitude <= lookDeltaSlowThreshold;
        }

        // Fallback: approximate by camera angular speed
        // Angle between forward vectors each fixed step -> deg/sec
        float angle = Vector3.Angle(_lastCamForward, cam.transform.forward);
        float degPerSec = angle / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        return degPerSec <= cameraAngularSlowThreshold;
    }

    private Transform ResolveInteractableTarget(RaycastHit hit)
    {
        if (hit.collider == null) return null;

        Transform t = hit.collider.transform;
        if (t.CompareTag("COLLECTABLE")) return t;

        Transform p = t;
        while (p != null)
        {
            int layer = p.gameObject.layer;

            bool isInteractableLayer =
                layer == pickuplayer || layer == drawerlayer || layer == doorlayer || layer == slidingdoorlayer ||
                layer == clickablelayer || layer == enemylayer || layer == evidencelayer || layer == staticevidencelayer || layer == terminallayer || layer == seatlayer;

            if (isInteractableLayer) return p;
            if (p.CompareTag("COLLECTABLE")) return p;

            p = p.parent;
        }

        return null;
    }

    private void ClearHit()
    {
        if (!hasHit) return;

        hasHit = false;
        currentTarget = null;
        SetCursor(idlesprite, "");

        _lastAssistTarget = null;
    }

    private void ApplyHoverToTarget(RaycastHit hit, Transform target)
    {
        int layer = target.gameObject.layer;

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

        if (layer == terminallayer)
        {
            SetCursor(terminalspritegreen, "", "green");
            return;
        }


        if (layer == seatlayer)
        {
            SetCursor(seatsprite, "", "green");
            return;
        }

        SetCursor(idlesprite, "");
    }

    private void TryAimAssistSnapAndLock(Camera cam, RaycastHit hit, Transform target)
    {
        if (GameMaster.Instance.PLAYERBUSY || GameMaster.Instance.TravelCompanion.CompanionOpen) return;
        if (!enableAimAssistOnSteamOS) return;
        if (GameMaster.Instance == null) return;

        // Only assist if the player has been aiming slowly for a short moment
        if (_slowAimTimer < slowAimingHoldSeconds) return;

        if (Time.time < _nextAssistTime) return;

        var look = Player.Instance.FirstPersonLook;
        if (look == null) return;

        if (target == _lastAssistTarget) return;

        Vector3 aimPoint = GetAimAssistPoint(target, hit);

        float yawAngle = FlatAngle(cam.transform.forward, aimPoint - cam.transform.position);
        if (yawAngle > aimAssistMaxAngle) return;

        // Snap yaw and disable look input briefly
        look.SnapYawTowardWorldPoint(aimPoint);
        look.AimAssistLock(aimAssistLockSeconds);

        _lastAssistTarget = target;
        _nextAssistTime = Time.time + aimAssistCooldown;

        // Reset slow timer so it doesn't immediately chain to the next thing
        _slowAimTimer = 0f;
    }

    private Vector3 GetAimAssistPoint(Transform target, RaycastHit hit)
    {
        if (hit.collider != null)
            return hit.collider.bounds.center;

        Collider c = target.GetComponentInChildren<Collider>();
        if (c != null) return c.bounds.center;

        Renderer r = target.GetComponentInChildren<Renderer>();
        if (r != null) return r.bounds.center;

        return target.position;
    }

    private float FlatAngle(Vector3 forward, Vector3 toTarget)
    {
        Vector3 a = new Vector3(forward.x, 0f, forward.z);
        Vector3 b = new Vector3(toTarget.x, 0f, toTarget.z);
        if (a.sqrMagnitude < 0.000001f || b.sqrMagnitude < 0.000001f) return 0f;
        return Vector3.Angle(a, b);
    }

    private void HandleClick(InputAction.CallbackContext callbackContext)
    {
        if (GameMaster.Instance.PLAYERBUSY) return;
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

        if (currentHit.transform.GetComponentInParent<ComputerSystem>() is ComputerSystem pc)
        {
            pc.OnStartComputer();
            Debug.Log("this is pooter");
        }

        if (currentHit.transform.GetComponentInParent<Seat>() is Seat thisSeat)
        {
            thisSeat.NoraSit();
        }
    }

    private bool IsCloseEnough(RaycastHit hit)
    {
        float dist = Vector3.Distance(Player.Instance.transform.position, hit.point);
        return dist <= Player.Instance.RayCastDistance;
    }

    private void SetCursor(Sprite sprite, string text = "", string color = "white")
    {
        if (!Player.Instance.gameObject.activeSelf) return;
        
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
