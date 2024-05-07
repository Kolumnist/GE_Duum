using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
	public float trampolinStrength;
	public float trampolinEffectDuration;

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<CharacterControl>() == null) return;

		other.gameObject.GetComponent<CharacterControl>().Knockback(trampolinStrength, trampolinEffectDuration);
	}
}
