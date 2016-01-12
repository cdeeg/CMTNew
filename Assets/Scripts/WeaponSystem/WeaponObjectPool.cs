using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class WeaponObjectPool
{
	/** Holds Weapon instances. As long as they're in here, they're inactive. **/
	List<Weapon> instances;
	List<WeaponProjectile> projectiles;

	public WeaponObjectPool()
	{
		instances = new List<Weapon>();
		projectiles = new List<WeaponProjectile>();
	}

	public void Purge()
	{
		instances.Clear();
		projectiles.Clear();
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

	/** Adds a weapon's projectile to the pool and sets it inactive. */
	public void AddProjectileToPool(WeaponProjectile proj)
	{
		if( projectiles != null && proj != null )
		{
			projectiles.Add( proj );
			proj.gameObject.SetActive( false );
		}
	}

	/** Returns an existing Weapon instance with the same ID if one is in the pool, else it returns null.
	 * The weapon is also reset (ammo is full).
	 **/
	public Weapon GetWeaponInstanceByID( int id )
	{
		Weapon instance = null;

		if( instances == null || instances.Count == 0 ) return instance;

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

	/** Returns a projectile for a specific weapon. Returns null if no existing projectile is found. */
	public WeaponProjectile GetProjectileInstanceForWeapon( int id )
	{
		WeaponProjectile proj = null;

		if( projectiles == null || projectiles.Count == 0 ) return proj;

		for( int i = 0; i < projectiles.Count; ++i )
		{
			if( projectiles[id] != null && projectiles[id].GetWeaponId() == id )
			{
				proj = projectiles[id];		// store instance
				projectiles.RemoveAt( id );	// remove instance from pool
				proj.Reset();				// show visual representation and stop impact particle effect
				return proj;
			}
		}

		return proj;
	}
}
