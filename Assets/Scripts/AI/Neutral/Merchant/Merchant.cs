using Assets.Scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Merchant : MonoBehaviour
{
  public AudioClip sellSE;
  public List<Sellable> catalog = new List<Sellable>();
  private List<Spot> spots = new();

  public void Awake()
  {
    for(int i = 1; i <= 3; i++)
    {
      var spot = new Spot();
      spot.transform = transform.Find("Item" + i);
      spot.textMeshPRO = spot.transform.Find("PriceTag").GetComponent<TextMeshPro>();
      spots.Add(spot);
    }
  }

  public void Start()
  {
    CreateShop();
  }

  void CreateShop()
  {
    foreach(var spot in spots)
    {
      var sellable = catalog[Random.Range(0, catalog.Count)];
      var go = Instantiate(sellable.item, spot.transform.position + sellable.item.transform.position, sellable.item.transform.rotation);

      var shopItem = spot.transform.gameObject.AddComponent<ShopItem>();
      shopItem.item = go;
      shopItem.price = sellable.Price;
      shopItem.onItemSold += HandleItemBeingBought;

      spot.textMeshPRO.text = sellable.Price.ToString();
      shopItem.priceTag = spot.textMeshPRO;
    }
  }

  public void HandleItemBeingBought(ShopItem item)
  {
    if (PlayerEntity.Instance.souls < item.price) return;

    PlayerEntity.Instance.souls -= item.price;
    PlayerEntity.onPlayerSoulsUpdate?.Invoke();
    item.SellItem();
    AudioSource.PlayClipAtPoint(sellSE, item.transform.position, 0.1f);
  }

  internal struct Spot
  {
    public Transform transform;
    public TextMeshPro textMeshPRO;
  }
}
