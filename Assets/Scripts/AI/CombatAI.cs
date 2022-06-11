using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AI
{
  public class CombatAI : EntityAI
  {
    public int Damage = 1;
    public float attackRange = 10f;
    public float attackSpeed = 1f;

    private float lastAttackTime;

    public override void Start()
    {
      base.Start();
      lastAttackTime = Time.time;
    }

    public override void Update()
    {
      base.Update();

      switch (state)
      {
        case State.Chase: Chase(); break;
        case State.Attack: Attack(); break;
      }
    }

    private void Chase()
    {
      var distance = Vector3.Distance(rig.worldCenterOfMass, target.transform.position);
      if (distance < attackRange)
      {
        state = State.Attack;
        return;
      }

      WalkTo(target.transform.position);
      //if (distance < findRange)
      //{
      //  WalkTo(target.transform.position);
      //  return;
      //}

      //if (distance >= findRange)
      //else
      //{
      //  target = null;
      //  state = State.Roam;
      //  return;
      //}
    }

    private void Attack()
    {
      //I've have someone to attack
      if (target != null)
      {
        var hits = Physics.OverlapSphere(rig.worldCenterOfMass, attackRange, whatIsEnemy);
        var hit = hits.FirstOrDefault(e => e.transform.parent.gameObject.GetInstanceID() == target.gameObject.GetInstanceID());

        //enemy in range, attack!
        if (hit != null && Time.time > attackSpeed + lastAttackTime)
        {
          target.TakeDamage(Damage);
          lastAttackTime = Time.time;

          if (target.life <= 0)
          {
            target = null;
            state = State.Roam;
          }

        }

        //Too far, chase him!
        else
        {
          state = State.Chase;
        }
      }

      //No one to attack, back to roaming.
      else
      {
        state = State.Roam;
      }
    }
  }
}
