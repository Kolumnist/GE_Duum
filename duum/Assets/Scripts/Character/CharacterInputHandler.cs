using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class CharacterInputHandler : MonoBehaviour
{
	[SerializeField]
	private CharacterControl control;

	[Header("Input Action Asset")]
    [SerializeField]
    private InputActionAsset characterControls;

    [Header("Action Map Name Reference")]
    [SerializeField]
    private string actionMapName = "Player";

    [Header("Action Name References")]
    [SerializeField]
    private string moveName = "Move";
    [SerializeField]
    private string lookName = "Look";

	private InputAction moveAction;
    private InputAction lookAction;

	public static Vector2 MoveInput { get; private set; }
    public static Vector2 LookInput { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        InputActionMap inputs = characterControls.FindActionMap(actionMapName);
		moveAction = inputs.FindAction(moveName);
        lookAction = inputs.FindAction(lookName);
		RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => LookInput = Vector2.zero;
	}

	private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
	}

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
	}

/*
    private void PrintDevices()
    {
        foreach(var device in InputSystem.devices)
        {
            if (device.enabled)
            {
                Debug.Log("Active Device: " + device.name);
            }
        }
    }
*/
}
