using System.Collections.Generic;
using UnityEngine;

public class Summoner : MonoBehaviour
{
    public readonly List<GameObject> undeadUnderCommand = new List<GameObject>();
    public Color teamColor;

    [HideInInspector] public GameObject command;
    [HideInInspector] public GameObject reanimate;

    public void Start()
    {
        var reanimateScript = Resources.Load<GameObject>("Prefabs/Reanimate");
        var commandScript = Resources.Load<GameObject>("Prefabs/Command");

        reanimate = Instantiate(reanimateScript, transform);
        command = Instantiate(commandScript, transform);

        teamColor = GetComponent<SpriteRenderer>().color;
    }

    //When the summoner dies
    public void Die()
    {
        //free controlled creatures
        foreach (var undead in undeadUnderCommand)
        {
            undead.GetComponent<Zombie>().SetFree();
        }

        Destroy(gameObject);
    }

    internal void AddZombie(GameObject zombie)
    {
        undeadUnderCommand.Add(zombie);
        zombie.layer = LayerMask.NameToLayer("Zombie");

        var zombieScript = zombie.GetComponent<Zombie>();

        zombieScript.currentFreeWill = zombieScript.maxFreeWill;
        zombieScript.Master = gameObject;
    }
}