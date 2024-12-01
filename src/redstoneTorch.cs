using MCGalaxy;
using System;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        public class RedstoneTorch : SensitiveToSignalBlock
        {
            public new string name = "Redstone Torch";
            public override bool strictConnection {get; set;} = true;
            public override int[,,] connectionMap {get; set;} =
            {{{0,0,0}, {0,1,0}, {0,0,0}},
             {{0,0,0}, {1,0,1}, {0,1,0}},
             {{0,0,0}, {0,1,0}, {0,0,0}}};

            public static BlockID ACTIVE_ID;
            public static BlockID INACTIVE_ID;

            public override bool hasData {get;} = true;
            private int updateNum = 0;


            public RedstoneTorch(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions()
            {
                ACTIVE_ID = loadDefinition("RedstoneTorchActive.json", typeof(RedstoneTorch));
                INACTIVE_ID = loadDefinition("RedstoneTorchInactive.json", typeof(RedstoneTorch));
                baseID = ACTIVE_ID;
            }

            public override ushort getSignal()
            {
                ushort signal = (id == (ushort)(ACTIVE_ID+256)) ? (ushort)16 : (ushort)0;
                return signal;
            }

            public override void setSignal(ushort signal)
            {
                BlockID id = (signal > 0) ? ACTIVE_ID : INACTIVE_ID;
                id += (ushort)256;

                if(this.id != id) {
                    this.id = id;
                    level.setBlock(index, id);
                    level.delayUpdateOfNeighbors(index);
                }
            }

            public override void update()
            {
                for(int x = -1; x < 2; x++)
                    for(int z = -1; z < 2; z++)
                    {
                        if(!((x == 0 && z != 0) || (z == 0 && x != 0)))
                            continue;

                        int index = level.level.IntOffset(this.index, x,1,z);
                        if(index == level.ignoreSignalFromThisBlock)
                            continue;

                        MetaBlock block = level.getMetaBlock(index);
                        ushort signal = block.getSignal();
                        if(signal > 0) {
                            setSignal(0);
                            goto end;
                        }
                    }

                setSignal(16);

            end:
                updateNum++;
                if(updateNum > 16) {
                    level.setBlock(index, 0);
                    level.removeBlockInstance(index);
                }
            }

            public override void onEndingUpdateCycle()
            {
                updateNum = 0;
            }
        }
    }
}
