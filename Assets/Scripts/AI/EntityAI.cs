using Assets.Scripts.Managers;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.AI
{
  public abstract class EntityAI : Entity
  {
    public bool log;

    public LayerMask whatCanBeSeen;
    public LayerMask whatIsTarget;
    public State state;

    public CreatureKind Kind;

    public float viewDistance = 3f;
    public float roamDistance = 0f;

    public Vector3? roamingPosition;
    private Vector3 anchorPosition;

    //[HideInInspector]
    public Entity target;

    //Move to parent class?
    [HideInInspector]
    public Sprite sprite;

    protected NavMeshAgent agent;

    //Used to prevent SetPosition multiple times
    public bool isWalking;

    //Starting speed, taken from NavMeshAgent
    [HideInInspector]
    public float originalSpeed;

    public delegate void OnStateChange();
    public OnStateChange onStateChange;

    public SearchStrategy searchStrategy;

    public override void Start()
    {
      base.Start();

      agent = GetComponent<NavMeshAgent>();
      spriteRenderer = GetComponentInChildren<SpriteRenderer>();
      if (spriteRenderer != null) sprite = spriteRenderer.sprite;

      state = State.Roam;
      anchorPosition = transform.position;
      roamingPosition = GetNextPosition(viewDistance);
      originalSpeed = agent.speed;

      searchStrategy = new SearchStrategy(whatIsTarget, viewDistance);
    }

    // Update is called once per frame
    public override void Update()
    {
      base.Update();
      if (state == State.Dying) return;

      if (state != State.Roam && target == null)
      {
        state = State.Roam;
      }

      switch (state)
      {
        case State.Roam: Roam(); break;
      }
    }

    public override void OnDeath(Entity entity)
    {
      var corspe = CorpseManager.GetRandomCorspe(team, Kind);
      Instantiate(corspe, transform.position, corspe.transform.rotation);
      Destroy(gameObject);
    }

    protected virtual void Roam()
    {
      //Try find for a target. If found stop roaming.
      if (FindTarget()) return;

      //if (roamDistance > 0 && roamingPosition != null)
      //{
      //  var distance = Vector3.Distance(transform.position.ZeroY(), roamingPosition.Value);
      //
      //  //Reached destination
      //  if (distance <= 0.55f)
      //  {
      //    roamingPosition = null;
      //    isWalking = false;
      //  }
      //  else
      //  {
      //    if (!isWalking)
      //    {
      //      WalkTo(roamingPosition);
      //      isWalking = true;
      //    }
      //  }
      //}
      //else
      //{
      //  roamingPosition = GetNextPosition(roamDistance);
      //}
    }

    public void WalkTo(Vector3? destination, float? movSpeed = null)
    {
      //if (log) Debug.Log("Walking to:" + destination);
      if (destination == null) return;

      agent.speed = movSpeed ?? originalSpeed;

      agent.SetDestination(destination.Value);
    }

    private Vector3? GetNextPosition(float distance)
    {
      //if (viewDistance == 0) return null;

      //return startingPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * viewDistance;
      if (distance == 0) return null;

      var nextPosition = anchorPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * distance;
      var heading = center - nextPosition;
      var direction = (heading / heading.magnitude) * distance;

      if (Physics.Raycast(center, direction, out RaycastHit hitInfo, viewDistance, whatCanBeSeen))
      {
        nextPosition = hitInfo.point;
      }

      return nextPosition;
    }

    private bool FindTarget()
    {
      if (target != null) return false;

      var center = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
      var entity = searchStrategy.SearchTarget(center);
      if (entity == null) return false;

      SetTarget(entity);
      return true;

      //var center = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

      //var possibleTargets = Physics.OverlapSphere(center, viewDistance, whatIsTarget);
      //if (possibleTargets == null || possibleTargets.Length == 0) return false;

      ////Check if targets in range are hidden by walls
      //var visibleTargets = new List<Collider>();
      //foreach (var possibleTarget in possibleTargets)
      //{
      //  if (!possibleTarget.transform.parent.TryGetComponent<Entity>(out var e)) continue;

      //  var heading = possibleTarget.transform.position - center;
      //  var direction = (heading / heading.magnitude) * viewDistance;

      //  var hit = Physics.Raycast(center, direction, out var hitInfo, viewDistance, whatCanBeSeen);
      //  if (!hit) continue;

      //  var isVisible = hitInfo.collider.gameObject.GetInstanceID() == possibleTarget.gameObject.GetInstanceID();
      //  if (isVisible) visibleTargets.Add(possibleTarget);
      //}
      //if (visibleTargets == null || visibleTargets.Count == 0) return false;

      //var distances = visibleTargets.Select(hit => (hit, distance: Vector3.Distance(hit.transform.position, transform.position))).OrderBy(x => x.distance);
      //var entity = distances.First().hit.GetComponentInParent<Entity>();
      //SetTarget(entity);
      //return true;
    }

    //Reset to default state. TODO: remove and use onStateChange?.Invoke();
    public virtual void ResetToDefaultState()
    {
      if (agent != null)
      {
        agent.ResetPath();
        agent.isStopped = false;
        agent.speed = originalSpeed;
      }

      if (transform != null)
      {
        anchorPosition = transform.position;
      }

      isWalking = false;
      state = State.Roam;
    }

    public virtual void SetTarget(Entity target)
    {
      if (target == null) return;
      target.onEntityDied += ClearTarget;
      this.target = target;
    }

    public void ClearTarget(Entity entity)
    {
      target = null;
      if (agent != null)
      {
        agent.isStopped = false;
        agent.ResetPath();
      }
    }

    public enum State
    {
      Roam,
      Chase,
      Fight,
      Flee,
      Dying
    }

    public void OnDrawGizmos()
    {
      Gizmos.color = Color.green;

      if (roamingPosition != null)
      {
        Gizmos.DrawSphere(roamingPosition.Value, 0.1f);
      }
    }
  }
}
