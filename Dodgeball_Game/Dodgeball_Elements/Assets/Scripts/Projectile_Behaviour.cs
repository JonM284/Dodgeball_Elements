using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Behaviour : MonoBehaviour
{
    /// <summary>
    /// Public variables
    /// </summary>
    [Header("Projectile speeds")]
    [Tooltip("Speed projectile will move at, corresponding to level it was thrown at.")]
    public float[] move_Speed = new float[3];
    [Header("Shown Projectile Object")]
    [Tooltip("Mesh object representing the projectile")]
    public GameObject mesh_Object;
    //Is this object actively able to attack other players?
    [HideInInspector]
    public bool is_Live;



    /// <summary>
    /// Private variables
    /// </summary>

    //Rigidbody of the projectile
    private Rigidbody rb;
    //direction projectile will move in
    private Vector3 shoot_Dir;
    //Is this object currently allowed to move?
    private bool can_Move;
    //Level player has charged to throw.
    private int m_throw_Level;

    public void Setup_Projectile(int _level, Vector3 _shoot_Dir)
    {
        rb = GetComponent<Rigidbody>();
        shoot_Dir = _shoot_Dir;
        m_throw_Level = _level;
        transform.forward = _shoot_Dir;
        can_Move = true;
        mesh_Object.GetComponent<Object_Rotator>().is_Active = true;
        GetComponent<Collider>().isTrigger = false;
    }

    private void FixedUpdate()
    {
        if (can_Move) {
            rb.MovePosition(rb.position + (shoot_Dir.normalized * move_Speed[m_throw_Level - 1]) * Time.deltaTime);
        }
    }

    /// <summary>
    /// Stop the projectile from moving, stop mesh object from rotating, turn collider into trigger, set parent.
    /// </summary>
    /// <param name="other">Game object collided with.</param>
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Ground")
        {
            can_Move = false;
            mesh_Object.GetComponent<Object_Rotator>().is_Active = false;
            rb.isKinematic = true;
            if (!GetComponent<Collider>().isTrigger) GetComponent<Collider>().isTrigger = true;
            if (other.transform.parent != null) transform.parent = other.transform.parent;
            if (rb.useGravity) rb.useGravity = false;
        }

       
    }

}
