using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secondary_Camera_Follower : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: This script is for the secondary camera (particle camera) this will render only particle effects above
    ///everything else in the scene. It will also copy the main camera's current field_Of_Vision.
    /// </summary>

    

    //reference to main camera, aka parent object
    private Camera primary_Camera;

    // Start is called before the first frame update
    void Awake()
    {
        primary_Camera = transform.GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (this.GetComponent<Camera>().fieldOfView != primary_Camera.fieldOfView) this.GetComponent<Camera>().fieldOfView = primary_Camera.fieldOfView;
    }
}
