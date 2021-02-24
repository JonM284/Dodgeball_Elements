using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Third_Person_Player_Movement : MonoBehaviour
{

    [Header("Movement Speeds")]
    [Tooltip("Average speed player will be moving at when not sprinting.")]
    public float walk_Speed;
    [Tooltip("Speed player will be moving at when sprinting forward")]
    public float run_Speed;

    [Header("Movement Affectors")]
    [Tooltip("how fast the player will fall towards the ground")]
    public float gravity;
    [HideInInspector]
    public float horizontal_Comp, vertical_Comp;
    [Tooltip("How high player can jump off ground vertically")]
    public float jump_Height;

    private CharacterController char_Controller;

    [SerializeField]
    private Vector3 vel, jumpingVel;
    private float speed = 1f;
    private float input_Y, input_X, sensitivity = 1.0f;
    public float smooth_Turn_Amount;
    public float turn_Smooth_Time = 0.1f;
    [SerializeField]
    private float antiBumpFactor = 0.75f;
    private bool m_can_Sprint = false, isGrounded;
    [HideInInspector]
    public bool m_Is_Sprinting = false;
    private bool limitDiagonalSpeed = true;

    //variables for Sphere cast
    [Header("SphereCast Variables")]
    [Tooltip("Radius of the sphere to be created")]
    public float sphere_Radius;
    public float sphere_Dist;

    
    
    
    private Vector3 respawn_Pos;

    public Transform cam;

    // Start is called before the first frame update
    void Start()
    {
        char_Controller = GetComponent<CharacterController>();
        
        respawn_Pos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        Check_Inputs();
        Movement();
    }

    void Check_Inputs()
    {
        if (Input.GetKey(KeyCode.W))
        {
            vertical_Comp = 1;
            if (isGrounded)
            {
                m_can_Sprint = true;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            vertical_Comp = -1;
            m_can_Sprint = false;
        }
        else
        {
            vertical_Comp = 0;
            m_can_Sprint = false;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontal_Comp = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            horizontal_Comp = -1;
        }
        else
        {
            horizontal_Comp = 0;
        }


        if (Input.GetKeyDown(KeyCode.LeftShift) && m_can_Sprint && Input.GetKey(KeyCode.W))
        {
            m_Is_Sprinting = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.W) || Input.GetMouseButton(0))
        {
            m_Is_Sprinting = false;
        }

        if (Can_Jump())
        {
            if (Input.GetKeyDown(KeyCode.Space)) Jump();
        }

        Debug.Log($"Can Jump: {Can_Jump()}");
    }

    //due to new concept (TP movement) this part was made with the help of Brackeys
    void Movement()
    {
        input_Y = Mathf.Lerp(input_Y, vertical_Comp, Time.deltaTime * 19f);
        input_X = Mathf.Lerp(input_X, horizontal_Comp, Time.deltaTime * 19f);

        sensitivity = Mathf.Lerp(sensitivity,
            (input_Y != 0 && input_X != 0 && limitDiagonalSpeed) ? 0.75f : 1.0f, Time.deltaTime * 19f);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(horizontal, 0, vertical).normalized;

        if (dir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smooth_Turn_Amount, turn_Smooth_Time);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 move_Dir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            char_Controller.Move(move_Dir.normalized * speed * Time.deltaTime);
        }

        speed = m_Is_Sprinting && m_can_Sprint ? run_Speed : walk_Speed;

    

    }

    void Jump()
    {
        vel.y = jump_Height;
    }

    bool Can_Jump()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, sphere_Radius, Vector3.down, out hit, sphere_Dist))
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + (Vector3.down * sphere_Dist), sphere_Radius);
    }

}
