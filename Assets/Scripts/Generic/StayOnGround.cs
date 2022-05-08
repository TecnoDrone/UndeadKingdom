using UnityEngine;

public class StayOnGround : MonoBehaviour
{
  // Update is called once per frame
  private void LateUpdate()
  {
    transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
  }
}
