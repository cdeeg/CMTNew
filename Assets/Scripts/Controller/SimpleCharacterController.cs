using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using InControl;
using System.Collections.Generic;

struct PlayerMove
{
	public int moveNum;
	public int playerId;

	public Vector3 currentPosition;
	public bool hasRangeEquipped;
	public bool useWeapon;
}

struct PlayerNetworkActionContainer
{
	// the player's current action
	public PlayerNetworkAction action;

	// additional values for two-axis sticks
	public float moveX;
	public float moveY;
	public float moveZ;
	public bool hasRange;
}

[RequireComponent(typeof(WeaponController))]
[NetworkSettings(channel=2)]public class SimpleCharacterController : NetworkBehaviour
{
	public Transform weaponAnchor;
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
	bool hasRangeWeapon = false;

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

		// get WeaponController
		weaponController = GetComponent<WeaponController>();
		weaponController.Initialize(weaponAnchor, myManager.GetWeaponObjectPool());

		SpawnPoint[] spawners = FindObjectsOfType<SpawnPoint>();
		if( spawners.Length == 0 )
		{
			Debug.LogWarning("SimpleCharacterController: OnStartClient: Can't find any spawn points in this level. Aborting...");
			return;
		}
		Debug.Log("spawner "+spawners.Length);
		home = null;
		for( int i = 0; i < spawners.Length; ++i )
		{
			if( !spawners[i].HasTeamAssigned )
			{
				spawners[i].AssignTeam();
				home = spawners[i];
				Debug.Log("HOME IS "+home.GetHashCode()+" FOR "+GetHashCode());
				break;
			}
		}

		if( home == null )
		{
			Debug.LogWarning("SimpleCharacterController: OnStartClient: Couldn't find any free spawn points. Aborting...");
			return;
		}

		// initialize controls
		actions = PlayerActions.CreateWithDefaultBindings();

		// TODO check what PlayerPrefs do exactly
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
		Quaternion lookAt = Quaternion.identity;

		PlayerMove currentState = isLocalPlayer ? predictedState : serverMove;

		// match up equipped weapon
		if( currentState.hasRangeEquipped != weaponController.HasRangeWeaponEquipped() ) weaponController.ToggleRanged();
		else if( currentState.useWeapon ) weaponController.UseWeapon();

		if( !weaponController.HasRangeWeaponEquipped() )
		{
			transform.position = currentState.currentPosition;
		}

		lookAt = Quaternion.LookRotation( Vector3.forward, Vector3.up );

		// current position is not set (aka the player didn't move)? don't change look rotation
		if( currentState.currentPosition != Vector3.zero )
			transform.rotation = Quaternion.Slerp( transform.rotation, lookAt, Time.deltaTime * settings.rotationSpeed );
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
			serverMove = new PlayerMove
			{
				moveNum = 0,
				currentPosition = home.GetSpawnPosition(),
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
			PlayerNetworkActionContainer pressedKey = new PlayerNetworkActionContainer();
			pressedKey.action = PlayerNetworkAction.NONE;

			// use WasPressed here to avoid mass toggling range/mass using weapon
			if( actions.UseWeapon.WasPressed )
			{
				pressedKey.action = PlayerNetworkAction.USE_WEAPON;
			}
			else if( actions.ToggleRanged.WasPressed )
			{
				hasRangeWeapon = !hasRangeWeapon;
				//pressedKey.action = PlayerNetworkAction.RANGE_EQUIPPED;
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
			}
			else
			{
				pressedKey.action = PlayerNetworkAction.NONE;
				pressedKey.moveX = transform.position.x;
				pressedKey.moveZ = transform.position.y;
				pressedKey.moveY = transform.position.z;
			}

			pressedKey.hasRange = hasRangeWeapon;

			pendingMoves.Add(pressedKey);
			UpdatePredictedState();
			CmdExecute( pressedKey );
		}

		// synchronize state with the server
		SyncState( false );
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

		// this thing will go to the server!
		return new PlayerMove
		{
			moveNum = prev.moveNum + 1,
			currentPosition = dire,
			hasRangeEquipped = action.hasRange,
			useWeapon = action.action == PlayerNetworkAction.USE_WEAPON
		};
	}
	#endregion
}
