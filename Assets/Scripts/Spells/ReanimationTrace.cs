using Assets.Scripts.AI.Undead;
using Extentions;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Spells.Projectile
{
  public class ReanimationTrace : MonoBehaviour
  {
    public float upwardTime;
    public float movementSpeed;
    public float rotationSpeed;

    private GameObject Target;
    public void SetTarget(GameObject target) => Target = target;

    Coroutine MoveUpwardCoroutine;
    Coroutine HomeInCoroutine;

    private ParticleSystem particleSystem;

    public void Start()
    {
      var audioSource = GetComponent<AudioSource>();
      audioSource.pitch = Random.Range(0.9f, 1.1f);
      audioSource.PlayClipAtPoint(transform.position);

      particleSystem = GetComponent<ParticleSystem>();
    }

    public void Launch()
    {
      if (Target == null)
      {
        Destroy(gameObject);
        throw new ArgumentNullException("Reanimate failed. Target is null.");
      }

      Destroy(gameObject, 10);
      MoveUpwardCoroutine = StartCoroutine(MoveUpward());
    }

    public void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.GetInstanceID() != Target.GetInstanceID()) return;

      //Generate undead
      var corpseBehavior = other.GetComponent<Corpse>();
      var undead = Instantiate(corpseBehavior.Reanimation, other.GetComponentInChildren<SpriteRenderer>().bounds.center, corpseBehavior.Reanimation.transform.rotation);

      //Set undead under player control
      PlayerEntity.ControlledMinions.Add(undead.GetComponent<Undead>());

      //Destroy corpse
      Destroy(other.gameObject);

      //Destroy trace
      //Destroy(gameObject);

      if (MoveUpwardCoroutine != null) StopCoroutine(MoveUpwardCoroutine);
      if (HomeInCoroutine != null) StopCoroutine(HomeInCoroutine);
      particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
      StartCoroutine(QueueDestruction());
    }

    IEnumerator QueueDestruction()
    {
      while (true)
      {
        if (particleSystem.particleCount == 0) Destroy(gameObject);
        yield return null;
      }
    }

    IEnumerator MoveUpward()
    {
      var time = 0.0f;
      while (time < upwardTime)
      {
        transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
        time += Time.deltaTime;
        yield return null;
      }

      HomeInCoroutine = StartCoroutine(HomeIn());
    }

    IEnumerator HomeIn()
    {
      while (true)
      {
        Vector3 targetDirection = Target.transform.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, rotationSpeed * Time.deltaTime, 0.0F);
        transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(newDirection);
        rotationSpeed += 0.1f;
        yield return null;
      }
    }
  }
}
