using MCGalaxy;
using System;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        public class RedstoneBlock : SensitiveToSignalBlock
        {
            public new string name = "Redstone Block";
            public override bool connectable {get; set;} = true;
            public override int[,,] connectionMap {get; set;} =
            {{{0,0,0}, {0,1,0}, {0,1,0}},
             {{0,1,0}, {1,0,1}, {0,1,0}},
             {{0,0,0}, {0,1,0}, {0,1,0}}};

            public RedstoneBlock(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions()
            {
                baseID = loadDefinition("RedstoneBlock.json", typeof(RedstoneBlock));
            }

            public override ushort getSignal() {
                return (ushort)16;
            }

            public override void onPlacement()
            {
                level.updateBasesOfNeighboringWires(index);
                level.delayUpdateOfNeighbors(index);
                level.update();
            }

            public override void onDisplacement()
            {
                resetNeighboringBlocks();
            }
        }
    }
}
