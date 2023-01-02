using Extentions;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Spells
{
  public class DarkFireball : MonoBehaviour
  {
    public AudioClip hitSound;
    public GameObject hitObject;

    public float speed;

    [HideInInspector]
    public Vector3 destination;

    [HideInInspector]
    public float trajectoryHeight;

    private float cTime = 0;
    private float timerThrow = 0;
    private Vector3 startPos;

    private ParticleSystem[] ps;
    private AudioSource audioSource;

    private Coroutine followPath;
    private State state = State.Moving;

    private void Start()
    {
      startPos = transform.position;
      timerThrow = Vector3.Distance(transform.position, destination);
      ps = transform.GetComponentsInChildren<ParticleSystem>();
      audioSource = GetComponent<AudioSource>();
      audioSource.pitch += Random.Range(-0.2f, 0.2f);

      ChangeState(State.Moving);
    }

    private void ChangeState(State value)
    {
      state = value;

      if (state == State.Moving && followPath == null)
      {
        followPath = StartCoroutine(FollowPath());
      }

      if (state == State.Hitting)
      {
        if (followPath != null)
        {
          StopCoroutine(followPath);
          followPath = null;
        }

        Hit();
      }
    }

    IEnumerator FollowPath()
    {
      while (transform.position != destination)
      {
        cTime += speed * Time.deltaTime;

        // calculate straight-line lerp position:
        Vector3 currentPos = Vector3.Lerp(startPos, destination, cTime / timerThrow);

        // add a value to Y, using Sine to give a curved trajectory in the Y direction
        currentPos.y += trajectoryHeight * Mathf.Sin(Mathf.Clamp01(cTime / timerThrow) * Mathf.PI);

        transform.position = currentPos;

        yield return null;
      }

      ChangeState(State.Hitting);
    }

    private void Hit()
    {
      foreach (var p in ps)
      {
        p.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
      }

      GetComponentInChildren<SpriteRenderer>().enabled = false;

      audioSource.PlayClipAtPoint(transform.position, hitSound);
      Instantiate(hitObject, new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z), hitObject.transform.rotation); //NB: looks good only when near terrain

      StartCoroutine(TearDown());
    }

    IEnumerator TearDown()
    {
      while (true)
      {
        if (!audioSource.isPlaying)
          Destroy(gameObject);

        yield return null;
      }
    }

    private enum State
    {
      Moving, Hitting
    }
  }
}
