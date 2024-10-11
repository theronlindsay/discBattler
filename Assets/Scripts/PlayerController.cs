using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor;

//using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem; // Required for Input System




public class PlayerController : MonoBehaviour
{

    //Player components
    [Header("Player Components")]
    public Rigidbody rb;
    public GameObject camHolder;
    public GameObject thirdPersonCam;
    public GameObject firstPersonCam;
    public float lookX, lookY;
    public float speed, sprintSpeed, sensitivity, jumpForce, maxForce;
    public bool grounded;
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
    private Vector2 move, look;
    private float lookRotation;

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
        Jump();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        toggleAim(context.ReadValue<float>());
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        toddleSlide(context.ReadValue<float>());
    }
    public static Quaternion Multiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }

    public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
    {

        if (Quaternion.Dot(a, b) < 0)
        {

            return a * Quaternion.Inverse(Multiply(b, -1));

        }else return a * Quaternion.Inverse(b);
    }

    //runs every physics update
    private void FixedUpdate()
    {
        Move();
        CheckForColliders();
        UpdateUprightForce(Time.fixedDeltaTime);
    }

    //Moves the player
    void Move(){
        //Find target velocity
        Vector3 currentVelocity = rb.velocity;
        
        //Zero out the y velocity
        currentVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        //new direction is the direction the player is facing, multiplied by the speed
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);

        //Apply speed
        if (isSprinting)
        {
            targetVelocity *= sprintSpeed;
        }
        else
        {
            targetVelocity *= speed;
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
        
        Vector3 jumpForces = Vector3.zero;
        //Check if player is grounded
        if(grounded){
            jumpForces = Vector3.up * jumpForce;
        }

        //Apply force
        rb.AddForce(jumpForces, ForceMode.Impulse);
    }

    public void toddleSlide(float value)
    {
        //Add this

    }

    void Look(){
        
        //Set the player rotation based on the look transform
        camHolder.transform.rotation *= Quaternion.AngleAxis(look.x * sensitivity, Vector3.up);
        camHolder.transform.rotation *= Quaternion.AngleAxis(-look.y * sensitivity, Vector3.right);

        //reset the y rotation of the look transform
        var angles = camHolder.transform.localEulerAngles;
        angles.z = 0;
        //Get the current angle of the look transform
        var angle = camHolder.transform.localEulerAngles.x;


        //Clamp the Up/Down rotation
        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        }
        else if(angle < 180 && angle > 60)
        {
            angles.x = 60;
        }


        camHolder.transform.localEulerAngles = angles;

        nextRotation = Quaternion.Lerp(camHolder.transform.rotation, nextRotation, Time.deltaTime * 0.5f);

        // if (move.x == 0 && move.y == 0) 
        // {   
        //     nextPosition = transform.position;

        //     if (aimValue == 1)
        //     {
        //         //Set the player rotation based on the look transform
        //         transform.rotation = Quaternion.Euler(0, camHolder.transform.rotation.eulerAngles.y, 0);
        //         //reset the y rotation of the look transform
        //         camHolder.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
        //     }

        //     return; 
        // }

        //Move Camera Side to Side
        //camHolder.transform.Rotate(Vector3.up * look.x * sensitivity);

        //Moves Camera Up and Down
        //camHolder.transform.Rotate(Vector3.right * -look.y * sensitivity);
    }

    // Start is called before the first frame update
    void Start()
    {
        //Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //Happens after all updates
    private void LateUpdate(){
        Look();
        SetCamera();

    }

    public void SetGrounded(bool value){
        grounded = value;
    }

    public void toggleAim(float value)
    {
        aimValue = value;
        SetCamera();
    }
    private void SetCamera()
    {
        if (aimValue== 1f && !firstPersonCam.activeInHierarchy)
        {
            thirdPersonCam.SetActive(false);
            firstPersonCam.SetActive(true);
        }
        else if(aimValue != 1f && !thirdPersonCam.activeInHierarchy)
        {
            thirdPersonCam.SetActive(true);
            firstPersonCam.SetActive(false);
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

        // Check if player is wall running
        //Create a new horizontal ray
        Ray wallRay = new Ray(transform.position, transform.right);
        Debug.DrawLine(transform.position, transform.position + transform.right * maxRayDist, Color.cyan);

        //Check if the ray hits anything (except the player's layer) ~ is 'all but' `1<<3` is the player's layer
        bool _wallRayDidHit = Physics.Raycast(wallRay, out RaycastHit wallHit, maxRayDist, ~(1<<3));
        Debug.DrawLine(transform.position, wallHit.point, Color.magenta);

        if (_wallRayDidHit)
        {
            //TODO: Add wall run logic
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
}
