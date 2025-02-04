
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Validates if the performer is in range and not in crit, which are typical requirements for all
/// various interactions. Can also optionally allow soft crit.
///
/// Also works for AimApply, but reach range is always UNLIMITED.
///
/// </summary>
public class CanApply : IInteractionValidator<HandApply>, IInteractionValidator<MouseDrop>,
	IInteractionValidator<AimApply>
{
	public static readonly CanApply EVEN_IF_SOFT_CRIT = new CanApply(true, ReachRange.STANDARD);
	public static readonly CanApply ONLY_IF_CONSCIOUS = new CanApply(false, ReachRange.STANDARD);

	private readonly bool allowSoftCrit;
	private ReachRange reachRange;

	private CanApply(bool allowSoftCrit, ReachRange range)
	{
		this.allowSoftCrit = allowSoftCrit;
		reachRange = range;
	}

	/// <summary>
	/// Adjust the way of determining if the object is in range.
	/// </summary>
	/// <param name="range"></param>
	/// <returns>this</returns>
	public CanApply WithRange(ReachRange range)
	{
		reachRange = range;
		return this;
	}

	private ValidationResult ValidateAll(TargetedInteraction toValidate, NetworkSide side)
	{
		return Validate(toValidate.Performer, toValidate.TargetObject, allowSoftCrit, side, reachRange);
	}

	public ValidationResult Validate(HandApply toValidate, NetworkSide side)
	{
		return ValidateAll(toValidate, side);
	}

	public ValidationResult Validate(MouseDrop toValidate, NetworkSide side)
	{
		return ValidateAll(toValidate, side);
	}

	public ValidationResult Validate(AimApply toValidate, NetworkSide side)
	{
		return Validate(toValidate.Performer, null, allowSoftCrit, side, ReachRange.UNLIMITED);
	}

	/// <summary>
	/// Perform the validation in a static context
	/// </summary>
	/// <param name="player">object of player trying to initiate the action</param>
	/// <param name="target">object being targeted</param>
	/// <param name="allowSoftCrit">whether to allow soft crit</param>
	/// <param name="networkSide">whether client or server-side logic should be used</param>
	/// <param name="reachRange">range of reach to allow</param>
	/// <returns></returns>
	public static ValidationResult Validate(GameObject player, GameObject target, bool allowSoftCrit, NetworkSide networkSide,
		ReachRange reachRange = ReachRange.STANDARD)
	{
		var playerScript = player.GetComponent<PlayerScript>();

		if (playerScript.canNotInteract() && (!playerScript.playerHealth.IsSoftCrit || !allowSoftCrit))
		{
			return ValidationResult.FAIL;
		}

		var result = ValidationResult.FAIL;
		switch (reachRange)
		{
			case ReachRange.UNLIMITED:
				result = ValidationResult.SUCCESS;
				break;
			case ReachRange.STANDARD:
				result = playerScript.IsInReach(target.transform.position, networkSide == NetworkSide.SERVER)
					? ValidationResult.SUCCESS : ValidationResult.FAIL;
				break;
			case ReachRange.EXTENDED_SERVER:
				//we don't check range client-side for this case.
				if (networkSide == NetworkSide.CLIENT)
				{
					result = ValidationResult.SUCCESS;
				}
				else
				{
					var cnt = target.GetComponent<CustomNetTransform>();
					result = ServerCanReachExtended(playerScript, cnt.ServerState) ? ValidationResult.SUCCESS : ValidationResult.FAIL;

				}
				break;
		}

		if (result == ValidationResult.FAIL && networkSide == NetworkSide.SERVER)
		{
			//client tried to pick up something out of range, report it
			var cnt = target.GetComponent<CustomNetTransform>();
			Logger.LogTraceFormat( "Not in reach! server pos:{0} player pos:{1} (floating={2})", Category.Security,
				cnt.ServerState.WorldPosition, player.transform.position, cnt.IsFloatingServer);
		}

		return result;
	}

	private static bool ServerCanReachExtended(PlayerScript ps, TransformState state)
	{
		return ps.IsInReach(state.WorldPosition, true) || ps.IsInReach(state.WorldPosition - (Vector3)state.Impulse, true, 1.75f);
	}
}

/// <summary>
/// Defines a particular way of determining if a player can reach something.
/// </summary>
public enum ReachRange
{
	//based on standard interaction distance (playerScript.interactionDistance)
	STANDARD,
	//object can still be in reach even if outside standard interactionDistance - such as for an object not
	//perfectly aligned on the tile. In either case, range will not be checked on the client side - it will only
	//be checked on server side
	EXTENDED_SERVER,
	//no range check
	UNLIMITED,
}
