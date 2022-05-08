using UnityEngine;

namespace Assets.Scripts.AI
{
  public class Villager : Entity
  {
    public bool showRoam;
    public float roamDistance = 3f;
    public float speed = 1f;
    public State state;

    private Rigidbody rig;
    private Vector3 roamingPosition;
    private Vector3 startingPosition;
    private GameObject corpse;

    public override void Start()
    {
      base.Start();
      corpse = Resources.Load<GameObject>("Prefabs/Corpse");
      rig = GetComponent<Rigidbody>();
      startingPosition = rig.worldCenterOfMass.ZeroY();
      roamingPosition = GetRoamingPosition();
      state = State.Roam;
    }

    public override void Update()
    {
      base.Update();

      switch (state)
      {
        case State.Roam: Roam(); break;
      }
    }

    public void Roam() 
    {
      if (roamDistance > 0)
      {
        if (Vector3.Distance(rig.worldCenterOfMass.ZeroY(), roamingPosition) < 0.1f) roamingPosition = GetRoamingPosition();

        WalkTo(roamingPosition);
      }
    }

    private Vector3 GetRoamingPosition()
    {
      return startingPosition + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * roamDistance;
    }

    private void WalkTo(Vector3 destination)
    {
      var heading = destination - rig.worldCenterOfMass;
      var direction = heading / heading.magnitude;

      rig.MovePosition(rig.position + direction * speed * Time.deltaTime);
    }

    public override void Death()
    {
      base.Death();

      var rand = Random.Range(0, 360f);
      Instantiate(corpse, rig.worldCenterOfMass.ZeroY(0.01f), Quaternion.Euler(new Vector3(90, 0, rand)));
    }

    public void OnDrawGizmos()
    {
      if (showRoam)
      {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(startingPosition.ZeroY(), roamDistance);
      }
    }
  }
}
