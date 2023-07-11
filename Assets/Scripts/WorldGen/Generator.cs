using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Rendering;

public class Generator : MonoBehaviour
{
  public GameObject wall;
  [SerializeField] public GameObject wallFiller;
  [SerializeField] public GameObject door;

  [SerializeField] public Sprite[] caps;
  [SerializeField] public Sprite tileSprite;
  [SerializeField] public Sprite wallSprite;
  [SerializeField] public Material material;

  public GameObject level;
  public Sprite map;

  public Tile[][] matrix;

  public int width;
  public int depth;

  private RuleSet ruleSet;
  private GameObject floor;
  private GameObject walls;

  public void GenerateLevel()
  {
    if (matrix == null) return;
    if (caps.Length < 14) throw new ArgumentException("Missing caps.");

    CleanUp();

    floor = new GameObject("Floor");
    floor.transform.SetParent(level.transform);
    floor.transform.position = new Vector3(floor.transform.position.x, floor.transform.position.y, floor.transform.position.z + 0.5f);

    walls = new GameObject("Walls");
    walls.transform.SetParent(level.transform);

    //Assign each sprite to its ruleset. Ruleset it's hardcoded so the sprite order is very important.
    ruleSet = new RuleSet();
    for (int i = 0; i < caps.Length; ruleSet.caps[i].sprite = caps[i], i++) ;

    for (int z = 0; z < matrix.Length; z++)
    {
      var row = matrix[z];
      for (int x = 0; x < row.Length; x++)
      {
        if (row[x] == 0) continue;

        switch (row[x])
        {
          case Tile.Floor: PlaceTile(x, z); break;
          case Tile.Door: PlaceDoor(x, z); break;
          case Tile.Wall: PlaceWall(x, z); break;
        }
      }
    }

    UpdateNavmesh();
  }

  public void CleanUp()
  {
    if (walls != null) DestroyImmediate(walls.gameObject);
    if (floor != null) DestroyImmediate(floor.gameObject);
  }

  public void UpdateNavmesh()
  {
    var nms = floor.AddComponent<NavMeshSurface>();
    nms.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
    nms.layerMask = 1 << LayerMask.NameToLayer("Ground");
    nms.BuildNavMesh();
  }

  public void PlaceTile(int x, int z)
  {
    var tileRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));

    var tile = new GameObject($"Tile_z{z}_x{x}");
    tile.layer = LayerMask.NameToLayer("Ground");
    tile.transform.rotation = tileRotation;

    var sr = tile.AddComponent<SpriteRenderer>();
    sr.sprite = tileSprite;
    sr.material = material;
    sr.sortingOrder = -1;
    sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

    var bx = tile.AddComponent<BoxCollider>();
    bx.size = new Vector3(1f, 1f, 0.1f);
    bx.center = new Vector3(0f, 0f, bx.size.z / 2);

    tile.transform.SetParent(floor.transform);
    tile.transform.localPosition = new Vector3(x, 0, z);
  }

  public void PlaceDoor(int x, int z)
  {
    PlaceTile(x, z);
    var goDoor = Instantiate(door, floor.transform);
    goDoor.name = "Door";
    goDoor.transform.localPosition = new Vector3(x, 0, z);
  }

  //Todo merge walls
  //Todo dont create walls under caps
  public void PlaceWall(int x, int z)
  {
    var name = $"Wall_z{z}_x{x}";

    //If below there should be a wall, do not place a wall here.
    //This is to prevent generating a wall that will not be visible
    //because of the caps.
    if (z > 0 && matrix[z - 1][x] != Tile.Wall)
    {
      var goWall = Instantiate(this.wall, walls.transform);
      goWall.transform.localPosition = new Vector3(x, 0, z);
      goWall.name = name;

      //Instantiate a filler which will block light passing through
      var filler = Instantiate(wallFiller, goWall.transform);
      filler.name = "Filler";
    }

    //Place a vertical wall which is not visible and will only cast shadows.
    //collision detection and light will still work
    else
    {
      var filler = GameObject.CreatePrimitive(PrimitiveType.Cube);
      filler.name = $"h{name}";
      filler.transform.SetParent(walls.transform);
      filler.transform.localPosition = new Vector3(x, 0, z + 0.5f);
      filler.transform.localScale = new Vector3(1, 2, 1);
      filler.layer = LayerMask.NameToLayer("Wall");
      filler.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    PlaceCap(x, z);
  }

  public void PlaceCap(int x, int z)
  {
    Tile nw, n, ne, e, w, se, s, sw;

    bool atRowStart = x == 0;
    bool atRowEnd = x == width - 1;

    bool atColumnStart = z == 0;
    bool atColumnEnd = z == depth - 1;

    n = atColumnEnd ? Tile.Wall : matrix[z + 1][x];
    w = atRowStart ? Tile.Wall : matrix[z][x - 1];
    e = atRowEnd ? Tile.Wall : matrix[z][x + 1];
    s = atColumnStart ? Tile.Wall : matrix[z - 1][x]; 
    nw = atColumnEnd || atRowStart ? Tile.Wall : matrix[z + 1][x - 1];
    sw = atColumnStart || atRowStart ? Tile.Wall : matrix[z - 1][x - 1];
    ne = atRowEnd || atColumnEnd ? Tile.Wall : matrix[z + 1][x + 1];
    se = atColumnStart || atRowEnd ? Tile.Wall : matrix[z - 1][x + 1];

    //Define the directions of the current pixel 
    Cap capToSpawn = null;
    //var nw = z == matrix.Length - 1 || x == 0 ? 1 : matrix[z + 1][x - 1];
    //var n = z == matrix.Length - 1 ? 1 : matrix[z + 1][x];
    //var ne = x == width - 1 || z == matrix.Length - 1 ? 1 : matrix[z + 1][x + 1];
    //var e = x == width - 1 ? 1 : matrix[z][x + 1];
    //var w = x == 0 ? 1 : matrix[z][x - 1];
    //var se = z == 0 || x == width - 1 ? 1 : matrix[z - 1][x + 1];
    //var s = z == 0 ? 1 : matrix[z - 1][x];
    //var sw = x == 0 || z == 0 ? 1 : matrix[z - 1][x - 1];

    var neighbors = new Tile[] { nw, n, ne, e, w, se, s, sw }.Count(tile => tile == Tile.Wall);

    //Try to find the correct cap with the correct rotation.
    //When found, exit the loop.
    var possibleCaps = ruleSet.caps.Where(c => c.neighbors <= neighbors).OrderByDescending(x => x.neighbors).ToList();
    foreach (var possibleCap in possibleCaps)
    {
      foreach (var rotation in new float[] { 0f, 90f, 180f, 270f })
      {
        //Shift the directions array depending on the current rotation.
        var shiftAmount = (int)rotation / 45;
        var directions = new Queue<Directions>(Enum.GetValues(typeof(Directions)).Cast<Directions>());
        for (int i = 0; i < shiftAmount; i++)
        {
          directions.Dequeue();
          directions.Enqueue((Directions)i);
        }

        //Compare matrix direction with rotated cap directions.
        var matches = 0;

        var rotatedDirs = directions.ToArray();
        //foreach (var dir in Enum.GetValues(typeof(Directions)))
        //{
        //  var currRotatedDir = rotatedDirs[(int)dir];
        //  if (!cap.rules.ContainsKey(currRotatedDir)) continue;
        //
        //  if (cap.rules[currRotatedDir]) ...
        //}
        if (nw == Tile.Wall && possibleCap.rules.ContainsKey(rotatedDirs[0]) && possibleCap.rules[rotatedDirs[0]] == true) matches++;
        if (n == Tile.Wall && possibleCap.rules.ContainsKey(rotatedDirs[1]) && possibleCap.rules[rotatedDirs[1]] == true) matches++;
        if (ne == Tile.Wall && possibleCap.rules.ContainsKey(rotatedDirs[2]) && possibleCap.rules[rotatedDirs[2]] == true) matches++;
        if (e == Tile.Wall && possibleCap.rules.ContainsKey(rotatedDirs[3]) && possibleCap.rules[rotatedDirs[3]] == true) matches++;
        if (se == Tile.Wall && possibleCap.rules.ContainsKey(rotatedDirs[4]) && possibleCap.rules[rotatedDirs[4]] == true) matches++;
        if (s == Tile.Wall && possibleCap.rules.ContainsKey(rotatedDirs[5]) && possibleCap.rules[rotatedDirs[5]] == true) matches++;
        if (sw == Tile.Wall && possibleCap.rules.ContainsKey(rotatedDirs[6]) && possibleCap.rules[rotatedDirs[6]] == true) matches++;
        if (w == Tile.Wall && possibleCap.rules.ContainsKey(rotatedDirs[7]) && possibleCap.rules[rotatedDirs[7]] == true) matches++;

        //If all directions match, it's the right cap.
        if (matches == possibleCap.neighbors)
        {
          capToSpawn = possibleCap;
          capToSpawn.rotation = rotation * -1;
          break;
        }
      }

      if (capToSpawn != null) break;
    }

    //var capOffset = new Vector3(x - 0.5f, 0.5f, z + 0.3f);
    var capRotation = Quaternion.Euler(new Vector3(90f, capToSpawn.rotation, 0f));

    var cap = new GameObject("Cap");
    cap.transform.SetPositionAndRotation(new Vector3(x, 0.5f, z + 1.35f), capRotation);

    var sr = cap.AddComponent<SpriteRenderer>();
    sr.sprite = capToSpawn.sprite;
    sr.material = material;
    sr.shadowCastingMode = ShadowCastingMode.On;

    cap.transform.SetParent(walls.transform);
  }

  class RuleSet
  {
    public List<Cap> caps;

    public RuleSet()
    {
      caps = new List<Cap>
      {
        new Cap(
          (Directions.N, false),
          (Directions.E, false),
          (Directions.S, false),
          (Directions.W, false)),
        new Cap (
          (Directions.N, false ),
          (Directions.E, false ),
          (Directions.S, false ),
          (Directions.W, true)),
        new Cap (
          (Directions.N, false ),
          (Directions.E, true ),
          (Directions.S, false ),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, false ), (Directions.N, true ),
          (Directions.E, false ),
          (Directions.S, false ),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, true ), (Directions.N, true ),
          (Directions.E, false ),
          (Directions.S, false ),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, false ), (Directions.N, true ), (Directions.NE, false ),
          (Directions.E, true ),
          (Directions.S, false ),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, false ), (Directions.N, true ), (Directions.NE, true ),
          (Directions.E, true ),
          (Directions.S, false ),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, true ), (Directions.N, true ), (Directions.NE, false ),
          (Directions.E, true ),
          (Directions.S, false ),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, true ), (Directions.N, true ), (Directions.NE, true ),
          (Directions.E, true ),
          (Directions.S, false ),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, false ), (Directions.N, true ), (Directions.NE, false ),
          (Directions.E, true ),
          (Directions.SE, false), (Directions.S, true ), (Directions.SW, false),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, false ), (Directions.N, true ), (Directions.NE, false ),
          (Directions.E, true ),
          (Directions.SE, false), (Directions.S, true ), (Directions.SW, true),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, false ), (Directions.N, true ), (Directions.NE, true ),
          (Directions.E, true ),
          (Directions.SE, false), (Directions.S, true ), (Directions.SW, true),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, false ), (Directions.N, true ), (Directions.NE, false ),
          (Directions.E, true ),
          (Directions.SE, true), (Directions.S, true ), (Directions.SW, true),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, false ), (Directions.N, true ), (Directions.NE, true ),
          (Directions.E, true ),
          (Directions.SE, true), (Directions.S, true ), (Directions.SW, true),
          (Directions.W, true)),
        new Cap (
          (Directions.NW, true ), (Directions.N, true ), (Directions.NE, true ),
          (Directions.E, true ),
          (Directions.SE, true), (Directions.S, true ), (Directions.SW, true),
          (Directions.W, true)),
      };
    }
  }

  class Cap
  {
    public Dictionary<Directions, bool> rules = new Dictionary<Directions, bool>();
    public Sprite sprite;
    public float rotation;

    public int neighbors => rules.Count(r => r.Value == true);

    public Cap(params (Directions, bool)[] rules)
    {
      foreach (var rule in rules) this.rules.Add(rule.Item1, rule.Item2);
    }
  }

  enum Directions
  {
    NW = 0,
    N = 1,
    NE = 2,
    E = 3,
    SE = 4,
    S = 5,
    SW = 6,
    W = 7
  }
}
