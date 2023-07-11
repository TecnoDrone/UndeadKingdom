using System.Collections.Generic;
using UnityEngine;

public enum Tile
{
  Empty, Wall, Floor, Door
}


public static class ColorExtentions
{
  internal static Dictionary<Color, Tile> Palette = new Dictionary<Color, Tile>
    {
      { Color.black, Tile.Wall },
      { Color.white, Tile.Floor },
      { new Color(102f/255f, 57f/255f, 49f/255f), Tile.Door }
    };

  public static Tile ToTile(this Color color) => Palette.ContainsKey(color) ? Palette[color] : Tile.Empty;
}

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

  public static Tile[][] PixelsToMatrix(Sprite sprite)
  {
    if (!Validate(sprite)) return null;

    var matrix = new List<Tile[]>();
    for (int y = 0; y < sprite.texture.height; y++)
    {
      var row = new List<Tile>();

      //If at any point a tile is on top of a wall,
      //copy the entire line one row above and
      //replace every tile in this row from floor to empty

      for (int x = 0; x < sprite.texture.width; x++)
      {
        row.Add(sprite.texture
          .GetPixel(x, y)
          .ToTile());
      }
      matrix.Add(row.ToArray());
    }

    return matrix.ToArray();
  }
}