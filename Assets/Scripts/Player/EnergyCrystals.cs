using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCrystals : MonoBehaviour
{
  public float radius;
  public float rotationSpeed;
  public GameObject CrystalEntity;

  private int maxCrystalAmount;
  private List<GameObject> crystals = new List<GameObject>();

  void Start()
  {
    maxCrystalAmount = PlayerEntity.Instance.maxLife;

    PlayerEntity.onPlayerLifeConsumed += RemoveCrystals;
    PlayerEntity.onPlayerLifeGained += AddCrystals;

    //Offset crystal to life by 1, so that on 1hp the player has no crystals
    AddCrystals(PlayerEntity.Instance.life - 1);
  }

  private void Update()
  {
    transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
  }

  private void AddCrystals(int amount)
  {
    if (amount == 0) return;
    if (crystals.Count >= maxCrystalAmount) return;
    if (crystals.Count + amount > maxCrystalAmount) return;

    //Cant exceed max energy
    if (crystals.Count + amount > PlayerEntity.Instance.maxLife) return;

    for(int i = 0; i < amount; i++)
    {
      var crystal = Instantiate(CrystalEntity, transform);
      crystals.Add(crystal);
    }

    CircleUtility.ArrangeInCircle(transform.position, radius, crystals);
  }

  private void RemoveCrystals(int amount)
  {
    if (amount == 0) return;
    if (crystals.Count == 0) return;

    if (amount > crystals.Count) amount = crystals.Count;

    for (int i = 0; i < amount; i++)
    {
      Destroy(crystals[i]);
      crystals.RemoveAt(i);
    }

    CircleUtility.ArrangeInCircle(transform.position, radius, crystals);
  }
}
