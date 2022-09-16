using Assets.Scripts;
using Assets.Scripts.Generic;
using Assets.Scripts.Managers;
using UnityEngine;

public class Spellcasting : MonoBehaviour
{
  public float fireRate;
  public Transform castPoint;
  public LayerMask Ground;

  public bool canCast = true;
  private float lastCastTime;

  public void Start()
  {
    castPoint = transform.Find("Spawn").transform;
    lastCastTime = Time.time;
  }

  public void Cast(string spellKind)
  {
    if (!canCast) return;

    if (Time.time > fireRate + lastCastTime)
    {
      (var script, var go) = SpellsManager.GetSpellFromCache(spellKind);
      if (go != null)
      {
        if(Player.Instance.energy >= script.EnergyCost)
        {
          var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
          if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
          {
            script.Destination = hit.point;
          }

          var spell = Instantiate(go, castPoint.position, transform.rotation);
          spell.name = spellKind;

          lastCastTime = Time.time;
        }
      }
    }

  }
}
