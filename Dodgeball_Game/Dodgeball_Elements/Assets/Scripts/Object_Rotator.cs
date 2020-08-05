using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Rotator : MonoBehaviour
{
    //Is this object supposed to be rotating currently?
    public bool is_Active = false;
    //Speed at which object will rotate
    public float rotation_Speed;

    private void LateUpdate()
    {
        if (is_Active)
        {
            transform.localEulerAngles += transform.up * rotation_Speed * Time.deltaTime;
        }
    }
}
