using UnityEngine;

namespace Assets.Scripts.AI.GoodOnes
{
  public class KingV2 : GoodOne
  {
    private Animator animator;
    public GameObject threat;
    private Vector3? fleePosition;

    public override void Start()
    {
      base.Start();
      animator = GetComponentInChildren<Animator>();
      onStateChange += CheckState;
    }

    public override void Update()
    {
      base.Update();

      animator?.SetFloat("Horizontal", agent.velocity.x);
      animator?.SetFloat("Vertical", agent.velocity.y);

      if (state == State.Flee)
      {
        Flee();
      }
    }

    public override void SetTarget(Entity target)
    {
      threat = target.gameObject;
      state = State.Flee;
    }

    void Flee()
    {
      if (LookForThreat(out var threat) == false)
      {
        ResetToDefaultState();
        return;
      }

      //Has reached flee position
      if (fleePosition == null)
      {
        fleePosition = GetOppositePosition(threat.transform.position, viewDistance);
        return;
      }

      //Flee!
      var distance = Vector3.Distance(transform.position.ZeroY(), fleePosition.Value);
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

    bool LookForThreat(out Entity threat)
    {
      threat = null;

      int RaysToShoot = 16;

      float angle = 0;
      for (int i = 0; i < RaysToShoot; i++)
      {
        float x = Mathf.Sin(angle);
        float z = Mathf.Cos(angle);
        angle += 2 * Mathf.PI / RaysToShoot;

        Vector3 direction = new(x * viewDistance, 0, z * viewDistance);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, viewDistance, whatIsEnemy | whatCanBeSeen))
        {
          if (1 << hitInfo.collider.gameObject.layer == whatCanBeSeen)
          {
            continue;
          }

          threat = hitInfo.collider.GetComponentInParent<Entity>();
          return true;
        }
      }

      return false;
    }

    public void CheckState()
    {
      if(state == State.Roam)
      {
        target = null;
      }
    }

    Vector3 GetOppositePosition(Vector3 position, float distance)
    {
      var center = transform.position;

      var vx = center.x - position.x;
      var vz = center.z - position.z;
      var length = Mathf.Sqrt(vx * vx + vz * vz);
      var opposite = new Vector3
      {
        x = vx / length * distance + center.x,
        z = vz / length * distance + center.z
      };

      //Check if the destination is reachable
      var heading = opposite - center;
      var direction = (heading / heading.magnitude);

      Debug.DrawRay(center, direction, Color.red, 1);

      if (Physics.Raycast(center, direction, out RaycastHit hitInfo, distance, whatCanBeSeen))
      {
        opposite = hitInfo.point;
      }

      return opposite;
    }
  }
}
