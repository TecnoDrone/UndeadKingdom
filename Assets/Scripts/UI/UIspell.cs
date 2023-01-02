using Assets.Scripts.Player;
using Assets.Scripts.Utilities.ResponsabilityChain;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
  public class UIspell : ChainedHandler<SpellObject>
  {
    public Slider slider;
    public TextMeshProUGUI timer;

    public SpellKind spellKind;
    public PlayerStance spellStance;
    public string spellName;
    public int spellCost;

    private bool isSameState = true;
    private bool isCostTooHigh = false;

    private void Start()
    {
      var spellIcon = Resources.Load<Sprite>($"UI/{spellName}");
      transform.Find("Icon").GetComponent<Image>().sprite = spellIcon;

      CheckCost(default);
      CheckState();

      PlayerEntity.onPlayerStanceChange += CheckState;
      PlayerEntity.onPlayerLifeGained += CheckCost;
      PlayerEntity.onPlayerLifeConsumed += CheckCost;
    }

    private IEnumerator CooldownAnimation(float cooldown)
    {
      timer.enabled = true;

      var time = 0.0f;
      while (time < cooldown)
      {
        time += Time.deltaTime;

        slider.value = ((cooldown - time) / cooldown) * 100;
        timer.text = Math.Round(cooldown - time, 1).ToString();

        yield return null;
      }

      timer.enabled = false;
    }

    private void Hide()
    {
      transform.position = new Vector3(transform.position.x, -10, transform.position.z);
    }

    private void Show()
    {
      transform.position = new Vector3(transform.position.x, 48, transform.position.z);
    }

    private void CheckState()
    {
      if (PlayerEntity.Instance.Stance == spellStance)
      {
        isSameState = true;
      }
      else
      {
        isSameState = false;
      }

      UpdateVisibility();
    }

    private void CheckCost(int amount)
    {
      if (PlayerEntity.Instance.life - spellCost <= 0)
      {
        isCostTooHigh = true;
      }
      else
      {
        isCostTooHigh = false;
      }

      UpdateVisibility();
    }

    private void UpdateVisibility()
    {
      if (!isCostTooHigh && isSameState)
      {
        Show();
      }
      else
      {
        Hide();
      }
    }

    public override bool CanHandle(string spell)
    {
      if (spell!= spellKind.ToString()) return false;

      else return true;
    }

    public override void Cooldown(float cooldown)
    {
      StartCoroutine(CooldownAnimation(cooldown));
    }
  }
}
