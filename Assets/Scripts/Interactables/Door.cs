using Assets.Scripts;
using UnityEngine;

public class Door : MonoBehaviour
{
  public AudioClip openSE;
  public AudioClip lockedSE;

  public bool Interact()
  {
    if (PlayerEntity.Instance.keys == 0)
    {
      AudioSource.PlayClipAtPoint(lockedSE, transform.position, 0.1f);
      return false;
    }

    PlayerEntity.Instance.keys--;
    PlayerEntity.onPlayerKeyUpdate?.Invoke();
    AudioSource.PlayClipAtPoint(openSE, transform.position, 0.1f);
    Destroy(gameObject);
    return true;
  }
}