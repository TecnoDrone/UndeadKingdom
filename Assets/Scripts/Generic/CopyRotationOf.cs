using UnityEngine;

namespace Assets.Scripts.Generic
{
  public class CopyRotationOf : MonoBehaviour
  {
    public GameObject ObjectToCopyRotationFrom;

    public void Start()
    {
      transform.rotation = ObjectToCopyRotationFrom.transform.rotation; 
    }
  }
}
