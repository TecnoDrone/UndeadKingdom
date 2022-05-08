using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
  public static class Utility
  {
    public static List<GameObject> FindInCircle(Vector2 position, float range, LayerMask layer)
    {
      var colliders = Physics2D.OverlapCircleAll(position, range, layer);

      if (colliders != null && colliders.Length > 0)
      {
        var gameobjects = new List<GameObject>();
        for (int i = 0; i < colliders.Length; i++)
        {
          gameobjects.Add(colliders[i].gameObject);
        }
        return gameobjects;
      }

      return new List<GameObject>();
    }

    public static Vector2 GetUnitOnCircle(float radius)
    {
      float angle = Random.Range(0f, Mathf.PI * 2);
      float x = Mathf.Sin(angle) * radius;
      float y = Mathf.Cos(angle) * radius;

      return new Vector2(x, y);
    }

    public static Vector3 ZeroY(this Vector3 position, float yOffset = 0)
    {
      return new Vector3(position.x, yOffset, position.z);
    }
  }
}
