﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Systems.Electricity;
using HealthV2;
using Initialisation;

namespace Doors.Modules
{
	public class ElectrifiedDoorModule : DoorModuleBase
	{
		[SerializeField] private int voltageDamage = 9080;
		[SerializeField] private bool isElectrecuted = false;

		public bool IsElectrecuted
		{
			get => isElectrecuted;
			set => isElectrecuted = value;
		}

		private bool OneTimeElectrecuted = false;

		protected override void Awake()
		{
			base.Awake();
			LoadManager.RegisterActionDelayed(DelayedRegister, 2);
		}

		public void DelayedRegister()
		{
			master.HackingProcessBase.RegisterPort(ToggleElectrocution, master.GetType());
			master.HackingProcessBase.RegisterPort(PreventElectrocution, master.GetType());
		}

		public void ToggleElectrocutionInput()
		{
			master.HackingProcessBase.ImpulsePort(ToggleElectrocution);
		}


		public void ToggleElectrocution()
		{
			if (master.HasPower) return;
			IsElectrecuted = !IsElectrecuted;
		}

		public override ModuleSignal OpenInteraction(HandApply interaction, HashSet<DoorProcessingStates> States)
		{
			if (interaction == null)
			{
				return ModuleSignal.Continue;
			}

			return CanElectricute(interaction.Performer);
		}

		public override ModuleSignal ClosedInteraction(HandApply interaction, HashSet<DoorProcessingStates> States)
		{
			return CanElectricute(interaction.Performer);
		}

		public override ModuleSignal BumpingInteraction(GameObject mob, HashSet<DoorProcessingStates> States)
		{
			return CanElectricute(mob);
		}

		public void PulsePreventElectrocution()
		{
			master.HackingProcessBase.ImpulsePort(PreventElectrocution);
		}

		public void PreventElectrocution()
		{
			if (master.HasPower)
			{
				OneTimeElectrecuted = false;
			}
		}

		private ModuleSignal CanElectricute(GameObject mob)
		{
			if (master.HasPower)
			{
				if (IsElectrecuted == false)
				{
					OneTimeElectrecuted = true;
					PulsePreventElectrocution();
					if (OneTimeElectrecuted)
					{
						OneTimeElectrecuted = false;
						if (PlayerHasInsulatedGloves(mob) == false)
						{
							ServerElectrocute(mob);
							return ModuleSignal.Continue;
						}

						return ModuleSignal.ContinueRegardlessOfOtherModulesStates;
					}
				}
				else
				{
					if (PlayerHasInsulatedGloves(mob) == false)
					{
						ServerElectrocute(mob);
						return ModuleSignal.Continue;
					}

					return ModuleSignal.ContinueRegardlessOfOtherModulesStates;
				}
			}
			return ModuleSignal.Continue;
		}

		private bool PlayerHasInsulatedGloves(GameObject mob)
		{
			List<ItemSlot> slots = mob.GetComponent<PlayerScript>().OrNull()?.DynamicItemStorage.OrNull()
				?.GetNamedItemSlots(NamedSlot.hands);
			if (slots != null)
			{
				foreach (ItemSlot slot in slots)
				{
					if (Validations.HasItemTrait(slot.ItemObject, CommonTraits.Instance.Insulated))
					{
						return true;
					}
				}
			}

			return false;
		}

		private void ServerElectrocute(GameObject obj)
		{
			LivingHealthMasterBase healthScript = obj.GetComponent<LivingHealthMasterBase>();
			if (healthScript != null)
			{
				var electrocution =
					new Electrocution(voltageDamage, master.RegisterTile.WorldPositionServer,
						"wire"); //More magic numbers.
				healthScript.Electrocute(electrocution);
			}
		}

		public override bool CanDoorStateChange()
		{
			if (master.HasPower && IsElectrecuted)
			{
				return false;
			}

			return true;
		}
	}
}