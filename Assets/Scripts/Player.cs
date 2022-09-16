using Assets.Scripts.AI;
using Assets.Scripts.Generic;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
  public class Player : Entity
  {
    public static Player Instance { get; private set; }

    public static List<CombatAI> ControlledMinions;
    public static Stance stance;
    public float fireRate;
    public int energy;
    public int maxEnergy;

    [HideInInspector]
    public static AudioListener listener;
    private Spellcasting spellCasting;

    public void Awake()
    {
      Init();
    }

    public override void Update()
    {
      base.Update();

      if (GameManager.gameState == GameManager.GameState.Running)
      {
        if (Input.GetKeyUp(KeyCode.Tab)) SwapState();

        if (stance == Stance.Combat)
        {
          if (Input.GetKeyDown(KeyCode.Mouse0))
          {
            spellCasting.Cast("Fireball");
          }

          if (Input.GetKeyDown(KeyCode.Mouse1))
          {
            spellCasting.Cast("Reanimate");
          }

          if (Input.GetKeyDown(KeyCode.Q))
          {
            spellCasting.Cast("Consume");
          }
        }

        if (stance == Stance.Tactical)
        {
          if (Input.GetKeyDown(KeyCode.Mouse0))
          {
            spellCasting.Cast("SelectTroops");
          }

          if (Input.GetKeyDown(KeyCode.Mouse1))
          {
            spellCasting.Cast("CommandTroops");
          }
        }
      }
    }

    public override void TakeDamage(int dmg)
    {
      base.TakeDamage(dmg);
      HealthBar.Instance.RemoveHealth(dmg);
    }

    public void AddEnergy(int amount)
    {
      if (amount < 0) return;
      if (energy >= maxEnergy) return;

      energy = Mathf.Clamp(energy + amount, 0, maxEnergy);
      EnergyBar.Instance.AddEnergy(amount);
    }

    public void UseEnergy(int amount)
    {
      if (amount < 0) return;
      if (energy == 0) return;

      energy = Mathf.Clamp(energy - amount, 0, maxEnergy);
      EnergyBar.Instance.RemoveEnergy(amount);
    }

    public override void Death()
    {
      base.Death();
      Camera.main.transform.SetParent(null);
    }

    private void Init()
    {
      Instance = this;

      ControlledMinions = new List<CombatAI>();
      stance = Stance.Combat;

      ControlledMinions = FindObjectsOfType<CombatAI>()?.ToList();

      listener = gameObject.GetComponent<AudioListener>();

      spellCasting = gameObject.AddComponent<Spellcasting>();
      spellCasting.fireRate = fireRate;
    }

    private void SwapState()
    {
      if (stance == Stance.Combat)
      {
        stance = Stance.Tactical;
        SpellPickerManager.CombatPicker.SetActive(false);
        SpellPickerManager.TacticPicker.SetActive(true);
      }
      else
      {
        stance = Stance.Combat;
        SpellPickerManager.CombatPicker.SetActive(true);
        SpellPickerManager.TacticPicker.SetActive(false);
      }
    }
  }

  public enum Stance
  {
    Combat, Tactical
  }
}
