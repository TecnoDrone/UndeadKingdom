using Assets.Scripts;
using Assets.Scripts.AI;
using System.Collections;
using UnityEngine;

public class Consume : Spell
{
  public CombatAI target;
  public int damage;
  private bool isConsuming;

  // Start is called before the first frame update
  public override void Start()
  {
    base.Start();

    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Undead")))
    {
      target = hit.transform.GetComponent<CombatAI>();
      transform.position = target.transform.position;
    }
    else
    {
      Destroy(gameObject);
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyUp(KeyCode.Q))
    {
      Destroy(gameObject);
    }

    StartCoroutine(ConsumeTarget());
  }

  IEnumerator ConsumeTarget()
  {
    if(!isConsuming)
    {
      isConsuming = true;
      target.TakeDamage(damage);
      Player.Instance.AddEnergy(damage);

      if (target.IsDead)
      {
        Destroy(gameObject);
      }

      yield return new WaitForSeconds(1);
      isConsuming = false;
    }
  }
}
