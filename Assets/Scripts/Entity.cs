using Assets.Scripts;
using Assets.Scripts.AI.Undead;
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
  public Team team;

  [HideInInspector]
  public bool IsDead => life <= 0;

  public delegate void OnEntityDied(Entity entity);
  public OnEntityDied onEntityDied;

  public delegate void OnLifeChanged();
  public OnLifeChanged onLifeChanged;

  private float counter; //for visual effect when hit
  private bool restartAnimation; //for visual effect when hit
  private bool isDead;
  private Color originalColor; //for visual effect when hit

  internal Vector3 center => new Vector3(
    transform.position.x,
    transform.position.y + 0.25f,
    transform.position.z - 0.5f
  );

  //Death variables
  public AudioClip deathSoundEffect;
  public AudioClip hurtSoundEffect;

  [HideInInspector]
  public SpriteRenderer spriteRenderer;
  protected AudioSource audioSource;
  private float defaultPitch;

  private float healingTime;

  public virtual void Start()
  {
    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    audioSource = GetComponent<AudioSource>();

    healingTime = Time.time;

    audioSource.clip = hurtSoundEffect;
    defaultPitch = audioSource.pitch;

    originalColor = spriteRenderer.material.color;

    onEntityDied += OnDeath;

    if (life == 0) onEntityDied?.Invoke(this);
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

  public virtual void OnDeath(Entity entity) => Destroy(entity);

  public virtual void TakeDamage(int amount)
  {
    if (amount == 0) return;
    if (isDead) return;

    life -= amount;
    life = Mathf.Max(life, 0);
    onLifeChanged?.Invoke();

    if (life <= 0)
    {
      isDead = true;  
      onEntityDied?.Invoke(this);
      return;
    }

    if (audioSource != null)
    {
      audioSource.pitch = defaultPitch + Random.Range(-0.05f, 0.05f);
      audioSource.PlayOneShot(hurtSoundEffect, 0.1f);
    }

    restartAnimation = true;
    counter = 0;
  }

  public virtual bool Heal(int amount)
  {
    if (life == maxLife) return false;

    life += amount;
    if (life > maxLife) life = maxLife;
    onLifeChanged?.Invoke();

    healingTime = Time.time;
    StartCoroutine(HealingAnimation());

    return true;
  }

  IEnumerator HealingAnimation()
  {
    while(true)
    {
      if(Time.time <= healingTime + 1)
      {
        spriteRenderer.material.SetFloat("_Healing", 1);
      }
      else
      {
        spriteRenderer.material.SetFloat("_Healing", 0);
      }

      yield return null;
    }
  }
}
