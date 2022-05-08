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
      var chachedSpell = GameManager.GetSpellFromCache(spellKind);
      if (chachedSpell != null)
      {
        var spell = Instantiate(chachedSpell, castPoint.position, transform.rotation);
        spell.name = spellKind;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
          spell.GetComponent<Spell>().Destination = hit.point;
        }

        lastCastTime = Time.time;
      }
    }

  }
}
