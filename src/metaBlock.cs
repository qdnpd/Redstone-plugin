using System;
using MCGalaxy;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        public abstract class MetaBlock
        {
            public int index;
            public BlockID id;
            public CustomLevel level;
            public virtual bool hasData {get;} = false;
            public virtual bool connectable {get; set;} = false;
            public bool updateOnTick = false;
            public string name = "none";

            public static BlockID baseID;

            public MetaBlock(int block, CustomLevel level, BlockID id) {
                this.index = block;
                this.level = level;
                this.id    = id;
            }

            public virtual ushort getSignal() {
                return 0;
            }

            public virtual void setSignal(ushort signal) {}

            public virtual bool mayNeedUpdate() {
                return false;
            }

            public virtual void update() {}
            public virtual void onEndingUpdateCycle() {}
            public virtual void onPlacement() {}
            public virtual void onDisplacement() {}
            public virtual void onClick() {}
            public virtual void onStep() {}
            public virtual void onUnstep() {}
            public static void addDefinitions() {}

            public static BlockID loadDefinition(string path, Type type)
            {
                var def = loadDefinitionFromFile(path);
                BlockID id = getDefinitionID();
                addDefinition(id, def, type);

                return id;
            }

            public static BlockDefinition loadDefinitionFromFile(string file)
            {
                BlockDefinition[] defs = BlockDefinition.Load("plugins/Redstone/Data/" + file);
                var def = defs[0];
                return def;
            }
        }


        public class DefaultBlock : MetaBlock
        {
            public DefaultBlock(int block, CustomLevel level, BlockID id) : base(block, level, id) {}
        }

        #if SURVIVAL
        public class AirBlock : MetaBlock
        {
            public AirBlock(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public override void onPlacement()
            {
                level.updateBasesOfNeighboringWires(index);

                level.everyNeighboringBlock(index, (MetaBlock block) =>
                {
                    if(block.mayNeedUpdate() && !isDoor(block.id))
                        level.delayUpdateBlock(block.index);
                });

                level.update();
            }
        }
        #endif


        public abstract class SensitiveToSignalBlock : MetaBlock
        {
            public abstract int[,,] connectionMap {get; set;}
            public override bool connectable {get; set;} = true;
            public virtual bool strictConnection {get; set;} = false;


            public SensitiveToSignalBlock(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public override void onPlacement()
            {
                level.updateBasesOfNeighboringWires(index);
                level.delayUpdateOfNeighbors(index);
                level.update();
            }

            public override void onDisplacement()
            {
                level.updateBasesOfNeighboringWires(index);
                level.ignoreSignalFromThisBlock = index;
                level.delayUpdateOfNeighbors(index);
                level.update();
                level.ignoreSignalFromThisBlock = 0;
            }

            public override bool mayNeedUpdate()
            {
                return true;
            }

            public virtual bool isBlockAccessibleForConnection(MetaBlock block)
            {
                ushort x1, y1, z1;
                level.level.IntToPos(this.index, out x1, out y1, out z1);
                ushort x2, y2, z2;
                level.level.IntToPos(block.index, out x2, out y2, out z2);
                ushort dx = (ushort)(x1-x2+1);
                ushort dy = (ushort)(y1-y2+1);
                ushort dz = (ushort)(z1-z2+1);

                if(connectionMap[dx,dy,dz] == 1)
                    return true;
                return false;
            }

            public virtual bool canBeConnected(MetaBlock block)
            {
                if(block.index == level.ignoreSignalFromThisBlock ||
                  !block.connectable)
                    return false;

                SensitiveToSignalBlock b = (SensitiveToSignalBlock)block;

                if(b.strictConnection || this.strictConnection) {
                    return (this.isBlockAccessibleForConnection(b) &&
                            b.isBlockAccessibleForConnection(this)    );
                }

                return (this.isBlockAccessibleForConnection(b) ||
                        b.isBlockAccessibleForConnection(this)    );
            }

            public virtual int maxNearbySignal()
            {
                int max = 0;

                for(int x = -1; x < 2; x++)
                    for(int y = -1; y < 2; y++)
                        for(int z = -1; z < 2; z++)
                        {
                            if(x == 0 && y == 0 && z == 0)
                                continue;

                            int neighbor = level.level.IntOffset(index, x,y,z);
                            MetaBlock block = level.getMetaBlock(neighbor);
                            if(!canBeConnected(block))
                                continue;

                            ushort signal = block.getSignal();
                            if(signal > max)
                                max = signal;
                            if(max == 16)
                                return 16;
                        }

                return max;
            }

            public virtual void resetNeighboringBlocks()
            {
                level.updateBasesOfNeighboringWires(index);
                level.ignoreSignalFromThisBlock = index;
                level.setNeighborWiresToZero(index);
                level.delayUpdateOfNeighbors(index);
                level.update();
                level.ignoreSignalFromThisBlock = 0;
            }
        }


        public abstract class SignalConsumerBlock : SensitiveToSignalBlock
        {
            public override int[,,] connectionMap {get; set;} =
            {{{0,0,0}, {0,1,0}, {0,0,0}},
             {{0,1,0}, {1,0,1}, {0,1,0}},
             {{0,0,0}, {0,1,0}, {0,0,0}}};


            public SignalConsumerBlock(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public override void onPlacement()
            {
                level.updateBasesOfNeighboringWires(index);
                update();
                level.update();
            }

            public override void onDisplacement()
            {
                level.ignoreSignalFromThisBlock = index;
                level.updateBasesOfNeighboringWires(index);
                level.update();
                level.ignoreSignalFromThisBlock = 0;
            }
        }
    }
}
