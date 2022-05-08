using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AI
{
  public class FighterAI : Entity
  {
    public float findRange = 50f;
    public int Damage = 1;
    public float attackRange = 10f;
    public float attackSpeed = 1f;

    public LayerMask whatIsEnemy;
    public State state;
    private float lastAttackTime;

    public float roamDistance = 3f;
    public Vector3? roamingPosition;

    private Vector3 startingPosition;

    [HideInInspector]
    public Entity target;

    //Move to parent class?
    [HideInInspector]
    public Sprite sprite;

    private Rigidbody rig;
    private NavMeshAgent agent;

    public override void Start()
    {
      base.Start();
      lastAttackTime = Time.time;

      rig = GetComponent<Rigidbody>();
      spriteRenderer = GetComponentInChildren<SpriteRenderer>();
      if (spriteRenderer != null)
      {
        sprite = spriteRenderer.sprite;
      }
      agent = GetComponent<NavMeshAgent>();

      state = State.Roam;
      startingPosition = rig.worldCenterOfMass.ZeroY();
      roamingPosition = GetRoamingPosition();
    }

    // Update is called once per frame
    public override void Update()
    {
      base.Update();

      if (state != State.Roam && target == null)
      {
        state = State.Roam;
      }

      switch (state)
      {
        case State.Roam: Roam(); break;
        case State.Chase: Chase(); break;
        case State.Attack: Attack(); break;
      }
    }

    private void Roam()
    {
      if (roamingPosition != null)
      {
        if (Vector3.Distance(rig.worldCenterOfMass.ZeroY(), roamingPosition.Value) < 0.1f)
        {
          roamingPosition = GetRoamingPosition();
        }

        WalkTo(roamingPosition);
      }

      FindTarget();
    }

    public void WalkTo(Vector3? destination)
    {
      if (destination == null) return;

      //var heading = destination.Value - rig.worldCenterOfMass;
      //var direction = heading / heading.magnitude;
      //
      //rig.MovePosition(rig.position + direction * speed * Time.deltaTime);

      agent.SetDestination(destination.Value);
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

    private Vector3? GetRoamingPosition()
    {
      if (roamDistance == 0) return null;

      return startingPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * roamDistance;
    }

    private bool FindTarget()
    {
      if (target == null)
      {
        var hits = Physics.OverlapSphere(transform.position, findRange, whatIsEnemy).ToList();
        if (hits != null && hits.Count > 0)
        {
          var entity = hits.First().GetComponentInParent<Entity>();
          SetTarget(entity);
          return true;
        }
      }

      return false;
    }

    public void SetTarget(Entity target)
    {
      this.target = target;
      GameManager.FightersTargets[this] = target;
      state = State.Chase;
    }

    public enum State
    {
      Roam,
      Chase,
      Attack
    }
  }
}
