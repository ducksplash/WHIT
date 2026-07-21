using UnityEngine;

[CreateAssetMenu(fileName = "Nora", menuName = "{!!} Tawley Scriptable Object/Nora", order = 10)]
public class Nora : ScriptableObject
{
    [Header("Nora Parameters")] 
    public NoraID NoraID = NoraID.zero;
    public OutfitName SelectedOutfit = OutfitName.Work;
    public bool IsDead;





    private void OnEnable()
    {
        Debug.Log("Nora Loaded");
        
    }
    
    
    
}
