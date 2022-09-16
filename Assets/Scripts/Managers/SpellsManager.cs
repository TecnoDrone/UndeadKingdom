using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers
{
  public class SpellsManager : MonoBehaviour
  {
    public static Dictionary<string, (Spell, GameObject)> CachedSpells = new Dictionary<string, (Spell, GameObject)>();

    //TODO: on start init all spells in the cache to avoid stuttering during gameplay

    public static (Spell, GameObject) GetSpellFromCache(string spellKind)
    {
      if (!CachedSpells.TryGetValue(spellKind, out (Spell script, GameObject go) spell))
      {
        var go = (GameObject)Resources.Load("Prefabs/Spells/" + spellKind);
        var script = go.GetComponent<Spell>();

        spell = (CachedSpells[spellKind] = (script, go));
      }

      return spell;
    }
  }
}
