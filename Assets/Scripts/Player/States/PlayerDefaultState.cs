using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "PlayerDefaultState", menuName = "ScriptableObjects/Player/States/Default", order = 1)]
public class PlayerDefaultState : State
{
    [Header("General Movement")]
    public float movementSpeed = 5;
    public float crouchMoveSpeed = 2;
    [Range(0, 1)] public float airControl = 0.95f;
    public int maximumJumps = 1;
    public float jumpHeight = 2.5f;
    private int m_JumpCounter = 0;

    #region Crouch

    [Header("Crouching")]
    public float crouchTime = 0.5f;
    private float m_InitCenter;
    private float m_InitHeight;
    private float m_CrouchCenter;
    private float m_CrouchHeight;
    private bool m_CrouchReady;
    private bool m_CrouchQueue;

    #endregion

    [Header("Physics")]
    public float maxFallSpeed = 25.0f;
    public float gravity = Physics.gravity.y;
    public float slideFriction = 0.3f;
    public static Vector3 groundedVelocity = new Vector3(0,-0.5f,0);

    public override void OnEnter() 
    {
        isKinematic = false;
        character.SetBaseMovementSpeed(movementSpeed);

        // Crouching data
        m_InitCenter = character.controller.center.y;
        m_InitHeight = character.controller.height;
        m_CrouchHeight = character.controller.height/2;
        m_CrouchCenter = -character.controller.height/4;
        m_CrouchReady = true;
    }

    public override void OnExit() { }

    public override void Tick()
    {
        HandlePhysics(ref character.externalVelocity);
        character.inputDir = VelocityParrellelToSurface(character.inputDir);
        character.inputDir = InputParrallelToGround(character.inputDir, character.collisionData.hit);
        //SlopeSliding(ref character.inputDir, character.collisionData.hit);
        //AirMove();

        if(character.collisionData.isGrounded){
            m_JumpCounter = 0;
        }
    }

    private void HandlePhysics(ref Vector3 velocity)
    {
        if(isKinematic)
            return;

        bool conditionalExceptions = !character.collisionData.onJumpPad;
        if(character.collisionData.isGrounded && conditionalExceptions) {
            velocity = groundedVelocity;
        } else {
            if(velocity.magnitude > jumpHeight){
                velocity = VelocityParrellelToSurface(velocity);
            }
            
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Clamp(velocity.y, -maxFallSpeed, Mathf.Infinity);
        }

        // Headhit
        if(character.collisionData.headHit && velocity.y > 0){
            velocity = groundedVelocity;
        }
    }

    // Makes user input parrellel to surface 
    private Vector3 InputParrallelToGround(Vector3 inputDir, ControllerColliderHit hit)
    {
        if(hit == null || !character.collisionData.isCenterGrounded)
            return inputDir;
        
        Vector3 adjustedDir = Vector3.ProjectOnPlane(inputDir, hit.normal).normalized;
        if(adjustedDir.y < 0 && !character.collisionData.headHit) {
            return adjustedDir * inputDir.magnitude;
        }

        return inputDir;
    }

    private Vector3 VelocityParrellelToSurface(Vector3 velocity)
    {            
        float extend = 1.01f;
        float length = character.controller.skinWidth * extend;
        float radius = character.controller.radius * extend;
        Vector3 newVelocity = velocity;
        Vector3 p1 = character.transform.position + character.controller.center + Vector3.up * -character.controller.height * 0.5f; // bottom
        Vector3 p2 = p1 + Vector3.up * character.controller.height; // top

        if(character.isCrouching)
            p2 = p1;
        
        if(Physics.CapsuleCast(p1, p2, radius, newVelocity.normalized, out RaycastHit hit, length, ~0 , QueryTriggerInteraction.Ignore)) {
            // Make input parrellel to surface normal
            Vector3 temp = Vector3.Cross(hit.normal, newVelocity);
            newVelocity = Vector3.Cross(temp, hit.normal);

            // If the newdir still goes into the wall, player is in a corner and input can be zero
            if(Physics.CapsuleCast(p1, p2, radius, newVelocity.normalized, out RaycastHit tmp, length, ~0 , QueryTriggerInteraction.Ignore)){
                newVelocity = Vector3.zero;
            }
        }

        return newVelocity;
    }

    // TODO: Rework
    private void SlopeSliding(ref Vector3 inputVector, ControllerColliderHit hit)
    {
        if(hit == null)
            return;
        
        Vector3 hitNormal = hit.normal;
        if(character.collisionData.onSlope) {
            inputVector.x += (1f - hitNormal.y) * hitNormal.x * (1f - slideFriction) * 5;
            inputVector.z += (1f - hitNormal.y) * hitNormal.z * (1f - slideFriction) * 5;
        }
    }

    public override void Jump(ref Vector3 velocity)
    {
        if(character.isCrouching || character.collisionData.onSlope || (maximumJumps == 1 && !character.collisionData.isGrounded))
            return;

        m_JumpCounter += 1;
        if (m_JumpCounter <= maximumJumps) {            
            velocity.y += Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
        }
    }

    public override void Crouch(ref IEnumerator routine)
    {
        if(m_CrouchReady && character.collisionData.isGrounded && character.collisionData.headClearance) {
            character.isCrouching = !character.isCrouching;
            routine = CrouchRoutine(crouchTime);
        }
    }
    
    private IEnumerator CrouchRoutine(float time)
    {
        m_CrouchReady = false;
        float elapsedTime = 0;

        Vector3 startingCenter = character.controller.center;
        float startingHeight = character.controller.height;

        float desiredHeight = !character.isCrouching ? m_InitHeight : m_CrouchHeight;
        float desiredCenterY = !character.isCrouching ? m_InitCenter : m_CrouchCenter;
        Vector3 desiredCenter = new Vector3(0, desiredCenterY, 0);

        while (elapsedTime < time + 0.1f) {
            character.controller.height = Mathf.Lerp(startingHeight, desiredHeight, (elapsedTime / time));
            character.controller.center = Vector3.Lerp(startingCenter, desiredCenter, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        m_CrouchReady = true;
    }
}
