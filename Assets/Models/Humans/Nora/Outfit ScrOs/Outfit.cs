using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Outfit",
    menuName = "{!!} Tawley Scriptable Object/Outfit",
    order = 10)]
public class Outfit : ScriptableObject
{
    public OutfitName thisOutfit;
    public OutfitType outfitType;
    public string outfitTitle;
    public bool OutfitEnabled = true;
    public List<GameObject> OutfitPrefabs;
    public Color lipsColor = new Color(0.95f, 0.6f, 0.7f);
    public Color nailsColor = new Color(0.9f, 0.7f, 0.8f);
    public bool Jiggle;
    public bool Wings;
    public bool Apron;
    public bool Hat;
    public bool Choker;
    public HairName Hair = HairName.DefaultHair;
}