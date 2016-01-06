using System.Collections;
using System;
using UnityEngine;

public class Utilities
{
	static Utilities instance;

	public static Utilities GetInstance()
	{
		if(instance == null)
			instance = new Utilities();

		return instance;
	}

	private Utilities() {}

	/** Calculate new camera position according to current camera behaviour. **/
	public static Vector3 GetCameraPositionByBehaviour( Vector3 currentPosition, CameraBehaviour currentBehaviour, Vector2 movement, float speed )
	{
		if( currentBehaviour == CameraBehaviour.StraightLine ) return GetLineCameraPosition( currentPosition, movement, speed );
		if( currentBehaviour == CameraBehaviour.Cone ) return GetConeCameraPosition( currentPosition, movement, speed );

		return Vector3.zero;
	}

	static Vector3 GetConeCameraPosition( Vector3 currentPosition, Vector2 movement, float speed )
	{
		return Vector3.zero;
	}

	static Vector3 GetLineCameraPosition( Vector3 currentPosition, Vector2 movement, float speed )
	{
		return Vector3.zero;
	}
}


[Serializable]
public class PlayerCharacterSettings
{
	public int health;
	public float moveSpeed;
	public float ladderSpeed;
	public float respawnDelay;
	public float rotationSpeed;

	public PlayerCharacterSettings()
	{
		health = 10;
		rotationSpeed = 3.0f;
	}
}

[Serializable]
public class CameraControlSettings
{
	[Header("General")]
	public float minHeight;
	public float maxHeight;
	public float cameraMovementSpeed;
	[Header("Controls/Behaviour")]
	public CameraBehaviour movementBehaviour;
	public float coneDiameterLower;
	public float coneDiameterUpper;
	public bool invertYAxis;

	public CameraControlSettings()
	{
		minHeight = 0f;
		maxHeight = 10f;
		movementBehaviour = CameraBehaviour.StraightLine;
		coneDiameterLower = 5f;
		coneDiameterUpper = 10f;
	}
}

public enum CameraBehaviour
{
	StraightLine,
	Cone
}


public enum PlayerNetworkAction
{
	NONE,
	USE_WEAPON,
	TOGGLE_RANGED,
	MOVE,
	DODO_INTERACTION
}

public enum WeaponType
{
	Melee,
	ShortRange,
	LongRange,
	Passive
}

public enum RangeWeaponBehaviour
{
	Line,
	Curve
}

public enum RangeWeaponRefireTime
{
	AnimationIsDone,
	ProjectileImpact
}