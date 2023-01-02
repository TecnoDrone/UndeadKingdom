using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Projectiles
{
  public class EnergyTrace : MonoBehaviour
  {
    public float speed;
    public int energy;
    private float startTime;
    private Vector3 start;
    private float journeyLength;

    private ParticleSystem particleSystem;
    private AudioSource audioSource;

    public event EventHandler OnHitHandler;

    public void Start()
    {
      particleSystem = GetComponent<ParticleSystem>();
      audioSource = GetComponent<AudioSource>();
      audioSource.pitch = Random.Range(0.9f, 1.1f);

      startTime = Time.time;
      start = transform.position;
      journeyLength = Vector3.Distance(start, PlayerEntity.Instance.transform.position);
    }

    public void Update()
    {
      if (particleSystem.isStopped && particleSystem.particleCount == 0) Destroy(gameObject);

      if (!particleSystem.isStopped)
      {
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        transform.position = Vector3.Lerp(start, PlayerEntity.Instance.transform.position, fractionOfJourney);
      }
    }

    public void OnTriggerEnter(Collider other)
    {
      if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
      {
        PlayerEntity.Instance.GainLife(energy);
        particleSystem.Stop();
        OnHitHandler?.Invoke(this, null);
        Destroy(GetComponent<SphereCollider>());
      }
    }
  }
}
