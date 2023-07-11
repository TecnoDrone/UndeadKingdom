using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Managers
{
  public class CorpseManager : MonoBehaviour
  {
    static Dictionary<(Team, CreatureKind), List<GameObject>> Corpses = new Dictionary<(Team, CreatureKind), List<GameObject>>();

    private void Start()
    {
      //Cache every corpse of every creature kind
      foreach (CreatureKind kind in Enum.GetValues(typeof(CreatureKind)))
      {
        foreach(Team team in Enum.GetValues(typeof(Team)))
        {
          Corpses[(team, kind)] = LoadCorpses(team, kind);
        }
      }
    }

    private List<GameObject> LoadCorpses(Team team, CreatureKind kind)
    {
      var corpses = Resources.LoadAll<GameObject>($"Prefabs/Creatures/{team}/{kind}/Corpse").ToList();
      return corpses;
    }

    public static GameObject GetRandomCorspe(Team team, CreatureKind kind)
    {
      var corpses = Corpses[(team, kind)];
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
    UndeadKnight,
    Cultist
  }
}
