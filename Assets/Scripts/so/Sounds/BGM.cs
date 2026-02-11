using UnityEngine;

[CreateAssetMenu(
    fileName = "BGM",
    menuName = "{!!} Tawley Scriptable Object/BGM SO",
    order = 11)]
public class BGM : ScriptableObject
{
    [Header("Audio Parameters")]
    public BGMResource AudioResource = BGMResource.SongOne;

    [Header("Audio Clip (from Resources or Addressables)")]
    public AudioClip AudioClip;

    [Header("Volume")] public float BGMVolume = 1f;
}

public enum BGMResource
{
    SongOne,
    SongTwo
    // add more as needed
}