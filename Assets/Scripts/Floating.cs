using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Floating : MonoBehaviour
  {
    public float amplitude = 1f;
    public float speed = 0.1f;

    private float startingY;

    private void Start()
    {
      startingY = transform.position.y + amplitude;
    }

    private void Update()
    {
      transform.position = new Vector3(transform.position.x, startingY + amplitude * Mathf.Sin(speed * Time.time), transform.position.z);
    }
  }
}
