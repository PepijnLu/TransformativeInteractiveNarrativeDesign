using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform playerCamera = null;
    [SerializeField] float mouseSensitivity = 3.5f;
    [SerializeField] float walkSpeed = 6.0f;
    float gravity = -13.0f;
    private float moveSmoothTime = 0.3f;
    private float mouseSmoothTime = 0.03f;
    private bool lockCursor = true;

    [SerializeField] float interactionDistance = 3f;
    [SerializeField] AudioSource footstepSfx;
    private float lookThreshold = 0.3f;
    private IInteractable currentInteractable = null;

    float cameraPitch = 0.0f;
    float velocityY = 0.0f;
    CharacterController controller = null;
    Rigidbody rb;
    Vector2 currentDir = Vector2.zero;
    Vector2 currentDirVelocity = Vector2.zero;
    Vector2 currentMouseDelta = Vector2.zero;
    Vector2 currentMouseDeltaVelocity = Vector2.zero;
    float moveX, moveZ, pitch;
    [SerializeField] float footstepTiming;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        StartCoroutine(PlayFootstepSound());
    }

    void Update()
    {
        if(!GameManager.instance.playerCanMove) return;

        UpdateMouseLook();
        //UpdateMovement();
        
        CheckForInteractables();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if(currentInteractable != null)
            {
                currentInteractable.OnInteract();
                Debug.Log($"Triggered interactable: {currentInteractable.GetType().Name}");
            }
            else
            {
                Debug.Log("Interactable is null");
            }
        }

        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");
    }

    IEnumerator PlayFootstepSound()
    {
        while(true)
        {
            while((rb.linearVelocity.x != 0 || rb.linearVelocity.z != 0) && GameManager.instance.playerCanMove)
            {
                footstepSfx.Play();
                yield return new WaitForSeconds(footstepTiming);
            }
            yield return null;
        }
    }

    void FixedUpdate()
    {
        PlayerMovement();
    }

    void UpdateMouseLook()
    {
        // Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        // currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
        // cameraPitch -= currentMouseDelta.y * mouseSensitivity;
        // cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);
        // playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        // transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);

        if(GameManager.instance.playerCanMove)
        {
            // Mouse Look
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    // void UpdateMovement()
    // {
    //     Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    //     targetDir.Normalize();
    //     currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);
    //     if (controller.isGrounded)
    //         velocityY = 0.0f;
    //     velocityY += gravity * Time.deltaTime;
    //     Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * walkSpeed + Vector3.up * velocityY;
    //     controller.Move(velocity * Time.deltaTime);
    // }

    void PlayerMovement()
    {
        if(GameManager.instance.playerCanMove)
        {
            // Movement
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            Vector3 newVelocity = move.normalized * walkSpeed;
            newVelocity.y = rb.linearVelocity.y; // Preserve gravity

            rb.linearVelocity = newVelocity;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    void CheckForInteractables()
    {
        Collider[] nearby = Physics.OverlapSphere(playerCamera.position, interactionDistance);

        float bestMatch = lookThreshold;
        IInteractable bestInteractable = null;

        foreach (var col in nearby)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Vector3 dirToObject = (col.bounds.center - playerCamera.position).normalized;
                float lookAlignment = Vector3.Dot(playerCamera.forward, dirToObject);

                if (lookAlignment > bestMatch)
                {
                    if(!(interactable == currentInteractable && GameManager.instance.fireExtinguisherEquipped))
                    {
                        bestMatch = lookAlignment;
                        bestInteractable = interactable;
                    }
                }
            }
        }

        if(bestInteractable != null && currentInteractable != bestInteractable)
        {
            Debug.Log($"Set currentInteractable to {bestInteractable.GetType().Name}");
            currentInteractable = bestInteractable;
        }
        if(bestInteractable == null && currentInteractable != null) currentInteractable = null; 
    }
}