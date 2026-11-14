/*
 * Bulk Sweep tool: a toolbar tool that works like Bulk Dig, for loose items. Activate it,
 * pick a priority, then click one loose item. Every revealed item of that same element on
 * the current asteroid is marked for Sweep at that priority.
 *
 * Fogged cells and other asteroids are never touched (see BulkSweepable.SweepAll).
 */

using UnityEngine;

namespace WillRowe.BulkSweepByType {
	public sealed class BulkSweepTool : DragTool {
		public static BulkSweepTool Instance { get; private set; }

		private bool handledThisClick;

		protected override void OnPrefabInit() {
			base.OnPrefabInit();
			Instance = this;
			// Same setup as BulkDigTool: Brush mode fires OnDragTool straight from the
			// click, and needs a non-null visualizer, borrowed from the vanilla tool.
			SetMode(Mode.Brush);
			var source = ClearTool.Instance != null ? ClearTool.Instance.visualizer : null;
			if (source == null && DigTool.Instance != null)
				source = DigTool.Instance.visualizer;
			if (source != null) {
				visualizer = Util.KInstantiate(source);
				visualizer.SetActive(false);
			}
		}

		protected override void OnCleanUp() {
			if (Instance == this)
				Instance = null;
			base.OnCleanUp();
		}

		protected override void OnActivateTool() {
			base.OnActivateTool();
			ToolMenu.Instance.PriorityScreen.Show(true);
		}

		protected override void OnDeactivateTool(InterfaceTool newTool) {
			ToolMenu.Instance.PriorityScreen.Show(false);
			base.OnDeactivateTool(newTool);
		}

		public override void OnLeftClickUp(Vector3 cursorPos) {
			base.OnLeftClickUp(cursorPos);
			handledThisClick = false;
		}

		protected override void OnDragTool(int cell, int distFromOrigin) {
			if (handledThisClick || !Grid.IsValidCell(cell) || !Grid.IsVisible(cell))
				return;
			var head = Grid.Objects[cell, (int)ObjectLayer.Pickupables];
			if (head == null || !head.TryGetComponent(out Pickupable pickupable))
				return;
			var node = pickupable.objectLayerListItem;
			while (node != null) {
				var content = node.gameObject;
				node = node.nextItem;
				if (content == null || content.TryGetComponent(out MinionIdentity _))
					continue;
				if (content.TryGetComponent(out PrimaryElement pe) &&
						content.TryGetComponent(out Clearable cc) && cc.isClearable) {
					handledThisClick = true;
					BulkSweepable.SweepAll(cell, pe.ElementID, ToolMenu.Instance.
						PriorityScreen.GetLastSelectedPriority());
					return;
				}
			}
		}
	}
}
