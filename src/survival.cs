#if SURVIVAL
using System;
using System.Collections.Generic;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        void configureSurvival()
        {
            RedstoneDust.addDefinitions();
            RedstoneDoor.addDefinitions();

            // mine config

            SimpleSurvival.BlockMineConfig mineConfigRedstoneWire = new SimpleSurvival.BlockMineConfig() {
                MiningTime = 1,
                overrideBlock = RedstoneDust.baseID
            };

            SimpleSurvival.BlockMineConfig mineConfigRedstoneOre = new SimpleSurvival.BlockMineConfig() {
                MiningTime = 1,
                overrideBlock = RedstoneDust.baseID
            };

            SimpleSurvival.blockMiningTimes[88] = mineConfigRedstoneOre;
            for(int i = 0; i < 16*3; i++) {
                SimpleSurvival.blockMiningTimes[(ushort)(i+config.START_ID)] = mineConfigRedstoneWire;
            }

            // recipes

            // redstone block = redstone dust * 4
            addCraftingRecipe(RedstoneBlock.baseID, 1, false, RedstoneDust.baseID, 4);
        }

        void addCraftingRecipe(BlockID product, ushort produced, bool needCraftingTable, params ushort[] ingredients)
        {
            Dictionary<BlockID, ushort> ingredientEntries = new Dictionary<BlockID, ushort>(ingredients.Length);

            int i = 0;
            while(i < ingredients.Length) {
                ingredientEntries[ingredients[i]] = ingredients[i+1];
                i += 2;
            }

            SimpleSurvival.CraftRecipe recipe = new SimpleSurvival.CraftRecipe(ingredientEntries, produced, needCraftingTable);
            SimpleSurvival.craftingRecipies[product] = recipe;
        }

        public class RedstoneDust : MetaBlock
        {
            public RedstoneDust(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions() {
                baseID = loadDefinition("RedstoneDust.json", typeof(RedstoneDust));
            }

            public override void onPlacement() {
                level.setBlock(index, (BlockID)(config.START_ID+256));
                level.applyBlockChanges();
                MetaBlock block = level.getMetaBlock(index);
                block.onPlacement();
            }
        }

        public class RedstoneDoor : SignalConsumerBlock
        {
            public RedstoneDoor(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions()
            {
                for(int i = 0; i < 3*8; i++) {
                    BlockID id = (BlockID)(300+i+256);
                    metaBlocksTypes[id] = typeof(RedstoneDoor);
                }
            }

            public override void update()
            {
                ushort x,y,z;
                level.level.IntToPos(index, out x, out y, out z);
                bool active = (maxNearbySignal() > 0) ? true : false;

                if(active) {
                    Door.OpenDoor(level.level, x,y,z);

                    // try to cancel update of block of the common structure
                    int yo; //ffset
                    if(Door.IsDoorBottom(level.level, x,y,z)) {
                        yo = 1;
                    } else {
                        yo = -1;
                    }

                    int i = level.level.IntOffset(index, 0,yo,0);
                    level.ignoreBlock(i);
                }
                else {
                    Door.CloseDoor(level.level, x,y,z);
                    Console.WriteLine("closing door");
                }
            }
        }
    }
}

#endif
