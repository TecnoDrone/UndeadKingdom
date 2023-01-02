using Assets.Scripts.SpellCasting;
using UnityEngine;

namespace Assets.Scripts.Player.Casting
{
  public class PlayerDarkHailCasting : DarkHailCasting
  {
    public PlayerStance stance;
    public KeyCode key;

    private void Start()
    {
      PlayerEntity.onPlayerStanceChange += CheckStance;
    }

    void Update()
    {
      if (PlayerEntity.Instance.Stance != stance) return;

      if (Input.GetKeyDown(key))
      {
        if (State != SpellCastingState.Ready) return;
        if (PlayerEntity.Instance.life <= energyCost) return;

        if (TryCast())
        {
          PlayerEntity.Instance.State = PlayerState.Casting;
          PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Casting);
          PlayerEntity.Instance.ConsumeLife(energyCost);
        }
      }

      if (Input.GetKeyUp(key))
      {
        if (State != SpellCastingState.Channeling) return;

        Shoot();
        PlayerEntity.Instance.State = PlayerState.Idle;
        PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Idle);
      }
    }

    void CheckStance()
    {
      if (PlayerEntity.Instance.Stance != stance)
      {
        Stop();
        PlayerEntity.Instance.State = PlayerState.Idle;
        PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Idle);
      }
    }
  }
}
