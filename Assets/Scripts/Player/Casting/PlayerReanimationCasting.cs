using Assets.Scripts.SpellCasting;
using UnityEngine;

namespace Assets.Scripts.Player.Casting
{
  public class PlayerReanimationCasting : ReanimationCasting
  {
    public PlayerStance stance;
    public KeyCode key;

    void Update()
    {
      if (PlayerEntity.Instance.Stance != stance) return;

      if (Input.GetKeyUp(key))
      {
        if (State == SpellCastingState.Ready)
        {
          if(TryCast())
          {
            PlayerEntity.Instance.State = PlayerState.Casting;
            PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Casting);
          }
        }
      }
    }
  }
}
