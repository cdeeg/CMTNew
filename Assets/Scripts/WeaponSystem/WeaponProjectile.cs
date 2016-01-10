using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

struct ProjectileMove
{
	public int moveNum;

	public float posX;
	public float posY;
	public float posZ;

	public bool impact;
}
	
public class WeaponProjectile : NetworkBehaviour
{
	public GameObject visualRepresentation;

	[SyncVar(hook="OnServerStateChanged")] ProjectileMove serverMove;
	ProjectileMove myMove;
	List<Vector3> nextMoves;

	ParticleSystem impactParticles;
	bool impacting;
	Vector3 startPosition;
	Vector3 targetPosition;
	float flyingSpeed;

	public void Initialize( Vector3 start, Vector3 target, float speed )
	{
		impactParticles = GetComponent<ParticleSystem>();
		impactParticles.Pause();

		startPosition = start;
		targetPosition = target;
		flyingSpeed = speed;
	}

	void OnServerStateChanged( ProjectileMove move )
	{
		serverMove = move;
		if (nextMoves != null)
		{
			// remove moves until the move count of local and server match
			while (nextMoves.Count > (myMove.moveNum - serverMove.moveNum))
			{
				nextMoves.RemoveAt( 0 );
			}
			// update state
			UpdatePosition();
		}
	}

	void UpdatePosition()
	{
		myMove = serverMove;
		foreach (Vector3 action in nextMoves)
		{
			myMove = DoMove(myMove, action);
		}
	}

	ProjectileMove DoMove( ProjectileMove prev, Vector3 next )
	{
		Vector3 dire = transform.position;
		dire.x = prev.posX;
		dire.y = prev.posY;
		dire.z = prev.posZ;

		return new ProjectileMove
		{
			moveNum = prev.moveNum + 1,
			posX = dire.x,
			posY = dire.y,
			posZ = dire.z
		};
	}

	void Synchronize()
	{
		ProjectileMove currentState = isLocalPlayer ? myMove : serverMove;

		Vector3 pos = transform.position;
		pos.x = currentState.posX;
		pos.y = currentState.posY;
		pos.z = currentState.posZ;
	}
	
	void Update ()
	{
		if( isLocalPlayer )
		{
			if( impacting )
			{
				// TODO
			}
			else
			{
				Vector3 newPos = Vector3.Lerp( startPosition, targetPosition, flyingSpeed * Time.deltaTime );
				transform.position = newPos;

				if( Vector3.Distance( newPos, targetPosition ) < 0.1f )
				{
					impacting = true;
				}
			}
		}
	}
}
