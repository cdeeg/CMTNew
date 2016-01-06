using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class WeaponObjectPool
{
	/** Holds Weapon instances. As long as they're in here, they're inactive. **/
	List<Weapon> instances;

	public WeaponObjectPool()
	{
		instances = new List<Weapon>();
	}

	public void Purge()
	{
		instances.Clear();
	}


	/** Adds a Weapon instance to the object pool and sets it inactive. **/
	public void AddToPool(Weapon weapon)
	{
		if( instances != null && weapon != null )
		{
			weapon.transform.SetParent( null );		// detach from player's weaponAnchor
			instances.Add( weapon );				// add to object pool list
			weapon.gameObject.SetActive( false );	// hide unused instance
		}
	}

	/** Returns an existing Weapon instance with the same ID if one is in the pool, else it returns null.
	 * The weapon is also reset (ammo is full).
	 **/
	public Weapon GetWeaponInstanceByID( int id )
	{
		Weapon instance = null;

		if( instances == null ) return instance;

		for( int i = 0; i < instances.Count; ++i )
		{
			if( instances[id] != null && instances[id].WeaponID == id )
			{
				instance = instances[id];	// store instance
				instances.RemoveAt( id );	// remove instance from pool
				instance.Reset();			// refill ammo
				return instance;
			}
		}

		return instance;
	}
}
