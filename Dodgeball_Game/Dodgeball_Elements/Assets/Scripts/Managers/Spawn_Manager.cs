using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Manager : MonoBehaviour
{

    public List<Transform> Level_Spawn_Positions;
    public List<Transform> Usable_Spawn_Positions;
    public List<Player_Movement> all_Players;

    // Start is called before the first frame update
    void Awake()
    {
        Set_Initial_Spawn_Points();
        Set_Usable_Spawn_Points();
        Find_All_Players();
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
