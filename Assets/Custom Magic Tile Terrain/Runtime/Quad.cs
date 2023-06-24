using System.Linq;
using UnityEngine;

namespace CustomMagicSandbox.TileTerrain
{
  public class Quad
  {
    //If we know the size of the tile... and all the tiles have the same size...
    //Why not recursivelly generate the points of the grid by creating the tiles one by one.
    public float Size;
    public Triangle[] Triangles = new Triangle[2];
    public Vector2[] UVtexture = new Vector2[4];
    public Vector2[] UVgi = new Vector2[4];
    public Vector2[] UVlightmap = new Vector2[4];

    public Quad(float size, Triangle[] triangles, Vector2[] uV = default)
    {
      Size = size;
      Triangles = triangles;
      UVtexture = uV == default ? UV.Points : uV;
    }
  }

  public class Triangle
  {
    public Vertice[] Vertices = new Vertice[3];

    public Triangle(params Vertice[] vertices)
    {
      Vertices = vertices;
    }
  }

  public class Vertice
  {
    public Vector3 Position;
    public Color Color;

    public Vertice(Vector3 position, Color color = default)
    {
      Position = position;
      Color = color == default ? new Color(1, 1, 1) : color;
    }
  }
}