using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public float speed;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.TryGetComponent<CharacterControl>(out var control))
		{
			control.ApplyDamage(damage);
		}
		gameObject.SetActive(false);
		Destroy(gameObject, 0.5f);
	}
}
