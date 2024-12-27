#if SURVIVAL
using System;
using System.Collections.Generic;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        private static List<BlockID> sprites = new List<BlockID>()
        { 6, 66, 67, 68, 83, 84 };


        void configureSurvival()
        {
            // hack for this version to update neigbors when a block is displaced by mining
            metaBlocksTypes[0] = typeof(AirBlock);

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

            SimpleSurvival.BlockMineConfig mineConfigLamp = new SimpleSurvival.BlockMineConfig() {
                MiningTime = 6,
                overrideBlock = RedstoneLamp.INACTIVE_ID
            };

            SimpleSurvival.BlockMineConfig mineConfigRedstoneBlock = new SimpleSurvival.BlockMineConfig() {
                MiningTime = 15,
                //overrideBlock = RedstoneBlock.baseID
                overrideBlock = 208
            };

            SimpleSurvival.BlockMineConfig mineConfigRepeater = new SimpleSurvival.BlockMineConfig() {
                MiningTime = 1,
                overrideBlock = Repeater.INACTIVE_ID
            };

            SimpleSurvival.BlockMineConfig mineConfigSwitch = new SimpleSurvival.BlockMineConfig() {
                MiningTime = 6,
                overrideBlock = Switch.INACTIVE_ID
            };

            SimpleSurvival.BlockMineConfig mineConfigRedstoneTorch = new SimpleSurvival.BlockMineConfig() {
                MiningTime = 3,
                overrideBlock = RedstoneTorch.ACTIVE_ID
            };

            SimpleSurvival.blockMiningTimes[88] = mineConfigRedstoneOre;
            SimpleSurvival.blockMiningTimes[RedstoneLamp.ACTIVE_ID] = mineConfigLamp;
            SimpleSurvival.blockMiningTimes[RedstoneLamp.INACTIVE_ID] = mineConfigLamp;
            SimpleSurvival.blockMiningTimes[Repeater.ACTIVE_ID] = mineConfigRepeater;
            SimpleSurvival.blockMiningTimes[Repeater.INACTIVE_ID] = mineConfigRepeater;
            SimpleSurvival.blockMiningTimes[Switch.ACTIVE_ID] = mineConfigSwitch;
            SimpleSurvival.blockMiningTimes[Switch.INACTIVE_ID] = mineConfigSwitch;
            SimpleSurvival.blockMiningTimes[RedstoneTorch.ACTIVE_ID] = mineConfigRedstoneTorch;
            SimpleSurvival.blockMiningTimes[RedstoneTorch.INACTIVE_ID] = mineConfigRedstoneTorch;
            //SimpleSurvival.blockMiningTimes[RedstoneBlock.baseID] = mineConfigRedstoneBlock;
            SimpleSurvival.blockMiningTimes[208] = mineConfigRedstoneBlock;
            for(int i = 0; i < 16*3; i++) {
                SimpleSurvival.blockMiningTimes[(ushort)(i+config.START_ID)] = mineConfigRedstoneWire;
            }

            // recipes

            // redstone block = 2 redstone dust
            addCraftingRecipe(RedstoneBlock.baseID, 1, false, RedstoneDust.baseID, 2);
            // lamp = 1 redstone dust + 1 glowstone
            addCraftingRecipe(RedstoneLamp.INACTIVE_ID, 1, true, RedstoneDust.baseID, 1, 79, 1);
            // switch = 1 wood + 4 cobblestone
            addCraftingRecipe(Switch.INACTIVE_ID, 1, true, 5, 1, 4, 4);
            // redstone torch = 1 stick + 1 redstone dust
            addCraftingRecipe(RedstoneTorch.ACTIVE_ID, 1, false, 115, 1, RedstoneDust.baseID, 1);
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
            public static new BlockID baseID;

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
                for(int i = 0; i < 3; i++) {
                    for(int j = 0; j < 8; j++) {
                        if(j == 0 || j == 1 || j == 4 || j == 5)
                            continue;

                        BlockID id = (BlockID)(300+(i*8)+j+256);
                        metaBlocksTypes[id] = typeof(RedstoneDoor);
                    }
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
                }
            }
        }

        public static bool isDoor(BlockID id)
        {
            id -= (BlockID)300+256;
            return (id >= 0 && id <= 3*8);
        }
    }
}

#endif
