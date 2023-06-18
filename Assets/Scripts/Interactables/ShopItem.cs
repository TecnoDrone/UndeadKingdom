using System;
using TMPro;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
  public delegate void OnItemSold(ShopItem item);
  public OnItemSold onItemSold;

  public int price;
  public GameObject item;
  public TextMeshPro priceTag; 

  private BoxCollider collider = null;

  public void Start()
  {
    if (!item.TryGetComponent(out collider)) throw new ArgumentException("Invalid shop item");
    collider.enabled = false;
  }

  //Tell the owner someone is trying to buy this item
  public void Interact()
  {
    onItemSold?.Invoke(this);
  }

  //Actually sell the item
  public void SellItem()
  {
    collider.enabled = true;

    Destroy(priceTag.gameObject);
    Destroy(this);
  }
}