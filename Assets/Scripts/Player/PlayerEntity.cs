using Assets.Scripts.AI.Undead;
using Assets.Scripts.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
  public class PlayerEntity : Entity
  {
    public static PlayerEntity Instance { get; private set; }

    public static List<Undead> ControlledMinions;
    public List<Undead> SelectedUnits = new();

    public int keys;
    public delegate void OnPlayerKeyPickup();
    public static OnPlayerKeyPickup onPlayerKeyUpdate;

    public int souls;
    public delegate void OnPlayerSoulsPickup();
    public static OnPlayerSoulsPickup onPlayerSoulsUpdate;

    [HideInInspector] public static AudioListener listener;
    private Animator animator;
    public Coroutine dying;

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

    public override void OnDeath(Entity entity)
    {
      GetComponentInChildren<BoxCollider>().enabled = false;
      animator.SetInteger("IsDead", 1);
      audioSource.PlayOneShot(deathSoundEffect, 0.1f);

      State = PlayerState.Dying;
      onPlayerStateChange?.Invoke(State);

      dying = StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
      while(true)
      {
        if(animator.speed != 0)
        {
          var currentAnimatorState = animator.GetCurrentAnimatorStateInfo(0);
          //if animation is over, exit
          if (currentAnimatorState.normalizedTime >= 1f && currentAnimatorState.IsName("Death"))
          {
            animator.speed = 0;
          }
        }

        yield return null;
      }
    }

    public override void TakeDamage(int amount)
    {
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

    private void Init()
    {
      Instance = this;
      team = Team.Undead;

      listener = GetComponent<AudioListener>();
      animator = GetComponentInChildren<Animator>();

      ControlledMinions = new List<Undead>();
      ControlledMinions = FindObjectsOfType<Undead>()?.ToList();
    }
  }
}
