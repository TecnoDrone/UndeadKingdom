using Assets.Scripts.Managers;

namespace Assets.Scripts.AI.Undead.T2minions
{
  public class Cultist : Undead
  {
    public override void Start()
    {
      base.Start();
      Kind = CreatureKind.Cultist;
      order = 1;

      searchStrategy.rules.Add(new SearchRuleNotFullHealth());


    }

    public override void SetTarget(Entity target)
    {
      base.SetTarget(target);
      target.onLifeChanged += CheckTargetHealth;
    }

    public override void Attack()
    {
      target.Heal(Damage);
    }

    public void CheckTargetHealth()
    {
      if (target == null) return;
      if (target.life == target.maxLife) target = null;
    }
  }
}
