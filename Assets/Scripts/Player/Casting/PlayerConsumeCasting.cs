using Assets.Scripts.Projectiles;
using Assets.Scripts.SpellCasting;
using System;
using UnityEngine;

namespace Assets.Scripts.Player.Casting
{
  public class PlayerConsumeCasting : ConsumeCasting
  {
    public PlayerStance stance;
    public KeyCode key;

    private void Start()
    {
      PlayerEntity.onPlayerStanceChange += CheckStance;
      onEnergyTraceConsumed += CheckPlayerLife;
      OnTraceSpawnedHandler += CheckTraces;
    }

    void Update()
    {
      if (PlayerEntity.Instance.Stance != stance) return;

      if (Input.GetKeyDown(key))
      {
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

    void CheckTraces(object sender, EventArgs e) => CheckPlayerLife();

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
