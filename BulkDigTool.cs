/*
 * Bulk Dig tool: a new toolbar tool (added next to the vanilla tools, not replacing any
 * of them). Activate it, pick a priority like the normal Dig tool, then click any one
 * tile of solid terrain. Every tile of that same material anywhere on the map gets a Dig
 * marker at that priority - e.g. click one Slime tile to queue every Slime deposit on the
 * map, or one Polluted Dirt tile to queue every Polluted Dirt deposit.
 *
 * Dig has a genuine public API for marking a tile: DigTool.PlaceDig(cell, delay) is a
 * public static method (confirmed by reflecting over the installed game assembly), so no
 * private-field reflection is needed there.
 */

using UnityEngine;

namespace WillRowe.BulkSweepByType {
	public sealed class BulkDigTool : DragTool {
		public static BulkDigTool Instance { get; private set; }

		private bool handledThisClick;

		protected override void OnPrefabInit() {
			base.OnPrefabInit();
			Instance = this;
			// DragTool defaults to Mode.Box, which only fires OnDragTool from
			// OnLeftClickUp, and only then if an areaVisualizer is set. Brush mode
			// fires OnDragTool directly from the click, and only needs `visualizer`
			// to be non-null - borrowed here from the real Dig tool (its field is
			// public, so no reflection needed).
			SetMode(Mode.Brush);
			var digTool = DigTool.Instance;
			if (digTool != null && digTool.visualizer != null) {
				visualizer = Util.KInstantiate(digTool.visualizer);
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
			// Brush mode never calls OnDragComplete, so reset the debounce here.
			handledThisClick = false;
		}

		protected override void OnDragTool(int cell, int distFromOrigin) {
			// Brush mode always reports distFromOrigin 0, and calls this once per
			// interpolated point if the mouse moves at all during the click - debounce
			// so one click only triggers one map-wide scan.
			if (handledThisClick || !Grid.IsValidCell(cell) || !Grid.Solid[cell])
				return;
			var element = Grid.Element[cell];
			if (element == null)
				return;
			handledThisClick = true;
			DigAllMatching(element.id, ToolMenu.Instance.PriorityScreen.
				GetLastSelectedPriority());
		}

		private void DigAllMatching(SimHashes elementId, PrioritySetting priority) {
			int cellCount = Grid.CellCount;
			for (int cell = 0; cell < cellCount; cell++) {
				if (!Grid.IsValidCell(cell) || !Grid.Solid[cell])
					continue;
				var element = Grid.Element[cell];
				if (element == null || element.id != elementId || !Diggable.IsDiggable(cell))
					continue;
				var diggable = Diggable.GetDiggable(cell);
				var placer = diggable != null ? diggable.gameObject : DigTool.PlaceDig(cell, 0);
				if (placer != null && placer.TryGetComponent(out Prioritizable pr))
					pr.SetMasterPriority(priority);
			}
		}
	}
}
