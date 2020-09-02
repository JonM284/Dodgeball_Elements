using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Rotator : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: this script handles the visual effect of spinning the projectile object mesh while it is being thrown.
    /// </summary>

    //Is this object supposed to be rotating currently?
    public bool is_Active = false;
    //Speed at which object will rotate
    public float rotation_Speed;

    private void LateUpdate()
    {
        if (is_Active)
        {
            transform.localEulerAngles += transform.forward * rotation_Speed * Time.deltaTime;
        }
    }
}
