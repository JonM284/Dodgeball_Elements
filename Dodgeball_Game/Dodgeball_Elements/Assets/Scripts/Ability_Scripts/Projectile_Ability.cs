using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Ability : Ability
{

    /// <summary>
    /// DESCRIPTION: this script handles projectile ability scriptable object. NOT ACTUAL PROJECtiLE MOVEMENT
    /// </summary>

    [Header("Spawning variables")]
    private GameObject object_To_Spawn;
    private Vector3 spawn_Pos;

    [Header("Projectile variables")]
    public float projectile_Speed;
    public float projectile_Duration;


    public override void Use_Ability()
    {
        throw new NotImplementedException();
    }
}
