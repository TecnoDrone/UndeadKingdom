using System.Collections;
using UnityEngine;

public class Spiketrap : MonoBehaviour
{
  public int damage;
  public LayerMask whatCanHit;
  public float upSpeed;
  public float downSpeed;
  public float maxDistance;
  public float waitTime;

  public AudioClip triggerSE;
  public AudioClip unTriggerSE;

  private Transform spikes;

  private bool isTriggered = false;

  private Vector3 startingPos;
  private Coroutine goingUp;
  private Coroutine goingDown;

  public void Awake()
  {
    spikes = transform.Find("Spikes");
    startingPos = spikes.transform.position;
  }

  public void OnTriggerEnter(Collider other)
  {
    if (isTriggered) return;
    if ((1 << other.gameObject.layer & whatCanHit.value) <= 0) return;

    var entity = other.GetComponentInParent<Entity>();
    entity.TakeDamage(damage);

    AudioSource.PlayClipAtPoint(triggerSE, transform.position, 0.1f);
    isTriggered = true;
    goingUp = StartCoroutine(Up());
  }

  public IEnumerator Up()
  {
    while (true)
    {
      var distance = spikes.TransformDirection(Vector3.up) * upSpeed * Time.deltaTime;

      if ((spikes.position + distance).y >= maxDistance)
      {
        StopCoroutine(goingUp);
        goingDown = StartCoroutine(Down());
      }

      spikes.position += distance;

      //spikes.position += mov;
      yield return null;
    }
  }

  public IEnumerator Down()
  {
    yield return new WaitForSeconds(waitTime);
    AudioSource.PlayClipAtPoint(unTriggerSE, transform.position, 0.01f);

    while (true)
    {
      var mov = spikes.TransformDirection(Vector3.up) * downSpeed * Time.deltaTime;
      spikes.position -= mov;

      if (spikes.position.y <= startingPos.y)
      {
        isTriggered = false;
        spikes.position = startingPos;
        StopCoroutine(goingDown);
      }

      yield return null;
    }
  }
}
