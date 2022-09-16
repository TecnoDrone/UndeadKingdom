using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
