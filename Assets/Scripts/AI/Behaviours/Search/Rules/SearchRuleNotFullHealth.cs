using System.Collections.Generic;

public class SearchRuleNotFullHealth : SearchRule
{
  public override List<Entity> Apply(List<Entity> visibleEntities)
  {
    for (int i = visibleEntities.Count - 1; i >= 0; i--)
    {
      var target = visibleEntities[i];

      if(target.life == target.maxLife)
      {
        visibleEntities.RemoveAt(i);
        if(visibleEntities.Count == 0) return visibleEntities;
      }
    }

    return visibleEntities;
  }
}