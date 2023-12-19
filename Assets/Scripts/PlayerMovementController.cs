using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag; // stopping speed and ground resistance

    [Header("Jumping")]
    public float jumpForce;
    public float gravity;
    public float jumpCooldown;
    public float airMultiplier; // movement control in air
    bool readyToJump = true;
    Vector3 velocity;

    [Header("Ground Check")]
    //Ground check
    public float playerHeight;
    public LayerMask playerMask;
    public bool isGrounded;

    public Transform orientation;

    // input
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rigidBody;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
    }
    private void Update()
    {
        // ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ~playerMask);

        MyInput();
        SpeedControl();

        //handle drag
        if (isGrounded)
            rigidBody.drag = groundDrag;
        else
            rigidBody.drag = 0;
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //jump
        if (Input.GetButton("Jump") && readyToJump && isGrounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //extra gravity
        rigidBody.AddForce(Vector3.down * gravity);

        if (isGrounded) //on ground
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!isGrounded) // in air
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }
    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);

        //limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rigidBody.velocity = new Vector3(limitedVel.x, rigidBody.velocity.y, limitedVel.z);
        }
    }
    void Jump()
    {
        //reset y 
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);

        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

}
