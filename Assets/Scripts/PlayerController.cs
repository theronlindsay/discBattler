using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;

//using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem; // Required for Input System

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
            //Destroy(this);
        }

        //if owner, set the camera priority to 10
        if (IsOwner)
        {
            firstPersonCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 10;
            thirdPersonCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 10;
        }   else {
            //Set the first and third person cameras to priority 0
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
    private float aimValue;

    //Spring components
    [Header("Spring Components")]
    public bool _rayDidHit;
    public Vector3 DownDir;
    public float RideHeight;
    public float RideSpringStrength;
    //value from 0 to 1, 0 is no damping, 1 is no spring
    public float RideSpringDampener;
    public float maxRayDist;
    public Quaternion uprightJointTargetRot;
    public float uprightJointSpringStrength;
    //value from 0 to 1, 0 is no damping, 1 is no spring
    public float uprightJoinSpringDamper;

    //Private variables
    public Vector2 move, look;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {  

        animator = GameObject.Find("PlayerModel").GetComponent<Animator>();
        //Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //runs every physics update
    private void FixedUpdate()
    {
        UpdateUprightForce(Time.fixedDeltaTime);
        CheckForColliders();

        
        if(!Application.isFocused || !IsOwner){
            
            return;
        }   

        Move();
        Look();
        if(grounded){
            animator.SetBool("IsFalling", false);
        }
    }

    //Multiply a quaternion by a scalar
    public static Quaternion Multiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }

    //Find the shortest rotation between two quaternions
    public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
    {

        if (Quaternion.Dot(a, b) < 0)
        {

            return a * Quaternion.Inverse(Multiply(b, -1));

        } else return a * Quaternion.Inverse(b);
    }



    //Moves the player
    void Move(){
        
        //Find target velocity
        Vector3 currentVelocity = rb.velocity;

        //Zero out the y velocity
        currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        //new direction is the direction the player is facing, multiplied by the speed
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);

        //If velocity is zero, turn animator off
        if (targetVelocity == Vector3.zero)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("Backwards", false);
            animator.SetBool("StrafeLeft", false); 
            animator.SetBool("StrafeRight", false);
        } else if (targetVelocity.z < Vector3.zero.z)
        {
            Debug.Log("Walking Backwards");
            animator.SetBool("Backwards", true);
            animator.SetBool("StrafeLeft", false);
            animator.SetBool("StrafeRight", false);
        } else if(targetVelocity.x < Vector3.zero.x && animator.GetBool("IsFalling") == false){ 
            Debug.Log("Strafing Left");
            animator.SetBool("StrafeLeft", true);
            animator.SetBool("StrafeRight", false);
            animator.SetBool("Backwards", false);
        } else if(targetVelocity.x > Vector3.zero.x && animator.GetBool("IsFalling") == false){
            Debug.Log("Strafing Right");
            animator.SetBool("StrafeRight", true);
            animator.SetBool("StrafeLeft", false);
            animator.SetBool("Backwards", false);
        }
        else {
            animator.SetBool("Backwards", false);
            Debug.Log("Walking");
            animator.SetBool("IsWalking", true);
            animator.SetBool("StrafeLeft", false);
            animator.SetBool("StrafeRight", false);
        }

        //Apply speed
        if (isSprinting)
        {
            Debug.Log("Sprinting");
            animator.SetBool("IsRunning", true);
            targetVelocity *= sprintSpeed;
        }
        else
        {
            targetVelocity *= speed;
            animator.SetBool("IsRunning", false);
        }

        //Make sure the camHolder still exists
        if (camHolder == null)
        {
            return;
        }

        //Align direction
        targetVelocity = camHolder.transform.TransformDirection(targetVelocity);

        //Calculate forces
        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        //Limit force
        Vector3.ClampMagnitude(velocityChange, maxForce);

        //Apply a minimum x and y force
        if(Mathf.Abs(velocityChange.x) < 0.1f){
            velocityChange = new Vector3(0, velocityChange.y, velocityChange.z);
        }

        //Apply force
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        
    }
    
    void Jump(){
        grounded = false;
        animator.SetTrigger("Jump");
        Invoke("DelayedJump", 0.1f);
    }
    void DelayedJump(){
        Vector3 jumpForces = Vector3.zero;
        jumpForces = Vector3.up * jumpForce;
        rb.AddForce(jumpForces, ForceMode.Impulse);
        animator.SetBool("IsFalling", true);
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("Backwards", false);
        animator.SetBool("StrafeLeft", false);
        animator.SetBool("StrafeRight", false);
        Invoke("SetFalling", 0.2f);
        
    }   
    void SetFalling(){
        animator.SetBool("IsFalling", true);
        canJump = true;
    }   

    void Look(){
        
        //Set the player rotation based on the look transform
        nextRotation *= Quaternion.AngleAxis(look.x * sensitivity, Vector3.up);
        nextRotation *= Quaternion.AngleAxis(-look.y * sensitivity, Vector3.right);

        //reset the y rotation of the look transform
        var angles = nextRotation.eulerAngles;
        angles.z = 0;
        //Get the current angle of the look transform
        var angle = nextRotation.eulerAngles.x;

        //Clamp the Up/Down rotation
        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        }
        else if(angle < 180 && angle > 60)
        {
            angles.x = 60;
        }

        nextRotation = Quaternion.Euler(angles);
        camHolder.transform.rotation = nextRotation;
    }


    //Set the grounded variable
    public void SetGrounded(bool value){
        grounded = value;
    }

    //Toggle CameraMode
    public void toggleAim(float value)
    {        
        aimValue = value;

        if (aimValue== 1f && !firstPersonCam.activeInHierarchy)
        {
            thirdPersonCam.SetActive(false);
            firstPersonCam.SetActive(true);
        }
        else if(aimValue != 1f && !thirdPersonCam.activeInHierarchy)
        {
            thirdPersonCam.SetActive(true);
            firstPersonCam.SetActive(false);
        } else {
            Debug.Log("Camera already active");
        }
    }

    //Toggle Sliding
    public void toddleSlide(float value)
    {
        isSliding = value == 1f;

        if (isSliding)
        {
            rb.AddForce(camHolder.transform.forward * 10, ForceMode.Impulse);
            // rotate player so model is facing the direction of the slide
            transform.rotation = Quaternion.Euler(0, rb.transform.rotation.eulerAngles.y, 0);


        } else {
            rb.AddForce(-camHolder.transform.forward * 10, ForceMode.Impulse);
        }
    }


    void CheckForColliders(){

        //Create a ray
        Ray ray = new Ray(transform.position, DownDir);
        Debug.DrawLine(transform.position, transform.position + DownDir * maxRayDist, Color.green);

        //Check if the ray hits anything (except the player's layer) ~ is 'all but' `1<<3` is the player's layer
        _rayDidHit = Physics.Raycast(ray, out RaycastHit hit, maxRayDist, ~(1<<3));
        Debug.DrawLine(transform.position, hit.point, Color.blue);

        //Get the rigidbody of the hit object
        Rigidbody hitBody = hit.rigidbody;

        //Check if the hit object has a rigidbody
        if(_rayDidHit){
            SetGrounded(true);
            // Check if the hit object has a rigidbody
            
            if (hitBody != null)
                {
                    // The ray hit a rigidbody
                    // You can now access the velocity of the hit rigidbody
                    Vector3 otherVel = hitBody.velocity;
                }
        } else {
            SetGrounded(false);
        }

        //If the ray hit something
        if(hitBody != null){
            Vector3 vel = rb.velocity;
            Vector3 rayDir = transform.TransformDirection(DownDir);

            Vector3 otherVel = Vector3.zero; //Other rigidbody velocity, if it exists

            //Check if the hit object has a rigidbody
            if(hitBody != null){
                //Get the other rigidbody's velocity
                otherVel = hitBody.velocity;
            }

            //Calculate relative velocity
            float rayDirVel = Vector3.Dot(rayDir, vel);
            //Calculate relative velocity of other rigidbody, if it exists
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            //Calculate relative velocity for collision with other rigidbody
            float relVel = rayDirVel - otherDirVel;

            //Calculate spring force
            float x = hit.distance - RideHeight;
            float springForce = (x * RideSpringStrength) - (relVel * RideSpringDampener);

            //Apply force to player
            rb.AddForce(rayDir * springForce);

            //Apply force to other rigidbody, if it exists
            if(hitBody != null){
                hitBody.AddForceAtPosition(rayDir * -springForce, hit.point);
            }
        }
    }

    public void UpdateUprightForce(float elapsed){
        //Get the current rotation of the character
        Quaternion characterCurrent = transform.rotation;
    
        //Calculate the rotation to the goal
        Quaternion toGoal = Quaternion.FromToRotation(transform.up, Vector3.up);
        Debug.DrawLine(transform.position, transform.position + toGoal * Vector3.up, Color.red);

        //Calculate the rotation to the target rotation
        Vector3 rotAxis;
        float rotDegrees;

        //Get the rotation to the target rotation
        toGoal.ToAngleAxis(out rotDegrees, out rotAxis);

        //Convert the angle to 0-360
        float rotRadians = rotDegrees * Mathf.Deg2Rad; 
        //Add torque to the rigidbody, don't ask me why it's this way, it just works
        rb.AddTorque((rotAxis * (rotRadians * uprightJointSpringStrength)) - (rb.angularVelocity * uprightJoinSpringDamper));
    }

    //Input System Functions


    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(canJump && grounded){
            canJump = false;
            Jump();
        } else {
            Debug.Log("Can't Jump");
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        toggleAim(context.ReadValue<float>());
        Debug.Log("Aim: " + context.ReadValue<float>());
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        toddleSlide(context.ReadValue<float>());
    }

    public void OnRecall(InputAction.CallbackContext context)
    {
        Debug.Log("Recall");
    }   

}
