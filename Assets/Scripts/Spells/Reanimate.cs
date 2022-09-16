using Assets.Scripts;
using Assets.Scripts.AI;
using Extentions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/***********************************************************************
 * Author            : e.oliosi
 * Date created      : 2020-10-26
 * Purpose           : Turns a Corpse in a Zombie
 * *********************************************************************/
public class Reanimate : Spell, ISpell
{
  public float reanimationRange = 0;
  public LayerMask target;
  public bool startFading = false;

  public Color color;
  public Color emissionColor;

  private float lastFadeTime = 0f;
  private float intensity = 3f;
  private AudioSource audio;
  private ParticleSystem particles;

  public override void Start()
  {
    //Destination = transform.position;
    base.Start();

    lastFadeTime = Time.time;
    audio = GetComponent<AudioSource>();
    particles = GetComponent<ParticleSystem>();

    spriteRenderer.material.SetColor("_Color", color);
    spriteRenderer.material.SetColor("_EmissionColor", emissionColor.Intesify(intensity));

    var hits = Physics.OverlapSphere(transform.position, reanimationRange, target);

    //Turn nearby corpses into zombies
    foreach (var hit in hits)
    {
      //Generate undead
      var corpse = hit.GetComponent<Corpse>();
      var undead = Instantiate(corpse.Reanimation, hit.transform.position, corpse.Reanimation.transform.rotation);

      //Set undead under player control
      Player.ControlledMinions.Add(undead.GetComponent<CombatAI>());

      //Destroy corpse
      Destroy(hit.gameObject);
    }
  }

  public void Update()
  {
    startFading = !audio.isPlaying;

    if (startFading) FadeOut();
  }

  public override void StartingOrientation()
  {
    transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
    transform.position = new Vector3(Destination.x, Destination.y + 0.01f, Destination.z);
  }

  public void FadeOut() 
  {
    if (Time.time > lastFadeTime + 1) 
    {
      var currentOpacity = spriteRenderer.material.GetFloat("_Opacity");
      spriteRenderer.material.SetFloat("_Opacity", currentOpacity - 0.1f);
      spriteRenderer.material.SetColor("_EmissionColor", emissionColor.Intesify(intensity));
      intensity -= intensity == 0 ? 0 : 1;

      var colorOverLifetime = particles.colorOverLifetime;
      var particleColor = colorOverLifetime.color;
      particleColor.colorMin = particleColor.colorMin.ReduceAlpha(0.1f);
      particleColor.colorMax = particleColor.colorMax.ReduceAlpha(0.1f);
      colorOverLifetime.color = particleColor;

      if (spriteRenderer.material.GetFloat("_Opacity") <= 0) Destroy(gameObject);

      lastFadeTime += 0.2f;
    }
  }
}
