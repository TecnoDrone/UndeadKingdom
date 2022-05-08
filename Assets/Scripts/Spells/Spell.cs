using System;
using UnityEngine;

/***********************************************************************
 * Author            : e.oliosi
 * Date created      : 2020-10-26
 * Purpose           : Basic structure of a spell
 * *********************************************************************/
public abstract class Spell : MonoBehaviour, ISpell
{
  public string Name => GetType().Name;

  [HideInInspector]
  public Vector3 Destination;

  [HideInInspector]
  public SpriteRenderer spriteRenderer;

  public virtual void Start()
  {
    spriteRenderer = GetComponent<SpriteRenderer>();
    if(spriteRenderer != null) spriteRenderer.material.color = spriteRenderer.color;

    StartingOrientation();
  }

  public virtual void StartingOrientation() { }
}

public interface ISpell 
{
  void StartingOrientation();
}