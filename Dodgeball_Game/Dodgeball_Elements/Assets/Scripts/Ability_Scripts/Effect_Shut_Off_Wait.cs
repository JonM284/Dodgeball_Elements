using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Shut_Off_Wait : MonoBehaviour
{
    private Vector3 start_Pos;

    // Start is called before the first frame update
    void Start()
    {
        start_Pos = transform.position;
    }

    public void Start_Effect_Time(float _effect_Duration)
    {
        StartCoroutine(wait_To_Shut_Off(_effect_Duration));
    }

    IEnumerator wait_To_Shut_Off(float _effect_Duration)
    {
        yield return new WaitForSeconds(_effect_Duration);
        transform.position = start_Pos;
        gameObject.SetActive(false);
    }

    
}
