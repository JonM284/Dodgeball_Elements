using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Handler : MonoBehaviour
{

    private Animator m_Anim;
    private Player_Movement m_Player_Mov;

    public enum Player_States
    {
        IDLE,
        MOVING,
        DASHING,
        THROWING,
        CHARGING_THROW,
        MELEEING,
        EFFECTED


    };

    public Player_States current_State;

    // Start is called before the first frame update
    void Start()
    {
        m_Anim = GetComponent<Animator>();
        m_Player_Mov = GetComponent<Player_Movement>();
    }

    // Update is called once per frame
    public void Custom_Update()
    {
        if ((Mathf.Abs(m_Player_Mov.m_Horizontal_Comp) >= 0.1f || Mathf.Abs(m_Player_Mov.m_Vertical_Comp) >= 0.1f)
            && m_Player_Mov.speed > 0 && !m_Player_Mov.m_holding_Throw && !m_Player_Mov.m_Player_Dashing
             && !m_Player_Mov.m_Is_Meleeing)
        {
            current_State = Player_States.MOVING;
        }else if ((Mathf.Abs(m_Player_Mov.m_Horizontal_Comp) == 0 && Mathf.Abs(m_Player_Mov.m_Vertical_Comp) == 0)
            && m_Player_Mov.speed > 0 && !m_Player_Mov.m_holding_Throw && !m_Player_Mov.m_Player_Dashing
             && !m_Player_Mov.m_Is_Meleeing)
        {
            current_State = Player_States.IDLE;
        }

        if (m_Player_Mov.m_holding_Throw && m_Player_Mov.ball_Held_Prc < 0.08f)
        {
            current_State = Player_States.THROWING;
        }
        else if (m_Player_Mov.m_holding_Throw && m_Player_Mov.ball_Held_Prc >= 0.08f)
        {
            current_State = Player_States.CHARGING_THROW;
        }


        Run_Animations();
    }

    void Run_Animations()
    {
        m_Anim.SetInteger("Animation_State", (int)current_State);
        m_Anim.SetBool("Effected", m_Player_Mov.m_Is_Effected);
        m_Anim.SetBool("Throwing", m_Player_Mov.m_holding_Throw);
    }

    public bool Is_Still_Holding_Throw()
    {
        return m_Player_Mov.m_holding_Throw;
    }

    /// <summary>
    /// Set trigger to go on for quick animations like releasing a throw, or meleeing
    /// </summary>
    /// <param name="_Quick_Animation_Trigger_Int">0 = release throw, 1 = melee</param>
    public void Quick_Animation_Trigger(int _Quick_Animation_Trigger_Int)
    {
        switch (_Quick_Animation_Trigger_Int)
        {
            case 1:
                m_Anim.SetTrigger("Melee");
                break;
            case 0:
                m_Anim.SetTrigger("Release_Throw");
                break;
        }
    }


    /// <summary>
    /// Set trigger to go off for quick animations like releasing a throw, or meleeing
    /// </summary>
    /// <param name="_Quick_Animation_Trigger_Int">0 = release throw, 1 = melee</param>
    public void Quick_Reset_Animation_Trigger(int _Quick_Animation_Trigger_Reset_Int)
    {
        switch (_Quick_Animation_Trigger_Reset_Int)
        {
            case 1:
                m_Anim.ResetTrigger("Melee");
                break;
            case 0:
                m_Anim.ResetTrigger("Release_Throw");
                break;
        }
    }
}
