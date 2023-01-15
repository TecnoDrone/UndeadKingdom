using Assets.Scripts.AI.Undead;
using Assets.Scripts.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
  public class PlayerEntity : Entity
  {
    public static PlayerEntity Instance { get; private set; }

    public static List<Undead> ControlledMinions;

    [HideInInspector]
    public static AudioListener listener;

    public PlayerStance Stance;
    public delegate void OnPlayerStanceChange();
    public static OnPlayerStanceChange onPlayerStanceChange;

    public PlayerState State;
    public delegate void OnPlayerStateChange(PlayerState state);
    public static OnPlayerStateChange onPlayerStateChange;

    public delegate void OnPlayerLifeConsumed(int amount);
    public static OnPlayerLifeConsumed onPlayerLifeConsumed;

    public delegate void OnPlayerLifeGained(int amount);
    public static OnPlayerLifeGained onPlayerLifeGained;

    public void Awake() => Init();

    public override void Update()
    {
      base.Update();

      if (Input.GetKeyUp(KeyCode.LeftShift))
      {
        if (Stance == PlayerStance.Combat)
        {
          Stance = PlayerStance.Command;
        }

        else if (Stance == PlayerStance.Command)
        {
          Stance = PlayerStance.Combat;
        }

        onPlayerStanceChange?.Invoke();
      }
    }

    public override void TakeDamage(int amount)
    {
      if (amount == 0) return;

      base.TakeDamage(amount);
      onPlayerLifeConsumed?.Invoke(amount);
    }

    public void ConsumeLife(int amount)
    {
      if (amount == 0) return;

      life -= amount;
      if (life < 1) life = 1;

      onPlayerLifeConsumed?.Invoke(amount);
    }

    public void GainLife(int amount)
    {
      if (amount == 0) return;

      life += amount;
      if (life > maxLife) life = maxLife;

      onPlayerLifeGained?.Invoke(amount);
    }

    public override void Death()
    {
      base.Death();
      Camera.main.transform.SetParent(null);
    }

    private void Init()
    {
      Instance = this;

      listener = GetComponent<AudioListener>();

      ControlledMinions = new List<Undead>();
      ControlledMinions = FindObjectsOfType<Undead>()?.ToList();
    }
  }
}
