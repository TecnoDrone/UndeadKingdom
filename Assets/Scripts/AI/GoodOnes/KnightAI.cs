using Assets.Scripts.AI.GoodOnes;
using Assets.Scripts.Managers;

public class KnightAI : GoodOne
{
  public KnightAI()
  {
    attackRange = 1f;
    viewDistance = 3f;
    attackSpeed = 1f;
    Kind = CreatureKind.Knight;
  }
}
