using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour
{
    public State currentState;
    public CollisionData collisionData;
    public float currentMoveSpeed;
    public bool disableInput = false;
    public bool isCrouching;
    public bool isJumping;
    private float m_BaseMoveSpeed;
    private float[] m_MoveSpeedModifiers;
    [System.NonSerialized] public Transform cameraContainer;
    [System.NonSerialized] public CharacterController controller;
    private CharacterCollidierHelper helper;

    private Transform model;

    [Header("Input")]
    public Vector3 inputDir;
    public Vector3 rawInput;
    private Vector3 m_CameraInputEuler;

    [Header("Velocity")]
    public Vector3 velocity;
    public Vector3 externalVelocity;
    public State[] stateDrawer;
    
    private void Start() 
    {
        cameraContainer = transform.GetChild(0);
        model = transform.GetChild(1);

        helper = GetComponent<CharacterCollidierHelper>();
        controller = helper.controller;
        
        SetState(stateDrawer[0]);
    }

    public void SetState(State state)
    {
        if (currentState != null)
            currentState.OnExit();

        currentState = state;
        currentState.Init(this);

        if (currentState != null)
            currentState.OnEnter();
    }

    public void SetBaseMovementSpeed(float baseSpeed){
        m_BaseMoveSpeed = baseSpeed;
        currentMoveSpeed = baseSpeed;
    }

    private void Update()
    {
        rawInput = GetInput();
        inputDir = disableInput ? Vector3.zero : currentState.ProcessRawInput(transform, rawInput);
        collisionData = helper.collisionData;
        
        currentState.RotatePlayer(cameraContainer.eulerAngles, transform);
        model.rotation = transform.rotation;
        currentState.Tick();
        
        TemporaryInputListener();

        Vector3 inputVelocity = inputDir * currentMoveSpeed;
        currentState.Accelerate(inputDir, ref inputVelocity, currentMoveSpeed, currentState.acceleration);
        velocity = inputVelocity + externalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private Vector3 GetInput(){
        // Setup a input manager later
        return new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
    }

    private void TemporaryInputListener()
    {
        // Setup a input manager later
        if (Input.GetKeyDown("space"))
            currentState.Jump(ref externalVelocity);
        
        if(Input.GetKeyDown("c")) {
            IEnumerator crouchRoutine = null;
            currentState.Crouch(ref crouchRoutine);

            if(crouchRoutine != null)
                StartCoroutine(crouchRoutine);
        }
    }

    private void OnDrawGizmos()
    {
        if(!helper)
            return;

        // External Velocity
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + externalVelocity.normalized * 2);

        Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, transform.position + transform.rotation * new Vector3(m_RawInput.x, 0, m_RawInput.y).normalized * (controller.radius + controller.skinWidth + 0.01f));
        //Gizmos.DrawSphere(origin + inputDir.normalized * controller.skinWidth, controller.radius);
        Vector3 p1 = transform.position + controller.center + Vector3.up * -controller.height * 0.5F;
        Vector3 p2 = p1 + Vector3.up * controller.height;

        if(isCrouching)
            p2 = p1;
        
        Gizmos.DrawSphere(p1, 0.1f);
        Gizmos.DrawSphere(p2, 0.1f);
        // Desired input dir
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + inputDir.normalized);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + velocity.normalized);
    }
}
