using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
	[SerializeField]
	private Transform cam;
	[SerializeField]
	private Transform gunTip;
	[SerializeField]
	private LayerMask isGrappingHookApproved;
	[SerializeField]
	private LineRenderer lineRenderer;

	private Vector3 grapplePoint = Vector3.zero;

	private bool isGrappling = false;
	private float grapplingCdTimer = 0f;

	public float grappleForce;
	public float grappleDistance;
	public float grappleCooldown;
	public float grappleDuration;
	public float overshootYAxis;

	private void Start()
	{
		lineRenderer.enabled = false;
	}

	private void Update()
	{
		if (isGrappling)
		{
			Vector3 lowestPoint = new(transform.position.x, transform.position.y, transform.position.z);
			float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
			float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;
			if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

			GetComponentInParent<CharacterControl>().PullToPosition(grapplePoint, grappleForce, highestPointOnArc);
		}

		if (grapplingCdTimer > 0)
		{
			grapplingCdTimer -= Time.deltaTime;
		}
	}

	private void LateUpdate()
	{
		if (lineRenderer.enabled)
		{
			lineRenderer.SetPosition(0, gunTip.position);
		}
	}

	public void Grapple(InputAction.CallbackContext context)
	{
		if (!context.started) return;
		if (isGrappling || grapplingCdTimer > 0) return;


		if (Physics.Raycast(gunTip.position, cam.transform.forward, out RaycastHit hit, grappleDistance, isGrappingHookApproved))
		{
			grapplePoint = hit.point;
			isGrappling = context.started;
		}
		else
		{
			grapplePoint = gunTip.position + cam.forward * grappleDistance;
		}

		lineRenderer.enabled = true;
		lineRenderer.SetPosition(1, grapplePoint);
		grapplingCdTimer = grappleCooldown;
		StartCoroutine(EndGrappling());
	}

	private IEnumerator EndGrappling()
	{
		yield return new WaitForSecondsRealtime(grappleDuration);
		isGrappling = false;
		lineRenderer.enabled = false;
		grapplingCdTimer = grappleCooldown;
	}
}
