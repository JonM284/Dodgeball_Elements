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
    [HideInInspector]
    public float m_Original_Rot_Speed;

    private void Start()
    {
        m_Original_Rot_Speed = rotation_Speed;
        Set_Speed(0);
    }

    private void LateUpdate()
    {
        if (is_Active)
        {
            transform.localEulerAngles += transform.forward * rotation_Speed * Time.deltaTime;
        }
    }

    public void Set_Speed(float _speed)
    {
        rotation_Speed = _speed;
    }
}
