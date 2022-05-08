using Assets.Scripts.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SpellPicker
{
  public class SpellPicker : MonoBehaviour
  {
    public List<Slot> spellSlots = new List<Slot>();
    private List<GameObject> UIelemets = new List<GameObject>();

    public Sprite spellSlotSprite;

    private readonly float spacing = 10;
    private readonly float size = 50;

    public void Start()
    {
      foreach (var slot in spellSlots)
      {
        AddSpellSlot(slot.Key, slot.KeyIcon, slot.Spell);
      }

      ArrangeSlots();
    }

    /// <summary>
    /// Adds a spell slot in the UI
    /// </summary>
    public void AddSpellSlot(KeyCode key, Sprite keyIcon, string spell)
    {
      //Spell slot ICON
      var spellSlotUI = new GameObject();
      spellSlotUI.transform.parent = gameObject.transform;
      spellSlotUI.name = "SpellSlot";

      var spellSlotImage = spellSlotUI.AddComponent<Image>();
      spellSlotImage.sprite = spellSlotSprite;
      spellSlotImage.rectTransform.sizeDelta = new Vector2(size, size);

      //Spell ICON
      var spellUI = new GameObject();
      spellUI.transform.parent = spellSlotUI.transform;
      spellUI.name = spell;

      var spellImage = spellUI.AddComponent<Image>();

      var sprite = GetSpellSprite(spell);
      if (sprite != null) spellImage.sprite = sprite;

      spellImage.rectTransform.sizeDelta = new Vector2(size, size);

      var spellIconUI = new GameObject();
      spellIconUI.transform.parent = spellSlotUI.transform;
      spellIconUI.name = key.ToString();
      spellIconUI.transform.localScale = new Vector3(.3f, .3f, 1);
      spellIconUI.transform.localPosition = new Vector3(0, size, 0);

      var mouseImage = spellIconUI.AddComponent<Image>();
      mouseImage.sprite = keyIcon;

      UIelemets.Add(spellSlotUI);
    }

    /// <summary>
    /// Makes each slot be near the other
    /// </summary>
    public void ArrangeSlots()
    {
      if (!UIelemets?.Any() == null) return;

      var totalWidth = UIelemets.Count * size;
      var start = -(totalWidth / 2) + (size / 2);
      var offset = 0f;

      for (int i = 0; i < UIelemets.Count; i++, offset += size + spacing)
      {
        UIelemets[i].transform.localPosition = new Vector3(start + offset, size / 2 + 10f, 0);
      }
    }

    /// <summary>
    /// Given a spell, can return the correct sprite to use on the UI
    /// </summary>
    public Sprite GetSpellSprite(string spell) => Resources.Load<Sprite>($"UI/{spell}");
  }
}
