using Assets.Scripts.AI;
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

    public void Start()
    {
      var audioSource = GetComponent<AudioSource>();
      audioSource.pitch = Random.Range(0.9f, 1.1f);
      transform.rotation = Quaternion.LookRotation(-transform.right);
    }

    public void Launch()
    {
      if (Target == null)
      {
        Destroy(gameObject);
        throw new ArgumentNullException("Reanimate failed. Target is null.");
      }

      Destroy(gameObject, 10);
      StartCoroutine(MoveUpward());
    }

    public void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.GetInstanceID() != Target.GetInstanceID()) return;

      //Generate undead
      var corpseBehavior = other.GetComponent<Corpse>();
      var undead = Instantiate(corpseBehavior.Reanimation, other.transform.position, corpseBehavior.Reanimation.transform.rotation);

      //Set undead under player control
      PlayerEntity.ControlledMinions.Add(undead.GetComponent<CombatAI>());

      //Destroy corpse
      Destroy(other.gameObject);

      Destroy(gameObject);
    }

    IEnumerator MoveUpward()
    {
      var time = 0.0f;
      while (time < upwardTime)
      {
        transform.Translate(transform.forward * movementSpeed * Time.deltaTime, Space.Self);
        time += Time.deltaTime;
        yield return null;
      }

      //StartCoroutine(HomeIn());
    }

    IEnumerator HomeIn()
    {
      while (true)
      {
        Vector3 targetDirection = Target.transform.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, rotationSpeed * Time.deltaTime, 0.0F);
        transform.Translate(transform.forward * movementSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(newDirection);
        yield return null;
      }
    }
  }
}
