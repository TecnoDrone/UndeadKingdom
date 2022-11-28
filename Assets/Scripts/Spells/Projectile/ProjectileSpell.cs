using UnityEngine;

namespace Assets.Scripts.Spells
{
  [CreateAssetMenu]
  public class ProjectileSpell : SpellObject
  {
    protected override void Init(GameObject parent)
    {
      //var projectile = spellObject.GetComponent<Projectile>();

      castPosition = new Vector3(parent.transform.position.x, parent.transform.position.y + 0.2f, parent.transform.position.z);

      //TODO figure out how to pass this vector3 from the above
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
      {
        //projectile.destination = hit.point;

        var direction = hit.point - castPosition;
        castRotation = Quaternion.LookRotation(-parent.transform.up, direction);
      }
    }
  }
}