using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NoraKinematics : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [Tooltip("Your FirstPersonLook component")]
    [SerializeField] private FirstPersonLook look;

    [Header("Tuning")]
    [Range(0f, 1f)] public float torsoFollow = 0.65f;

    [Header("Strength")]
    public float torsoStrengthMultiplier = 1.0f;

    [Header("Phone Camera")]
    public float phoneCameraExtraMultiplier = 1.15f;
    public float phoneCameraSmooth = 25f;

    [Header("Pitch Limits")]
    public float maxTorsoUp = 12f;
    public float maxTorsoDown = 18f;
    public float phoneCameraMaxUp = 55f;
    public float phoneCameraMaxDown = 60f;

    [Header("Core Body Weights")]
    [Range(0f, 1f)] public float hipsWeight = 0.45f;
    [Range(0f, 1f)] public float spineWeight = 0.35f;
    [Range(0f, 1f)] public float chestWeight = 0.25f;
    [Range(0f, 1f)] public float upperChestWeight = 0.15f;

    [Header("Main Pitch Weights")]
    [Range(0f, 1f)] public float upperLegWeight = 1.0f;
    [Range(0f, 1f)] public float lowerLegWeight = 0.75f;
    [Range(0f, 1f)] public float shoulderWeight = 0.75f;
    [Range(0f, 1f)] public float upperArmWeight = 0.7f;
    [Range(0f, 1f)] public float lowerArmWeight = 0.6f;
    [Range(0f, 1f)] public float neckWeight = 0.95f;
    [Range(0f, 1f)] public float headWeight = 1.0f;

    [Header("Hands & Feet Weights")]
    [Range(0f, 1f)] public float handWeight = 0.75f;
    [Range(0f, 1f)] public float footWeight = 0.7f;

    [Header("3-Axis Fine Control - Torso")]
    public float hipsPitchBonus = 0f; public float hipsYawBonus = 0f; public float hipsRollBonus = 0f;
    public float spinePitchBonus = 0f; public float spineYawBonus = 0f; public float spineRollBonus = 0f;
    public float chestPitchBonus = 0f; public float chestYawBonus = 0f; public float chestRollBonus = 0f;
    public float upperChestPitchBonus = 0f; public float upperChestYawBonus = 0f; public float upperChestRollBonus = 0f;

    [Header("3-Axis Fine Control - Head & Neck")]
    public float neckPitchBonus = 0f; public float neckYawBonus = 0f; public float neckRollBonus = 0f;
    public float headPitchBonus = 0f; public float headYawBonus = 0f; public float headRollBonus = 0f;

    [Header("3-Axis Fine Control - Left Arm")]
    public float leftUpperArmPitchBonus = 0f; public float leftUpperArmYawBonus = 0f; public float leftUpperArmRollBonus = 0f;
    public float leftLowerArmPitchBonus = 0f; public float leftLowerArmYawBonus = 0f; public float leftLowerArmRollBonus = 0f;

    [Header("3-Axis Fine Control - Right Arm")]
    public float rightUpperArmPitchBonus = 0f; public float rightUpperArmYawBonus = 0f; public float rightUpperArmRollBonus = 0f;
    public float rightLowerArmPitchBonus = 0f; public float rightLowerArmYawBonus = 0f; public float rightLowerArmRollBonus = 0f;

    [Header("3-Axis Fine Control - Left Leg")]
    public float leftUpperLegPitchBonus = 0f; public float leftUpperLegYawBonus = 0f; public float leftUpperLegRollBonus = 0f;
    public float leftLowerLegPitchBonus = 0f; public float leftLowerLegYawBonus = 0f; public float leftLowerLegRollBonus = 0f;

    [Header("3-Axis Fine Control - Right Leg")]
    public float rightUpperLegPitchBonus = 0f; public float rightUpperLegYawBonus = 0f; public float rightUpperLegRollBonus = 0f;
    public float rightLowerLegPitchBonus = 0f; public float rightLowerLegYawBonus = 0f; public float rightLowerLegRollBonus = 0f;

    [Header("3-Axis Fine Control - Hands & Feet")]
    public float leftHandPitchBonus = 0f; public float leftHandYawBonus = 0f; public float leftHandRollBonus = 0f;
    public float rightHandPitchBonus = 0f; public float rightHandYawBonus = 0f; public float rightHandRollBonus = 0f;
    public float leftFootPitchBonus = 0f; public float leftFootYawBonus = 0f; public float leftFootRollBonus = 0f;
    public float rightFootPitchBonus = 0f; public float rightFootYawBonus = 0f; public float rightFootRollBonus = 0f;

    [Header("Dedicated Spread Controls (Negative values often best)")]
    public float armSpreadBonus = 35f;
    public float upperLegSpreadBonus = 25f;
    public float lowerLegSpreadBonus = 20f;

    [Header("Crawl Settings")]
    public float maxRootDrop = 0.8f;
    public float armForwardBonus = 48f;

    // Bone Transforms
    private Transform _hips, _spine, _chest, _upperChest;
    private Transform _leftUpperLeg, _rightUpperLeg, _leftLowerLeg, _rightLowerLeg;
    private Transform _leftShoulder, _rightShoulder, _leftUpperArm, _rightUpperArm, _leftLowerArm, _rightLowerArm;
    private Transform _neck, _head;
    private Transform _leftHand, _rightHand;
    private Transform _leftFoot, _rightFoot;

    private float _currentTorsoPitch;
    private Vector3 _originalRootPosition;

    private void Reset() => animator = GetComponent<Animator>();

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        CacheBones();
        _originalRootPosition = transform.localPosition;
    }

    private void CacheBones()
    {
        if (!animator || !animator.isHuman) return;

        _hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        _spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        _chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        _upperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);

        _leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        _rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        _leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        _rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

        _leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        _rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
        _leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        _rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        _leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        _rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

        _neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        _head = animator.GetBoneTransform(HumanBodyBones.Head);

        _leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        _leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    private void LateUpdate()
    {
        if (!animator || !animator.isHuman || look == null) return;
        if (!IsPhoneCameraMode() && GameMaster.Instance?.PLAYERBUSY == true) return;

        float cameraPitch = look.CurrentPitch;
        float targetPitch = -cameraPitch * torsoFollow * torsoStrengthMultiplier;

        if (IsPhoneCameraMode())
        {
            targetPitch *= phoneCameraExtraMultiplier;
            targetPitch = Mathf.Clamp(targetPitch, -phoneCameraMaxDown, phoneCameraMaxUp);
            _currentTorsoPitch = Mathf.Lerp(_currentTorsoPitch, targetPitch, 1f - Mathf.Exp(-phoneCameraSmooth * Time.deltaTime));
        }
        else
        {
            targetPitch = Mathf.Clamp(targetPitch, -maxTorsoDown, maxTorsoUp);
            _currentTorsoPitch = Mathf.Lerp(_currentTorsoPitch, targetPitch, 1f - Mathf.Exp(-12f * Time.deltaTime));
        }

        ApplyFullBodyPitch();
    }

    private void ApplyFullBodyPitch()
    {
        float pitch = _currentTorsoPitch;
        float leanAmount = Mathf.Clamp01(Mathf.Max(0f, pitch) / 50f);

        // Root Drop
        float rootDrop = Mathf.Lerp(0f, maxRootDrop, leanAmount);
        transform.localPosition = _originalRootPosition - new Vector3(0, rootDrop, 0);

        // Main Torso Pitch
        ApplyPitch(_hips, hipsWeight, pitch);
        ApplyPitch(_spine, spineWeight, pitch);
        ApplyPitch(_chest, chestWeight, pitch);
        ApplyPitch(_upperChest, upperChestWeight, pitch);

        // Main Limb Pitch
        float armBonus = Mathf.Max(0f, pitch) * (armForwardBonus / 60f);

        ApplyPitch(_leftUpperLeg, upperLegWeight, pitch);
        ApplyPitch(_rightUpperLeg, upperLegWeight, pitch);
        ApplyPitch(_leftLowerLeg, lowerLegWeight, pitch);
        ApplyPitch(_rightLowerLeg, lowerLegWeight, pitch);

        ApplyPitch(_leftShoulder, shoulderWeight, pitch + armBonus);
        ApplyPitch(_rightShoulder, shoulderWeight, pitch + armBonus);
        ApplyPitch(_leftUpperArm, upperArmWeight, pitch + armBonus);
        ApplyPitch(_rightUpperArm, upperArmWeight, pitch + armBonus);
        ApplyPitch(_leftLowerArm, lowerArmWeight, pitch + armBonus);
        ApplyPitch(_rightLowerArm, lowerArmWeight, pitch + armBonus);

        ApplyPitch(_neck, neckWeight, pitch);
        ApplyPitch(_head, headWeight, pitch);

        // ====================== 3-AXIS FINE CONTROL ======================
        float factor = leanAmount;

        // Torso
        ApplyPitch(_hips, hipsWeight, hipsPitchBonus * factor);
        ApplyRotation(_hips, hipsWeight, hipsYawBonus * factor, Vector3.up);
        ApplyRotation(_hips, hipsWeight, hipsRollBonus * factor, Vector3.forward);

        ApplyPitch(_spine, spineWeight, spinePitchBonus * factor);
        ApplyRotation(_spine, spineWeight, spineYawBonus * factor, Vector3.up);
        ApplyRotation(_spine, spineWeight, spineRollBonus * factor, Vector3.forward);

        ApplyPitch(_chest, chestWeight, chestPitchBonus * factor);
        ApplyRotation(_chest, chestWeight, chestYawBonus * factor, Vector3.up);
        ApplyRotation(_chest, chestWeight, chestRollBonus * factor, Vector3.forward);

        ApplyPitch(_upperChest, upperChestWeight, upperChestPitchBonus * factor);
        ApplyRotation(_upperChest, upperChestWeight, upperChestYawBonus * factor, Vector3.up);
        ApplyRotation(_upperChest, upperChestWeight, upperChestRollBonus * factor, Vector3.forward);

        // Head & Neck
        ApplyPitch(_neck, neckWeight, neckPitchBonus * factor);
        ApplyRotation(_neck, neckWeight, neckYawBonus * factor, Vector3.up);
        ApplyRotation(_neck, neckWeight, neckRollBonus * factor, Vector3.forward);

        ApplyPitch(_head, headWeight, headPitchBonus * factor);
        ApplyRotation(_head, headWeight, headYawBonus * factor, Vector3.up);
        ApplyRotation(_head, headWeight, headRollBonus * factor, Vector3.forward);

        // Left Arm
        ApplyPitch(_leftUpperArm, upperArmWeight, leftUpperArmPitchBonus * factor);
        ApplyRotation(_leftUpperArm, upperArmWeight, leftUpperArmYawBonus * factor, Vector3.up);
        ApplyRotation(_leftUpperArm, upperArmWeight, leftUpperArmRollBonus * factor, Vector3.forward);

        ApplyPitch(_leftLowerArm, lowerArmWeight, leftLowerArmPitchBonus * factor);
        ApplyRotation(_leftLowerArm, lowerArmWeight, leftLowerArmYawBonus * factor, Vector3.up);
        ApplyRotation(_leftLowerArm, lowerArmWeight, leftLowerArmRollBonus * factor, Vector3.forward);

        // Right Arm
        ApplyPitch(_rightUpperArm, upperArmWeight, rightUpperArmPitchBonus * factor);
        ApplyRotation(_rightUpperArm, upperArmWeight, rightUpperArmYawBonus * factor, Vector3.up);
        ApplyRotation(_rightUpperArm, upperArmWeight, rightUpperArmRollBonus * factor, Vector3.forward);

        ApplyPitch(_rightLowerArm, lowerArmWeight, rightLowerArmPitchBonus * factor);
        ApplyRotation(_rightLowerArm, lowerArmWeight, rightLowerArmYawBonus * factor, Vector3.up);
        ApplyRotation(_rightLowerArm, lowerArmWeight, rightLowerArmRollBonus * factor, Vector3.forward);

        // Left Leg
        ApplyPitch(_leftUpperLeg, upperLegWeight, leftUpperLegPitchBonus * factor);
        ApplyRotation(_leftUpperLeg, upperLegWeight, leftUpperLegYawBonus * factor, Vector3.up);
        ApplyRotation(_leftUpperLeg, upperLegWeight, leftUpperLegRollBonus * factor, Vector3.forward);

        ApplyPitch(_leftLowerLeg, lowerLegWeight, leftLowerLegPitchBonus * factor);
        ApplyRotation(_leftLowerLeg, lowerLegWeight, leftLowerLegYawBonus * factor, Vector3.up);
        ApplyRotation(_leftLowerLeg, lowerLegWeight, leftLowerLegRollBonus * factor, Vector3.forward);

        // Right Leg
        ApplyPitch(_rightUpperLeg, upperLegWeight, rightUpperLegPitchBonus * factor);
        ApplyRotation(_rightUpperLeg, upperLegWeight, rightUpperLegYawBonus * factor, Vector3.up);
        ApplyRotation(_rightUpperLeg, upperLegWeight, rightUpperLegRollBonus * factor, Vector3.forward);

        ApplyPitch(_rightLowerLeg, lowerLegWeight, rightLowerLegPitchBonus * factor);
        ApplyRotation(_rightLowerLeg, lowerLegWeight, rightLowerLegYawBonus * factor, Vector3.up);
        ApplyRotation(_rightLowerLeg, lowerLegWeight, rightLowerLegRollBonus * factor, Vector3.forward);

        // Hands & Feet
        ApplyPitch(_leftHand, handWeight, leftHandPitchBonus * factor);
        ApplyRotation(_leftHand, handWeight, leftHandYawBonus * factor, Vector3.up);
        ApplyRotation(_leftHand, handWeight, leftHandRollBonus * factor, Vector3.forward);

        ApplyPitch(_rightHand, handWeight, rightHandPitchBonus * factor);
        ApplyRotation(_rightHand, handWeight, rightHandYawBonus * factor, Vector3.up);
        ApplyRotation(_rightHand, handWeight, rightHandRollBonus * factor, Vector3.forward);

        ApplyPitch(_leftFoot, footWeight, leftFootPitchBonus * factor);
        ApplyRotation(_leftFoot, footWeight, leftFootYawBonus * factor, Vector3.up);
        ApplyRotation(_leftFoot, footWeight, leftFootRollBonus * factor, Vector3.forward);

        ApplyPitch(_rightFoot, footWeight, rightFootPitchBonus * factor);
        ApplyRotation(_rightFoot, footWeight, rightFootYawBonus * factor, Vector3.up);
        ApplyRotation(_rightFoot, footWeight, rightFootRollBonus * factor, Vector3.forward);

        // Dedicated Spread
        float spreadFactor = leanAmount;
        float armSpread = spreadFactor * armSpreadBonus;
        float uLegSpread = spreadFactor * upperLegSpreadBonus;
        float lLegSpread = spreadFactor * lowerLegSpreadBonus;

        ApplyRotation(_leftUpperArm, 0.85f, armSpread, Vector3.forward);
        ApplyRotation(_rightUpperArm, 0.85f, -armSpread, Vector3.forward);

        ApplyRotation(_leftUpperLeg, upperLegWeight, uLegSpread, Vector3.forward);
        ApplyRotation(_rightUpperLeg, upperLegWeight, -uLegSpread, Vector3.forward);

        ApplyRotation(_leftLowerLeg, lowerLegWeight, lLegSpread, Vector3.forward);
        ApplyRotation(_rightLowerLeg, lowerLegWeight, -lLegSpread, Vector3.forward);
    }

    private static void ApplyPitch(Transform bone, float weight, float degrees)
    {
        if (!bone || weight <= 0.001f) return;
        Quaternion animated = bone.localRotation;
        bone.localRotation = animated * Quaternion.AngleAxis(degrees * weight, Vector3.right);
    }

    private static void ApplyRotation(Transform bone, float weight, float degrees, Vector3 axis)
    {
        if (!bone || weight <= 0.001f) return;
        Quaternion animated = bone.localRotation;
        bone.localRotation = animated * Quaternion.AngleAxis(degrees * weight, axis);
    }

    private bool IsPhoneCameraMode()
    {
        return Player.Instance?.PlayerPhone?.CameraOpen == true;
    }

    // ====================== SAVE / LOAD SYSTEM ======================
[System.Serializable]
    private class KinematicState
    {
        // (All fields as in your previous version - unchanged)
        public float torsoFollow;
        public float torsoStrengthMultiplier;
        public float phoneCameraExtraMultiplier;
        public float phoneCameraSmooth;
        public float maxTorsoUp;
        public float maxTorsoDown;
        public float phoneCameraMaxUp;
        public float phoneCameraMaxDown;

        public float hipsWeight;
        public float spineWeight;
        public float chestWeight;
        public float upperChestWeight;

        public float upperLegWeight;
        public float lowerLegWeight;
        public float shoulderWeight;
        public float upperArmWeight;
        public float lowerArmWeight;
        public float neckWeight;
        public float headWeight;
        public float handWeight;
        public float footWeight;

        public float hipsPitchBonus; public float hipsYawBonus; public float hipsRollBonus;
        public float spinePitchBonus; public float spineYawBonus; public float spineRollBonus;
        public float chestPitchBonus; public float chestYawBonus; public float chestRollBonus;
        public float upperChestPitchBonus; public float upperChestYawBonus; public float upperChestRollBonus;

        public float neckPitchBonus; public float neckYawBonus; public float neckRollBonus;
        public float headPitchBonus; public float headYawBonus; public float headRollBonus;

        public float leftUpperArmPitchBonus; public float leftUpperArmYawBonus; public float leftUpperArmRollBonus;
        public float leftLowerArmPitchBonus; public float leftLowerArmYawBonus; public float leftLowerArmRollBonus;
        public float rightUpperArmPitchBonus; public float rightUpperArmYawBonus; public float rightUpperArmRollBonus;
        public float rightLowerArmPitchBonus; public float rightLowerArmYawBonus; public float rightLowerArmRollBonus;

        public float leftUpperLegPitchBonus; public float leftUpperLegYawBonus; public float leftUpperLegRollBonus;
        public float leftLowerLegPitchBonus; public float leftLowerLegYawBonus; public float leftLowerLegRollBonus;
        public float rightUpperLegPitchBonus; public float rightUpperLegYawBonus; public float rightUpperLegRollBonus;
        public float rightLowerLegPitchBonus; public float rightLowerLegYawBonus; public float rightLowerLegRollBonus;

        public float leftHandPitchBonus; public float leftHandYawBonus; public float leftHandRollBonus;
        public float rightHandPitchBonus; public float rightHandYawBonus; public float rightHandRollBonus;

        public float leftFootPitchBonus; public float leftFootYawBonus; public float leftFootRollBonus;
        public float rightFootPitchBonus; public float rightFootYawBonus; public float rightFootRollBonus;

        public float armSpreadBonus;
        public float upperLegSpreadBonus;
        public float lowerLegSpreadBonus;

        public float maxRootDrop;
        public float armForwardBonus;
    }

    public void SaveKinematicState(string customFileName = null)
    {
        KinematicState state = new KinematicState
        {
            torsoFollow = torsoFollow,
            torsoStrengthMultiplier = torsoStrengthMultiplier,
            phoneCameraExtraMultiplier = phoneCameraExtraMultiplier,
            phoneCameraSmooth = phoneCameraSmooth,
            maxTorsoUp = maxTorsoUp,
            maxTorsoDown = maxTorsoDown,
            phoneCameraMaxUp = phoneCameraMaxUp,
            phoneCameraMaxDown = phoneCameraMaxDown,

            hipsWeight = hipsWeight,
            spineWeight = spineWeight,
            chestWeight = chestWeight,
            upperChestWeight = upperChestWeight,

            upperLegWeight = upperLegWeight,
            lowerLegWeight = lowerLegWeight,
            shoulderWeight = shoulderWeight,
            upperArmWeight = upperArmWeight,
            lowerArmWeight = lowerArmWeight,
            neckWeight = neckWeight,
            headWeight = headWeight,
            handWeight = handWeight,
            footWeight = footWeight,

            hipsPitchBonus = hipsPitchBonus, hipsYawBonus = hipsYawBonus, hipsRollBonus = hipsRollBonus,
            spinePitchBonus = spinePitchBonus, spineYawBonus = spineYawBonus, spineRollBonus = spineRollBonus,
            chestPitchBonus = chestPitchBonus, chestYawBonus = chestYawBonus, chestRollBonus = chestRollBonus,
            upperChestPitchBonus = upperChestPitchBonus, upperChestYawBonus = upperChestYawBonus, upperChestRollBonus = upperChestRollBonus,

            neckPitchBonus = neckPitchBonus, neckYawBonus = neckYawBonus, neckRollBonus = neckRollBonus,
            headPitchBonus = headPitchBonus, headYawBonus = headYawBonus, headRollBonus = headRollBonus,

            leftUpperArmPitchBonus = leftUpperArmPitchBonus, leftUpperArmYawBonus = leftUpperArmYawBonus, leftUpperArmRollBonus = leftUpperArmRollBonus,
            leftLowerArmPitchBonus = leftLowerArmPitchBonus, leftLowerArmYawBonus = leftLowerArmYawBonus, leftLowerArmRollBonus = leftLowerArmRollBonus,
            rightUpperArmPitchBonus = rightUpperArmPitchBonus, rightUpperArmYawBonus = rightUpperArmYawBonus, rightUpperArmRollBonus = rightUpperArmRollBonus,
            rightLowerArmPitchBonus = rightLowerArmPitchBonus, rightLowerArmYawBonus = rightLowerArmYawBonus, rightLowerArmRollBonus = rightLowerArmRollBonus,

            leftUpperLegPitchBonus = leftUpperLegPitchBonus, leftUpperLegYawBonus = leftUpperLegYawBonus, leftUpperLegRollBonus = leftUpperLegRollBonus,
            leftLowerLegPitchBonus = leftLowerLegPitchBonus, leftLowerLegYawBonus = leftLowerLegYawBonus, leftLowerLegRollBonus = leftLowerLegRollBonus,
            rightUpperLegPitchBonus = rightUpperLegPitchBonus, rightUpperLegYawBonus = rightUpperLegYawBonus, rightUpperLegRollBonus = rightUpperLegRollBonus,
            rightLowerLegPitchBonus = rightLowerLegPitchBonus, rightLowerLegYawBonus = rightLowerLegYawBonus, rightLowerLegRollBonus = rightLowerLegRollBonus,

            leftHandPitchBonus = leftHandPitchBonus, leftHandYawBonus = leftHandYawBonus, leftHandRollBonus = leftHandRollBonus,
            rightHandPitchBonus = rightHandPitchBonus, rightHandYawBonus = rightHandYawBonus, rightHandRollBonus = rightHandRollBonus,

            leftFootPitchBonus = leftFootPitchBonus, leftFootYawBonus = leftFootYawBonus, leftFootRollBonus = leftFootRollBonus,
            rightFootPitchBonus = rightFootPitchBonus, rightFootYawBonus = rightFootYawBonus, rightFootRollBonus = rightFootRollBonus,

            armSpreadBonus = armSpreadBonus,
            upperLegSpreadBonus = upperLegSpreadBonus,
            lowerLegSpreadBonus = lowerLegSpreadBonus,

            maxRootDrop = maxRootDrop,
            armForwardBonus = armForwardBonus
        };

        string folderPath = Path.Combine(Application.persistentDataPath, "Kinematics");
        Directory.CreateDirectory(folderPath);

        string fileName;
        if (string.IsNullOrWhiteSpace(customFileName))
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            fileName = $"UpperBodyKinematic_{timestamp}.json";
        }
        else
        {
            fileName = customFileName.EndsWith(".json") ? customFileName : customFileName + ".json";
        }

        string fullPath = Path.Combine(folderPath, fileName);
        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(fullPath, json);

        Debug.Log($"✅ Saved: {fullPath}");
    }

    public void LoadKinematicState(string filePath)
    {
        if (!File.Exists(filePath)) return;

        string json = File.ReadAllText(filePath);
        KinematicState state = JsonUtility.FromJson<KinematicState>(json);

        torsoFollow = state.torsoFollow;
        torsoStrengthMultiplier = state.torsoStrengthMultiplier;
        phoneCameraExtraMultiplier = state.phoneCameraExtraMultiplier;
        phoneCameraSmooth = state.phoneCameraSmooth;
        maxTorsoUp = state.maxTorsoUp;
        maxTorsoDown = state.maxTorsoDown;
        phoneCameraMaxUp = state.phoneCameraMaxUp;
        phoneCameraMaxDown = state.phoneCameraMaxDown;

        hipsWeight = state.hipsWeight;
        spineWeight = state.spineWeight;
        chestWeight = state.chestWeight;
        upperChestWeight = state.upperChestWeight;

        upperLegWeight = state.upperLegWeight;
        lowerLegWeight = state.lowerLegWeight;
        shoulderWeight = state.shoulderWeight;
        upperArmWeight = state.upperArmWeight;
        lowerArmWeight = state.lowerArmWeight;
        neckWeight = state.neckWeight;
        headWeight = state.headWeight;
        handWeight = state.handWeight;
        footWeight = state.footWeight;

        hipsPitchBonus = state.hipsPitchBonus; hipsYawBonus = state.hipsYawBonus; hipsRollBonus = state.hipsRollBonus;
        spinePitchBonus = state.spinePitchBonus; spineYawBonus = state.spineYawBonus; spineRollBonus = state.spineRollBonus;
        chestPitchBonus = state.chestPitchBonus; chestYawBonus = state.chestYawBonus; chestRollBonus = state.chestRollBonus;
        upperChestPitchBonus = state.upperChestPitchBonus; upperChestYawBonus = state.upperChestYawBonus; upperChestRollBonus = state.upperChestRollBonus;

        neckPitchBonus = state.neckPitchBonus; neckYawBonus = state.neckYawBonus; neckRollBonus = state.neckRollBonus;
        headPitchBonus = state.headPitchBonus; headYawBonus = state.headYawBonus; headRollBonus = state.headRollBonus;

        leftUpperArmPitchBonus = state.leftUpperArmPitchBonus; leftUpperArmYawBonus = state.leftUpperArmYawBonus; leftUpperArmRollBonus = state.leftUpperArmRollBonus;
        leftLowerArmPitchBonus = state.leftLowerArmPitchBonus; leftLowerArmYawBonus = state.leftLowerArmYawBonus; leftLowerArmRollBonus = state.leftLowerArmRollBonus;
        rightUpperArmPitchBonus = state.rightUpperArmPitchBonus; rightUpperArmYawBonus = state.rightUpperArmYawBonus; rightUpperArmRollBonus = state.rightUpperArmRollBonus;
        rightLowerArmPitchBonus = state.rightLowerArmPitchBonus; rightLowerArmYawBonus = state.rightLowerArmYawBonus; rightLowerArmRollBonus = state.rightLowerArmRollBonus;

        leftUpperLegPitchBonus = state.leftUpperLegPitchBonus; leftUpperLegYawBonus = state.leftUpperLegYawBonus; leftUpperLegRollBonus = state.leftUpperLegRollBonus;
        leftLowerLegPitchBonus = state.leftLowerLegPitchBonus; leftLowerLegYawBonus = state.leftLowerLegYawBonus; leftLowerLegRollBonus = state.leftLowerLegRollBonus;
        rightUpperLegPitchBonus = state.rightUpperLegPitchBonus; rightUpperLegYawBonus = state.rightUpperLegYawBonus; rightUpperLegRollBonus = state.rightUpperLegRollBonus;
        rightLowerLegPitchBonus = state.rightLowerLegPitchBonus; rightLowerLegYawBonus = state.rightLowerLegYawBonus; rightLowerLegRollBonus = state.rightLowerLegRollBonus;

        leftHandPitchBonus = state.leftHandPitchBonus; leftHandYawBonus = state.leftHandYawBonus; leftHandRollBonus = state.leftHandRollBonus;
        rightHandPitchBonus = state.rightHandPitchBonus; rightHandYawBonus = state.rightHandYawBonus; rightHandRollBonus = state.rightHandRollBonus;

        leftFootPitchBonus = state.leftFootPitchBonus; leftFootYawBonus = state.leftFootYawBonus; leftFootRollBonus = state.leftFootRollBonus;
        rightFootPitchBonus = state.rightFootPitchBonus; rightFootYawBonus = state.rightFootYawBonus; rightFootRollBonus = state.rightFootRollBonus;

        armSpreadBonus = state.armSpreadBonus;
        upperLegSpreadBonus = state.upperLegSpreadBonus;
        lowerLegSpreadBonus = state.lowerLegSpreadBonus;

        maxRootDrop = state.maxRootDrop;
        armForwardBonus = state.armForwardBonus;

        Debug.Log($"✅ Loaded: {Path.GetFileName(filePath)}");
    }
    
    
    
    
    
    
    public void DeleteKinematicState(string filePath)
    {
        if (File.Exists(filePath) && EditorUtility.DisplayDialog("Delete File?", 
                $"Are you sure you want to delete:\n{Path.GetFileName(filePath)}?", "Yes", "Cancel"))
        {
            File.Delete(filePath);
            Debug.Log($"🗑 Deleted: {Path.GetFileName(filePath)}");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NoraKinematics))]
public class NoraKinematicsEditor : Editor
{
    private string customFileName = "MyKinematicPreset";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Kinematic Save / Load System", EditorStyles.boldLabel);

        // === Save Section ===
        customFileName = EditorGUILayout.TextField("Custom Filename", customFileName);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save with Custom Name", GUILayout.Height(35)))
        {
            ((NoraKinematics)target).SaveKinematicState(customFileName);
        }

        if (GUILayout.Button("Quick Save (Timestamp)", GUILayout.Height(35)))
        {
            ((NoraKinematics)target).SaveKinematicState();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Saved Files", EditorStyles.boldLabel);

        string folderPath = Path.Combine(Application.persistentDataPath, "Kinematics");

        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath, "*.json");

            if (files.Length == 0)
            {
                EditorGUILayout.HelpBox("No saved files yet.", MessageType.Info);
            }
            else
            {
                foreach (string file in files)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(Path.GetFileName(file), GUILayout.Width(280));

                    if (GUILayout.Button("Load", GUILayout.Width(50)))
                    {
                        ((NoraKinematics)target).LoadKinematicState(file);
                        EditorUtility.SetDirty(target);
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        ((NoraKinematics)target).DeleteKinematicState(file);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Kinematics folder will be created on first save.", MessageType.Info);
        }
    }
}
#endif