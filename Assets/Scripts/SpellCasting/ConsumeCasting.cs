using Assets.Scripts.AI;
using Assets.Scripts.Projectiles;
using System;
using System.Collections;
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

    public event EventHandler OnTraceSpawnedHandler;

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
        //consume life from target
        Target.TakeDamage(damage); 

        //Generate a new trace
        var go = Instantiate(energyTrace, Target.transform.position, transform.rotation);
        go.TryGetComponent<EnergyTrace>(out var script);

        //Specify some parameters on the trace
        script.energy = damage; //amount healed
        energyTraces++; //update trace count spawned
        OnTraceSpawnedHandler?.Invoke(this, null); //allow to listen on trace spawn
        script.OnHitHandler += new EventHandler(OnTraceHit); //listen on hit effect of trace

        if (Target.IsDead) forceStop = true;

        yield return new WaitForSeconds(timeBetweenCasts);
      }

      forceStop = false;
      StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
      channeling = null;
      State = SpellCastingState.Cooldown;
      onSpellCooldown.Invoke("Consume", cooldownTime);
      yield return new WaitForSeconds(cooldownTime);
      State = SpellCastingState.Ready;
    }

    public void OnTraceHit(object sender, EventArgs e) => energyTraces--;

    public void Stop()
    {
      if (channeling != null)
      {
        StopCoroutine(channeling);
        StartCoroutine(Cooldown());
      }

      forceStop = false;
    }
  }
}
