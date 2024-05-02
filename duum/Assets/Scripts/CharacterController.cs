using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterControl : MonoBehaviour
{
	private CharacterController characterController;
    private Camera mainCamera;
    private CharacterInputHandler inputHandler;

    private Vector3 currentMovement;
	private float verticalRotation;


	private Vector3 moveDirection = Vector3.zero;

	private float gravity = 9.81f;
	
    public float walkSpeed = 8f;
    public float sprintSpeed = 15f; // sprintmultiplier
    public float jumpForce = 6f;


    public float mouseSensitivity = 2f;
    public float lookUpDownRange = 80f;

    public bool canMove = true;

	// Start is called before the first frame update
	private void Awake()
    {
		characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        inputHandler = CharacterInputHandler.Instance;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        float speed = inputHandler.SprintValue > 0 ? sprintSpeed : walkSpeed;

        Vector3 inputDirection = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        worldDirection.Normalize();

		currentMovement.x = worldDirection.x * speed;
        currentMovement.z = worldDirection.z * speed;

        HandleJumping();
        characterController.Move(currentMovement * Time.deltaTime);
	}

    private void HandleJumping()
    {
        if(characterController.isGrounded)
        {
            currentMovement.y = -0.5f;

            if(inputHandler.JumpTriggered)
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y -= gravity * Time.deltaTime;
        }
    }

    private void HandleRotation()
    {
        float mouseXRotation = inputHandler.LookInput.x * mouseSensitivity;
        transform.Rotate(0, mouseXRotation, 0);
        verticalRotation -= inputHandler.LookInput.y * mouseSensitivity;
		verticalRotation = Mathf.Clamp(verticalRotation, -lookUpDownRange, lookUpDownRange);

        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}
