using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;


public class MeshPulsatingGlowUniTask : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color colorA = Color.black;
    [SerializeField] private Color colorB = Color.white;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 1f;

    [Header("Attachment")]
    [Tooltip("Exact name of the material to affect (matches sharedMaterials[i].name)")]
    public string MaterialName;
    public SkinnedMeshRenderer meshRenderer;

    [Header("Glow")]
    [SerializeField] private float glowIntensity = 2f;

    [Header("HDRP")]
    [Tooltip("HDRP Lit emission property. Usually \"_EmissiveColor\".")]
    [SerializeField] private string emissionProperty = "_EmissiveColor";

    private MaterialPropertyBlock propertyBlock;
    private CancellationTokenSource cts;
    private int targetMaterialIndex = -1;

    private void OnEnable()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<SkinnedMeshRenderer>();
        }

        propertyBlock = new MaterialPropertyBlock();
        CacheTargetMaterialIndex();

        cts = new CancellationTokenSource();
        PulseRoutine(cts.Token).Forget();
    }

    private void OnDisable()
    {
        Dispose();
    }

    private void CacheTargetMaterialIndex()
    {
        targetMaterialIndex = -1;

        if (meshRenderer == null || string.IsNullOrEmpty(MaterialName))
            return;

        var materials = meshRenderer.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null && materials[i].name == MaterialName)
            {
                targetMaterialIndex = i;
                return;
            }
        }

        Debug.LogWarning($"[MeshPulsatingGlowUniTask] Material named \"{MaterialName}\" not found on {meshRenderer.name}.", this);
    }

    private async UniTaskVoid PulseRoutine(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (targetMaterialIndex < 0 || meshRenderer == null)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                continue;
            }

            float t = Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f);

            Color finalColor = Color.Lerp(colorA, colorB, t);
            // HDR emission / glow
            finalColor *= glowIntensity;

            // Apply only to the named material slot
            meshRenderer.GetPropertyBlock(propertyBlock, targetMaterialIndex);
            propertyBlock.SetColor(emissionProperty, finalColor);
            meshRenderer.SetPropertyBlock(propertyBlock, targetMaterialIndex);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void OnDestroy()
    {
        //Dispose();
    }

    private void Dispose()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}