using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "DungeonObstacles", menuName = "{!!} Tawley Scriptable Object/DungeonObstacles", order = 10)]
public class DungeonObstacles : ScriptableObject
{
    [Header("Dungeon Obstacle Details")] 
    public ObstacleType ObstacleType = ObstacleType.RedBarrel;

    public GameObject ObstaclePrefab;

    public ObstaclePlace ObstaclePlace = ObstaclePlace.Floor;
    
    public float MinContiguous = 0.0f;
    
    public float MaxContiguous = 0.0f;

    public float ObjectYOffset = 0.0f;
    
    public bool KnockoutPreordainedTile = false;
    
    public string TemplateMappedCharacter = "@";

}