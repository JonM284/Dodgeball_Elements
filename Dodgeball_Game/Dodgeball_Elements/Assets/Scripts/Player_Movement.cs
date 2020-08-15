using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player_Movement : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: this script handles such including PLAYER_MOVEMENT, PLAYER_FX, BALL_BEHAVIOUR
    /// </summary>

    [Header("Player physics variables")]
    [Tooltip("Player speed")]
    public float speed;
    [Tooltip("Amount of gravity to pull downward.")]
    public float gravity;
    [Tooltip("Jump Amount")]
    public float Jump_Amount;
    [Tooltip("Input read speed, lower number = slower pickup and drop off")]
    public float Input_Speed;
    [Tooltip("Rotation speed")]
    public float rot_Mod;


    [Tooltip("Dash regular dash speed")]
    [SerializeField]
    private float m_Original_Dash_Speed;
    [Tooltip("Dash regular duration")]
    [SerializeField]
    private float m_Original_Dash_Duration;
    [Tooltip("Slowed regular duration")]
    [SerializeField]
    private float m_Slowed_Reg_Duration;
    [Tooltip("Jump cooldown, keep this low (player just needs to get off the ground)")]
    [SerializeField]
    private float m_Jump_Reset_Cooldown;
    [Tooltip("Time it takes to complete knockback")]
    [SerializeField]
    private float m_Knockback_Duration;
    [Tooltip("Speed at which player will perform knockback")]
    [SerializeField]
    private float m_Knockback_Speed;

    //modified inputs
    private float m_Input_X, m_Input_Y;
    //raw controller inputs
    private float m_Horizontal_Comp, m_Vertical_Comp;
    //current dash timer, will reset to zero
    private float m_Current_Dash_Duration = 0;
    //original speed variable
    private float m_original_Speed;
    //Max Dash timer
    private float m_Dash_Duration;
    //Current Invulnerability timer
    private float m_current_Invul_Timer = 0;
    //Max Invulnerable duration
    private float m_Max_Invul_Duration;
    //Jump timer
    private float m_Jump_Reset_Duration;
    //Current slowed duration
    private float m_Current_Slowed_Duration;
    //player rigidbody
    private Rigidbody rb;
    //can the player move their character now?
    private bool m_Read_Player_Inputs = true;
    //Is the player slowed?
    private bool m_Player_Slowed;
    //Is the player dashing?
    private bool m_Player_Dashing;
    //Is the player Invulnerable
    private bool m_Player_Invul;
    //Is the player jumping?
    private bool m_Player_Jumping;
    //Is the player bouncing back?
    private bool m_Player_Knockback;

    [Header("Ground spherecast variables")]
    [Tooltip("Distance for ground spherecast")]
    [SerializeField]private float m_Sphere_Dist;
    [Tooltip("Radius for ground spherecast")]
    [SerializeField]private float m_Sphere_Rad;

    [Header("Wall spherecast variables")]
    [Tooltip("Distance for wall spherecast")]
    [SerializeField]
    private float m_wall_Dist;
    [Tooltip("Radius for wall spherecast")]
    [SerializeField]
    private float m_Wall_Rad;


    [Header("Vectors")]
    [Tooltip("Actual player velocity, modifiable")]
    [SerializeField]private Vector3 vel;

    //vector3 for player forward direction.
    private Vector3 ray_Dir;


    [Header("Needed locations")]
    public Transform ball_Held_Pos;
    [Header("Projectile_List")]
    [Tooltip("Keeps track of how many/ what type of projectiles the player has.")]
    public List<GameObject> owned_Projectiles;

    [Header("Ball variables")]
    [Tooltip("Current ball of the player")]
    [SerializeField]
    private GameObject m_Owned_Ball;
    [Tooltip("Maximum force of the ball")]
    [SerializeField]
    private float m_Ball_Force;
    [Tooltip("Maximum throw held time")]
    [SerializeField]
    private float m_Max_Throw_Held_Time;

    //current built up force of the ball
    private float m_current_Ball_Force;
    //current amount of time ball is held for
    private float m_Current_Throw_Held_Time;
    //Percentage of ball held down
    private float ball_Held_Prc;
    //Throw Level
    private int m_Throw_Level = 1;
    //Is the player holding the throw button down?
    private bool m_holding_Throw;
    //can the player pick up another ball?
    private bool m_Can_Pick_Up;
    //pickup cooldown for ball, how long after throwing can a ball be picked up? This is to 
    //avoid throwing and immidiately catching what the player threw.
    private float m_Pick_Up_Cooldown_Max = 0.1f, m_current_Pick_Up_Cooldown;


    [Header("Rewired info")]
    [Tooltip("ID for player (**KEEP NUMBER ABOVE ZERO**)")]
    public int player_ID;
    //refernce to use rewired.
    private Player m_Player;

    //Reference to use abilities
    private Ability_Use_Behavior m_ability_Use;

    //DISCLAIMER**** MELEE TIMER MAY BE REMOVED AND USED WITH ANIMATION INSTEAD. (more than likely.)
    [Header("Melee variables")]
    //public
    [Tooltip("Max amount of time for melee hitbox to be active.")]
    public float Melee_Timer_Max;

    //private
    //reference to the melee gameobject, assigned through code.
    private GameObject m_Melee_GameObject;
    //current duration of the melee attack.
    private float current_Melee_Timer;
    //is the player currently using the melee ability?
    private bool m_Is_Meleeing;
    

    //[Header("Effects")]

    //THIS IS FOR TESTING, REMOVE EARLY IN THE PROCESS
    private Vector3 start_pos;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        m_ability_Use = GetComponent<Ability_Use_Behavior>();
        m_Player = ReInput.players.GetPlayer(player_ID - 1);
        m_original_Speed = speed;
        start_pos = transform.position;
        if (player_ID <= 0) Debug.LogError("Assigned player ID number is outside of the range of use. Please set this above zero.");

        m_Melee_GameObject = transform.Find("Melee_Object").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        m_Horizontal_Comp = m_Player.GetAxisRaw("Horizontal");
        m_Vertical_Comp = m_Player.GetAxisRaw("Vertical");

        Check_Inputs();
        Check_Cooldowns();

        if (m_Wall_In_Front())
        {
            Hault_Speed();
        }else if(!m_Wall_In_Front() && !m_Player_Dashing && !m_holding_Throw && !m_Player_Knockback)
        {
            speed = m_original_Speed;
        }

        //testing
        if (transform.position.y < -3) transform.position = start_pos;
    }

    private void FixedUpdate()
    {
        if (m_Read_Player_Inputs)
        {
            Movement();
        }
        else
        {
            if (m_Player_Dashing)
            {
                Do_Dash(transform.forward);
            }
            else if (m_Player_Knockback) Do_Dash(-transform.forward);
        }

        if (!m_Is_Grounded() && !m_Player_Dashing)
        {
            vel.y -= gravity * Time.deltaTime;
        }
        else if(m_Is_Grounded() && !m_Player_Jumping)
        {
            vel.y = 0;
        }

        rb.MovePosition(rb.position + new Vector3(Mathf.Clamp(vel.x, -speed, speed),
            vel.y, Mathf.Clamp(vel.z, -speed, speed)) * Time.deltaTime);
    }


    void Movement()
    {
        m_Input_X = Mathf.Lerp(m_Input_X, m_Horizontal_Comp, Time.deltaTime * Input_Speed);
        m_Input_Y = Mathf.Lerp(m_Input_Y, m_Vertical_Comp, Time.deltaTime * Input_Speed);


        vel.x = m_Input_X * speed;
        vel.z = m_Input_Y * speed;



        ///change forward direction
        Vector3 tempDir = new Vector3(m_Input_X, 0, m_Input_Y);


        if (tempDir.magnitude > 0.1f)
        {
            ray_Dir = tempDir.normalized;
        }
        if (m_Is_Grounded())
        {
            transform.forward = Vector3.Slerp(transform.forward, ray_Dir, Time.deltaTime * rot_Mod);
        }
    }

    void Jump()
    {
        m_Player_Jumping = true;
        m_Jump_Reset_Duration = m_Jump_Reset_Cooldown;
        vel.y = Jump_Amount;
    }

    public void Do_Dash(Vector3 _dash_Dir)
    {
        vel.x = _dash_Dir.x * speed;
        vel.z = _dash_Dir.z * speed;
    }

    void Check_Inputs()
    {

        //Initiate Dash when pressed
        if (m_Player.GetButtonDown("Dash") && !m_Player_Dashing && m_Is_Grounded())
        {
            Initiate_Dash_Type(m_Original_Dash_Duration, m_Original_Dash_Speed, false);
            //Jump();
        }

        if (m_Player.GetButton("Throw") && owned_Projectiles.Count > 0)
        {
            Debug.Log("Holding throw button");
            m_holding_Throw = true;
        }else if (m_Player.GetButtonUp("Throw") && owned_Projectiles.Count > 0)
        {
            Debug.Log("Released ball");
            m_holding_Throw = false;
            Throw_Ball(m_Ball_Force * m_Throw_Level);
            Initiate_Pick_Up_Cooldown();
            Reset_Throw_Speed();
        }

        if (m_Player.GetButtonDown("Ability_1") && !m_ability_Use.ability_Info[0].ability_Used)
        {
            m_ability_Use.Use_Ability(0);
        }

        if (m_Player.GetButtonDown("Ability_2") && !m_ability_Use.ability_Info[1].ability_Used)
        {
            m_ability_Use.Use_Ability(1);
        }


        if (m_Player.GetButtonDown("Melee") && !m_Is_Meleeing)
        {
            Initiate_Melee();
        }

    }

    void Check_Cooldowns()
    {

        //Dash timer
        if (!m_Read_Player_Inputs && m_Current_Dash_Duration < m_Dash_Duration)
        {
            m_Current_Dash_Duration += Time.deltaTime;
        }

        if ((!m_Read_Player_Inputs && m_Current_Dash_Duration >= m_Dash_Duration) || m_Wall_In_Front())
        {
            Reset_Dash_Variables();
        }

        //Invulnerability timer
        if (m_Player_Invul && m_current_Invul_Timer < m_Max_Invul_Duration)
        {
            m_current_Invul_Timer += Time.deltaTime;
        }

        if (m_Player_Invul && m_current_Invul_Timer >= m_Max_Invul_Duration)
        {
            Reset_Invulnerability_Variables();
        }

        //Slowed duration timer
        if (m_Current_Slowed_Duration >= 0 && m_Player_Slowed)
        {
            m_Current_Slowed_Duration -= Time.deltaTime;
        }

        if (m_Current_Slowed_Duration <= 0 && m_Player_Slowed)
        {
            Reset_Speed();
        }

        //Jump duration timer
        if (m_Jump_Reset_Duration > 0 && m_Player_Jumping)
        {
            m_Jump_Reset_Duration -= Time.deltaTime;
        }

        if (m_Jump_Reset_Duration <= 0 && m_Player_Jumping)
        {
            Reset_Jump();
        }

        //Holding throw button
        if (m_holding_Throw)
        {
            m_Current_Throw_Held_Time += Time.deltaTime;
            ball_Held_Prc = m_Current_Throw_Held_Time / m_Max_Throw_Held_Time;
            if (ball_Held_Prc < 0.3f)
            {
                m_Throw_Level = 1;
            }
            else if (ball_Held_Prc >= 0.3f && ball_Held_Prc < 0.6f)
            {
                m_Throw_Level = 2;
                Slow_Throw_Speed();
            }
            else if (ball_Held_Prc > 0.6f)
            {
                m_Throw_Level = 3;
                Slow_Throw_Speed();
            }

        }

        //ball pickup cooldown
        if (m_current_Pick_Up_Cooldown < m_Pick_Up_Cooldown_Max && !m_Can_Pick_Up)
        {
            m_current_Pick_Up_Cooldown += Time.deltaTime;
        }

        if (m_current_Pick_Up_Cooldown >= m_Pick_Up_Cooldown_Max && !m_Can_Pick_Up)
        {
            Reset_Pick_Up_Variables();
        }

        //Melee timer ***May be changed***
        if (current_Melee_Timer < Melee_Timer_Max && m_Is_Meleeing)
        {
            current_Melee_Timer += Time.deltaTime;
        }

        if (current_Melee_Timer >= Melee_Timer_Max && m_Is_Meleeing)
        {
            Reset_Melee_Variables();
        }

    }

    /// <summary>
    /// Initiate dashing
    /// </summary>
    /// <param name="_duration">Duration of the dash.</param>
    /// <param name="_speed">Player speed during dash.</param>
    public void Initiate_Dash_Type(float _duration, float _speed, bool _bounce_Back)
    {
        if (!_bounce_Back) m_Player_Dashing = true;
        else m_Player_Knockback = true;
        m_Read_Player_Inputs = false;
        m_Dash_Duration = _duration;
        speed = _speed;
    }

    /// <summary>
    /// Reset dash variables when complete.
    /// </summary>
    void Reset_Dash_Variables()
    {
        m_Current_Dash_Duration = 0;
        if(m_Player_Dashing) m_Player_Dashing = false;
        if (m_Player_Knockback) m_Player_Knockback = false;
        m_Read_Player_Inputs = true;
        speed = m_original_Speed;
    }

    /// <summary>
    /// Player will not be hit by abilities or killable objects during this time.
    /// </summary>
    /// <param name="_duration">How long the player can avoid being hit for.</param>
    public void Initiate_Invulnerability(float _duration)
    {
        m_Player_Invul = true;
        m_Max_Invul_Duration = _duration;
    }

    /// <summary>
    /// Reset invul. variables when complete.
    /// </summary>
    void Reset_Invulnerability_Variables()
    {
        m_current_Invul_Timer = 0;
        m_Player_Invul = false;
        
    }

    /// <summary>
    /// Slow the players speed when charging throw
    /// </summary>
    public void Slow_Throw_Speed()
    {
        float _Slowed_Speed = m_original_Speed / 3f;
        speed = _Slowed_Speed;
    }

    /// <summary>
    /// Reset the players speed after throwing
    /// </summary>
    public void Reset_Throw_Speed()
    {
        speed = m_original_Speed;
    }

    /// <summary>
    /// Cause player to slow down.
    /// </summary>
    /// <param name="_slowed_Duration">Amount of time till slowed effect is removed.</param>
    public void Slow_Speed_Effector(float _slowed_Duration)
    {
        float _Slowed_Speed = m_original_Speed / 3f;
        speed = _Slowed_Speed;
        m_Player_Slowed = true;
        m_Current_Slowed_Duration = _slowed_Duration;
    }

    /// <summary>
    /// Reset speed to original speed, reset variables.
    /// </summary>
    public void Reset_Speed()
    {
        speed = m_original_Speed;
        m_Player_Slowed = false;
        m_Current_Slowed_Duration = 0;
    }

    /// <summary>
    /// Make speed go to zero.
    /// </summary>
    public void Hault_Speed()
    {
        speed = 0;
    }

    /// <summary>
    /// Reset jump variables
    /// </summary>
    public void Reset_Jump()
    {
        m_Player_Jumping = false;
        m_Jump_Reset_Duration = 0;
    }

    /// <summary>
    /// Start timer for being able to pick up ball after throwing.
    /// </summary>
    public void Initiate_Pick_Up_Cooldown()
    {
        m_Can_Pick_Up = false;
    }

    /// <summary>
    /// Reset variables for picking up ball.
    /// </summary>
    public void Reset_Pick_Up_Variables()
    {
        m_Can_Pick_Up = true;
        m_current_Pick_Up_Cooldown = 0;
    }

    /// <summary>
    /// Begin melee ability (Collider and Cooldown)
    /// </summary>
    public void Initiate_Melee()
    {
        m_Is_Meleeing = true;
        m_Melee_GameObject.SetActive(true);
    }

    /// <summary>
    /// Reset melee ability (Collider, Cooldown, boolean)
    /// </summary>
    public void Reset_Melee_Variables()
    {
        m_Is_Meleeing = false;
        current_Melee_Timer = 0;
        m_Melee_GameObject.SetActive(false);
    }

    /// <summary>
    /// Give force to the ball in the direction player is facing.
    /// </summary>
    /// <param name="_ball_Force">Calculated force added onto ball after charging shot.</param>
    public void Throw_Ball(float _ball_Force)
    {
        owned_Projectiles[0].transform.parent = null;
        owned_Projectiles[0].GetComponent<Rigidbody>().isKinematic = false;
        owned_Projectiles[0].GetComponent<Collider>().enabled = true;
        /* Ball behaviour
        m_Owned_Ball.GetComponent<Ball_Behaviour>().Activate_Trail();
        if(m_Throw_Level == 1) m_Owned_Ball.GetComponent<Rigidbody>().useGravity = true;
        m_Owned_Ball.GetComponent<Ball_Behaviour>().Assign_Level(m_Throw_Level);
        */
        owned_Projectiles[0].GetComponent<Projectile_Behaviour>().Setup_Projectile(m_Throw_Level, transform.forward);
        owned_Projectiles[0].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        //m_Owned_Ball.GetComponent<Rigidbody>().AddForce(transform.forward * _ball_Force, ForceMode.Impulse);
        owned_Projectiles.Remove(owned_Projectiles[0]);
        if (owned_Projectiles.Count > 0)
        {
            Show_Next_Porjectile();
        }
        m_Throw_Level = 1;
        m_Current_Throw_Held_Time = 0;
        
    }


    /// <summary>
    /// Grabs the first projectile in the list and makes it visible
    /// </summary>
    void Show_Next_Porjectile()
    {
        owned_Projectiles[0].SetActive(true);
    }

    /// <summary>
    /// Force the ball to move.
    /// </summary>
    void Release_Ball()
    {
        owned_Projectiles[0].transform.parent = null;
        owned_Projectiles[0].GetComponent<Rigidbody>().isKinematic = false;
        owned_Projectiles[0].GetComponent<Collider>().enabled = true;
        //m_Owned_Ball.GetComponent<Ball_Behaviour>().Activate_Trail();
        //m_Owned_Ball.GetComponent<Rigidbody>().useGravity = true;
        owned_Projectiles[0].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        owned_Projectiles.Remove(owned_Projectiles[0]);
    }

    /// <summary>
    /// Pick up a "DEAD" ball.
    /// </summary>
    /// <param name="other">Ball object.</param>
    void Pick_Up_Ball(GameObject other)
    {
        owned_Projectiles.Add(other.gameObject);
        if (owned_Projectiles.Count >= 2)
        {
            for (int i = 1; i < owned_Projectiles.Count; i++)
            {
                owned_Projectiles[i].SetActive(false);
            }
        }
        owned_Projectiles[owned_Projectiles.Count - 1].GetComponent<Collider>().enabled = false;
        owned_Projectiles[owned_Projectiles.Count - 1].GetComponent<Rigidbody>().useGravity = false;
        owned_Projectiles[owned_Projectiles.Count - 1].transform.parent = ball_Held_Pos;
        owned_Projectiles[owned_Projectiles.Count - 1].transform.position = ball_Held_Pos.position;
        owned_Projectiles[owned_Projectiles.Count - 1].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        owned_Projectiles[owned_Projectiles.Count - 1].GetComponent<Rigidbody>().isKinematic = true;

        //m_Owned_Ball.GetComponent<Ball_Effects>().Deactivate_Trail();
        //m_Owned_Ball.GetComponent<Ball_Effects>().Play_Catch_Kick();
        
    }


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ball" && m_Can_Pick_Up && !other.gameObject.GetComponent<Projectile_Behaviour>().is_Live)
        {
            //change trail to be left if player throws at full power
            m_ability_Use.Change_Trail(other.gameObject.GetComponent<Projectile_Behaviour>());
            //attach throwable to player, put into list, and turn off
            Pick_Up_Ball(other.gameObject);
           
        }
        rb.velocity = Vector3.zero;
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Ball" && m_Can_Pick_Up && !other.gameObject.GetComponent<Projectile_Behaviour>().is_Live)
        {
            //change trail to be left if player throws at full power
            m_ability_Use.Change_Trail(other.gameObject.GetComponent<Projectile_Behaviour>());
            //attach throwable to player, put into list, and turn off
            Pick_Up_Ball(other.gameObject);
            
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        rb.velocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball" && m_Can_Pick_Up && !other.gameObject.GetComponent<Projectile_Behaviour>().is_Live)
        {
            //change trail to be left if player throws at full power
            m_ability_Use.Change_Trail(other.gameObject.GetComponent<Projectile_Behaviour>());
            //attach throwable to player, put into list, and turn off
            Pick_Up_Ball(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Ball" && m_Can_Pick_Up && !other.gameObject.GetComponent<Projectile_Behaviour>().is_Live)
        {
            //change trail to be left if player throws at full power
            m_ability_Use.Change_Trail(other.gameObject.GetComponent<Projectile_Behaviour>());
            //attach throwable to player, put into list, and turn off
            Pick_Up_Ball(other.gameObject);
        }

        if (other.gameObject.tag == "Melee")
        {
            Debug.Log("Is touching melee");
            if (m_Is_Meleeing)
            {
                Initiate_Dash_Type(m_Knockback_Duration, m_Knockback_Speed, true);
            }
        }
    }

    /// <summary>
    /// Is the player standing above the ground?
    /// </summary>
    /// <returns>Grounded or ungrounded</returns>
    bool m_Is_Grounded()
    {

        Collider[] hit_Colliders = Physics.OverlapSphere((transform.position + (Vector3.down * m_Sphere_Dist)), m_Sphere_Rad);
        int i = 0;
        while (i < hit_Colliders.Length)
        {
            if (hit_Colliders[i].tag == "Ground")
            {
                return true;
            }
            i++;
        }
        return false;


    }

    /// <summary>
    /// Is the player standing in front of a wall?
    /// </summary>
    /// <returns>In front or not</returns>
    bool m_Wall_In_Front()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, m_Wall_Rad * 1.5f, transform.forward, out hit, m_wall_Dist))
        {
            if (hit.collider.tag == "Wall")
            {
                return true;
            }
        }
        return false;
    }


    ///////////////////////////////// GIZMO DRAWS
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * m_Sphere_Dist, m_Sphere_Rad);
       
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * m_wall_Dist, m_Wall_Rad);
        
    }
}
