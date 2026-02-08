using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace FaeAndFolk
{
    public static class HeartWillowPatch
    {
        public static bool Prefix(FruitTree __instance, Tool t, int explosion, Vector2 tileLocation)
        {
            // 1. Check if it's our custom tree
            if (__instance.treeId.Value != "cosmicopal.HeartSapling")
            {
                return true; // Not our tree, run vanilla logic
            }

            // 2. Get Location safely
            GameLocation location = __instance.Location;
            if (location == null) return true;

            // 3. Calculate Damage
            float damage = 0f;
            if (explosion > 0)
            {
                damage = explosion;
            }
            else if (t != null && t is StardewValley.Tools.Axe)
            {
                damage = t.getLastFarmerToUse().toolPower.Value + 1;
            }

            // 4. If tree is destroyed (Health <= 0)
            if (__instance.health.Value - damage <= 0)
            {
                location.playSound("treeCrack");
                location.playSound("stumpCrack");

                // Drop Hardwood (3-5)
                int hardwoodCount = Game1.random.Next(3, 6);
                for (int i = 0; i < hardwoodCount; i++)
                {
                    Game1.createItemDebris(ItemRegistry.Create("(O)709", 1), __instance.Tile * 64f, -1, location);
                }

                // Drop Star Tar (1) - Special Item
                Game1.createItemDebris(ItemRegistry.Create("(O)cosmicopal.StarTar", 1), __instance.Tile * 64f, -1, location);

                // Drop Sapling (1) - Refund seed
                Game1.createItemDebris(ItemRegistry.Create("(O)cosmicopal.HeartSapling", 1), __instance.Tile * 64f, -1, location);

                // Add explosion animation
                location.TemporarySprites.Add(new TemporaryAnimatedSprite(5, __instance.Tile * 64f, Color.White));

                // Remove the tree from the map
                location.terrainFeatures.Remove(__instance.Tile);

                return false; // Stop original logic (prevent vanilla wood drops)
            }

            return true; // Continue normal damage logic
        }
    }
}