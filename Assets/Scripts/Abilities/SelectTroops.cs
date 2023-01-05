using Assets.Scripts.Abilities;
using Assets.Scripts.AI;
using Assets.Scripts.Managers;
using Assets.Scripts.Player;
using System.Collections;
using UnityEngine;

/***********************************************************************
 * Author            : e.oliosi
 * Date created      : 2021-10-01
 * Purpose           : 
 * https://medium.com/codex/making-a-rts-game-4-selecting-units-unity-c-1c823b6823a5
 * *********************************************************************/
namespace Assets.Scripts.Spells
{
  public class SelectTroops : MonoBehaviour
  {
    public KeyCode key;
    public PlayerStance state;

    private Vector3 DragStartPosition;

    private Texture2D _whiteTexture;
    public Texture2D WhiteTexture
    {
      get
      {
        if (_whiteTexture == null)
        {
          _whiteTexture = new Texture2D(1, 1);
          _whiteTexture.SetPixel(0, 0, Color.white);
          _whiteTexture.Apply();
        }

        return _whiteTexture;
      }
    }

    private Coroutine selecting;

    private void Start()
    {
      PlayerEntity.onPlayerStanceChange += CheckPlayerStance;
    }

    private void Update()
    {
      if (PlayerEntity.Instance.Stance != state) return;

      if (Input.GetKeyDown(key) && PlayerEntity.Instance.State == PlayerState.Idle)
      {
        DeselectAll();

        DragStartPosition = Input.mousePosition;
        selecting = StartCoroutine(Selecting());

        PlayerEntity.Instance.State = PlayerState.Casting;
        PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Casting);
      }

      if (Input.GetKeyUp(key) && selecting != null)
      {
        Stop();
      }
    }

    private void CheckPlayerStance()
    {
      if (PlayerEntity.Instance.Stance != state)
      {
        Stop();
        DeselectAll();
      }
    }

    private void Stop()
    {
      if (selecting != null)
      {
        StopCoroutine(selecting);
        selecting = null;

        PlayerEntity.Instance.State = PlayerState.Idle;
        PlayerEntity.onPlayerStateChange.Invoke(PlayerState.Idle);
      }
    }

    IEnumerator Selecting()
    {
      while (true)
      {
        var selectionBounds = GetViewportBounds(DragStartPosition, Input.mousePosition);
        var selectableUnits = PlayerEntity.ControlledMinions;

        foreach (var unit in selectableUnits)
        {
          if (unit == null) continue;
          var inBounds = selectionBounds.Contains(Camera.main.WorldToViewportPoint(unit.transform.position));

          if (inBounds)
          {
            Select(unit);
          }
          else
          {
            Deselect(unit);
          }
        }

        yield return null;
      }
    }

    private void OnGUI()
    {
      if (selecting != null)
      {
        // Create a rect from both mouse positions
        var rect = GetScreenRect(DragStartPosition, Input.mousePosition);
        DrawScreenRect(rect, new Color(0.5f, 1f, 0.4f, 0.2f));
        DrawScreenRectBorder(rect, 1, new Color(0.5f, 1f, 0.4f));
      }
    }

    private void Select(CombatAI AIs)
    {
      if (AIs == null) return;

      int id = AIs.GetInstanceID();
      if (GameManager.SelectedUnits.ContainsKey(id)) return;
      GameManager.SelectedUnits.Add(id, AIs);

      var selection = AIs.transform.Find("Selection");
      if (selection != null)
      {
        selection.gameObject.SetActive(true);
      }
    }

    //2023-01-04 e.oliosi - TODO: stop using GameManager, replace with local list of
    private void DeselectAll()
    {
      foreach (var skeleton in GameManager.SelectedUnits.Values)
      {
        if (skeleton == null) continue;

        var selection = skeleton.transform.Find("Selection");
        if (selection != null)
        {
          selection.gameObject.SetActive(false);
        }
      }

      GameManager.SelectedUnits.Clear();
    }

    private void Deselect(CombatAI skeleton)
    {
      if (skeleton == null) return;

      int id = skeleton.GetInstanceID();
      if (!GameManager.SelectedUnits.ContainsKey(id)) return;
      GameManager.SelectedUnits.Remove(id);

      var selection = skeleton.transform.Find("Selection");
      if (selection != null)
      {
        selection.gameObject.SetActive(false);
      }
    }

    private Bounds GetViewportBounds(Vector3 screenPosition1, Vector3 screenPosition2)
    {
      var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
      var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
      var min = Vector3.Min(v1, v2);
      var max = Vector3.Max(v1, v2);
      min.z = Camera.main.nearClipPlane;
      max.z = Camera.main.farClipPlane;

      var bounds = new Bounds();
      bounds.SetMinMax(min, max);
      return bounds;
    }

    private void DrawScreenRect(Rect rect, Color color)
    {
      GUI.color = color;
      GUI.DrawTexture(rect, WhiteTexture);
      GUI.color = Color.white;
    }

    private void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
      // Top
      DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
      // Left
      DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
      // Right
      DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
      // Bottom
      DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    private Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
      // Move origin from bottom left to top left
      screenPosition1.y = Screen.height - screenPosition1.y;
      screenPosition2.y = Screen.height - screenPosition2.y;
      // Calculate corners
      var topLeft = Vector3.Min(screenPosition1, screenPosition2);
      var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
      // Create Rect
      return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }
  }
}