using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class WallMeater : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        CreateWallStuff();
    }

    private void CreateWallStuff()
    {
        if (WallMeatManager.Instance == null)
        {
            return;
        }

        Material material = WallMeatManager.Instance.GetRandomWallMeat();

        if (material != null)
        {
            meshRenderer.material = material;
        }
    }
}