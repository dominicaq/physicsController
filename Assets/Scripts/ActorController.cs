using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    public Vector3 currentVelocity;
    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Gravity(ref currentVelocity);
        characterController.Move(currentVelocity * Time.deltaTime);
    }

    void InputVelocity()
    {

    }

    void Gravity(ref Vector3 velocity)
    {
        if(characterController.isGrounded)
        {
            velocity.y = Mathf.Epsilon;
        }
        else
            velocity.y += Physics.gravity.y * Time.deltaTime;
    }
}
