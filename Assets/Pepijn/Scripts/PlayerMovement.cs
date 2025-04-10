using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private Rigidbody rb;
    private float pitch = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(GameManager.instance.playerCanMove)
        {
            // Mouse Look
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            //cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        if(GameManager.instance.playerCanMove)
        {
            // Movement
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            Vector3 newVelocity = move.normalized * moveSpeed;
            newVelocity.y = rb.linearVelocity.y; // Preserve gravity

            rb.linearVelocity = newVelocity;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}
