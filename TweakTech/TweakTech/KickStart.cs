using System;
using Nuterra.NativeOptions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

#if !STEAM
using ModHelper.Config;
#else
using ModHelper;
#endif


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

#if STEAM
        public static bool EnableThis = false;
#else
        public static bool EnableThis = true;
#endif

        internal static bool RandomAdditionsAvail = false;
        internal static bool ActiveDefensesAvail = false;
        internal static bool WeaponAimModAvail = false;
        internal static bool TACAIModAvail = false;
        internal static bool FusionBlockAvail = false;
        internal static bool isBlockInjectorPresent = false;

        internal static bool hasPatched = false;

        internal static Harmony harmonyInstance = new Harmony("legioniteterratech.tweaktech");

#if STEAM
        public static void Enable()
        {
            EnableThis = true;
            RandomAdditionsAvail = LookForMod("RandomAdditions");
            WeaponAimModAvail = LookForMod("WeaponAimMod");
            TACAIModAvail = LookForMod("TAC_AI");
            FusionBlockAvail = LookForMod("Fusion Block");
            isBlockInjectorPresent = LookForMod("BlockInjector");
            if (!hasPatched)
            {
                try
                {
                    harmonyInstance.PatchAll();
                    hasPatched = true;
                }
                catch 
                { 
                    ManUI.inst.ShowErrorPopup("TweakTech: Please install Harmony (2.1.0) on the Steam Workshop to use the mod TweakTech, it is a functional dependancy.");
                    return;
                }
            }
            if (!RandomAdditionsAvail)
                ManUI.inst.ShowErrorPopup("TweakTech: Please install Random Additions on the Steam Workshop to use the mod TweakTech, it is a functional dependancy.");

            ActiveDefensesAvail = LookForMod("ActiveDefenses");

            try
            {
                //KickStartInitOptions.TryInit();
            }
            catch (Exception e) { Debug.LogError("TweakTech: Failed on Options&Config " + e); }

            Debug.Log("TweakTech: Init");

        }
        public static void Disable()
        {
            EnableThis = false;
            if (hasPatched)
            {
                harmonyInstance.UnpatchAll("legioniteterratech.tweaktech");
                hasPatched = false;
            }
            

            Debug.Log("TweakTech: DeInit");

        }
#else

        public static void Main()
        {
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());


            RandomAdditionsAvail = LookForMod("RandomAdditions");
            WeaponAimModAvail = LookForMod("WeaponAimMod");
            TACAIModAvail = LookForMod("TAC_AI");
            FusionBlockAvail = LookForMod("Fusion Block");
            isBlockInjectorPresent = LookForMod("BlockInjector");
            Debug.Log("TweakTech: Kickstarted");

            
            try
            {
                //KickStartInitOptions.TryInit();
            }
            catch (Exception e) { Debug.LogError("TweakTech: Failed on Options&Config " + e); }
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

    public static class KickStartInitOptions
    {
        const string ModName = "TweakTech";

        public static OptionToggle enabledMod;
        //public static OptionRange multiHP;
        public static OptionToggle blockHP150;
        public static OptionToggle maxRestrict;
        public static OptionRange reduceProjectiles;
        public static OptionRange reduceProjectilesThreshold;

        public static void TryInit()
        {
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
                
                if (KickStart.EnableThis)
                    FDBookmark.EnableAll();
                else
                    FDBookmark.DisableAll();
            });*/
            maxRestrict = new OptionToggle("Lock to Max Restriction", Tweakables, ChangePatcher.MaximumFireRateAdjust);
            maxRestrict.onValueSaved.AddListener(() => {
                ChangePatcher.MaximumFireRateAdjust = maxRestrict.SavedValue;

                if (KickStart.EnableThis)
                    FDBookmark.EnableAll();
            });
            reduceProjectilesThreshold = new OptionRange("Maximum Firerate to Reduce", Tweakables, ChangePatcher.FirerateReductionMin, 0, 6);
            reduceProjectilesThreshold.onValueSaved.AddListener(() => {
                ChangePatcher.FirerateReductionMin = reduceProjectilesThreshold.SavedValue;

                if (KickStart.EnableThis)
                    FDBookmark.EnableAll();
            });
            reduceProjectiles = new OptionRange("Above Maximum Cooldown Multiplier", Tweakables, ChangePatcher.ProjectileReduction, 1, 8);
            reduceProjectiles.onValueSaved.AddListener(() => {
                ChangePatcher.ProjectileReduction = reduceProjectiles.SavedValue;

                if (KickStart.EnableThis)
                    FDBookmark.EnableAll();
            });
            /*
            blockHP150 = new OptionToggle("(SP Only) Disable 1.5 Health Multiplier", Tweakables, !ChangePatcher.UseGlobalHealthMulti);
            blockHP150.onValueSaved.AddListener(() => {
                ChangePatcher.UseGlobalHealthMulti = !blockHP150.SavedValue;
                
                if (KickStart.EnableThis)
                     FDBookmark.EnableAll();
            });
            
            multiHP = new OptionRange("Block Health Muliplier [0.5 - 2.5] \n- Requires World Restart", Tweakables, ChangePatcher.GlobalHealthMulti, 0.5f, 2.5f, 0.5f);
            multiHP.onValueSaved.AddListener(() => {
                ChangePatcher.GlobalHealthMulti = multiHP.SavedValue;
                
            });*/
            NativeOptionsMod.onOptionsSaved.AddListener(() => { thisModConfig.WriteConfigJsonFile(); });
        }
    }
}
