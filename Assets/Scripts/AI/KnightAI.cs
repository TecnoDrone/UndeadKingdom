using Assets.Scripts.AI;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using Extentions;
using UnityEngine;

public class KnightAI : CombatAI
{
  public List<AudioClip> ReanimationClips;

  public KnightAI()
  {
    attackRange = 1f;
    viewDistance = 3f;
    attackSpeed = 1f;
    Kind = CreatureKind.Knight;
    order = 1;
  }

  public override void Start()
  {
    base.Start();

    var randomClip = ReanimationClips[Random.Range(0, ReanimationClips.Count - 1)];
    audioSource.PlayClipAtPoint(transform.position, randomClip, Random.Range(0.9f, 1.1f), audioSource.volume);
  }
}
