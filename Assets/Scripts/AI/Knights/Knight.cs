using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.AI
{
  public class Knight : CombatAI
  {
    public Sprite[] deathSprites;

    public Knight()
    {
      attackRange = 1f;
      viewDistance = 3f;
      attackSpeed = 1f;
      Kind = CreatureKind.Knight;
    }
  }
}
