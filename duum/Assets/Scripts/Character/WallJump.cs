using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallJump : NetworkBehaviour
{
	[SerializeField]
	private LayerMask whatIsWall;

	private CharacterController characterController;

	private bool wallLeft;
	private bool wallRight;
	private bool wallForward;
	private bool wallBackward;

	public float rayWallCheckDistance;
	public bool isWallJumping;
	public float climbSpeed;
	public bool holdsOntoWall;

	// Start is called before the first frame update
	void Start()
	{
		if (!IsOwner) return;
		characterController = GetComponent<CharacterController>();
	}

	private void FixedUpdate()
	{
		if (!IsOwner) return;
		if (holdsOntoWall)
		{
			var normalized = CharacterInputHandler.MoveInput.normalized;
			if (normalized.y == 0) return; 
			characterController.Move(new Vector3(0, climbSpeed * normalized.y, 0) * Time.deltaTime);
			
			if (wallLeft || wallRight || wallForward || wallBackward)
			{
				return;
			}
			holdsOntoWall = false;
			GetComponent<CharacterControl>().DoJump();
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (!IsOwner) return;
		CheckForWall();
	}

	private void CheckForWall()
	{
		wallRight = Physics.Raycast(transform.position, characterController.gameObject.transform.right, rayWallCheckDistance, whatIsWall);
		wallLeft = Physics.Raycast(transform.position, -characterController.gameObject.transform.right, rayWallCheckDistance, whatIsWall);
		wallForward = Physics.Raycast(transform.position, characterController.gameObject.transform.forward, rayWallCheckDistance, whatIsWall);
		wallBackward = Physics.Raycast(transform.position, -characterController.gameObject.transform.forward, rayWallCheckDistance, whatIsWall);
	}

	public void Jump(InputAction.CallbackContext context)
	{
		if (!IsOwner) return;

		if (context.performed && 
			(wallLeft || wallRight || wallForward || wallBackward) && 
			!characterController.isGrounded)
		{
			holdsOntoWall = true;
		}
		else if (context.canceled && holdsOntoWall)
		{
			holdsOntoWall = false;
			GetComponent<CharacterControl>().DoJump();
		}
	}
}
