using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : MonoBehaviour
{
    public Transform pivot;
    public float radius = 2.5f;
    public float maxSwingAngle = 45.0f;
    [Range(0, 3)]
    public float swingSpeed = 1.0f;
    public float coyoteLaunchSpeed = 10.0f;
    private CharacterMotor character;
    private Vector3 m_prevPos;
    private Vector3 m_launchVelocity;
    private float m_baseGravity = Physics.gravity.y;

    private void Start(){
        pivot.eulerAngles = transform.forward;
        m_prevPos = pivot.position + Vector3.down * radius;
        character = GetComponent<CharacterMotor>();
    }

    private void SimmulateSwing(){
        if(pivot == null)
            return;
        
        character.currentState.isKinematic = true;
        RotatePivot();

        // Simple harmonic motion
        float harmonicMotion = maxSwingAngle * Mathf.Sin(Time.time * swingSpeed);
        character.externalVelocity = pivot.forward * harmonicMotion;
        character.externalVelocity.y = m_baseGravity;

        float length = Vector3.Distance(transform.position, pivot.position);
        Vector3 grappleDir = Vector3.Normalize(pivot.position - transform.position);
        if(length > radius){
            transform.position = pivot.position - grappleDir * radius;
        }

        float speed = Vector3.Distance(m_prevPos, transform.position) / Time.deltaTime;
        if(speed < coyoteLaunchSpeed){
            speed = coyoteLaunchSpeed;
        }

        m_launchVelocity = Vector3.Normalize(transform.position - m_prevPos) * speed;
        m_prevPos = transform.position;
    }

    private void UnGrapple(){
        pivot = null;
        character.externalVelocity = m_launchVelocity;
        character.currentState.isKinematic = false;
        character.disableInput = false;
    }

    void LateUpdate()
    {
        SimmulateSwing();

        if (Input.GetMouseButtonDown(0)){
            UnGrapple();
        }
    }

    private void RotatePivot() {
        character.disableInput = true;
        pivot.eulerAngles += new Vector3(0,character.rawInput.x,0) * 0.05f;
    }

    private void OnDrawGizmos(){
        Color color = Color.yellow;
        color.a = 0.1f;
        
        Gizmos.color = color;
        Gizmos.DrawLine(transform.position, pivot.position);
        Gizmos.DrawSphere(pivot.position, radius);

        Color color2 = Color.cyan;
        Gizmos.color = color2;
        Gizmos.DrawLine(transform.position, transform.position + m_launchVelocity);
    }
}
