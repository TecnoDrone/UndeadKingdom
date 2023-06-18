using UnityEngine;

namespace Assets.Scripts.AI
{
  public class Villager : FleeAI
  {
    public Sprite[] deathSprites;

    public override void Death()
    {
      base.Death();
      var randomRotation = Random.Range(0f, 360f);

      var corpse = new GameObject();
      corpse.layer = LayerMask.NameToLayer("Corpse");
      corpse.transform.rotation = Quaternion.Euler(90f, 0f, randomRotation);
      corpse.transform.position = new Vector3(
        transform.position.x,
        transform.position.y + 0.05f,
        transform.position.z);

      var rand = Random.Range(0, deathSprites.Length - 1);
      var randomSprite = deathSprites[rand];
      var spriteRenderer = corpse.AddComponent<SpriteRenderer>();
      spriteRenderer.sprite = randomSprite;

      var rig = corpse.AddComponent<Rigidbody>();
      rig.useGravity = false;

      var collider = corpse.AddComponent<BoxCollider>();
      collider.isTrigger = true;
      collider.center = new Vector3(0f, 0.5f, 0f);
      collider.size = new Vector3(1f, 1f, 0.2f);
    }
  }

}
