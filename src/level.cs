using MCGalaxy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockID = System.UInt16;


namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        public class CustomLevel
        {
            public Level level;
            public List<int> updateNeededBlocks;
            public List<int> ignoredBlocks = new List<int>();
            public bool updating;

            public Dictionary<int, MetaBlock> metaBlocks = new Dictionary<int, MetaBlock>();
            public Dictionary<int, BlockID> setBlockQueue = new Dictionary<int, BlockID>();
            public int ignoreSignalFromThisBlock;

            //public List<MetaBlock> blocksToUpdateEveryTick = new List<MetaBlock>();


            public CustomLevel(Level level)
            {
                this.level = level;
                updateNeededBlocks = new List<int>();
                updating = false;
            }

            public void delayUpdateBlock(int block) {
                updateNeededBlocks.Add(block);
            }

            public void ignoreBlock(int index) {
                ignoredBlocks.Add(index);
            }

            public void everyNeighboringBlock(int block, Action<MetaBlock> callMe)
            {
                for(int x = -1; x < 2; x++)
                    for(int y = -1; y < 2; y++)
                        for(int z = -1; z < 2; z++)
                        {
                            if(x == 0 && y == 0 && z == 0)
                                continue;

                            int index = level.IntOffset(block, x,y,z);
                            MetaBlock neighbor = getMetaBlock(index);
                            callMe(neighbor);
                        }
            }

            public void updateBaseOfNeighboringWire(MetaBlock block)
            {
                if(!RedstoneWire.isRedstone(block.id)) {
                    return;
                }
                RedstoneWire wire = (RedstoneWire)block;
                wire.updateBase();
            }

            public void updateBasesOfNeighboringWires(int index) {
                everyNeighboringBlock(index, updateBaseOfNeighboringWire);
            }

            public void delayUpdateOfNeighbors(int index)
            {
                everyNeighboringBlock(index, (MetaBlock block) =>
                                             {
                                                 if(block.mayNeedUpdate())
                                                     delayUpdateBlock(block.index);
                                             });
            }

            public void setWireToZero(MetaBlock block)
            {
                if((RedstoneWire.isRedstone(block.id) || isRepeater(block.id)) &&
                    block.getSignal() > 0) {
                    ushort x, y, z;
                    level.IntToPos(block.index, out x, out y, out z);

                    block.setSignal(0);
                    applyBlockChanges();
                    delayUpdateOfNeighbors(block.index);
                    setNeighborWiresToZero(block.index);
                }
            }

            public void setNeighborWiresToZero(int index) {
                everyNeighboringBlock(index, setWireToZero);
            }

            public void updateBlocks()
            {
                List<int> list = new List<int>(updateNeededBlocks);
                updateNeededBlocks.Clear();
                ignoredBlocks.Clear();

                foreach(var block in list) {
                    if(ignoredBlocks.Contains(block)) {
                        ignoredBlocks.Remove(block);
                        continue;
                    }

                    MetaBlock instance = getMetaBlock(block);
                    instance.update();
                }
            }

            public void update()
            {
                updating = true;

                while(updateNeededBlocks.Count > 0) {
                    updateBlocks();
                }

                applyBlockChanges();
                updating = false;

                foreach(var entry in metaBlocks) {
                    MetaBlock block = entry.Value;
                    block.onEndingUpdateCycle();
                }
            }

            // public void updateCycle()
            // {
            //     while(true)
            //     {
            //         if(!updating)
            //         {
            //             update();
            //
            //             foreach(var block in blocksToUpdateEveryTick) {
            //                 block.onTickUpdate();
            //             }
            //         }
            //
            //         Thread.Sleep(50);
            //     }
            // }


            // utils


            public BlockID getBlock(int block) {
                BlockID id;
                bool queuedToBeChanged = setBlockQueue.TryGetValue(block, out id);
                if(queuedToBeChanged) {
                    return id;
                }

                return this.level.FastGetBlock(block);
            }

            public void setBlock(int block, BlockID id) {
                setBlockQueue[block] = id;
            }

            public void applyBlockChanges()
            {
                foreach(var entry in setBlockQueue)
                {
                    int index = entry.Key;
                    BlockID id = entry.Value;

                    ushort x, y, z;
                    level.IntToPos(index, out x, out y, out z);
                    this.level.UpdateBlock(Player.Console, x, y, z, (ushort)(id));
                }

                setBlockQueue.Clear();
            }

            public BlockID getRawID(int block)
            {
                return (ushort)(getBlock(block) - 256);
            }

            public BlockID getRawID(BlockID id)
            {
                return (ushort)(id - 256);
            }

            public MetaBlock getMetaBlock(int block)
            {
                if(metaBlocks.ContainsKey(block)) {
                    return (MetaBlock)metaBlocks[block];
                }

                BlockID id = getBlock(block);
                if(!metaBlocksTypes.ContainsKey(id)) {
                    return defaultInstance;
                }

                Type type = metaBlocksTypes[id];
                MetaBlock def = (MetaBlock)Activator.CreateInstance(type, block, this, id);

                if(def.hasData) {
                    metaBlocks[block] = def;
                }

                return def;
            }

            public void removeBlockInstance(int block)
            {
                metaBlocks.Remove(block);
            }
        }

        public static bool isRepeater(BlockID id)
        {
            id -= 256;
            return (id == Repeater.ACTIVE_ID || id == Repeater.INACTIVE_ID);
        }
    }
}
