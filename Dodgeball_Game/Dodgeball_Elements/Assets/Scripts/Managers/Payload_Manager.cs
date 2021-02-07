using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Payload_Manager : MonoBehaviour
{
    [HideInInspector]
    public float speed;
    [Header("Payload Model Rotation")]
    [Tooltip("Payload rotation script")]
    public Object_Rotator payload_Rotator;
    [Header("Payload Zone Colors")]
    [Tooltip("Payload zone script")]
    public SpriteRenderer zone_Sprite;
    public Color team_1_Color, team_2_Color;
    public float color_Change_Timer_Max;

    //Change speed depending on the amount of players that are pushing the payload max 3 speeds
    [SerializeField]
    private float[] variable_Speeds;

    //Is the main ball inside the payload zone?
    private bool m_Ball_In_Zone = false;
    private bool change_Color = false;
    private int m_Current_Possession_Team_ID;
    private float m_Max_Distance;
    //rigidbody reference
    private Rigidbody rb;
    //Velocity of this payload
    private Vector3 vel;
    private float m_Current_Color_Change_Timer;
    //check distance
    [SerializeField]
    private Transform m_Ball_Position, m_Distance_Check;
    private Color m_Current_Color, m_Old_Color;
    private Color m_Neutral_Color;


    private Color m_Debug_Color;

    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        speed = variable_Speeds[0];
        m_Max_Distance = Vector3.Distance(this.transform.position, m_Distance_Check.position);
        m_Neutral_Color = zone_Sprite.color;
        m_Current_Color = m_Neutral_Color;
        m_Old_Color = m_Neutral_Color;
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

      

        if (m_Ball_In_Zone)
        {
            if(m_Current_Possession_Team_ID == 1)
            {
                vel.x = speed;
                if (payload_Rotator.rotation_Speed != -payload_Rotator.m_Original_Rot_Speed)
                {
                    payload_Rotator.Set_Speed(-payload_Rotator.m_Original_Rot_Speed);
                    Change_Zone_Color(m_Current_Color, team_1_Color);
                }
            }
            else if(m_Current_Possession_Team_ID == 2)
            {
                vel.x = -speed;
                if (payload_Rotator.rotation_Speed != payload_Rotator.m_Original_Rot_Speed)
                {
                    payload_Rotator.Set_Speed(payload_Rotator.m_Original_Rot_Speed);
                    Change_Zone_Color(m_Current_Color, team_2_Color);
                }

            }else if(m_Current_Possession_Team_ID == 0)
            {
                if (vel.x != 0) vel.x = 0;
                if (payload_Rotator.rotation_Speed != 0)
                {
                    payload_Rotator.Set_Speed(0);
                    Change_Zone_Color(m_Current_Color, m_Neutral_Color);
                }

            }
        }
        else
        {
            if (vel.x != 0) vel.x = 0;
            if (payload_Rotator.rotation_Speed != 0)
            {
                payload_Rotator.Set_Speed(0);
                Change_Zone_Color(m_Current_Color, m_Neutral_Color);
            }

        }
    }
    
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + Vector3.ClampMagnitude(vel, speed) * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (change_Color && m_Current_Color_Change_Timer < color_Change_Timer_Max)
        {
            m_Current_Color_Change_Timer += Time.deltaTime;
            float prc = m_Current_Color_Change_Timer / color_Change_Timer_Max;
            zone_Sprite.color = Color.Lerp(m_Old_Color, m_Current_Color, prc);
        }

        if (change_Color && m_Current_Color_Change_Timer >= color_Change_Timer_Max)
        {
            Reset_Color_Change_Variables();
        }
    }


    public void Change_Zone_Color(Color _old_Color, Color _new_Color)
    {
    
        change_Color = true;
        m_Current_Color_Change_Timer = 0;
        m_Current_Color = _new_Color;
        m_Old_Color = _old_Color;
        
    }

    void Reset_Color_Change_Variables()
    {
        change_Color = false;
        m_Current_Color_Change_Timer = 0; 
    }

    public void Set_Possession_Team_ID(int _ID)
    {
        m_Current_Possession_Team_ID = _ID;
        Debug.Log(_ID);
    }

    
}
