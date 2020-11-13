using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Manager : MonoBehaviour
{

    /// <summary>
    /// This script houses all the logic for each round.
    /// </summary>


    [Tooltip("This is how many players were ready in the previous lobby screen.")]
    [HideInInspector]
    public int Num_Of_Players;
    [Tooltip("This is how many players are currently playing in the match.")]
    [HideInInspector]
    public int Num_Of_Current_Active_Players;
    [Tooltip("Reference to game spawn manager, to initialize round start and restart.")]
    public Spawn_Manager spawn_Manager;
    //Is the current game ongoing?
    private bool m_Game_Is_Active;

    public static Game_Manager m_GM;

    private void Awake()
    {
        try{
            Num_Of_Players = PlayerPrefs.GetInt("Num_Of_Players");
            Num_Of_Current_Active_Players = Num_Of_Players;
            //amount of active players must be added
            spawn_Manager.Initialize_Match();
            m_Game_Is_Active = true;
        }
        catch
        {
            spawn_Manager.Initialize_Match();
            m_Game_Is_Active = true;
            Debug.Log("PlayerPref Num_Of_Players is not assigned. Check if previous lobby scene has set this before this scene.");
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (Num_Of_Current_Active_Players <= 1 && m_Game_Is_Active)
        {
            Reset_Round();
        }
    }

    public void Remove_Player()
    {
        Num_Of_Current_Active_Players--;
    }

    void Reset_Round()
    {
        m_Game_Is_Active = false;
        try
        {
            spawn_Manager.Reset_Round(Num_Of_Players);
        }
        catch
        {
            Debug.Log("PlayerPref Num_Of_Players is not assigned. Check if previous lobby scene has set this before this scene.");
        }
    }
}
