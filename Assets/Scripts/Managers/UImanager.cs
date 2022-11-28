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

    private void Start()
    {
      CreateSpellSlots();
      CreateChain();

      PlayerSpellContainer.onSpellCooldown += ChainHandleCooldown;
    }

    private void CreateSpellSlots()
    {
      var playerSpellContainsers = PlayerEntity.Instance.GetComponents<PlayerSpellContainer>().ToList();

      var width = 96;
      var inputs = GUI.Find("Inputs");
      var offsetX = -width * (playerSpellContainsers.Count / 2);

      for (int i = 0; i < playerSpellContainsers.Count; i++)
      {
        var x = offsetX + (i * 96);

        var resource = Resources.Load<GameObject>($"UI/SpellSlot");

        var spellSlot = Instantiate(resource, inputs, true);
        spellSlot.transform.localPosition = new Vector3(x, 48);

        var uiSpell = spellSlot.GetComponent<UIspell>();
        uiSpell.spellContainer = playerSpellContainsers[i];
        uiSpells.Add(uiSpell);
      }
    }

    private void CreateChain()
    {
      for(int i = 0; i < uiSpells.Count - 1; i++)
      {
        uiSpells[i].SetNext(uiSpells[i + 1]);
      }
    }

    public void ChainHandleCooldown(SpellObject spell)
    {
      uiSpells.First().Handle(spell);
    }
  }
}
