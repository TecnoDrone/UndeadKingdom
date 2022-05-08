namespace Assets.Scripts.AI
{
  public class Knight : FighterAI
  {
    public Knight()
    {
      findRange = 50f;
      attackRange = 1f;
      roamDistance = 3f;
      attackSpeed = 1f;
    }

    public override void OnDeath()
    {
      
    }
  }
}
