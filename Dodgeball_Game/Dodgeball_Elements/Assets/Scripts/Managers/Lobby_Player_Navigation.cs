using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Lobby_Player_Navigation : MonoBehaviour
{
    
    ///This Code deals with individual player UI Navigation. Used on lobby scene, with Lobby_Manager script.

    /// <summary>
    /// Public Variables.
    /// </summary>
    [Tooltip("ID number assigned to this player, 1-8.")]
    public int Player_ID;
    [Tooltip("How many rows can be found in the current UI?")]
    public int rows, columns;
    [Tooltip("Player Blip Navigation: character icon spaces.")]
    public GameObject[] character_Spots;
    [Tooltip("Player Blip")]
    public GameObject player_Blip;

    /// <summary>
    /// Private Variables.
    /// </summary>
    //this players horizontal and vertical input data.
    private float m_horizontal_Input, m_vertical_Input;
    [SerializeField]
    [Tooltip("How far the player has to move their analog to make a move.")]
    private float m_controller_Input_Deadzone;
    //whether or not the player has made the decision to move.
    private bool m_move_select = false;
    //is the player currently active? aka controller plugged in and pressed join button.
    private bool m_player_Active = false;
    //Has the player selected their character? If so, wait for all players to pick. Afterward, move to element selection screen.
    private bool m_Selected = false;
    //Has the player changed cells?
    private bool m_Changed_Cell = false;
    //Current cell the player is in.
    private int m_Current_Cell_X = 0, m_Current_Cell_Y = 0;
  

    //Rewired required reference to access rewired libraries.
    private Player m_Player;


    // Start is called before the first frame update
    void Start()
    {
        m_Player = ReInput.players.GetPlayer(Player_ID - 1);
        if (player_Blip.activeInHierarchy) player_Blip.SetActive(false);
    }

    // Update is called once per frame
    public void Custom_Update()
    {
        if (!m_player_Active && m_Player.GetButtonDown("Dash") && !m_Selected)
        {
            m_player_Active = true;
            player_Blip.SetActive(true);
        }
        else if (m_player_Active && m_Player.GetButtonDown("Taunt") && !m_Selected)
        {
            m_player_Active = false;
            player_Blip.SetActive(false);
        }else if (m_player_Active && m_Player.GetButtonDown("Dash") && !m_Selected)
        {
            m_Selected = true;
        }
        else if (m_player_Active && m_Player.GetButtonDown("Taunt") && m_Selected)
        {
            m_Selected = false;
        }


        if (!m_Selected && m_player_Active)
        {
            m_horizontal_Input = m_Player.GetAxisRaw("Horizontal");
            m_vertical_Input = m_Player.GetAxisRaw("Vertical");
            Input_Navigation();
        }
    }

    void Input_Navigation()
    {
        if (m_horizontal_Input == 0 && m_vertical_Input == 0)
        {
            m_Changed_Cell = false;
            if (m_move_select)
            {
                m_move_select = false;
            }
        }

        if (Mathf.Abs(m_horizontal_Input) >= m_controller_Input_Deadzone || Mathf.Abs(m_vertical_Input) >= m_controller_Input_Deadzone)
        {
            if (!m_move_select)
            {
                m_move_select = true;
                //myPlayer.SetVibration(1, vibration_low, vibration_duration_low);
            }
        }

        //actual navigation based on input.
        if (m_horizontal_Input >= m_controller_Input_Deadzone && !m_Changed_Cell)
        {
            m_Changed_Cell = true;
            m_Current_Cell_X++;
            if (m_Current_Cell_X < 0)
            {
                m_Current_Cell_X = columns;
            }
            if (m_Current_Cell_X > columns)
            {
                m_Current_Cell_X = 0;
            }
            Update_Blip_Space(m_Current_Cell_X);
        }
        else if (m_horizontal_Input <= -m_controller_Input_Deadzone && !m_Changed_Cell)
        {
            m_Changed_Cell = true;
            m_Current_Cell_X--;
            if (m_Current_Cell_X < 0)
            {
                m_Current_Cell_X = columns;
            }
            if (m_Current_Cell_X > columns)
            {
                m_Current_Cell_X = 0;
            }
            Update_Blip_Space(m_Current_Cell_X);
        }
        else if (m_vertical_Input >= m_controller_Input_Deadzone && !m_Changed_Cell)
        {
            m_Changed_Cell = true;
            m_Current_Cell_X++;
            if (m_Current_Cell_Y < 0)
            {
                m_Current_Cell_Y = rows;
            }
            if (m_Current_Cell_Y > rows)
            {
                m_Current_Cell_Y = 0;
            }
        }
        else if (m_vertical_Input <= -m_controller_Input_Deadzone && !m_Changed_Cell)
        {
            m_Changed_Cell = true;
            m_Current_Cell_Y--;
            if (m_Current_Cell_Y < 0)
            {
                m_Current_Cell_Y = rows;
            }
            if (m_Current_Cell_Y > rows)
            {
                m_Current_Cell_Y = 0;
            }
        }

    }

    void Update_Blip_Space(int _new_Blip_Space)
    {
        player_Blip.transform.parent = null;
        player_Blip.transform.parent = character_Spots[_new_Blip_Space].transform;
    }

    public void Set_Grid_Amount(int _X_Max, int _Y_Max)
    {
        columns = _X_Max;
        rows = _X_Max;
    }
}
