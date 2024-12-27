using System;
using System.IO;
using System.Web.UI;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using MCGalaxy.Blocks;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.LevelEvents;
using BlockID = System.UInt16;


namespace MCGalaxy
{
    public partial class Redstone : Plugin
    {
        public override string name { get { return "redstone"; } }
        public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
        public override int build { get { return 4; } }
        public override string welcome { get { return ""; } }
        public override string creator { get { return ""; } }
        public override bool LoadAtStartup { get { return true; } }

        private static BlockID definitionGlobalID;
        Dictionary<Level, CustomLevel> levels = new Dictionary<Level, CustomLevel>();
        public static Dictionary<ushort, Type> metaBlocksTypes = new Dictionary<ushort, Type>();

        public static DefaultBlock defaultInstance = new DefaultBlock(0,null,0);


        public struct Config {
            public BlockID START_ID;
            public int TEXTURES_PER_WIRE_DEFS;
            public ushort REDSTONE_BASE_WIRE_TEXTURE;
        }
        public static Config config = new Config();


        public void loadConfiguration()
        {
            string content = File.ReadAllText("plugins/Redstone/config.json");
            var serializer = new JavaScriptSerializer();
            config = serializer.Deserialize<Config>(content);
        }

        public override void Load(bool startup)
        {
            loadConfiguration();
            definitionGlobalID = config.START_ID;

            OnBlockChangingEvent.Register(OnBlockChanging, Priority.Low);
            OnBlockChangedEvent.Register(OnBlockChanged, Priority.Low);
            OnLevelLoadedEvent.Register(OnLevelLoaded, Priority.High);
            //OnPlayerMoveEvent.Register(OnPlayerMove, Priority.High);
            OnPlayerClickEvent.Register(OnPlayerClick, Priority.Low);

            RedstoneWire.addDefinitions();
            RedstoneBlock.addDefinitions();
            RedstoneTorch.addDefinitions();
            RedstoneLamp.addDefinitions();
            Switch.addDefinitions();
            Repeater.addDefinitions();

            #if SURVIVAL
            configureSurvival();
            #endif

            levels[Server.mainLevel] = new CustomLevel(Server.mainLevel);
        }

        public override void Unload(bool shutdown)
        {
            OnBlockChangingEvent.Unregister(OnBlockChanging);
            OnBlockChangedEvent.Unregister(OnBlockChanged);
            OnLevelLoadedEvent.Unregister(OnLevelLoaded);
            //OnPlayerMoveEvent.Unregister(OnPlayerMove);
            OnPlayerClickEvent.Unregister(OnPlayerClick);
        }

        public void OnBlockChanging(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
            log(Logtype.DEBUG, "event trigerred: OnBlockChanging");
            cancel = false;
            CustomLevel level = levels[p.level];

            if(RedstoneWire.isRedstone(level.level.FastGetBlock(x, (ushort)(y-1), z)) &&
               RedstoneWire.isRedstone(block) && placing) {
                p.RevertBlock(x,y,z);
                cancel = true;
                return;
            }

            if(placing) {
            #if SURVIVAL
                BlockID id = block > 60 ? (BlockID)(block - 256) : block;
                if(sprites.Contains(id))
                    cancel = true;
                else
                    cancel = false;
            #else
                cancel = false;
            #endif
            }

            else {
                int index = p.level.PosToInt(x,y,z);
                MetaBlock instance = level.getMetaBlock(index);
                instance.onDisplacement();

                if(instance.hasData)
                    level.removeBlockInstance(instance.index);
            }
        }

        public void OnBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult result)
        {
            log(Logtype.DEBUG, "event trigerred: OnBlockChanged");
            if(p == Player.Console) {
                return;
            }
            CustomLevel level = levels[p.level];
            int index = p.level.PosToInt(x,y,z);
            MetaBlock instance = level.getMetaBlock(index);
            log(Logtype.DEBUG, $"block placed: {level.blockInfo(index)}");
            instance.onPlacement();
        }

        public void OnLevelLoaded(Level lvl)
        {
            levels[lvl] = new CustomLevel(lvl);
        }

        public void OnLevelUnload(Level lvl, ref bool cancel)
        {
            levels[lvl] = null;
        }

        public void OnPlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
        {
            CustomLevel level = levels[p.level];
            int index = p.level.PosToInt((ushort)next.X,(ushort)next.Y,(ushort)next.Z);
            MetaBlock block1 = level.getMetaBlock(index);
            block1.onStep();

            //MetaBlock block2 = level.getMetaBlock(p.oldIndex);
            //block2.onUnstep();

            cancel = false;
        }

        public void OnPlayerClick(Player p, MouseButton button, MouseAction action,
                                           ushort yaw, ushort pitch, byte entity,
                                           ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            log(Logtype.DEBUG, "event trigerred: OnPlayerClick");
            if(action == MouseAction.Pressed && button == MouseButton.Right)
            {
                if(p.level.IsAirAt(x,y,z))
                    return;

                CustomLevel level = levels[p.level];
                int index = p.level.PosToInt(x,y,z);
                MetaBlock block = level.getMetaBlock(index);
                block.onClick();
            }
        }

        public static void addDefinition(BlockID RawID, BlockDefinition def, Type customdef)
        {
            def.RawID = RawID;
            BlockDefinition.Add(def, BlockDefinition.GlobalDefs, null);

            if(RawID >= 80)
                RawID += 256;

            metaBlocksTypes[RawID] = customdef;
        }

        public static BlockID getDefinitionID() {
            return definitionGlobalID++;
        }

        public static void assignNonBlankTexture(BlockDefinition def, ushort texture, ushort blankTexture)
        {
            if(def.TopTex != blankTexture)
                def.TopTex = texture;
            if(def.BottomTex != blankTexture)
                def.BottomTex = texture;
            if(def.LeftTex != blankTexture)
                def.LeftTex = texture;
            if(def.RightTex != blankTexture)
                def.RightTex = texture;
            if(def.FrontTex != blankTexture)
                def.FrontTex = texture;
            if(def.BackTex != blankTexture)
                def.BackTex = texture;
        }

        private enum Logtype {
            LOG, DEBUG, ERROR
        }

        private static string[] logtype_names = {"log", "debug", "error"};

        private static void log(Logtype type, string message)
        {
            #if !DEBUG
            if(type == Logtype.DEBUG)
                return;
            #endif

            string type_name = logtype_names[(int)type];
            Console.WriteLine($"[{type_name}] {message}");
        }
    }
}
