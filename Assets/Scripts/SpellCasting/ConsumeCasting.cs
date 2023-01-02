using Assets.Scripts.AI;
using Assets.Scripts.Projectiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Managers.UI.UImanager;

namespace Assets.Scripts.SpellCasting
{
  public class ConsumeCasting : MonoBehaviour
  {
    public SpellCastingState State = SpellCastingState.Ready;

    public GameObject energyTrace;
    public int damage;
    public float timeBetweenCasts;
    public float cooldownTime;

    [HideInInspector]
    public bool forceStop;
    [HideInInspector]
    public int energyTraces = 0;

    private EntityAI Target;
    private Coroutine channeling;

    public delegate void OnEnergyTraceConsumed();
    public OnEnergyTraceConsumed onEnergyTraceConsumed;

    protected bool TryCast()
    {
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

      if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Undead"))) return false;
      if (!hit.transform.TryGetComponent(out Target)) return false;

      channeling = StartCoroutine(Channeling());
      return true;
    }

    IEnumerator Channeling()
    {
      State = SpellCastingState.Channeling;

      while (!forceStop)
      {
        Target.TakeDamage(damage);
        var go = Instantiate(energyTrace, Target.transform.position, transform.rotation);
        go.TryGetComponent<EnergyTrace>(out var script);
        script.energy = damage;
        energyTraces++;

        script.OnHitHandler += new EventHandler(OnTraceHit);
        if (Target.IsDead) forceStop = true;

        yield return new WaitForSeconds(timeBetweenCasts);
      }

      StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
      channeling = null;
      forceStop = false;
      State = SpellCastingState.Cooldown;
      onSpellCooldown.Invoke("Consume", cooldownTime);
      yield return new WaitForSeconds(cooldownTime);
      State = SpellCastingState.Ready;
    }

    public void OnTraceHit(object sender, EventArgs e)
    {
      energyTraces--;
      onEnergyTraceConsumed?.Invoke();
    }

    public void Stop()
    {
      if (channeling != null)
      {
        StopCoroutine(channeling);
        StartCoroutine(Cooldown());
      }
    }
  }
}
