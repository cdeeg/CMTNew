using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CameraControl : MonoBehaviour
{
	public CameraControlSettings settings;

	PlayerActions actions;

	void Start()
	{
		// create player actions (not the same as in SimpleCharacterController, since
		// this stuff here will be ignored by the Oculus)
		actions = PlayerActions.CreateWithDefaultBindings();

		// invert Y axis if necessary
		actions.CamMove.InvertYAxis = settings.invertYAxis;

	}

	void Update()
	{
		// don't move other player's cameras!
//		if( !isLocalPlayer ) return;

		if( actions.CamMove.IsPressed )
		{
			float x = actions.CamMove.X;
			float y = actions.CamMove.Y;

			transform.Rotate( new Vector3( 0, x, 0 ) );
			Vector3 move = transform.position;
			move.y += Time.deltaTime * settings.cameraMovementSpeed * y;
			transform.position = move;
		}
	}
}
