using StardewValley;
using StardewValley.Buffs;

namespace FaeAndFolk
{
    public static class ArtifactHandler
    {
        public static void TryConsumeArtifact(Farmer player)
        {
            if (player.CurrentItem == null) return;

            string itemId = player.CurrentItem.ItemId;

            // 1. WITCH BELLS (Protection)
            if (itemId == "(O)cosmicopal.WitchBells")
            {
                if (!player.modData.ContainsKey("cosmicopal.HasWitchBells"))
                {
                    ConsumeItem(player, "cosmicopal.HasWitchBells", "The bells dissolve into light... Fortune smiles upon you daily now.");
                    // Apply immediate buff for today
                    player.applyBuff(new Buff("cosmicopal.WitchBellsLuck", duration: 120000, effects: new BuffEffects() { LuckLevel = { 1 } }));
                }
                else
                {
                    Game1.showRedMessage("You have already absorbed this power.");
                }
            }
            // 2. DOWSING ROD (Instinct)
            else if (itemId == "(O)cosmicopal.DowsingRod")
            {
                if (!player.modData.ContainsKey("cosmicopal.HasDowsingRod"))
                {
                    ConsumeItem(player, "cosmicopal.HasDowsingRod", "Your senses expand. Press 'F' to sense forage.");
                }
                else
                {
                    Game1.showRedMessage("You already possess the Forage Instinct.");
                }
            }
            // 3. HAG STONE (Vision)
            else if (itemId == "(O)cosmicopal.HagStone")
            {
                if (!player.modData.ContainsKey("cosmicopal.HasSpiritSight"))
                {
                    ConsumeItem(player, "cosmicopal.HasSpiritSight", "The stone crumbles... Your Third Eye is opened.");
                    Game1.playSound("yoba"); // Mystical sound
                }
                else
                {
                    Game1.showRedMessage("Your Third Eye is already open.");
                }
            }
        }

        private static void ConsumeItem(Farmer player, string modDataKey, string successMessage)
        {
            // Set the permanent flag
            player.modData[modDataKey] = "true";
            
            // Remove one item from stack
            player.reduceActiveItemByOne();
            
            // Feedback
            Game1.playSound("powerup");
            Game1.showGlobalMessage(successMessage);
            player.doEmote(Farmer.happyEmote);
        }
    }
}