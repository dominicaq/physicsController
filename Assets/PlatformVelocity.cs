using UnityEngine;

public class PlatformVelocity : MonoBehaviour
{
    public Transform platform;
    public Vector3 platformVelocity;
    private Vector3 m_prevPosition;
    private CharacterMotor m_character;
    private Rigidbody m_body;

    private void Awake() {
        m_prevPosition = platform.position;
        m_body = platform.GetComponent<Rigidbody>();
    }

    private void Update() 
    {
        platformVelocity = m_body.velocity;
        if(platformVelocity.y > -0.5f)
            platformVelocity.y = -0.5f;
        
        UpdateCharacterVelocity();
        m_prevPosition = platform.position;
    }

    private void UpdateCharacterVelocity(){
        if(!m_character)
            return;

        // Could be replaced with isJumping when implemented
        if(m_character.externalVelocity.y > Mathf.Abs(platformVelocity.y) + 1f){
            m_character.currentState.isKinematic = false;
            return;
        }

        Debug.DrawLine(m_character.transform.position, m_character.transform.position + Vector3.down * (m_character.collisionData.halfCapsuleDistance + m_character.controller.skinWidth), m_character.collisionData.isGrounded ? Color.green : Color.red, 1);
        m_character.currentState.isKinematic = m_character.collisionData.isGrounded;        
        if(m_character.collisionData.isGrounded){
            m_character.externalVelocity = platformVelocity;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        m_character = other.GetComponent<CharacterMotor>();
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject != m_character.gameObject)
            return;
        
        m_character.currentState.isKinematic = false;
        m_character = null;
    }
}
