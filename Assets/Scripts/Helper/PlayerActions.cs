using UnityEngine;
using System.Collections;
using InControl;

public class PlayerActions : PlayerActionSet
{
	public PlayerAction Left;
	public PlayerAction Right;
	public PlayerAction Up;
	public PlayerAction Down;
	public PlayerTwoAxisAction Move;

	public PlayerAction CamLeft;
	public PlayerAction CamRight;
	public PlayerAction CamUp;
	public PlayerAction CamDown;
	public PlayerTwoAxisAction CamMove;

	public PlayerAction DodoInteraction;	// pickup and throw dodd
	public PlayerAction UseWeapon;			// use equipped weapon (fire ranged weapon, use melee weapon)
	public PlayerAction ToggleRanged;		// equip/unequip range weapon

	public PlayerActions()
	{
		Left = CreatePlayerAction( "Move Left" );
		Right = CreatePlayerAction( "Move Right" );
		Up = CreatePlayerAction( "Move Up" );
		Down = CreatePlayerAction( "Move Down" );
		Move = CreateTwoAxisPlayerAction( Left, Right, Down, Up );

		CamLeft = CreatePlayerAction( "Move Camera Left" );
		CamRight = CreatePlayerAction( "Move Camera Right" );
		CamUp = CreatePlayerAction( "Move Camera Up" );
		CamDown = CreatePlayerAction( "Move Camera Down" );
		CamMove = CreateTwoAxisPlayerAction( CamLeft, CamRight, CamDown, CamUp );

		DodoInteraction = CreatePlayerAction( "Pick up/Throw Dodo" );
		UseWeapon = CreatePlayerAction( "Use Equipped Weapon" );
		ToggleRanged = CreatePlayerAction( "Equip/Unequip Range Weapon" );
	}

	public static PlayerActions CreateWithDefaultBindings()
	{
		PlayerActions playerActions = new PlayerActions();

		// camera movement
		playerActions.CamUp.AddDefaultBinding( Key.I );
		playerActions.CamDown.AddDefaultBinding( Key.K );
		playerActions.CamLeft.AddDefaultBinding( Key.J );
		playerActions.CamRight.AddDefaultBinding( Key.L );

		playerActions.CamUp.AddDefaultBinding( InputControlType.RightStickUp );
		playerActions.CamDown.AddDefaultBinding( InputControlType.RightStickDown );
		playerActions.CamLeft.AddDefaultBinding( InputControlType.RightStickLeft );
		playerActions.CamRight.AddDefaultBinding( InputControlType.RightStickRight );

		// player movement
		playerActions.Up.AddDefaultBinding( Key.UpArrow );
		playerActions.Down.AddDefaultBinding( Key.DownArrow );
		playerActions.Left.AddDefaultBinding( Key.LeftArrow );
		playerActions.Right.AddDefaultBinding( Key.RightArrow );
		
		playerActions.Left.AddDefaultBinding( InputControlType.LeftStickLeft );
		playerActions.Right.AddDefaultBinding( InputControlType.LeftStickRight );
		playerActions.Up.AddDefaultBinding( InputControlType.LeftStickUp );
		playerActions.Down.AddDefaultBinding( InputControlType.LeftStickDown );
		
		playerActions.Left.AddDefaultBinding( InputControlType.DPadLeft );
		playerActions.Right.AddDefaultBinding( InputControlType.DPadRight );
		playerActions.Up.AddDefaultBinding( InputControlType.DPadUp );
		playerActions.Down.AddDefaultBinding( InputControlType.DPadDown );
		
		playerActions.Up.AddDefaultBinding( Key.W );
		playerActions.Down.AddDefaultBinding( Key.S );
		playerActions.Left.AddDefaultBinding( Key.A );
		playerActions.Right.AddDefaultBinding( Key.D );

		// dodo
		playerActions.DodoInteraction.AddDefaultBinding( Key.Space );
		playerActions.DodoInteraction.AddDefaultBinding( Key.O );
		playerActions.DodoInteraction.AddDefaultBinding( InputControlType.Action1 );

		// using weapon/punching
		playerActions.UseWeapon.AddDefaultBinding( Key.P );
		playerActions.UseWeapon.AddDefaultBinding( Key.M );
		playerActions.UseWeapon.AddDefaultBinding( InputControlType.Action2 );

		// toggle range weapon/melee weapon
		playerActions.ToggleRanged.AddDefaultBinding( Key.R );
		playerActions.ToggleRanged.AddDefaultBinding( Key.U );
		playerActions.ToggleRanged.AddDefaultBinding( InputControlType.RightBumper );
		
		playerActions.ListenOptions.IncludeUnknownControllers = true;
		playerActions.ListenOptions.MaxAllowedBindings = 3;
		
		playerActions.ListenOptions.OnBindingFound = ( action, binding ) =>
		{
			if (binding == new KeyBindingSource( Key.Escape ))
			{
				action.StopListeningForBinding();
				return false;
			}
			return true;
		};
		
		playerActions.ListenOptions.OnBindingAdded += ( action, binding ) =>
		{
			Debug.Log( "Binding added... " + binding.DeviceName + ": " + binding.Name );
		};
		
		playerActions.ListenOptions.OnBindingRejected += ( action, binding, reason ) =>
		{
			Debug.Log( "Binding rejected... " + reason );
		};
		
		return playerActions;
	}
}
