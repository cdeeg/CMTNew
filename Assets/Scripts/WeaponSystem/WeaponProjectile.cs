using UnityEngine;
using System.Collections;
	
public class WeaponProjectile : MonoBehaviour
{
	public GameObject visualRepresentation;

	ParticleSystem impactParticles;
	Vector3 startPosition;
	Vector3 targetPosition;
	float flyingSpeed;
	bool didImpact;
	int myWeaponId;

	public int GetWeaponId() { return myWeaponId; }

	public void Initialize( Vector3 start, Vector3 target, float speed, int weaponId )
	{
		if( visualRepresentation != null ) visualRepresentation.SetActive( true );

		impactParticles = GetComponent<ParticleSystem>();
		if( impactParticles != null ) impactParticles.Pause();

		startPosition = start;
		targetPosition = target;
		flyingSpeed = speed;
		myWeaponId = weaponId;
	}
	
	void Update ()
	{
		if( didImpact ) return;

		Vector3 newPos = Vector3.Lerp( startPosition, targetPosition, flyingSpeed * Time.deltaTime );
		transform.position = newPos;

		if( !didImpact && Vector3.Distance( newPos, targetPosition ) < 0.1f )
		{
			if( impactParticles != null && !impactParticles.isPlaying ) impactParticles.Play();
			if( visualRepresentation != null ) visualRepresentation.SetActive( false );
			didImpact = true;
		}
	}

	public void Reset()
	{
		if( impactParticles != null ) impactParticles.Stop();
		if( visualRepresentation != null ) visualRepresentation.SetActive( false );
	}
}
