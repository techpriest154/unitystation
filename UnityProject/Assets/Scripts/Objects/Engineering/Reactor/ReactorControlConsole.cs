﻿using System.Collections.Generic;
using UnityEngine;
using Systems.ObjectConnection;


namespace Objects.Engineering
{
	public class ReactorControlConsole : MonoBehaviour, IMultitoolSlaveable
	{
		[SceneObjectReference] public ReactorGraphiteChamber ReactorChambers = null;

		[SceneObjectReference] public List<ReactorGraphiteChamber> ReactorChambers2 = new List<ReactorGraphiteChamber>();

		public void SuchControllRodDepth(float requestedDepth)
		{
			requestedDepth = requestedDepth.Clamp(0, 1);

			if (ReactorChambers != null)
			{
				ReactorChambers.SetControlRodDepth(requestedDepth);
			}
		}

		#region Multitool Interaction

		MultitoolConnectionType IMultitoolLinkable.ConType => MultitoolConnectionType.ReactorChamber;
		IMultitoolMasterable IMultitoolSlaveable.Master { get => ReactorChambers; set => SetMaster(value); }
		bool IMultitoolSlaveable.RequireLink => true;

		private void SetMaster(IMultitoolMasterable master)
		{
			var Chamber = (master as Component)?.gameObject.GetComponent<ReactorGraphiteChamber>();
			if (Chamber != null)
			{
				ReactorChambers = Chamber;
			}
		}

		#endregion
	}
}
