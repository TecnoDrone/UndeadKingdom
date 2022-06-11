using Assets.Scripts;
using Assets.Scripts.AI;
using Extentions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static HashSet<Entity> DeadEntities = new HashSet<Entity>();

  public static Dictionary<EntityAI, Entity> FightersTargets = new Dictionary<EntityAI, Entity>();
  public static Dictionary<int, Skeleton> SelectedUnits = new Dictionary<int, Skeleton>();

  public static Dictionary<string, GameObject> CachedSpells = new Dictionary<string, GameObject>();

  public static GameState gameState = GameState.Running;
  public GameObject WinScreen;
  public GameObject Menu;

  public enum GameState
  {
    Running,
    Paused
  }

  private void Update()
  {
    if (Input.GetKeyUp(KeyCode.Escape))
    {
      if (gameState == GameState.Running)
      {
        PauseGame();
        Menu.SetActive(true);
      }
      else
      {
        UnpauseGame();
        Menu.SetActive(false);
      }
    }
  }

  private void LateUpdate()
  {
    //Clear dead entities from targets
    if (FightersTargets.Any())
    {
      var deepCopy = FightersTargets.ToDictionary(x => x.Key, x => x.Value);
      foreach (var (skeleton, target) in deepCopy)
      {
        if (DeadEntities.Contains(target))
        {
          skeleton.target = null;
          FightersTargets.Remove(skeleton);
        }
      }
    }

    //Clear dead entities from selected units
    if (SelectedUnits.Any())
    {
      var deepCopy = SelectedUnits.ToDictionary(x => x.Key, x => x.Value);
      foreach (var (skeleton, target) in deepCopy)
      {
        if (DeadEntities.Contains(target))
        {
          SelectedUnits.Remove(skeleton);
        }
      }
    }

    foreach (var entity in DeadEntities)
    {
      if (entity.name == "King")
      {
        WinGame();
      }

      Destroy(entity.gameObject);
    }

    DeadEntities.Clear();
  }

  public void WinGame()
  {
    PauseGame();
    WinScreen.SetActive(true);
  }

  public void PauseGame()
  {
    Time.timeScale = 0;
    gameState = GameState.Paused;
    Player.audio.enabled = false;
  }

  public void UnpauseGame()
  {
    Time.timeScale = 1;
    gameState = GameState.Running;
    Player.audio.enabled = true;
  }

  public static GameObject GetSpellFromCache(string spellKind)
  {
    if (!CachedSpells.TryGetValue(spellKind, out GameObject cachedSpell))
    {
      cachedSpell = (CachedSpells[spellKind] = (GameObject)Resources.Load("Prefabs/Spells/" + spellKind));
    }

    return cachedSpell;
  }
}
