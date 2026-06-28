using UnityEngine;

public class CharacterProfile : MonoBehaviour
{
    [Header("Character Profile")]
    public string Name;
    public int Age;
    public CharacterGender Gender;    

    [TextArea(6, 10)]
    public string Biography;



}



public enum CharacterGender
{
    Male,
    Female,
    Transgender,
    Other
}