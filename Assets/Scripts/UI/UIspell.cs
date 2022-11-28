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

    [HideInInspector]
    public PlayerSpellContainer spellContainer;

    private bool isSameState = true;
    private bool isCostTooHigh = false;

    private void Start()
    {
      if(spellContainer.spellObject == null)
      {
        Destroy(gameObject);
        return;
      }

      var spellIcon = Resources.Load<Sprite>($"UI/{spellContainer.spellObject.name}");
      transform.Find("Icon").GetComponent<Image>().sprite = spellIcon;

      CheckCost(default);
      CheckState();

      PlayerEntity.onPlayerStateChange += CheckState;
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
      if (PlayerEntity.Instance.State == spellContainer.state)
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
      if (PlayerEntity.Instance.life - spellContainer.spellObject.Cost <= 0)
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

    protected override bool CanHandle(SpellObject spellObject)
    {
      if (spellObject.spellKind != spellContainer.spellObject.spellKind) return false;

      else return true;
    }

    protected override void Process(SpellObject spellObject)
    {
      StartCoroutine(CooldownAnimation(spellObject.CooldownTime));
    }
  }
}
