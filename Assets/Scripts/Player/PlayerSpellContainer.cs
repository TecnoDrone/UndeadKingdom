using UnityEngine;

namespace Assets.Scripts.Player
{
  public class PlayerSpellContainer : MonoBehaviour
  {
    public SpellObject spellObject;
    public KeyCode key;
    public PlayerState state;

    private float activeTime;
    private float cooldownTime;

    public delegate void OnSpellCooldown(SpellObject spell);
    public static OnSpellCooldown onSpellCooldown;

    private void Update()
    {
      switch (spellObject.state)
      {
        case SpellStates.Ready:
          if (Input.GetKeyDown(key))
          {
            if (PlayerEntity.Instance.State != state) return;
            if (PlayerEntity.Instance.life <= spellObject.Cost) return;

            if(spellObject.TryCast(gameObject))
            {
              PlayerEntity.Instance.ConsumeLife(spellObject.Cost);
              spellObject.state = SpellStates.Active;
              activeTime = spellObject.ActiveTime;
            }
          }
          break;

        case SpellStates.Active:
          if (activeTime > 0)
          {
            activeTime -= Time.deltaTime;
          }
          else
          {
            spellObject.state = SpellStates.Cooldown;
            cooldownTime = spellObject.CooldownTime;

            onSpellCooldown?.Invoke(spellObject);
          }
          break;

        case SpellStates.Cooldown:
          if (cooldownTime > 0)
          {
            cooldownTime -= Time.deltaTime;
          }
          else
          {
            spellObject.state = SpellStates.Ready;
          }
          break;
      }

    }
  }
}
