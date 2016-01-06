using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

struct PlayerWeaponReceive
{
	public int weaponIdx;
	public int playerId;
}

public class PickupManager : NetworkBehaviour
{
	public GameObject pickupPrefab;
	public PickupItem[] configuredPickups;
	public Weapon[] weapons;

	[SyncVar(hook="OnServerStateChanged")]PlayerWeaponReceive currentAction;

	public Weapon[] GetAllWeapons() { return weapons; }

	void Start()
	{
		Init();

		// give all weapons a unique ID for reference
		for( int i = 0; i < weapons.Length; ++i )
		{
			weapons[i].WeaponID = i;
		}
	}

	/** Reset pickups. If hardReset is set to true, all pickups are reconfigured, not only used ones. **/
	public void Reset(bool hardReset = false)
	{
		Init (hardReset);
	}

	void Init (bool resetAll = false)
	{
		// only server is allowed to configure pickups
		if( !isServer ) return;

		for( int i = 0; i < configuredPickups.Length; ++i )
		{
			if( !resetAll && configuredPickups[i].IsActive() ) continue;

			// indeces 0 and 1 are the standard weapons, so we exlude them (random can be id 2 to weapons.Length -1)
			int rnd = Random.Range( 2, weapons.Length );
			configuredPickups[i].Init( rnd, this );
		}
	}

	public void GiveWeaponToPlayer(int idx)
	{
		if( idx < 0 || idx >= weapons.Length )
		{
			Debug.LogWarning("PickupManager: Could not process index "+idx+" (weapons found: "+weapons.Length+"), defaulting to 0.");
			idx = 0;
		}

		CmdPickupWeaponNo( idx );
	}

	[Command(channel=0)]void CmdPickupWeaponNo( int index )
	{
		currentAction = PickupWeapon( index );
	}

	PlayerWeaponReceive PickupWeapon( int index )
	{
		return new PlayerWeaponReceive
		{
			weaponIdx = index,
			playerId = 0
		};
	}

	void OnServerStateChanged( PlayerWeaponReceive weaponData )
	{
		currentAction = weaponData;
	}
}
