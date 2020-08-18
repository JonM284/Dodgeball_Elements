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
        AIR
    };

    [Header("Passed variables")]
    //Int value for element type
    [HideInInspector]
    public int Element_ID;
    [Tooltip("Current elemental type of this ability.")]
    public Element_Type E_Type;
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


    [Header("Abilities")]
    [Tooltip("This will grab all info from inserted ability. DO NOT ADJUST ABILITIES HERE.")]
    public Ability_Info[] ability_Info;
    [Tooltip("Visual representer of cooldowns: max 2")]
    public Object_Follower[] O_Follower = new Object_Follower[2];

    private void Awake()
    {
        for (int i = 0; i < ability_Info.Length; i++)
        {
            Debug.Log("Initializing ability: " + ability_Info[i].Ability_Name);
            ability_Info[i].ability.Initialize(this.gameObject, i);
            switch (ability_Info[i].Element_ID)
            {
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
            O_Follower[i].target = this.transform;
            O_Follower[i].Set_Particle_Gradient(ability_Info[i].ability_Gradient);
        }
    }
   

    private void Update()
    {
        Check_Cooldowns();
    }

    void Check_Cooldowns()
    {
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
        }
    }

    public void Change_Trail(Projectile_Behaviour _current_Projectile)
    {
        _current_Projectile.Element_Trail = ability_Info[0].Projectile_Trail;
        _current_Projectile.trail_Type_Name = ability_Info[0].E_Type.ToString();
    }

    public void Use_Ability(int _Ability_ID)
    {
        ability_Info[_Ability_ID].ability.Use_Ability();
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
