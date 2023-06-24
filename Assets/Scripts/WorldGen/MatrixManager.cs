using System;
using System.Collections.Generic;
using UnityEngine;

public class MatrixManager
{
  private static bool Validate(Sprite sprite)
  {
    if (sprite == null)
    {
      Debug.LogWarning("Trying to generate matrix from NULL sprite.");
      return false;
    }

    var texture = sprite.texture;
    if (texture.width == 0 || texture.height == 0)
    {
      Debug.Log("Trying to generate matrix from sprite withouth height or widht.");
      return false;
    }

    return true;
  }

  public static int[][] PixelsToWallMatrix(Sprite sprite)
  {
    if (!Validate(sprite)) return null;

    var matrix = new List<int[]>();
    for (int y = 0; y < sprite.texture.height; y++)
    {
      var row = new List<int>();
      for (int x = 0; x < sprite.texture.width; x++)
      {
        var pixel = sprite.texture.GetPixel(x, y);
        var isWall = pixel == Color.black ? 1 : 0;
        row.Add(isWall);
      }
      matrix.Add(row.ToArray());
    }

    return matrix.ToArray();
  }

  public static int[][] PixelsToFloorMatrix(Sprite sprite)
  {
    if (!Validate(sprite)) return null;

    var tileColor = Color.white;

    var waterColor = new Color(
      95f / 255f, 
      205f / 255f, 
      228f / 255f);

    var doorColor = new Color(
      102f / 255f, 
      57f / 255f,
      49f / 255f);

    var matrix = new List<int[]>();
    for (int y = 0; y < sprite.texture.height; y++)
    {
      var row = new List<int>();
      for (int x = 0; x < sprite.texture.width; x++)
      {
        var pixel = sprite.texture.GetPixel(x, y);

        if (pixel == tileColor) row.Add(1);
        else if (pixel == doorColor) row.Add(2);
        else row.Add(0);
      }
      matrix.Add(row.ToArray());
    }

    return matrix.ToArray();
  }
}