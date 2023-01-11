using Assets.Scripts.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Entity : MonoBehaviour
{
  //Alive variables
  public int life = 10;
  public int maxLife = 10;

  [HideInInspector]
  public bool IsDead => life <= 0;

  private float counter; //for visual effect when hit
  private bool restartAnimation; //for visual effect when hit
  private bool isDead;
  private Color originalColor; //for visual effect when hit

  //Death variables
  private AudioClip deathSoundEffect;
  private AudioClip disintegrateSoundEffect;
  public AudioClip hurtSoundEffect;
  private GameObject bodypartsContainer;

  [HideInInspector]
  public SpriteRenderer spriteRenderer;
  protected AudioSource audioSource;
  private float defaultPitch;

  public virtual void Start()
  {
    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    audioSource = GetComponent<AudioSource>();

    audioSource = gameObject.GetComponent<AudioSource>();
    audioSource.clip = hurtSoundEffect;
    defaultPitch = audioSource.pitch;

    originalColor = spriteRenderer.material.color;

    if (life == 0) Death();
  }

  public virtual void Update()
  {
    if (restartAnimation)
    {
      counter += Time.deltaTime * 3;
      spriteRenderer.material.color = Color.Lerp(Color.red, originalColor, counter);

      if (spriteRenderer.material.color == originalColor)
      {
        restartAnimation = false;
      }
    }
  }

  public virtual void TakeDamage(int amount)
  {
    life -= amount;
    if (life <= 0 && !isDead)
    {
      Death();
      return;
    }

    if (audioSource != null)
    {
      audioSource.pitch = defaultPitch + Random.Range(-0.05f, 0.05f);
      audioSource.PlayOneShot(hurtSoundEffect);
    }

    restartAnimation = true;
    counter = 0;
  }

  public virtual bool Heal(int amount)
  {
    if (life == maxLife) return false;

    life += amount;
    if (life > maxLife) life = maxLife;

    StartCoroutine(HealingAnimation());

    return true;
  }

  //The entity dies and leave a corpse on the ground
  public virtual void Death()
  {
    //Play death sound
    //Start death animation
    GameManager.DeadEntities.Add(this);
    isDead = true;
    OnDeath();
  }

  public virtual void OnDeath() { }

  //The entity dies and leave NO corpse on the ground
  public List<Transform> Disintegrate()
  {
    GameManager.DeadEntities.Add(this);

    //Play Disintegrate sound
    if (disintegrateSoundEffect != null) AudioSource.PlayClipAtPoint(disintegrateSoundEffect, transform.position, 0.1f);

    //Spawn separate pieces
    List<Transform> bodyparts = null;
    if (bodypartsContainer != null)
    {
      var container = Instantiate(bodypartsContainer, transform.position, transform.rotation);
      bodyparts = new List<Transform>();
      foreach (Transform bodypart in container.transform)
      {
        bodyparts.Add(bodypart);
      }
      container.transform.DetachChildren();
      Destroy(container);
    }

    //Destory entity (x_x)
    if (!isDead) Death();

    return bodyparts;
  }

  IEnumerator HealingAnimation()
  {
    spriteRenderer.material.SetFloat("_Healing", 1);

    yield return new WaitForSeconds(1);

    spriteRenderer.material.SetFloat("_Healing", 0);
  }
}
