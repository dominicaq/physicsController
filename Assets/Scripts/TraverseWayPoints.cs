using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraverseWayPoints : MonoBehaviour
{
    public enum CycleMode{
        Once,
        Loop,
    };

    public float speed = 1;
    public CycleMode cycleMode = CycleMode.Once;
    public float cyclePauseTime = 2.5f;
    public Transform[] points;
    private Vector3 destination;
    private int index = 0;
    private bool goForward = true;
    private bool canContinue = true;
    private Rigidbody body;

    private void Start() {
        destination = points[0].position;
        body = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {      
        Vector3 smoothDelta = Vector3.MoveTowards(transform.position, destination, Time.smoothDeltaTime * speed);
        body.MovePosition(smoothDelta);

        if(Vector3.Distance(transform.position, destination) < 0.001f && canContinue)
        {
            if(goForward)
                GoForward();
            else
                GoBackward();
        }
    }

    private void GoForward()
    {
        if(index < points.Length)
        {
            destination = points[index].position;
            index++;
        }
        else if(cycleMode == CycleMode.Loop)
        {
            goForward = false;
            StartCoroutine(Counter());
        }
    }

    private void GoBackward()
    {
        if(index > 0)
        {
            index--;
            destination = points[index].position;
        }
        else
        {
            goForward = true;
            StartCoroutine(Counter());
        }
    }

    private IEnumerator Counter()
    {
        canContinue = false;
        yield return new WaitForSeconds(cyclePauseTime);
        canContinue = true;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);

        // Way points
        Gizmos.color = Color.green;
        for(int i = 0; i < points.Length; i++)
        {
            if(points[i] == null)
                return;

            Gizmos.DrawSphere(points[i].position, 0.1f);

            if(i < points.Length-1)
                Gizmos.DrawLine(points[i].position, points[i+1].position);
        }
    }
}
