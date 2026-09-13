using HarmonyLib;
using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;
using UnityEngine;

namespace WillRowe.BulkSweepByType {
	/// <summary>
	/// Mod entry point: loads Harmony patches that attach BulkSweepable to every
	/// sweepable prefab as it is created, and registers the Bulk Dig toolbar tool.
	/// </summary>
	public sealed class BulkSweepByTypeMod : KMod.UserMod2 {
		/// <summary>
		/// A real, properly-registered "no default key" action, used everywhere this mod
		/// needs to hand the game a hotkey it isn't actually binding to anything. Action.
		/// Invalid throws (GameUtil.GetHotkeyString rejects any action that was never
		/// registered in GameInputBindings), so this mod must register one of its own via
		/// PLib before the toolbar or info-panel button are built.
		/// </summary>
		internal static Action NoHotkey { get; private set; }

		public override void OnLoad(Harmony harmony) {
			base.OnLoad(harmony);
			PUtil.InitLibrary();
			NoHotkey = new PActionManager().CreateAction("WillRowe.BulkSweepByType.NoHotkey",
				"Bulk Sweep/Dig Tools").GetKAction();
			harmony.PatchAll();
		}
	}

	/// <summary>
	/// Makes the Bulk Dig tool instantiable, alongside every other tool.
	/// </summary>
	[HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
	public static class PlayerController_OnPrefabInit_Patch {
		internal static void Postfix(PlayerController __instance) {
			PToolMode.RegisterTool<BulkDigTool>(__instance);
		}
	}

	/// <summary>
	/// Adds a new "Bulk Dig" button to the toolbar, alongside the vanilla tools rather
	/// than replacing any of them.
	/// </summary>
	[HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
	public static class ToolMenu_CreateBasicTools_Patch {
		private const string TOOL_TEXT = "Bulk Dig";

		internal static void Postfix(ToolMenu __instance) {
			var tools = __instance.basicTools;
			// CreateBasicTools runs again every time a colony is loaded, so guard
			// against adding a second (third, fourth...) copy of our entry.
			for (int i = 0; i < tools.Count; i++)
				if (tools[i].text == TOOL_TEXT)
					return;
			tools.Add(ToolMenu.CreateToolCollection(TOOL_TEXT,
				"icon_action_dig", BulkSweepByTypeMod.NoHotkey, nameof(BulkDigTool),
				"Digs every tile of one material on the map, at the priority you pick.",
				false));
		}
	}

	/// <summary>
	/// Attaches BulkSweepable to loose ore/mineral chunk prefabs (rock, ice, ore, etc).
	/// </summary>
	[HarmonyPatch(typeof(EntityTemplates), nameof(EntityTemplates.CreateBaseOreTemplates))]
	public static class EntityTemplates_CreateBaseOreTemplates_Patch {
		internal static void Postfix(GameObject ___baseOreTemplate) {
			___baseOreTemplate.AddOrGet<BulkSweepable>();
		}
	}

	/// <summary>
	/// Attaches BulkSweepable to other loose/pickupable prefabs (bottled liquids and
	/// gases, food, eggs, artifacts, and similar).
	/// </summary>
	[HarmonyPatch(typeof(EntityTemplates), nameof(EntityTemplates.CreateLooseEntity))]
	public static class EntityTemplates_CreateLooseEntity_Patch {
		internal static void Postfix(GameObject __result) {
			__result.AddOrGet<BulkSweepable>();
		}
	}
}
