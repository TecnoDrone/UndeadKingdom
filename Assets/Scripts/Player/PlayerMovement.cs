using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
  //private Animator[] animators;
  private Animator animator;
  private NavMeshAgent agent;
  public float movementSpeed;
  public bool IsMoving;

  private float horizontal;
  private float vertical;

  public void Start()
  {
    //animators = GetComponentsInChildren<Animator>();
    animator = GetComponentInChildren<Animator>();
    agent = GetComponentInChildren<NavMeshAgent>();
  }

  public void Update()
  {
    //transform.position = new Vector3(transform.position.x + horizontal, transform.position.y, transform.position.z + vertical);
    //rigidbody.MovePosition(new Vector3(transform.position.x + horizontal, transform.position.y, transform.position.z + vertical));

    horizontal = Input.GetAxisRaw("Horizontal") * movementSpeed * Time.deltaTime;
    vertical = Input.GetAxisRaw("Vertical") * movementSpeed * Time.deltaTime;

    var destination = new Vector3(transform.position.x + horizontal, transform.position.y, transform.position.z + vertical);
    var dir = destination - transform.position;

    agent.Move(dir);// = agent.velocity.normalized + dir;// new Vector3(transform.position.x + horizontal, transform.position.y, transform.position.z + vertical);

    animator.SetFloat("Horizontal", horizontal);
    animator.SetFloat("Vertical", vertical);

    //IsMoving = x != 0 || z != 0;
    //for (var i = 0; i < animators.Length; animators[i].SetBool("Walking", IsMoving), i++) ;
  }
}
