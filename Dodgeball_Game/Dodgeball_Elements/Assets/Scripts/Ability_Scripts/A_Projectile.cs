using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Projectile : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: this script handles projectile ability movement
    /// </summary>


    [Header("Variables")]
    [Tooltip("How long after the particles have reached their end, for gameobject to turn itself off")]
    public float particle_Hangover_Time;

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

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Set the projectile to be able to move.
    /// </summary>
    /// <param name="_proj_Speed">How fast the projectile will move forward.</param>
    /// <param name="_duration">How long the projectile is active after launch.</param>
    /// <param name="_direction">Direction the projectile will move in. (forward)</param>
    /// <param name="_ignore_wall_Collision">Can the projectile ignore walls?</param>
    public void Setup_Projectile(float _proj_Speed, float _duration ,Vector3 _direction, bool _ignore_wall_Collision)
    {
        speed = _proj_Speed;
        duration = _duration;
        shoot_Dir = _direction;
        m_Ignore_Wall_Collision = _ignore_wall_Collision;
        Play_Startup_Particles();
        StopAllCoroutines();
        StartCoroutine(wait_Duration());
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

        try
        {
            GetComponent<Collider>().enabled = true;
        }
        catch
        {
            Debug.LogError("Projectile does not have a collider.");
        }
        


    }

    //stop duration particles, play projectile death particles
    void Play_End_Particles()
    {
        

        for (int i = 0; i < duration_Particles.Length; i++)
        {
            duration_Particles[i].Stop();
        }

        if (Main_Mesh_Object != null)
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

    // any physics updates
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + (shoot_Dir.normalized * speed) * Time.deltaTime);
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


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall" && !m_Ignore_Wall_Collision)
        {
            speed = 0;
            Play_End_Particles();
            StopAllCoroutines();
            StartCoroutine(wait_To_End());
        }
    }

}
