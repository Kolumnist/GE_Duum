using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
	[SerializeField]
	private Transform cam;
	[SerializeField]
	private Transform gunTip;
	[SerializeField]
	private LineRenderer lineRenderer;

	private Vector3 grapplePoint = Vector3.zero;

	private bool isGrappling = false;
	private float grapplingCdTimer = 0f;
	public float grapplingCd;
	public float grappleDistance;
	public float overshootYAxis;
	public float grappleDelayTime;

	public static bool freeze = false;

	private void Update()
	{
		if (CharacterInputHandler.GrapplingHookTriggered)
		{
			StartGrapple();
		}
		else if (isGrappling && !freeze)
		{
			Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
			float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
			float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;
			GetComponentInParent<CharacterControl>().JumpToPosition(grapplePoint, highestPointOnArc);
		}

		if (grapplingCdTimer > 0)
		{
			grapplingCdTimer -= Time.deltaTime;
		}
	}

	private void LateUpdate()
	{
		if (isGrappling)
		{
			lineRenderer.SetPosition(0, gunTip.position);
		}
	}

	private void StartGrapple()
	{
		if (grapplingCdTimer > 0) return;
		LayerMask layerMask = LayerMask.GetMask("Default");
		isGrappling = true;
		freeze = false;

		if (Physics.Raycast(gunTip.position, cam.transform.forward, out RaycastHit hit, grappleDistance, layerMask))
		{
			lineRenderer.enabled = true;
			grapplePoint = hit.point;
			Invoke(nameof(ExecuteGrappling), grappleDelayTime);
			CharacterInputHandler.GrapplingHookTriggered = false;
		}
		else
		{
			grapplePoint = gunTip.position + cam.forward * grappleDistance;
			Invoke(nameof(EndGrappling), grappleDelayTime);
		}
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(1, grapplePoint);
	}

	private void ExecuteGrappling()
	{
		freeze = false;

		Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
		float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
		float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

		if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

		GetComponentInParent<CharacterControl>().JumpToPosition(grapplePoint, highestPointOnArc);

		Invoke(nameof(EndGrappling), 0.5f);
	}

	private void EndGrappling()
	{
		freeze = false;
		isGrappling = false;
		lineRenderer.enabled = false;
		grapplingCdTimer = grapplingCd;
	}
}
