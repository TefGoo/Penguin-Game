using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float normalMoveSpeed = 5.0f;       // Normal movement speed.
    public float slideMoveSpeed = 10.0f;      // Movement speed while sliding.
    public float jumpForce = 7.0f;            // Jump force.
    public float diveForce = 10.0f;           // Dive force.
    public float acceleration = 20.0f;        // Acceleration when starting or stopping movement.
    public Transform standingGroundCheck;     // Transform for standing ground check.
    public Transform slidingGroundCheck;      // Transform for sliding ground check.
    public LayerMask groundLayer;             // Layer mask for identifying ground.

    private Rigidbody rb;
    private bool isGroundedStanding = false;
    private bool isGroundedSliding = false;
    private bool isSliding = false;
    private Quaternion standingRotation;      // Original standing rotation.
    private Vector3 slideEulerAngles = new Vector3(0.0f, 0.0f, -90.0f);

    private Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        standingRotation = transform.rotation;
    }

    void Update()
    {
        // Check if the character is grounded in the standing state.
        isGroundedStanding = Physics.CheckSphere(standingGroundCheck.position, 0.1f, groundLayer);

        // Check if the character is grounded in the sliding state.
        isGroundedSliding = Physics.CheckSphere(slidingGroundCheck.position, 0.1f, groundLayer);

        HandleJump();
        HandleDive();
    }

    void HandleJump()
    {
        // Handle jumping.
        if ((isGroundedStanding || (isSliding && isGroundedSliding)) && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleDive()
    {
        // Handle diving.
        if (Input.GetButtonDown("Dive"))
        {
            // Check if the character is in the air before diving.
            if (!isGroundedStanding && !isGroundedSliding)
            {
                ApplyDiveForce();
                ToggleSlidingState();
            }
        }
    }

    void ApplyDiveForce()
    {
        // Calculate the dive force vector in local space.
        Vector3 localDiveForce = transform.TransformVector(Vector3.right * diveForce);

        // Apply the dive force.
        rb.AddForce(localDiveForce, ForceMode.Impulse);
    }

    void ToggleSlidingState()
    {
        // Toggle between standing and sliding states.
        isSliding = !isSliding;

        // Rotate the character.
        transform.rotation = isSliding ? Quaternion.Euler(slideEulerAngles) : standingRotation;

        // Adjust movement speed based on the sliding state.
        float currentMoveSpeed = isSliding ? slideMoveSpeed : normalMoveSpeed;
        rb.velocity = new Vector3(rb.velocity.x * (currentMoveSpeed / normalMoveSpeed), rb.velocity.y, rb.velocity.z);
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Handle basic movement (left and right).
        float moveInput = Input.GetAxis("Horizontal");
        float targetVelocityX = moveInput * (isSliding ? slideMoveSpeed : normalMoveSpeed);
        Vector3 moveDirection = new Vector3(moveInput, 0.0f, 0.0f);

        // Adjust movement speed based on the sliding state.
        float currentMoveSpeed = isSliding ? slideMoveSpeed : normalMoveSpeed;
        rb.velocity = new Vector3(moveDirection.x * currentMoveSpeed, rb.velocity.y, rb.velocity.z);

        // Apply acceleration to smoothly change velocity.
        currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);


        // Rotate the character to face the movement direction or maintain the last direction when not moving.
        Vector3 newRotation = Vector3.zero;

        if (moveDirection.x > 0.1f)
        {
            // Moving right, rotate to face right.
            newRotation = new Vector3(0.0f, 0.0f, 0.0f);
        }
        else if (moveDirection.x < -0.1f)
        {
            // Moving left, rotate to face left.
            newRotation = new Vector3(0.0f, 180.0f, 0.0f);
        }
        else
        {
            // If not moving, maintain the current Y rotation.
            newRotation = new Vector3(0.0f, transform.rotation.eulerAngles.y, 0.0f);
        }

        // Apply the rotation only to the Y-axis, allowing the X-axis to change freely.
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, newRotation.y, transform.eulerAngles.z);

        // Update the rigidbody's velocity.
        rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, rb.velocity.z);
    }
}



