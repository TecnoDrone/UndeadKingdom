using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class Floating : MonoBehaviour
  {
    public float amplitude = 1f;
    public float speed = 0.1f;
    //private Vector3 _startPosition;

    void Start()
    {
      //_startPosition = transform.position;
    }

    void Update()
    {
      var _newPosition = transform.position;
      _newPosition.y += Mathf.Sin(Time.time * speed) * amplitude * Time.deltaTime;
      transform.position = _newPosition;
      //transform.position = _startPosition + new Vector3(Mathf.Sin(Time.time), 0.0f, 0.0f);
    }
  }
}
