using UnityEngine;

public class ImpulseForce : MonoBehaviour
{
    public float forceMultiplier = 1.0f;
    private CharacterMotor character;
    private GameObject gChar;

    private void OnTriggerStay(Collider other) {
        character = other.GetComponent<CharacterMotor>();

        if(character != null) {
            gChar = other.gameObject;
            Vector3 velocity = (transform.position - other.transform.position).normalized * forceMultiplier;
            character.externalVelocity -= velocity;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject == gChar)
            gChar = null;
    }
}
