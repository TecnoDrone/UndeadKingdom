using UnityEngine;

namespace Assets.Scripts
{
  public class Corpse : MonoBehaviour
  {
    public GameObject Reanimation;

    public void Start()
    {
      var rotation = Random.Range(0, 360f);
      transform.rotation = Quaternion.Euler(90f, 0f, rotation);
    }
  }
}
