
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a sequence of IInteractionValidators to use to validate an interaction.
///
/// This is NOT immutable, but it supports method chaining.
/// </summary>
public class InteractionValidationChain<T>
	where T : Interaction
{
	private IList<IInteractionValidator<T>> validations;

	/// <summary>
	/// Validation chain which performs the specified sequence of validations.
	/// </summary>
	/// <param name="validations">Sequence of validations to perform</param>
	public InteractionValidationChain(IList<IInteractionValidator<T>> validations)
	{
		this.validations = validations;
		if (this.validations == null)
		{
			this.validations = new List<IInteractionValidator<T>>();
		}
	}

	/// <summary>
	/// Validates the specified interaction using the validations in this chain
	/// </summary>
	/// <param name="interaction"></param>
	/// <param name="networkSide">whether to do client-side or server-side validation. Server-side validation
	/// should only be used when the server is validating a client's attempt to perform an interaction.</param>
	/// <returns>result of validation</returns>
	public ValidationResult Validate(T interaction, NetworkSide networkSide)
	{
		foreach (var validator in validations)
		{
			if (validator.Validate(interaction, networkSide) == ValidationResult.FAIL)
			{
				return ValidationResult.FAIL;
			}
		}

		return ValidationResult.SUCCESS;
	}

	/// <summary>
	/// Create a new empty validation chain.
	/// </summary>
	/// <returns></returns>
	public static InteractionValidationChain<T> Create()
	{
		return new InteractionValidationChain<T>(null);
	}

	/// <summary>
	/// Create a validation chain that only consists of the provided validation.
	/// </summary>
	/// <returns></returns>
	public static InteractionValidationChain<T> Create(IInteractionValidator<T> toAdd)
	{
		return new InteractionValidationChain<T>(new List<IInteractionValidator<T>>()
		{
			toAdd
		});
	}

	/// <summary>
	/// Create a validation chain which performs the specified sequence of validations.
	/// </summary>
	/// <param name="validations">Sequence of validations to perform</param>
	public static InteractionValidationChain<T> Create(IList<IInteractionValidator<T>> validations)
	{
		return new InteractionValidationChain<T>(validations);
	}

	/// <summary>
	/// Adds the specified interaction to the end of the chain
	/// </summary>
	/// <param name="toAdd"></param>
	/// <param name="onFail">invoked when this validation fails. This will only be invoked on the side (client or server)
	/// that validation fails - if it fails client side, the onFail will be invoked on client side but
	/// will not be invoked on the server.</param>
	/// <returns>this</returns>
	public InteractionValidationChain<T> WithValidation(IInteractionValidator<T> toAdd, Action<T, NetworkSide> onFail = null)
	{
		if (onFail != null)
		{
			toAdd = new OnFailValidator<T>(toAdd, onFail);
		}
		validations.Add(toAdd);
		return this;
	}
}


/// <summary>
/// Refers to a "side" of the network - client or server.
/// </summary>
public enum NetworkSide
{
	CLIENT,
	SERVER
}

/// <summary>
/// Result of validation of an interaction.
/// </summary>
public enum ValidationResult
{
	//validation failed - interaction should not be performed
	FAIL,
	//validation succeeded, proceed with the interaction
	SUCCESS
}
