using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Behaviour : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: this script handles THROWABLE OBJECTS (not ability projectile) 
    /// </summary>

    /// <summary>
    /// Public variables
    /// </summary>
    [Header("Projectile speeds")]
    [Tooltip("Speed projectile will move at, corresponding to level it was thrown at.")]
    public float[] move_Speed = new float[3];
    [Header("Object children")]
    [Tooltip("Mesh object representing the projectile")]
    public GameObject mesh_Object;
    [Tooltip("Trail renderer for when object is thrown")]
    public TrailRenderer m_Trail;
    [Tooltip("Spawning objects rate, higher = faster")]
    public float fire_Rate;
    [Tooltip("Spawning object")]
    public GameObject Element_Trail;
    
    //Is this object actively able to attack other players?
    [HideInInspector]
    public bool is_Live;
    //name to pass to get the correct passive
    [HideInInspector]
    public string trail_Type_Name;
    [HideInInspector]
    public int player_Thrown_ID;


    /// <summary>
    /// Private variables
    /// </summary>
    /// 

    //Rigidbody of the projectile
    private Rigidbody rb;
    //direction projectile will move in
    private Vector3 shoot_Dir;
    //Is this object currently allowed to move?
    private bool can_Move;
    //Level player has charged to throw.
    [SerializeField]
    private int m_throw_Level;
    //modifiable speed
    private float mod_Speed;
    //next time to instantiate
    private float next_Time_To_Fire;
    //offset to spawn objects, set in ability
    [SerializeField]
    private Vector3 spawn_Offset;

    [Header("Effects")]
    [Tooltip("Spark particles that play when hitting a wall.")]
    [SerializeField]
    private ParticleSystem spark_Particles;
    


    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (m_Trail == null) m_Trail = transform.GetChild(1).GetComponent<TrailRenderer>();
    }

    /// <summary>
    /// Set up projectile to be thrown.
    /// </summary>
    /// <param name="_level">Throw level.</param>
    /// <param name="_shoot_Dir">Direction player facing.</param>
    /// <param name="_New_ID">Player ID</param>
    public void Setup_Projectile(int _level, Vector3 _shoot_Dir, int _New_ID)
    {
        player_Thrown_ID = _New_ID;
        shoot_Dir = _shoot_Dir;
        m_throw_Level = _level;
        transform.forward = _shoot_Dir;
        can_Move = true;
        mod_Speed = move_Speed[m_throw_Level - 1];
        mesh_Object.GetComponent<Object_Rotator>().is_Active = true;
        GetComponent<Collider>().isTrigger = true;
        is_Live = true;
        if(rb == null) rb = GetComponent<Rigidbody>();
        if (m_Trail == null) m_Trail = transform.GetChild(1).GetComponent<TrailRenderer>();
        if (!m_Trail.emitting) m_Trail.emitting = true;
    }

    /// <summary>
    /// Change current passive to be used.
    /// </summary>
    /// <param name="_New_Trail">New passive prefab</param>
    public void Change_Passive(GameObject _New_Trail)
    {
        Element_Trail = _New_Trail;
    }

    /// <summary>
    /// Change ID when reflecting.
    /// </summary>
    /// <param name="_New_ID">Player ID</param>
    void Change_ID(int _New_ID)
    {
        player_Thrown_ID = _New_ID;
    }

    /// <summary>
    /// Change color of this projectile to new players color
    /// </summary>
    /// <param name="_new_Color">Player Color</param>
    public void Change_Color(Color _new_Color)
    {
        mesh_Object.GetComponent<MeshRenderer>().materials[0].color = _new_Color;
        var main = spark_Particles.main;
        main.startColor = _new_Color;
    }

    /// <summary>
    /// all physics updates
    /// </summary>
    private void FixedUpdate()
    {
        if (can_Move) {
            rb.MovePosition(rb.position + (shoot_Dir.normalized * mod_Speed) * Time.deltaTime);
        }
    }

    /// <summary>
    /// non-physics based updates
    /// </summary>
    private void Update()
    {
        if (m_throw_Level == 3 && can_Move && Time.time > next_Time_To_Fire)
        {
            next_Time_To_Fire = Time.time + 1f / fire_Rate;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
            {
                if (hit.transform.tag == "Ground")
                {
                    Object_Pool_Spawner.spawner_Instance.SpawnFromPool(trail_Type_Name+"_Passive", hit.point + spawn_Offset, Quaternion.identity);
                }
            }
            
        }

        if (can_Move)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, shoot_Dir, out hit, 3f))
            {
                if (hit.collider.tag == "Wall")
                {
                    transform.position = hit.point + (hit.normal * 0.5f);
                    spark_Particles.gameObject.transform.forward = hit.normal;
                    Stop_Projectile(hit.transform.gameObject);
                }
            }
        }

        
    }

    /// <summary>
    /// Stop this projectile.
    /// </summary>
    /// <param name="_other">Colliding gameobject</param>
    void Stop_Projectile(GameObject _other)
    {
        spark_Particles.Play();
        is_Live = false;
        can_Move = false;
        mesh_Object.GetComponent<Object_Rotator>().is_Active = false;
        rb.isKinematic = true;
        mod_Speed = 0;
        if (!GetComponent<Collider>().isTrigger) GetComponent<Collider>().isTrigger = true;
        if (_other.transform.parent != null) transform.parent = _other.transform.parent;
        if (rb.useGravity) rb.useGravity = false;
        if (m_Trail.emitting) m_Trail.emitting = false;
    }

    /// <summary>
    /// Stop the projectile from moving, stop mesh object from rotating, turn collider into trigger, set parent.
    /// </summary>
    /// <param name="other">Game object collided with.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Ground")
        {
            Stop_Projectile(other.gameObject);
            
        }


        if (other.gameObject.tag == "Melee")
        {
            mod_Speed *= -1;
            Change_ID(other.gameObject.GetComponentInParent<Player_Movement>().player_ID);
            Change_Color(other.gameObject.GetComponentInParent<Player_Movement>().player_Color);
        }
       
    }

    


   

}
