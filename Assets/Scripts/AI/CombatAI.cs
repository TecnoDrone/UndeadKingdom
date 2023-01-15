using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AI
{
  public class CombatAI : EntityAI
  {
    public int Damage = 1;
    public float attackRange = 10f;
    public float attackSpeed = 1f;

    public int order = 0;

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
        case State.Fight: Fight(); break;
      }
    }

    private void Chase()
    {
      var distance = Vector3.Distance(rig.worldCenterOfMass, target.transform.position);
      if (distance < attackRange)
      {
        state = State.Fight;
        return;
      }

      //WalkTo(target.transform.position);
      if (distance < viewDistance)
      {
        WalkTo(target.transform.position);
        return;
      }

      if (distance >= viewDistance)
      {
        target = null;
        state = State.Roam;
        return;
      }
    }

    private void Fight()
    {
      //I've have someone to attack
      if (target != null)
      {
        //stop to attack
        agent.isStopped = true;

        //Check if target is still in attack range
        var hits = Physics.OverlapSphere(rig.worldCenterOfMass, attackRange, whatIsEnemy);
        var hit = hits.FirstOrDefault(e => e.transform.parent.gameObject.GetInstanceID() == target.gameObject.GetInstanceID());

        //If not, chase him.
        if (hit == null)
        {
          state = State.Chase;
          agent.isStopped = false;
        }
        else
        {
          //enemy in range, attack!
          if (Time.time > attackSpeed + lastAttackTime)
          {
            Attack();

            if (target.life <= 0)
            {
              ResetToDefaultState();
            }

            lastAttackTime = Time.time;
          }
        }
      }

      //No one to attack, back to roaming.
      else
      {
        state = State.Roam;
        agent.isStopped = false;
      }
    }

    public virtual void Attack()
    {
      target.TakeDamage(Damage);
    }
  }
}
