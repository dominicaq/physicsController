using UnityEngine;

public struct CollisionData
{
    public bool onSlope;
    public bool onPlatform;
    public bool onJumpPad;
    public bool isGrounded;
    public bool isCenterGrounded;
    public bool headClearance;
    public bool headHit;
    public float halfCapsuleDistance;
    public Vector3 feetPosition;
    public Vector3 feetScale;
    public RaycastHit centerHit;
    public ControllerColliderHit hit;
};

public class CharacterCollidierHelper : MonoBehaviour
{
    public CollisionData collisionData;
    public CharacterController controller;
    private Vector3 m_previousPosition;
    private bool m_wasOnPlatform;
    // World interactions
    private ControllerColliderHit m_controllerHit;
    private GameObject m_triggerHit;

    private void Start() {
        controller = GetComponent<CharacterController>();
        m_previousPosition = transform.position;
    }
    
    private void Update()
    {
        collisionData = GetColliderData();
        m_controllerHit = null;
    }

    private CollisionData GetColliderData()
    {
        CollisionData data = new CollisionData();
        Vector3 feetScale = transform.localScale * 0.99f;
        feetScale.y = feetScale.y / 100;
        
        float halfCapsule = transform.localScale.y / 2;
        Vector3 footPlacement = transform.position;
        footPlacement.y = footPlacement.y - halfCapsule;

        data.feetScale = feetScale;
        data.halfCapsuleDistance = Vector3.Distance(transform.position, footPlacement) + controller.skinWidth;
        data.feetPosition = footPlacement;

        data.headClearance = HasHeadClearance();
        data.isCenterGrounded = IsCentered(ref collisionData.centerHit);

        // For vertical platform stuff
        Vector3 dir = Vector3.Normalize(transform.position - m_previousPosition);
        float footBuffer = dir.y < 0 && m_wasOnPlatform ? 0.1f : 0f;
        data.onPlatform = Physics.Raycast(transform.position, Vector3.down, collisionData.halfCapsuleDistance + footBuffer, 1<<6, QueryTriggerInteraction.Ignore);
        data.isGrounded = data.onPlatform || controller.isGrounded;

        // Collider Hits
        if(m_controllerHit != null) {
            data.hit = m_controllerHit;
            data.onSlope = (Vector3.Angle(Vector3.up, m_controllerHit.normal) >= controller.slopeLimit) && !(Vector3.Angle(Vector3.up, m_controllerHit.normal) >= 85);    
            data.headHit = m_controllerHit.point.y >= transform.position.y;
        }

        // Trigger hits
        if(m_triggerHit != null) {
            data.onJumpPad = m_triggerHit.layer == 7;
        }

        m_wasOnPlatform = data.onPlatform;
        m_previousPosition = transform.position;
        return data;
    }

    private bool HasHeadClearance()
    {
        return !Physics.SphereCast(collisionData.feetPosition, controller.radius, Vector3.up, out RaycastHit hit, (collisionData.halfCapsuleDistance - controller.skinWidth) * 1.5f, ~0, QueryTriggerInteraction.Ignore);
    }

    private bool IsCentered(ref RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, collisionData.halfCapsuleDistance + transform.localScale.y * 0.1f, ~0, QueryTriggerInteraction.Ignore) && controller.isGrounded;
    }

    private void OnTriggerEnter(Collider other) 
    {
        m_triggerHit = other.gameObject;
    }

    private void OnTriggerExit(Collider other) 
    {
        if(other.gameObject == m_triggerHit)
            m_triggerHit = null;    
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(!hit.collider.isTrigger)
            m_controllerHit = hit;
    }

    private void OnDrawGizmos() 
    {
        // Feet ray
        Gizmos.color = Color.black;
        float scale = 0.1f;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (collisionData.halfCapsuleDistance + transform.localScale.y * scale));

        // Collision Hull
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        if(collisionData.halfCapsuleDistance == 0)
            return;
        // Bottom Of Player
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(collisionData.feetPosition, 0.025f);

        // Top and bottom cast
        Gizmos.DrawWireSphere(collisionData.feetPosition + Vector3.up * (collisionData.halfCapsuleDistance - controller.skinWidth) * 1.5f, controller.radius);
        Gizmos.DrawWireCube(transform.position + Vector3.down * collisionData.halfCapsuleDistance, collisionData.feetScale);
    }
}
