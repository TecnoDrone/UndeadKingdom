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
      this.onStateChange += ReactToStatusChange;
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

    public override void SetTarget(Entity target)
    {
      base.SetTarget(target);
      state = State.Chase;
      onStateChange?.Invoke();
    }

    private void Chase()
    {
      //if (log) Debug.Log("Chasing...");
      var distance = Vector3.Distance(rig.worldCenterOfMass, target.transform.position);
      if (distance >= viewDistance)
      {
        target = null;
        state = State.Roam;
        onStateChange?.Invoke();
        return;
      }

      //if (log) Debug.Log("Target in view.");
      var heading = target.transform.position - rig.worldCenterOfMass;
      var direction = (heading / heading.magnitude) * viewDistance;

      //Look in attach range
      var hit = Physics.Raycast(rig.worldCenterOfMass, direction, out var hitInfo, attackRange, whatCanBeSeen);
      if (hit && log) Debug.Log("I'm seeing: " + hitInfo.collider.transform.parent?.gameObject?.name ?? hitInfo.collider.gameObject.name);

      //If something is in front of the entity
      if (hit && hitInfo.collider.transform?.parent != null)
      {
        var seenTarget = hitInfo.collider.transform.parent.gameObject;

        if(whatIsEnemy == (whatIsEnemy | (1 << seenTarget.layer)))
        {
          if (ReferenceEquals(seenTarget, target.gameObject))
          {
            if (log) Debug.Log("Fighting: " + target.name);
            state = State.Fight;
            onStateChange?.Invoke();
            return;
          }

          //It's not my target, but it's blocking the way.
          //So I decide to switch target  to this one >:}
          else
          {
            if (log) Debug.Log("Changing target: " + seenTarget.name);
            base.SetTarget(seenTarget.GetComponent<EntityAI>());
            state = State.Fight;
            onStateChange?.Invoke();
            return;
          }
        }
      }

      //Keep walking until target is reached or is lost.
      WalkTo(target.transform.position);
    }

    private void ReactToStatusChange()
    {
      switch (state)
      {
        case State.Roam:
          target = null;
          break;
        case State.Fight:
          agent.ResetPath();
          break;
        case State.Chase:
          break;
      }
    }

    private void Fight()
    {
      if (target == null)
      {
        state = State.Roam;
        onStateChange?.Invoke();
        return;
      }

      //stop moving to attack
      agent.ResetPath();

      //TODO: make so archers keep target at max range.
      //this way you can remove agent.ResetPath()

      //Check if target is still in attack range
      var heading = target.transform.position - rig.worldCenterOfMass;
      var direction = (heading / heading.magnitude) * viewDistance;

      var hit = Physics.Raycast(rig.worldCenterOfMass, direction, out var hitInfo, attackRange, whatCanBeSeen);

      //No hit means the target is too far away to be attacked
      if (!hit || hitInfo.collider.transform.parent == null)
      {
        //if (log) Debug.Log("Stop Fight and Roam. hit:" + hit + " hitInfo.collider.transform?.parent: " + hitInfo.collider.gameObject.name ?? "null");
        state = State.Roam;
        onStateChange?.Invoke();
        return;
      }

      var isVisible = ReferenceEquals(hitInfo.collider.transform.parent.gameObject, target.gameObject);//hitInfo.collider.gameObject.GetInstanceID() == target.gameObject.GetInstanceID();

      //If not, chase him.
      if (!isVisible)
      {
        state = State.Chase;
        onStateChange?.Invoke();
        return;
      }

      //enemy in range, attack!
      if (Time.time > attackSpeed + lastAttackTime)
      {
        Attack();
        lastAttackTime = Time.time;

        if (target == null || target.life <= 0)
        {
          state = State.Roam;
          onStateChange?.Invoke();
          return;
        }
      }
    }

    public virtual void Attack()
    {
      target.TakeDamage(Damage);
    }
  }
}
