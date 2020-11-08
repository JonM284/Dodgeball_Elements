using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Ability_Info
{

    [Header("Ability Asset")]
    public Ability ability;

    //Elemental type of this ability being passed;
    public enum Element_Type
    {
        NONE,
        FIRE,
        WATER,
        EARTH,
        AIR,
        TOXIN
    };

    [Header("Passed variables")]
    //Int value for element type
    [HideInInspector]
    public int Element_ID;
    [Tooltip("Current elemental type of this ability.")]
    public Element_Type E_Type;
    [Tooltip("Effect type to be assigned to player.")]
    public int ef_Type;
    [Tooltip("Name to be looked up.")]
    public string Ability_Name;
    [Tooltip("Max amount of time for this ability to get off cooldown.")]
    public float ability_Cooldown;
    [Tooltip("Color to be passed for visual cooldown.")]
    public Gradient ability_Gradient;
    [Tooltip("GameObject to be spawned when throwing at full power, set this in ABILITY scriptable object")]
    public GameObject Projectile_Trail;

    //current time during ability cooldown
    [HideInInspector]
    public float current_Ability_Cooldown = 0;
    [HideInInspector]
    public bool ability_Used = true;


}

public class Ability_Use_Behavior : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: this script handles player reading and being able to use their chosen abilities.
    /// </summary>

    [Header("Abilities")]
    [Tooltip("This will grab all info from inserted ability. DO NOT ADJUST ABILITIES HERE.")]
    public Ability_Info[] ability_Info;
    [Tooltip("Visual representer of cooldowns: max 2")]
    public Object_Follower[] O_Follower = new Object_Follower[2];

    [HideInInspector]
    public GameObject m_Player;

    private void Awake()
    {
        m_Player = this.gameObject;
        //--------******** ADD NEW ABILITY HERE
        for (int i = 0; i < ability_Info.Length; i++)
        {
            Debug.Log("Initializing ability: " + ability_Info[i].Ability_Name);
            ability_Info[i].ability.Initialize(this.gameObject, i);
            switch (ability_Info[i].Element_ID)
            {
                case 5:
                    ability_Info[i].E_Type = Ability_Info.Element_Type.TOXIN;
                    break;
                case 4:
                    ability_Info[i].E_Type = Ability_Info.Element_Type.AIR;
                    break;
                case 3:
                    ability_Info[i].E_Type = Ability_Info.Element_Type.EARTH;
                    break;
                case 2:
                    ability_Info[i].E_Type = Ability_Info.Element_Type.WATER;
                    break;
                case 1:
                    ability_Info[i].E_Type = Ability_Info.Element_Type.FIRE;
                    break;
                default:
                    ability_Info[i].E_Type = Ability_Info.Element_Type.NONE;
                    break;
            }
        }

        for (int i = 0; i < O_Follower.Length; i++)
        {
            O_Follower[i].Set_Particle_Gradient(ability_Info[i].ability_Gradient);
        }
    }
   

    public void Custom_Update()
    {
        Check_Cooldowns();
    }

    void Check_Cooldowns()
    {

        if (ability_Info[0].ability_Used || ability_Info[1].ability_Used)
        {
            for (int i = 0; i < ability_Info.Length; i++)
            {
                if (ability_Info[i].current_Ability_Cooldown < ability_Info[i].ability_Cooldown && ability_Info[i].ability_Used)
                {
                    ability_Info[i].current_Ability_Cooldown += Time.deltaTime;
                }

                if (ability_Info[i].current_Ability_Cooldown >= ability_Info[i].ability_Cooldown && ability_Info[i].ability_Used)
                {
                    Reset_Ability_Variables(i);
                }
            }
        }

        /*
        //Ability 0 cooldown
        if (ability_Info[0].current_Ability_Cooldown < ability_Info[0].ability_Cooldown && ability_Info[0].ability_Used)
        {
            ability_Info[0].current_Ability_Cooldown += Time.deltaTime;
        }

        if (ability_Info[0].current_Ability_Cooldown >= ability_Info[0].ability_Cooldown && ability_Info[0].ability_Used)
        {
            Reset_Ability_Variables(0);
        }

        //ability 1 cooldown
        if (ability_Info[1].current_Ability_Cooldown < ability_Info[1].ability_Cooldown && ability_Info[1].ability_Used)
        {
            ability_Info[1].current_Ability_Cooldown += Time.deltaTime;
        }

        if (ability_Info[1].current_Ability_Cooldown >= ability_Info[1].ability_Cooldown && ability_Info[1].ability_Used)
        {
            Reset_Ability_Variables(1);
        }*/
    }

    public void Change_Trail(Projectile_Behaviour _current_Projectile)
    {
        _current_Projectile.Element_Trail = ability_Info[0].Projectile_Trail;
        _current_Projectile.trail_Type_Name = ability_Info[0].E_Type.ToString();
        _current_Projectile.effect_Duration = ability_Info[0].ability.effect_Duration;
        _current_Projectile.effect_ID = ability_Info[0].ef_Type;
    }

    public void Use_Ability(int _Ability_ID)
    {
        ability_Info[_Ability_ID].ability.Use_Ability(m_Player);
        ability_Info[_Ability_ID].ability_Used = true;
        O_Follower[_Ability_ID].Stop_Cooldown_Particle_Emmision();

        Debug.Log("Have used: " +ability_Info[_Ability_ID].Ability_Name);
    }

    void Reset_Ability_Variables(int _Ability_ID)
    {
        ability_Info[_Ability_ID].ability_Used = false;
        ability_Info[_Ability_ID].current_Ability_Cooldown = 0;
        O_Follower[_Ability_ID].Start_Cooldown_Particle_Emmision();
    }
    
}
