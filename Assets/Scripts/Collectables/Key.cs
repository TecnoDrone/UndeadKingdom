using Assets.Scripts;
using UnityEngine;

public class Key : MonoBehaviour
{
  public AudioClip pickupSE;

  private void OnTriggerEnter(Collider other)
  {
    if (other.tag == "Player")
    {
      PlayerEntity.Instance.keys++;
      PlayerEntity.onPlayerKeyUpdate?.Invoke();

      AudioSource.PlayClipAtPoint(pickupSE, transform.position, 0.1f);
      Destroy(gameObject);
    }
  }
}