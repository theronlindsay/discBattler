using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class EscapeMenu : MonoBehaviour
{
    
    [SerializeField] private InputActionReference menuButton; // the menu button
    [SerializeField] private GameObject menu; // disabled by default
    [SerializeField] private InputSystemUIInputModule uiInput; // the UI input module


    // Update is called once per frame
    void Update()
    {
        // if the menu button is pressed
        if (menuButton.action.triggered)
        {
            // toggle menu
            menu.SetActive(!menu.activeSelf);
            // if the menu is active, switch to UI input mode
            if (menu.activeSelf)
            {
                //uiInput.enabled = true;
            }
        }

    }
}
