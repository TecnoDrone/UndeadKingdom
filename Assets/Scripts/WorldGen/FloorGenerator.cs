using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
  public Sprite map;
  public Sprite[] tiles;
  public int[][] matrix;

  public void Generate()
  {
    if (matrix == null) return;

    foreach (Transform child in transform)
    {
      DestroyImmediate(child.gameObject);
    }

    for (int z = 0; z < matrix.Length; z++)
    {
      var row = matrix[z];
      for (int x = 0; x < row.Length; x++)
      {
        //false means no cap or wall to place.
        if (row[x] == 0) continue;

        var offset = new Vector3(x + 0.5f, 0, z + 0.5f);

        var tile = Instantiate(
          new GameObject(),
          transform.position + offset,
          Quaternion.Euler(new Vector3(90f, 0f, 0f)));

        tile.name = $"Tile_z{z}_x{x}";
        tile.transform.SetParent(transform);

        var sr = tile.AddComponent<SpriteRenderer>();
        sr.sprite = tiles[0];
      }
    }
  }
}