using Assets.Scripts.SpellCasting;
using UnityEngine;

namespace Assets.Scripts.Player.Casting
{
  public class PlayerReanimationCasting : ReanimationCasting
  {
    public KeyCode key;

    void Update()
    {
      if (Input.GetKeyUp(key))
      {
        if (State == SpellCastingState.Ready) TryCast();
      }
    }
  }
}
