using MCGalaxy;
using System;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        public abstract class SignalSwitchBlock : SensitiveToSignalBlock
        {
            public static BlockID ACTIVE_ID;
            public static BlockID INACTIVE_ID;
            public static string defPath_Active;
            public static string defPath_Inactive;
            public virtual Type type {get;}

            public override int[,,] connectionMap {get; set;} =
            {{{0,0,0}, {0,1,0}, {0,0,0}},
             {{0,1,0}, {1,0,1}, {0,1,0}},
             {{0,0,0}, {0,1,0}, {0,0,0}}};


            public SignalSwitchBlock(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

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
                    level.setBlock(index, id);
                    level.delayUpdateOfNeighbors(index);
                    level.update();
                }
            }
        }

        public class Repeater : SignalSwitchBlock
        {
            public new static BlockID ACTIVE_ID;
            public new static BlockID INACTIVE_ID;
            public new static string defPath_Active = "RepeaterActive.json";
            public new static string defPath_Inactive = "RepeaterInactive.json";
            public override Type type {get {return typeof(Repeater);} }


            public Repeater(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions()
            {
                Repeater.ACTIVE_ID = loadDefinition(Repeater.defPath_Active, typeof(Repeater));
                Repeater.INACTIVE_ID = loadDefinition(Repeater.defPath_Inactive, typeof(Repeater));
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
                level.setBlock(index, id);
            }

            public override void update()
            {
                ushort newSignal = (ushort)maxNearbySignal();
                if(newSignal > 0) newSignal = 16;
                ushort signal = getSignal();

                if(signal != newSignal) {
                    setSignal(newSignal);
                    level.delayUpdateOfNeighbors(index);
                    level.update();
                }
            }

            public override void onPlacement()
            {
                level.updateBasesOfNeighboringWires(index);
                update();
                level.update();
            }
        }

        public class Switch : SignalSwitchBlock
        {
            public new static BlockID ACTIVE_ID;
            public new static BlockID INACTIVE_ID;
            public new static string defPath_Active = "SwitchActive.json";
            public new static string defPath_Inactive = "SwitchInactive.json";
            public override Type type {get {return typeof(Switch);} }


            public Switch(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions()
            {
                Switch.ACTIVE_ID = loadDefinition(Switch.defPath_Active, typeof(Switch));
                Switch.INACTIVE_ID = loadDefinition(Switch.defPath_Inactive, typeof(Switch));
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
                    level.setBlock(index, id);
                    level.delayUpdateOfNeighbors(index);
                    level.update();
                }
            }

            public override bool mayNeedUpdate() {
                return false;
            }

            public override void onClick()
            {
                ushort signal = (getSignal() > 0) ? (ushort)0 : (ushort)16;
                setSignal(signal);
            }

            public override void onDisplacement()
            {
                level.updateBasesOfNeighboringWires(index);
                level.ignoreSignalFromThisBlock = index;
                level.setNeighborWiresToZero(index);
                level.delayUpdateOfNeighbors(index);
                level.update();
                level.ignoreSignalFromThisBlock = 0;
            }
        }

        public class Plate : SignalSwitchBlock
        {
            public new static BlockID ACTIVE_ID;
            public new static BlockID INACTIVE_ID;
            public new static string defPath_Active = "PlateActive.json";
            public new static string defPath_Inactive = "PlateInactive.json";
            public override Type type {get {return typeof(Plate);} }


            public Plate(int block, CustomLevel level, BlockID id) : base(block, level, id) {}

            public new static void addDefinitions()
            {
                ACTIVE_ID = loadDefinition(defPath_Active, typeof(Plate));
                INACTIVE_ID = loadDefinition(defPath_Inactive, typeof(Plate));
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
                    level.setBlock(index, id);
                    level.delayUpdateOfNeighbors(index);
                    level.update();
                }
            }

            public override bool mayNeedUpdate() {
                return false;
            }

            public override void update() {}

            public override void onStep()
            {
                setSignal(16);
            }

            public override void onUnstep()
            {
                setSignal(0);
            }
        }
    }
}
