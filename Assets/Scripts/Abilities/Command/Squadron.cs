using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Spells
{
  public static class SquadronService
  {
    public static List<Vector3> GetPositions(int unitCount, Vector3 anchor, int spacingY)
    {
      if (unitCount > 50) throw new ArgumentException("too many units.");

      var configuration = Configurations.Get(unitCount);
      var maxAmount = configuration.Max();
      var positions = new List<Vector3>();

      for (int i = 0; i < configuration.Length; i++)
      {
        var amount = configuration[i];

        var zOffset = i * spacingY;
        var xOffset = (maxAmount - amount) / 2f;
        var rowPosition = new Vector3(anchor.x + xOffset, anchor.y, anchor.z + zOffset);

        var rowPositions = RowService.GeneratePositions(amount, rowPosition);
        positions.AddRange(rowPositions);
      }

      return positions;
    }
  }

  //Have to figure out a formula to do this dynamically
  public static class Configurations
  {
    private static readonly Dictionary<int, int[]> HowManyRows = new Dictionary<int, int[]>
    {
      //1 row
      { 1, new int[] { 1 }},
      { 2, new int[] { 2 }},
      { 3, new int[] { 3 }},
      { 4, new int[] { 4 }},
      { 5, new int[] { 5 }},
      { 6, new int[] { 6 }},

      //2 rows
      { 7, new int[] { 4, 3}},
      { 8, new int[] { 4, 4}},
      { 9, new int[] { 5, 4 }},
      { 10, new int[] { 5, 5 }},
      { 11, new int[] { 6, 5 }},
      { 12, new int[] { 6, 6 }},
      { 13, new int[] { 7, 6 }},
      { 14, new int[] { 7, 7 }},
      { 15, new int[] { 8, 7 }},
      { 16, new int[] { 8, 8 }},
      { 17, new int[] { 9, 8 }},
      { 18, new int[] { 9, 9 }},
      { 19, new int[] { 10, 9 }},
      { 20, new int[] { 10, 10 }}, 
      
      //3 rows
      { 21, new int[] { 7, 7, 7}},
      { 22, new int[] { 8, 8, 6}},
      { 23, new int[] { 8, 8, 7}},
      { 24, new int[] { 8, 8, 8}},
      { 25, new int[] { 9, 9, 7}},
      { 26, new int[] { 9, 9, 8}},
      { 27, new int[] { 9, 9, 9}},
      { 28, new int[] { 10, 10, 8}},
      { 29, new int[] { 10, 10, 9}},
      { 30, new int[] { 10, 10, 10}},

      //4 rows
      { 31, new int[] { 8, 8, 8, 7}},
      { 32, new int[] { 8, 8, 8, 8}},
      { 33, new int[] { 9, 9, 9, 6}},
      { 34, new int[] { 9, 9, 9, 7}},
      { 35, new int[] { 9, 9, 9, 8}},
      { 36, new int[] { 9, 9, 9, 9}},
      { 37, new int[] { 10, 10, 10, 7}},
      { 38, new int[] { 10, 10, 10, 8}},
      { 39, new int[] { 10, 10, 10, 9}},
      { 40, new int[] { 10, 10, 10, 10}},

      // 5 rows
      { 41, new int[] { 9, 9, 9, 9, 5 }},
      { 42, new int[] { 9, 9, 9, 9, 6 }},
      { 43, new int[] { 9, 9, 9, 9, 7 }},
      { 44, new int[] { 9, 9, 9, 9, 8 }},
      { 45, new int[] { 9, 9, 9, 9, 9 }},
      { 46, new int[] { 10, 10, 10, 10, 6 }},
      { 47, new int[] { 10, 10, 10, 10, 7 }},
      { 48, new int[] { 10, 10, 10, 10, 8 }},
      { 49, new int[] { 10, 10, 10, 10, 9 }},
      { 50, new int[] { 10, 10, 10, 10, 10 }}
    };

    public static int[] Get(int amount) => HowManyRows[amount];
  }
}
