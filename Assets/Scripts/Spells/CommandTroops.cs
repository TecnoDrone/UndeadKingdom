using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Extentions;

//2022-03-22 e.oliosiWhat to do to make this more sophisticated
//Create a parent gameobject "squadron" and have each child to chase its assigned place in the squadron
//then move the parent "squadron" to the destination
//To solve the problem when a unit cannot reach its position, simply shoot a ray from the center of the squadron to the position,
//If a wall is hit, then the unit position is shortened.
namespace Assets.Scripts.Spells
{
  public class CommandTroops : Spell
  {
    public Dictionary<int, GameObject> Ghosts = new Dictionary<int, GameObject>();

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

      //ATTACK
      if (layerHit == LayerMask.NameToLayer("Slime"))
      {
        var entity = gameobjectHit.GetComponent<Entity>();
        if (entity != null)
        {
          OrderAttack(entity);
          Destroy(gameObject);
        }
      }
    }

    public void Update()
    {
      //if (Input.GetKey(KeyCode.Mouse1))
      //{
      //  RotatePreview(transform.position);
      //}

      if (Input.GetKeyUp(KeyCode.Mouse1))
      {
        //check location under mouse and decide one of the following
        //- Attack
        //- Move
        foreach (var (id, ghost) in Ghosts)
        {
          var unit = GameManager.SelectedUnits[id];
          unit.WalkTo(ghost.transform.position);
        }

        Destroy(gameObject);
      }
    }

    public void InitializePreview()
    {
      var army = GameManager.SelectedUnits;
      if (army == null || !army.Any()) return;

      foreach (var (id, unit) in army)
      {
        var ghost = new GameObject();

        var renderer = ghost.AddComponent<SpriteRenderer>();
        renderer.sprite = unit.sprite;
        renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.47f); //120/255

        ghost.transform.parent = transform;
        ghost.transform.rotation = Quaternion.Euler(new Vector3(60, 0, 0));
        ghost.name = "Ghost";

        ghost.SetActive(false);
        Ghosts.Add(id, ghost);
      }

      var squadrons = DivideIntoSquadrons(Ghosts.Values.ToList());

      ArrangeSquadrons(squadrons, transform.position);
    }

    /// <summary>
    /// Shows the user a ghost preview of the position each troop will occupy once the command is given
    /// </summary>
    public void RotatePreview(Vector3 destination)
    {
      //TODO: rotate army to mouse position
    }

    /// <summary>
    /// All selected troops will try to attack the given target
    /// </summary>
    //2022-03-21 e.oliosi
    public void OrderAttack(Entity entity)
    {
      //logic here...
      //also add graphic & audio
      var army = GameManager.SelectedUnits;
      if (army == null || !army.Any()) return;

      foreach (var unit in army.Values)
      {
        unit.target = entity;
        unit.state = Skeleton.State.Attack;
      }
    }

    /// <summary>
    /// All selected troop will try to reach the position
    /// </summary>
    public void OrderMovement(Vector3 destination)
    {
      //logic here...
      //also add graphic & audio
    }

    /// <param name="unitSlotSize">space occupied by each unit</param>
    /// <param name="unitSlotSpacing">distance between each space</param>
    /// <param name="maxRows">Max number of rows in the squadron</param>
    private void ArrangeUnits(Squadron squadron, Vector3 center, float unitSpacing = 2f, float rowSpacing = 1f)
    {
      var maxRows = 2f;
      if (squadron.Units.Count <= 2) maxRows = 1; //I want squadrons with 2 units or less to be on a single row

      //Divide squadron into rows
      int maxUnitsEachRow = Mathf.CeilToInt(squadron.Units.Count / maxRows);
      var rows = new List<GameObject[]>();
      for (int i = 0; i < maxRows; i++)
      {
        rows.Add(squadron.Units.Skip(maxUnitsEachRow * i).Take(maxUnitsEachRow).ToArray());
      }

      //Place each unit in each row
      for (int y = 0; y < rows.Count; y++)
      {
        var row = rows[y];
        float rowCenter = (maxUnitsEachRow - 1) / 2;

        float offset = 0f;
        for (int x = 0; x < row.Length; offset += unitSpacing, x++)
        {
          float start = rowCenter - ((unitSpacing / 2) * (row.Count() - 1));
          var unit = row[x];

          var ghostPostition = new Vector3(center.x + start + offset, center.y, center.z + rowSpacing * y); //for the Y here you might want to raycast the terrain...
          unit.transform.position = ghostPostition;
          unit.SetActive(true);
        }
      }
    }

    private void ArrangeSquadrons(List<Squadron> squadrons, Vector3 center)
    {
      var unitSpacing = 1f;

      var maxSquadronRows = 2f;
      if (squadrons.Count <= 2) maxSquadronRows = 1; //I want squadrons with 2 units or less to be on a single row

      var totalUnits = squadrons.SelectMany(s => s.Units).Count();

      //Divide squadron into rows
      int maxSquadronsEachRow = Mathf.CeilToInt(squadrons.Count / maxSquadronRows);
      var squadronRows = new List<Squadron[]>();
      for (int i = 0; i < maxSquadronRows; i++)
      {
        squadronRows.Add(squadrons.Skip(maxSquadronsEachRow * i).Take(maxSquadronsEachRow).ToArray());
      }

      var offsetY = 0f;
      foreach (var squadronRow in squadronRows)
      {
        var startX = -((squadronRow.Sum(s => s.Width) / 2));
        var offsetX = 0f;

        foreach (var squadron in squadronRow)
        {
          var squadronCenterZ = (squadron.Height / 2) - (unitSpacing / 2);

          var start = new Vector3
          {
            x = center.x + startX + offsetX + (squadron.Width % 2 == 0 ? unitSpacing : unitSpacing / 2),
            y = center.y,
            z = center.z - squadronCenterZ + offsetY,
          };

          ArrangeUnits(squadron, start, unitSpacing);

          offsetX += squadron.Width;
        }

        offsetY += squadronRow.Max(s => s.Height);
      }
    }

    private List<Squadron> DivideIntoSquadrons(List<GameObject> units)
    {
      var squadrons = new List<Squadron>();

      if (units.Count <= 10)
      {
        squadrons.Add(new Squadron(units));
        return squadrons;
      }

      var squadronCount = Mathf.CeilToInt(units.Count / 10f);

      for (int i = 0; i < squadronCount; i++)
      {
        squadrons.Add(new Squadron(units.Skip(10 * i).Take(10).ToList()));
      }

      //make the last 2 squadrons the same size, to prevent the very last squadron to have too few skeletons
      var lastSquadrons = squadrons.Skip(Math.Max(0, squadrons.Count - 2));
      var squadronPrev = lastSquadrons.First();
      var squadronPost = lastSquadrons.Last();
      var lastSquadronSkeletons = squadronPost.Units.Count;
      if (lastSquadronSkeletons <= 8)
      {
        var remainingSkeletons = lastSquadrons.SelectMany(s => s.Units).Count();

        //The desired number of skeletons for the last 2 squadrons
        var skeletonsToHave = Mathf.FloorToInt(remainingSkeletons / 2f);
        var skeletonsToTake = skeletonsToHave - lastSquadronSkeletons;

        for (int i = 0; i < skeletonsToTake; i++)
        {
          squadronPost.Units.Add(squadronPrev.Units[i]);
          squadronPrev.Units.RemoveAt(i);
        }

        squadrons.Remove(squadrons.Last());
        squadrons.Remove(squadrons.Last());
        squadrons.Add(new Squadron(squadronPrev.Units));
        squadrons.Add(new Squadron(squadronPost.Units));
      }

      return squadrons;
    }
  }

  public class Squadron
  {
    private int MaxUnitsPerColumn = 2;
    private int MaxUnitsPerRow = 5;

    private int UnitsPerColumn = 0;
    private int UnitsPerRow = 0;

    public List<GameObject> Units = new List<GameObject>();

    public float Width => UnitsPerRow * 1f;
    public float Height => UnitsPerColumn * 1f;

    public Squadron(List<GameObject> skeletons)
    {
      if (skeletons.Count > MaxUnitsPerColumn * MaxUnitsPerRow)
      {
        throw new ArgumentException("Too many skeletons for this platoon.");
      }

      Units = skeletons;

      if (skeletons.Count <= 2)
      {
        UnitsPerColumn = 1;
        UnitsPerRow = skeletons.Count;
      }

      if (skeletons.Count > 2)
      {
        UnitsPerColumn = 2;
        UnitsPerRow = Mathf.CeilToInt(skeletons.Count / 2f);
      }
    }
  }
}
