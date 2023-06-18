using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
  public class UIKeyCounter : MonoBehaviour
  {
    private TextMeshProUGUI keyCounter;


    public void Awake()
    {
      keyCounter = GetComponent<TextMeshProUGUI>();
      PlayerEntity.onPlayerKeyUpdate += UpdateCounter;
    }

    public void UpdateCounter()
    {
      keyCounter.text = PlayerEntity.Instance.keys.ToString();
    }
  }
}
