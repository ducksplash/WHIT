using UnityEngine;

[CreateAssetMenu(
    fileName = "BGA",
    menuName = "{!!} Tawley Scriptable Object/BGA SO",
    order = 11)]
public class BGA : ScriptableObject
{
    [Header("Audio Parameters")]
    public BGAResource AudioResource = BGAResource.None;

    [Header("Audio Clip (from Resources or Addressables)")]
    public AudioClip AudioClip;

    [Header("Volume")] public float BGAVolume = 1f;
}

