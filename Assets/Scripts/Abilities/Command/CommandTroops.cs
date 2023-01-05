using Assets.Scripts.Generic;
using Assets.Scripts.Managers;
using Assets.Scripts.Player;
using System.Collections;
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
namespace Assets.Scripts.Spells
{
  public class CommandTroops : MonoBehaviour
  {
    public KeyCode key;
    public PlayerStance state;

    private Dictionary<int, Ghost> Ghosts = new Dictionary<int, Ghost>();
    private float? speed = null;
    private GameObject container;
    private Vector3 destination;

    private Coroutine rotating;

    //2023-01-04 e.oliosi - TODO: remove container when skill is over with
    private void Start()
    {
      container = new GameObject("Command");
      PlayerEntity.onPlayerStanceChange += CheckPlayerStance;
    }

    private void Update()
    {
      if (PlayerEntity.Instance.Stance != state) return;
      if (!GameManager.SelectedUnits.Any()) return;

      if (Input.GetKeyDown(key))
      {
        if (PlayerEntity.Instance.State == PlayerState.Casting) return;

        if (Preview())
        {
          PlayerEntity.Instance.State = PlayerState.Casting;
          PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Casting);
          rotating = StartCoroutine(RotatePreview());
        }
      }

      if (Input.GetKeyUp(key) && rotating != null)
      {
        Stop();
        Command();
      }
    }

    private void CheckPlayerStance()
    {
      if (PlayerEntity.Instance.Stance != state) Stop();
    }

    private void Stop()
    {
      if (rotating != null)
      {
        StopCoroutine(rotating);
        rotating = null;

        PlayerEntity.Instance.State = PlayerState.Idle;
        PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Idle);

        foreach (var (_, ghost) in Ghosts)
        {
          Destroy(ghost.go);
        }

        Ghosts.Clear();
        container.transform.rotation = default;
      }
    }

    /// <summary>
    /// Shows the user a ghost preview of the position each troop will occupy once the command is given
    /// </summary>
    IEnumerator RotatePreview()
    {
      while (true)
      {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out var hit, Mathf.Infinity);

        destination = new Vector3(hit.point.x, 0, hit.point.z);

        Vector3 direction = container.transform.localPosition - destination;

        var rotation = Quaternion.LookRotation(-container.transform.up, direction).eulerAngles.y;
        container.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));

        yield return null;
      }
    }

    private void Command()
    {
      //check location under mouse and decide one of the following
      //- Attack
      //- Move
      foreach (var (id, ghost) in Ghosts)
      {
        var unit = GameManager.SelectedUnits[id];
        unit.WalkTo(ghost.go.transform.position, speed);
        Destroy(ghost.go);
      }

      Ghosts.Clear();
      container.transform.rotation = default;
    }

    private bool Preview()
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      var hasHit = Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground"));
      if (!hasHit) return false;

      container.transform.position = hit.point;
      var gameobjectHit = hit.transform.gameObject;

      var army = GameManager.SelectedUnits;
      if (army == null || !army.Any()) return false;

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
      var zStart = container.transform.position.z - (divisionHeight / 2f);
      var zOffset = 0;

      foreach ((var squadron, var configuration, var direction, var speed) in groups)
      {
        var xOffset = ((10f - configuration.Max()) / 2) - 4.5f;
        zOffset += 1;//configuration.Length;

        //Calculate current division position and calculate the position of each unit from that starting point
        var squadronPosition = new Vector3(container.transform.position.x + xOffset, container.transform.position.y, zStart + zOffset);
        var positions = SquadronService.GetPositions(squadron.Count, squadronPosition, direction);

        //Apply position to each ghost
        for (int i = 0; i < positions.Count; i++)
        {
          (var id, var unit) = squadron.ElementAt(i);
          var position = positions[i];

          var ghost = new Ghost(unit.sprite, container.transform, position);

          if (!Ghosts.ContainsKey(id)) Ghosts.Add(id, ghost);
        }
      }

      return true;
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
