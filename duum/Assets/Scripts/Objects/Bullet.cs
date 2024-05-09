using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float damage;
    public float speed;
	public Vector3 velocity;

	private float lifetime = 0;

	private void Update()
	{
		if (lifetime > 3)
		{
			gameObject.SetActive(false); 
			HandleHitServerRpc();
		}

		lifetime += Time.deltaTime;
		transform.position += velocity * speed * Time.deltaTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Hit: " + other.name);
		if (other.gameObject.TryGetComponent<CharacterControl>(out var control))
		{
			control.ApplyDamage(damage);
		}
		gameObject.SetActive(false);
		HandleHitServerRpc();
	}

	[Rpc(SendTo.Server)]
	private void HandleHitServerRpc()
	{
		Debug.Log("Worked");
		Destroy(gameObject, 0.5f);
	}

}
