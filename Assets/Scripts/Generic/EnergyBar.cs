using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Generic
{
  public class EnergyBar : MonoBehaviour
  {
    public static EnergyBar Instance { get; private set; }
    private Slider slider;

    public float Energy { get; private set; }
    private float MaxEnergy;

    public void Start()
    {
      Instance = this;
      slider = GetComponent<Slider>();
      MaxEnergy = Player.Instance.maxEnergy;
      Energy = Player.Instance.energy;

      slider.minValue = 0;
      slider.maxValue = MaxEnergy;
      slider.value = Energy;
    }

    public void AddEnergy(int energy) => slider.value += energy;

    public void RemoveEnergy(int energy) => slider.value -= energy;
  }
}
