using Assets.Scripts;
using UnityEngine;

public class Soul : MonoBehaviour
{
  public AudioClip pickupSE;

  private void OnTriggerEnter(Collider other)
  {
    if (other.tag == "Player")
    {
      PlayerEntity.Instance.souls++;
      PlayerEntity.onPlayerSoulsUpdate?.Invoke();

      AudioSource.PlayClipAtPoint(pickupSE, transform.position, 0.1f);
      Destroy(gameObject);
    }
  }
}
