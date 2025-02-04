﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
///     Tells client to update respiratory health stats
/// </summary>
public class HealthRespiratoryMessage : ServerMessage
{
	public static short MessageType = (short)MessageTypes.HealthRespiratoryStats;
	public bool IsSuffocating;

	public override IEnumerator Process()
	{
		yield return null;
		PlayerManager.LocalPlayerScript.playerHealth?.UpdateClientRespiratoryStats(IsSuffocating);
	}

	public static HealthRespiratoryMessage Send(GameObject entityToUpdate, bool IsSuffocating)
	{
		HealthRespiratoryMessage msg = new HealthRespiratoryMessage
		{
			IsSuffocating = IsSuffocating
		};
		msg.SendTo(entityToUpdate);
		return msg;
	}
}
