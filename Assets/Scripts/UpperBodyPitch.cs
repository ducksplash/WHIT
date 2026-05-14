using UnityEngine;

public class UpperBodyPitch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [Tooltip("Your FirstPersonLook component")]
    [SerializeField] private FirstPersonLook look;

    [Header("Tuning")]
    [Range(0f, 1f)] public float torsoFollow = 0.65f;           // Higher for phone camera

    [Header("Strength")]
    public float torsoStrengthMultiplier = 1.0f;                // Keep near 1.0 for lockstep

    [Header("Phone Camera Lockstep")]
    [Tooltip("Extra multiplier when phone camera is open - for fine tuning match")]
    public float phoneCameraExtraMultiplier = 1.15f;

    [Tooltip("Smoothing - lower = tighter lockstep")]
    public float phoneCameraSmooth = 25f;   // High value = very responsive

    [Header("Pitch Limits")]
    public float maxTorsoUp = 12f;
    public float maxTorsoDown = 18f;

    [Header("Phone Camera Pitch Limits")]
    public float phoneCameraMaxUp = 55f;
    public float phoneCameraMaxDown = 60f;

    [Header("Bones (Humanoid)")]
    public HumanBodyBones spineBone = HumanBodyBones.Spine;
    public HumanBodyBones chestBone = HumanBodyBones.Chest;
    public HumanBodyBones upperChestBone = HumanBodyBones.UpperChest;

    [Header("Per-bone weight")]
    [Range(0f, 1f)] public float spineWeight = 0.35f;
    [Range(0f, 1f)] public float chestWeight = 0.45f;
    [Range(0f, 1f)] public float upperChestWeight = 0.20f;

    private Transform _spine, _chest, _upperChest;
    private float _currentTorsoPitch;

    private void Reset() => animator = GetComponent<Animator>();

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private void Start() => CacheBones();

    private void CacheBones()
    {
        if (!animator || !animator.isHuman) return;

        _spine = animator.GetBoneTransform(spineBone);
        _chest = animator.GetBoneTransform(chestBone);
        _upperChest = animator.GetBoneTransform(upperChestBone);
    }

    private void LateUpdate()
    {
        if (!animator || !animator.isHuman || look == null) return;

        if (!IsPhoneCameraMode() && GameMaster.Instance?.PLAYERBUSY == true)
            return;

        float cameraPitch = look.CurrentPitch;

        float targetTorsoPitch = -cameraPitch * torsoFollow * torsoStrengthMultiplier;

        if (IsPhoneCameraMode())
        {
            targetTorsoPitch *= phoneCameraExtraMultiplier;
            targetTorsoPitch = Mathf.Clamp(targetTorsoPitch, -phoneCameraMaxDown, phoneCameraMaxUp);
            
            // Tight lockstep smoothing
            _currentTorsoPitch = Mathf.Lerp(_currentTorsoPitch, targetTorsoPitch,
                1f - Mathf.Exp(-phoneCameraSmooth * Time.deltaTime));
        }
        else
        {
            targetTorsoPitch = Mathf.Clamp(targetTorsoPitch, -maxTorsoDown, maxTorsoUp);
            _currentTorsoPitch = Mathf.Lerp(_currentTorsoPitch, targetTorsoPitch,
                1f - Mathf.Exp(-11f * Time.deltaTime));
        }

        ApplyAdditivePitch(_spine, spineWeight, _currentTorsoPitch);
        ApplyAdditivePitch(_chest, chestWeight, _currentTorsoPitch);
        ApplyAdditivePitch(_upperChest, upperChestWeight, _currentTorsoPitch);
    }

    private bool IsPhoneCameraMode()
    {
        return Player.Instance?.PlayerPhone?.CameraOpen == true;
    }

    private static void ApplyAdditivePitch(Transform bone, float weight, float pitchDeg)
    {
        if (!bone || weight <= 0f) return;

        Quaternion animated = bone.localRotation;
        Quaternion offset = Quaternion.AngleAxis(pitchDeg * weight, Vector3.right);
        bone.localRotation = animated * offset;
    }
}