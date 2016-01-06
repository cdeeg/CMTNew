using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CameraControl : NetworkBehaviour
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
		if( !isLocalPlayer ) return;

		if( actions.CamMove.IsPressed )
		{
			float x = actions.CamMove.X;
			float y = actions.CamMove.Y;

			Utilities.GetInstance();
		}
	}
}
