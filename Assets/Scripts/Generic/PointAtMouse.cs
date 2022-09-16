using UnityEngine;

public class PointAtMouse : MonoBehaviour
{
  private void Update()
  {
    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out var hit))
    {
      Vector3 direction = hit.point - transform.position;
      direction.y = 0;

      //float rotation = -Quaternion.LookRotation(Vector3.up, direction).eulerAngles.y + offset;
      //transform.rotation = Quaternion.Euler(0f, rotation, 0f);
      transform.forward = direction;
    }
  }
}
