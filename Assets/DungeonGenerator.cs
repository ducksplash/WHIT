using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Random = UnityEngine.Random;

[System.Serializable]
public class RoomTypeConfig
{
    public string typeName;
    public Vector2Int minDimensions;
    public Vector2Int maxDimensions;
    public Material floorMaterial;
    public int minPerFloor;
    public int maxPerFloor;
    public bool containsEntrance;
    public bool containsExit;
    public bool containsRedTelepad;
    public bool containsBlueTelepad;
    public bool containsBoss;
}

public class DungeonGenerator : MonoBehaviour
{
    private static DungeonGenerator _instance;
    public static DungeonGenerator Instance => _instance;
    public bool IsNavMeshReady { get; private set; }

    public int maximumRealtimeLights;
    public int maximumEmissiveLights;
    
    public int gridWidth = 50;
    public int gridHeight = 50;
    public int floorCount = 3;
    public float cellSize = 1f;
    public int minRooms = 5;
    public int maxRooms = 10;
    public int minRoomDistance = 3;
    public float wallHeight = 3f;
    public float ceilingHeight = 3f;
    public bool autoRegenerate = false;
    public Vector3 spawnPoint;
    public int minLoots = 1;
    public int maxLoots = 3;
    public int minEnemies = 1;
    public int maxEnemies = 3;

    [Header("Prefabs")]
    public GameObject wallNorthPrefab;
    public GameObject wallEastPrefab;
    public GameObject wallSouthPrefab;
    public GameObject wallWestPrefab;
    public GameObject floorPrefab;
    public GameObject ceilingPrefab;
    public GameObject entrancePrefab;
    public GameObject exitPrefab;
    public GameObject redTelepadPrefab;
    public GameObject blueTelepadPrefab;
    public GameObject spawnableObjectPrefab;
    public GameObject lootPrefab;
    public GameObject holePrefab;
    public GameObject specialTilePrefab;
    public GameObject ambientLightPrefab;
    public float ambientLightHeightOffset = 100f;

    [Header("Spawn Toggles")]
    public bool spawnLoot = true;
    public bool spawnRedTelepads = true;
    public bool spawnBlueTelepads = true;
    public bool spawnSpecialTiles = true;

    public bool allowBackwardTravel = true;


    [Header("Template Generation")]
    public bool createFromTemplate = false;
    public List<TextAsset> floorTemplates;
    
    public List<Light> lightList;
    
    public LayerMask dungeonMask;

    private List<RoomTypeConfig> _roomTypes = null;


    [Header("Modules")] public SpikeHandler spikeMan;
    
    public int floorTilesCount;
    public int floorTilesCovered;

    public bool IsDungeoning;
    
    public List<RoomTypeConfig> roomTypes
    {
        get
        {
            if (_roomTypes == null)
            {
                _roomTypes = InitializeRoomTypes();
            }
            return _roomTypes;
        }
        set
        {
            _roomTypes = value;
        }
    }

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public int FloorCount => floorCount;
    public float CellSize => cellSize;

    public List<Vector2Int>[] floorCells;
    private Vector3Int entrancePos;
    private Vector3Int exitPos;
    private Vector3Int spawnPos;
    private List<(Vector3Int redPad, Vector3Int bluePad)> telepadLinks;
    private List<Vector3Int> redPads;
    private List<Vector3Int> bluePads;
    private List<Vector3Int> spawnableObjectTiles;
    private List<Vector3Int> enemyTiles;
    private CellType[,,] grid;
    private List<(Vector2Int center, Vector2Int dimensions, RoomTypeConfig roomType)>[] roomsPerFloor;
    public List<GameObject> floorObjects;
    private GameObject ambientLightInstance;

    public CellType[,,] GetGrid() => grid;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        floorObjects = new List<GameObject>();
        enemyTiles = new List<Vector3Int>();
        spawnableObjectTiles = new List<Vector3Int>();
        redPads = new List<Vector3Int>();
        bluePads = new List<Vector3Int>();
        telepadLinks = new List<(Vector3Int redPad, Vector3Int bluePad)>();
        IsNavMeshReady = false;

        if (_roomTypes == null)
        {
            _roomTypes = InitializeRoomTypes();
        }
    }

    void Start()
    {
        if (autoRegenerate)
        {
            StartDungeoning();
        }
    }

    public void StartDungeoning()
    {
        if (IsDungeoning) return;
        IsDungeoning = true;
        StartCoroutine(GenerateDungeonAndSpawn());
    }

    private IEnumerator GenerateDungeonAndSpawn()
    {
        while (GameMaster.Instance == null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        GenerateDungeon();

        yield return new WaitUntil(() => IsNavMeshReady);

        Vector3 playerSpawnPosition = new Vector3(
            entrancePos.x * cellSize,
            entrancePos.z * ceilingHeight + 0.2f,
            entrancePos.y * cellSize
        );

        Player.Instance.SpawnOverride(playerSpawnPosition);

        yield return new WaitForSeconds(2f);

        spikeMan.BeginSpiking();

        while (spikeMan.spiking)
        {
            yield return null;
        }
    }

    public void GenerateDungeon()
    {
        if (createFromTemplate)
        {
            GenerateDungeonFromTemplate();
            return;
        }

        int maxAttempts = 5;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            IsNavMeshReady = false;
            foreach (var floorObj in floorObjects)
            {
                if (floorObj != null) DestroyImmediate(floorObj);
            }
            floorObjects.Clear();

            grid = new CellType[gridWidth, gridHeight, floorCount];
            floorCells = new List<Vector2Int>[floorCount];
            roomsPerFloor = new List<(Vector2Int center, Vector2Int dimensions, RoomTypeConfig roomType)>[floorCount];
            telepadLinks = new List<(Vector3Int redPad, Vector3Int bluePad)>();
            redPads = new List<Vector3Int>();
            bluePads = new List<Vector3Int>();
            spawnableObjectTiles = new List<Vector3Int>();
            enemyTiles = new List<Vector3Int>();
            for (int z = 0; z < floorCount; z++)
            {
                floorCells[z] = new List<Vector2Int>();
                roomsPerFloor[z] = new List<(Vector2Int center, Vector2Int dimensions, RoomTypeConfig roomType)>();
            }

            for (int z = 0; z < floorCount; z++)
            {
                Vector2Int startPos = z == 0 ? new Vector2Int(5, 5) : floorCells[z - 1][Random.Range(0, floorCells[z - 1].Count)];
                if (z > 0 && !IsValidCell(new Vector3Int(startPos.x, startPos.y, z)))
                {
                    startPos = new Vector2Int(5, 5);
                    grid[startPos.x, startPos.y, z] = CellType.Floor;
                    floorCells[z].Add(startPos);
                }
                GeneratePath(startPos, z);
            }

            PlaceSpecialTiles();
            BuildTelepadLinks();
            if (ValidateDungeonPath())
            {
                PlaceCubes();
                SpawnAmbientLight();
                StartCoroutine(BuildNavMeshAsync());
                return;
            }
            
            
            //Debug.Log($"Dungeon generation attempt {attempt + 1} failed. Retrying...");
        }
        //Debug.LogError("Failed to generate a valid dungeon after maximum attempts.");
    }

    void GenerateDungeonFromTemplate()
    {
        if (floorTemplates == null || floorTemplates.Count == 0)
        {
            return;
        }

        IsNavMeshReady = false;
        foreach (var floorObj in floorObjects)
        {
            if (floorObj != null) DestroyImmediate(floorObj);
        }
        floorObjects.Clear();

        floorCount = floorTemplates.Count;

        List<string[]> parsedFloors = new List<string[]>();
        int templateWidth = 0;
        int templateHeight = 0;

        for (int z = 0; z < floorCount; z++)
        {
            TextAsset asset = floorTemplates[z];
            string text = asset != null ? asset.text : string.Empty;
            string[] rawLines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

            List<string> cleanLines = new List<string>();
            foreach (var line in rawLines)
            {
                if (line.Length > 0)
                {
                    cleanLines.Add(line);
                }
            }

            parsedFloors.Add(cleanLines.ToArray());

            templateHeight = Mathf.Max(templateHeight, cleanLines.Count);
            foreach (var line in cleanLines)
            {
                templateWidth = Mathf.Max(templateWidth, line.Length);
            }
        }

        gridWidth = Mathf.Max(1, templateWidth);
        gridHeight = Mathf.Max(1, templateHeight);

        grid = new CellType[gridWidth, gridHeight, floorCount];
        floorCells = new List<Vector2Int>[floorCount];
        roomsPerFloor = new List<(Vector2Int center, Vector2Int dimensions, RoomTypeConfig roomType)>[floorCount];
        telepadLinks = new List<(Vector3Int redPad, Vector3Int bluePad)>();
        redPads = new List<Vector3Int>();
        bluePads = new List<Vector3Int>();
        spawnableObjectTiles = new List<Vector3Int>();
        enemyTiles = new List<Vector3Int>();

        for (int z = 0; z < floorCount; z++)
        {
            floorCells[z] = new List<Vector2Int>();
            roomsPerFloor[z] = new List<(Vector2Int center, Vector2Int dimensions, RoomTypeConfig roomType)>();
        }

        for (int z = 0; z < floorCount; z++)
        {
            string[] lines = parsedFloors[z];

            for (int y = 0; y < gridHeight; y++)
            {
                string line = y < lines.Length ? lines[y] : string.Empty;

                for (int x = 0; x < gridWidth; x++)
                {
                    char symbol = x < line.Length ? line[x] : '#';
                    ApplyTemplateSymbol(symbol, x, y, z);
                }
            }
        }

        BuildTelepadLinks();
        ValidateDungeonPath();
        PlaceCubes();
        SpawnAmbientLight();
        StartCoroutine(BuildNavMeshAsync());
    }

    void ApplyTemplateSymbol(char symbol, int x, int y, int z)
    {
        switch (symbol)
        {
            case '#':
                grid[x, y, z] = CellType.Empty;
                break;

            case '0':
            case '_':
                grid[x, y, z] = CellType.Floor;
                floorCells[z].Add(new Vector2Int(x, y));
                break;

            case 'E':
                grid[x, y, z] = CellType.Entrance;
                floorCells[z].Add(new Vector2Int(x, y));
                entrancePos = new Vector3Int(x, y, z);
                spawnPos = entrancePos;
                break;

            case 'X':
                grid[x, y, z] = CellType.Exit;
                floorCells[z].Add(new Vector2Int(x, y));
                exitPos = new Vector3Int(x, y, z);
                break;

            case 'R':
                if (spawnRedTelepads && z != floorCount - 1)
                {
                    grid[x, y, z] = CellType.RedTelepad;
                    redPads.Add(new Vector3Int(x, y, z));
                }
                else
                {
                    grid[x, y, z] = CellType.Floor;
                }
                floorCells[z].Add(new Vector2Int(x, y));
                break;

            case 'B':
                if (spawnBlueTelepads && z != 0)
                {
                    grid[x, y, z] = CellType.BlueTelepad;
                    bluePads.Add(new Vector3Int(x, y, z));
                }
                else
                {
                    grid[x, y, z] = CellType.Floor;
                }
                floorCells[z].Add(new Vector2Int(x, y));
                break;

            case '$':
                if (spawnLoot)
                {
                    grid[x, y, z] = CellType.SpawnableObject;
                    spawnableObjectTiles.Add(new Vector3Int(x, y, z));
                }
                else
                {
                    grid[x, y, z] = CellType.Floor;
                }
                floorCells[z].Add(new Vector2Int(x, y));
                break;

            case '*':
                if (spawnSpecialTiles)
                {
                    grid[x, y, z] = CellType.Special;
                }
                else
                {
                    grid[x, y, z] = CellType.Floor;
                }
                floorCells[z].Add(new Vector2Int(x, y));
                break;

            default:
                grid[x, y, z] = CellType.Empty;
                break;
        }
    }

    void SpawnAmbientLight()
    {
        if (ambientLightPrefab == null)
        {
            return;
        }

        if (ambientLightInstance != null)
        {
            DestroyImmediate(ambientLightInstance);
        }

        float centerX = (gridWidth * cellSize) / 2f;
        float centerZ = (gridHeight * cellSize) / 2f;
        Vector3 lightPosition = new Vector3(centerX, ambientLightHeightOffset, centerZ);

        ambientLightInstance = Instantiate(ambientLightPrefab, lightPosition, ambientLightPrefab.transform.rotation, transform);
        ambientLightInstance.name = "AmbientAreaLight";
    }

    void OnValidate()
    {
        gridWidth = Mathf.Max(10, gridWidth);
        gridHeight = Mathf.Max(10, gridHeight);
        floorCount = Mathf.Max(1, floorCount);
        cellSize = Mathf.Max(0.1f, cellSize);
        minRooms = Mathf.Max(1, minRooms);
        maxRooms = Mathf.Max(minRooms, maxRooms);
        minRoomDistance = Mathf.Max(1, minRoomDistance);
        wallHeight = Mathf.Max(0.1f, wallHeight);
        ceilingHeight = Mathf.Max(2f, wallHeight);
        minLoots = Mathf.Max(0, minLoots);
        maxLoots = Mathf.Max(minLoots, maxLoots);
        minEnemies = Mathf.Max(0, minEnemies);
        maxEnemies = Mathf.Max(minEnemies, maxEnemies);

        if (_roomTypes == null || _roomTypes.Count == 0)
        {
            _roomTypes = InitializeRoomTypes();
        }

        foreach (var roomType in _roomTypes)
        {
            roomType.minPerFloor = Mathf.Max(0, roomType.minPerFloor);
            roomType.maxPerFloor = Mathf.Max(roomType.minPerFloor, roomType.maxPerFloor);
            roomType.minDimensions.x = Mathf.Max(3, roomType.minDimensions.x);
            roomType.minDimensions.y = Mathf.Max(3, roomType.minDimensions.y);
            roomType.maxDimensions.x = Mathf.Max(roomType.minDimensions.x, roomType.maxDimensions.x);
            roomType.maxDimensions.y = Mathf.Max(roomType.minDimensions.y, roomType.maxDimensions.y);
        }
    }

    void PlaceSpecialTiles()
    {
        spawnableObjectTiles.Clear();
        enemyTiles.Clear();

        for (int z = 0; z < floorCount; z++)
        {
            if (floorCells[z].Count == 0)
            {
                Vector2Int defaultPos = new Vector2Int(5, 5);
                grid[defaultPos.x, defaultPos.y, z] = CellType.Floor;
                floorCells[z].Add(defaultPos);
            }

            List<List<Vector2Int>> components = FindConnectedComponents(z);
            List<Vector2Int> mainComponent = components.OrderByDescending(c => c.Count).First();

            if (spawnLoot)
            {
                int targetLoots = Random.Range(minLoots, maxLoots);
                for (int i = 0; i < targetLoots; i++)
                {
                    bool placed = false;
                    for (int attempt = 0; attempt < 20; attempt++)
                    {
                        Vector2Int cell = mainComponent[Random.Range(0, mainComponent.Count)];
                        Vector3Int pos = new Vector3Int(cell.x, cell.y, z);
                        if (IsValidSpawnableObjectTile(pos))
                        {
                            grid[pos.x, pos.y, pos.z] = CellType.SpawnableObject;
                            spawnableObjectTiles.Add(pos);
                            placed = true;
                            break;
                        }
                    }

                    if (!placed)
                    {
                        foreach (var cell in mainComponent)
                        {
                            Vector3Int pos = new Vector3Int(cell.x, cell.y, z);
                            if (IsValidSpawnableObjectTile(pos))
                            {
                                grid[pos.x, pos.y, pos.z] = CellType.SpawnableObject;
                                spawnableObjectTiles.Add(pos);
                                placed = true;
                                break;
                            }
                        }
                    }
                }
            }

            int targetEnemies = Random.Range(minEnemies, maxEnemies + 1);
            for (int i = 0; i < targetEnemies; i++)
            {
                bool placed = false;
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    Vector2Int cell = mainComponent[Random.Range(0, mainComponent.Count)];
                    Vector3Int pos = new Vector3Int(cell.x, cell.y, z);
                    if (IsValidEnemyTile(pos))
                    {
                        grid[pos.x, pos.y, pos.z] = CellType.Enemy;
                        enemyTiles.Add(pos);
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    foreach (var cell in mainComponent)
                    {
                        Vector3Int pos = new Vector3Int(cell.x, cell.y, z);
                        if (IsValidEnemyTile(pos))
                        {
                            grid[pos.x, pos.y, pos.z] = CellType.Enemy;
                            enemyTiles.Add(pos);
                            placed = true;
                            break;
                        }
                    }
                }
            }

            int targetHoles = Random.Range(1, 3);
            for (int i = 0; i < targetHoles; i++)
            {
                bool placed = false;
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    Vector2Int cell = mainComponent[Random.Range(0, mainComponent.Count)];
                    Vector3Int pos = new Vector3Int(cell.x, cell.y, z);
                    if (IsValidHoleTile(pos))
                    {
                        grid[pos.x, pos.y, pos.z] = CellType.Hole;
                        placed = true;
                        break;
                    }
                }
            }
        }
    }

    bool IsValidHoleTile(Vector3Int pos)
    {
        if (grid[pos.x, pos.y, pos.z] != CellType.Floor ||
            pos == entrancePos || pos == exitPos ||
            grid[pos.x, pos.y, pos.z] == CellType.RedTelepad ||
            grid[pos.x, pos.y, pos.z] == CellType.BlueTelepad)
            return false;

        int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] dy = { 1, 0, -1, 0, 1, -1, 1, -1 };
        for (int i = 0; i < 8; i++)
        {
            Vector2Int neighbor = new Vector2Int(pos.x + dx[i], pos.y + dy[i]);
            if (IsInBounds(neighbor))
            {
                if (grid[neighbor.x, neighbor.y, pos.z] == CellType.Entrance ||
                    grid[neighbor.x, neighbor.y, pos.z] == CellType.Exit ||
                    grid[neighbor.x, neighbor.y, pos.z] == CellType.RedTelepad ||
                    grid[neighbor.x, neighbor.y, pos.z] == CellType.BlueTelepad)
                    return false;
                int floorNeighbors = 0;
                for (int j = 0; j < 4; j++)
                {
                    Vector2Int adj = new Vector2Int(neighbor.x + dx[j], neighbor.y + dy[j]);
                    if (IsInBounds(adj) && grid[adj.x, adj.y, pos.z] == CellType.Floor)
                        floorNeighbors++;
                }
                if (floorNeighbors == 2)
                    return false;
            }
        }
        return true;
    }
    
    bool IsValidEnemyTile(Vector3Int pos)
    {
        if (grid[pos.x, pos.y, pos.z] != CellType.Floor ||
            enemyTiles.Contains(pos) ||
            spawnableObjectTiles.Contains(pos) ||
            grid[pos.x, pos.y, pos.z] == CellType.Spawn ||
            grid[pos.x, pos.y, pos.z] == CellType.Entrance ||
            grid[pos.x, pos.y, pos.z] == CellType.Exit ||
            grid[pos.x, pos.y, pos.z] == CellType.RedTelepad ||
            grid[pos.x, pos.y, pos.z] == CellType.BlueTelepad)
            return false;

        int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] dy = { 1, 0, -1, 0, 1, -1, 1, -1 };
        for (int i = 0; i < 8; i++)
        {
            Vector2Int neighbor = new Vector2Int(pos.x + dx[i], pos.y + dy[i]);
            if (IsInBounds(neighbor) &&
                (grid[neighbor.x, neighbor.y, pos.z] == CellType.RedTelepad ||
                 grid[neighbor.x, neighbor.y, pos.z] == CellType.BlueTelepad))
                return false;
        }

        bool hasNonEmptyNeighbor = false;
        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighbor = new Vector2Int(pos.x + dx[i], pos.y + dy[i]);
            if (IsInBounds(neighbor) && grid[neighbor.x, neighbor.y, pos.z] != CellType.Empty)
            {
                hasNonEmptyNeighbor = true;
                break;
            }
        }
        if (!hasNonEmptyNeighbor)
            return false;

        Vector3 worldPos = new Vector3(pos.x * cellSize, pos.z * ceilingHeight + 1.1f, pos.y * cellSize);
        if (Physics.OverlapBox(worldPos, new Vector3(0.25f, 0.5f, 0.25f), Quaternion.identity, LayerMask.GetMask("Default")).Length > 0)
            return false;

        return true;
    }

    bool IsValidSpawnableObjectTile(Vector3Int pos)
    {
        if (grid[pos.x, pos.y, pos.z] != CellType.Floor ||
            spawnableObjectTiles.Contains(pos) ||
            grid[pos.x, pos.y, pos.z] == CellType.Spawn ||
            grid[pos.x, pos.y, pos.z] == CellType.Entrance ||
            grid[pos.x, pos.y, pos.z] == CellType.Exit ||
            grid[pos.x, pos.y, pos.z] == CellType.RedTelepad ||
            grid[pos.x, pos.y, pos.z] == CellType.BlueTelepad)
            return false;

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };
        bool hasNonEmptyNeighbor = false;
        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighbor = new Vector2Int(pos.x + dx[i], pos.y + dy[i]);
            if (IsInBounds(neighbor) && grid[neighbor.x, neighbor.y, pos.z] != CellType.Empty)
            {
                hasNonEmptyNeighbor = true;
                break;
            }
        }
        if (!hasNonEmptyNeighbor)
            return false;

        Vector3 worldPos = new Vector3(pos.x * cellSize, pos.z * ceilingHeight + 1.1f, pos.y * cellSize);
        if (Physics.OverlapBox(worldPos, new Vector3(0.25f, 0.5f, 0.25f), Quaternion.identity, LayerMask.GetMask("Default")).Length > 0)
            return false;

        return true;
    }

    void GeneratePath(Vector2Int startPos, int floor)
    {
        roomsPerFloor[floor].Clear();
        floorCells[floor].Clear();

        List<RoomTypeConfig> criticalRoomTypes = new List<RoomTypeConfig>();
        if (floor == 0)
            criticalRoomTypes.Add(roomTypes.Find(rt => rt.containsEntrance));
        if (floor == floorCount - 1)
            criticalRoomTypes.Add(roomTypes.Find(rt => rt.containsExit));
        criticalRoomTypes.Add(roomTypes.Find(rt => rt.containsRedTelepad));
        criticalRoomTypes.Add(roomTypes.Find(rt => rt.containsBlueTelepad));

        foreach (var roomType in criticalRoomTypes)
        {
            if (roomType == null) continue;
            if (roomType.containsEntrance && floor > 0) continue;
            if (roomType.containsExit && floor < floorCount - 1) continue;

            Vector2Int roomDimensions = new Vector2Int(
                Random.Range(roomType.minDimensions.x, roomType.maxDimensions.x + 1),
                Random.Range(roomType.minDimensions.y, roomType.maxDimensions.y + 1)
            );
            Vector2Int roomCenter = startPos;
            bool placed = false;

            for (int attempt = 0; attempt < 50; attempt++)
            {
                roomCenter = new Vector2Int(
                    Random.Range(2, gridWidth - roomDimensions.x - 2),
                    Random.Range(2, gridHeight - roomDimensions.y - 2)
                );
                if (CanPlaceRoom(roomCenter, roomDimensions, floor) && IsRoomCenterValid(roomCenter, floor))
                {
                    PlaceRoom(roomCenter, roomDimensions, floor, roomType);
                    roomsPerFloor[floor].Add((roomCenter, roomDimensions, roomType));
                    floorCells[floor].Add(roomCenter);
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                for (int x = 2; x < gridWidth - roomDimensions.x - 2; x++)
                {
                    for (int y = 2; y < gridHeight - roomDimensions.y - 2; y++)
                    {
                        roomCenter = new Vector2Int(x, y);
                        if (CanPlaceRoom(roomCenter, roomDimensions, floor) && IsRoomCenterValid(roomCenter, floor))
                        {
                            PlaceRoom(roomCenter, roomDimensions, floor, roomType);
                            roomsPerFloor[floor].Add((roomCenter, roomDimensions, roomType));
                            floorCells[floor].Add(roomCenter);
                            placed = true;
                            break;
                        }
                    }
                    if (placed) break;
                }
            }
        }

        int totalRooms = Random.Range(minRooms, maxRooms + 1);
        int placedRooms = roomsPerFloor[floor].Count;
        int remainingRooms = totalRooms - placedRooms;

        var standardRoomType = roomTypes.Find(rt => rt.typeName == "StandardRoom");
        for (int i = 0; i < remainingRooms; i++)
        {
            Vector2Int roomDimensions = new Vector2Int(
                Random.Range(standardRoomType.minDimensions.x, standardRoomType.maxDimensions.x + 1),
                Random.Range(standardRoomType.minDimensions.y, standardRoomType.maxDimensions.y + 1)
            );
            bool placed = false;
            Vector2Int roomCenter = startPos;

            for (int attempt = 0; attempt < 50; attempt++)
            {
                roomCenter = new Vector2Int(
                    Random.Range(2, gridWidth - roomDimensions.x - 2),
                    Random.Range(2, gridHeight - roomDimensions.y - 2)
                );
                if (CanPlaceRoom(roomCenter, roomDimensions, floor) && IsRoomCenterValid(roomCenter, floor))
                {
                    PlaceRoom(roomCenter, roomDimensions, floor, standardRoomType);
                    roomsPerFloor[floor].Add((roomCenter, roomDimensions, standardRoomType));
                    floorCells[floor].Add(roomCenter);
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                for (int x = 2; x < gridWidth - roomDimensions.x - 2; x++)
                {
                    for (int y = 2; y < gridHeight - roomDimensions.y - 2; y++)
                    {
                        roomCenter = new Vector2Int(x, y);
                        if (CanPlaceRoom(roomCenter, roomDimensions, floor) && IsRoomCenterValid(roomCenter, floor))
                        {
                            PlaceRoom(roomCenter, roomDimensions, floor, standardRoomType);
                            roomsPerFloor[floor].Add((roomCenter, roomDimensions, standardRoomType));
                            floorCells[floor].Add(roomCenter);
                            placed = true;
                            break;
                        }
                    }
                    if (placed) break;
                }
            }
        }

        if (roomsPerFloor[floor].Count > 1)
        {
            List<(Vector2Int from, Vector2Int to)> corridors = PrimConnectRooms(floor);
            foreach (var corridor in corridors)
            {
                PlaceCorridor(corridor.from, corridor.to, floor);
            }
        }

        List<List<Vector2Int>> components = FindConnectedComponents(floor);
        if (components.Count > 1)
        {
            List<Vector2Int> mainComponent = components.OrderByDescending(c => c.Count).First();
            foreach (var component in components)
            {
                if (component == mainComponent) continue;
                Vector2Int from = component[0];
                Vector2Int to = mainComponent[0];
                float minDistance = float.MaxValue;
                foreach (var cell in mainComponent)
                {
                    float dist = Vector2Int.Distance(from, cell);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        to = cell;
                    }
                }
                PlaceCorridor(from, to, floor);
            }
        }

        AddDeadEnds(floor);
    }

    private List<RoomTypeConfig> InitializeRoomTypes()
    {
        return new List<RoomTypeConfig>
        {
            new RoomTypeConfig
            {
                typeName = "SpawnRoom",
                minDimensions = new Vector2Int(4, 4),
                maxDimensions = new Vector2Int(4, 4),
                floorMaterial = null,
                minPerFloor = 1,
                maxPerFloor = 1,
                containsEntrance = true,
                containsExit = false,
                containsRedTelepad = false,
                containsBlueTelepad = false,
                containsBoss = false
            },
            new RoomTypeConfig
            {
                typeName = "ExitRoom",
                minDimensions = new Vector2Int(4, 4),
                maxDimensions = new Vector2Int(4, 4),
                floorMaterial = null,
                minPerFloor = 1,
                maxPerFloor = 1,
                containsEntrance = false,
                containsExit = true,
                containsRedTelepad = false,
                containsBlueTelepad = false,
                containsBoss = false
            },
            new RoomTypeConfig
            {
                typeName = "RedTelepadRoom",
                minDimensions = new Vector2Int(3, 3),
                maxDimensions = new Vector2Int(5, 5),
                floorMaterial = null,
                minPerFloor = 1,
                maxPerFloor = 1,
                containsEntrance = false,
                containsExit = false,
                containsRedTelepad = true,
                containsBlueTelepad = false,
                containsBoss = false
            },
            new RoomTypeConfig
            {
                typeName = "BlueTelepadRoom",
                minDimensions = new Vector2Int(3, 3),
                maxDimensions = new Vector2Int(5, 5),
                floorMaterial = null,
                minPerFloor = 1,
                maxPerFloor = 1,
                containsEntrance = false,
                containsExit = false,
                containsRedTelepad = false,
                containsBlueTelepad = true,
                containsBoss = false
            },
            new RoomTypeConfig
            {
                typeName = "StandardRoom",
                minDimensions = new Vector2Int(3, 3),
                maxDimensions = new Vector2Int(7, 7),
                floorMaterial = null,
                minPerFloor = 2,
                maxPerFloor = 6,
                containsEntrance = false,
                containsExit = false,
                containsRedTelepad = false,
                containsBlueTelepad = false,
                containsBoss = false
            }
        };
    }

    void BuildTelepadLinks()
    {
        telepadLinks.Clear();
        for (int z = 0; z < floorCount - 1; z++)
        {
            Vector3Int redPos = redPads.Find(p => p.z == z);
            Vector3Int bluePos = bluePads.Find(p => p.z == z + 1);
            if (redPos == Vector3Int.zero || bluePos == Vector3Int.zero)
            {
                if (redPos == Vector3Int.zero && z < floorCount - 1)
                {
                    var redRoomType = roomTypes.Find(rt => rt.containsRedTelepad);
                    Vector2Int roomCenter = new Vector2Int(5, 5);
                    Vector2Int roomDimensions = redRoomType.minDimensions;
                    if (CanPlaceRoom(roomCenter, roomDimensions, z))
                    {
                        PlaceRoom(roomCenter, roomDimensions, z, redRoomType);
                        roomsPerFloor[z].Add((roomCenter, roomDimensions, redRoomType));
                        redPos = redPads.Find(p => p.z == z);
                    }
                }
                if (bluePos == Vector3Int.zero && z + 1 < floorCount)
                {
                    var blueRoomType = roomTypes.Find(rt => rt.containsBlueTelepad);
                    Vector2Int roomCenter = new Vector2Int(5, 5);
                    Vector2Int roomDimensions = blueRoomType.minDimensions;
                    if (CanPlaceRoom(roomCenter, roomDimensions, z + 1))
                    {
                        PlaceRoom(roomCenter, roomDimensions, z + 1, blueRoomType);
                        roomsPerFloor[z + 1].Add((roomCenter, roomDimensions, blueRoomType));
                        bluePos = bluePads.Find(p => p.z == z + 1);
                    }
                }
            }
            if (redPos != Vector3Int.zero && bluePos != Vector3Int.zero)
            {
                telepadLinks.Add((redPos, bluePos));
            }
        }
    }

    bool ValidateDungeonPath()
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<(Vector3Int pos, int floor)> queue = new Queue<(Vector3Int pos, int floor)>();
        Dictionary<Vector3Int, Vector3Int> telepadDestinations = new Dictionary<Vector3Int, Vector3Int>();

        foreach (var link in telepadLinks)
        {
            telepadDestinations[link.redPad] = link.bluePad;
            telepadDestinations[link.bluePad] = link.redPad;
        }

        queue.Enqueue((entrancePos, 0));
        visited.Add(entrancePos);

        while (queue.Count > 0)
        {
            var (current, floor) = queue.Dequeue();

            if (current == exitPos)
                return true;

            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };
            for (int i = 0; i < 4; i++)
            {
                Vector3Int neighbor = new Vector3Int(current.x + dx[i], current.y + dy[i], floor);
                if (IsInBounds(new Vector2Int(neighbor.x, neighbor.y)) &&
                    !visited.Contains(neighbor) &&
                    grid[neighbor.x, neighbor.y, floor] != CellType.Empty &&
                    grid[neighbor.x, neighbor.y, floor] != CellType.Hole)
                {
                    queue.Enqueue((neighbor, floor));
                    visited.Add(neighbor);
                }
            }

            if (grid[current.x, current.y, floor] == CellType.RedTelepad || grid[current.x, current.y, floor] == CellType.BlueTelepad)
            {
                if (telepadDestinations.ContainsKey(current))
                {
                    Vector3Int dest = telepadDestinations[current];
                    if (!visited.Contains(dest))
                    {
                        queue.Enqueue((dest, dest.z));
                        visited.Add(dest);
                    }
                }
            }
        }

        return false;
    }

    void PlaceRoom(Vector2Int startPos, Vector2Int dimensions, int floor, RoomTypeConfig roomType)
    {
        for (int x = startPos.x; x < startPos.x + dimensions.x; x++)
        {
            for (int y = startPos.y; y < startPos.y + dimensions.y; y++)
            {
                if (IsInBounds(new Vector2Int(x, y)))
                {
                    grid[x, y, floor] = CellType.Floor;
                    floorCells[floor].Add(new Vector2Int(x, y));
                }
            }
        }

        if (spawnSpecialTiles && dimensions.x >= 3 && dimensions.y >= 3)
        {
            Vector2Int center = startPos + dimensions / 2;
            Vector3Int pos = new Vector3Int(center.x, center.y, floor);
            if (IsInBounds(center) && grid[pos.x, pos.y, floor] == CellType.Floor)
            {
                grid[pos.x, pos.y, floor] = CellType.Special;
            }
        }

        if (roomType.containsEntrance && floor == 0)
        {
            entrancePos = new Vector3Int(startPos.x, startPos.y, 0);
            grid[entrancePos.x, entrancePos.y, 0] = CellType.Entrance;
            spawnPos = entrancePos;
        }

        if (roomType.containsExit && floor == floorCount - 1)
        {
            exitPos = new Vector3Int(startPos.x, startPos.y, floor);
            grid[exitPos.x, exitPos.y, floor] = CellType.Exit;
        }

        if (roomType.containsRedTelepad || roomType.containsBlueTelepad)
        {
            List<Vector2Int> validTiles = new List<Vector2Int>();
            for (int x = startPos.x; x < startPos.x + dimensions.x; x++)
            {
                for (int y = startPos.y; y < startPos.y + dimensions.y; y++)
                {
                    Vector2Int tile = new Vector2Int(x, y);
                    if (IsInBounds(tile) && grid[x, y, floor] == CellType.Floor)
                    {
                        validTiles.Add(tile);
                    }
                }
            }

            for (int i = 0; i < validTiles.Count; i++)
            {
                Vector2Int temp = validTiles[i];
                int randomIndex = Random.Range(i, validTiles.Count);
                validTiles[i] = validTiles[randomIndex];
                validTiles[randomIndex] = temp;
            }

            int tileIndex = 0;
            if (roomType.containsRedTelepad && spawnRedTelepads && tileIndex < validTiles.Count && floor != floorCount - 1)
            {
                Vector3Int pos = new Vector3Int(validTiles[tileIndex].x, validTiles[tileIndex].y, floor);
                grid[pos.x, pos.y, floor] = CellType.RedTelepad;
                redPads.Add(pos);
                tileIndex++;
            }

            if (roomType.containsBlueTelepad && spawnBlueTelepads && tileIndex < validTiles.Count && floor != 0)
            {
                Vector3Int pos = new Vector3Int(validTiles[tileIndex].x, validTiles[tileIndex].y, floor);
                grid[pos.x, pos.y, floor] = CellType.BlueTelepad;
                bluePads.Add(pos);
                tileIndex++;
            }
        }
    }

    void PlaceCubes()
    {
        foreach (Transform child in transform)
        {
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

        floorObjects.Clear();

        for (int z = 0; z < floorCount; z++)
        {
            GameObject floorParent = new GameObject($"Floor_{z}_Geometry");
            floorParent.transform.parent = transform;
            floorParent.layer = LayerMask.NameToLayer("ground");
            floorObjects.Add(floorParent);

            float floorY = z * ceilingHeight;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    CellType cell = grid[x, y, z];
                    Vector3 floorPos = new Vector3(x * cellSize, floorY + 0.05f, y * cellSize);
                    GameObject floorObj = null;
                    string objName = cell == CellType.Spawn ? $"Spawn_{x}_{y}_F{z}" :
                        cell == CellType.RedTelepad ? $"RedTelepad_{x}_{y}_F{z}" :
                        cell == CellType.BlueTelepad ? $"BlueTelepad_{x}_{y}_F{z}" :
                        cell == CellType.SpawnableObject ? $"SpawnableObject_{x}_{y}_F{z}" :
                        cell == CellType.Entrance ? $"Entrance_{x}_{y}_F{z}" :
                        cell == CellType.Exit ? $"Exit_{x}_{y}_F{z}" :
                        cell == CellType.Enemy ? $"Enemy_{x}_{y}_F{z}" :
                        cell == CellType.Hole ? $"Hole_{x}_{y}_F{z}" :
                        cell == CellType.Floor ? $"Floor_{x}_{y}_F{z}" :
                        $"Empty_{x}_{y}_F{z}";

                    if (cell == CellType.Floor)
                    {
                        if (floorPrefab == null)
                        {
                            grid[x, y, z] = CellType.Empty;
                            continue;
                        }
                        floorObj = Instantiate(floorPrefab, floorPos, Quaternion.identity, floorParent.transform);
                        floorTilesCount++;
                        floorObj.name = floorTilesCount.ToString();
                        floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                    }
                    else if (cell == CellType.Entrance || cell == CellType.Spawn)
                    {
                        if (entrancePrefab == null)
                        {
                            grid[x, y, z] = CellType.Empty;
                            continue;
                        }
                        floorObj = Instantiate(entrancePrefab, floorPos, Quaternion.identity, floorParent.transform);
                        floorObj.name = objName;
                        floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                    }
                    else if (cell == CellType.Exit)
                    {
                        if (exitPrefab == null)
                        {
                            grid[x, y, z] = CellType.Empty;
                            continue;
                        }
                        floorObj = Instantiate(exitPrefab, floorPos, Quaternion.identity, floorParent.transform);
                        floorObj.name = objName;
                        floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                    }
                    else if (cell == CellType.RedTelepad)
                    {
                        if (redTelepadPrefab == null)
                        {
                            grid[x, y, z] = CellType.Empty;
                            continue;
                        }
                        floorObj = Instantiate(redTelepadPrefab, floorPos, Quaternion.identity, floorParent.transform);
                        floorObj.name = objName;
                        floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                        if (floorObj.GetComponent<Telepad>() == null)
                        {
                            Telepad addedTelepad = floorObj.AddComponent<Telepad>();
                            addedTelepad.telepadType = TelepadType.RedTelepad;

                        }
                        NavMeshModifier modifier = floorObj.GetComponent<NavMeshModifier>();
                        if (modifier == null)
                        {
                            modifier = floorObj.AddComponent<NavMeshModifier>();
                        }
                        modifier.overrideArea = true;
                        modifier.area = 1;
                    }
                    else if (cell == CellType.BlueTelepad)
                    {
                        if (blueTelepadPrefab == null)
                        {
                            grid[x, y, z] = CellType.Empty;
                            continue;
                        }
                        floorObj = Instantiate(blueTelepadPrefab, floorPos, Quaternion.identity, floorParent.transform);
                        floorObj.name = objName;
                        floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                        if (floorObj.GetComponent<Telepad>() == null)
                        {
                            Telepad addedTelepad = floorObj.AddComponent<Telepad>();
                            addedTelepad.telepadType = TelepadType.BlueTelepad;
                        }
                        NavMeshModifier modifier = floorObj.GetComponent<NavMeshModifier>();
                        if (modifier == null)
                        {
                            modifier = floorObj.AddComponent<NavMeshModifier>();
                        }
                        modifier.overrideArea = true;
                        modifier.area = 1;
                    }
                    else if (cell == CellType.SpawnableObject)
                    {
                        if (spawnableObjectPrefab == null)
                        {
                            grid[x, y, z] = CellType.Empty;
                            continue;
                        }
                        floorObj = Instantiate(spawnableObjectPrefab, floorPos, Quaternion.identity, floorParent.transform);
                        floorObj.name = objName;
                        floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                        floorObj.layer = LayerMask.NameToLayer("ground");
                        if (floorObj.GetComponent<SpawnableObject>() == null)
                        {
                            SpawnableObject spawnComp = floorObj.AddComponent<SpawnableObject>();
                            spawnComp.type = SpawnableType.Default;
                        }
                        if (lootPrefab == null)
                        {
                            continue;
                        }
                        GameObject lootObj = Instantiate(lootPrefab, new Vector3(floorPos.x, floorPos.y + 0.55f, floorPos.z), Quaternion.identity, transform);
                        lootObj.name = $"Loot_{x}_{y}_F{z}";
                    }
                    else if (cell == CellType.Enemy)
                    {
                        if (floorPrefab == null)
                        {
                            grid[x, y, z] = CellType.Empty;
                            continue;
                        }
                        floorObj = Instantiate(floorPrefab, floorPos, Quaternion.identity, floorParent.transform);
                        floorObj.name = $"Floor_{x}_{y}_F{z}";
                        floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                    }
                    else if (cell == CellType.Hole)
                    {
                        continue;
                    }
                    else if (cell == CellType.Special)
                    {
                        if (specialTilePrefab == null)
                        {
                            grid[x, y, z] = CellType.Floor;
                            continue;
                        }
                        floorObj = Instantiate(specialTilePrefab, floorPos, Quaternion.identity, floorParent.transform);
                        floorObj.name = $"Special_{x}_{y}_F{z}";
                        floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                    }
                    else if (cell == CellType.Empty)
                    {
                        continue;
                    }
                    else
                    {
                        continue;
                    }

                    if (cell == CellType.RedTelepad || cell == CellType.BlueTelepad)
                    {
                        BoxCollider[] colliders = floorObj.GetComponents<BoxCollider>();
                        BoxCollider triggerCollider = null;
                        BoxCollider solidCollider = null;
                        foreach (var collider in colliders)
                        {
                            if (collider.isTrigger)
                                triggerCollider = collider;
                            else
                                solidCollider = collider;
                        }

                        if (triggerCollider == null)
                        {
                            triggerCollider = floorObj.AddComponent<BoxCollider>();
                            triggerCollider.isTrigger = true;
                        }

                        triggerCollider.size = new Vector3(1f, 10f, 1f);
                        triggerCollider.center = new Vector3(0f, 5f, 0f);
                        if (solidCollider == null)
                        {
                            solidCollider = floorObj.AddComponent<BoxCollider>();
                            solidCollider.isTrigger = false;
                        }

                        solidCollider.size = new Vector3(cellSize, 0.1f, cellSize);
                        solidCollider.center = Vector3.zero;

                        Telepad telepad = floorObj.GetComponent<Telepad>();
                        var link = telepadLinks.Find(link => link.redPad == new Vector3Int(x, y, z) || link.bluePad == new Vector3Int(x, y, z));
                        if (cell == CellType.RedTelepad && z < floorCount - 1)
                        {
                            if (!link.Equals(default) && link.bluePad != Vector3Int.zero)
                            {
                                telepad.destination = new Vector3(link.bluePad.x * cellSize, link.bluePad.z * ceilingHeight + 0.2f, link.bluePad.y * cellSize);
                                triggerCollider.enabled = true;
                            }
                            else
                            {
                                triggerCollider.enabled = false;
                                telepad.destination = Vector3.zero;
                            }
                        }
                        else if (cell == CellType.BlueTelepad && z > 0)
                        {
                            if (!link.Equals(default) && link.redPad != Vector3Int.zero)
                            {
                                telepad.destination = new Vector3(link.redPad.x * cellSize, link.redPad.z * ceilingHeight + 0.2f, link.redPad.y * cellSize);
                                triggerCollider.enabled = true;
                            }
                            else
                            {
                                triggerCollider.enabled = false;
                                telepad.destination = Vector3.zero;
                            }
                        }
                        else
                        {
                            triggerCollider.enabled = false;
                            telepad.destination = Vector3.zero;
                        }
                    }

                    if (cell != CellType.Hole && cell != CellType.Empty)
                    {
                        Vector3 ceilingPos = new Vector3(x * cellSize, floorY + ceilingHeight - 0.1f, y * cellSize);
                        if (ceilingPrefab == null)
                        {
                            continue;
                        }
                        GameObject ceilingObj = Instantiate(ceilingPrefab, ceilingPos, Quaternion.identity, transform);
                        ceilingObj.name = $"Ceiling_{x}_{y}_F{z}";
                        ceilingObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
                        ceilingObj.layer = LayerMask.NameToLayer("solid");

                        int[] dx = { 0, 1, 0, -1 };
                        int[] dy = { 1, 0, -1, 0 };
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2Int neighbor = new Vector2Int(x + dx[i], y + dy[i]);
                            if (!IsInBounds(neighbor) || grid[neighbor.x, neighbor.y, z] == CellType.Empty || grid[neighbor.x, neighbor.y, z] == CellType.Hole)
                            {
                                Vector3 wallPos = new Vector3(x * cellSize, floorY + wallHeight / 2 + 0.05f, y * cellSize);
                                GameObject wallPrefabToUse = null;
                                Quaternion wallRotation = Quaternion.identity;

                                if (i == 0)
                                {
                                    wallPos.z += cellSize / 2;
                                    wallPrefabToUse = wallNorthPrefab;
                                }
                                else if (i == 1)
                                {
                                    wallPos.x += cellSize / 2;
                                    wallPrefabToUse = wallEastPrefab;
                                }
                                else if (i == 2)
                                {
                                    wallPos.z -= cellSize / 2;
                                    wallPrefabToUse = wallSouthPrefab;
                                }
                                else if (i == 3)
                                {
                                    wallPos.x -= cellSize / 2;
                                    wallPrefabToUse = wallWestPrefab;
                                }

                                if (wallPrefabToUse == null)
                                {
                                    continue;
                                }
                                GameObject wallObj = Instantiate(wallPrefabToUse, wallPos, wallRotation, transform);
                                wallObj.name = $"Wall_{(i == 0 ? "North" : i == 1 ? "East" : i == 2 ? "South" : "West")}_{x}_{y}_F{z}";
                                Vector3 wallScale = new Vector3(
                                    i % 2 == 0 ? cellSize : 0.1f,
                                    wallHeight,
                                    i % 2 == 0 ? 0.1f : cellSize
                                );
                                wallObj.transform.localScale = wallScale;
                                wallObj.layer = LayerMask.NameToLayer("solid");
                            }
                        }
                    }
                }
            }
        }
    }

    bool IsInRoom(Vector2Int pos, Vector2Int center, Vector2Int dimensions)
    {
        return pos.x >= center.x && pos.x < center.x + dimensions.x &&
               pos.y >= center.y && pos.y < center.y + dimensions.y;
    }

    
    List<(Vector2Int from, Vector2Int to)> PrimConnectRooms(int floor)
    {
        List<(Vector2Int from, Vector2Int to)> corridors = new List<(Vector2Int from, Vector2Int to)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<(Vector2Int from, Vector2Int to, float weight)> edges = new List<(Vector2Int from, Vector2Int to, float weight)>();

        Vector2Int startRoom = roomsPerFloor[floor][0].center;
        visited.Add(startRoom);

        for (int i = 1; i < roomsPerFloor[floor].Count; i++)
        {
            Vector2Int otherRoom = roomsPerFloor[floor][i].center;
            float weight = Vector2Int.Distance(startRoom, otherRoom);
            edges.Add((startRoom, otherRoom, weight));
        }

        while (visited.Count < roomsPerFloor[floor].Count && edges.Count > 0)
        {
            int minEdgeIndex = 0;
            float minWeight = float.MaxValue;
            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].weight < minWeight)
                {
                    minWeight = edges[i].weight;
                    minEdgeIndex = i;
                }
            }

            var edge = edges[minEdgeIndex];
            edges.RemoveAt(minEdgeIndex);

            if (!visited.Contains(edge.to))
            {
                corridors.Add((edge.from, edge.to));
                visited.Add(edge.to);
                for (int i = 0; i < roomsPerFloor[floor].Count; i++)
                {
                    Vector2Int otherRoom = roomsPerFloor[floor][i].center;
                    if (!visited.Contains(otherRoom))
                    {
                        float weight = Vector2Int.Distance(edge.to, otherRoom);
                        edges.Add((edge.to, otherRoom, weight));
                    }
                }
            }
        }

        foreach (var room in roomsPerFloor[floor])
        {
            if (!visited.Contains(room.center))
            {
                Vector2Int closest = visited.First();
                float minDist = float.MaxValue;
                foreach (var visitedRoom in visited)
                {
                    float dist = Vector2Int.Distance(room.center, visitedRoom);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = visitedRoom;
                    }
                }
                corridors.Add((closest, room.center));
                visited.Add(room.center);
            }
        }

        return corridors;
    }

    List<List<Vector2Int>> FindConnectedComponents(int floor)
    {
        List<List<Vector2Int>> components = new List<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (var cell in floorCells[floor])
        {
            if (!visited.Contains(cell))
            {
                List<Vector2Int> component = new List<Vector2Int>();
                FloodFill(cell, floor, visited, component);
                if (component.Count > 0)
                    components.Add(component);
            }
        }

        return components;
    }

    void FloodFill(Vector2Int start, int floor, HashSet<Vector2Int> visited, List<Vector2Int> component)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            component.Add(current);

            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };
            for (int i = 0; i < 4; i++)
            {
                Vector2Int neighbor = new Vector2Int(current.x + dx[i], current.y + dy[i]);
                if (IsInBounds(neighbor) && !visited.Contains(neighbor) && grid[neighbor.x, neighbor.y, floor] != CellType.Empty)
                {
                    visited.Add(neighbor);
                    stack.Push(neighbor);
                }
            }
        }
    }

    void PlaceCorridor(Vector2Int start, Vector2Int end, int floor)
    {
        Vector2Int current = start;
        while (current != end)
        {
            if (current.x != end.x)
            {
                current.x += current.x < end.x ? 1 : -1;
            }
            else if (current.y != end.y)
            {
                current.y += current.y < end.y ? 1 : -1;
            }
            if (IsInBounds(current) && grid[current.x, current.y, floor] == CellType.Empty)
            {
                grid[current.x, current.y, floor] = CellType.Floor;
                floorCells[floor].Add(current);
            }
        }
    }

    void AddDeadEnds(int floor)
    {
        int deadEndCount = Random.Range(3, 8);
        for (int i = 0; i < deadEndCount; i++)
        {
            if (floorCells[floor].Count == 0) continue;
            Vector2Int floorCell = floorCells[floor][Random.Range(0, floorCells[floor].Count)];
            List<Vector2Int> neighbors = GetValidNeighbors(floorCell, floor);
            if (neighbors.Count > 0)
            {
                Vector2Int deadEnd = neighbors[Random.Range(0, neighbors.Count)];
                grid[deadEnd.x, deadEnd.y, floor] = CellType.Floor;
                floorCells[floor].Add(deadEnd);
            }
        }
    }

    List<Vector2Int> GetValidNeighbors(Vector2Int pos, int floor)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        for (int i = 0; i < 4; i++)
        {
            Vector2Int neighbor = new Vector2Int(pos.x + dx[i], pos.y + dy[i]);
            if (IsInBounds(neighbor) && grid[neighbor.x, neighbor.y, floor] == CellType.Empty)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    bool CanPlaceRoom(Vector2Int startPos, Vector2Int dimensions, int floor)
    {
        for (int x = startPos.x; x < startPos.x + dimensions.x; x++)
        {
            for (int y = startPos.y; y < startPos.y + dimensions.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!IsInBounds(pos) || grid[pos.x, pos.y, floor] != CellType.Empty)
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool IsRoomCenterValid(Vector2Int center, int floor)
    {
        foreach (var room in roomsPerFloor[floor])
        {
            if (Vector2Int.Distance(center, room.center) < minRoomDistance + Mathf.Max(room.dimensions.x, room.dimensions.y))
                return false;
        }
        return true;
    }

    bool IsValidCell(Vector3Int pos)
    {
        return IsInBounds(new Vector2Int(pos.x, pos.y)) && grid[pos.x, pos.y, pos.z] == CellType.Empty;
    }

    IEnumerator BuildNavMeshAsync()
    {
        yield return new WaitForEndOfFrame();
        foreach (var floorObj in floorObjects)
        {
            NavMeshSurface surface = floorObj.GetComponent<NavMeshSurface>();
            if (surface == null)
                surface = floorObj.AddComponent<NavMeshSurface>();
            surface.layerMask = LayerMask.GetMask("ground");
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.agentTypeID = NavMesh.GetSettingsByIndex(0).agentTypeID;
            surface.defaultArea = 0;
            surface.overrideVoxelSize = true;
            surface.voxelSize = 0.03f;
            surface.BuildNavMesh();
        }

        for (int z = 0; z < floorCount; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y, z] == CellType.Hole)
                    {
                        Vector3 holePos = new Vector3(x * cellSize, z * ceilingHeight + 0.05f, y * cellSize);
                        GameObject holeVolume = new GameObject($"HoleVolume_{x}_{y}_F{z}");
                        holeVolume.transform.position = holePos;
                        holeVolume.transform.parent = floorObjects[z].transform;
                        NavMeshModifierVolume modifier = holeVolume.AddComponent<NavMeshModifierVolume>();
                        modifier.size = new Vector3(cellSize * 1.2f, 1f, cellSize * 1.2f);
                        modifier.center = Vector3.zero;
                        modifier.area = 1;
                    }
                }
            }
        }

        IsNavMeshReady = true;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonGenerator generator = (DungeonGenerator)target;
        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (GUILayout.Button("Regenerate Dungeon"))
        {
            generator.GenerateDungeon();
            EditorUtility.SetDirty(generator);
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(generator);
        }
    }
}
#endif

public enum SpawnableType
{
    Default
}

public class SpawnableObject : MonoBehaviour
{
    public SpawnableType type;
}



public enum CellType { Empty, Floor, Entrance, Exit, RedTelepad, BlueTelepad, Spawn, SpawnableObject, Enemy, Hole, Special }