using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bomb : NetworkBehaviour
{
	[SerializeField]
	private GameObject explosionParticlesPrefab;

	[SerializeField]
	private LayerMask playerMask;

	public float tillExplode;
	public float damage;

	private GameObject effect;

	void Start()
    {
		StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
		yield return new WaitForSecondsRealtime(tillExplode);
		effect = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);

		
		var colliders = Physics.OverlapSphere(transform.position, 10, playerMask);
		foreach (var collider in colliders)
		{
			collider.GetComponent<Rigidbody>().AddExplosionForce(10, transform.position, 10, 0, ForceMode.Impulse);

			if (collider.gameObject.TryGetComponent<CharacterControl>(out var control))
			{
				Debug.Log("Explode on: " + collider.name);
				control.ApplyDamage(damage);
				control.SetHinder();
			}
		}

		GetComponent<Renderer>().enabled = false;
		GetComponent<Collider>().enabled = false;
		DestroyObjectServerRpc();
	}

	[Rpc(SendTo.Server)]
	private void DestroyObjectServerRpc()
	{
		Destroy(effect, 9);
		Destroy(gameObject, 10);
	}

}
