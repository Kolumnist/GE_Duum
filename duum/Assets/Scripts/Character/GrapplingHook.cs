using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : NetworkBehaviour
{
	[SerializeField]
	private Transform gunTip;
	[SerializeField]
	private LayerMask isGrappingHookApproved;
	[SerializeField]
	private LineRenderer lineRenderer;

	private Transform cam;
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
		cam = transform.parent;
		lineRenderer.enabled = false;
	}

	private void Update()
	{
		if (isGrappling)
		{
			Vector3 lowestPoint = new(transform.position.x, transform.position.y, transform.position.z);
			float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
			float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;
			if (grapplePointRelativeYPos < 0) highestPointOnArc = 0.5f;

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

	public void Grapple()
	{
		if (isGrappling || grapplingCdTimer > 0) return;


		if (Physics.Raycast(gunTip.position, cam.transform.forward, out RaycastHit hit, grappleDistance, isGrappingHookApproved))
		{
			grapplePoint = hit.point;
			isGrappling = true;
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
