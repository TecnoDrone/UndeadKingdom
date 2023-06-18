using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
  public KeyCode key;
  public float range;

  private bool interact;
  private Rigidbody rig;

  private void Awake()
  {
    rig = GetComponent<Rigidbody>();
  }

  public void Update()
  {
    if (Input.GetKeyDown(key))
    {
      interact = true;
    }
  }

  public void FixedUpdate()
  {
    if (!interact) return;
    interact = false;

    var hits = Physics.OverlapSphere(rig.worldCenterOfMass, range);
    if (hits == null || hits.Length == 0) return;

    foreach (var hit in hits)
    {
      hit.gameObject.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
    }
  }
}