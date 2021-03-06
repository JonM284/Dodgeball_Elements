﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class A_Projectile : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: this script handles projectile ability movement
    /// </summary>


    [Header("Variables")]
    [Tooltip("How long after the particles have reached their end, for gameobject to turn itself off")]
    public float particle_Hangover_Time;
    [HideInInspector]
    public string Ability_Name;
    //reference to this objects rigidbody
    private Rigidbody rb;
    //how fast the projectile will be moving
    private float speed;
    //direction object was shot in.
    private Vector3 shoot_Dir;
    //duration of projectile if it does not hit anything
    private float duration;
    //does this projectile ignore collisions with walls?
    private bool m_Ignore_Wall_Collision;
    //does this projectile hit collide with players?
    private bool m_Ignore_Player_Hit;
    //elemental ID
    private int element_ID;
    //how long this wil effect hit players
    private float effect_Duration;

    [Header("Particle Systems")]
    [Tooltip("particles to be played when instantiated or brought in through OPS (object pool spawner)")]
    public ParticleSystem[] startup_Particles;
    [Tooltip("particles to be played in the duration of the particles lifetime.")]
    public ParticleSystem[] duration_Particles;
    [Tooltip("particles to be played when the particles reach it's end.")]
    public ParticleSystem[] end_Particles;
    [Header("ONLY IF APPLICABLE")]
    [Tooltip("Particle system containing projectile mesh")]
    public GameObject Main_Mesh_Object;
    [Tooltip("Is this gameobject the rockwall projectile?")]
    public bool is_Rock_Wall;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Set the projectile to be able to move. (script called in: Projectile_Ability)
    /// </summary>
    /// <param name="_proj_Speed">How fast the projectile will move forward.</param>
    /// <param name="_duration">How long the projectile is active after launch.</param>
    /// <param name="_direction">Direction the projectile will move in. (forward)</param>
    /// <param name="_ignore_wall_Collision">Can the projectile ignore walls?</param>
    /// <param name= "_ignore_Player_Hit">Does the projectile stop when it hits a player?</param>param>
    public void Setup_Projectile(float _proj_Speed, float _duration ,Vector3 _direction, bool _ignore_wall_Collision, bool _ignore_Player_Hit, int _Element_ID, float _effect_Duration, string _name)
    {
        speed = _proj_Speed;
        duration = _duration;
        shoot_Dir = _direction;
        element_ID = _Element_ID;
        effect_Duration = _effect_Duration;
        Ability_Name = _name;
        Debug.Log(element_ID);
        m_Ignore_Wall_Collision = _ignore_wall_Collision;
        Play_Startup_Particles();
        StopAllCoroutines();
        StartCoroutine(wait_Duration());
        StartCoroutine(wait_To_Detect(0.1f));
    }


    /// <summary>
    /// Change all colors for start, duration, and end particles.
    /// </summary>
    /// <param name="_New_Color">Color particles will be changed to.</param>
    public void Change_All_Particle_Color(Color _New_Color)
    {
        for (int i = 0; i < startup_Particles.Length; i++)
        {
            var main = startup_Particles[i].main;
            main.startColor = _New_Color;
        }

        for (int i = 0; i < duration_Particles.Length; i++)
        {
            var main = duration_Particles[i].main;
            main.startColor = _New_Color;
        }

        for (int i = 0; i < end_Particles.Length; i++)
        {
            var main = end_Particles[i].main;
            main.startColor = _New_Color;
        }

        
    }

    //play launch particles and duration particles.
    void Play_Startup_Particles()
    {

        for (int i = 0; i < startup_Particles.Length; i++)
        {
            startup_Particles[i].Play();
        }


        for (int i = 0; i < duration_Particles.Length; i++)
        {
            duration_Particles[i].Play();
        }

        
        


    }

    //stop duration particles, play projectile death particles
    void Play_End_Particles()
    {
        

        for (int i = 0; i < duration_Particles.Length; i++)
        {
            duration_Particles[i].Stop();
        }

        if (Main_Mesh_Object != null & is_Rock_Wall)
        {
            Main_Mesh_Object.GetComponent<Animator>().SetBool("Rock_Wall_Up", false);
        }

        for (int i = 0; i < end_Particles.Length; i++)
        {
            end_Particles[i].Play();
        }

        try
        {
            GetComponent<Collider>().enabled = false;
        }
        catch
        {
            Debug.LogError("Projectile does not have a collider.");
        }
    }

    void End_Projectile()
    {
        speed = 0;
        Play_End_Particles();
        StopAllCoroutines();
        StartCoroutine(wait_To_End());
    }

    // any physics updates
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + (shoot_Dir.normalized * speed) * Time.deltaTime);
    }


    private void Apply_Effect_Particles(GameObject _effected_Gameobject)
    {
        GameObject spawned = null;
        spawned = Object_Pool_Spawner.spawner_Instance.SpawnFromPool(Ability_Name + "_Effect_Particles", _effected_Gameobject.transform.position, Quaternion.identity);
        spawned.transform.parent = _effected_Gameobject.transform;
        spawned.GetComponent<Effect_Shut_Off_Wait>().Start_Effect_Time(effect_Duration);
    }

    IEnumerator wait_To_Detect(float _wait_Time)
    {
        yield return new WaitForSeconds(_wait_Time);
        try
        {
            GetComponent<Collider>().enabled = true;
        }
        catch
        {
            Debug.LogError("Projectile does not have a collider.");
        }
    }

    /// <summary>
    /// Extra time to wait after end particles have fired off. ie. set gameobject inactive after x seconds
    /// </summary>
    /// <returns></returns>
    IEnumerator wait_To_End()
    {
        StopCoroutine(wait_Duration());
        yield return new WaitForSeconds(particle_Hangover_Time);
        gameObject.SetActive(false);
        
    }

    /// <summary>
    /// play projectile death particles if the timer reaches 0 and has not hit a wall
    /// </summary>
    /// <returns></returns>
    IEnumerator wait_Duration()
    {
        yield return new WaitForSeconds(duration);
        Play_End_Particles();
        speed = 0;
        StartCoroutine(wait_To_End());
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall" && !m_Ignore_Wall_Collision)
        {
            End_Projectile();
        }

        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall" && !m_Ignore_Wall_Collision)
        {
            End_Projectile();
        }

        if (other.gameObject.tag == "Player" && !m_Ignore_Player_Hit)
        {
            if(element_ID != 0){
                other.gameObject.GetComponent<Player_Movement>().Reset_Player_Effect();
                other.gameObject.GetComponent<Player_Movement>().Initiate_Player_Effect(element_ID, effect_Duration);
                Apply_Effect_Particles(other.gameObject);
            }
            End_Projectile();
        }
    }



}
