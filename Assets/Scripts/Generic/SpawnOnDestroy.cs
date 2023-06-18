using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
  public GameObject spawnPrefab;

  private void Awake()
  {
    var entity = GetComponent<Entity>();
    entity.onEntityDied += SpawnPrefab;
  }

  public void SpawnPrefab(Entity entity)
  {
    Instantiate(spawnPrefab, transform.position, spawnPrefab.transform.rotation);
  }
}