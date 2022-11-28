using Assets.Scripts.AI;
using Assets.Scripts.Projectiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Spells.Consume
{
  public class Consume : MonoBehaviour
  {
    public GameObject energyTrace;
    public int damage;

    private List<EntityAI> Targets;

    public void Start()
    {
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
      {
        var hits = Physics.SphereCastAll(hit.point, 1f, transform.up, 0, 1 << LayerMask.NameToLayer("Undead"));
        if (hits == null || !hits.Any())
        {
          Destroy(gameObject);
          return;
        }

        Targets = hits.Select(h => h.transform.GetComponent<EntityAI>())?.ToList();
        if(Targets == null || !Targets.Any())
        {
          Destroy(gameObject);
          return;
        }

        StartConsume();
      }
    }

    private void StartConsume()
    {
      //transform.position = Targets.transform.position;
      foreach(var target in Targets)
      {
        target.TakeDamage(damage);
        var go = Instantiate(energyTrace, target.transform.position, transform.rotation);
        go.GetComponent<EnergyTrace>().energy = damage;
      }

      Destroy(gameObject);
    }
  }
}