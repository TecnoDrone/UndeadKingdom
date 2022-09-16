using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.AI
{
  public class FleeAI : EntityAI
  {
    private Animator animator;

    public Entity currentThreat;

    public Vector3? fleePosition;

    public override void Start()
    {
      base.Start();

      animator = GetComponentInChildren<Animator>();
      agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public override void Update()
    {
      base.Update();

      switch (state)
      {
        case State.Flee: Flee(); break;
      }

      //Move to entityAI?
      var horizontal = agent.velocity.x;
      var vertical = agent.velocity.z;

      if (animator != null)
      {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
      }
    }

    protected override void Roam()
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
        base.Roam();
      }
    }

    private void Flee()
    {
      //If there is no enemy, reset to default state
      if (!LookForThreat(out currentThreat))
      {
        ResetToDefaultState();
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

    public override void ResetToDefaultState()
    {
      base.ResetToDefaultState();
      fleePosition = null;
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

    private Vector3 GetOppositePosition(Vector3 position, float distance)
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

      if (Physics.Raycast(center, direction, out RaycastHit hitInfo, distance, whatBlocksVision))
      {
        opposite = hitInfo.point;
      }

      return opposite;
    }
  }
}
