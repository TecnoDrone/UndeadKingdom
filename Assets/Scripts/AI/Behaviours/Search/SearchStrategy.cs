using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchStrategy
{
  internal LayerMask target;
  internal LayerMask whatCanBeSeen;
  internal float viewDistance;

  internal List<SearchRule> rules = new();

  public SearchStrategy(LayerMask target, float viewDistance)
  {
    this.target = target;
    whatCanBeSeen = target | (1 << LayerMask.NameToLayer("Wall"));
    this.viewDistance = viewDistance;
  }

  private List<Entity> FindVisibileTargets(Vector3 pos)
  {
    var possibleTargets = Physics.OverlapSphere(pos, viewDistance, target);
    if (possibleTargets == null || possibleTargets.Length == 0) return new();

    //Check if targets in range are hidden by walls
    var visibleTargets = new List<Entity>();
    foreach (var possibleTarget in possibleTargets)
    {
      if (!possibleTarget.transform.parent.TryGetComponent<Entity>(out var entity)) continue;

      var heading = possibleTarget.transform.position - pos;
      var direction = (heading / heading.magnitude) * viewDistance;

      var hit = Physics.Raycast(pos, direction, out var hitInfo, viewDistance, whatCanBeSeen);
      if (!hit) continue;

      var isVisible = hitInfo.collider.gameObject.GetInstanceID() == possibleTarget.gameObject.GetInstanceID();
      if (isVisible) visibleTargets.Add(entity);
    }

    //var distances = visibleTargets.Select(hit => (hit, distance: Vector3.Distance(hit.transform.position, pos))).OrderBy(x => x.distance);
    //var entity = distances.First().hit.GetComponentInParent<Entity>();
    return visibleTargets;
  }

  private Entity GetCloserTarget(Vector3 pos, List<Entity> visibleTargets)
  {
    var distances = visibleTargets.Select(hit => (hit, distance: Vector3.Distance(hit.transform.position, pos))).OrderBy(x => x.distance);
    var entity = distances.First().hit.GetComponentInParent<Entity>();
    return entity;
  }

  public Entity? SearchTarget(Vector3 pos)
  {
    //Look for visible targets
    var possibleTargets = FindVisibileTargets(pos);
    if (possibleTargets.Count == 0) return null;

    //Filter targets by applying each rule in order
    if(rules.Count > 0)
    {
      foreach(var rule in rules)
      {
        possibleTargets = rule.Apply(possibleTargets);
        if (possibleTargets.Count == 0) return null;
      }
    }

    //Return closes target
    if (possibleTargets.Count == 1) return possibleTargets[0];
    var entity = GetCloserTarget(pos, possibleTargets);
    return entity;
  }
}