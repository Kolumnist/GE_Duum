using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallJump : MonoBehaviour
{
	[SerializeField]
	private CharacterController characterController;

	public LayerMask whatIsWall;
	public LayerMask whatIsGround;

	public float wallJumpUpForce;
	public float wallJumpSideForce;

	public float rayWallCheckDistance;
	public float minJumpHeight;
	private RaycastHit leftWallhit;
	private RaycastHit rightWallhit;
	private RaycastHit forwardWallhit;
	private RaycastHit backwardWallhit;

	private bool wallLeft;
	private bool wallRight;
	private bool wallForward;
	private bool wallBackward;

	public bool isWallJumping;
	public bool holdsOntoWall;

	private readonly float exitWallTime = 0.25f;
	private float exitWallTimer;

	// Start is called before the first frame update
	void Start()
	{
		characterController = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	void Update()
	{
	}

	Vector3 wallNormal;
	private void WallJumpMove()
	{
		if(wallLeft)
		{
			wallNormal = leftWallhit.normal;
		}
		else if (wallRight) 
		{ 
			wallNormal = rightWallhit.normal;
		}
		else if (wallForward)
		{
			wallNormal = forwardWallhit.normal;
		}
		else if (wallBackward)
		{
			wallNormal = backwardWallhit.normal;
		}

		Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
	}

	private void CheckForWall()
	{
		wallRight = Physics.Raycast(transform.position, characterController.gameObject.transform.right, out rightWallhit, rayWallCheckDistance, whatIsWall);
		wallLeft = Physics.Raycast(transform.position, -characterController.gameObject.transform.right, out leftWallhit, rayWallCheckDistance, whatIsWall);
		wallForward = Physics.Raycast(transform.position, characterController.gameObject.transform.forward, out forwardWallhit, rayWallCheckDistance, whatIsWall);
		wallBackward = Physics.Raycast(transform.position, -characterController.gameObject.transform.forward, out backwardWallhit, rayWallCheckDistance, whatIsWall);
	}

	private bool AboveGround()
	{
		return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
	}

	public void Jump(InputAction.CallbackContext context)
	{
		if ((wallLeft || wallRight || wallForward || wallBackward) && !characterController.isGrounded)
		{
			holdsOntoWall = context.started || context.performed;
		}
		
		if (!holdsOntoWall)
		{
			holdsOntoWall = false;
			isWallJumping = true;
			exitWallTimer = exitWallTime;
			//WallJumpMove();
			Invoke(nameof(EndOfWallJump), exitWallTime);
		}
	}

	private void EndOfWallJump()
	{
		isWallJumping = false;
	}
}
