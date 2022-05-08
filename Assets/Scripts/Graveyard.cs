using Extentions;
using UnityEngine;

public class Graveyard : MonoBehaviour
{
    public GameObject corpse;
    public int count;

    private BoxCollider2D boxCollider2D;

    public void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        SpawnCorpses();
    }

    public void SpawnCorpses()
    {
        for(int i = 0; i < count; i++)
        {
            Instantiate(corpse, boxCollider2D.GetRandomPointInCollider(), transform.rotation);
        }
    }
}
