using MCGalaxy;
using System;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        public class RedstoneWire : SensitiveToSignalBlock
        {
            public new string name = "wire";
            public override bool connectable {get; set;} = true;
            public override int[,,] connectionMap {get; set;} = null;

            private static BlockID HOR_STARTID;
            private static BlockID VER_STARTID;
            private static BlockID CROSS_STARTID;


            public RedstoneWire(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions()
            {
                var def = loadDefinitionFromFile("RedstoneWire.json");
                ushort baseTexture = config.REDSTONE_BASE_WIRE_TEXTURE;

                baseID        = config.START_ID;
                HOR_STARTID   = config.START_ID;
                VER_STARTID   = (ushort)(config.START_ID+16);
                CROSS_STARTID = (ushort)(config.START_ID+32);

                for(int j = 0; j < 3; j++)
                {
                    for(int i = 0; i < 16; i++)
                    {
                        if(def == null)
                            break;
                        BlockDefinition final = def.Copy();

                        int offset;
                        if(i == 0)
                            offset = 0;
                        else
                            offset = (int)Math.Floor((double)(i * (config.TEXTURES_PER_WIRE_DEFS-1) / 16)) + 1;

                        assignNonBlankTexture(final, (ushort)(baseTexture + offset), 255);
                        addDefinition(getDefinitionID(), final, typeof(RedstoneWire));
                    }

                    baseTexture += (ushort)config.TEXTURES_PER_WIRE_DEFS;
                }
            }

            public override ushort getSignal()
            {
                return (ushort)(id & 0x000f);
            }

            public override void setSignal(ushort signal)
            {
                ushort newID = (ushort)((id & 0xfff0) | signal);
                level.setBlock(index, newID);
            }

            public BlockID getBase()
            {
                return (BlockID)(id & 0xfff0);
            }

            public override bool mayNeedUpdate()
            {
                return true;
            }

            public override void update()
            {
                int maxSignal = maxNearbySignal() - 1;
                if(maxSignal < 0)
                    maxSignal = 0;
                ushort newSignal = (ushort)(maxSignal);

                if(getSignal() != newSignal) {
                    setSignal(newSignal);
                    level.delayUpdateOfNeighbors(index);
                }
            }

            public override void onPlacement()
            {
                updateBase();
                level.updateBasesOfNeighboringWires(index);
                update();
                level.update();
            }

            public override void onDisplacement()
            {
                level.ignoreSignalFromThisBlock = index;
                level.updateBasesOfNeighboringWires(index);
                level.delayUpdateOfNeighbors(index);
                level.update();
                level.ignoreSignalFromThisBlock = 0;
            }

            public void updateBase()
            {
                BlockID newBase;
                bool[,] map = neighborsBitmap();

                if((map[1,0] || map[1,2]) && !(map[0,1] || map[2,1]))
                    newBase = VER_STARTID;
                else if((map[0,1] || map[2,1]) && !(map[1,0] || map[1,2]))
                    newBase = HOR_STARTID;
                else
                    newBase = CROSS_STARTID;

                newBase += (ushort)256;
                if(newBase != getBase()) {
                    id = (BlockID)(newBase | getSignal());
                    level.setBlock(index, id);
                }
            }

            private bool[,] neighborsBitmap()
            {
                bool[,] map = new bool[3,3];

                for(int x = -1; x < 2; x++)
                    for(int y = -1; y < 2; y++)
                        for(int z = -1; z < 2; z++)
                        {
                            if(x == 0 && y == 0 && z == 0)
                                continue;

                            int nindex = level.level.IntOffset(index, x,y,z);
                            if(nindex == level.ignoreSignalFromThisBlock)
                                continue;

                            MetaBlock neighbor = (MetaBlock)level.getMetaBlock(nindex);
                            if(canBeConnected(neighbor))
                                map[x+1,z+1] = true;
                        }

                return map;
            }

            public override bool isBlockAccessibleForConnection(MetaBlock block)
            {
                ushort x1, y1, z1;
                level.level.IntToPos(this.index, out x1, out y1, out z1);
                ushort x2, y2, z2;
                level.level.IntToPos(block.index, out x2, out y2, out z2);

                if(x1 == x2 && z1 == z2)
                    return false;

                int dx = x1-x2;
                int dy = y1-y2;
                int dz = z1-z2;
                if(!((dx == 0 && dz != 0) || (dz == 0 && dx != 0)))
                    return false;

                if(y2 != y1) {
                    BlockID blockThere = level.level.FastGetBlock(x2, y1, z2);
                    if(blockThere != 0)
                        return false;
                }

                return true;
            }

            public static bool isRedstone(BlockID id) {
                id -= 256;
                return ((id >= HOR_STARTID && id < CROSS_STARTID + 16));
            }
        }
    }
}
