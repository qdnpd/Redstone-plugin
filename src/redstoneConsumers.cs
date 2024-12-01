using System;
using MCGalaxy;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        public class RedstoneLamp : SignalConsumerBlock
        {
            public new string name = "Redstone Lamp";

            public static BlockID ACTIVE_ID;
            public static BlockID INACTIVE_ID;


            public RedstoneLamp(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions()
            {
                ACTIVE_ID = loadDefinition("RedstoneLampActive.json", typeof(RedstoneLamp));
                INACTIVE_ID = loadDefinition("RedstoneLampInactive.json", typeof(RedstoneLamp));
                baseID = ACTIVE_ID;
            }

            public override void update()
            {
                BlockID newID = (maxNearbySignal() > 0) ? ACTIVE_ID : INACTIVE_ID;
                newID += (ushort)256;
                if(id != newID)
                    level.setBlock(index, newID);
            }
        }
    }
}
