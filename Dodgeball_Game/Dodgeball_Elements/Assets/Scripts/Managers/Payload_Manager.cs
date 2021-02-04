using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Payload_Manager : MonoBehaviour
{
    [HideInInspector]
    public float speed;

    //Change speed depending on the amount of players that are pushing the payload max 3 speeds
    [SerializeField]
    private float[] variable_Speeds;

    //Is the main ball inside the payload zone?
    private bool m_Ball_In_Zone = false;
    private int m_Current_Possession_Team_ID;
    private float m_Max_Distance;
    //rigidbody reference
    private Rigidbody rb;
    //Velocity of this payload
    private Vector3 vel;

    //check distance
    [SerializeField]
    private Transform m_Ball_Position, m_Distance_Check;

    private Color m_Debug_Color;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        speed = variable_Speeds[0];
        m_Max_Distance = Vector3.Distance(this.transform.position, m_Distance_Check.position);
    }

    private void Update()
    {
        if (Vector3.Distance(m_Ball_Position.position, this.transform.position) <= m_Max_Distance)
        {
            if (!m_Ball_In_Zone) m_Ball_In_Zone = true;
            m_Debug_Color = Color.green;

        }
        else
        {
            if (m_Ball_In_Zone) m_Ball_In_Zone = false;
            m_Debug_Color = Color.red;
        }

        Debug.DrawLine(transform.position, m_Ball_Position.position, m_Debug_Color);

        if (m_Ball_In_Zone)
        {
            if(m_Current_Possession_Team_ID == 1)
            {
                vel.x = speed;
            }else if(m_Current_Possession_Team_ID == 2)
            {
                vel.x = -speed;
            }
        }else
        {
            if (vel.x != 0) vel.x = 0;
        }
    }
    
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Vector3.ClampMagnitude(vel, speed) * Time.deltaTime);
    }


    public void Set_Possession_Team_ID(int _ID)
    {
        m_Current_Possession_Team_ID = _ID;
    }

    
}
