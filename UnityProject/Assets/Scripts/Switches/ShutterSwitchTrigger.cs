﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ShutterSwitchTrigger : InputTrigger
{
	private Animator animator;

	[SyncVar(hook = "SyncShutters")] public bool IsClosed;
	public ObjectTrigger[] TriggeringObjects;

	private void Start()
	{
		//This is needed because you can no longer apply shutterSwitch prefabs (it will move all of the child sprite positions)
		gameObject.layer = LayerMask.NameToLayer("WallMounts");

		animator = GetComponent<Animator>();
	}

	public override void OnStartClient()
	{
		StartCoroutine(WaitForLoad());
		base.OnStartClient();
	}

	private IEnumerator WaitForLoad()
	{
		yield return WaitFor.Seconds(3f);
		SyncShutters(IsClosed);
	}

	public override bool Interact(GameObject originator, Vector3 position, string hand)
	{
		if (!PlayerManager.LocalPlayerScript.IsInReach(transform.position, false, 1.5f) ||
		    PlayerManager.LocalPlayerScript.IsGhost)
		{
			return true;
		}

		//if the button is idle and not animating it can be pressed
		//this is weird it should check all children objects to see if they are idle and finished
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			PlayerManager.LocalPlayerScript.playerNetworkActions.CmdToggleShutters(gameObject);
		}
		else
		{
			Logger.Log("DOOR NOT FINISHED CLOSING YET!", Category.Shutters);
		}

		return true;
	}

	private void SyncShutters(bool isClosed)
	{
		foreach (ObjectTrigger s in TriggeringObjects)
		{
			if (s != null)
			{ //Apparently unity can't handle the null reference Properly for this case
				s.Trigger(isClosed);
			}
			else {
				Logger.LogError("Missing reference to shutter.", Category.Shutters);
			}
		}
	}
}