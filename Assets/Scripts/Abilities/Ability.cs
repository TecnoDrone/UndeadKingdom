using UnityEngine;

namespace Assets.Scripts.Abilities
{
  public abstract class Ability : MonoBehaviour
  {
    public KeyCode key;
    public Player.PlayerStance state;

    public virtual void Update()
    {
      if (PlayerEntity.Instance.Stance != state) return;

      if (Input.GetKeyDown(key)) OnKeyDown();

      if (Input.GetKey(key)) OnKey();

      if (Input.GetKeyUp(key)) OnKeyUp();
    }

    public abstract void OnKeyUp();

    public abstract void OnKeyDown();

    public abstract void OnKey();
  }
}
