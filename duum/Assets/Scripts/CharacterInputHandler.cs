using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class CharacterInputHandler : MonoBehaviour
{
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
    [SerializeField]
    private string sprintName = "Sprint";
    [SerializeField]
    private string jumpName = "Jump";

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction jumpAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public float SprintValue { get; private set; }
    public bool JumpTriggered { get; private set; }

    public static CharacterInputHandler Instance { get; private set; }


    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        moveAction = characterControls.FindActionMap(actionMapName).FindAction(moveName);
        lookAction = characterControls.FindActionMap(actionMapName).FindAction(lookName);
        sprintAction = characterControls.FindActionMap(actionMapName).FindAction(sprintName);
        jumpAction = characterControls.FindActionMap(actionMapName).FindAction(jumpName);
        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => LookInput = Vector2.zero;

        sprintAction.performed += context => SprintValue = context.ReadValue<float>();
        sprintAction.canceled += context => SprintValue = 0f;

        jumpAction.performed += context => JumpTriggered = true;
        jumpAction.canceled += context => JumpTriggered = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        sprintAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        sprintAction.Disable();
        jumpAction.Disable();
    }

#if DEBUG
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
#endif
}
