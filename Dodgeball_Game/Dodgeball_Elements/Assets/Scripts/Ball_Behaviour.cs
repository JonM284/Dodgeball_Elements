using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball_Behaviour : MonoBehaviour
{

    

    public TrailRenderer trail;

    private Rigidbody rb;

    private Vector3 starting_Pos;

    public float bounce_Force;

    private int m_Throw_Level;

    private bool extra_Bounce;


    private void Start()
    {
        Deactivate_Trail();
        rb = GetComponent<Rigidbody>();
    }

    public void Assign_Level(int _level)
    {
        m_Throw_Level = _level;
        if (_level == 3)
        {
            extra_Bounce = true;
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


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Floor")
        {
            rb.isKinematic = true;
            
            Deactivate_Trail();
        }
    }



}
