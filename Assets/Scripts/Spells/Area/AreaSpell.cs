using UnityEngine;

namespace Assets.Scripts.Spells
{
  [CreateAssetMenu]
  public class AreaSpell : SpellObject
  {
    protected override void Init(GameObject parent)
    {
      //TODO figure out how to pass this vector3 from the above
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
      {
        castPosition = hit.point;
      }
    }
  }
}
