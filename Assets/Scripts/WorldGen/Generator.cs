using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
  public GameObject wall;
  public GameObject cap;
  [SerializeField]
  public Sprite[] caps;
  public GameObject floor;
  public Sprite map;
  public int[][] matrix;

  public void Generate()
  {
    if (map == null) Debug.LogWarning("Cannot generate room, map is missing.");
    if (caps.Length < 14) throw new ArgumentException("Missing caps.");

    foreach (Transform child in floor.transform.Cast<Transform>().ToList())
    {
      DestroyImmediate(child.gameObject);
    }

    if (matrix == null) return;

    //return;
    for (int y = 0; y < matrix.Length; y++)
    {
      var row = matrix[y];
      for (int x = 0; x < row.Length; x++)
      {
        //false means no cap or wall to place.
        if (row[x] == 0) continue;

        //Todo merge walls
        var wallsOffset = new Vector3(x - 1, 0, y - 1);
        var goWall = Instantiate(wall, transform.position + wallsOffset, default);
        goWall.transform.parent = floor.transform;

        //Assign each sprite to its ruleset. Ruleset it's hardcoded so the sprite order is very important.
        var ruleSet = new RuleSet();
        for (int i = 0; i < caps.Length; ruleSet.caps[i].sprite = caps[i], i++) ;

        //Define the directions of the current pixel 
        Cap capToSpawn = null;
        var nw = y == matrix.Length - 1 || x == 0 ? 1 : matrix[y + 1][x - 1];
        var n = y == matrix.Length - 1 ? 1 : matrix[y + 1][x];
        var ne = x == row.Length - 1 || y == matrix.Length - 1 ? 1 : matrix[y + 1][x + 1];
        var e = x == row.Length - 1 ? 1 : matrix[y][x + 1];
        var w = x == 0 ? 1 : matrix[y][x - 1];
        var se = y == 0 || x == row.Length - 1 ? 1 : matrix[y - 1][x + 1];
        var s = y == 0 ? 1 : matrix[y - 1][x];
        var sw = x == 0 || y == 0 ? 1 : matrix[y - 1][x - 1];

        var neighbors = nw + n + ne + e + w + se + s + sw;

        //Try to find the correct cap with the correct rotation.
        //When found, exit the loop.
        var possibleCaps = ruleSet.caps.Where(c => c.neighbors <= neighbors).OrderByDescending(x => x.neighbors).ToList();
        foreach (var cap in possibleCaps)
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
            if (nw == 1 && cap.rules.ContainsKey(rotatedDirs[0]) && cap.rules[rotatedDirs[0]] == true) matches++;
            if (n == 1 && cap.rules.ContainsKey(rotatedDirs[1]) && cap.rules[rotatedDirs[1]] == true) matches++;
            if (ne == 1 && cap.rules.ContainsKey(rotatedDirs[2]) && cap.rules[rotatedDirs[2]] == true) matches++;
            if (e == 1 && cap.rules.ContainsKey(rotatedDirs[3]) && cap.rules[rotatedDirs[3]] == true) matches++;
            if (se == 1 && cap.rules.ContainsKey(rotatedDirs[4]) && cap.rules[rotatedDirs[4]] == true) matches++;
            if (s == 1 && cap.rules.ContainsKey(rotatedDirs[5]) && cap.rules[rotatedDirs[5]] == true) matches++;
            if (sw == 1 && cap.rules.ContainsKey(rotatedDirs[6]) && cap.rules[rotatedDirs[6]] == true) matches++;
            if (w == 1 && cap.rules.ContainsKey(rotatedDirs[7]) && cap.rules[rotatedDirs[7]] == true) matches++;

            //If all directions match, it's the right cap.
            if (matches == cap.neighbors)
            {
              capToSpawn = cap;
              capToSpawn.rotation = rotation * -1;
              break;
            }
          }

          if (capToSpawn != null) break;
        }

        var capOffset = new Vector3(x - 0.5f, 0.5f, y + 0.3f);
        var capRotation = Quaternion.Euler(new Vector3(90f, capToSpawn.rotation, 0f));

        var goCap = Instantiate(this.cap, transform.position + capOffset, capRotation);
        goCap.transform.position = transform.position + capOffset;
        goCap.transform.rotation = capRotation;
        goCap.GetComponent<SpriteRenderer>().sprite = capToSpawn.sprite;

        goCap.transform.parent = floor.transform;
      }
    }
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
