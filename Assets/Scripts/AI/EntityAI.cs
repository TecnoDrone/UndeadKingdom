using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AI
{
  public abstract class EntityAI : Entity
  {
    public LayerMask whatBlocksVision;
    public LayerMask whatIsEnemy;
    public State state;

    public float viewDistance = 3f;
    public float roamDistance = 3f;
    public Vector3? roamingPosition;

    private Vector3 anchorPosition;

    [HideInInspector]
    public Entity target;

    //Move to parent class?
    [HideInInspector]
    public Sprite sprite;

    protected Rigidbody rig;
    protected NavMeshAgent agent;

    //Used to prevent SetPosition multiple times
    protected bool isWalking;

    public override void Start()
    {
      base.Start();

      agent = GetComponent<NavMeshAgent>();
      rig = GetComponent<Rigidbody>();
      spriteRenderer = GetComponentInChildren<SpriteRenderer>();
      if (spriteRenderer != null) sprite = spriteRenderer.sprite;

      state = State.Roam;
      anchorPosition = transform.position;
      roamingPosition = GetNextPosition(viewDistance);
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
      }
    }

    protected virtual void Roam()
    {
      //if (roamingPosition != null)
      //{
      //  if (Vector3.Distance(rig.worldCenterOfMass.ZeroY(), roamingPosition.Value) < 0.5f)
      //  {
      //    roamingPosition = GetRoamingPosition();
      //  }

      //  WalkTo(roamingPosition);
      //}

      //FindTarget();

      if (roamDistance > 0 && roamingPosition != null)
      {
        var distance = Vector3.Distance(transform.position.ZeroY(), roamingPosition.Value);

        //Reached destination
        if (distance <= 0.55f)
        {
          roamingPosition = null;
          isWalking = false;
        }
        else
        {
          if (!isWalking)
          {
            WalkTo(roamingPosition);
            isWalking = true;
          }
        }
      }
      else
      {
        roamingPosition = GetNextPosition(roamDistance);
      }
    }

    public void WalkTo(Vector3? destination)
    {
      if (destination == null) return;
      agent.SetDestination(destination.Value);
    }

    private Vector3? GetNextPosition(float distance)
    {
      //if (viewDistance == 0) return null;

      //return startingPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * viewDistance;
      if (distance == 0) return null;

      var center = transform.position;

      var nextPosition = anchorPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * distance;
      var heading = center - nextPosition;
      var direction = (heading / heading.magnitude) * distance;

      if (Physics.Raycast(center, direction, out RaycastHit hitInfo, viewDistance, whatBlocksVision))
      {
        nextPosition = hitInfo.point;
      }

      return nextPosition;
    }

    private bool FindTarget()
    {
      if (target == null)
      {
        var hits = Physics.OverlapSphere(transform.position, viewDistance, whatIsEnemy).ToList();
        if (hits != null && hits.Count > 0)
        {
          var distances = hits.Select(hit => (hit, distance: Vector3.Distance(hit.transform.position, transform.position))).OrderBy(x => x.distance);
          var entity = distances.First().hit.GetComponentInParent<Entity>();
          SetTarget(entity);
          return true;
        }
      }

      return false;
    }

    //Reset to default state
    protected virtual void ResetToDefaultState()
    {
      agent.SetDestination(transform.position);
      anchorPosition = transform.position;
      isWalking = false;
      state = State.Roam;
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
      Attack,
      Flee
    }

    public void OnDrawGizmos()
    {
      Gizmos.color = Color.green;

      if (roamingPosition != null)
      {
        Gizmos.DrawSphere(roamingPosition.Value, 0.1f);
      }
    }

    //public void OnDrawGizmos()
    //{
    //  if (roamingPosition.HasValue)
    //  {
    //    Gizmos.DrawSphere(roamingPosition.Value, 0.1f);
    //  }
    //}
  }
}
