using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Ability/Dash_Ability")]
public class Dash_Ability : Ability
{

    /// <summary>
    /// DESCRIPTION: this script handles DASH ABILITY
    /// </summary>

    [Header("Dash Variables")]
    [Tooltip("Duration of this dash ability")]
    public float Dash_Duration;
    [Tooltip("Speed of this dash ability")]
    public float Dash_Speed;
    [Tooltip("Can the player enter a un-hitable state when dashing?")]
    public bool is_Invulnerable;



    public override void Use_Ability()
    {
        m_Player.Initiate_Dash_Type(Dash_Duration, Dash_Speed, false);
        if (is_Invulnerable) m_Player.Initiate_Invulnerability(Dash_Duration);
    }
}
