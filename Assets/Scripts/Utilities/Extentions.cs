using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Extentions
{
  public static class GameObjectExtentions
  {
    public static void DisableSpriteRenderer(this GameObject gameObject)
    {
      SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

      if (spriteRenderer == null)
      {
        throw new Exception($"Cannot call {nameof(DisableSpriteRenderer)} on {gameObject.name}. Missing SpriteRenderer.");
      }

      spriteRenderer.enabled = false;
    }

    public static void EnableSpriteRenderer(this GameObject gameObject)
    {
      SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

      if (spriteRenderer == null)
      {
        throw new Exception($"Cannot call {nameof(EnableSpriteRenderer)} on {gameObject.name}. Missing SpriteRenderer.");
      }

      spriteRenderer.enabled = true;
    }
  }

  public static class BoxColliderExtentions
  {
    public static Vector2 GetRandomPointInCollider(this BoxCollider2D boxCollider2D)
    {
      float randomX = Random.Range(boxCollider2D.transform.position.x - boxCollider2D.size.x / 2, boxCollider2D.transform.position.x + boxCollider2D.size.x / 2);
      float randomY = Random.Range(boxCollider2D.transform.position.y - boxCollider2D.size.y / 2, boxCollider2D.transform.position.y + boxCollider2D.size.y / 2);

      return new Vector2(randomX, randomY);
    }
  }

  public static class DictionaryExtentions
  {
    public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
    {
      key = tuple.Key;
      value = tuple.Value;
    }
  }

  public static class ColorExtentions
  {
    public static Color ReduceAlpha(this Color color, float alpha)
    {
      return new Color(color.r, color.g, color.b, Mathf.Clamp(color.a, 0, color.a - alpha));
    }

    public static Color Intesify(this Color color, float intensity)
    {
      float factor = Mathf.Pow(2, intensity);
      return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }
  }

  public static class AudioSourceExtentions
  {
    public static void PlayClipAtPoint(this AudioSource audioSource, Vector3 point, AudioClip clip = null, float? pitch = null, float? volume = null)
    {
      var go = new GameObject("One shot audio");
      go.transform.position = point;

      var source = go.AddComponent<AudioSource>();
      source.clip = clip ?? audioSource.clip;
      source.pitch = pitch ?? audioSource.pitch;
      source.volume = volume ?? audioSource.volume;
      source.Play();
      UnityEngine.Object.Destroy(go, clip?.length ?? audioSource.clip.length);
    }
  }
}