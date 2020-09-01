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


    public override void Use_Ability()
    {
        GameObject spawned = Instantiate(object_To_Spawn, m_Player.transform.position + m_Player.transform.TransformDirection(spawn_Pos), m_Player.transform.rotation) as GameObject;
        spawned.GetComponent<A_Projectile>().Setup_Projectile(projectile_Speed, projectile_Duration, m_Player.transform.forward, ignore_Walls);
    }
}
