using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Lobby_Manager : MonoBehaviour
{

    public Lobby_Player_Navigation[] players_Nav;


    // Start is called before the first frame update
    void Start()
    {
        ReInput.ControllerConnectedEvent += On_Controller_Connected;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < ReInput.controllers.GetControllerCount(ControllerType.Joystick); i++)
        {
            players_Nav[i].Custom_Update();
            Debug.Log($"Player {i}, is connected and updating.");
        }
    }

    void On_Controller_Connected(ControllerStatusChangedEventArgs args)
    {
        Debug.Log("A controller was connected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
    }
   
}
