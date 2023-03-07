using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
  private Wall[,] wallMatrix;



  bool TryGetWalls(out List<Wall> rows, out List<Wall> columns)
  {
    columns = new List<Wall>();
    rows = new List<Wall>();

    foreach (GameObject child in transform)
    {
      if (!child.TryGetComponent<Wall>(out var wall)) continue;


    }

    if (!columns.Any() || !rows.Any()) return false;

    return true;
  }

  Wall[,] GenerateMatrix(List<Wall> walls)
  {
    if (walls == null || !walls.Any()) return null;

    var xMax = (int)walls.Max(c => c.transform.position.x);
    var xMin = (int)walls.Min(c => c.transform.position.x);
    var zMax = (int)walls.Max(c => c.transform.position.z);
    var zMin = (int)walls.Min(c => c.transform.position.z);

    var width = Mathf.Abs(xMax - xMin);
    var height = Mathf.Abs(zMax - zMin);

    var matrix = new Wall[width, height];

    //populate the matrix
    foreach(var wall in walls)
    {
      matrix[(int)wall.transform.position.x, (int)wall.transform.position.z] = wall;
    }

    //Fill empty spaces with filler walls
    for(int x = xMin; x < xMax; x++)
    {
      for(int z = zMin; z < zMax; z++)
      {
        if(matrix[x,z] == null)
        {
          var wall = new GameObject().AddComponent<Wall>();
          wall.transform.position = new Vector3(x, 0, z);
          wall.name = "Wall";
          matrix[x, z] = wall;
        }
      }
    }

    return matrix;
  }

  void CalculateMatrixDirections(Wall[,] matrix)
  {
    foreach(var wall in matrix)
    {
      
    }
  }

  void InitializeMatrixWalls(Wall[,] matrix)
  {

  }
}
