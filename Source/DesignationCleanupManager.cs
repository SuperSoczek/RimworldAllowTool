﻿using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AllowTool {
	/// <summary>
	/// Periodically cleans up designations that no longer have valid targets.
	/// </summary>
	public static class DesignationCleanupManager {
		private const int TickInterval = 30;

		private static readonly Queue<Designation> cleanupList = new Queue<Designation>();

		public static void Tick(int currentTick) {
			if (Current.Game == null || Current.Game.Maps == null) return;
			var maps = Current.Game.Maps;
			var tickStep = currentTick % TickInterval;
			for (int i = 0; i < maps.Count; i++) {
				var map = maps[i];
				// distribute maps on the time spectrum for better performance
				if (tickStep == map.uniqueID % TickInterval) {
					CleanupDesignations(map);
				}
			}
		}

		private static void CleanupDesignations(Map map) {
			if(map.designationManager == null) return;
			var mapDesignations = map.designationManager.allDesignations;
			for (int i = 0; i < mapDesignations.Count; i++) {
				var des = mapDesignations[i];
				var desThing = des.target.Thing;
				if (desThing != null &&
					((des.def == AllowToolDefOf.FinishOffDesignation && !Designator_FinishOff.IsValidDesignationTarget(des.target.Thing))
					|| (des.def == AllowToolDefOf.HaulUrgentlyDesignation && desThing.IsInValidBestStorage())
					|| (des.def == AllowToolDefOf.RearmUrgentlyDesignation && (!(desThing is Building_TrapRearmable) || (desThing as  Building_TrapRearmable).Armed))
					)){
					cleanupList.Enqueue(des);
				}
			}
			while (cleanupList.Count > 0) {
				var des = cleanupList.Dequeue();
				des.designationManager.RemoveDesignation(des);
			}
		}
	}
}