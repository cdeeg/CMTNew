using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class WeaponController : NetworkBehaviour {

	public Weapon standardMelee;
	public Weapon standardRange;

	public Transform weaponAnchor;

	Weapon standardRangeInstance;
	Weapon standardMeleeInstance;
	
	Weapon currentMeleeWeapon;
	Weapon currentRangeWeapon;
	Weapon currentWeapon;

	public int GetDamage() { return currentWeapon.damage; }
	public bool HasRangeWeaponEquipped() { return currentWeapon.type != WeaponType.Melee; }

	public void SetWeapon( Weapon w )
	{
		if( w.type == WeaponType.Melee )
		{
			currentMeleeWeapon = w;
		}
		else
		{
			currentRangeWeapon = w;
		}
	}

	public bool ToggleRanged()
	{
		if( currentWeapon.type != WeaponType.Melee )
			return EquipMelee();
		else
			return EquipRange();
	}

	public void Purge()
	{
		currentMeleeWeapon = standardRange;
		currentRangeWeapon = standardMelee;

		EquipMelee();
	}

	/** Equip range weapon */
	public bool EquipRange()
	{
		bool change = currentWeapon.type == WeaponType.Melee;

		currentWeapon = currentRangeWeapon;

		return change;
	}

	/** Equip melee weapon. Returns true if the current weapon changed, false if nothing changed. */
	public bool EquipMelee( bool alwaysReturnTrue = false )
	{
		bool change = currentWeapon.type != WeaponType.Melee;
		currentWeapon = currentMeleeWeapon;

		return change;
	}

	/** Shoot current range weapon. Returns true if the weapon still has ammo left, false if it's empty (the melee weapon will be
	 *  equipped automatically).
	 */
	public bool UseWeapon()
	{
		bool hasAmmo = currentWeapon.UseAmmo();

		if( !hasAmmo )
		{
			if( currentWeapon.type != WeaponType.Melee )
			{
				currentRangeWeapon = standardRange;
				//EquipMelee();
			}
			else
			{
				currentMeleeWeapon = standardMelee;
			}
		}

		return hasAmmo;
	}

	public void Initialize ()
	{
		if( standardMelee == null && standardRange == null )
		{
			Debug.LogError("WeaponController: No standard weapons found!");
			return;
		}

		CmdCreateStandardWeapons();
		
		currentRangeWeapon = standardRangeInstance;
		currentMeleeWeapon = standardMeleeInstance;

		currentWeapon = standardMelee;
	}

	[Command]void CmdCreateStandardWeapons()
	{
		GameObject insta = (GameObject)Instantiate(standardRange.gameObject, weaponAnchor.localPosition, Quaternion.identity);
		NetworkServer.Spawn( insta );
		insta.transform.SetParent( weaponAnchor );
		standardRangeInstance = insta.GetComponent<Weapon>();
		
		insta = (GameObject)Instantiate(standardMelee.gameObject, Vector3.zero, Quaternion.identity);
		NetworkServer.Spawn( insta );
		insta.transform.SetParent( weaponAnchor );
		standardMeleeInstance = insta.GetComponent<Weapon>();
	}
}
