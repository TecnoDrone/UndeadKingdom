using Assets.Scripts.Spells;
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

    void Start()
    {
      //ReanimateObject = Resources.Load<GameObject>("/Prefabs/Spells/Reanimate/Reanimate.prefab");
      if (ReanimateObject == null) throw new ArgumentNullException(nameof(ReanimateObject));
    }

    protected bool TryCast()
    {
      //controlla se ci sono cadaveri nel raggio
      var foundCorpses = Physics.OverlapSphere(transform.position, reanimationRange, target);
      if (foundCorpses == null || !foundCorpses.Any()) return false;

      Corpses = foundCorpses.Select(c => c.gameObject).ToList();

      StartCoroutine(Channeling());
      return true;
    }

    IEnumerator Channeling()
    {
      State = SpellCastingState.Channeling;
      Instantiate(ReanimateObject, transform.position, default);
      foreach(var corpse in Corpses)
      {
        var reanimationTrace = Instantiate(ReanimationTrace, transform.position, transform.rotation);
        var traceScript = reanimationTrace.GetComponent<ReanimationTrace>();
        traceScript.SetTarget(corpse);
        traceScript.Launch();

        yield return new WaitForSeconds(timeBetweenReanimations);
      }

      ResetSpell();
      StartCoroutine(Cooldown());
      State = SpellCastingState.Cooldown;
    }

    IEnumerator Cooldown()
    {
      yield return new WaitForSeconds(cooldownTime);
      State = SpellCastingState.Ready;
    }

    void ResetSpell() => Corpses.Clear();
  }
}
