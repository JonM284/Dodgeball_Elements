using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Reaction_Behaviour : MonoBehaviour
{

    [Header("Elemental Reaction Variables")]
    [SerializeField]
    [Tooltip("How long the player will be unable to throw or melee")]
    private float m_Disable_Timer;
    [SerializeField]
    [Tooltip("How long the player will be unable to move")]
    private float m_Stun_Timer;
    [SerializeField]
    [Tooltip("How long the player will be pulled in a random direction.")]
    private float m_Burn_Timer;
    [SerializeField]
    [Tooltip("How long the player will have their controls swapped")]
    private float m_Reversal_Timer;


    //current cooldown timers for reactions/ effects
    private float m_Cur_Disable_Timer, m_Cur_Stun_Timer, m_Cur_Burn_Timer, m_Cur_Reversal_Timer;
    //booleans for whether or not the player is affected by an element (individual due to many effects possible at once)
    private bool m_Is_Disabled, m_Is_Stunned, m_Is_Burned, m_Is_Reversed;

   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Check_Cooldowns();
    }


    void Check_Cooldowns()
    {
        //timer for player being disabled (can't throw, melee, or use abilities)
        if (m_Cur_Disable_Timer < m_Disable_Timer && m_Is_Disabled)
        {
            m_Cur_Disable_Timer += Time.deltaTime;
            //disable player
        }

        if (m_Cur_Disable_Timer >= m_Disable_Timer && m_Is_Disabled)
        {
            Reset_Disable_Variables();
        }

        //timer for player being Stunned (can't move)
        if (m_Cur_Stun_Timer < m_Stun_Timer && m_Is_Stunned)
        {
            m_Cur_Stun_Timer += Time.deltaTime;
            //stun player
        }

        if (m_Cur_Stun_Timer >= m_Stun_Timer && m_Is_Stunned)
        {
            Reset_Stun_Variables();
        }

        //timer for player being burned (will force player to move in random direction *with slight control* )
        if (m_Cur_Burn_Timer < m_Burn_Timer && m_Is_Burned)
        {
            m_Cur_Burn_Timer += Time.deltaTime;
            //burn player (repeatingly)
        }

        if (m_Cur_Burn_Timer >= m_Burn_Timer && m_Is_Burned)
        {
            Reset_Burn_Variables();
        }

        //timer for player being reversed (movement inputs or buttons reversed)
        if (m_Cur_Reversal_Timer < m_Reversal_Timer && m_Is_Reversed)
        {
            m_Cur_Reversal_Timer += Time.deltaTime;
            //reverse player controls
        }

        if (m_Cur_Reversal_Timer >= m_Reversal_Timer && m_Is_Reversed)
        {
            Reset_Reversal_Values();
        }
    }

    public void Initiate_Disable(float _Disable_Time)
    {
        m_Disable_Timer = _Disable_Time;
        m_Is_Disabled = true;
    }

    void Disable_Player()
    {

    }

    void Reset_Disable_Variables()
    {
        m_Cur_Disable_Timer = 0;
        m_Is_Disabled = false;
    }

    public void Initiate_Stun(float _Stun_Time)
    {
        m_Stun_Timer = _Stun_Time;
        m_Is_Stunned = true;
    }

    void Reset_Stun_Variables()
    {
        m_Cur_Stun_Timer = 0;
        m_Is_Stunned = false;
    }

    public void Initiate_Burn(float _Burn_Time)
    {
        m_Burn_Timer = _Burn_Time;
        m_Is_Burned = true;
    }

    void Reset_Burn_Variables()
    {
        m_Cur_Burn_Timer = 0;
        m_Is_Burned = false;
    }

    public void Initiate_Input_Reversal(float _Reversal_Time)
    {
        m_Reversal_Timer = _Reversal_Time;
        m_Is_Reversed = true;
    }

    void Reset_Reversal_Values()
    {
        m_Cur_Reversal_Timer = 0;
        m_Is_Reversed = false;
    }

}
