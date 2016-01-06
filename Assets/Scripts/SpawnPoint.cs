using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SpawnPoint : NetworkBehaviour
{
	public GameObject spawnHere;

	/** Get the position where the player character should be located at the beginning of the level. */
	public Vector3 GetSpawnPosition() { if( spawnHere == null ) return Vector3.zero; return spawnHere.transform.position; }

	/** Indicator whether this spawn point is occupied or not. */
	public bool HasTeamAssigned { get; private set; }

	public void AssignTeam()
	{
		// don't try to assign an occupied spawner
		if( HasTeamAssigned ) return;

		HasTeamAssigned = true;
	}

	void Start()
	{
		HasTeamAssigned = false;
	}
}
