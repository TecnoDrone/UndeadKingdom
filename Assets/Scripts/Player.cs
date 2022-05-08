using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
  public class Player : Entity
  {
    public float fireRate;

    public static Player Instance { get; private set; }

    public static List<Skeleton> controlledMinions = new List<Skeleton>();
    public static Stance stance = Stance.Combat;

    [HideInInspector]
    public static AudioListener audio;

    private Spellcasting spellCasting;

    public override void Start()
    {
      base.Start();
      Instance = this;

      controlledMinions = FindObjectsOfType<Skeleton>()?.ToList();

      spellCasting = gameObject.AddComponent<Spellcasting>();
      audio = gameObject.GetComponent<AudioListener>();
      spellCasting.fireRate = fireRate;
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

    public Vector3 GetPosition()
    {
      return Instance.transform.position;
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
