using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball_Effects : MonoBehaviour {

    [Tooltip("Catch and throw VFX")]
    public ParticleSystem Catch_Kick_Effect;
    [Tooltip("Trail to play while ball is flying in the air.")]
    public TrailRenderer trail;

    private Rigidbody rb;

    private Vector3 starting_Pos;

    [Tooltip("Max amount of force to apply when hitting a wall")]
    public float Max_Bounce_Force;
    public float Max_Force_Timer;

    //Force amount to give ball when hitting a wall (goes down over time)
    private float m_Current_Bounce_Force;
    //
    private float m_Current_Bounce_Force_Timer = 0;
    //Is the call being held currently?
    private bool m_Is_Being_Held = false;
    //was this ball just recently thrown?
    private bool m_Was_Thrown = false;

    [SerializeField]
    private Payload_Manager m_Payload_Manager;
    
    private void Start()
    {
        Deactivate_Trail();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (m_Was_Thrown && m_Current_Bounce_Force_Timer < Max_Force_Timer)
        {
            m_Current_Bounce_Force_Timer += Time.deltaTime;
            float prc = m_Current_Bounce_Force_Timer / Max_Force_Timer;
            m_Current_Bounce_Force = Mathf.Lerp(Max_Bounce_Force, 0, prc);
        }

        if (m_Was_Thrown && m_Current_Bounce_Force_Timer >= Max_Force_Timer)
        {
            Reset_Force_Variables();
        }
    }

    private void LateUpdate()
    {
        if (transform.position.y < -1f)
        {
            transform.position = starting_Pos;
        }
    }

    public void Activate_Trail()
    {
        trail.enabled = true;
    }

    public void Deactivate_Trail()
    {
        trail.enabled = false;
    }

    public void Set_Possession_ID_Num(int _ID)
    {
        m_Payload_Manager.Set_Possession_Team_ID(_ID);
    }
    

    public void Play_Catch_Kick()
    {
        Catch_Kick_Effect.Play();
        m_Is_Being_Held = !m_Is_Being_Held;
    }

    public void Reset_Force_Variables()
    {
        m_Was_Thrown = false;
        m_Current_Bounce_Force_Timer = 0;
        m_Current_Bounce_Force = 0;
    }

    public void Set_Ball_Variable(float _initial_Throw_Force)
    {
        m_Was_Thrown = true;
        Max_Bounce_Force = _initial_Throw_Force;
    }

    /*private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            Vector3 _dir = other.contacts[0].point - transform.position;
            Vector3 _opposite_Dir = Vector3.Reflect(_dir, other.contacts[0].normal);
            rb.AddForce(_opposite_Dir * m_Current_Bounce_Force);
            
        }
    }*/



}
