using Assets.Scripts.Projectiles;
using Assets.Scripts.SpellCasting;
using UnityEngine;

namespace Assets.Scripts.Player.Casting
{
  public class PlayerConsumeCasting : ConsumeCasting
  {
    public PlayerStance stance;
    public KeyCode key;

    private float energyCost;

    private void Start()
    {
      PlayerEntity.onPlayerStanceChange += CheckStance;
      onEnergyTraceConsumed += CheckPlayerLife;

      energyCost = energyTrace.GetComponent<EnergyTrace>().energy;
    }

    void Update()
    {
      if (Input.GetKeyDown(key))
      {
        if (PlayerEntity.Instance.Stance != stance) return;
        if (PlayerEntity.Instance.life == PlayerEntity.Instance.maxLife) return;
        if (State != SpellCastingState.Ready) return;

        if (TryCast())
        {
          PlayerEntity.Instance.State = PlayerState.Casting;
          PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Casting);
        }
      }

      if (Input.GetKeyUp(key))
      {
        Stop();
        PlayerEntity.Instance.State = PlayerState.Idle;
        PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Idle);
      }
    }

    void CheckPlayerLife()
    {
      if (PlayerEntity.Instance.life + energyTraces >= PlayerEntity.Instance.maxLife)
        forceStop = true;
    }

    void CheckStance()
    {
      if (PlayerEntity.Instance.Stance != stance)
      {
        Stop();
        PlayerEntity.Instance.State = PlayerState.Idle;
        PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Idle);
      }
    }
  }
}
