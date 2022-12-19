using UnityEngine;

public class DestroyWhenOver : MonoBehaviour
{
  void Start()
  {
    var main = GetComponent<ParticleSystem>().main;
    main.stopAction = ParticleSystemStopAction.Callback;
  }

  void OnParticleSystemStopped()
  {
    Destroy(gameObject);
  }
}
