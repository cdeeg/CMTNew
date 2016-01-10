using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(ParticleSystem),typeof(BoxCollider))]
public class PickupItem : NetworkBehaviour
{
	[SyncVar]bool active;

	int myWeapon;
	PickupManager myManager;
	ParticleSystem particles;

	public bool IsActive() { return active; }

	public void Init( int weaponIdx, PickupManager manager )
	{
		active = true;
		if( particles == null ) particles = GetComponent<ParticleSystem>();
		if( particles.isStopped ) particles.Play(); // restart particle system if necessary
		myWeapon = weaponIdx;
		myManager = manager;
	}

	void OnTriggerEnter(Collider collider)
	{
		if( !active ) return;

		if( collider.gameObject.tag == "Player" )
		{
			SimpleCharacterController sctrl = collider.gameObject.GetComponent<SimpleCharacterController>();
			if( sctrl != null )
			{
				active = false;
//				sctrl.ReceivePickupWeapon( myWeapon );
				myManager.GiveWeaponToPlayer(myWeapon);
				particles.Stop(); // disable particle emission
			}
		}
	}
}
