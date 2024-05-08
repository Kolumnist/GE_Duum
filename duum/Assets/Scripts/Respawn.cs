using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

	[SerializeField]
	private Transform spawnPoint;

	public void SpawnPlayer()
    {
        Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}
