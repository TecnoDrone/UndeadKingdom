using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
  public float minFlickerIntensity = 0.5f;
  public float maxFlickerIntensity = 2.5f;
  public float flickerSpeed = 0.035f;

  private Light light;

  // Start is called before the first frame update
  void Start()
  {
    light = GetComponent<Light>();
    StartCoroutine(Flicker());
  }

  IEnumerator Flicker()
  {
    while(true)
    {
      light.intensity = Random.Range(minFlickerIntensity, maxFlickerIntensity);
      yield return new WaitForSeconds(flickerSpeed);
    }
  }
}