using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    //If the not the owner, destroy the script
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("OnNetworkSpawn");

        //If not local player, destroy the script and audio listener
        if (!IsOwner)
        {
            GetComponent<Renderer>().material.color = Color.red;
            Destroy(GetComponent<AudioListener>());
        }

        // Set camera priorities
        if (IsOwner)
        {
            firstPersonCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 10;
            thirdPersonCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 10;
        }
        else
        {
            firstPersonCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 0;
            thirdPersonCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 0;
        }
    }

    //Player components
    [Header("Player Components")]
    public Rigidbody rb;
    public GameObject camHolder;
    public GameObject thirdPersonCam;
    public GameObject firstPersonCam;
    public float speed, sprintSpeed, sensitivity, jumpForce, maxForce;
    public bool grounded;
    public bool canJump = true;
    private bool isSprinting;
    private bool isSliding;
    public Quaternion nextRotation;

    [Header("Spring Components")]
    public bool _rayDidHit;
    public Vector3 DownDir;
    public float RideHeight;
    public float RideSpringStrength;
    public float RideSpringDampener;
    public float maxRayDist;
    public Quaternion uprightJointTargetRot;
    public float uprightJointSpringStrength;
    public float uprightJoinSpringDamper;

    public Vector2 move, look;
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GameObject.Find("PlayerModel").GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        UpdateUprightForce(Time.fixedDeltaTime);
        CheckForColliders();

        if (!Application.isFocused || !IsOwner)
        {
            return;
        }

        Move();
        Look();

        if (grounded)
        {
            animator.SetBool("IsFalling", false);
        }
    }

    // Reset the player's state and position
    [ServerRpc(RequireOwnership = false)]
    public void ResetStateServerRpc(Vector3 position, Quaternion rotation)
    {
        ResetState(position, rotation);
    }

    public void ResetState(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"{name} is resetting to position {position}.");

        // Reset position and rotation
        transform.position = position;
        transform.rotation = rotation;

        // Reset velocity and animations
        rb.velocity = Vector3.zero;
        animator.ResetTrigger("Throw");

        // Reset the disc
        GetComponentInChildren<gun>()?.ResetDisc();
    }

    void Move()
    {
        Vector3 currentVelocity = rb.velocity;
        currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);

        if (targetVelocity == Vector3.zero)
        {
            animator.SetBool("IsWalking", false);
        }
        else
        {
            animator.SetBool("IsWalking", true);
        }

        if (isSprinting)
        {
            targetVelocity *= sprintSpeed;
            animator.SetBool("IsRunning", true);
        }
        else
        {
            targetVelocity *= speed;
            animator.SetBool("IsRunning", false);
        }

        if (camHolder != null)
        {
            targetVelocity = camHolder.transform.TransformDirection(targetVelocity);
        }

        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Look()
    {
        nextRotation *= Quaternion.AngleAxis(look.x * sensitivity, Vector3.up);
        nextRotation *= Quaternion.AngleAxis(-look.y * sensitivity, Vector3.right);

        var angles = nextRotation.eulerAngles;
        angles.z = 0;
        angles.x = Mathf.Clamp(angles.x > 180 ? angles.x - 360 : angles.x, -60, 60);

        nextRotation = Quaternion.Euler(angles);
        camHolder.transform.rotation = nextRotation;
    }

    public void SetGrounded(bool value)
    {
        grounded = value;
    }

    private void CheckForColliders()
    {
        Ray ray = new Ray(transform.position, DownDir);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDist, ~(1 << 3)))
        {
            SetGrounded(true);
        }
        else
        {
            SetGrounded(false);
        }
    }

    public void UpdateUprightForce(float elapsed)
    {
        Quaternion toGoal = Quaternion.FromToRotation(transform.up, Vector3.up);
        toGoal.ToAngleAxis(out float rotDegrees, out Vector3 rotAxis);
        float rotRadians = rotDegrees * Mathf.Deg2Rad;

        rb.AddTorque((rotAxis * (rotRadians * uprightJointSpringStrength)) - (rb.angularVelocity * uprightJoinSpringDamper));
    }

    public void OnMove(InputAction.CallbackContext context) => move = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => look = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context)
    {
        if (canJump && grounded)
        {
            canJump = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }
    public void OnSprint(InputAction.CallbackContext context) => isSprinting = context.ReadValueAsButton();
}
