using UnityEngine;

/***********************************************************************
 * Author            : e.oliosi
 * Date created      : 2020-10-26
 * Purpose           : 
 * - Control an uncontrolled Undead
 * - Control other summoners's Undeads
 * *********************************************************************/
public class Command : Spell
{
  public LayerMask target;
  public int spellPower;
  public int maxControllableZombies = 3;

  public override void Start()
  {
    base.Start();
  }

  public void CastSpell(GameObject spellTarget = null)
  {
    if (spellTarget == null)
    {
      var hit = Physics2D.Raycast(transform.position, transform.forward, Mathf.Infinity, target).collider;

      if (hit != null)
      {
        ApplySpell(hit.gameObject);
      }
    }
    else
    {
      ApplySpell(spellTarget);
    }
  }

  private void ApplySpell(GameObject zombieGameObject)
  {
    var zombieScript = zombieGameObject.GetComponent<Zombie>();
    //var zombieWill = zombieScript.DamageWill(spellPower, CasterScript.teamColor);

    //if (zombieWill <= 0)
    //{
    //  CasterScript.AddZombie(zombieGameObject);
    //}
  }
}