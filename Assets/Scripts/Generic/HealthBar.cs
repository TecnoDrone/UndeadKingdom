using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Generic
{
  public class HealthBar : MonoBehaviour
  {
    public static HealthBar Instance { get; private set; }
    private Slider slider;
    public Image damaged;

    private float Health;
    private float MaxHealth;

    public void Start()
    {
      Instance = this;
      slider = GetComponent<Slider>();
      MaxHealth = PlayerEntity.Instance.life;
      Health = MaxHealth;

      slider.minValue = 0;
      slider.maxValue = MaxHealth;
      slider.value = Health;
    }

    public void AddHealth(float health)
    {
      if (health < 0) return;
      if (Health >= MaxHealth) return;

      Health = Mathf.Clamp(Health + health, 0, MaxHealth);
      slider.value = Health;
    }

    public void RemoveHealth(float health)
    {
      if (health < 0) return;
      if (Health == 0) return;

      Health = Mathf.Clamp(Health - health, 0, MaxHealth);
      slider.value = Health;
    }
  }
}
