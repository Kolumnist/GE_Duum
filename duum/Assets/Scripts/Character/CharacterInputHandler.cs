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
    private string dashName = "Dash";
	[SerializeField]
    private string jumpName = "Jump";
    [SerializeField]
	private string grapplingHookName = "Grappling Hook";

	private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction dashAction;
	private InputAction jumpAction;
    private InputAction grapplingHookAction;

	public static Vector2 MoveInput { get; private set; }
    public static Vector2 LookInput { get; private set; }
    public static float SprintValue { get; private set; }
    public static bool JumpTriggered { get; private set; }
    public static bool DashTriggered { get; set; }
    public static bool GrapplingHookTriggered { get; set; }

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

        InputActionMap inputs = characterControls.FindActionMap(actionMapName);
		moveAction = inputs.FindAction(moveName);
        lookAction = inputs.FindAction(lookName);
        sprintAction = inputs.FindAction(sprintName);
        dashAction = inputs.FindAction(dashName);
        jumpAction = inputs.FindAction(jumpName);
		grapplingHookAction = inputs.FindAction(grapplingHookName);
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

		dashAction.started += context => DashTriggered = true;
		dashAction.canceled += context => DashTriggered = false;

        grapplingHookAction.started += context => GrapplingHookTriggered = true;
        grapplingHookAction.canceled += context => GrapplingHookTriggered = false;
	}

	private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        sprintAction.Enable();
		dashAction.Enable();
		jumpAction.Enable();
		grapplingHookAction.Enable();
	}

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        sprintAction.Disable();
		dashAction.Disable();
		jumpAction.Disable();
		grapplingHookAction.Disable();
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
