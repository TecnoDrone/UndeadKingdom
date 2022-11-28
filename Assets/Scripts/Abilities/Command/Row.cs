using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Spells
{
  public static class RowService
  {
    //Given a list of CAI, distribute them in a line to the right
    public static List<Vector3> GeneratePositions(int unitCount, Vector3 anchor)
    {
      if (unitCount > 10) throw new ArgumentException("Too many units.");

      var spacing = 1f;

      var positions = new List<Vector3>();
      for (int i = 0; i < unitCount; i++)
      {
        var width = 1;
        var heigth = 1;
        var xOffset = i * (spacing + width / 2);

        var x = anchor.x + xOffset;
        var y = anchor.y;
        var z = anchor.z + (heigth / 2);

        positions.Add(new Vector3(x, y, z));
      }

      return positions;
    }
  }
}
