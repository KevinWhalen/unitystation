﻿using UnityEngine;


/// <summary>
/// Player 2 Player interactions. Also used for clicking on yourself
/// </summary>
public class P2PInteractions : InputTrigger
{
	public override bool Interact(GameObject originator, Vector3 position, string hand)
	{
		if (UIManager.Hands.CurrentSlot.Item != null)
		{
			//Is the item edible?
			if (CheckEdible(UIManager.Hands.CurrentSlot.Item))
			{
				return true;
			}

			if (CheckMedical(UIManager.Hands.CurrentSlot.Item, originator))
			{
				return true;
			}
		}

		return false;
	}


	private bool CheckMedical(GameObject itemInHand, GameObject originator){
		if(itemInHand.GetComponent<ItemAttributes>().itemType == ItemType.Medical){
			PlayerManager.LocalPlayerScript.playerNetworkActions.CmdAttack(gameObject, originator, UIManager.DamageZone, itemInHand);
			return true;
		}
		return false;
	}

	private bool CheckEdible(GameObject itemInHand)
	{
		FoodBehaviour baseFood = itemInHand.GetComponent<FoodBehaviour>();
		if (baseFood == null || UIManager.CurrentIntent == Intent.Harm)
		{
			return false;
		}

		if (PlayerManager.LocalPlayer == gameObject)
		{
			//Clicked on yourself, try to eat the food
			baseFood.TryEat();
		}
		else
		{
			//Clicked on someone else
			//TODO create a new method on FoodBehaviour for feeding others
			//and use that here
		}
		return true;
	}
}
