using Extentions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AI.Undead
{
  public class Undead : CombatAI
  {
    public List<AudioClip> ReanimationClips;

    public override void Start()
    {
      base.Start();

      squad = Team.Undead;

      if (ReanimationClips.Any())
      {
        var randomClip = ReanimationClips[Random.Range(0, ReanimationClips.Count - 1)];
        audioSource.PlayClipAtPoint(transform.position, randomClip, Random.Range(0.9f, 1.1f), audioSource.volume);
      }
    }
  }
}
