using Assets.Scripts;
using Assets.Scripts.Player;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
  //private Animator[] animators;
  private Animator animator;
  private NavMeshAgent agent;
  public float movementSpeed;
  public bool IsMoving;
  public bool CanMove = true;

  private float horizontal;
  private float vertical;

  public void Start()
  {
    PlayerEntity.onPlayerStateChange = CheckPlayerState;

    //animators = GetComponentsInChildren<Animator>();
    animator = GetComponentInChildren<Animator>();
    agent = GetComponentInChildren<NavMeshAgent>();
  }

  public void Update()
  {
    if (!CanMove) return;

    horizontal = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
    vertical = Input.GetAxisRaw("Vertical") * movementSpeed * Time.deltaTime;

    var destination = new Vector3(transform.position.x + horizontal, transform.position.y, transform.position.z + vertical);
    var dir = destination - transform.position;

    agent.Move(dir);

    animator.SetFloat("Horizontal", horizontal);
    animator.SetFloat("Vertical", vertical);
  }

  public void CheckPlayerState(PlayerState state)
  {
    switch(state)
    {
      case PlayerState.Idle: CanMove = true; break;
      case PlayerState.Casting: CanMove = false; break;
      case PlayerState.Dying: CanMove = false; break;
    }
  }
}
