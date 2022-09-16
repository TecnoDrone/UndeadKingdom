using Assets.Scripts;
using Assets.Scripts.UI.SpellPicker;
using System.Collections.Generic;
using UnityEngine;

public class SpellPickerManager : MonoBehaviour
{
  public static GameObject CombatPicker;
  public static GameObject TacticPicker;

  public List<Slot> CombatSlots = new List<Slot>();
  public List<Slot> TacticSlots = new List<Slot>();
  public Stance stance = Stance.Combat;

  private Sprite spellSlotSprite;
  private Object[] IconsTilemap;

  private void Start()
  {
    spellSlotSprite = Resources.Load<Sprite>("UI/SpellSlot");

    IconsTilemap = Resources.LoadAll("UI/tilemap_packed");

    CombatSlots = new List<Slot>
    {
      new Slot(KeyCode.Mouse0, (Sprite)IconsTilemap[85], "Consume"),
      new Slot(KeyCode.Mouse0, (Sprite)IconsTilemap[77], "Fireball"),
      new Slot(KeyCode.Mouse1, (Sprite)IconsTilemap[78], "Reanimate"),
    };

    TacticSlots = new List<Slot>
    {
      new Slot(KeyCode.Mouse0, (Sprite)IconsTilemap[77], "SelectTroops"),
      new Slot(KeyCode.Mouse1, (Sprite)IconsTilemap[78], "CommandTroops"),
    };

    CombatPicker = AddSpellPicker(Stance.Combat, CombatSlots);
    TacticPicker = AddSpellPicker(Stance.Tactical, TacticSlots);

    TacticPicker.SetActive(false);
  }

  public GameObject AddSpellPicker(Stance stance, List<Slot> slots)
  {
    var go = new GameObject { name = stance.ToString() };

    var spellPicker = go.AddComponent<SpellPicker>();
    spellPicker.spellSlotSprite = spellSlotSprite;
    spellPicker.spellSlots = slots;
    spellPicker.transform.parent = transform;

    var rt = go.AddComponent<RectTransform>();
    rt.sizeDelta = rt.anchoredPosition = Vector2.zero;
    rt.anchorMin = new Vector2(0.5f - 1 / 2, 0);
    rt.anchorMax = new Vector2(0.5f - 1 / 2, 0);

    return go;
  }
}

public class Slot
{
  public KeyCode Key;
  public Sprite KeyIcon;
  public string Spell;

  public Slot(KeyCode key, Sprite keyIcon)
  {
    Key = key;
    KeyIcon = keyIcon;
  }

  public Slot(KeyCode key, Sprite keyIcon, string spell)
  {
    Key = key;
    KeyIcon = keyIcon;
    Spell = spell;
  }
}