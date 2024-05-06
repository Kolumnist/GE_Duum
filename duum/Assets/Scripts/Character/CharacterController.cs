using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;

[RequireComponent(typeof(CharacterController))]
public class CharacterControl : MonoBehaviour
{
	[SerializeField]
	private Camera cam;

	[SerializeField]
	private Transform gunTip;

	[SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	private Movement movement;

	[SerializeField]
	private Transform spawnPoint;

	private CharacterController characterController;
	private bool IsGrounded() => characterController.isGrounded;

	private Vector3 currentMovement = Vector3.zero;
	private float velocity = 0f;
	private float verticalRotation = 0f;
	private float dashCooldownLeft = 0f;
	private bool isGettingKnockedBack = false;

	// Character stats
	public float maxHp;
	private float currentHp = 0f;

	public float jumpForce;

	public float mouseSensitivity = 2f;
	public float lookUpDownRange = 80f;

	private float jumps = 2f;
	public float maxJumps = 2f;

	//

	[SerializeField]
	private float gravityMultiplier;
	private readonly float gravity = 9.81f;

	// Start is called before the first frame update
	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Start()
	{
		currentHp = maxHp;
	}

	// Update is called once per frame
	private void Update()
	{
		HandleRotation();
		if (!GetComponent<WallJump>().holdsOntoWall)
		{
			ApplyGravity();
			HandleMove();
		}

		if (dashCooldownLeft > 0)
		{
			dashCooldownLeft -= Time.deltaTime;
		}
	}

	private void HandleMove()
	{
		Vector3 inputDirection = new(CharacterInputHandler.MoveInput.x, 0f, CharacterInputHandler.MoveInput.y);
		Vector3 worldDirection = transform.TransformDirection(inputDirection);
		worldDirection.Normalize();

		var normalSpeed = movement.speed;

		if (movement.isSprinting)
		{
			normalSpeed = movement.speed * movement.sprintMultiplier;
		}
		movement.currentSpeed = Mathf.MoveTowards(movement.currentSpeed, normalSpeed, movement.acceleration * Time.deltaTime);

		currentMovement.x = worldDirection.x * movement.currentSpeed;
		currentMovement.z = worldDirection.z * movement.currentSpeed;

		if (movement.isDashing)
		{
			currentMovement.x *= movement.dashMultiplier / movement.currentSpeed;
			currentMovement.y = 0.2f;
			velocity = 0;
			currentMovement.z *= movement.dashMultiplier / movement.currentSpeed;
		}
		characterController.Move(currentMovement * Time.deltaTime);
	}

	private void HandleRotation()
	{
		float mouseXRotation = CharacterInputHandler.LookInput.x * mouseSensitivity;
		transform.Rotate(0, mouseXRotation, 0);
		verticalRotation -= CharacterInputHandler.LookInput.y * mouseSensitivity;
		verticalRotation = Mathf.Clamp(verticalRotation, -lookUpDownRange, lookUpDownRange);

		cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
	}

	private void ApplyGravity()
	{
		if (IsGrounded() && velocity < 0.0f) velocity = -0.5f; 
		else velocity -= gravity * gravityMultiplier * Time.deltaTime;

		currentMovement.y = velocity;
	}

	public void PullToPosition(Vector3 targetPosition, float grappleForce, float trajectoryHeight)
	{
		Vector3 startPoint = transform.position;
		Vector3 endPoint = targetPosition;

		float gravity = Physics.gravity.y;
		float displacementY = endPoint.y - startPoint.y;
		Vector3 displacementXZ = new(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

		Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight) * 0.25f;
		Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
			+ Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

		velocity = 0;
		characterController.Move((velocityXZ + velocityY) * grappleForce * Time.deltaTime);
	}


	private IEnumerator WaitForLanding()
	{
		yield return new WaitUntil(() => !IsGrounded());
		yield return new WaitUntil(IsGrounded);

		jumps = 2;
	}
	public void Jump(InputAction.CallbackContext context)
	{
		if (GetComponent<WallJump>().holdsOntoWall) jumps = 1;
		if (!context.started || isGettingKnockedBack) return;
		if (!IsGrounded() && jumps <= 0) return;

		jumps--;

		DoJump();
	}
	public void DoJump()
	{
		if (jumps == maxJumps-1) StartCoroutine(WaitForLanding());
		if (velocity < 0 || velocity > (jumpForce / 2)) velocity = 0;
		velocity += jumpForce;
	}

	public void Sprint(InputAction.CallbackContext context)
	{
		movement.isSprinting = context.started || context.performed;
	}

	private IEnumerator WaitForDashToEnd()
	{
		yield return new WaitUntil(() => movement.isDashing);
		yield return new WaitUntil(() => dashCooldownLeft > 0);
		yield return new WaitForSecondsRealtime(movement.dashDuration);

		movement.isDashing = false;
	}
	public void Dash(InputAction.CallbackContext context)
	{
		if (!context.started) return;
		if (dashCooldownLeft > 0) return;

		movement.isDashing = context.started;
		dashCooldownLeft = movement.dashCooldown;
		StartCoroutine(WaitForDashToEnd());
	}

	private IEnumerator WaitForKnockbackToEnd(float duration)
	{
		yield return new WaitForSecondsRealtime(duration);
		isGettingKnockedBack = false;
	}
	public void Knockback(float knockbackStrength, float knockbackDuration)
	{
		isGettingKnockedBack = true;
		velocity += knockbackStrength;
		if (jumps == maxJumps) StartCoroutine(WaitForLanding());
		if (jumps ==2 ) jumps--;
		StartCoroutine(WaitForKnockbackToEnd(knockbackDuration));
	}

	private IEnumerator Die()
	{
		yield return new WaitForEndOfFrame();

		transform.position = spawnPoint.position;
		currentHp = maxHp;
	}

	public void ApplyDamage(float damage)
	{
		currentHp -= damage;

		if (currentHp <= 0)
		{
			Debug.Log("You Die");
			StartCoroutine(Die());
		}
	}

	private IEnumerator DeleteBullet(GameObject bullet)
	{
		yield return new WaitForSecondsRealtime(6);
		if (bullet != null)
		{
			Destroy(bullet);
		}
	}

	public void Shoot(InputAction.CallbackContext context)
	{
		if (!context.started) return;

		GameObject bullet = Instantiate(bulletPrefab, gunTip.position, Quaternion.Euler(gunTip.rotation.x - 45, gunTip.rotation.y, gunTip.rotation.z - 45));
		StartCoroutine(DeleteBullet(bullet));
	}
}

[Serializable]
public struct Movement
{
	public float speed;
	public float wallClimbSpeed;
	public float sprintMultiplier;
	public float dashMultiplier;
	public float dashDuration;
	public float dashCooldown;
	public float acceleration;

	[HideInInspector]
	public bool isSprinting;

	[HideInInspector]
	public bool isDashing;

	[HideInInspector]
	public float currentSpeed;
}
