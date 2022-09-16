using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Managers
{
  public class CorpseManager : MonoBehaviour
  {
    static Dictionary<CreatureKind, List<GameObject>> Corpses = new Dictionary<CreatureKind, List<GameObject>>();

    private void Start()
    {
      //Cache every corpse of every creature kind
      foreach (CreatureKind kind in Enum.GetValues(typeof(CreatureKind)))
      {
        Corpses[kind] = LoadCorpses(kind);
      }
    }

    private List<GameObject> LoadCorpses(CreatureKind kind)
    {
      var corpses = Resources.LoadAll<GameObject>($"Prefabs/Creatures/{kind}/Corpse").ToList();
      return corpses;
    }

    public static GameObject GetRandomCorspe(CreatureKind kind)
    {
      var corpses = Corpses[kind];
      var random = Random.Range(0, corpses.Count - 1);

      var corpse = corpses.ElementAt(random);
      return corpse;
    }
  }

  public enum CreatureKind
  {
    Archer,
    Knight,
    UndeadArcher,
    UndeadKnight
  }
}
