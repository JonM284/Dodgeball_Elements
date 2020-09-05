using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Projectile_Ability")]
public class Projectile_Ability : Ability
{

    /// <summary>
    /// DESCRIPTION: this script handles projectile ability scriptable object. NOT ACTUAL PROJECtiLE MOVEMENT
    /// </summary>

    [Header("Spawning variables")]
    [Tooltip("Gameobject to spawn (change with object pool spawner)")]
    public GameObject object_To_Spawn;
    [Tooltip("Spawn point relative to player's forward direction")]
    public Vector3 spawn_Pos;

    [Header("Projectile variables")]
    [Tooltip("Speed to set for projectile to move at.")]
    public float projectile_Speed;
    [Tooltip("Duration projectile is active for.")]
    public float projectile_Duration;
    [Tooltip("Can the projectile go through walls?")]
    public bool ignore_Walls;
    [Tooltip("Can the projectile go through players?")]
    public bool ignore_Player;

    public enum Shot_Type
    {
        REGULAR,
        RANDOM_CONE,
        SET_CONE,
        ALL_AROUND
    }
    [Tooltip("Which way the player will shoot: regular= 1 forward, random_Cone= multiple random in front, set_Cone = multiple in set direction, all_ways= around the player")]
    public Shot_Type s_type;
    [Tooltip("Amount to be shot")]
    public int amount;

    [Tooltip("Set_Cone_Offsets")]
    public float[] offset_Amount;

    [Tooltip("Will the particles need to change color?")]
    public bool change_Particle_Color;


    public override void Use_Ability()
    {
        GameObject spawned = null;
        Vector3 dir = Vector3.zero;
        switch (s_type)
        {
            
            case Shot_Type.ALL_AROUND:
                //this portion of code was made with the help of https://www.youtube.com/watch?v=NivKaNN7I00
                float angle_Step = 360f / amount;
                float angle = 0;
            
                Vector3 start_pos = m_Player.transform.position;

                for (int i = 0; i < amount; i++)
                {
                    float proj_X = start_pos.x + Mathf.Sin((angle * Mathf.PI)/ 180) * spawn_Pos.z;
                    float proj_Z = start_pos.z + Mathf.Cos((angle * Mathf.PI) / 180) * spawn_Pos.z;

                    Vector3 proj_Vec = new Vector3(proj_X, 0 , proj_Z);
                
                    Vector3 proj_Dir = new Vector3(proj_Vec.x - start_pos.x, 0 , proj_Vec.z - start_pos.z);
                    
                    spawned = Instantiate(object_To_Spawn, m_Player.transform.position, Quaternion.Euler(proj_Dir)) as GameObject;
                    spawned.transform.forward = proj_Dir;
                    if (change_Particle_Color) spawned.GetComponent<A_Projectile>().Change_All_Particle_Color(Ability_Color);
                    spawned.GetComponent<A_Projectile>().Setup_Projectile(projectile_Speed, projectile_Duration, proj_Dir, ignore_Walls, ignore_Player);
                    angle += angle_Step;
                }
                break;
            case Shot_Type.SET_CONE:
                
                for (int i = 0; i < amount; i++)
                {
                    dir = m_Player.transform.forward + m_Player.transform.TransformDirection(new Vector3(offset_Amount[i], 0, 0));
                    spawned = Instantiate(object_To_Spawn, m_Player.transform.position + m_Player.transform.TransformDirection(spawn_Pos), Quaternion.Euler(dir)) as GameObject;
                    spawned.transform.forward = dir;
                    if (change_Particle_Color) spawned.GetComponent<A_Projectile>().Change_All_Particle_Color(Ability_Color);
                    spawned.GetComponent<A_Projectile>().Setup_Projectile(projectile_Speed, projectile_Duration, dir, ignore_Walls, ignore_Player);
                }
                
                break;
            case Shot_Type.RANDOM_CONE:
                for (int i = 0; i < amount; i++)
                {
                    dir = m_Player.transform.forward + m_Player.transform.TransformDirection(new Vector3(UnityEngine.Random.Range(-offset_Amount[i], offset_Amount[i]), 0, 0));
                    spawned = Instantiate(object_To_Spawn, m_Player.transform.position + m_Player.transform.TransformDirection(spawn_Pos), Quaternion.Euler(dir)) as GameObject;
                    spawned.transform.forward = dir;
                    if (change_Particle_Color) spawned.GetComponent<A_Projectile>().Change_All_Particle_Color(Ability_Color);
                    spawned.GetComponent<A_Projectile>().Setup_Projectile(projectile_Speed, projectile_Duration, dir, ignore_Walls, ignore_Player);
                }
                break;
            case Shot_Type.REGULAR:
                spawned = Instantiate(object_To_Spawn, m_Player.transform.position + m_Player.transform.TransformDirection(spawn_Pos), m_Player.transform.rotation) as GameObject;
                if (change_Particle_Color) spawned.GetComponent<A_Projectile>().Change_All_Particle_Color(Ability_Color);
                spawned.GetComponent<A_Projectile>().Setup_Projectile(projectile_Speed, projectile_Duration, m_Player.transform.forward, ignore_Walls, ignore_Player);
                break;
            
        }
        
        
    }
}
