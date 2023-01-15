using UnityEngine;

namespace Assets.Scripts
{
  public class Corpse : MonoBehaviour
  {
    public GameObject Reanimation;
    public bool randomizeRotation = true;

    public void Start()
    {
      if(randomizeRotation)
      {
        var rotation = Random.Range(0, 360f);
        transform.rotation = Quaternion.Euler(90f, 0f, rotation);
      }
    }
  }
}
