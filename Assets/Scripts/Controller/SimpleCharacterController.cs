using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using InControl;
using System.Collections.Generic;

struct PlayerMove
{
	public int moveNum;
	public int playerId;

	public Vector3 moveDire;
	public bool toggleRange;
	public bool useWeapon;
}

struct PlayerNetworkActionContainer
{
	// the player's current action
	public PlayerNetworkAction action;

	// additional values for two-axis sticks
	public float moveX;
	public float moveY;
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

	int initialHealth; // TODO remove?

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

		if( currentState.toggleRange ) weaponController.ToggleRanged();
		else if( currentState.useWeapon ) weaponController.UseWeapon();

		transform.position += currentState.moveDire;

		lookAt = Quaternion.LookRotation( Vector3.forward, Vector3.up );
		
		if( currentState.moveDire != Vector3.zero )
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
				pressedKey.action = PlayerNetworkAction.TOGGLE_RANGED;
			}
			else
			{
				if( !weaponController.HasRangeWeaponEquipped() )
				{
					pressedKey.action = PlayerNetworkAction.MOVE;
					pressedKey.moveX = actions.Move.X;
					pressedKey.moveY = actions.Move.Y;
				}
			}

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
			// calculate move vector
			dire.x = Time.deltaTime * settings.moveSpeed * action.moveX;
			dire.z = Time.deltaTime * settings.moveSpeed * action.moveY;
		}

		// this thing will go to the server!
		return new PlayerMove
				{
					moveNum = prev.moveNum + 1,
					moveDire = dire,
					toggleRange = action.action == PlayerNetworkAction.TOGGLE_RANGED,
					useWeapon = action.action == PlayerNetworkAction.USE_WEAPON
				};
	}
	#endregion
}
