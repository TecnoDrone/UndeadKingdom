namespace Assets.Scripts.AI.GoodOnes
{
  public class GoodOne : CombatAI
  {
    public override void Start()
    {
      base.Start();
      team = Team.GoodOnes;
    }
  }
}
