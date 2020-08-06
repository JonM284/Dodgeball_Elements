﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Spawning_Object")]
public class Spawning_Object_Ability : Ability
{
    //to be replaced with an object spawner later on.
    [Tooltip("Object to spawn")]
    public GameObject Spawning_Object;
    [Tooltip("Size applied to spawning object.")]
    public Vector3 object_Size;
    [Tooltip("Offset for spawning object relative to player position and direction")]
    public Vector3 spawning_Offset;

    public override void Initialize(GameObject _reciever)
    {
        m_Player = _reciever.GetComponent<Player_Movement>();
    }

    public override void Use_Ability()
    {
        GameObject _spawned_Obj = Instantiate(Spawning_Object, m_Player.transform.position + m_Player.transform.TransformDirection(spawning_Offset), m_Player.transform.rotation) as GameObject;
        _spawned_Obj.transform.localScale = new Vector3(object_Size.x, object_Size.y, object_Size.z);
        //_spawned_Obj.GetComponent<Area_Effects>().Setup(_player_Instance, duration, effect_Duration);
    }
}
