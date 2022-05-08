using Assets.Scripts;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class King : Entity
{
  public LayerMask whatIsEnemy;
  public LayerMask whatBlocksVision;

  public float viewDistance = 3f;
  public float roamDistance = 0f;

  private Animator animator;
  private NavMeshAgent agent;

  public Entity currentThreat;
  public State state;

  public Vector3? roamingPosition;
  public Vector3? fleePosition;
  private Vector3 anchorPosition;

  //Used to prevent SetPosition multiple times
  private bool isWalking;

  public override void Start()
  {
    base.Start();

    animator = GetComponentInChildren<Animator>();
    agent = GetComponent<NavMeshAgent>();

    anchorPosition = transform.position;
    state = State.Roam;
  }

  public override void Update()
  {
    base.Update();

    switch (state)
    {
      case State.Roam: Roam(); break;
      case State.Flee: Flee(); break;
    }

    var horizontal = agent.velocity.x;
    var vertical = agent.velocity.z;
    animator.SetFloat("Horizontal", horizontal);
    animator.SetFloat("Vertical", vertical);
  }

  private void Roam()
  {
    //Threat is found, flee
    if (LookForThreat(out var threat))
    {
      state = State.Flee;
      currentThreat = threat;
    }

    //Keep roaming
    else
    {
      if (roamingPosition != null)
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
  }

  private void Flee()
  {
    //If there is no enemy, reset to default state
    if (!LookForThreat(out currentThreat))
    {
      ResetToDefault();
    }
    else
    {
      //Has a flee position to reach
      if (fleePosition != null)
      {
        var distance = Vector3.Distance(transform.position.ZeroY(), fleePosition.Value);

        //Reached destination
        if (distance <= 0.55f)
        {
          fleePosition = null;
          isWalking = false;
        }
        else
        {
          if (!isWalking)
          {
            WalkTo(fleePosition);
            isWalking = true;
          }
        }
      }
      else
      {
        fleePosition = GetOppositePosition(currentThreat.transform.position, viewDistance);//GetNextPosition(viewDistance);
      }
    }
  }

  private bool LookForThreat(out Entity threat)
  {
    threat = null;

    int RaysToShoot = 16;

    float angle = 0;
    for (int i = 0; i < RaysToShoot; i++)
    {
      float x = Mathf.Sin(angle);
      float z = Mathf.Cos(angle);
      angle += 2 * Mathf.PI / RaysToShoot;

      Vector3 direction = new Vector3(x * viewDistance, 0, z * viewDistance);

      //mby check all direction and return closest?
      //mby check all direction, but only every second? to make it come realistic and less resource intensive
      if (Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, viewDistance, whatIsEnemy | whatBlocksVision))
      {
        if (1 << hitInfo.collider.gameObject.layer == whatBlocksVision)
        { 
          continue;
        }

        threat = hitInfo.collider.GetComponentInParent<Entity>();
        return true;
      }
    }

    return false;

    //v1.0
    //var hits = Physics.OverlapSphere(transform.position, viewDistance, whatIsEnemy).ToList();
    //if (hits != null && hits.Any())
    //{
    //  threat = hits.First().GetComponentInParent<Entity>();
    //  return true;
    //}
    //else
    //{
    //  return false;
    //}
  }

  //Reset to default state
  private void ResetToDefault()
  {
    agent.SetDestination(transform.position);
    anchorPosition = transform.position;
    isWalking = false;
    fleePosition = null;
    state = State.Roam;
  }

  private void WalkTo(Vector3? destination)
  {
    if (destination == null) return;

    agent.SetDestination(destination.Value);

    animator.SetFloat("Horizontal", agent.velocity.x);
    animator.SetFloat("Vertical", agent.velocity.y);
  }

  private Vector3 GetOppositePosition(Vector3 position, float distance)
  {
    var center = transform.position;

    var vx = center.x - position.x;
    var vz = center.z - position.z;
    var length = Mathf.Sqrt(vx * vx + vz * vz);
    var opposite = new Vector3 {
      x = vx / length * distance + center.x,
      z = vz / length * distance + center.z
    };

    //Check if the destination is reachable
    var heading = opposite - center;
    var direction = (heading / heading.magnitude);

    Debug.DrawRay(center, direction, Color.red, 1);

    if (Physics.Raycast(center, direction, out RaycastHit hitInfo, distance, whatBlocksVision))
    {
      opposite = hitInfo.point;
    }

    return opposite;
  }

  private Vector3? GetNextPosition(float distance)
  {
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

  public enum State
  {
    Roam,
    Flee
  }

  public void OnDrawGizmos()
  {
    Gizmos.color = Color.green;

    if (fleePosition != null)
    {
      Gizmos.DrawSphere(fleePosition.Value, 0.1f);
    }
  }
}
