using Assets.Scripts.Managers;
using Assets.Scripts.Projectiles;
using UnityEngine;

namespace Assets.Scripts.AI
{
  public class ArcherAI : CombatAI
  {
    public GameObject Arrow;
    private Vector3 shootingPosition;

    public ArcherAI()
    {
      Damage = 1;
      attackRange = 5;
      viewDistance = 10;
      attackSpeed = 1f;
      Kind = CreatureKind.Archer;
      order = 2;
    }

    public override void Attack()
    {
      shootingPosition = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);

      var direction = target.transform.position - shootingPosition;
      Instantiate(Arrow, shootingPosition, Quaternion.LookRotation(-transform.up, direction))
        .GetComponent<Projectile>()
        .SetSquad(squad);
    }
  }
}
