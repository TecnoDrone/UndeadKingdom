using Assets.Scripts;
using TMPro;
using UnityEngine;

public class UISoulsCounter : MonoBehaviour
{
  private TextMeshProUGUI soulsCounter;

  public void Awake()
  {
    soulsCounter = GetComponent<TextMeshProUGUI>();
    PlayerEntity.onPlayerSoulsUpdate += UpdateCounter;
  }

  public void UpdateCounter()
  {
    soulsCounter.text = PlayerEntity.Instance.souls.ToString();
  }
}
