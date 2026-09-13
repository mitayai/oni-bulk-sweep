/*
 * Bulk Sweep by Type
 *
 * Adds a button to the info panel of any sweepable item ("Sweep All: <Material>") that
 * marks every item of the same element anywhere on the map for Sweep, at the priority
 * currently set on the item you clicked.
 */

using UnityEngine;

namespace WillRowe.BulkSweepByType {
	/// <summary>
	/// Attached to every sweepable debris item. Shows a button in its info panel that fans
	/// the Sweep errand + priority out to every other item of the same element on the map.
	/// </summary>
	public sealed class BulkSweepable : KMonoBehaviour {
		private Clearable clearable;
		private PrimaryElement primaryElement;
		private Prioritizable prioritizable;

		protected override void OnSpawn() {
			base.OnSpawn();
			TryGetComponent(out clearable);
			TryGetComponent(out primaryElement);
			TryGetComponent(out prioritizable);
			Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenu);
		}

		/// <summary>
		/// Adds our button to the selected item's action row, only for items that can
		/// actually be swept and have both an element and a priority dial.
		/// </summary>
		private void OnRefreshUserMenu(object _) {
			if (clearable == null || primaryElement == null || prioritizable == null ||
					!clearable.isClearable)
				return;
			string elementName = primaryElement.Element != null ?
				primaryElement.Element.name : "this material";
			Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
				"action_clear",
				"Sweep All: " + elementName,
				SweepAllMatching,
				BulkSweepByTypeMod.NoHotkey,
				null, null, null,
				"Marks every " + elementName +
					" on the map for Sweep, at this item's current priority."));
		}

		/// <summary>
		/// Scans the whole map for debris of the same element as this item and marks it
		/// all for Sweep at this item's current priority.
		/// </summary>
		private void SweepAllMatching() {
			if (primaryElement == null || prioritizable == null)
				return;
			SimHashes elementId = primaryElement.ElementID;
			PrioritySetting priority = prioritizable.GetMasterPriority();
			int cellCount = Grid.CellCount;
			for (int cell = 0; cell < cellCount; cell++) {
				if (!Grid.IsValidCell(cell))
					continue;
				var head = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
				if (head == null || !head.TryGetComponent(out Pickupable pickupable))
					continue;
				// Multiple items can stack in one cell as a linked list.
				var node = pickupable.objectLayerListItem;
				while (node != null) {
					var content = node.gameObject;
					node = node.nextItem;
					if (content == null || content.TryGetComponent(out MinionIdentity _))
						continue;
					if (content.TryGetComponent(out PrimaryElement pe) &&
							pe.ElementID == elementId &&
							content.TryGetComponent(out Clearable cc) && cc.isClearable) {
						cc.MarkForClear();
						if (content.TryGetComponent(out Prioritizable pr))
							pr.SetMasterPriority(priority);
					}
				}
			}
		}
	}
}
