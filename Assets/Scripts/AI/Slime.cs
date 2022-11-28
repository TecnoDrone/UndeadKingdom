using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.AI
{
  public class Slime : Entity
  {
    public bool showRoam = false;
    public bool showFind = false;

    public AIstate state;
    public float roamDistance = 3f;
    public float speed = 1f;
    public float attackRange = 10f;
    public float findRange = 50f;
    public float waitBetweenJumps = 1f;
    public LayerMask ground;
    public GameObject target;

    private Vector3 roamingPosition;
    private Vector3 startingPosition;
    private Rigidbody rig;
    private ParticleSystem particleSystem;
    public bool isJumping;
    public bool onGround = true;

    private void Awake()
    {
      rig = GetComponent<Rigidbody>();
      particleSystem = GetComponent<ParticleSystem>();
    }

    public override void Start()
    {
      base.Start();
      state = AIstate.Roam;
      startingPosition = transform.position.ZeroY();
      roamingPosition = GetRoamingPosition();
    }

    public override void Update()
    {
      base.Update();
      switch (state)
      {
        case AIstate.Roam: Roam(roamingPosition); break;
        case AIstate.Chase: Chase(target); break;
        case AIstate.Attack: Attack(target); break;
      }
    }

    private void Roam(Vector3 position)
    {
      if (Vector3.Distance(transform.position.ZeroY(), roamingPosition) < 0.1f)
      {
        roamingPosition = GetRoamingPosition();
      }

      StartCoroutine(HopTo(position));

      if (target == null) FindTarget();
    }

    private void Chase(GameObject toChase)
    {
      var distance = Vector3.Distance(transform.position, toChase.transform.position);

      if (distance < attackRange)
      {
        state = AIstate.Attack;
      }
      else if (distance >= findRange)
      {
        target = null;
        state = AIstate.Roam;
      }
      else
      {
        StartCoroutine(HopTo(toChase.transform.position));
      }
    }

    private void Attack(GameObject target)
    {
      //Debug.Log("Attacking");

      if (Vector3.Distance(transform.position, target.transform.position) > attackRange)
      {
        state = AIstate.Roam;
      }
    }

    private void FindTarget()
    {
      if (Vector3.Distance(transform.position, PlayerEntity.Instance.transform.position) <= findRange)
      {
        target = PlayerEntity.Instance.gameObject;
        state = AIstate.Chase;
      }
    }

    private Vector3 GetRoamingPosition()
    {
      return startingPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * roamDistance;
    }

    private IEnumerator HopTo(Vector3 destination)
    {
      if (!isJumping && speed > 0)
      {
        var heading = destination - transform.position;
        var direction = heading / heading.magnitude;
        direction.y = 5f / speed;

        rig.velocity = direction * speed;
        isJumping = true;
        
      }

      if (rig.velocity == Vector3.zero) 
      {
        isJumping = false;
        var temp = Random.Range(1, 5f);

        yield return new WaitForSeconds(temp);
      }
    }

    public override void TakeDamage(int dmg) 
    {
      particleSystem.Play();
      base.TakeDamage(dmg);
    }

    public void OnDrawGizmos()
    {
      if (showRoam)
      {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(startingPosition.ZeroY(), roamDistance);
      }

      if (showFind)
      {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position.ZeroY(), findRange);
      }

      //Gizmos.DrawSphere(roamingPosition, 0.1f);
      //
      //Gizmos.color = Color.red;
      //Gizmos.DrawWireSphere(transform.position.ZeroY(), attackRange);
    }
  }

  public enum AIstate
  {
    Roam,
    Chase,
    Attack
  }
}
