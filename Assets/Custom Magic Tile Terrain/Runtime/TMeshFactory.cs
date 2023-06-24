using System.Collections.Generic;
using UnityEngine;

namespace CustomMagicSandbox.TileTerrain
{
  public class TMeshFactory
  {
    private Vector3Int Size;
    private float TileSize = 1f;

    //TODO: replace with new logic
    private List<Vector3> gridPoints = new();

    public TMeshFactory WithMeshSize(int X, int Z)
    {
      Size.x = X <= 0 ? 1 : X;
      Size.z = Z <= 0 ? 1 : Z;
      return this;
    }

    public TMeshFactory WithTileSize(float size)
    {
      TileSize = size;
      return this;
    }

    //Create vertices for the new Quad.
    //Vertices are arranged in a N shape into the vertices list.
    //Because grid points are ordered in a linear fashion row by row we need to map the vertices to that format.
    Quad CreateQuadFromGrid(int gridPointIndex)
    {
      var quadIndex = Mathf.FloorToInt(gridPointIndex / Size.x);
      var triangles = new Triangle[]
      {
        new Triangle(new Vertice(gridPoints[quadIndex]),
                     new Vertice(gridPoints[quadIndex + Size.x + 1]),
                     new Vertice(gridPoints[quadIndex + 1])),

        new Triangle(new Vertice(gridPoints[quadIndex + 1]),
                     new Vertice(gridPoints[quadIndex + Size.x + 1]),
                     new Vertice(gridPoints[quadIndex + Size.x + 2]))
      };

      var quad = new Quad(TileSize, triangles);
      return quad;
    }

    //public TMeshDTO Build()
    //{
    //  var mesh = new TMeshDTO(Size);

    //  //Define Grid point positions
    //  for (int z = 0, i = 0; z <= Size.z; z++) //Z axis
    //  {
    //    for (int x = 0; x <= Size.x; x++, i++) //X axis
    //    {
    //      if (i > gridPoints.Count)
    //      {
    //        Debug.Log("i=" + i + " gridPoints.Count=" + gridPoints.Count);
    //        continue;
    //      }
    //      gridPoints[i] = new Vector3((float)x * TileSize, 0, (float)z * TileSize);
    //    }
    //  }

    //  for (int i = 0; i < Size.x * Size.z; i++)
    //  {
    //    var quad = CreateQuadFromGrid(i);
    //    mesh.Quads.Add(quad);
    //  }

    //  return mesh;
    //}
  }
}