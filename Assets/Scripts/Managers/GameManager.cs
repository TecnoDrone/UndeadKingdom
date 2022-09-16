using Assets.Scripts.AI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Managers
{
  public class GameManager : MonoBehaviour
  {
    public static HashSet<Entity> DeadEntities = new HashSet<Entity>();

    public static Dictionary<EntityAI, Entity> FightersTargets = new Dictionary<EntityAI, Entity>();
    public static Dictionary<int, CombatAI> SelectedUnits = new Dictionary<int, CombatAI>();

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
            if (skeleton != null)
            {
              skeleton.ResetToDefaultState();
              skeleton.target = null;
            }

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
      Player.listener.enabled = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    public void LoadLevel(string level)
    {
      SceneManager.LoadScene(level);
    }

    public void UnpauseGame()
    {
      Time.timeScale = 1;
      gameState = GameState.Running;
      Player.listener.enabled = true;
    }
  }
}