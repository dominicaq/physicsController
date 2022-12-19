using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public int resolution = 5;
    public float maxHeight = 5;
    public float gravity = Physics.gravity.y;
    public Transform target;
    private LaunchData runtimeData;

    private void Start() {
        runtimeData = CalculateLaunchData();
    }

    private LaunchData CalculateLaunchData()
    {
        Vector3 displacementXZ = target.position - transform.position;
        float displacementY = displacementXZ.y;
        displacementXZ.y = 0;

        float time = Mathf.Sqrt(-2 * maxHeight/gravity) + Mathf.Sqrt(2*(displacementY- maxHeight)/gravity);
        Vector3 Vy = Vector3.up * Mathf.Sqrt(-2 * gravity * maxHeight);
        Vector3 Vxz = displacementXZ / time;
        Vector3 initalVelocity = Vxz + Vy * -Mathf.Sign(gravity);
        return new LaunchData(initalVelocity, time);
    }

    private void OnTriggerStay(Collider other) 
    {
        if(other.attachedRigidbody)
            other.attachedRigidbody.velocity = runtimeData.initialVelocity;

        CharacterMotor character = other.GetComponent<CharacterMotor>();
        
        if(character) {
            character.externalVelocity = runtimeData.initialVelocity;
        }
    }

    private void OnDrawGizmos() 
    {
        LaunchData launchData = CalculateLaunchData();
		Vector3 previousDrawPoint = transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(DrawPoint(launchData, launchData.timeToTarget/2), 0.5f);
        Gizmos.color = Color.green;
		for (int i = 1; i <= resolution; i++) 
        {
			float t = i / (float)resolution * launchData.timeToTarget;
            Vector3 point = DrawPoint(launchData, t);
			Gizmos.DrawLine(previousDrawPoint, point);
			previousDrawPoint = point;
		}
    }

    private Vector3 DrawPoint(LaunchData data, float t)
    {
        Vector3 displacement = data.initialVelocity * t + Vector3.up * gravity * t * t / 2f;
	    return transform.position + displacement;
    }

    struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget){
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;            
        }
    }

}
