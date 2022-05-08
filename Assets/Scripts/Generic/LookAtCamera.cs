using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
  private void Update()
  {
    transform.rotation = Quaternion.Euler(new Vector3(60f, 0f, transform.rotation.z));
    //transform.LookAt(Camera.main.transform, Vector3.up);
  }
}
