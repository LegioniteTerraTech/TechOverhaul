using System;
using Nuterra.NativeOptions;
using ModHelper.Config;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace TweakTech
{
    // This mod ultimately relies off of RandomAdditions to operate!
    //   WeaponAimMod is also advised for better tracking as this mod has simple target aim prediction.
    /// <summary>
    /// Please reach out to LegioniteTerraTech if you would like to request changes.
    /// </summary>
    public class KickStart
    {
        //Let hooks happen i guess
        const string ModName = "TweakTech";

#if STEAM
        public static bool EnableThis = false;
#else
        public static bool EnableThis = true;
#endif

        internal static bool RandomAdditionsAvail = false;
        internal static bool WeaponAimModAvail = false;
        internal static bool TACAIModAvail = false;
        internal static bool FusionBlockAvail = false;
        internal static bool isBlockInjectorPresent = false;

        internal static bool hasPatched = false;

        internal static Harmony harmonyInstance = new Harmony("legioniteterratech.tweaktech");

#if STEAM
        public static void Enable()
        {
            RandomAdditionsAvail = LookForMod("RandomAdditions");
            WeaponAimModAvail = LookForMod("WeaponAimMod");
            TACAIModAvail = LookForMod("TAC_AI");
            FusionBlockAvail = LookForMod("Fusion Block");
            isBlockInjectorPresent = LookForMod("BlockInjector");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            Debug.Log("TweakTech: Init");
            EnableThis = true;

        }
        public static void Disable()
        {
            harmonyInstance.UnpatchAll();

            Debug.Log("TweakTech: DeInit");
            EnableThis = false;

        }
#else
        public static OptionToggle enabledMod;
        //public static OptionRange multiHP;
        public static OptionToggle blockHP150;
        public static OptionToggle maxRestrict;
        public static OptionRange reduceProjectiles;
        public static OptionRange reduceProjectilesThreshold;

        public static void Main()
        {
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());


            RandomAdditionsAvail = LookForMod("RandomAdditions");
            WeaponAimModAvail = LookForMod("WeaponAimMod");
            TACAIModAvail = LookForMod("TAC_AI");
            FusionBlockAvail = LookForMod("Fusion Block");
            isBlockInjectorPresent = LookForMod("BlockInjector");
            Debug.Log("TweakTech: Kickstarted");

            ModConfig thisModConfig = new ModConfig();
            thisModConfig.BindConfig<KickStart>(null, "EnableThis");
            thisModConfig.BindConfig<ChangePatcher>(null, "GlobalHealthMulti");
            thisModConfig.BindConfig<ChangePatcher>(null, "MaximumFireRateAdjust");
            thisModConfig.BindConfig<ChangePatcher>(null, "ProjectileReduction");
            thisModConfig.BindConfig<ChangePatcher>(null, "FirerateReductionMin");

            string Tweakables = ModName;
            /*
            enabledMod = new OptionToggle("Enable TweakTech", Tweakables, EnableThis);
            enabledMod.onValueSaved.AddListener(() => {
                EnableThis = enabledMod.SavedValue;
                
                if (EnableThis)
                    FDBookmark.EnableAll();
                else
                    FDBookmark.DisableAll();
            });*/
            maxRestrict = new OptionToggle("Lock to Max Restriction", Tweakables, ChangePatcher.MaximumFireRateAdjust);
            maxRestrict.onValueSaved.AddListener(() => {
                ChangePatcher.MaximumFireRateAdjust = maxRestrict.SavedValue;
                
                if (EnableThis)
                    FDBookmark.EnableAll();
            });
            reduceProjectilesThreshold = new OptionRange("Maximum Firerate to Reduce", Tweakables, ChangePatcher.FirerateReductionMin, 0, 6);
            reduceProjectilesThreshold.onValueSaved.AddListener(() => {
                ChangePatcher.FirerateReductionMin = reduceProjectilesThreshold.SavedValue;
                
                if (EnableThis)
                    FDBookmark.EnableAll();
            });
            reduceProjectiles = new OptionRange("Above Maximum Cooldown Multiplier", Tweakables, ChangePatcher.ProjectileReduction, 1, 8);
            reduceProjectiles.onValueSaved.AddListener(() => { ChangePatcher.ProjectileReduction = reduceProjectiles.SavedValue; 
                
                if (EnableThis)
                    FDBookmark.EnableAll();
            });
            /*
            blockHP150 = new OptionToggle("(SP Only) Disable 1.5 Health Multiplier", Tweakables, !ChangePatcher.UseGlobalHealthMulti);
            blockHP150.onValueSaved.AddListener(() => {
                ChangePatcher.UseGlobalHealthMulti = !blockHP150.SavedValue;
                
                if (EnableThis)
                     FDBookmark.EnableAll();
            });
            
            multiHP = new OptionRange("Block Health Muliplier [0.5 - 2.5] \n- Requires World Restart", Tweakables, ChangePatcher.GlobalHealthMulti, 0.5f, 2.5f, 0.5f);
            multiHP.onValueSaved.AddListener(() => {
                ChangePatcher.GlobalHealthMulti = multiHP.SavedValue;
                
            });*/
            NativeOptionsMod.onOptionsSaved.AddListener(() => { thisModConfig.WriteConfigJsonFile(); });
        }
#endif

        public static bool LookForMod(string name)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith(name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
