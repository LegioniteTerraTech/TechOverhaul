using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using Nuterra.BlockInjector;

namespace TweakTech
{
    /// <summary>
    /// Externally Unreferencable class for a reason:
    /// - This is only used to decode and is incomplete without most major functions found
    ///     within TAC AI's complete RawTech loader
    ///     <para> Reuse this if you absolutely need a standalone. </para>
    /// </summary>
    internal static class RawTechLoaderLite
    {
        // INIT
        internal static bool PreppedOnce = false;
        internal static void PrepBeforeExporting()
        {
            Debug.Log("LightRawTechLoader: Init");
            ConstructErrorBlocksList(); 
            PrepareModdedBlocksSearch();
            PreppedOnce = true;
        }
        internal static void PackUp()
        {
            Debug.Log("LightRawTechLoader: De-Init");
            PreppedOnce = false;
            errorNames.Clear();
            ModdedBlocksGrabbed = null;
        }
        internal static TechData RawTechToTechData(string name, string blueprint, out int[] blockIDs)
        {
            if (!PreppedOnce)
            {
                PrepBeforeExporting();
            }
            TechData data = new TechData();
            data.Name = name;
            data.m_Bounds = new IntVector3(new Vector3(18, 18, 18));
            data.m_SkinMapping = new Dictionary<uint, string>();
            data.m_TechSaveState = new Dictionary<int, TechComponent.SerialData>();
            data.m_CreationData = new TechData.CreationData();
            data.m_BlockSpecs = new List<TankPreset.BlockSpec>();
            List<BlockMemory> mems = JSONToMemory(blueprint);
            List<int> BTs = new List<int>();

            bool skinChaotic = UnityEngine.Random.Range(0, 100) < 2;
            byte skinset = (byte)UnityEngine.Random.Range(0, 2);
            byte skinset2 = (byte)UnityEngine.Random.Range(0, 1);
            foreach (BlockMemory mem in mems)
            {
                BlockTypes type = StringToBlockType(mem.t);
                Debug.Log("TACtical_AI: InstantTech - " + mem.t + " | " + type.ToString());
                if (!Singleton.Manager<ManSpawn>.inst.IsBlockAllowedInCurrentGameMode(type) || !TechDataAvailValidation.IsBlockAvailableInMode(type))
                {
                    Debug.Log("TACtical_AI: InstantTech - Removed " + mem.t + " as it was invalidated");
                    continue;
                }
                if (!BTs.Contains((int)type))
                {
                    BTs.Add((int)type);
                }

                TankPreset.BlockSpec spec = default;
                spec.block = mem.t;
                spec.m_BlockType = type;
                spec.orthoRotation = new OrthoRotation(mem.r);
                spec.position = mem.p;
                spec.saveState = new Dictionary<int, Module.SerialData>();
                spec.textSerialData = new List<string>();
                FactionTypesExt factType = GetCorpExtended(type);
                if (skinChaotic)
                {
                    byte rand = (byte)UnityEngine.Random.Range(0, 2);
                    if (factType == FactionTypesExt.GSO && rand != 0)
                        rand += 3;
                    if (!ManDLC.inst.IsSkinDLC(rand, CorpExtToCorp(factType)))
                        spec.m_SkinID = rand;
                    else
                        spec.m_SkinID = 0;
                }
                else
                {
                    if (factType == FactionTypesExt.GSO)
                    {
                        if (!ManDLC.inst.IsSkinDLC(skinset + (skinset != 0 ? 3 : 0), CorpExtToCorp(factType)))
                            spec.m_SkinID = (byte)(skinset + (skinset != 0 ? 3 : 0));
                        else if (!ManDLC.inst.IsSkinDLC(skinset2 + (skinset2 != 0 ? 3 : 0), CorpExtToCorp(factType)))
                            spec.m_SkinID = (byte)(skinset2 + (skinset2 != 0 ? 3 : 0));
                        else
                            spec.m_SkinID = 0;
                    }
                    else
                    {
                        if (!ManDLC.inst.IsSkinDLC(skinset, CorpExtToCorp(factType)))
                            spec.m_SkinID = skinset;
                        else if (!ManDLC.inst.IsSkinDLC(skinset2, CorpExtToCorp(factType)))
                            spec.m_SkinID = skinset2;
                        else
                            spec.m_SkinID = 0;
                    }
                }

                data.m_BlockSpecs.Add(spec);
            }
            //Debug.Log("TACtical_AI: ExportRawTechToTechData - Exported " + name);

            blockIDs = BTs.ToArray();
            return data;
        }
        internal static List<BlockTypes> GetAllBlockTypesUsed(string blueprint)
        {
            if (!PreppedOnce)
            {
                PrepBeforeExporting();
            }
            List<BlockMemory> mems = JSONToMemory(blueprint);
            mems = mems.Distinct().ToList();
            List<BlockTypes> BTs = new List<BlockTypes>();
            foreach (BlockMemory mem in mems)
            {
                if (StringToBIBlockType(mem.t, out BlockTypes type))
                    BTs.Add(type);
            }
            return BTs;
        }


        // Handled within
        private static FieldInfo allModdedBlocks = typeof(ManMods).GetField("m_BlockIDReverseLookup", BindingFlags.NonPublic | BindingFlags.Instance);
        private static Dictionary<int, BlockTypes> errorNames = new Dictionary<int, BlockTypes>(); 
        private static Dictionary<string, int> ModdedBlocksGrabbed;

        // Get those blocks right!
        private static void ConstructErrorBlocksList()
        {
            errorNames.Clear();
            List<BlockTypes> types = Singleton.Manager<ManSpawn>.inst.GetLoadedTankBlockNames().ToList();
            foreach (BlockTypes type in types)
            {
                TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(type);
                string name = prefab.name;
                if (prefab.GetComponent<Damageable>() && type.ToString() != name) //&& !Singleton.Manager<ManMods>.inst.IsModdedBlock(type))
                {
                    int hash = name.GetHashCode();
                    if (!errorNames.Keys.Contains(hash))
                    {
                        errorNames.Add(hash, type);
#if DEBUG
                        if ((int)type > 5000)
                            Debug.Log("TACtical_AI: ConstructErrorBlocksList - Added Modded Block " + name + " | " + type.ToString());
#endif
                    }
                }
            }
            Debug.Log("TACtical_AI: ConstructErrorBlocksList - There are " + errorNames.Count + " blocks with names not equal to their type");
        }
        private static bool TryGetMismatchNames(string name, ref BlockTypes type)
        {
            if (errorNames.TryGetValue(name.GetHashCode(), out BlockTypes val))
            {
                type = val;
                return true;
            }
            return false;
        }
        
        
        private static void PrepareModdedBlocksSearch()
        {
            ModdedBlocksGrabbed = (Dictionary<string, int>)allModdedBlocks.GetValue(Singleton.Manager<ManMods>.inst);
        }
        private static BlockTypes GetBlockIDLogFree(string name)
        {
            int blockType = 3;
            ModdedBlocksGrabbed.TryGetValue(name, out blockType);
            return (BlockTypes)blockType;
        }



        private static BlockTypes StringToBlockType(string mem)
        {
            if (!Enum.TryParse(mem, out BlockTypes type))
            {
                if (!TryGetMismatchNames(mem, ref type))
                {
                    if (StringToBIBlockType(mem, out BlockTypes BTC))
                    {
                        return BTC;
                    }
                    type = GetBlockIDLogFree(mem);
                }
            }
            return type;
        }
        private static bool StringToBIBlockType(string mem, out BlockTypes BT) // BLOCK INJECTOR
        {
            BT = BlockTypes.GSOAIController_111;
            if (!KickStart.isBlockInjectorPresent)
                return false;
            int hashName = mem.GetHashCode();
            foreach (KeyValuePair<int, CustomBlock> pair in BlockLoader.CustomBlocks)
            {
                CustomBlock CB = pair.Value;
                if (CB != null)
                {
                    if (CB.Name.GetHashCode() == hashName)
                    {
                        //Debug.Log("TACtical_AI: StringToBIBlockType - Found Match in BlockInjector for " + mem);
                        BT = (BlockTypes)pair.Key;
                        return true;
                    }
                }
            }
            return false;
        }

        private static List<BlockMemory> JSONToMemory(string toLoad)
        {   // Loading a Tech from the BlockMemory
            StringBuilder RAW = new StringBuilder();
            foreach (char ch in toLoad)
            {
                if (ch != '\\')
                {
                    RAW.Append(ch);
                }
            }
            List<BlockMemory> mem = new List<BlockMemory>();
            StringBuilder blockCase = new StringBuilder();
            string RAWout = RAW.ToString();
            foreach (char ch in RAWout)
            {
                if (ch == '|')//new block
                {
                    mem.Add(JsonUtility.FromJson<BlockMemory>(blockCase.ToString()));
                    blockCase.Clear();
                }
                else
                    blockCase.Append(ch);
            }
            mem.Add(JsonUtility.FromJson<BlockMemory>(blockCase.ToString()));
            //Debug.Log("TACtical_AI:  DesignMemory: saved " + mem.Count);
            return mem;
        }
        private static FactionTypesExt GetCorpExtended(BlockTypes type)
        {
            return (FactionTypesExt)Singleton.Manager<ManSpawn>.inst.GetCorporation(type);
        }
        private static FactionSubTypes CorpExtToCorp(FactionTypesExt corpExt)
        {
            switch (corpExt)
            {
                case FactionTypesExt.GT:
                case FactionTypesExt.IEC:
                    return FactionSubTypes.GSO;
                case FactionTypesExt.EFF:
                case FactionTypesExt.LK:
                    return FactionSubTypes.GC;
                case FactionTypesExt.OS:
                    return FactionSubTypes.VEN;
                case FactionTypesExt.BL:
                case FactionTypesExt.TAC:
                    return FactionSubTypes.HE;
                case FactionTypesExt.DL:
                case FactionTypesExt.EYM:
                case FactionTypesExt.HS:
                    return FactionSubTypes.BF;
            }
            return (FactionSubTypes)corpExt;
        }

        [Serializable]
        private class BlockMemory
        {   // Save the blocks!
            public string t = BlockTypes.GSOAIController_111.ToString(); //blocktype
            public Vector3 p = Vector3.zero;
            public OrthoRotation.r r = OrthoRotation.r.u000;
        }

        private enum FactionTypesExt
        {
            // No I do not make any of the corps below (exclusing some of TAC and EFF) 
            //  - but these are needed to allow the AI to spawn the right bases with 
            //    the right block ranges
            // OFFICIAL
            NULL,   // not a corp, really, probably the most unique of all lol
            GSO,    // Galactic Survey Organization
            GC,     // GeoCorp
            EXP,    // Reticule Research
            VEN,    // VENture
            HE,     // HawkEye
            SPE,    // Special
            BF,     // Better Future
            SJ,     // Space Junkers
            LEG,    // LEGION!!1!

            // Community
            AER,    // Aerion
            BL,     // Black Labs (EXT OF HE)
            CC,     // CrystalCorp
            DC,     // DEVCorp
            DL,     // DarkLight
            EYM,    // Ellydium
            GT,     // GreenTech
            HS,     // Hyperion Systems
            IEC,    // Independant Earthern Colonies
            LK,     // Lemon Kingdom
            OS,     // Old Stars
            TC,     // Tofuu Corp
            TAC,    // Technocratic AI Colony

            // idk
            EFF,    // Emperical Forge Fabrication
            MCC,    // Mechaniccoid Cooperative Confederacy 
            BLN,    // BuLwark Nation (Bulin)
            CNC,    // ClaNg Clads (ChanClas)
            LOL,    // Larry's Overlord Laser
        }
    }
}
