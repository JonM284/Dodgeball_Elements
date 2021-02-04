using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player_Behaviour : MonoBehaviour {


    public float speed, gravity, sprint_speed_Mod, rot_Mod;
    public float ball_Force;
    public int Player_ID, Team_ID;

    public Transform Ball_Held_Pos;
    public bool player_Controlled;
    public Vector3 vel;
    

    [Header("Ray variables")]
    [SerializeField]private float ground_Ray_Rad;
    [SerializeField]private float ground_Ray_Dist, m_ball_Check_Radius, m_Melee_Range;

    [Header("Passing Variables")]
    public float reg_Pass_H_Offset;
    public float lob_Pass_H_Offset, Pass_Angle;

    [Header("Player Variables")]
    public float tackle_Dur_Max;
    public float slide_Tackle_Dur_Max, attack_Speed_Cooldown_Max, pass_Range, Input_Speed, slide_Tackle_Speed_Mod,
        tackle_Speed_Mod, damage_Cooldown_Max, tackle_Damage_Cooldown = 0.5f, slide_Tackle_Damage_Cooldown = 1.5f, attack_Cooldown;


    [HideInInspector]public Ball_Effects ball_reference;

    private Rigidbody rb;
    private float m_speed_Modifier = 1;
    private Vector3 rayDir;
    private float m_Input_X, m_Input_Y, m_Horizontal_Comp, m_Vertical_Comp, m_anti_Bump_Factor = 0.75f;
    private float min_Z, max_Z, min_X, max_X;
    private float m_Ball_Throw_Cooldown = 0.5f, m_Orig_Cooldown, m_Tackle_Duration, m_Slide_Tackle_Duration
        , m_original_Speed, m_Attack_Speed_Cooldown = 1f, m_Time_To_Reach, m_Damage_Cooldown, m_DC_Max_Original, m_Electric_Damage_Cooldown, 
        m_orig_attack_Cooldown, m_Dash_Duration, m_Current_Dash_Duration, m_invulnerability_Dur, m_Cur_Invul_Dur, m_Stun_Duration, m_Cur_Stun_Dur;
    
    [HideInInspector] public GameObject m_Owned_Ball;
    private Player m_Player;
    private bool m_can_Catch_Ball = true, m_Is_Holding_Lob = false, m_Is_Tackling = false, m_Is_Slide_Tackling = false
        , m_Read_Player_Inputs = true, m_Has_Attacked = false, m_Is_Attacking = false, m_Taking_Damage = false, m_Is_Dashing = false;
    [HideInInspector] public bool m_Can_Be_Attacked = true, m_Is_Being_Stunned = false, m_Is_Hit_Vertically = false;
    private ParticleSystem impact_PS, Electrify_PS;

    [SerializeField] private bool m_Is_Moving, m_Is_Being_Passed_To = false;
    private Vector3 m_Ball_End_Position, damage_Dir;

    

    private void Awake()
    {

        //impact_PS = transform.Find("Hit_Effect").GetComponent<ParticleSystem>();
        //Electrify_PS = transform.Find("Electrified_Particles").GetComponent<ParticleSystem>();
        //Stop_Shock_Particles();

        //TEMPORARY
        transform.name = "Test_Player_Team_" + Team_ID;
    }

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        m_Player = ReInput.players.GetPlayer(Player_ID-1);
        m_Orig_Cooldown = m_Ball_Throw_Cooldown;
        m_original_Speed = speed;
        m_Attack_Speed_Cooldown = attack_Speed_Cooldown_Max;
        m_DC_Max_Original = damage_Cooldown_Max;
        m_Electric_Damage_Cooldown = damage_Cooldown_Max + 1.5f;
        m_orig_attack_Cooldown = attack_Cooldown;   

    }

    private void FixedUpdate()
    {
        if (m_Read_Player_Inputs) {
            Movement();
        } else
        {
            if (m_Is_Dashing && !m_Is_Being_Stunned) {
                Do_Dash(transform.forward);
            } else if (m_Taking_Damage && !m_Is_Being_Stunned)
            {
                Do_Dash(damage_Dir);
                Debug.Log("Should be knocked back: " +m_Taking_Damage);
                Debug.Log("Should be Stunned back: " + m_Is_Being_Stunned);
            }
            else if (m_Is_Being_Stunned)
            {
                m_Horizontal_Comp = 0;
                m_Vertical_Comp = 0;
                Debug.Log("Should be Stunned back: " + m_Is_Being_Stunned);
            }
        }

        if (!m_Is_Grounded() && !m_Taking_Damage && !m_Is_Hit_Vertically)
        {
            vel.y -= gravity * Time.deltaTime;
        }
        else if(m_Is_Grounded() && !m_Taking_Damage && !m_Is_Hit_Vertically)
        {
            vel.y = 0;
        }

        rb.MovePosition(rb.position + new Vector3(Mathf.Clamp(vel.x, -speed, speed),
            vel.y, Mathf.Clamp(vel.z, -speed, speed)) * m_speed_Modifier * Time.deltaTime);

    }

    // Update is called once per frame
    void Update() {
        m_Horizontal_Comp = m_Player.GetAxisRaw("Horizontal");
        m_Vertical_Comp = m_Player.GetAxisRaw("Vertical");

        Check_Inputs();
        Check_Cooldowns();
        
    }




    void Movement()
    {
        
        m_Input_X = Mathf.Lerp(m_Input_X, m_Horizontal_Comp, Time.deltaTime * Input_Speed);
        m_Input_Y = Mathf.Lerp(m_Input_Y, m_Vertical_Comp, Time.deltaTime * Input_Speed);
        

        vel.x = m_Input_X * speed;
        vel.z = m_Input_Y * speed;


        


        m_speed_Modifier = (m_Player.GetButton("Sprint") && m_Owned_Ball != null) ? sprint_speed_Mod : 1;
        


        ///change forward direction
        Vector3 tempDir = new Vector3(m_Input_X, 0, m_Input_Y);


        if (tempDir.magnitude > 0.1f)
        {
            rayDir = tempDir.normalized;
        }
        if (m_Is_Grounded())
        {
            transform.forward = Vector3.Slerp(transform.forward, rayDir, Time.deltaTime * rot_Mod);
        }

        

        


    }

    public void Do_Dash(Vector3 _dash_Dir)
    {
        vel.x = _dash_Dir.x * speed;
        vel.z = _dash_Dir.z * speed;

        if (m_Is_Hit_Vertically)
        {
            vel.y = _dash_Dir.y * speed;
        }


       
    }

    

    void Check_Inputs()
    {
        //press w/o ball to pass, press w/ ball to swap players
        if (m_Player.GetButtonDown("Dash") && m_Owned_Ball != null && !m_Wall_In_Front())
        {
            //throw ball at closer teammate
            Throw_Ball();
        }
        else if (m_Player.GetButtonDown("Dash") && m_Owned_Ball == null && player_Controlled)
        {
            //Swap players
            Tackle();
        }
        

        //hold to preform a lob pass
        m_Is_Holding_Lob = (m_Player.GetButton("S_Lob") && m_Owned_Ball != null) ? true : false;

        //press without ball to preform a MAUL
        if (m_Player.GetButtonDown("Throw") && m_Owned_Ball == null && player_Controlled && !m_Taking_Damage && !m_Has_Attacked)
        {
            //Maul
            Slide_Tackle();
        }

        

        //check whether or not player is moving
        if (Mathf.Abs(m_Horizontal_Comp) > 0.1f || Mathf.Abs(m_Vertical_Comp) > 0.1f)
        {
            m_Is_Moving = true;
        } else if (m_Horizontal_Comp == 0 && m_Vertical_Comp == 0)
        {
            m_Is_Moving = false;
        }


    }

    void Check_Cooldowns()
    {
        if (!m_can_Catch_Ball && m_Ball_Throw_Cooldown > 0)
        {
            m_Ball_Throw_Cooldown -= Time.deltaTime;
        }

        if (m_Ball_Throw_Cooldown <= 0 && !m_can_Catch_Ball)
        {
            m_can_Catch_Ball = true;
            m_Ball_Throw_Cooldown = m_Orig_Cooldown;
        }

        if (m_Is_Dashing && m_Current_Dash_Duration < m_Dash_Duration)
        {
            m_Current_Dash_Duration += Time.deltaTime;
        }

        if ((m_Is_Dashing && m_Current_Dash_Duration >= m_Dash_Duration) || m_Wall_In_Front())
        {
            Debug.Log($"Stop {name}'s dash");
            Reset_Dash_Variables();
        }

        if (!m_Can_Be_Attacked && m_Cur_Invul_Dur < m_invulnerability_Dur)
        {
            m_Cur_Invul_Dur += Time.deltaTime;
        }

        if (!m_Can_Be_Attacked && m_Cur_Invul_Dur >= m_invulnerability_Dur)
        {
            m_Can_Be_Attacked = true;
            m_Cur_Invul_Dur = 0;
        }


        if (m_Attack_Speed_Cooldown >= 0 && m_Has_Attacked)
        {
            m_Attack_Speed_Cooldown -= Time.deltaTime;
        }

        if (m_Attack_Speed_Cooldown <= 0 && m_Has_Attacked)
        {
            Reset_Speed();
        }


        if (m_Damage_Cooldown <= damage_Cooldown_Max && m_Taking_Damage)
        {
            m_Damage_Cooldown += Time.deltaTime;
            Debug.Log($"Knockback called by {name}");
            if (!m_Wall_In_Damage_Dir()) {
                float prc = m_Damage_Cooldown / damage_Cooldown_Max;
                speed = Mathf.Lerp(speed, 0, prc);

            } else
            {
                Hault_Speed();
                damage_Cooldown_Max = m_Electric_Damage_Cooldown;
               /* if (!Electrify_PS.isPlaying)
                {
                    Play_Shock_Particles();
               }*/
            }
        }

        if (m_Damage_Cooldown >= damage_Cooldown_Max && m_Taking_Damage)
        {
            m_Damage_Cooldown = 0;
            if (damage_Cooldown_Max != m_DC_Max_Original) damage_Cooldown_Max = m_DC_Max_Original;
            m_Taking_Damage = false;
            m_Read_Player_Inputs = true;
            Slow_Speed();
          /*if (Electrify_PS.isPlaying && !m_Is_Being_Stunned)
            {
                Stop_Shock_Particles();
            }*/
        }

        if (m_Cur_Stun_Dur < m_Stun_Duration && m_Is_Being_Stunned)
        {
            m_Cur_Stun_Dur += Time.deltaTime;
            Hault_Speed();
        }

        if (m_Cur_Stun_Dur >= m_Stun_Duration && m_Is_Being_Stunned)
        {
            Reset_Stun();
        }

       

    }

    public void Hault_Speed()
    {
        speed = 0;
    }

    public void Slow_Speed()
    {
        m_Has_Attacked = true;
        m_Is_Attacking = false;
        float _Slowed_Speed = m_original_Speed / 3f;
        speed = _Slowed_Speed;

        m_Attack_Speed_Cooldown = attack_Speed_Cooldown_Max;
    }

    public void Reset_Speed()
    {
        speed = m_original_Speed;
        m_Has_Attacked = false;
    }

    public void Initiate_Stun(float _Stun_Duration)
    {
        m_Stun_Duration = _Stun_Duration;
        m_Read_Player_Inputs = false;
        m_Is_Being_Stunned = true;
        Hault_Speed();
       // Play_Shock_Particles();
    }

    /// <summary>
    /// Reset variable
    /// </summary>
    private void Reset_Stun()
    {
        m_Read_Player_Inputs = true;
        m_Is_Being_Stunned = false;
        m_Cur_Stun_Dur = 0;
        Reset_Speed();
       // Stop_Shock_Particles();
    }

    public void Initiate_Invulnerability(bool _Is_Player_Invulnerable, float _Invul_Duration)
    {
        m_invulnerability_Dur = _Invul_Duration;
        m_Can_Be_Attacked = !_Is_Player_Invulnerable;
        
    }

    public void Initiate_Dash_Type(bool _Slide_Tackle, bool _Steal, bool _Ability_Use, float _duration, float _speed)
    {
        m_Is_Slide_Tackling = _Slide_Tackle;
        m_Is_Tackling = _Steal;
        m_Is_Dashing = true;
        if (m_Is_Slide_Tackling || m_Is_Tackling)
        {
            m_Is_Attacking = true;
        }else
        {
            m_Is_Attacking = false;
        }
        
        m_Read_Player_Inputs = false;
        m_Dash_Duration = _duration;
        speed = _speed;
    }

    void Reset_Dash_Variables()
    {
        m_Current_Dash_Duration = 0;
        if (m_Is_Hit_Vertically) m_Is_Hit_Vertically = false;
        m_Is_Slide_Tackling = false;
        m_Is_Tackling = false;
        m_Is_Dashing = false;
        m_Read_Player_Inputs = true;
        Slow_Speed();
    }

    void Tackle()
    {
        float _tackle_Speed = m_original_Speed * tackle_Speed_Mod;
        Initiate_Dash_Type(false, true, false, tackle_Dur_Max, _tackle_Speed);
    }

    void Slide_Tackle()
    {
        float _tackle_Speed = m_original_Speed * slide_Tackle_Speed_Mod;
        Initiate_Dash_Type(true, false, false, slide_Tackle_Dur_Max, _tackle_Speed);
    }

    public void Swap_Possessor(GameObject _new_Owner)
    {
        m_Owned_Ball.transform.parent = null;
        _new_Owner.GetComponent<Player_Behaviour>().m_Owned_Ball = this.m_Owned_Ball;
        m_Owned_Ball.transform.parent = _new_Owner.GetComponent<Player_Behaviour>().Ball_Held_Pos;
        m_Owned_Ball.transform.position = _new_Owner.GetComponent<Player_Behaviour>().Ball_Held_Pos.position;
        m_Owned_Ball.GetComponent<Ball_Effects>().Set_Possession_ID_Num(_new_Owner.GetComponent<Player_Behaviour>().Team_ID);
        m_Owned_Ball = null;

    }

    

    /// <summary>
    /// apply force to the ball
    /// </summary>
    public void Throw_Ball()
    {
        m_can_Catch_Ball = false;
        
        m_Owned_Ball.transform.parent = null;
        if (!m_Taking_Damage) {
            m_Owned_Ball.GetComponent<Ball_Effects>().Play_Catch_Kick();
        }
        m_Owned_Ball.GetComponent<Rigidbody>().isKinematic = false;
        
         if (m_Taking_Damage)
        {
            Vector3 random_Dir = Vector3.zero;
            if (m_Wall_In_Damage_Dir()) {
                damage_Dir *= -1;
                random_Dir = new Vector3(damage_Dir.x + Random.Range(-0.4f, 0.4f), Random.Range(0f, 0.2f), damage_Dir.z + Random.Range(-0.4f, 0.4f));
            } else
            {
                random_Dir = new Vector3(damage_Dir.x + Random.Range(-0.4f, 0.4f), Random.Range(0f, 0.2f), damage_Dir.z + Random.Range(-0.4f, 0.4f));
            }
            float random_Force = Random.Range(ball_Force / 2, ball_Force);
            m_Owned_Ball.GetComponent<Rigidbody>().AddForce((random_Dir) * random_Force, ForceMode.Impulse);
            m_Owned_Ball.GetComponent<Ball_Effects>().Set_Ball_Variable(random_Force);
        }
        else
        {
            m_Owned_Ball.GetComponent<Rigidbody>().AddForce(transform.forward * ball_Force * m_speed_Modifier, ForceMode.Impulse);
            m_Owned_Ball.GetComponent<Ball_Effects>().Set_Ball_Variable(ball_Force);
        }

        m_Owned_Ball.GetComponent<Collider>().enabled = true;
        m_Owned_Ball.GetComponent<Ball_Effects>().Activate_Trail();
        m_Owned_Ball.GetComponent<Rigidbody>().useGravity = true;
        m_Owned_Ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        m_Owned_Ball.GetComponent<Ball_Effects>().Set_Possession_ID_Num(0);
        
        m_Owned_Ball = null;

        
    }

    public void Passed_To(Vector3 _end_Pos, float _time)
    {
        m_Is_Being_Passed_To = true;
        m_Ball_End_Position = _end_Pos;
        m_Time_To_Reach = _time;
    }

    public void Take_Damage(Vector3 _attacker_Pos, float _damage_Cooldown, bool _Is_Stealing, bool _knocked_Into_Air)
    {
        damage_Dir = (_attacker_Pos - transform.position) * -1f;
        m_Is_Hit_Vertically = _knocked_Into_Air;
        float _damage_Speed = 0;
        if (_knocked_Into_Air)
        {
            damage_Dir.y = 1;
            _damage_Speed = m_original_Speed * tackle_Speed_Mod;
        }
        else
        {
            _damage_Speed = m_original_Speed * tackle_Speed_Mod * 2f; 
        }
        speed = _damage_Speed;
        
        m_Taking_Damage = true;
        m_Read_Player_Inputs = false;
        damage_Cooldown_Max = _damage_Cooldown;

        if (m_Owned_Ball != null) {
            if (!_Is_Stealing) {
                Throw_Ball();
                
            }
            Camera_Controller.cam_Inst.Do_Camera_Shake(0.3f, 0.6f, 0);
        }
        else
        {
            Camera_Controller.cam_Inst.Do_Camera_Shake(0.2f, 0.4f, 0);
        }
        //impact_PS.Play();
    }



    

    

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ball" && other.gameObject.transform.parent == null && m_can_Catch_Ball && !m_Taking_Damage)
        {
            Pick_Up_Ball(other.gameObject);
        }


        if (other.gameObject.tag == "Player" && (m_Is_Tackling || m_Is_Slide_Tackling))
        {
            if (other.gameObject.GetComponent<Player_Behaviour>().m_Can_Be_Attacked) {
                if (m_Is_Tackling) {
                    other.gameObject.GetComponent<Player_Behaviour>().Take_Damage(transform.position, tackle_Damage_Cooldown, true, false);
                    if (other.gameObject.GetComponent<Player_Behaviour>().m_Owned_Ball != null) {
                        other.gameObject.GetComponent<Player_Behaviour>().Swap_Possessor(this.gameObject);
                    }
                    
                } else if (m_Is_Slide_Tackling)
                {
                    other.gameObject.GetComponent<Player_Behaviour>().Take_Damage(transform.position, slide_Tackle_Damage_Cooldown, false, false);
                    m_can_Catch_Ball = false;
                }
            }


        }

        rb.velocity = Vector3.zero;
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Ball" && other.gameObject.transform.parent == null && m_can_Catch_Ball && !m_Taking_Damage)
        {
            Pick_Up_Ball(other.gameObject);
            
        }

        if (other.gameObject.tag == "Player" && (m_Is_Tackling || m_Is_Slide_Tackling) 
            && !other.gameObject.GetComponent<Player_Behaviour>().m_Taking_Damage)
        {
            if (other.gameObject.GetComponent<Player_Behaviour>().m_Can_Be_Attacked)
            {
                if (m_Is_Tackling)
                {
                    other.gameObject.GetComponent<Player_Behaviour>().Take_Damage(transform.position, tackle_Damage_Cooldown, true, false);
                    if (other.gameObject.GetComponent<Player_Behaviour>().m_Owned_Ball != null)
                    {
                        other.gameObject.GetComponent<Player_Behaviour>().Swap_Possessor(this.gameObject);
                    }
                    
                }
                else if (m_Is_Slide_Tackling)
                {
                    other.gameObject.GetComponent<Player_Behaviour>().Take_Damage(transform.position, slide_Tackle_Damage_Cooldown, false, false);
                    m_can_Catch_Ball = false;
                }
            }


        }
    }

    private void OnCollisionExit(Collision collision)
    {
        rb.velocity = Vector3.zero;
    }


    

    void Pick_Up_Ball(GameObject other)
    {
        if (!player_Controlled)
        {
            player_Controlled = true;
        }

        
        m_Owned_Ball = other.gameObject;
        m_Owned_Ball.GetComponent<Collider>().enabled = false;
        m_Owned_Ball.GetComponent<Rigidbody>().useGravity = false;
        m_Owned_Ball.transform.parent = Ball_Held_Pos;
        m_Owned_Ball.transform.position = Ball_Held_Pos.position;
        m_Owned_Ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        m_Owned_Ball.GetComponent<Rigidbody>().isKinematic = true;

        m_Owned_Ball.GetComponent<Ball_Effects>().Deactivate_Trail();
        m_Owned_Ball.GetComponent<Ball_Effects>().Play_Catch_Kick();
        m_Owned_Ball.GetComponent<Ball_Effects>().Set_Possession_ID_Num(Team_ID);
        m_Owned_Ball.GetComponent<Ball_Effects>().Reset_Force_Variables();
    }



    bool m_Is_Grounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, ground_Ray_Rad, Vector3.down, out hit, ground_Ray_Dist))
        {
            if (hit.collider.tag == "Ground")
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

    

    
    

    public void Set_Min_Max(float _min_Z, float _max_Z, float _min_X, float _max_X)
    {
        min_Z = _min_Z;
        max_Z = _max_Z;
        min_X = _min_X;
        max_X = _max_X;
    }


    /// <summary>
    /// EFFECTS ARE AFTER THIS POINT, particle systems turn on and off here.
    /// </summary>
    
    /*void Play_Shock_Particles()
    {
        Electrify_PS.Play();
    }

    void Stop_Shock_Particles()
    {
        Electrify_PS.Stop();
    }*/

    ///////////////////////////////// GIZMO DRAWS
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * ground_Ray_Dist, ground_Ray_Rad);
        Gizmos.DrawRay(transform.position, damage_Dir);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * ground_Ray_Dist, ground_Ray_Rad * 1.5f);
        
    }
}
