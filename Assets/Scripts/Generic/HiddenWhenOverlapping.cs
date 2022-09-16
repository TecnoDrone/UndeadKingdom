using UnityEngine;

namespace Assets.Scripts
{
  public class HiddenWhenOverlapping : MonoBehaviour
  {
    private SpriteRenderer spriteRenderer;
    private SpriteMask spriteMask;

    private void Start()
    {
      spriteRenderer = GetComponent<SpriteRenderer>();
      spriteMask = GetComponent<SpriteMask>();
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
      spriteRenderer.material.SetFloat("_Opacity", 0.2f);
      spriteMask.enabled = false;
    }

    private void UnHide()
    {
      spriteRenderer.material.SetFloat("_Opacity", 1f);
      spriteMask.enabled = true;
    }
  }
}
