using Assets.Scripts.AI;
using Assets.Scripts.Managers;

public class KnightAI : CombatAI
{
  public KnightAI()
  {
    attackRange = 1f;
    viewDistance = 3f;
    attackSpeed = 1f;
    Kind = CreatureKind.Knight;
    order = 1;
  }
}
