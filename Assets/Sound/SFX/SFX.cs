using UnityEngine;

[CreateAssetMenu(
    fileName = "SFX",
    menuName = "{!!} Tawley Scriptable Object/SFX SO",
    order = 11)]
public class SFX : ScriptableObject
{
    [Header("Audio Parameters")]
    public SFXResource AudioResource = SFXResource.TypeWriter0;

    [Header("Audio Clip (from Resources or Addressables)")]
    public AudioClip AudioClip;

    [Header("Volume")] public float SFXVolume = 1f;
}

