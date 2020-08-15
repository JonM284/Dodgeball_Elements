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
    [Tooltip("Color for visual cooldown feedback.")]
    public Gradient Ability_Gradient;
    [Tooltip("GameObject to spawn if player throws at full power")]
    public GameObject element_Passive_Spawnable;

    public enum Element_Type
    {
        NONE,
        FIRE,
        WATER,
        EARTH,
        AIR
    };

    public enum Ability_Type
    {
        NONE,
        DASH,
        SPAWN_OBJECT,
    };

    [Tooltip("Type associated with this ability.")]
    public Ability_Type A_Type;
    [Tooltip("Element associated with this ability.")]
    public Element_Type E_type;

    //reference to player character using this ability
    [HideInInspector]
    public Player_Movement m_Player;
    [HideInInspector]
    public Ability_Use_Behavior m_player_Ability;

    public void Initialize(GameObject _reciever, int _Ability_ID)
    {
       

        m_Player = _reciever.GetComponent<Player_Movement>();
        m_player_Ability = _reciever.GetComponent<Ability_Use_Behavior>();

        try
        {
            m_player_Ability.ability_Info[_Ability_ID].ability_Cooldown = Cooldown;
            m_player_Ability.ability_Info[_Ability_ID].Ability_Name = Ability_Name;
            m_player_Ability.ability_Info[_Ability_ID].Projectile_Trail = element_Passive_Spawnable;
            switch (E_type)
            {
                case Element_Type.FIRE:
                    m_player_Ability.ability_Info[_Ability_ID].Element_ID = (int)Element_Type.FIRE;
                    break;
                case Element_Type.WATER:
                    m_player_Ability.ability_Info[_Ability_ID].Element_ID = (int)Element_Type.WATER;
                    break;
                case Element_Type.EARTH:
                    m_player_Ability.ability_Info[_Ability_ID].Element_ID = (int)Element_Type.EARTH;
                    break;
                case Element_Type.AIR:
                    m_player_Ability.ability_Info[_Ability_ID].Element_ID = (int)Element_Type.AIR;
                    break;
                case Element_Type.NONE:
                    m_player_Ability.ability_Info[_Ability_ID].Element_ID = (int)Element_Type.NONE;
                    break;
            }
            _reciever.GetComponent<Ability_Use_Behavior>().ability_Info[_Ability_ID].ability_Gradient = Ability_Gradient;
            Debug.Log("Initialized Ability: " + Ability_Name + " on object: " + _reciever.transform.name);
        }
        catch
        {
            Debug.LogError("Please add componenet 'Ability_Use_Behaviour' to: " + _reciever.transform.name);
        }
    }

    public abstract void Use_Ability();
}
