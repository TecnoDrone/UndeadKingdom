using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
  public class HiddenWhenOverlapping : MonoBehaviour
  {
    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private SpriteMask spriteMask;

    private void Start()
    {
      spriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
      spriteMask = GetComponentInChildren<SpriteMask>();
    }

    private void OnTriggerEnter(Collider other)
    {
      Hide();
    }

    private void OnTriggerExit(Collider other)
    {
      UnHide();
    }

    private void Hide()
    {
      foreach(var spriteRenderer in spriteRenderers)
      {
        spriteRenderer.material.SetFloat("_Opacity", 0.3f);
      }
      
      spriteMask.enabled = false;
    }

    private void UnHide()
    {
      foreach (var spriteRenderer in spriteRenderers)
      {
        spriteRenderer.material.SetFloat("_Opacity", 1f);
      }
      spriteMask.enabled = true;
    }
  }
}
