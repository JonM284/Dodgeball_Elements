using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_Ability_Behaviour : MonoBehaviour
{
    [Header("Variables")]
    [Tooltip("How long until this gameobject will stop emitting particles.")]
    public float Life_timer;
    [Tooltip("How long until the gameobject will set itself to inactive.")]
    public float Hang_Time;

    public ParticleSystem[] all_Particles;

    private Vector3 start_Position;

    public void Start()
    {
        start_Position = transform.position;
    }

    public void Begin_Countdown()
    {
        StopCoroutine(countdown_To_Turn_Off());
        StartCoroutine(countdown_To_Turn_Off());
        for (int i = 0; i < all_Particles.Length; i++)
        {
            all_Particles[i].Play();
        }
    }

    IEnumerator countdown_To_Turn_Off()
    {
        yield return new WaitForSeconds(Life_timer);
        for (int i = 0; i < all_Particles.Length; i++)
        {
            all_Particles[i].Stop();
        }
        yield return new WaitForSeconds(Hang_Time);
        transform.position = start_Position;
        gameObject.SetActive(false);
    }
}
