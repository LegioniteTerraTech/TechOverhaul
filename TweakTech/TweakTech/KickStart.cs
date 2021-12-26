﻿using System;
using Nuterra.NativeOptions;
using ModHelper.Config;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace TweakTech
{
    // This mod ultimately relies off of RandomAdditions to operate!
    //   WeaponAimMod is also advised for better tracking as this mod has simple target aim prediction.
    public class KickStart
    {
        //Let hooks happen i guess
        const string ModName = "TweakTech";

        internal static bool RandomAdditionsAvail = false;
        internal static bool WeaponAimModAvail = false;
        internal static bool TACAIModAvail = false;

        public static OptionToggle maxRestrict;
        public static OptionRange reduceProjectiles;
        public static OptionRange reduceProjectilesThreshold;

        public static void Main()
        {
            Harmony harmonyInstance = new Harmony("legioniteterratech.tweaktech");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());


            RandomAdditionsAvail = LookForMod("RandomAdditions");
            WeaponAimModAvail = LookForMod("WeaponAimMod");
            TACAIModAvail = LookForMod("TAC_AI");
            Debug.Log("TweakTech: Kickstarted");

            ModConfig thisModConfig = new ModConfig();
            thisModConfig.BindConfig<ChangePatcher>(null, "MaximumFireRateAdjust");
            thisModConfig.BindConfig<ChangePatcher>(null, "ProjectileReduction");
            thisModConfig.BindConfig<ChangePatcher>(null, "FirerateReductionMin");

            string Tweakables = ModName;
            maxRestrict = new OptionToggle("Lock to Max Restriction", Tweakables, ChangePatcher.MaximumFireRateAdjust);
            maxRestrict.onValueSaved.AddListener(() => {
                ChangePatcher.MaximumFireRateAdjust = maxRestrict.SavedValue;
                thisModConfig.WriteConfigJsonFile();
                FDBookmark.EnableAll();
            });
            reduceProjectilesThreshold = new OptionRange("Maximum Firerate to Reduce", Tweakables, ChangePatcher.FirerateReductionMin, 1, 8);
            reduceProjectilesThreshold.onValueSaved.AddListener(() => {
                ChangePatcher.FirerateReductionMin = reduceProjectilesThreshold.SavedValue;
                thisModConfig.WriteConfigJsonFile();
                FDBookmark.EnableAll();
            });
            reduceProjectiles = new OptionRange("Above Maximum Cooldown Multiplier", Tweakables, ChangePatcher.ProjectileReduction, 1, 8);
            reduceProjectiles.onValueSaved.AddListener(() => { ChangePatcher.ProjectileReduction = reduceProjectiles.SavedValue; 
                thisModConfig.WriteConfigJsonFile();
                FDBookmark.EnableAll();
            });
        }

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