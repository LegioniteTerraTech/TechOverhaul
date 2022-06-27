using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using FusionBlock;

namespace TweakTech
{
#if STEAM
    public class KickStartTweakTech : ModBase
    {
        
        internal static KickStartTweakTech oInst;

        bool isInit = false;
        bool firstInit = false;
        public override bool HasEarlyInit()
        {
            return true;
        }

        // IDK what I should init here...
        public override void EarlyInit()
        {
            if (oInst == null)
            {
                oInst = this;
                if (!KickStart.hasPatched)
                {
                    try
                    {
                        KickStart.harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
                        KickStart.hasPatched = true;
                    }
                    catch
                    {
                        Debug.LogAutoStackTrace("TweakTech: Error on patch");
                    }
                }
            }
        }
        public override void Init() 
        {
            if (isInit)
                return;
            if (oInst == null)
                oInst = this;

            try
            {
                KickStart.Enable();
            }
            catch (Exception e) { Debug.FatalError(e); }
            if (!firstInit)
            {
                StatusCondition.InitNewStatus();
                ChangePatcher.ApplyTweaks();
                firstInit = true;
            }
            else
            {
                FDBookmark.EnableAll();
                BTBookmark.EnableAll();
            }
            isInit = true;
        }
        public override void DeInit()
        {
            if (!isInit)
                return;
            FDBookmark.DisableAll();
            BTBookmark.DisableAll();
            KickStart.Disable();
            isInit = false;
        }
        
    }
#endif

    internal class Patches
    {
        private static Type[] AllowedTypes = new Type[] {
                typeof(BTBookmark),
                typeof(FDBookmark),
            };

        // Major Patches
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
                    //MS.ResetMaterialToDefault();
                }
                BlockTweak.ApplyToBlockLocal(__instance);
            }
        }

        /*
        [HarmonyPatch(typeof(TankBlock))]
        [HarmonyPatch("OnAttach")]//
        private class FixFailInCalls
        {
            private static void Postfix(TankBlock __instance)
            {
                ReAimer.CreateOrUpdateForBlock(__instance);
            }
        }*/

        [HarmonyPatch(typeof(Explosion))]
        [HarmonyPatch("Explode")]//
        private class UpdateExplosionASAP
        {
            private static void Prefix(Explosion __instance)
            {
                var SDT = __instance.GetComponent<SpecialDamageType>();
                if (SDT)
                {
                    SDT.OverrideExplosion();
                }
            }
        }

        /*
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
        }*/


        [HarmonyPatch(typeof(ModuleWeapon))]
        [HarmonyPatch("OnAttach")]//
        private class AddLazyAim
        {
            private static void Prefix(ModuleWeapon __instance)
            {
                if (KickStart.WeaponAimModAvail)
                    return;
                TankLazyAim.Add(__instance.block.tank, __instance);
                ReAimer.CreateOrUpdateForBlock(__instance.block);
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

        [HarmonyPatch(typeof(ModuleDetachableLink))]
        [HarmonyPatch("DetachBlock")]//
        private class ReturnBoltsOnFire
        {   // Come on, detachable bolts should be reusable lol
            private static void Prefix(ModuleDetachableLink __instance)
            {
                Tank tank = __instance.block.tank;
                if (tank)
                {
                    bool isMerge = false;
                    try
                    {
                        if (KickStart.FusionBlockAvail)
                            isMerge = SafeCheck(__instance);
                        if (isMerge || tank.Team != ManPlayer.inst.PlayerTeam)
                            return;
                    }
                    catch { }

                    if (Singleton.Manager<ManPlayer>.inst.InventoryIsUnrestricted) { }
                    else if (ManNetwork.IsNetworked)
                    {
                        if (ManNetwork.IsHost)
                        {
                            if (tank.netTech?.NetPlayer?.Inventory)
                                tank.netTech?.NetPlayer?.Inventory.HostAddItem(__instance.block.BlockType, 1);
                        }
                    }
                    else
                    {
                        Singleton.Manager<ManPlayer>.inst.AddBlockToInventory(__instance.block.BlockType);
                    }
                }
            }
            private static bool SafeCheck(ModuleDetachableLink __instance)
            {
                return __instance.GetComponent<ModuleFuseHalf>();
            }
        }

        
        
        [HarmonyPatch(typeof(Damageable))]
        [HarmonyPatch("TryToDamage")]//
        private class AddNewDamageTypesHandling
        {
            private static void Prefix(Damageable __instance, ref ManDamage.DamageInfo info, ref bool actuallyDealDamage)
            {
                if (actuallyDealDamage)
                {
                    info.ApplyDamageMultiplier(StatusCondition.RunStatusPre(__instance, info));
                }
            }
            /*
            private static void Postfix(Damageable __instance, ref ManDamage.DamageInfo info, ref bool actuallyDealDamage)
            {
                if (actuallyDealDamage)
                {
                    StatusCondition.RunStatusPost(__instance, info);
                }
            }*/
        }

        // ------------------------------------------------------
        //                      MP Changes
        // ------------------------------------------------------
        [HarmonyPatch(typeof(Mode))]
        [HarmonyPatch("EnterPreMode")]//On very late update
        private static class Startup
        {
            private static void Prefix()
            {
                if (!DeathmatchExt.Ready)
                {
                    DeathmatchExt.SetReady();
                }
            }
        }

        [HarmonyPatch(typeof(MultiplayerTechSelectGroupAsset))]
        [HarmonyPatch("GetTechPresets")]//
        private class ChangeDeathmatchChoices
        {
            /*
            static FieldInfo loadouts = typeof(ModePVP<>)
                       .GetField("m_AvailableLoadouts", BindingFlags.NonPublic | BindingFlags.Instance);
            */
            private static void Postfix(MultiplayerTechSelectGroupAsset __instance, ref List<MultiplayerTechSelectPresetAsset> __result)
            {
                try
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space))
                    {
                        Debug.Log("Load DeathmatchExt 1: " + __result.Count());
                    }
                    else
                    {
                        if (DeathmatchExt.Ready)
                            __result = DeathmatchExt.MakeNewDeathmatchTechs(__result);
                    }
                }
                catch (Exception e) { Debug.LogError("TweakTech: OOOOOOOF " + e); }
            }
        }



        // ------------------------------------------------------
        //                      Game-Changers
        // ------------------------------------------------------
        [HarmonyPatch(typeof(TankBlock))]
        [HarmonyPatch("InitNew")]//
        private class ResetBlock
        {
            private static void Prefix(TankBlock __instance)
            {
                var Status = __instance.GetComponent<StatusCondition>();
                if (Status)
                {
                    Status.OnRemove();
                }
            }
        }
        [HarmonyPatch(typeof(ModuleWeapon))]
        [HarmonyPatch("Process")]//
        private class MakeModuleWeaponAffectable
        {
            private static bool Prefix(ModuleWeapon __instance)
            {
                var Status = __instance.GetComponent<StatusCondition>();
                if (Status)
                {
                    if (!Status.allowModuleUpdate)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ModuleWheels))]
        [HarmonyPatch("ControlInput")]//
        private class MakeModuleWheelsAffectable
        {
            private static bool Prefix(ModuleWheels __instance, ref TankControl.ControlState drive)
            {
                var Status = __instance.GetComponent<StatusCondition>();
                if (Status)
                {
                    TankControl.ControlState driveR;
                    TankControl.State state;
                    float drivePower;
                    switch (Status.Status)
                    {
                        case StatusType.Freezing:
                            float MaxSped = __instance.m_WheelParams.radius * __instance.m_TorqueParams.torqueCurveMaxRpm * 0.15f;
                            float currentSpeed;
                            if (__instance.block.tank?.rbody)
                                currentSpeed = __instance.block.tank.rbody.velocity.magnitude;
                            else
                                currentSpeed = 0;
                            if (MaxSped < 1)
                                MaxSped = 4;
                            float maximalOut = Mathf.Max(-1, (MaxSped * (Status.GetOpPercent() + 0.2f)) - currentSpeed) / MaxSped;

                            if (currentSpeed > 4)
                                drivePower = Mathf.Min(1, maximalOut);
                            else
                                drivePower = 1;
                            state = new TankControl.State();
                            state.m_InputMovement = drive.InputMovement * drivePower;
                            state.m_InputRotation = drive.InputRotation * drivePower;
                            driveR = new TankControl.ControlState();
                            driveR.m_State = state;
                            drive = driveR;
                            break;
                        case StatusType.EMF:
                            drivePower = Status.GetOpPercent();
                            state = new TankControl.State();
                            state.m_InputMovement = drive.InputMovement * drivePower;
                            state.m_InputRotation = drive.InputRotation * drivePower;
                            driveR = new TankControl.ControlState();
                            driveR.m_State = state;
                            drive = driveR;
                            break;
                    }
                }
                return true;
            }

            [HarmonyPatch(typeof(ModuleBooster))]
            [HarmonyPatch("DriveControlInput")]//
            private class MakeBoostersAffectable
            {
                private static bool Prefix(ModuleBooster __instance, ref TankControl.ControlState driveData)
                {
                    var Status = __instance.GetComponent<StatusCondition>();
                    if (Status)
                    {
                        TankControl.ControlState driveR;
                        TankControl.State state;
                        float drivePower;
                        switch (Status.Status)
                        {
                            case StatusType.Freezing:
                            case StatusType.EMF:
                                drivePower = Status.GetOpPercent();
                                state = new TankControl.State();
                                state.m_BoostJets =  driveData.BoostJets ? Status.allowModuleUpdate : false;
                                state.m_BoostProps = driveData.BoostProps ? Status.allowModuleUpdate : false;
                                state.m_InputMovement = driveData.InputMovement * drivePower;
                                state.m_InputRotation = driveData.InputRotation * drivePower;
                                driveR = new TankControl.ControlState();
                                driveR.m_State = state;
                                driveData = driveR;
                                break;
                        }
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(ModuleLinearMotionEngine))]
            [HarmonyPatch("OnDriveControl")]//
            private class MakeLMEAffectable
            {
                private static bool Prefix(ModuleLinearMotionEngine __instance, ref TankControl.ControlState driveData)
                {
                    var Status = __instance.GetComponent<StatusCondition>();
                    if (Status)
                    {
                        TankControl.ControlState driveR;
                        TankControl.State state;
                        float drivePower;
                        switch (Status.Status)
                        {
                            case StatusType.Freezing:
                            case StatusType.EMF:
                                drivePower = Status.GetOpPercent();
                                state = new TankControl.State();
                                state.m_BoostJets = driveData.BoostJets ? Status.allowModuleUpdate : false;
                                state.m_BoostProps = driveData.BoostProps ? Status.allowModuleUpdate : false;
                                state.m_InputMovement = driveData.InputMovement * drivePower;
                                state.m_InputRotation = driveData.InputRotation * drivePower;
                                driveR = new TankControl.ControlState();
                                driveR.m_State = state;
                                driveData = driveR;
                                break;
                        }
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(ModuleHover))]
            [HarmonyPatch("DriveControlInput")]//
            private class MakeHoversAffectable
            {
                private static bool Prefix(ModuleHover __instance, ref TankControl.ControlState controlState)
                {
                    var Status = __instance.GetComponent<StatusCondition>();
                    if (Status)
                    {
                        TankControl.ControlState driveR;
                        TankControl.State state;
                        float drivePower;
                        switch (Status.Status)
                        {
                            case StatusType.Freezing:
                                drivePower = Status.GetOpPercent();
                                state = new TankControl.State();
                                state.m_InputMovement = controlState.InputMovement * drivePower;
                                state.m_InputRotation = controlState.InputRotation * drivePower;
                                driveR = new TankControl.ControlState();
                                driveR.m_State = state;
                                controlState = driveR;
                                break;
                            case StatusType.EMF:
                                foreach (HoverJet hj in __instance.GetComponentsInChildren<HoverJet>())
                                {
                                    hj.OnControlInput(controlState, 0);
                                }
                                return false;
                        }
                    }
                    return true;
                }
            }
        }
    }
}
