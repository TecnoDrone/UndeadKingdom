using System.Collections.Generic;

public abstract class SearchRule
{
  public abstract List<Entity> Apply(List<Entity> visibleEntities);
}