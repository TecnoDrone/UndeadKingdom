using Assets.Scripts.Generic;
using Assets.Scripts.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//per ordinamento vedi
//Algoritmi di flusso
//vedi anche https://it.wikipedia.org/wiki/Algoritmo_di_Ford-Fulkerson
//2022-03-22 e.oliosi What to do to make this more sophisticated
//Create a parent gameobject "squadron" and have each child to chase its assigned place in the squadron
//then move the parent "squadron" to the destination
//To solve the problem when a unit cannot reach its position, simply shoot a ray from the center of the squadron to the position,
//If a wall is hit, then the unit position is shortened.
namespace Assets.Scripts.Spells.Command
{
  public class CommandTroops : Spell
  {
    public Dictionary<int, Ghost> Ghosts = new Dictionary<int, Ghost>();
    private float? speed = null;

    public override void Start()
    {
      base.Start();

      LayerMask ground = (1 << LayerMask.NameToLayer("Ground"));
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      var hasHit = Physics.Raycast(ray, out var hit, Mathf.Infinity);
      if (!hasHit) return;

      transform.position = hit.point;

      var gameobjectHit = hit.transform.gameObject;
      var layerHit = gameobjectHit.layer;

      //MOVE
      if (layerHit == LayerMask.NameToLayer("Ground"))
      {
        InitializePreview();
      }

      //TODO: order attack
    }

    public void Update()
    {
      if (Input.GetKey(KeyCode.Mouse1))
      {
        RotatePreview(transform.position);
      }

      if (Input.GetKeyUp(KeyCode.Mouse1))
      {
        //check location under mouse and decide one of the following
        //- Attack
        //- Move
        foreach (var (id, ghost) in Ghosts)
        {
          var unit = GameManager.SelectedUnits[id];
          unit.WalkTo(ghost.go.transform.position, speed);
        }

        Destroy(gameObject);
      }
    }

    public void InitializePreview()
    {
      var army = GameManager.SelectedUnits;
      if (army == null || !army.Any()) return;

      //Order Divisions regroup by kind
      var groups =
        (from a in army
         orderby a.Value.order
         group a by a.Value.Kind into g
         select
         (
           Squadron: g.ToDictionary(k => k.Key, k => k.Value),
           Configuration: Configurations.Get(g.Count(x => x.Value)),
           Direction: g.Key == CreatureKind.UndeadArcher ? -1 : 1,
           Speed: g.First().Value.originalSpeed
         ))
        .ToList();

      speed = groups.Min(g => g.Speed);

      //Calculate where the first divion will be placed
      var divisionHeight = groups.Sum(s => s.Configuration.Length);
      var zStart = transform.position.z - (divisionHeight / 2f);
      var zOffset = 0;

      foreach ((var squadron, var configuration, var direction, var speed) in groups)
      {
        var xOffset = ((10f - configuration.Max()) / 2) - 4.5f;
        zOffset += 1;//configuration.Length;

        //Calculate current division position and calculate the position of each unit from that starting point
        var squadronPosition = new Vector3(transform.position.x + xOffset, transform.position.y, zStart + zOffset);
        var positions = SquadronService.GetPositions(squadron.Count, squadronPosition, direction);

        //Apply position to each ghost
        for (int i = 0; i < positions.Count; i++)
        {
          (var id, var unit) = squadron.ElementAt(i);
          var position = positions[i];

          var ghost = new Ghost(unit.sprite, transform, position);
          Ghosts.Add(id, ghost);
        }
      }
    }

    /// <summary>
    /// Shows the user a ghost preview of the position each troop will occupy once the command is given
    /// </summary>
    public void RotatePreview(Vector3 destination)
    {
      Vector3 direction = destination - transform.position;
      transform.rotation = Quaternion.LookRotation(-transform.up, direction);
    }
  }

  public class Ghost
  {
    public GameObject go;

    public Ghost(Sprite sprite, Transform parent, Vector3 position)
    {
      var ghost = new GameObject();

      var spriteGO = new GameObject();
      spriteGO.AddComponent<LookAtCamera>();
      var renderer = spriteGO.AddComponent<SpriteRenderer>();
      renderer.sprite = sprite;
      renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.47f); //120/255
      spriteGO.transform.rotation = Quaternion.Euler(new Vector3(60, 0, 0));
      spriteGO.transform.position = new Vector3(0, 0, -0.5f);
      spriteGO.transform.parent = ghost.transform;
      spriteGO.name = "Sprite";

      ghost.transform.parent = parent;
      ghost.transform.position = position;
      ghost.name = "Ghost";

      this.go = ghost;
    }
  }
}
