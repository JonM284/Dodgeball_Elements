using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [Header("Constant variables")]
    [Tooltip("Name to display for ability")]
    public string Ability_Name;
    [Tooltip("Description of ability")]
    [TextArea(3, 5)]
    public string Ability_Description;
    [Tooltip("Legnth of time until player can use ability again.")]
    public float Cooldown;

    public enum Ability_Type
    {
        NONE,
        DASH,
        SPAWN_OBJECT,
    };

    [Tooltip("Type associated with this ability.")]
    public Ability_Type A_Type;

    //reference to player character using this ability
    public Player_Movement m_Player;

    public abstract void Initialize(GameObject _reciever);
    public abstract void Use_Ability();
}
