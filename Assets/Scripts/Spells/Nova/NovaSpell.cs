using UnityEngine;

namespace Assets.Scripts.Spells.Nova
{
  [CreateAssetMenu]
  public class NovaSpell : SpellObject
  {
    protected override void Init(GameObject parent)
    {
      castPosition = parent.transform.position;
    }
  }
}
