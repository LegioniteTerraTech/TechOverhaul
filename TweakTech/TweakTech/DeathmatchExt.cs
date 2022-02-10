using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace TweakTech
{
    public class DeathmatchExt
    {

        public static FieldInfo techData = typeof(TankPreset)
                   .GetField("m_TechData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        public static FieldInfo assetName = typeof(MultiplayerTechSelectPresetAsset)
                   .GetField("m_TankName", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        public static int page = 0;

        public static List<MultiplayerTechSelectPresetAsset> page1;
        public static List<MultiplayerTechSelectPresetAsset> page2;
        public static List<List<MultiplayerTechSelectPresetAsset>> MorePages;

        public static MultiplayerTechSelectPresetAsset cTech1;
        public static MultiplayerTechSelectPresetAsset cTech2;
        public static MultiplayerTechSelectPresetAsset cTech3;
        public static MultiplayerTechSelectPresetAsset cTech4;

        public static bool Ready = false;
        private static bool AllowedSetModdedBlocks = false;
        public static void OnModeSwitch(Mode mode)
        {
            if (mode is ModeDeathmatch MD && !AllowedSetModdedBlocks)
            {
                Debug.Log("Error on load DeathmatchExt 1111");
            }
        }
        public static void SetReady()
        {
            try
            {
                ManGameMode.inst.ModeStartEvent.Subscribe(OnModeSwitch);
                UIScreenNetworkLobby UISNL = (UIScreenNetworkLobby)ManUI.inst.GetScreen(ManUI.ScreenType.MatchmakingLobbyScreen);

                if (page2 == null)
                {
                    Debug.Log("DeathmatchExt - Adding in techs...");
                    RawTechLoaderLite.PrepBeforeExporting();
                    cTech1 = MakeNewDeathmatchTech("Cab Spam", null);
                    cTech2 = MakeNewDeathmatchTech("Cab Spam", null);
                    cTech3 = MakeNewDeathmatchTech("Scorcher", DeathmatchPrefabs.TACAssault);
                    cTech4 = MakeNewDeathmatchTech("Cab Spam", null);
                    page2 = new List<MultiplayerTechSelectPresetAsset>();
                    page2.Add(cTech1);
                    page2.Add(cTech2);
                    page2.Add(cTech3);
                    page2.Add(cTech4);
                    page2.Add(MakeNewDeathmatchTech("Cab Spam", null));
                    RawTechLoaderLite.PackUp();
                }
            }
            catch (Exception e)
            { Debug.Log("Error on load DeathmatchExt 2: " + e); }
            Ready = true;
        }
        public static List<MultiplayerTechSelectPresetAsset> MakeNewDeathmatchTechs(List<MultiplayerTechSelectPresetAsset> hostInst)
        {
            try
            {
                if (page1 == null)
                {
                    if (hostInst != null)
                    {
                        page1 = hostInst;
                    }
                }
            }
            catch (Exception e)
            { Debug.Log("Error on load DeathmatchExt 1: " + e); }
            if (!Ready)
                return page1;
            return page2;
        }
        
        internal static MultiplayerTechSelectPresetAsset MakeNewDeathmatchTech(string Name, string RTBlueprint)
        {
            TechData TD = null;
            if (RTBlueprint != null)
                TD = RawTechLoaderLite.RawTechToTechData(Name, RTBlueprint, out _);

            if (TD == null)
            {
                TD = new TechData();
                TD.Name = Name;
                TD.m_BlockSpecs = new List<TankPreset.BlockSpec>();
                TD.m_TechSaveState = new Dictionary<int, TechComponent.SerialData>();
                TD.m_CreationData = new TechData.CreationData();
                TD.m_SkinMapping = new Dictionary<uint, string>();

                Debug.Log("DeathmatchExt: No tech blueprint was set!");
                BlockTypes BT = BlockTypes.GSOCockpit_111;
                TD.m_BlockSpecs.Add(
                        new TankPreset.BlockSpec
                        {
                            m_BlockType = BT,
                            m_SkinID = 0,
                            m_VisibleID = 0,
                            block = "GSO_Cab",
                            position = IntVector3.zero,
                            orthoRotation = new OrthoRotation(Quaternion.LookRotation(Vector3.forward)),
                            saveState = new Dictionary<int, Module.SerialData>(),
                            textSerialData = new List<string>(),
                        }
                    );
            }

            TankPreset TP = TankPreset.CreateInstance();
            techData.SetValue(TP, TD); // Creates the base instance

            BlockCount[] BC1 = new BlockCount[3]
            {
                new BlockCount(BlockTypes.GSO_Cab_211, 1),
                new BlockCount(BlockTypes.HE_AITurret_112, 1),
                new BlockCount(BlockTypes.HE_AIModule_Guard_112, 6)
            };
            BlockCount[] BC2 = new BlockCount[2]
            {
                new BlockCount(BlockTypes.GSOCockpit_111, 4),
                new BlockCount(BlockTypes.HE_AIModule_Guard_112, 6)
            };
            BlockCount[] BC3 = new BlockCount[5]
            {
                new BlockCount(BlockTypes.GSOCockpit_111, 1),
                new BlockCount(BlockTypes.GSOCockpit_111, 1),
                new BlockCount(BlockTypes.GSOCockpit_111, 1),
                new BlockCount(BlockTypes.GSOCockpit_111, 1),
                new BlockCount(BlockTypes.GSOCockpit_111, 1)
            };

            MultiplayerTechSelectPresetAsset MTSPA = ScriptableObject.CreateInstance<MultiplayerTechSelectPresetAsset>();
            MTSPA.m_TankName = new LocalisedStringOverride() { m_Bank = Name };
            MTSPA.m_TankPreset = TP;
            MTSPA.m_DeathStreakRewards = MakeDeathStreakRewards(BC3);

            MTSPA.m_KillStreakRewards = MakeKillStreakRewards(BC3);

            MTSPA.m_InventoryBlockList1 = MakeBlockList(BC1);
            MTSPA.m_InventoryBlockList2 = MakeBlockList(BC2);

            return MTSPA;
        }
        internal static void UpdateDisp()
        {
            UIScreenMultiplayerTechSelect UISMTS = (UIScreenMultiplayerTechSelect)ManUI.inst.GetScreen(ManUI.ScreenType.MultiplayerTechSelect);
            UISMTS.ApplySelection();
        }
        internal static InventoryBlockList MakeBlockList(BlockCount[] BC)
        {
            InventoryBlockList IBL = new InventoryBlockList();
            IBL.m_BlockList = BC;
            return IBL;
        }

        internal static MultiplayerDeathStreakRewards MakeDeathStreakRewards(BlockCount[] BC)
        {
            MultiplayerDeathStreakRewards MDSRS = ScriptableObject.CreateInstance<MultiplayerDeathStreakRewards>();
            MultiplayerDeathStreakReward[] MDSR = new MultiplayerDeathStreakReward[5]
            {
                new MultiplayerDeathStreakReward{
                    m_Rewards = new BlockCount[1]{
                        BC[0]
                    }
                },
                new MultiplayerDeathStreakReward{
                    m_Rewards = new BlockCount[1]{
                        BC[1]
                    }
                },
                new MultiplayerDeathStreakReward{
                    m_Rewards = new BlockCount[1]{
                        BC[2]
                    }
                },
                new MultiplayerDeathStreakReward{
                    m_Rewards = new BlockCount[1]{
                        BC[3]
                    }
                },
                new MultiplayerDeathStreakReward{
                    m_Rewards = new BlockCount[1]{
                        BC[4]
                    }
                },
            };

            MDSRS.m_RewardLevels = MDSR;
            return MDSRS;
        }
        internal static MultiplayerKillStreakRewardAsset MakeKillStreakRewards(BlockCount[] BC)
        {
            MultiplayerKillStreakRewardAsset MKSRA = ScriptableObject.CreateInstance<MultiplayerKillStreakRewardAsset>();
            MultiplayerKillStreakRewardLevel[] MDSR = new MultiplayerKillStreakRewardLevel[5]
            {
                new MultiplayerKillStreakRewardLevel{
                    m_KillsRequired = 1,
                    m_BlockReward = BC[0]
                },
                new MultiplayerKillStreakRewardLevel{
                    m_KillsRequired = 2,
                    m_BlockReward = BC[1]
                },
                new MultiplayerKillStreakRewardLevel{
                    m_KillsRequired = 3,
                    m_BlockReward = BC[2]
                },
                new MultiplayerKillStreakRewardLevel{
                    m_KillsRequired = 4,
                    m_BlockReward = BC[3]
                },
                new MultiplayerKillStreakRewardLevel{
                    m_KillsRequired = 5,
                    m_BlockReward = BC[4]
                },
            };

            MKSRA.m_RewardLevels = MDSR;
            return MKSRA;
        }
    }
    internal class LocalisedStringOverride : LocalisedString
    {
        public override string ToString()
        {
            return m_Bank;
        }
    }
}
