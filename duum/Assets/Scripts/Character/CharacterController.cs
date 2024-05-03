using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(CharacterController))]
public class CharacterControl : MonoBehaviour
{
	[SerializeField]
	private CharacterController characterController;
	[SerializeField]
	private Camera cam;
	[SerializeField]
	private Transform gunTip;

	private Vector3 currentMovement = Vector3.zero;
	private Vector3 moveDirection = Vector3.zero;
	private float verticalRotation = 0f;

	private float doubleJumpDelay = 0f;
	private float dashDelay = 0f;

	// Character stats

	public float walkSpeed = 8f;
	public float sprintSpeed = 15f;
	public float dashMultiplier = 10f;
	public float jumpForce = 4f;
	public float grappleForce = 10f;

	public float mouseSensitivity = 2f;
	public float lookUpDownRange = 80f;

	//

	public bool canDoubleJump = true;
	public float gravity = 9.81f;

	// Start is called before the first frame update
	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	// Update is called once per frame
	private void Update()
	{
		HandleMovement();
		HandleRotation();
	}

	private void HandleMovement()
	{
		float speed = CharacterInputHandler.SprintValue > 0 ? sprintSpeed : walkSpeed;
		Vector3 inputDirection = new(CharacterInputHandler.MoveInput.x, 0f, CharacterInputHandler.MoveInput.y);
		Vector3 worldDirection = transform.TransformDirection(inputDirection);
		worldDirection.Normalize();

		if (CharacterInputHandler.DashTriggered && Time.time > dashDelay)
		{
			HandleDashing();
			speed *= dashMultiplier;
		}
		currentMovement.x = worldDirection.x * speed;
		currentMovement.z = worldDirection.z * speed;

		HandleJumping();
		characterController.Move(currentMovement * Time.deltaTime);
	}

	private void HandleDashing()
	{
		walkSpeed *= dashMultiplier;
		sprintSpeed *= dashMultiplier;
		Invoke(nameof(ResetSpeed), 0.05f);

		dashDelay = Time.time + 2f;
		CharacterInputHandler.DashTriggered = false;
	}

	private void ResetSpeed()
	{
		walkSpeed /= dashMultiplier;
		sprintSpeed /= dashMultiplier;
	} 

	private void HandleJumping()
	{
		if (characterController.isGrounded)
		{
			currentMovement.y = -0.5f;
			canDoubleJump = true;

			if (CharacterInputHandler.JumpTriggered)
			{
				currentMovement.y = jumpForce;
				doubleJumpDelay = Time.time + 0.3f;
			}
		}
		else
		{
			currentMovement.y -= (gravity * 2) * Time.deltaTime;

			if (CharacterInputHandler.JumpTriggered && canDoubleJump && Time.time > doubleJumpDelay)
			{
				canDoubleJump = false;
				if (currentMovement.y < 0f) currentMovement.y = 0f;
				currentMovement.y += jumpForce;
			}
		}

	}

	private void HandleRotation()
	{
		float mouseXRotation = CharacterInputHandler.LookInput.x * mouseSensitivity;
		transform.Rotate(0, mouseXRotation, 0);
		verticalRotation -= CharacterInputHandler.LookInput.y * mouseSensitivity;
		verticalRotation = Mathf.Clamp(verticalRotation, -lookUpDownRange, lookUpDownRange);

		cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
	}

	public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
	{
		characterController.Move(CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight));
	}

	public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
	{
		float gravity = Physics.gravity.y;
		float displacementY = endPoint.y - startPoint.y;
		Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

		Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
		Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
			+ Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

		return (velocityXZ + velocityY) * grappleForce * Time.deltaTime;
	}
}
