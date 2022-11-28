using UnityEngine;

namespace Assets.Scripts.Generic
{
  public class LookAtCamera : MonoBehaviour
  {
    private void LateUpdate()
    {
      transform.forward = Camera.main.transform.forward;
    }
  }
}
