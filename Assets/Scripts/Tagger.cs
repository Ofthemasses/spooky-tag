using UnityEngine;

public abstract class Tagger : MonoBehaviour
{
    private Vector3 moveDirection;

    [Header("States")] public MoveState currentState;
    protected bool isSprinting;
    protected bool isOnGround;
    protected bool isCrouching;

    [Header("Speeds")] private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float airMultiplier;

    [Header("Slopes")] public float maxAngle;
    private RaycastHit slopeHit;

    [Header("Heights")] public float playerHeight;
    protected float playerHeightStartScale;
    public float jumpHeight;
    public float crouchHeightScale;

    [Header("Drag Control")] public float gDrag;
    public float aDrag;

    [Header("Misc")] public LayerMask ground;
    public Transform orientation;
    public bool canMove = true;


    protected Rigidbody rigidbody;
    protected Animator animator;
    private Vector3 nextAnimPosition;


    public enum MoveState
    {
        inAir,
        inSprint,
        inWalk,
        inCrouch,
        onSlope,
        inSlide
    }


    // Start is called before the first frame update
    protected void Start()
    {
        animator = GetComponentInChildren<CharacterCollection>().GetComponent<Animator>();
        moveDirection = Vector3.zero;
        rigidbody = GetComponent<Rigidbody>();
        playerHeightStartScale = transform.localScale.y;
    }

    protected void Update()
    {
        isOnGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);
        rigidbody.drag = isOnGround ? gDrag : aDrag;
        changeState();
    }

    protected void changeState()
    {
        if (isOnGround && isSprinting)
        {
            currentState = MoveState.inSprint;
            moveSpeed = sprintSpeed;
        }
        else if (isOnGround)
        {
            currentState = MoveState.inWalk;
            moveSpeed = walkSpeed;
        }
        else if (isOnGround && isCrouching)
        {
            currentState = MoveState.inCrouch;
            moveSpeed = crouchSpeed;
        }else if (OnSlope())
        {
            currentState = MoveState.onSlope;
        }
        else
        {
            currentState = MoveState.inAir;
        }
    }

    //Moves rigidbody by adding force in the direction of moveDirection
    protected void Move(float inputV, float inputH)
    {
        if (!canMove) return;
        moveDirection = orientation.forward * inputV + orientation.right * inputH;

        if (OnSlope())
        {
            rigidbody.AddForce(getSlopeMove() * (moveSpeed * 20f), ForceMode.Force);
        }
        else if (isOnGround)
        {
            rigidbody.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        }
        else
        {
            rigidbody.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
        }
    }

    protected void Jump()
    {
        moveDirection += Vector3.up * (jumpHeight / 100f);
        rigidbody.AddForce(moveDirection, ForceMode.Impulse);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxAngle && angle != 0;
        }

        return false;
    }

    private Vector3 getSlopeMove()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    protected void speedLimiter()
    {
        Vector3 velocityLimit = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        if (!(velocityLimit.magnitude > sprintSpeed)) return;
        Vector3 limitVelocity = velocityLimit.normalized * sprintSpeed;
        rigidbody.velocity = new Vector3(limitVelocity.x, limitVelocity.y, limitVelocity.z);
    }

    public void beginClimb(Vector3 pos, Vector3 dir)
    {
        // Clear Velocity
        moveDirection = Vector3.zero;
        // Disable Movement and Components
        canMove = false;
        DisableComponents();
        // Change the position to current - the difference in height from animating (2)
        Vector3 curPos = transform.position;
        transform.position = new Vector3(curPos.x, pos.y - 2, curPos.z);
        // Trigger the animation and set current state
        animator.SetTrigger("climb");
        // Save the target position
        nextAnimPosition = pos;
    }

    public void endClimb()
    {
        // Teleport the player to the expected position
        transform.position = nextAnimPosition;
        // Enable Movement and Components
        canMove = true;
        EnableComponents();
    }

    public void beginVault(Vector3 pos)
    {
        moveDirection = Vector3.zero;

        canMove = false;
        DisableComponents();

        animator.SetTrigger("vault");

        nextAnimPosition = pos;
    }

    public void endVault()
    {
        // Teleport the player to the expected position
        transform.position = nextAnimPosition;
        // Enable Movement and Components
        canMove = true;
        EnableComponents();
    }

    public void beginSlide()
    {
    }

    // Disables any components
    // NOTE: Could be used to disables the AI's tagging and attacking abilities
    protected abstract void DisableComponents();

    // Enables any components
    // NOTE: Could be sued to enable the AI's tagging and attacking abilities
    protected abstract void EnableComponents();
}