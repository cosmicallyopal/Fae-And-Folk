using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using StardewValley.Buffs;

namespace FaeAndFolk
{
    public class ModEntry : Mod
    {
        public static IMonitor SMonitor;
        public static IModHelper SHelper;

        // Configurable keys
        private SButton DowsingKey = SButton.F;

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;

            // 1. Initialize Harmony Patches
            var harmony = new Harmony(this.ModManifest.UniqueID);
            
            // Patch for Heart Willow Trees (Star Tar drops)
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.TerrainFeatures.FruitTree), nameof(StardewValley.TerrainFeatures.FruitTree.performToolAction)),
                prefix: new HarmonyMethod(typeof(HeartWillowPatch), nameof(HeartWillowPatch.Prefix))
            );

            // Patch for Spirit Sight (Hiding Sparks/Forage)
            SpiritSightPatch.ApplyPatches(harmony);

            // 2. Event Listeners
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Apply Witch Bells Luck Buff if power is unlocked
            if (Game1.player.modData.ContainsKey("cosmicopal.HasWitchBells"))
            {
                // Add a +1 Luck Buff for the day
                Buff luckBuff = new Buff(
                    id: "cosmicopal.WitchBellsLuck",
                    displayName: "Witch's Blessing",
                    iconTexture: Game1.objectSpriteSheet,
                    iconSheetIndex: 1, // Placeholder icon index
                    duration: -2, // Lasts all day
                    effects: new BuffEffects() { LuckLevel = { 1 } }
                );
                Game1.player.applyBuff(luckBuff);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            // HANDLE ARTIFACT CONSUMPTION (Right Click)
            if (e.Button.IsUseToolButton())
            {
                ArtifactHandler.TryConsumeArtifact(Game1.player);
            }

            // HANDLE DOWSING ABILITY (F Key)
            if (e.Button == DowsingKey)
            {
                if (Game1.player.modData.ContainsKey("cosmicopal.HasDowsingRod"))
                {
                    PerformDowsing(Game1.player);
                }
            }
        }

        private void PerformDowsing(Farmer player)
        {
            GameLocation location = player.currentLocation;
            Vector2 playerPos = player.Tile;
            
            Vector2? nearestForage = null;
            double nearestDistance = double.MaxValue;

            // Scan for forage
            foreach (var pair in location.objects.Pairs)
            {
                StardewValley.Object obj = pair.Value;
                // Check if it is a spawn (forage) or marked as forage
                if (obj.IsSpawnedObject || obj.isForage())
                {
                    // If it's a "Spirit" item, only show if they have Sight
                    if ((obj.ItemId == "(O)cosmicopal.WispDust" || obj.ItemId == "(O)cosmicopal.SpiritOrchid") 
                        && !player.modData.ContainsKey("cosmicopal.HasSpiritSight"))
                    {
                        continue;
                    }

                    double dist = Vector2.Distance(playerPos, pair.Key);
                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                        nearestForage = pair.Key;
                    }
                }
            }

            if (nearestForage.HasValue)
            {
                location.playSound("wand");
                
                // Visual Sparkle Effect on the forage
                this.Helper.Reflection
	                .GetMethod(Game1.Multiplayer, "broadcastSprites")
	                .Invoke(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors",
		                new Rectangle(346, 398, 7, 9), nearestForage.Value * 64f, false, 0f, Color.Gold)
	                {
		                scale = 5f,
		                totalNumberOfLoops = 2,
		                interval = 100f,
		                animationLength = 4,
		                hasLit = true,
		                lightcolor = Color.Gold,
		                lightRadius = 2f
	                });

                Game1.showGlobalMessage("Your instinct pulls you toward hidden bounty!");
            }
            else
            {
                location.playSound("breath");
                Game1.showGlobalMessage("You sense nothing nearby.");
            }
        }
    }
}