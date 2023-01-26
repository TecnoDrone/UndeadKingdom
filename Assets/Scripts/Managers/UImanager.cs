using Assets.Scripts.Player;
using Assets.Scripts.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Managers.UI
{
  public class UImanager : MonoBehaviour
  {
    [SerializeField]
    private Transform GUI;

    private List<UIspell> uiSpells = new();

    public delegate void OnSpellCooldown(string spell, float cooldown);
    public static OnSpellCooldown onSpellCooldown;


    private void Start()
    {
      CreateSpellSlots();
      CreateChain();

      onSpellCooldown += ChainHandleCooldown;
    }

    private void CreateSpellSlots()
    {
      //var playerSpellContainsers = PlayerEntity.Instance.GetComponents<PlayerSpellContainer>().ToList();
      //var playerSpellsAmount = 3;

      //var width = 96;
      var inputs = GUI.Find("Inputs");
      //var resource = Resources.Load<GameObject>($"UI/SpellSlot");
      //
      //var offsetX = -width * (playerSpellsAmount / 2);

      var spellContainers = inputs.GetComponentsInChildren<UIspell>();
      uiSpells.AddRange(spellContainers);

      ///*** Reanimate ***/
      //var reanimateSpellSlot = Instantiate(resource, inputs, true);
      //reanimateSpellSlot.transform.localPosition = new Vector3(offsetX + 96 * 0, 48);
      //var reanimateUI = reanimateSpellSlot.GetComponent<UIspell>();
      //reanimateUI.spellKind = SpellKind.Reanimate;
      //reanimateUI.spellName = "Reanimate";
      //reanimateUI.spellStance = PlayerStance.Combat;
      //reanimateUI.spellCost = 0;
      //uiSpells.Add(reanimateUI);
      //
      ///*** Consume ***/
      //var consumeSpellSlot = Instantiate(resource, inputs, true);
      //consumeSpellSlot.transform.localPosition = new Vector3(offsetX + 96 * 1, 48);
      //var consumeUI = consumeSpellSlot.GetComponent<UIspell>();
      //consumeUI.spellKind = SpellKind.Consume;
      //consumeUI.spellName = "Consume";
      //consumeUI.spellStance = PlayerStance.Combat;
      //consumeUI.spellCost = -1;
      //uiSpells.Add(consumeUI);
      //
      ///*** DarkHail ***/
      //var darkHailSpellSlot = Instantiate(resource, inputs, true);
      //darkHailSpellSlot.transform.localPosition = new Vector3(offsetX + 96 * 2, 48);
      //var darkHailUI = darkHailSpellSlot.GetComponent<UIspell>();
      //darkHailUI.spellKind = SpellKind.DarkHail;
      //darkHailUI.spellName = "DarkHail";
      //darkHailUI.spellStance = PlayerStance.Combat;
      //darkHailUI.spellCost = 1;
      //uiSpells.Add(darkHailUI);
    }

    private void CreateChain()
    {
      for (int i = 0; i < uiSpells.Count - 1; i++)
      {
        uiSpells[i].SetNext(uiSpells[i + 1]);
      }
    }

    public void ChainHandleCooldown(string spell, float cooldown)
    {
      uiSpells.First().Handle(spell, cooldown);
    }
  }
}
