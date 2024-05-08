using System;
using System.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;

[RequireComponent(typeof(CharacterController))]
public class CharacterControl : NetworkBehaviour
{
	[SerializeField]
	private Camera cam;

	[SerializeField]
	private GameObject bulletPrefab;

	[SerializeField]
	private GameObject bombPrefab;

	[SerializeField]
	private Movement movement;

	[SerializeField]
	private Transform spawnPoint;

	private CharacterController characterController;

	private Vector3 currentMovement = Vector3.zero;
	private float velocity = 0f;
	private float verticalRotation = 0f;
	private float dashCooldownLeft = 0f;
	private bool isGettingKnockedBack = false;

	// Character stats
	public float maxHp;
	private float currentHp = 20f;

	public float jumpForce;

	public float mouseSensitivity = 2f;
	public float lookUpDownRange = 80f;

	private float jumps = 2f;
	public float maxJumps = 2f;

	public float throwForce;
	public float grenadeCooldown;
	//

	[SerializeField]
	private float gravityMultiplier;
	private readonly float gravity = 9.81f;

	private int hinder = 1;
	private bool isDying = false;

	[SerializeField]
	private Transform gunTip;

	private void Start()
	{
		if (!IsOwner) return;
		characterController = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
		currentHp = maxHp;
	}

	public override void OnNetworkSpawn()
	{
		if (!IsOwner) {
			cam.enabled = false;
		}
	}

	// Update is called once per frame
	private void Update()
	{
		if (!IsOwner) return;

		if(characterController == null) characterController = GetComponent<CharacterController>();

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
		if (grenadeCooldown > 0)
		{
			grenadeCooldown -= Time.deltaTime;
		}
	}

	private void LateUpdate()
	{
		if (isDying)
		{
			Die();
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
		characterController.Move((currentMovement / hinder) * Time.deltaTime);
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
		if (characterController.isGrounded && velocity < 0.0f) velocity = -0.5f;
		else velocity -= gravity * gravityMultiplier * Time.deltaTime;

		currentMovement.y = velocity;
	}

	public void PullToPosition(Vector3 targetPosition, float grappleForce, float trajectoryHeight)
	{
		if (!IsOwner) return;
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
		yield return new WaitUntil(() => !characterController.isGrounded);
		yield return new WaitUntil(() => characterController.isGrounded);

		jumps = 2;
	}
	public void Jump(InputAction.CallbackContext context)
	{
		if (!IsOwner) return;
		if (GetComponent<WallJump>().holdsOntoWall) jumps = 1;
		if (!context.started || isGettingKnockedBack) return;
		if (!characterController.isGrounded && jumps <= 0) return;

		jumps--;

		DoJump();
	}
	public void DoJump()
	{
		if (!IsOwner) return;
		if (jumps == maxJumps-1) StartCoroutine(WaitForLanding());
		if (velocity < 0 || velocity > (jumpForce / 2)) velocity = 0;
		velocity += jumpForce / hinder;
	}

	public void Sprint(InputAction.CallbackContext context)
	{
		if (!IsOwner) return;
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
		if (!IsOwner) return;
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
		if (!IsOwner) return;
		isGettingKnockedBack = true;
		velocity += knockbackStrength;
		if (jumps == maxJumps) StartCoroutine(WaitForLanding());
		if (jumps ==2 ) jumps--;
		StartCoroutine(WaitForKnockbackToEnd(knockbackDuration));
	}

	private void NotDying()
	{
		isDying = false;
	}

	private void Die()
	{
		characterController.enabled = false;
		gameObject.transform.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z);
		gameObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
		gameObject.transform.SetLocalPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
		NetworkObject.TrySetParent(gameObject, spawnPoint.gameObject);
		
		if (gameObject.transform.position != spawnPoint.position)
		{
			Debug.Log("position not reset...");
			return;
		}
		currentHp = maxHp;
		characterController.enabled = true;
		gameObject.transform.position = spawnPoint.position;
		gameObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
		gameObject.transform.SetLocalPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
	}

	public void ApplyDamage(float damage)
	{
		Debug.Log(OwnerClientId + " Owner: " + this.gameObject);
		if (!IsOwner) return;
        currentHp -= damage;
		if (currentHp <= 0)
		{
			Debug.Log("You Die");
			isDying = true;
			Die();
			Invoke(nameof(NotDying), 2);
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
		if (!IsOwner) return;
		if (!context.started) return;

		GameObject bullet = Instantiate(bulletPrefab, gunTip.position, gunTip.rotation);
		bullet.GetComponent<Bullet>().velocity = new Vector3(cam.transform.forward.x * 1.2f, cam.transform.forward.y, cam.transform.forward.z * 1.2f);
		StartCoroutine(DeleteBullet(bullet));
	}

	private IEnumerator ResetHinder()
	{
		yield return new WaitForSecondsRealtime(2);
		hinder = 1;
	}
	public void SetHinder()
	{
		if (!IsOwner) return;

		hinder = 2;
		StartCoroutine(ResetHinder());
	}

	public void Grenade(InputAction.CallbackContext context)
	{
		if (!IsOwner) return;
		if (!context.started || grenadeCooldown > 0) return;

		GameObject bomb = Instantiate(bombPrefab, gunTip.position, gunTip.rotation);
		bomb.GetComponent<Rigidbody>().AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);

		grenadeCooldown = 2;
	}

	public void Grapple(InputAction.CallbackContext context)
	{
		if (!IsOwner) return;
		if (!context.started) return;
		GetComponentInChildren<GrapplingHook>().Grapple();
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
