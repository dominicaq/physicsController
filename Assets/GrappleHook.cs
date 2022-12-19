using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    [Header("Visuals")]
    public Transform hookPrefab;
    private Transform m_hookPrefab;
    public Transform player;

    [Header("Grappling")]
    public float throwLength = 5.0f;
    public float reelSpeed = 5.0f;
    public bool isGrappling = false;
    private CharacterMotor character;
    private Vector3 m_pivot;
    private float m_initRadius;
    private float m_minRadius;
    private Vector3 m_launchVelocity;
    private Vector3 m_prevPosition;

    [Header("Swinging")]
    public float swingRadius = 2.5f;
    public float swingAngle = 45.0f;
    [Range(0, 3)]
    public float swingSpeed = 1.0f;
    public float swingCoyoteLaunchSpeed = 10.0f;
    private Transform m_swingObject;
    private bool m_isSwinging = false;

    void Start()
    {
        character = player.GetComponent<CharacterMotor>();
    }

    private void LaunchPivot(){
        Vector3 cameraDir = character.cameraContainer.forward;
        if(Physics.Raycast(player.position, cameraDir, out RaycastHit hit, throwLength)){
            character.currentState.isKinematic = true;
            isGrappling = true;
            m_prevPosition = player.position;

            // Swing
            if(hit.transform.gameObject.tag == "Swingable") {
                m_swingObject = hit.transform;
                m_pivot = hit.transform.position;
                m_isSwinging = true;
                character.disableInput = true;
                return;
            }

            // Grapple pull
            m_pivot = hit.point;
            m_initRadius = Vector3.Distance(player.position, m_pivot);
            m_minRadius = m_initRadius/4;

            // Hook
            m_hookPrefab = Instantiate(hookPrefab, m_pivot, Quaternion.identity);
            m_hookPrefab.rotation = Quaternion.FromToRotation(m_hookPrefab.up, hit.normal);
        }
    }

    private void SimmulateGrapple(){
        float currentDistance = Vector3.Distance(player.position, m_pivot);
        Vector3 grappleDir = Vector3.Normalize(m_pivot - player.position);
        character.externalVelocity += grappleDir * (Time.deltaTime * reelSpeed);

        // Enforce Sphereical motion
        if(currentDistance >= m_initRadius){
            player.position = m_pivot - grappleDir * m_initRadius;
        }

        float speed = Vector3.Distance(m_prevPosition, player.position) / Time.deltaTime;
        m_launchVelocity = Vector3.Normalize(player.position - m_prevPosition) * speed;
        m_prevPosition = player.position;

        if(IsObstructed(grappleDir) || currentDistance > m_initRadius || currentDistance < m_minRadius){
            Release();
        }
    }

    private void SimmulateSwing(){
        // Rotate Pivot
        m_swingObject.eulerAngles += new Vector3(0,character.rawInput.x,0) * 0.05f;

        // Simple harmonic motion
        float harmonicMotion = swingAngle * Mathf.Sin(Time.time * swingSpeed);
        character.externalVelocity = m_swingObject.forward * harmonicMotion;
        character.externalVelocity.y = Physics.gravity.y;

        float currentLength = Vector3.Distance(player.position, m_pivot);
        Vector3 grappleDir = Vector3.Normalize(m_pivot - player.position);
        if(currentLength > swingRadius){
            player.position = m_pivot - grappleDir * swingRadius;
        }

        float speed = Vector3.Distance(m_prevPosition, player.position) / Time.deltaTime;
        if(speed < swingCoyoteLaunchSpeed){
            speed = swingCoyoteLaunchSpeed;
        }

        m_launchVelocity = Vector3.Normalize(player.position - m_prevPosition) * speed;
        m_prevPosition = player.position;
    }    

    // Update is called once per frame
    void LateUpdate()
    {
        if(Input.GetMouseButtonDown(0)){
            LaunchPivot();
        }

        if(isGrappling && !m_isSwinging){
            SimmulateGrapple();
        }

        if(m_isSwinging)
            SimmulateSwing();
    }

    private bool IsObstructed(Vector3 grappleDir){
        Vector3 floatingPivot = m_pivot - grappleDir * 0.99f;
        return Physics.Linecast(player.position, floatingPivot, ~0, QueryTriggerInteraction.Ignore);
    }

    private void Release(){
        isGrappling = false;
        m_pivot = Vector3.zero;
        character.currentState.isKinematic = false;
        character.disableInput = false;
        character.externalVelocity = m_launchVelocity;
        Destroy(m_hookPrefab.gameObject);
    }

    private void OnDrawGizmos() {
        if(m_pivot == Vector3.zero)
            return;

        Gizmos.DrawSphere(m_pivot, 0.1f);
        Gizmos.DrawLine(player.position, m_pivot);

        Color minColor = Color.cyan;
        minColor.a = 0.2f;
        Gizmos.color = minColor;
        Gizmos.DrawLine(player.position, player.position + m_launchVelocity);
        Gizmos.DrawSphere(m_pivot, m_minRadius);

        Color maxColor = Color.green;
        maxColor.a = 0.1f;
        Gizmos.color = maxColor;
        Gizmos.DrawSphere(m_pivot, m_initRadius);
    }
}
