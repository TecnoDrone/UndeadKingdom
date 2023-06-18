using System;
using System.Collections.Generic;
using UnityEngine;

public class MatrixManager
{
  //By design, will always return a square 12x12,
  //which is formed by a 10x10 terrain plus the state of other neighbor matrixes
  public static int[][] PixelsToMatrix(Sprite sprite)
  {
    if (sprite == null)
    {
      Debug.LogWarning("Trying to generate matrix from NULL sprite.");
      return null;
    }

    var texture = sprite.texture;
    if (texture.width == 0 || texture.height == 0)
    {
      Debug.Log("Trying to generate matrix from sprite withouth height or widht.");
      return null;
    }

    var matrix = new List<int[]>();
    for (int y = 0; y < texture.height; y++)
    {
      var row = new List<int>();
      for (int x = 0; x < texture.width; x++)
      {
        //NB: just for now that i'm working with a single room
        //if (y == -1 || x == -1 || x == texture.width || y == texture.height)
        //{
        //  row.Add(0);
        //  continue;
        //}

        var pixel = texture.GetPixel(x, y);
        var isWall = pixel == Color.black ? 1 : 0;
        row.Add(isWall);
      }
      matrix.Add(row.ToArray());
    }

    return matrix.ToArray();
  }
}