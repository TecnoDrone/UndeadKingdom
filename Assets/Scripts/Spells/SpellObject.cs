using Assets.Scripts.Spells;
using UnityEngine;

/***********************************************************************
 * Author            : e.oliosi
 * Date created      : 2020-10-26
 * Purpose           : Basic structure of a spell
 * *********************************************************************/
public abstract class SpellObject : ScriptableObject
{
  public SpellKind spellKind;
  public int Cost;

  public float ActiveTime;
  public float CooldownTime;

  public Vector3 castPosition;
  public Quaternion castRotation;

  [HideInInspector]
  public SpellStates state = SpellStates.Ready;

  protected GameObject spellObject;

  public virtual bool TryCast(GameObject parent)
  {
    spellObject = Resources.Load<GameObject>($"Prefabs/Spells/{spellKind}/{spellKind}");
    var spellBehaviour = spellObject.GetComponent<SpellBehaviour>();

    if (!spellBehaviour.CanCast()) return false;

    castPosition = spellObject.transform.position;
    castRotation = spellObject.transform.rotation;

    Init(parent);
    Instantiate(spellObject, castPosition, castRotation);

    return true;
  }

  protected abstract void Init(GameObject parent);
}

public enum SpellKind
{
  Reanimate,
  Consume,
  DarkHail
}

public enum SpellStates
{
  Ready,
  Active,
  Cooldown
}