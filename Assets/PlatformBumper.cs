using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBumper : MonoBehaviour
{
    private CharacterMotor m_character;
    private Rigidbody m_body;

    void Start()
    {
        m_body = transform.parent.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(!m_character)
            return;
        
        Vector3 velocity = m_body.velocity;
        m_character.externalVelocity.x = velocity.x;
        m_character.externalVelocity.z = velocity.z;
    }

    private void OnTriggerStay(Collider other) {
        Vector3 velocity = Vector3.Normalize(m_body.velocity);
        Vector3 playerDir = Vector3.Normalize(other.transform.position - transform.position);

        float dot = Vector3.Dot(velocity, playerDir);
        if(dot > 0.7f){
            m_character = other.GetComponent<CharacterMotor>();
            m_character.currentState.isKinematic = true;
        } else if(m_character){
            m_character.currentState.isKinematic = false;
            m_character = null;            
        }
    }

    private void OnTriggerExit(Collider other) {
        if(m_character && other.gameObject == m_character.gameObject){
            m_character.currentState.isKinematic = false;
            m_character = null;
        }
    }
}
