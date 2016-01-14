using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using InControl;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

struct PlayerMove
{
	public int moveNum;				// server sync

	public Vector3 currentPosition;	// position
	public Vector3 lookAtThis;		// rotation
	public bool hasRangeEquipped;	// current weapon type
	public bool useWeapon;			// is using weapon
}

struct PlayerNetworkActionContainer
{
	// the player's current action
	public PlayerNetworkAction action;

	// current position
	public float moveX;
	public float moveY;
	public float moveZ;

	// current rotation
	public float rotateX;
	public float rotateZ;

	// true if range weapon is equipped, false if not
	public bool hasRange;
}

[RequireComponent(typeof(WeaponController))]
[NetworkSettings(channel=2)]public class SimpleCharacterController : NetworkBehaviour
{
	public Transform weaponAnchor;
	public GameObject rayCastObject;

	// local variables
	PlayerActions actions;				// controls
	PlayerCharacterSettings settings;	// character settings (from Startup Manager)
	WeaponController weaponController;	// weapon controller

	// network variables
	[SyncVar(hook="OnServerStateChanged")] PlayerMove serverMove;	// current position (sync a if necessary)
//	[SyncVar]Color playerColor = Color.black;						// chosen color (sync once) // TODO
	[SyncVar]int currentHealth;										// current health

	List<PlayerNetworkActionContainer> pendingMoves;				// pending actions made by the player
	PlayerMove predictedState;										// state of the player deduced from their previous action
	SpawnPoint home;

	int initialHealth; // TODO remove?
	bool hasRangeWeapon;
	bool isJumping;
	Vector3 lastLookDirection;
	Rigidbody rb;

	//	public void SetColor( Color col ) { playerColor = col; } // set player color // TODO

	#region Network
	// called when client is started, aka the player is spawned -> initialize stuff
	public override void OnStartClient()
	{
		// get the settings (since the StartupManager object also contains the NetworkManager,
		// we can be sure it's there
		StartupManager myManager = FindObjectOfType<StartupManager>();
		settings = myManager.settings;

		// set initial and current health
		initialHealth = currentHealth = settings.health;

		isJumping = false;
		hasRangeWeapon = false;

		// get WeaponController
		weaponController = GetComponent<WeaponController>();
		weaponController.Initialize(weaponAnchor, myManager.GetWeaponObjectPool());

		FindFreeSpawnPoint();

		if( home == null )
		{
			Debug.LogWarning("SimpleCharacterController: OnStartClient: Couldn't find any free spawn points. Aborting...");
			return;
		}

		if( rayCastObject == null )
		{
			Debug.Log("SimpleCharacterController: OnStartClient: Can't do ray cast; disabling jumping...");
		}

		// initialize controls
		actions = PlayerActions.CreateWithDefaultBindings();

		// TODO check what PlayerPrefs do exactly
	}

	void OnStopClient()
	{
		if( home != null ) home.UnassignTeam();
		SceneManager.LoadScene("StartGame");
	}

	void OnStopHost()
	{
		if( home != null ) home.UnassignTeam();
		SceneManager.LoadScene("StartGame");
	}

	void OnDisable()
	{
		if( home != null ) home.UnassignTeam();
	}

	void FindFreeSpawnPoint()
	{
		SpawnPoint[] spawners = FindObjectsOfType<SpawnPoint>();
		if( spawners.Length == 0 )
		{
			Debug.LogWarning("SimpleCharacterController: OnStartClient: Can't find any spawn points in this level. Aborting...");
			return;
		}
		home = null;
		for( int i = 0; i < spawners.Length; ++i )
		{
			Debug.Log("SPAWNER "+spawners[i]);
			if( spawners[i].AssignTeam() )
			{
				home = spawners[i];
				break;
			}
		}
	}

	void OnServerStateChanged ( PlayerMove newMove )
	{
		serverMove = newMove;
		if (pendingMoves != null)
		{
			// remove moves until the move count of local and server match
			while (pendingMoves.Count > (predictedState.moveNum - serverMove.moveNum))
			{
				pendingMoves.RemoveAt( 0 );
			}
			// update state
			UpdatePredictedState();
		}
	}

	void UpdatePredictedState ()
	{
		predictedState = serverMove;
		foreach (PlayerNetworkActionContainer action in pendingMoves)
		{
			predictedState = DoAction(predictedState, action);
		}
	}

	void SyncState ( bool init )
	{
		PlayerMove currentState = isLocalPlayer ? predictedState : serverMove;

		// match up equipped weapon
		if( currentState.hasRangeEquipped != weaponController.HasRangeWeaponEquipped() )
		{
			weaponController.ToggleRanged();
		}
		else if( currentState.useWeapon )
		{
			bool success = weaponController.UseWeapon();
		}

		// current position is not set (aka the player didn't move)? don't change look rotation
		if( currentState.lookAtThis.x != -2 && currentState.lookAtThis.z != -2 )
			transform.rotation = Quaternion.LookRotation( currentState.lookAtThis );


		if( !weaponController.HasRangeWeaponEquipped() )
		{
			// lerp position for smooth movement
			transform.position = Vector3.Lerp( transform.position, currentState.currentPosition, Time.deltaTime * settings.moveSpeed );
		}
	}
	#endregion

	#region Combat
	public void ApplyDamage( int dmg )
	{
		currentHealth -= dmg;
		if( currentHealth <= 0 )
		{
			Die ();
		}
		else if( currentHealth > initialHealth )
		{
			currentHealth = initialHealth;
		}
	}

	void Die()
	{
		weaponController.Purge();
		// TODO activate ragdoll
	}

	public void Respawn()
	{
		currentHealth = initialHealth;
		// TODO deactivate ragdoll
	}
	#endregion

	#region Unity
	void Start()
	{
		if ( isLocalPlayer )
		{
			pendingMoves = new List<PlayerNetworkActionContainer>();
			transform.position = home.GetSpawnPosition();
			lastLookDirection = new Vector3(-2,0,-2);
			rb = GetComponent<Rigidbody>();
			serverMove = new PlayerMove
			{
				moveNum = 0,
				currentPosition = home.GetSpawnPosition(),
				lookAtThis = lastLookDirection,
				hasRangeEquipped = false,
				useWeapon = false
			};
			UpdatePredictedState();
		}
		SyncState( true );
		//SyncColor(); // TODO
		GetComponent<Renderer>().material.color = isLocalPlayer ? Color.white : Color.blue;
	}

	void Update ()
	{
		if (isLocalPlayer)
		{
			// set up PlayerNetworkActionContainer
			PlayerNetworkActionContainer pressedKey = new PlayerNetworkActionContainer();

			pressedKey.action = PlayerNetworkAction.NONE;
			pressedKey.moveX = transform.position.x;
			pressedKey.moveZ = transform.position.y;
			pressedKey.moveY = transform.position.z;

			pressedKey.rotateX = lastLookDirection.x;
			pressedKey.rotateZ = lastLookDirection.z;

			// use WasPressed here to avoid mass toggling range/mass using weapon
			if( actions.UseWeapon.WasPressed )
			{
				pressedKey.action = PlayerNetworkAction.USE_WEAPON;
			}
			else if( actions.ToggleRanged.WasPressed )
			{
				hasRangeWeapon = !hasRangeWeapon;
			}
			else if( actions.Move.IsPressed && !hasRangeWeapon )
			{
				pressedKey.action = PlayerNetworkAction.MOVE;

				Vector3 dire = transform.position;
				dire.x += Time.deltaTime * settings.moveSpeed * actions.Move.X;
				dire.z += Time.deltaTime * settings.moveSpeed * actions.Move.Y;

				transform.position = dire;
				pressedKey.moveX = dire.x;
				pressedKey.moveY = dire.z;
				pressedKey.moveZ = dire.y;

				lastLookDirection = new Vector3( actions.Move.X, 0, actions.Move.Y );
				transform.rotation = Quaternion.LookRotation( lastLookDirection );
				pressedKey.rotateX = lastLookDirection.x;
				pressedKey.rotateZ = lastLookDirection.z;
			}
			else
			{
				pressedKey.action = PlayerNetworkAction.NONE;
			}

			pressedKey.hasRange = hasRangeWeapon;

			pendingMoves.Add(pressedKey);
			UpdatePredictedState();
			CmdExecute( pressedKey );
		}

		// synchronize state with the server
		SyncState( false );
	}

	void FixedUpdate()
	{
		if( isLocalPlayer && rayCastObject != null )
		{
			RaycastHit hitJump;
			Physics.Raycast( new Ray( rayCastObject.transform.position, Vector3.down ), out hitJump );
			if( hitJump.distance > Mathf.Abs(rayCastObject.transform.localPosition.y) )
			{
				isJumping = true;
			}
			else
			{
				isJumping = false;
			}

			if( actions.Jump.WasPressed && !isJumping )
			{
				rb.AddForce( settings.jumpHeight * transform.up );
			}
		}
	}

	[Command(channel=0)] void CmdExecute(PlayerNetworkActionContainer action)
	{
		serverMove = DoAction(serverMove, action);
	}

	// create PlayerMove by using the info from the PlayerNetworkActionContainer
	PlayerMove DoAction( PlayerMove prev, PlayerNetworkActionContainer action )
	{
		// default: move nowhere
		Vector3 dire = Vector3.zero;
		Vector3 rot = Vector3.zero;

		if( action.action == PlayerNetworkAction.MOVE )
		{
			// get position
			dire.x = action.moveX;
			dire.z = action.moveY;
			dire.y = action.moveZ;
			transform.position = dire;

		}
		else if( action.action == PlayerNetworkAction.NONE )
		{
			dire = transform.position;
		}

		if( rot.x != -2 && rot.z != -2 )
		{
			rot.x = action.rotateX;
			rot.z = action.rotateZ;
			transform.rotation = Quaternion.LookRotation( rot, Vector3.up );
		}

		// this thing will go to the server!
		return new PlayerMove
		{
			moveNum = prev.moveNum + 1,
			currentPosition = dire,
			lookAtThis = rot,
			hasRangeEquipped = action.hasRange,
			useWeapon = action.action == PlayerNetworkAction.USE_WEAPON
		};
	}
	#endregion
}
