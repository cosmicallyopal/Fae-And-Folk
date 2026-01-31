using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile.Dimensions;

namespace FaeAndFolk
{
    public static class SpiritSightPatch
    {
        public static void ApplyPatches(Harmony harmony)
        {
            // Patch Object Drawing (Forage)
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.draw), new System.Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                prefix: new HarmonyMethod(typeof(SpiritSightPatch), nameof(Prefix_ObjectDraw))
            );

            // Patch NPC Drawing (Sparks)
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.draw), new System.Type[] { typeof(SpriteBatch), typeof(float) }),
                prefix: new HarmonyMethod(typeof(SpiritSightPatch), nameof(Prefix_NPCDraw))
            );

            // Patch Interaction (Prevent clicking invisible things)
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                prefix: new HarmonyMethod(typeof(SpiritSightPatch), nameof(Prefix_CheckAction))
            );
        }

        // Hide Wisp Dust / Spirit Orchids
        public static bool Prefix_ObjectDraw(StardewValley.Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (__instance.ItemId == "(O)cosmicopal.WispDust" || __instance.ItemId == "(O)cosmicopal.SpiritOrchid")
            {
                // If player DOES NOT have sight, skip drawing (return false)
                if (!Game1.player.modData.ContainsKey("cosmicopal.HasSpiritSight"))
                {
                    return false;
                }
            }
            return true; // Draw normally
        }

        // Hide Sparks (NPC)
        public static bool Prefix_NPCDraw(NPC __instance, SpriteBatch b, float alpha = 1f)
        {
            if (__instance.Name == "Sparks")
            {
                if (!Game1.player.modData.ContainsKey("cosmicopal.HasSpiritSight"))
                {
                    return false;
                }
            }
            return true;
        }

        // Prevent interacting with invisible Sparks
        public static bool Prefix_CheckAction(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            // Check for Sparks at this location
            NPC character = __instance.isCharacterAtTile(new Vector2(tileLocation.X, tileLocation.Y));
            if (character != null && character.Name == "Sparks")
            {
                 if (!who.modData.ContainsKey("cosmicopal.HasSpiritSight"))
                 {
                     __result = false;
                     return false; // Block interaction
                 }
            }
            return true;
        }
    }
}