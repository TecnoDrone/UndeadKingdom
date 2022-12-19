using Assets.Scripts.Player;
using Assets.Scripts.Spells.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.SpellCasting
{
  public class ReanimationCasting : MonoBehaviour
  {
    [InspectorName("What is corpse")]
    public LayerMask target;

    public float reanimationRange;
    public float timeBetweenReanimations;
    public float cooldownTime;

    protected SpellCastingState State = SpellCastingState.Ready;

    public GameObject ReanimateObject;
    public GameObject ReanimationTrace;
    private List<GameObject> Corpses;

    private Animator animator;

    void Start()
    {
      if (ReanimateObject == null) throw new ArgumentNullException(nameof(ReanimateObject));
      animator = GetComponentInChildren<Animator>();
    }

    protected bool TryCast()
    {
      //controlla se ci sono cadaveri nel raggio
      var foundCorpses = Physics.OverlapSphere(transform.position, reanimationRange, target);
      if (foundCorpses == null || !foundCorpses.Any()) return false;

      Corpses = foundCorpses.Select(c => c.gameObject).ToList();

      PlayerEntity.Instance.State = PlayerState.Casting; //Add a reference to the caster as a public variable...
      PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Casting);
      StartCoroutine(Buildup());
      return true;
    }

    IEnumerator Buildup()
    {
      //start animation
      animator.SetBool("Consume", true);

      while (true)
      {
        var currentAnimatorState = animator.GetCurrentAnimatorStateInfo(0);
        //if animation is over, exit
        if (currentAnimatorState.normalizedTime > 1 && currentAnimatorState.IsName("Consume"))
        {
          StartCoroutine(Channeling());
          break;
        }
        yield return null;
      }
    }

    IEnumerator Channeling()
    {
      State = SpellCastingState.Channeling;
      var reanimate = Instantiate(ReanimateObject, transform.position, default);
      foreach (var corpse in Corpses)
      {
        var reanimationTrace = Instantiate(ReanimationTrace, transform.position, ReanimationTrace.transform.rotation);
        var traceScript = reanimationTrace.GetComponent<ReanimationTrace>();
        traceScript.SetTarget(corpse);
        traceScript.Launch();

        yield return new WaitForSeconds(timeBetweenReanimations);
      }

      //Reset spell
      Corpses.Clear();
      State = SpellCastingState.Cooldown;
      StartCoroutine(Cooldown());

      //Close animation
      var ps = reanimate.GetComponent<ParticleSystem>();
      ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
      animator.SetBool("Consume", false);
      while (true)
      {
        var currentAnimatorState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentAnimatorState.normalizedTime > 1 && currentAnimatorState.IsName("Consume_End"))
        {
          PlayerEntity.Instance.State = PlayerState.Idle;
          PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Idle);
        }
        yield return null;
      }
    }

    IEnumerator Cooldown()
    {
      yield return new WaitForSeconds(cooldownTime);
      State = SpellCastingState.Ready;
    }
  }
}
