﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Do not derive from NetworkBehaviour, this is also used on tilemap layers
/// <summary>
/// Checks for and handles melee interactions. Note that other interactions (such as P2PInteraction) are possible and handled in other classes.
/// MeleeTrigger is placed on the TARGET of a melee interaction.
/// </summary>
public class MeleeTrigger : MonoBehaviour
{
	//Cache these on start for checking at runtime
	private Layer tileMapLayer;
	private GameObject gameObjectRoot;

	private void Start()
	{
		gameObjectRoot = transform.root.gameObject;

		var layer = gameObject.GetComponent<Layer>();
		if (layer != null)
		{
			//this is on a tilemap:
			tileMapLayer = layer;
		}
	}

	public virtual bool MeleeInteract(GameObject originator, string hand)
	{
		if (UIManager.Hands.CurrentSlot.Item != null)
		{
			var mousePos = Camera.main.ScreenToWorldPoint(CommonInput.mousePosition);
			var handItem = UIManager.Hands.CurrentSlot.Item.GetComponent<ItemAttributes>();

			if (handItem.itemType == ItemType.Food || handItem.itemType == ItemType.Medical) {
				return false;
			}

			//special case
			//We don't melee if we are wielding a gun with ammo and clicking ourselves (we will instead shoot ourselves)
			if (gameObject == originator)
			{
				var gun = handItem.GetComponent<Gun>();
				if (gun != null)
				{
					if (gun.CurrentMagazine != null && gun.CurrentMagazine.ammoRemains > 0)
					{
						//we have ammo and are clicking ourselves - don't melee. Shoot instead.
						return false;
					}
				}
			}

			if (handItem.itemType != ItemType.ID &&
				handItem.itemType != ItemType.Back &&
				handItem.itemType != ItemType.Ear &&
				handItem.itemType != ItemType.Food &&
				handItem.itemType != ItemType.Glasses &&
				handItem.itemType != ItemType.Gloves &&
				handItem.itemType != ItemType.Hat &&
				handItem.itemType != ItemType.Mask &&
				handItem.itemType != ItemType.Neck &&
				handItem.itemType != ItemType.Shoes &&
				handItem.itemType != ItemType.Suit &&
				handItem.itemType != ItemType.Uniform &&
				PlayerManager.LocalPlayerScript.IsInReach(mousePos, false))
			{
				if (UIManager.CurrentIntent == Intent.Harm ||
					handItem.itemType != ItemType.Gun ||
					handItem.itemType != ItemType.Knife ||
					handItem.itemType != ItemType.Belt)
				{
					Vector2 dir = (mousePos - PlayerManager.LocalPlayer.transform.position).normalized;

					//special case - when we have a gun and click ourselves, we should actually shoot ourselves rather than melee, which is handled elsewhere
					if (handItem.itemType == ItemType.Gun && originator == gameObject)
					{
						return false;
					}

					PlayerScript lps = PlayerManager.LocalPlayerScript;

					if (tileMapLayer == null)
					{
						lps.weaponNetworkActions.CmdRequestMeleeAttack(gameObject, UIManager.Hands.CurrentSlot.eventName, dir,
							UIManager.DamageZone, LayerType.None);
					}
					else
					{
						lps.weaponNetworkActions.CmdRequestMeleeAttack(gameObjectRoot, UIManager.Hands.CurrentSlot.eventName, dir,
							UIManager.DamageZone, tileMapLayer.LayerType);
					}
					return true;
				}
			}
		}
		return false;
	}
}