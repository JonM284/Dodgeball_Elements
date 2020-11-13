using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Manager : MonoBehaviour
{

    /// <summary>
    /// This script is in control of all player spawning locations and respawning effects (to be added.)
    /// This script is called in: Game_Manager
    /// </summary>

    public List<Transform> Level_Spawn_Positions;
    public List<Transform> Usable_Spawn_Positions;
    public List<Player_Movement> all_Players;

    // Start is called before the first frame update

    private void Awake()
    {
        Initialize_Match();
    }

    public void Initialize_Match()
    {
        Set_Initial_Spawn_Points();
        Set_Usable_Spawn_Points();
        Find_All_Players();
        //Set_Only_Playable_Players_To_Active(_num_Of_Players);
        Set_Player_Spawn_Position();
    }

    public void Reset_Round(int _num_Of_Players)
    {
        Find_All_Players();
        Set_Only_Playable_Players_To_Active(_num_Of_Players);
        Set_Player_Spawn_Position();
    }


    void Set_Initial_Spawn_Points()
    {
        foreach (Transform child in transform)
        {
            Level_Spawn_Positions.Add(child);
        }
    }

    void Set_Usable_Spawn_Points()
    {
        foreach (Transform t in Level_Spawn_Positions)
        {
            Usable_Spawn_Positions.Add(t);
        }
    }

    void Find_All_Players()
    {
        foreach (Player_Movement player in Resources.FindObjectsOfTypeAll(typeof(Player_Movement)))
        {
            all_Players.Add(player);
           
        }
    }

    void Set_Only_Playable_Players_To_Active(int _num_Of_Players)
    {
        if (_num_Of_Players < all_Players.Count) {
            for (int i = _num_Of_Players; i < all_Players.Count; i++)
            {
                all_Players[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Give calling player a random spawn position.
    /// </summary>
    /// <param name="_player"></param>
    public void Set_Player_Spawn_Position()
    {
        for (int i = 0; i < all_Players.Count; i++)
        {
            int random_Spawn = Random.Range(0, Usable_Spawn_Positions.Count);
            all_Players[i].transform.position = Usable_Spawn_Positions[random_Spawn].position;
            Usable_Spawn_Positions.RemoveAt(random_Spawn);
        }
    }


    IEnumerator Spawn_Sequence()
    {
        for (int i = 0; i < all_Players.Count; i++)
        {
            //set each player to do spawn animation
            yield return new WaitForSeconds(0.3f);
        }

    }
    
}
