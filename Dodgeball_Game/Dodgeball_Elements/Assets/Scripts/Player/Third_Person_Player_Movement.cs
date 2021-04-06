using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

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

    [Header("Ball Variables")]
    public float ball_Force;
    public float Min_Ball_Force, Max_Ball_Force;


    private CharacterController char_Controller;

    //variables for Sphere cast
    [Header("SphereCast Variables")]
    [Tooltip("Radius of the sphere to be created")]
    public float sphere_Radius;
    public float sphere_Dist;

    //variables for forwards wall check cast
    [Header("Ray variables")]
    [Tooltip("Radius of sphere cast to check if player is grounded.")]
    [SerializeField]
    private float ground_Ray_Rad;
    [Tooltip("Distance for sphere cast")]
    [SerializeField]
    private float ground_Ray_Dist;

    //ball positional references
    [Header("References")]
    [Tooltip("Position where the ball will be parented to when picked up")]
    public Transform Ball_Held_Pos;
    [Tooltip("Position where the ball will launch from when thrown")]
    public Transform Ball_Launch_Pos;


    [Header("Player Identification")]
    public int Player_ID, Team_ID;

    [HideInInspector]
    public GameObject m_Owned_Ball;

    [SerializeField]
    private Vector3 vel, jumpingVel;
    private Vector3 move_Dir;
    private float speed = 1f;
    private float input_Y, input_X, sensitivity = 1.0f;
    public float smooth_Turn_Amount;
    public float turn_Smooth_Time = 0.1f;
    [SerializeField]
    private float antiBumpFactor = 0.75f;
    private bool m_can_Sprint = false;
    [HideInInspector]
    public bool m_Is_Sprinting = false;
    private bool limitDiagonalSpeed = true;

    //cooldown floats
    private float m_Ball_Throw_Cooldown = 0.5f, m_Orig_Cooldown, m_Tackle_Duration, m_Slide_Tackle_Duration
        , m_original_Speed, m_Attack_Speed_Cooldown = 1f, m_Time_To_Reach, m_Damage_Cooldown, m_DC_Max_Original, m_Electric_Damage_Cooldown,
        m_Dash_Duration, m_Current_Dash_Duration, m_invulnerability_Dur, m_Cur_Invul_Dur, m_Stun_Duration, m_Cur_Stun_Dur,
        m_Ball_Held_Duration, m_Max_Ball_Held_Duration = 0.6f;

    //changes player's speed if they are walking or sprinting
    private float m_speed_Modifier = 1;

    private float Y_Dir;
    private int m_Jump_Amount = 1;
    private bool m_Can_Jump = false;

    //"Status" bools
    private bool m_can_Catch_Ball = true, m_Is_Holding_Throw = false, m_Is_Tackling = false, m_Is_Slide_Tackling = false
        , m_Read_Player_Inputs = true, m_Has_Attacked = false, m_Is_Attacking = false, m_Taking_Damage = false, m_Is_Dashing = false;

    private Vector3 m_Ball_End_Position, damage_Dir;

    //REWIRED
    private Player m_Player;

    private Vector3 respawn_Pos;

    public Transform cam;


    // Start is called before the first frame update
    void Start()
    {
        char_Controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        respawn_Pos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
        m_Player = ReInput.players.GetPlayer(Player_ID - 1);
        m_Orig_Cooldown = m_Ball_Throw_Cooldown;
        m_original_Speed = speed;
        
    }

    // Update is called once per frame
    void Update()
    {
        Check_Inputs();
        Check_Cooldowns();
        Movement();

        
    }

    void Check_Inputs()
    {
        if (Input.GetKey(KeyCode.W))
        {
            vertical_Comp = 1;
            if (Is_Grounded())
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

        if (m_Can_Jump) {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
              
            }
        }

        if (Is_Grounded())
        {
            if (m_Jump_Amount < 1) m_Jump_Amount = 1;
        }

        if (m_Jump_Amount <= 0)
        {
            m_Can_Jump = false;
        }
        else
        {
            m_Can_Jump = true;
        }


        //press w/o ball to pass, press w/ ball to swap players
        if (Input.GetMouseButton(0) && m_Owned_Ball != null && !m_Wall_In_Front() && !m_Is_Attacking)
        {
            //Charge ball
            m_Is_Holding_Throw = true;
        }
        else if (Input.GetMouseButtonUp(0) && m_Owned_Ball != null && !m_Wall_In_Front() && !m_Is_Attacking)
        {
            //throw ball
            if (m_Ball_Held_Duration >= m_Max_Ball_Held_Duration)
            {
                ball_Force = Max_Ball_Force;
            }
            else
            {
                ball_Force = Min_Ball_Force;
            }
            Debug.Log($"Speed: {ball_Force}");
            Throw_Ball();
        }

    }

    //due to new concept (TP movement) this part was made with the help of Brackeys
    /// <summary>
    /// Movement of player
    /// </summary>
    void Movement()
    {
        input_Y = Mathf.Lerp(input_Y, vertical_Comp, Time.deltaTime * 19f);
        input_X = Mathf.Lerp(input_X, horizontal_Comp, Time.deltaTime * 19f);

        sensitivity = Mathf.Lerp(sensitivity,
            (input_Y != 0 && input_X != 0 && limitDiagonalSpeed) ? 0.75f : 1.0f, Time.deltaTime * 19f);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(horizontal, 0, vertical).normalized;

        float yaw_Cam = cam.eulerAngles.y;
        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + yaw_Cam;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yaw_Cam, 0), smooth_Turn_Amount * Time.deltaTime);

        if (dir.magnitude >= 0.1f)
        {
            
            /*float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smooth_Turn_Amount, turn_Smooth_Time);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);*/

            move_Dir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            

            char_Controller.Move(move_Dir.normalized * speed * Time.deltaTime);
        }

        speed = m_Is_Sprinting && m_can_Sprint ? run_Speed : walk_Speed;

        if (!Is_Grounded())
        {
            Y_Dir -= gravity * Time.deltaTime;
        }

        vel.y = Y_Dir;


        char_Controller.Move(vel * Time.deltaTime);

      

    }

    /// <summary>
    /// Allow polayer to jump
    /// </summary>
    void Jump()
    {
        if (m_Jump_Amount > 0)
        {
            Y_Dir = jump_Height;
            m_Jump_Amount--;
        }
    }

    /// <summary>
    /// Player active cooldowns
    /// </summary>
    void Check_Cooldowns()
    {

        if (m_Is_Holding_Throw)
        {
            m_Ball_Held_Duration += Time.deltaTime;
        }

        if (!m_can_Catch_Ball && m_Ball_Throw_Cooldown > 0)
        {
            m_Ball_Throw_Cooldown -= Time.deltaTime;
        }

        if (m_Ball_Throw_Cooldown <= 0 && !m_can_Catch_Ball)
        {
            m_can_Catch_Ball = true;
            m_Ball_Throw_Cooldown = m_Orig_Cooldown;
        }
    }

        bool Is_Grounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, sphere_Radius, Vector3.down, out hit, sphere_Dist))
        {
            return true;
        }
        return false;
    }

    //SPORT RELATED CODE TO FOLLOW

    /// <summary>
    /// Pick up a "DEAD" ball.
    /// </summary>
    /// <param name="other">Ball object.</param>
    void Pick_Up_Ball(GameObject other)
    {

        m_Owned_Ball = other.gameObject;
        m_Owned_Ball.GetComponent<Collider>().enabled = false;
        m_Owned_Ball.GetComponent<Rigidbody>().useGravity = false;
        m_Owned_Ball.transform.parent = Ball_Held_Pos;
        m_Owned_Ball.transform.position = Ball_Held_Pos.position;
        m_Owned_Ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        m_Owned_Ball.GetComponent<Rigidbody>().isKinematic = true;

        m_Owned_Ball.GetComponent<Ball_Effects>().Deactivate_Trail();
        m_Owned_Ball.GetComponent<Ball_Effects>().Play_Catch_Kick();
        
        m_Owned_Ball.GetComponent<Ball_Effects>().Reset_Force_Variables();
    }

   

    /// <summary>
    /// apply force to the ball
    /// </summary>
    public void Throw_Ball()
    {
        m_can_Catch_Ball = false;

        m_Owned_Ball.transform.parent = null;
        if (!m_Taking_Damage)
        {
            m_Owned_Ball.GetComponent<Ball_Effects>().Play_Catch_Kick();
        }
        m_Owned_Ball.GetComponent<Rigidbody>().isKinematic = false;
        m_Owned_Ball.transform.position = Ball_Launch_Pos.position;
        if (m_Taking_Damage)
        {
            Vector3 random_Dir = Vector3.zero;
            if (m_Wall_In_Damage_Dir())
            {
                damage_Dir *= -1;
                random_Dir = new Vector3(damage_Dir.x + Random.Range(-0.4f, 0.4f), Random.Range(0f, 0.2f), damage_Dir.z + Random.Range(-0.4f, 0.4f));
            }
            else
            {
                random_Dir = new Vector3(damage_Dir.x + Random.Range(-0.4f, 0.4f), Random.Range(0f, 0.2f), damage_Dir.z + Random.Range(-0.4f, 0.4f));
            }
            float random_Force = Random.Range(Min_Ball_Force / 2, Min_Ball_Force);
            m_Owned_Ball.GetComponent<Rigidbody>().AddForce((random_Dir) * random_Force, ForceMode.Impulse);
            m_Owned_Ball.GetComponent<Ball_Effects>().Set_Ball_Variable(random_Force);
        }
        else
        {
            var _dir = cam.GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            m_Owned_Ball.GetComponent<Rigidbody>().AddForce(_dir.direction * ball_Force * m_speed_Modifier, ForceMode.Impulse);
            m_Owned_Ball.GetComponent<Ball_Effects>().Set_Ball_Variable(ball_Force);
        }

        ball_Force = Min_Ball_Force;
        m_Ball_Held_Duration = 0;
        m_Is_Holding_Throw = false;

        m_Owned_Ball.GetComponent<Collider>().enabled = true;
        m_Owned_Ball.GetComponent<Ball_Effects>().Activate_Trail();
        m_Owned_Ball.GetComponent<Rigidbody>().useGravity = true;
        m_Owned_Ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        

        m_Owned_Ball = null;


    }


    //CHECKS

    bool m_Wall_In_Damage_Dir()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, ground_Ray_Rad, damage_Dir, out hit, ground_Ray_Dist))
        {
            if (hit.collider.tag == "Wall")
            {
                return true;
            }
        }
        return false;
    }

    bool m_Wall_In_Front()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, ground_Ray_Rad * 1.5f, transform.forward, out hit, ground_Ray_Dist))
        {
            if (hit.collider.tag == "Wall")
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// COLLISIONS
    /// </summary>

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ball" && other.gameObject.transform.parent == null && m_can_Catch_Ball && !m_Taking_Damage)
        {
            Pick_Up_Ball(other.gameObject);
            Debug.Log("Contact with ball");
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Ball" && other.gameObject.transform.parent == null && m_can_Catch_Ball && !m_Taking_Damage)
        {
            Pick_Up_Ball(other.gameObject);

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + (Vector3.down * sphere_Dist), sphere_Radius);
    }

}
