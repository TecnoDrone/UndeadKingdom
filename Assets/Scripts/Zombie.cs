using System.Collections;
using UnityEngine;

/***********************************************************************
 * Author            : e.oliosi
 * Date created      : 2020-10-26
 * Purpose           : Zombie behaviour. Will follow parent.
 * *********************************************************************/
public class Zombie : MonoBehaviour
{
    public int maxFreeWill = 10;
    public int currentFreeWill;

    public float movementSpeed = 0.01f;
    public float waitingTime;
    public float wanderRange;

    private Vector2? wanderDestination; //Will try to reach this position if wandering near its master
    private Vector2? anchorPosition; //Will wander nearby this position if masterless
    private bool waiting; //State to determine if the zombie is currently not moving
    private SpriteRenderer spriteRenderer;

    public GameObject Master = null;
    //{
        //get => transform.parent != null ? transform.parent.gameObject : null;
        //set => transform.SetParent(value.transform);
    //}
    private bool HasMaster => Master != null;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentFreeWill = maxFreeWill;
    }

    public void Update()
    {
        if (HasMaster)
        {
            if(anchorPosition != null)
            {
                anchorPosition = null;
            }

            var distanceToMaster = Vector2.Distance(Master.transform.position, transform.position);
            if (distanceToMaster > wanderRange)
            {
                RunToMaster();
            }
            else
            {
                Wander();
            }
        }
        else
        {
            if(anchorPosition == null)
            {
                anchorPosition = transform.position;
            }

            Wander();
        }
    }

    public void SetFree()
    {
        transform.parent = null;
    }

    public int DamageWill(int damage, Color attackerColor)
    {
        currentFreeWill -= damage;

        UpdateColorToWill(attackerColor);

        return currentFreeWill;
    }

    private void UpdateColorToWill(Color color)
    {
        var currentColor = spriteRenderer.color;
        var destinationColor = color;
        var newColor = Color.Lerp(currentColor, destinationColor, (float)1/currentFreeWill);

        spriteRenderer.color = newColor;
    }

    #region behaviour

    private void RunToMaster()
    {
        wanderDestination = null;

        var masterPosition = Master.transform.position;
        var currentPosition = transform.position;

        transform.position = Vector2.MoveTowards(currentPosition, masterPosition, movementSpeed * Time.deltaTime);
    }

    private void Wander()
    {
        //If has to move towards a destination
        if (!waiting)
        {
            //If has reached destination, calculate a new one
            if (wanderDestination == null)
            {
                var anchor = Master == null ? anchorPosition : Master.transform.position;

                wanderDestination = Random.insideUnitCircle * wanderRange + anchor;
            }

            //Calculate distance to destination
            var currentPosition = transform.position;
            var distanceToWanderPosition = Vector2.Distance(currentPosition, wanderDestination.Value);

            //If destination has yet to be reached, move towards destination
            if (distanceToWanderPosition > movementSpeed * Time.deltaTime)
            {
                transform.position = Vector2.MoveTowards(currentPosition, wanderDestination.Value, movementSpeed / 2 * Time.deltaTime);
            }
            //If destination has been reached, wait for Xseconds
            else
            {
                StartCoroutine(Wait());
                wanderDestination = null;
            }
        }
    }

    IEnumerator Wait()
    {
        waiting = true;
        yield return new WaitForSeconds(waitingTime);
        waiting = false;
    }

    #endregion

    public void OnDrawGizmos()
    {
        var anchor = Master == null ? anchorPosition : Master.transform.position;

        if (anchor.HasValue)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(anchor.Value, 0.1f);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(anchor.Value, wanderRange);
        }

        if (wanderDestination != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(wanderDestination.Value, 0.1f);
        }
    }
}