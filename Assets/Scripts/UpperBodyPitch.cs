using UnityEngine;

public class UpperBodyPitch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [Tooltip("Your FirstPersonLook (to read pitch).")]
    [SerializeField] private FirstPersonLook look;

    [Header("Tuning")]
    [Tooltip("How much the torso follows pitch. 0.25 = subtle.")]
    [Range(0f, 1f)] public float torsoFollow = 0.25f;

    [Tooltip("Clamp torso pitch in degrees (keeps it natural).")]
    public float maxTorsoUp = 10f;
    public float maxTorsoDown = 15f;

    [Tooltip("Smoothing for the applied torso pitch.")]
    public float smooth = 12f;

    [Header("Bones (Humanoid)")]
    public HumanBodyBones spineBone = HumanBodyBones.Spine;
    public HumanBodyBones chestBone = HumanBodyBones.Chest;
    public HumanBodyBones upperChestBone = HumanBodyBones.UpperChest;

    [Header("Per-bone weight (adds up to a nice curve)")]
    [Range(0f, 1f)] public float spineWeight = 0.35f;
    [Range(0f, 1f)] public float chestWeight = 0.45f;
    [Range(0f, 1f)] public float upperChestWeight = 0.20f;

    private Transform _spine, _chest, _upperChest;

    private Quaternion _spineBase, _chestBase, _upperChestBase;
    private float _currentTorsoPitch;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        CacheBones();
    }

    private void CacheBones()
    {
        if (!animator || !animator.isHuman) return;

        _spine = animator.GetBoneTransform(spineBone);
        _chest = animator.GetBoneTransform(chestBone);
        _upperChest = animator.GetBoneTransform(upperChestBone);

        // Note: we capture "base" rotations each frame instead of once,
        // because animations change them.
    }

    private void LateUpdate()
    {
        if (!animator || !animator.isHuman) return;
        if (!look) return;

        // You rotate camera with -currentMouseLook.y, so:
        // look pitch down = positive mouseLook.y, camera rotates negative X.
        // We'll treat "look up" as positive torso pitch.
        float rawLookPitch = GetLookPitchDegrees();

        // Convert look pitch into a subtle torso pitch
        float targetTorsoPitch = -rawLookPitch * torsoFollow;

        // Clamp so it stays natural
        targetTorsoPitch = Mathf.Clamp(targetTorsoPitch, -maxTorsoDown, maxTorsoUp);

        _currentTorsoPitch = Mathf.Lerp(_currentTorsoPitch, targetTorsoPitch, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        // Capture animated rotations THIS frame, then add our offset
        ApplyAdditivePitch(_spine, spineWeight, _currentTorsoPitch);
        ApplyAdditivePitch(_chest, chestWeight, _currentTorsoPitch);
        ApplyAdditivePitch(_upperChest, upperChestWeight, _currentTorsoPitch);
    }

    // If you don't want to change FirstPersonLook, you can expose pitch through a property there.
    // For now, simplest: compute from camera local rotation.
    private float GetLookPitchDegrees()
    {
        // If your FirstPersonLook is on the camera pivot:
        // transform.localRotation = AngleAxis(-currentMouseLook.y, Vector3.right)
        // So pitchDegrees = -cameraLocalX
        float x = look.transform.localEulerAngles.x;
        if (x > 180f) x -= 360f; // convert to -180..180
        return -x;
    }

    private static void ApplyAdditivePitch(Transform bone, float weight, float pitchDeg)
    {
        if (!bone || weight <= 0f) return;

        // Axis is local X of the bone (bend forward/back)
        Quaternion animated = bone.localRotation;
        Quaternion offset = Quaternion.AngleAxis(pitchDeg * weight, Vector3.right);

        bone.localRotation = animated * offset;
    }
}