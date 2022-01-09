using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace TweakTech
{
    internal class Patches
    {
        [HarmonyPatch(typeof(ManSpawn))]
        [HarmonyPatch("OnDLCLoadComplete")]//
        private class AdjustBlocks
        {
            private static void Postfix(ManSpawn __instance)
            {
                StatusCondition.InitNewStatus();
                ChangePatcher.ApplyTweaks();
            }
        }

        
        [HarmonyPatch(typeof(MaterialSwapper))]
        [HarmonyPatch("StartMaterialPulse")]//
        private class FixStrobeError
        {
            private static bool Prefix(MaterialSwapper __instance)
            {
                var SC = __instance.GetComponent<StatusCondition>();
                if (SC)
                    return false;
                return true;
            }
        }
        [HarmonyPatch(typeof(MaterialSwapper))]
        [HarmonyPatch("SetDamageColourAndEmissiveScale")]//
        private class FixStrobeError2
        {
            private static bool Prefix(MaterialSwapper __instance)
            {
                var SC = __instance.GetComponent<StatusCondition>();
                if (SC)
                    return false;
                return true;
            }
        }
        [HarmonyPatch(typeof(TankBlock))]
        [HarmonyPatch("OnSpawn")]//
        private class FixRedError2
        {
            private static void Postfix(TankBlock __instance)
            {
                var MS = __instance.GetComponent<MaterialSwapper>();
                if (MS)
                {
                    MS.SwapMaterialDamage(false);
                }
            }
        }

        [HarmonyPatch(typeof(ShotgunRound))]
        [HarmonyPatch("Fire")]//
        private class AdjustDamageShotgun
        {
            private static void Prefix(ShotgunRound __instance, ref Vector3 fireDirection, ref FireData fireData, ref ModuleWeapon weapon, ref Tank shooter, ref bool seekingRounds, ref bool replayRounds)
            {
                var instModif = __instance.GetComponent<ShotgunOverride>();
                if (!(bool)instModif)
                    instModif = __instance.gameObject.AddComponent<ShotgunOverride>();
                instModif.Fire(fireDirection, fireData, weapon, shooter, seekingRounds, replayRounds);
            }
        }


        [HarmonyPatch(typeof(TargetAimer))]
        [HarmonyPatch("Init")]//
        private class ChangeAimDelegate
        {
            static FieldInfo aimD = typeof(TargetAimer).GetField("AimDelegate", BindingFlags.NonPublic | BindingFlags.Instance);
            private static bool Prefix(TargetAimer __instance, ref TankBlock block, ref Func<Vector3, Vector3> aimDelegate)
            {
                if (KickStart.WeaponAimModAvail)
                    return true;
                try
                {
                    if ((Func<Vector3, Vector3>)aimD.GetValue(__instance) != null && aimDelegate != null)
                        return false;
                }
                catch
                { //Debug.Log("TweakTech: Error with TargetAimer for " + block.name); 
                }
                return true;
            }
            private static void Postfix(TargetAimer __instance, ref TankBlock block, ref Func<Vector3, Vector3> aimDelegate)
            {
                if (KickStart.WeaponAimModAvail)
                    return;
                try
                {
                    //aimDelegate == null && 
                    if ((bool)block)
                    {
                        var RA = block.GetComponent<ReAimer>();
                        if (!(bool)RA)
                        {
                            RA = ReAimer.ApplyToBlock(block);
                        }
                        else
                            ReAimer.UpdateExisting(block);
                        aimD.SetValue(__instance, RA.swatch);
                    }
                }
                catch
                { //Debug.Log("TweakTech: Error with TargetAimer for " + block.name); 
                }
            }
        }


        [HarmonyPatch(typeof(ModuleWeapon))]
        [HarmonyPatch("OnAttach")]//
        private class AddLazyAim
        {
            private static void Prefix(ModuleWeapon __instance)
            {
                if (KickStart.WeaponAimModAvail)
                    return;
                TankLazyAim.Add(__instance.block.tank, __instance);
            }
        }

        [HarmonyPatch(typeof(ModuleWeapon))]
        [HarmonyPatch("OnDetach")]//
        private class RemoveLazyAim
        {
            private static void Prefix(ModuleWeapon __instance)
            {
                if (KickStart.WeaponAimModAvail)
                    return;
                TankLazyAim.Remove(__instance.block.tank, __instance);
            }
        }

        
        [HarmonyPatch(typeof(Damageable))]
        [HarmonyPatch("TryToDamage")]//
        private class AddNewDamageTypesHandling
        {
            private static void Prefix(Damageable __instance, ref ManDamage.DamageInfo info, ref bool actuallyDealDamage)
            {
                StatusCondition SC = __instance.GetComponent<StatusCondition>();
                if (SC)
                {
                    if (SC.Status == StatusType.Overheat)
                    {
                        if (__instance.MaxHealth > 0)
                        info.ApplyDamageMultiplier(1 + (SC.impactValue / (__instance.MaxHealth * 2)));
                    }
                }
            }
            private static void Postfix(Damageable __instance, ref ManDamage.DamageInfo info, ref bool actuallyDealDamage)
            {
                if (actuallyDealDamage)
                {
                    StatusCondition SC;
                    switch (info.DamageType)
                    {
                        case ManDamage.DamageType.Fire:
                            SC = StatusCondition.InitOrGet(__instance, StatusType.Overheat);
                            if (SC)
                            {
                                SC.AddToVal(info.Damage);
                            }
                            return;
                        case (ManDamage.DamageType)DamageTypesExt.Cyro:
                            return;
                        case (ManDamage.DamageType)DamageTypesExt.EMP:
                            return;
                    }
                }
            }
        }
    }
}
