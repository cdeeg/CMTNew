using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

struct CurrentSpawnStatus
{
	public bool occupied;
	public int score;
}
public class SpawnPoint : NetworkBehaviour
{
	public GameObject spawnHere;

	[SyncVar]CurrentSpawnStatus currentState;

	/** Get the position where the player character should be located at the beginning of the level. */
	public Vector3 GetSpawnPosition() { if( spawnHere == null ) return Vector3.zero; return spawnHere.transform.position; }

	/** Indicator whether this spawn point is occupied or not. */
	public bool HasTeamAssigned { get; private set; }

	public bool AssignTeam()
	{
		// don't try to assign an occupied spawner
		if( currentState.occupied ) { return false; }

		currentState.occupied = true;
		return true;
	}

	public void UnassignTeam()
	{
		currentState.occupied = false;
	}

	void Start()
	{
		HasTeamAssigned = false;

		currentState = new CurrentSpawnStatus { occupied = false, score = 0 };
	}
}
