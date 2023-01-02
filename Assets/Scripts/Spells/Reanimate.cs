using Assets.Scripts.AI;
using Extentions;
using System.Linq;
using UnityEngine;

/***********************************************************************
 * Author            : e.oliosi
 * Date created      : 2020-10-26
 * Purpose           : Turns a Corpse in a Zombie
 * *********************************************************************/
namespace Assets.Scripts.Spells
{
  public class Reanimate : SpellBehaviour
  {
    public LayerMask target;
    public float intensity = 3f;
    public float range = 1.5f;
    public Color color = Color.red;
    public Color emissionColor;

    private bool startFading = false;
    private float lastFadeTime = 0f;
    private AudioSource audio;
    private ParticleSystem particles;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
      lastFadeTime = Time.time;
      audio = GetComponent<AudioSource>();
      particles = GetComponent<ParticleSystem>();
      spriteRenderer = GetComponent<SpriteRenderer>();

      spriteRenderer.material.SetColor("_Color", color);
      spriteRenderer.material.SetColor("_EmissionColor", emissionColor.Intesify(intensity));

      transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
      transform.position = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
    }

    void Update()
    {
      startFading = !audio.isPlaying;

      if (startFading) FadeOut();
    }

    private void FadeOut()
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

        //if (spriteRenderer.material.GetFloat("_Opacity") <= 0) Destroy(gameObject);

        lastFadeTime += 0.2f;
      }
    }
  }
}