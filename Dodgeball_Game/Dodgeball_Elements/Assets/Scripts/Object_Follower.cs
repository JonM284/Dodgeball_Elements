using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Follower : MonoBehaviour
{
    
    //object that this gameobject will follow, to be assigned in Ability_Use_Behaviour
    

    [Header("Variables")]
   
  
    [Tooltip("Particle system to show that ability is usable")]
    public ParticleSystem off_Cooldown_Particles;
    [Tooltip("Particle system to show that ability was used")]
    public ParticleSystem[] ability_Use_Particles;
    [Tooltip("Particle system to show that ability has returned")]
    public ParticleSystem ability_Return_Particles;
    [Tooltip("gradient to be passed from ability_Use_Behaviour")]
    public Gradient lifetime_Gradient;

    

    void Set_Off_Ability_Return_Particles()
    {
        ability_Return_Particles.Play();
    }

    void Set_Off_Ability_Use_Particles()
    {
        for (int i = 0; i < ability_Use_Particles.Length; i++)
        {
            ability_Use_Particles[i].Play();
        }
        
    }

    public void Stop_Cooldown_Particle_Emmision()
    {
        Set_Off_Ability_Use_Particles();
        off_Cooldown_Particles.Stop();
    }

    public void Start_Cooldown_Particle_Emmision()
    {
        Set_Off_Ability_Return_Particles();
        off_Cooldown_Particles.Play();
    }

    public void Set_Particle_Gradient(Gradient _ability_Gradient)
    {
        lifetime_Gradient = _ability_Gradient;
        var main = off_Cooldown_Particles.colorOverLifetime;
        main.color = lifetime_Gradient;
        for (int i = 0; i < ability_Use_Particles.Length; i++)
        {
            var abil_Out = ability_Use_Particles[i].main;
            abil_Out.startColor = _ability_Gradient;
        }
        
        var abil_Return = ability_Return_Particles.main;
        abil_Return.startColor = _ability_Gradient;
    }
}
