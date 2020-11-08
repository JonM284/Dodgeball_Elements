using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_Ability_Behaviour : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: this script handles player elemental passive abilities
    /// </summary>

    [Header("Variables")]
    [Tooltip("Does the passive have a delay to start?")]
    public bool has_Delay;
    [Tooltip("How long until this gameobject will begin to emit particles.")]
    public float Delay_Time;
    [Tooltip("How long until this gameobject will stop emitting particles.")]
    public float Life_timer;
    [Tooltip("How long until the gameobject will set itself to inactive.")]
    public float Hang_Time;
    [HideInInspector]
    [Tooltip("Ability name to be passed to this passive. Used for effect particles.")]
    public string Ability_Name;
    [HideInInspector]
    public float effect_Duration;
    [Tooltip("Does this passive apply an effect particle?")]
    public bool applies_Effect_Particles;

    //ID for element being used. This is to be applied to the player.
    private int element_ID;


    public ParticleSystem[] all_Particles;

    private Vector3 start_Position;

    public void Start()
    {
        start_Position = transform.position;
    }

    /// <summary>
    /// Setup this passive with certain variables. set in script: Projectile behaviour
    /// </summary>
    /// <param name="_name">Name of Element type.</param>
    /// <param name="_effect_Duration">How long effect will be applied to reciever.</param>
    /// <param name="_element_ID">ID of element to be used.</param>
    public void Setup_Passive(string _name, float _effect_Duration, int _element_ID)
    {
        Ability_Name = _name;
        effect_Duration = _effect_Duration;
        element_ID = _element_ID;
    }

    public void Begin_Countdown()
    {
        StopCoroutine(countdown_To_Turn_Off());
        StartCoroutine(countdown_To_Turn_Off());
    }

    public void Set_Delay()
    {
        for (int i = 0; i < all_Particles.Length; i++)
        {
            var main = all_Particles[i].main;
            main.startDelay = Delay_Time - 0.1f;
        }
    }

    //This is called in: Projectile_Behaviour
    private void Apply_Effect_Particles(GameObject _effected_Gameobject)
    {
        GameObject spawned = null;
        spawned = Object_Pool_Spawner.spawner_Instance.SpawnFromPool(Ability_Name + "_Effect_Particles", _effected_Gameobject.transform.position, Quaternion.identity);
        spawned.transform.parent = _effected_Gameobject.transform;
        spawned.GetComponent<Effect_Shut_Off_Wait>().Start_Effect_Time(effect_Duration);
    }

    IEnumerator countdown_To_Turn_Off()
    {
        if (has_Delay) Set_Delay();
        yield return new WaitForSeconds(Delay_Time);
        for (int i = 0; i < all_Particles.Length; i++)
        {
            all_Particles[i].Play();
        }
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = true;
        yield return new WaitForSeconds(Life_timer);
        for (int i = 0; i < all_Particles.Length; i++)
        {
            all_Particles[i].Stop();
        }
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(Hang_Time);
        transform.position = start_Position;
        gameObject.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Player_Movement>().Reset_Player_Effect();
            other.gameObject.GetComponent<Player_Movement>().Initiate_Player_Effect(element_ID, effect_Duration);
            if (applies_Effect_Particles) {
                Apply_Effect_Particles(other.gameObject);
            }
        }
    }
}
