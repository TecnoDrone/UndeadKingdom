using Assets.Scripts.AI;
using Assets.Scripts.AI.Undead;
using Assets.Scripts.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Managers
{
  public enum GameState
  {
    Running,
    Paused
  }

  public class GameManager : MonoBehaviour
  {
    public static GameState gameState = GameState.Running;

    public Texture2D combatCursor;
    public Texture2D commandCursor;
    public GameObject WinScreen;
    public GameObject Menu;

    private void Start()
    {
      PlayerEntity.onPlayerStanceChange += ChangeCursor;

      Cursor.SetCursor(combatCursor, new Vector2(combatCursor.width / 2, combatCursor.height / 2), CursorMode.Auto);
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
      //TODO: Remove this and invoke event from King script
      //foreach (var entity in DeadEntities)
      //{
      //  if (entity.name == "King")
      //  {
      //    WinGame();
      //  }
      //
      //  Destroy(entity.gameObject);
      //}
    }

    public void ChangeCursor()
    {
      if(PlayerEntity.Instance.Stance == PlayerStance.Combat)
      {
        Cursor.SetCursor(combatCursor, new Vector2(combatCursor.width / 2, combatCursor.height / 2), CursorMode.Auto);
      }
      else if(PlayerEntity.Instance.Stance == PlayerStance.Command)
      {
        Cursor.SetCursor(commandCursor, Vector2.zero, CursorMode.Auto);
      }
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
      PlayerEntity.listener.enabled = false;
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
      PlayerEntity.listener.enabled = true;
    }
  }
}