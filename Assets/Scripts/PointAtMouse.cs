using UnityEngine;

public class PointAtMouse : MonoBehaviour
{
  public float offset = 0f;

  // Start is called before the first frame update
  private void Start()
  {

  }

  // Update is called once per frame
  private void Update()
  {
    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out var hit))
    {
      Vector3 toTarget = hit.point - transform.position;
      float rotation = -Quaternion.LookRotation(Vector3.up, toTarget).eulerAngles.y + offset;

      transform.rotation = Quaternion.Euler(60f, 0f, rotation);
    }
  }
}
