﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TweakTech
{
    /// <summary>
    /// This should only edit existing blocks, not the references.
    /// </summary>
    public class ChangePatcher
    {
        public static bool MaximumFireRateAdjust = true;
        public static float ProjectileReduction = 4f;
        public static float FirerateReductionMin = 4f;

        internal const float PlayerDamageMulti = 1f;

        public static bool UseGlobalHealthMulti = true;
        internal static float GlobalHealthMulti = 1;//1.5f;//2f;
        internal const float GlobalDetachMulti = 0.5f;

        internal const float HighExplosiveSpeedMulti = 0.6f;
        internal const float FasterSpeedMulti = 1.5f;

        internal const int GC_HealthPoolStandard = 275;
        internal const float GC_WeightPoolStandard = 1.25f;

        //internal const float ShotgunShieldMultiplier = 5f;
        //internal const float ShotgunKnockbackShieldMultiplier = 12.5f;

        public static bool worked = false;
        internal static int timesFired = 0;
        private static int AdditionalDamageTypes = 5;


        internal static void ApplyTweaks()
        {
            if (worked)
                return;
            worked = true;
            if (!KickStart.RandomAdditionsAvail)
            {
                Debug.Log("TweakTech: RandomAdditions must be installed and enabled for this to work properly!");
                KickStart.WeaponAimModAvail = false;
                return;
            }
            if (!KickStart.WeaponAimModAvail)
            {
                Debug.Log("TweakTech: WeaponAimMod is highly advised - The default target leading provided in this mod is primative");
            }

            ApplyGlobalBlockTweaks();
            ApplyDamageTweaks();
            ApplyBlockTweaks();
            ApplyFireRateBlockTweaks();
            BTBookmark.startWorking = true;
            FDBookmark.startWorking = true;
            if (!KickStart.EnableThis)
                FDBookmark.DisableAll();
        }

        internal static FieldInfo deals = typeof(Projectile).GetField("m_Damage", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo allBlocks = typeof(ManSpawn).GetField("m_BlockPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
        private static void ApplyGlobalBlockTweaks()
        {
            int count = 0;
            Dictionary<int, Transform> blocks = (Dictionary<int, Transform>)allBlocks.GetValue(Singleton.Manager<ManSpawn>.inst);
            int[] blocks1 = new int[blocks.Count];
            blocks.Keys.CopyTo(blocks1, 0);
            foreach (int num in blocks1)
            {
                BlockTypes BT = (BlockTypes)num;
                TankBlock bloc = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(BT);
                Transform trans = bloc.transform;
                try
                {
                    /*
                    if (Singleton.Manager<ManSpawn>.inst.GetCorporation(BT) == FactionSubTypes.GC)
                    {
                        var dmg = trans.GetComponent<Damageable>();
                        if ((bool)dmg)
                        {
                            if (dmg.DamageableType == ManDamage.DamageableType.Standard)
                            {
                                float healthAdd = GC_HealthPoolStandard * bloc.filledCells.Length;
                                if (dmg.MaxHealth < healthAdd)
                                {
                                    dmg.SetMaxHealth(healthAdd);
                                    bloc.m_DefaultMass = GC_WeightPoolStandard * bloc.filledCells.Length;
                                    //Debug.Log("TweakTech: ApplyGlobalBlockTweaks - changed GC block " + bloc.name + " to health " + healthAdd);
                                }
                            }
                        }
                    }*/

                    if (bloc.GetComponent<ModuleItemProducer>())
                    {   // no weak autominers
                        var dmg = trans.GetComponent<ModuleDamage>();
                        if ((bool)dmg)
                            dmg.m_DamageDetachFragility = 0;
                    }
                    /*
                    if (bloc.GetComponent<FireDataShotgun>())
                    {   // Shotguns ignore shields
                        var FD = trans.GetComponent<FireData>();
                        if ((bool)FD)
                            FD.m_BulletPrefab.gameObject.AddComponent<ShotgunOverride>();
                    }*/

                    count++;
                }
                catch (Exception e)
                {
                    Debug.Log("TweakTech: ApplyGlobalBlockTweaks - error " + e);
                }
            }
            Debug.Log("TweakTech: ApplyGlobalBlockTweaks - changed " + count);
            Globals.inst.playerDamageReceivedMultiplierPC = PlayerDamageMulti * Globals.inst.playerDamageReceivedMultiplierPC;
            Debug.Log("TweakTech: ApplyGlobalBlockTweaks - changed player damage multiplier to " + Globals.inst.playerDamageReceivedMultiplierPC);
            Globals.inst.moduleDamageParams.detachMeterFillFactor = GlobalDetachMulti * Globals.inst.moduleDamageParams.detachMeterFillFactor;
            Globals.inst.moduleDamageParams.detachMeterFillFactorPlayerPC = GlobalDetachMulti * Globals.inst.moduleDamageParams.detachMeterFillFactor;
            Debug.Log("TweakTech: ApplyGlobalBlockTweaks - changed ALL fragilities to " + Globals.inst.moduleDamageParams.detachMeterFillFactor);

        }
        private static void ApplyFireRateBlockTweaks()
        {   // this edits the vanilla prefabs.  DO NOT DO THIS
            /*
            Dictionary<int, Transform> prefabBlocks = (Dictionary<int, Transform>)allBlocks.GetValue(Singleton.Manager<ManSpawn>.inst);
            int[] blocks1 = new int[prefabBlocks.Count];
            prefabBlocks.Keys.CopyTo(blocks1, 0);
            foreach (int num in blocks1)
            {
                BlockTypes BT = (BlockTypes)num;
                TankBlock bloc = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(BT);
                ApplyFireRateBlockTweak(bloc, BT);
            }
            Debug.Log("TweakTech: ApplyFireRateBlockTweaks - changed " + timesFired);
            timesFired = 0;
            */
        }

        // Changing

        /// <summary>
        /// Set the prefabs? This one's broken asince it changes the base prefabs
        /// </summary>
        /// <param name="bloc">The block to apply to</param>
        /// <param name="BT">The block type of the tank block</param>
        /// <param name="useOG">Take the prefab's stats</param>
        private static void ApplyFireRateBlockTweak(TankBlock bloc, BlockTypes BT, bool useOG = true)
        {
            Transform trans = bloc.transform;
            try
            {
                var MW = bloc.GetComponent<ModuleWeapon>();
                if ((bool)MW)
                {   // reduce firerates
                    var MWG = bloc.GetComponent<ModuleWeaponGun>();
                    var FDS = bloc.GetComponent<FireDataShotgun>();
                    var BW = bloc.GetComponentInChildren<BeamWeapon>();
                    if ((bool)MWG && !(bool)BW && !(bool)FDS)
                    {
                        var FD = trans.GetComponent<FireData>();
                        bool shouldReduce = MWG.m_ShotCooldown < FirerateReductionMin;
                        if ((bool)FD && shouldReduce)
                        {
                            var WR = WeaponTweak.GetOrSetBPrefab(bloc, BT);
                            if ((bool)WR)
                            {
                                var WROG = FD.m_BulletPrefab;
                                var ProjOG = FD.m_BulletPrefab.GetComponent<Projectile>();
                                var Proj = WR.GetComponent<Projectile>();
                                if ((bool)Proj && (bool)ProjOG)
                                {
                                    float adjust = ProjectileReduction;
                                    if (MaximumFireRateAdjust)
                                    {
                                        adjust = FirerateReductionMin / Mathf.Max(MWG.m_ShotCooldown, 0.01f);
                                    }
                                    if (useOG)
                                    {
                                        try
                                        {
                                            var trans2 = (Transform)WeaponTweak.explode.GetValue(ProjOG);
                                            var OGSplode = trans2.GetComponent<Explosion>();
                                            var trans3 = WeaponTweak.GetOrSetEPrefab(bloc, BT);
                                            var retrofit = trans3.GetComponent<Explosion>();
                                            if ((bool)OGSplode && (bool)retrofit)
                                            {
                                                retrofit.m_MaxDamageStrength = OGSplode.m_MaxDamageStrength * adjust;
                                                retrofit.m_MaxImpulseStrength = (OGSplode.m_MaxImpulseStrength * (adjust / 4f)) + OGSplode.m_MaxImpulseStrength;
                                            }
                                            WeaponTweak.explode.SetValue(Proj, trans3);
                                        }
                                        catch { }
                                        WeaponTweak.deals.SetValue(WR, (int)((int)WeaponTweak.deals.GetValue(WROG) * adjust));
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var trans2 = (Transform)WeaponTweak.explode.GetValue(ProjOG);
                                            var OGSplode = trans2.GetComponent<Explosion>();
                                            var trans3 = WeaponTweak.GetOrSetEPrefab(bloc, BT);
                                            var retrofit = trans3.GetComponent<Explosion>();
                                            if ((bool)retrofit)
                                            {
                                                retrofit.m_MaxDamageStrength *= adjust;
                                                retrofit.m_MaxImpulseStrength = (retrofit.m_MaxImpulseStrength * (adjust / 4f)) + retrofit.m_MaxImpulseStrength;
                                            }
                                            WeaponTweak.explode.SetValue(Proj, trans3);
                                        }
                                        catch { }
                                        WeaponTweak.deals.SetValue(WR, (int)((int)WeaponTweak.deals.GetValue(WR) * adjust));
                                    }
                                    timesFired++;
                                    WeaponTweak.SetBPrefab(BT, WR);
                                    if (!bloc.GetComponent<FDBookmark>())
                                        bloc.gameObject.AddComponent<FDBookmark>().Main = WR;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: ApplyFireRateBlockTweaks - error " + e);
            }
        }
        
        /// <summary>
        /// Tweaks the existing active blocks without editing the prefabs
        /// </summary>
        /// <param name="BT"></param>
        /// <param name="BTW"></param>
        /// <returns></returns>
        internal static string ApplyFireRateBlockTweakActive(BlockTypes BT, BlockTweak BTW)
        {
            TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(BT);
            Transform transPrefab = prefab.transform;
            try
            {
                var MW = prefab.GetComponent<ModuleWeapon>();
                if ((bool)MW)
                {   // reduce firerates
                    var MWG = prefab.GetComponent<ModuleWeaponGun>();
                    if ((bool)MWG)
                    {
                        var FD = transPrefab.GetComponent<FireData>();
                        bool shouldReduce = MWG.m_ShotCooldown < FirerateReductionMin;
                        if ((bool)FD && shouldReduce)
                        {
                            var WR = WeaponTweak.GetOrSetBPrefab(prefab, BT);
                            if ((bool)WR)
                            {
                                // RESET
                                //Singleton.Manager<ComponentPool>.inst.RemovePool(WR);
                                var WROG = FD.m_BulletPrefab;
                                var ProjOG = FD.m_BulletPrefab.GetComponent<Projectile>();
                                var Proj = WR.GetComponent<Projectile>();
                                if ((bool)Proj && (bool)ProjOG)
                                {
                                    bool exploGet = false;
                                    Explosion OGSplode = null;
                                    Explosion retrofit = null;
                                    try
                                    {
                                        var trans2 = (Transform)WeaponTweak.explode.GetValue(ProjOG);
                                        OGSplode = trans2.GetComponent<Explosion>();
                                        var trans3 = WeaponTweak.GetOrSetEPrefab(prefab, BT);
                                        retrofit = trans3.GetComponent<Explosion>();
                                        if ((bool)OGSplode && (bool)retrofit)
                                        {
                                            retrofit.m_MaxDamageStrength = OGSplode.m_MaxDamageStrength;
                                            retrofit.m_MaxImpulseStrength = OGSplode.m_MaxImpulseStrength;
                                            exploGet = true;
                                        }
                                    }
                                    catch { }
                                    WeaponTweak.deals.SetValue(WR, (int)WeaponTweak.deals.GetValue(WROG));
                                    
                                    //if (BTW != null)
                                    //    BTW.ApplyToBlock();

                                    float adjust = ProjectileReduction;
                                    if (MaximumFireRateAdjust)
                                    {
                                        adjust = FirerateReductionMin / Mathf.Max(MWG.m_ShotCooldown, 0.01f);
                                    }
                                    if (exploGet)
                                    {
                                        retrofit.m_MaxDamageStrength *= adjust;
                                        retrofit.m_MaxImpulseStrength = (retrofit.m_MaxImpulseStrength * (adjust / 4f)) + retrofit.m_MaxImpulseStrength;
                                        //Debug.Log("TweakTech: ApplyFireRateBlockTweaks - Block " + BT.ToString() + " explosion is now " + retrofit.m_MaxDamageStrength);
                                    }

                                    int newDamageDirect = (int)((int)WeaponTweak.deals.GetValue(WR) * adjust);
                                    //Debug.Log("TweakTech: ApplyFireRateBlockTweaks - Block " + BT.ToString() + " direct is now " + newDamageDirect);
                                    //Debug.Log("TweakTech: ApplyFireRateBlockTweaks - Block " + BT.ToString() + " stored in " + WR.name);
                                    deals.SetValue(Proj, newDamageDirect);
                                    WeaponTweak.SetBPrefab(BT, WR);

                                    timesFired++;
                                    return WR.name;
                                    //if (!bloc.GetComponent<FDBookmark>())
                                    //    bloc.gameObject.AddComponent<FDBookmark>().Main = WR;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: ApplyFireRateBlockTweaks - error on " + BT.ToString() + " " + e);
            }
            return "";
        }

        /// <summary>
        /// Use this to reset the tweaks doe to the active block to the prefab's values
        /// </summary>
        /// <param name="BT"></param>
        /// <param name="WR"></param>
        internal static void CopyFireRateBlockTweakToPRESENT(BlockTypes BT, WeaponRound WR)
        {
            TankBlock Prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(BT);
            Transform transPrefab = Prefab.transform;
            try
            {
                var MW = Prefab.GetComponent<ModuleWeapon>();
                if ((bool)MW)
                {   // reduce firerates
                    var MWG = Prefab.GetComponent<ModuleWeaponGun>();
                    if ((bool)MWG)
                    {
                        var FD = transPrefab.GetComponent<FireData>();
                        bool shouldReduce = MWG.m_ShotCooldown < FirerateReductionMin;
                        if ((bool)FD && shouldReduce)
                        {
                            if ((bool)WR)
                            {
                                var WROG = FD.m_BulletPrefab;
                                if (WROG == WR)
                                    return;
                                var WRREF = WeaponTweak.GetOrSetBPrefab(null, BT);
                                var ProjREF = WRREF.GetComponent<Projectile>();
                                var Proj = WR.GetComponent<Projectile>();
                                if ((bool)Proj && (bool)ProjREF)
                                {
                                    bool exploGet = false;
                                    Explosion REFSplode = null;
                                    Explosion retrofit = null;
                                    try
                                    {
                                        var trans2 = (Transform)WeaponTweak.explode.GetValue(ProjREF);
                                        REFSplode = trans2.GetComponent<Explosion>();
                                        var trans3 = WeaponTweak.GetOrSetEPrefab(Prefab, BT);
                                        retrofit = trans3.GetComponent<Explosion>();
                                        if ((bool)REFSplode && (bool)retrofit)
                                        {
                                            exploGet = true;
                                        }
                                    }
                                    catch { }
                                    //WeaponTweak.deals.SetValue(WR, (int)WeaponTweak.deals.GetValue(WROG));


                                    if (exploGet)
                                    {
                                        retrofit.m_MaxDamageStrength = REFSplode.m_MaxDamageStrength;
                                        retrofit.m_MaxImpulseStrength = REFSplode.m_MaxImpulseStrength;
                                        //Debug.Log("TweakTech: CopyFireRateBlockTweakToPRESENT - Block " + BT.ToString() + " explosion is now " + retrofit.m_MaxDamageStrength);
                                    }
                                    int newDamageDirect = (int)WeaponTweak.deals.GetValue(WRREF);
                                    //Debug.Log("TweakTech: CopyFireRateBlockTweakToPRESENT - Block " + BT.ToString() + " direct is now " + newDamageDirect);
                                    //Debug.Log("TweakTech: CopyFireRateBlockTweakToPRESENT - Block " + BT.ToString() + " stored in " + WR.name);
                                    deals.SetValue(Proj, newDamageDirect);

                                    timesFired++;
                                    //if (!bloc.GetComponent<FDBookmark>())
                                    //    bloc.gameObject.AddComponent<FDBookmark>().Main = WR;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: CopyFireRateBlockTweakToPRESENT - error on " + BT.ToString() + " " + e);
            }
        }

        // Returning
        /// <summary>
        /// Resets the active block back to the prefab's values
        /// </summary>
        /// <param name="BT"></param>
        /// <param name="BTW"></param>
        /// <returns></returns>
        internal static string ResetBlockTweakActive(BlockTypes BT, BlockTweak BTW)
        {
            TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(BT);
            Transform transPrefab = prefab.transform;
            try
            {
                var MW = prefab.GetComponent<ModuleWeapon>();
                if ((bool)MW)
                {   // reduce firerates
                    var MWG = prefab.GetComponent<ModuleWeaponGun>();
                    if ((bool)MWG)
                    {
                        var FD = transPrefab.GetComponent<FireData>();
                        if ((bool)FD)
                        {
                            var WR = WeaponTweak.GetOrSetBPrefab(prefab, BT);
                            if ((bool)WR)
                            {   // RESET
                                var Proj = WR.GetComponent<Projectile>();
                                var WROG = FD.m_BulletPrefab;
                                var ProjOG = FD.m_BulletPrefab.GetComponent<Projectile>();
                                if ((bool)Proj && (bool)ProjOG)
                                {
                                    Explosion OGSplode = null;
                                    Explosion retrofit = null;
                                    try
                                    {
                                        var trans2 = (Transform)WeaponTweak.explode.GetValue(ProjOG);
                                        OGSplode = trans2.GetComponent<Explosion>();
                                        var trans3 = WeaponTweak.GetOrSetEPrefab(prefab, BT);
                                        retrofit = trans3.GetComponent<Explosion>();
                                        if ((bool)OGSplode && (bool)retrofit)
                                        {
                                            retrofit.m_MaxDamageStrength = OGSplode.m_MaxDamageStrength;
                                            retrofit.m_MaxImpulseStrength = OGSplode.m_MaxImpulseStrength;
                                        }
                                    }
                                    catch { }
                                    WeaponTweak.deals.SetValue(WR, (int)WeaponTweak.deals.GetValue(WROG));
                                    
                                    if (BTW != null)
                                        BTW.ResetApplyToBlock();


                                    deals.SetValue(Proj, (int)WeaponTweak.deals.GetValue(WR));
                                    WeaponTweak.SetBPrefab(BT, WR);

                                    timesFired++;
                                    return WR.name;
                                    //if (!bloc.GetComponent<FDBookmark>())
                                    //    bloc.gameObject.AddComponent<FDBookmark>().Main = WR;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: ApplyFireRateBlockTweaks - error on " + BT.ToString() + " " + e);
            }
            return "";
        }
        internal static void ResetFireRateBlockTweakToPRESENT(BlockTypes BT, WeaponRound WR)
        {
            TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(BT);
            Transform transPrefab = prefab.transform;
            try
            {
                var MW = prefab.GetComponent<ModuleWeapon>();
                if ((bool)MW)
                {   // reduce firerates
                    var MWG = prefab.GetComponent<ModuleWeaponGun>();
                    if ((bool)MWG)
                    {
                        var FD = transPrefab.GetComponent<FireData>();
                        if ((bool)FD)
                        {
                            if ((bool)WR)
                            {
                                var WROG = FD.m_BulletPrefab;
                                if (WROG == WR)
                                    return;
                                var ProjREF = WROG.GetComponent<Projectile>();
                                var Proj = WR.GetComponent<Projectile>();
                                if ((bool)Proj && (bool)ProjREF)
                                {
                                    bool exploGet = false;
                                    Explosion REFSplode = null;
                                    Explosion retrofit = null;
                                    try
                                    {
                                        var trans2 = (Transform)WeaponTweak.explode.GetValue(ProjREF);
                                        REFSplode = trans2.GetComponent<Explosion>();
                                        var trans3 = WeaponTweak.GetOrSetEPrefab(prefab, BT);
                                        retrofit = trans3.GetComponent<Explosion>();
                                        if ((bool)REFSplode && (bool)retrofit)
                                        {
                                            exploGet = true;
                                        }
                                    }
                                    catch { }


                                    if (exploGet)
                                    {
                                        retrofit.m_MaxDamageStrength = REFSplode.m_MaxDamageStrength;
                                        retrofit.m_MaxImpulseStrength = REFSplode.m_MaxImpulseStrength;
                                    }
                                    int newDamageDirect = (int)WeaponTweak.deals.GetValue(WROG);
                                    deals.SetValue(Proj, newDamageDirect);

                                    timesFired++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: CopyFireRateBlockTweakToPRESENT - error on " + BT.ToString() + " " + e);
            }
        }

        private static bool appliedFirstTweaks = false;
        private static bool appliedTweaks = false;
        internal static void ApplyBlockTweaks()
        {
            if (appliedTweaks)
                return;
            Debug.Log("TweakTech: ApplyBlockTweaks - Initializing");
            if (!appliedFirstTweaks)
            {
                Debug.Log("TweakTech: ApplyBlockTweaks(FIRST INIT) - Initializing");
                foreach (BlockTweak BT in Tweaks.BlockTweaks)
                {
                    try
                    {
                        BT.ApplyToBlockProjectile();
                    }
                    catch (Exception e)
                    {
                        Debug.Log("TweakTech: ApplyBlockTweaks(FIRST INIT) - error " + e);
                    }
                }
                appliedFirstTweaks = true;
            }
            else
            {
                foreach (BlockTweak BT in Tweaks.BlockTweaks)
                {
                    try
                    {
                        BT.ApplyToBlock();
                    }
                    catch (Exception e)
                    {
                        Debug.Log("TweakTech: ApplyBlockTweaks - error " + e);
                    }
                }
            }
            Debug.Log("TweakTech: ApplyBlockTweaks - Done building");
            appliedTweaks = true;
        }
        internal static void RemoveBlockTweaks()
        {
            if (!appliedTweaks)
                return;
            Debug.Log("TweakTech: RemoveBlockTweaks - Initializing");
            foreach (BlockTweak BT in Tweaks.BlockTweaks)
            {
                try
                {
                    WeaponTweak.ResetBPrefab(BT.ResetApplyToBlock(), BT.Type);
                }
                catch (Exception e)
                {
                    Debug.Log("TweakTech: RemoveBlockTweaks - error " + e);
                }
            }
            Debug.Log("TweakTech: RemoveBlockTweaks - Done building");
            appliedTweaks = false;
        }

        private static FieldInfo damageChart = typeof(ManDamage).GetField("m_DamageMultiplierTable", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo damageVals = typeof(DamageMultiplierTable).GetField("m_DamageTypeMultiplierLookup", BindingFlags.NonPublic | BindingFlags.Instance);
        private static void ApplyDamageTweaks()
        {
            DamageMultiplierTable DMT = (DamageMultiplierTable)damageChart.GetValue(Singleton.Manager<ManDamage>.inst);
            float[] arraydmge = (float[])damageVals.GetValue(DMT);

            /*
            float gdm = 0.65f;

            for (int step = 0; step < Enum.GetNames(typeof(ManDamage.DamageableType)).Length; step++)
            {
                for (int step2 = 0; step2 < Enum.GetNames(typeof(ManDamage.DamageType)).Length; step2++)
                {
                    arraydmge[(int)((int)step2 * ManDamage.NumDamageableTypes + step)] *= gdm;
                }
            }*/

            ReformatMultipliers(ref arraydmge);
            List<DamageTweak> allTweaks = Tweaks.DMGTweaks;
            //allTweaks.AddRange(Tweaks.DMGTweaksVanilla);
            foreach (DamageTweak DT in allTweaks)
            {
                arraydmge[(int)((int)DT.Dealer * ManDamage.NumDamageableTypes + DT.Taker)] = DT.Multiplier;
            }

            damageVals.SetValue(DMT, arraydmge);
        }
        private static void ReformatMultipliers(ref float[] arraydmge)
        {
            if (AdditionalDamageTypes < 1)
                return;
            int count = arraydmge.Length;
            float[] arraydmge2 = new float[count + (ManDamage.NumDamageableTypes * AdditionalDamageTypes)];

            for (int step = 0; step < count; step++)
            {
                arraydmge2[step] = arraydmge[step];
            }
            int extendedSet = arraydmge2.Length;
            for (int step = count; step < extendedSet; step++)
            {
                arraydmge2[step] = 1;
            }
            arraydmge = arraydmge2;
        }
    }

    internal enum ProjSpeedChange
    {
        Default,
        Slow,
        Fast,
        SlowedFast,
    }


    internal class FDBookmark : MonoBehaviour
    {
        internal static bool startWorking = false;
        internal bool DoReset = false;
        internal TankBlock Block;
        internal WeaponRound Main;
        internal static List<BlockTypes> reformatted;
        private short delay = 4;

        static FieldInfo FDaG = typeof(ModuleWeaponGun).GetField("m_FiringData", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void EnableAll()
        {
            ChangePatcher.ApplyBlockTweaks();
            reformatted = new List<BlockTypes>();
            List<int> names = new List<int>();
            foreach (FDBookmark FDB in Resources.FindObjectsOfTypeAll<FDBookmark>())
            {
                try
                {
                    TankBlock Block = FDB.GetComponent<TankBlock>();
                    TankBlock blockF = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Block.BlockType);
                    FDB.Block = Block;
                    if (!reformatted.Contains(Block.BlockType))
                    {
                        var BT = Tweaks.BlockTweaks.Find(delegate (BlockTweak cand) { return cand.Type == Block.BlockType; });
                        if (BT != null)
                        {
                            names.Add(ChangePatcher.ApplyFireRateBlockTweakActive(Block.BlockType, BT).GetHashCode());
                        }
                        else
                            names.Add(ChangePatcher.ApplyFireRateBlockTweakActive(Block.BlockType, null).GetHashCode());
                        reformatted.Add(Block.BlockType);

                    }
                    if (Block != blockF)
                    {
                        FDB.DoReset = true;
                        FDB.ApplyChanges(blockF);
                    }
                }
                catch { }
            }
            Debug.Log("TweakTech: ApplyFireRateBlockTweakActive - changed " + ChangePatcher.timesFired);
            ChangePatcher.timesFired = 0;
            foreach (WeaponRound WR2 in Resources.FindObjectsOfTypeAll<WeaponRound>())
            {
                try
                {
                    int hash = WR2.name.GetHashCode();
                    if (names.Contains(hash))
                    {
                        BlockTypes BT2 = reformatted[names.IndexOf(hash)];
                        ChangePatcher.CopyFireRateBlockTweakToPRESENT(BT2, WR2);
                    }
                }
                catch { }
            }
            Debug.Log("TweakTech: CopyFireRateBlockTweakToPRESENT - changed " + ChangePatcher.timesFired);
            ChangePatcher.timesFired = 0;
            reformatted = null;
        }
        public static void DisableAll()
        {
            ChangePatcher.RemoveBlockTweaks();
            reformatted = new List<BlockTypes>();
            List<int> names = new List<int>();
            foreach (FDBookmark FDB in Resources.FindObjectsOfTypeAll<FDBookmark>())
            {
                try
                {
                    TankBlock Block = FDB.GetComponent<TankBlock>();
                    TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Block.BlockType);
                    FDB.Block = Block;
                    if (!reformatted.Contains(Block.BlockType))
                    {
                        var BT = Tweaks.BlockTweaks.Find(delegate (BlockTweak cand) { return cand.Type == Block.BlockType; });
                        if (BT != null)
                        {
                            names.Add(ChangePatcher.ResetBlockTweakActive(Block.BlockType, BT).GetHashCode());
                        }
                        else
                            names.Add(ChangePatcher.ResetBlockTweakActive(Block.BlockType, null).GetHashCode());
                        reformatted.Add(Block.BlockType);

                    }
                    if (Block != prefab)
                    {
                        FDB.DoReset = true;
                        FDB.ResetChanges(prefab);
                    }
                }
                catch { }
            }
            Debug.Log("TweakTech: ResetBlockTweakActive - changed " + ChangePatcher.timesFired);
            ChangePatcher.timesFired = 0;
            foreach (WeaponRound WR2 in Resources.FindObjectsOfTypeAll<WeaponRound>())
            {
                try
                {
                    int hash = WR2.name.GetHashCode();
                    if (names.Contains(hash))
                    {
                        BlockTypes BT2 = reformatted[names.IndexOf(hash)];
                        ChangePatcher.ResetFireRateBlockTweakToPRESENT(BT2, WR2);
                    }
                }
                catch { }
            }
            Debug.Log("TweakTech: ResetFireRateBlockTweakToPRESENT - changed " + ChangePatcher.timesFired);
            ChangePatcher.timesFired = 0;
            reformatted = null;
        }

        public void Update()
        {
            if (delay <= 0)
            {
                try
                {
                    DelayedTrigger();
                }
                catch { }
                enabled = false;
                //Destroy(this);
            }
            else
                delay--;
        }
        public void DelayedTrigger()
        {
            if (!startWorking || !gameObject.activeSelf)
                return;
            Block = GetComponent<TankBlock>();
            TankBlock blockF = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Block.BlockType);

            var FD = blockF.GetComponent<FDBookmark>();
            if ((bool)FD)
            {
                if (Block == null)
                {
                    //Debug.Log("TweakTech: FDBookmark - BLOCK IS NULL!!!");
                    return;
                }
                if (FD.Main == null)
                {
                    //Debug.Log("TweakTech: FDBookmark - Init for block " + Block.name);
                    FD.Main = WeaponTweak.GetOrSetBPrefab(Block, Block.BlockType);
                }
                ApplyChanges(blockF);
            }
            else
                Destroy(this);
        }
        public void ApplyChanges(TankBlock prefab)
        {
            var FD = prefab.GetComponent<FDBookmark>();
            if (Block == prefab)
            {
                //Debug.Log("TweakTech: FDBookmark - First call for block " + Block.name);
            }
            else
            {
                Block = GetComponent<TankBlock>();
                //Debug.Log("TweakTech: FDBookmark - Tweaked block " + Block.name);
                var MWG = Block.GetComponent<ModuleWeaponGun>();
                MWG.GetVelocity();
                try
                {
                    var FDa = (FireData)FDaG.GetValue(MWG);
                    FDa.m_BulletPrefab = WeaponTweak.GetOrSetBPrefab(prefab, Block.BlockType);
                    //Debug.Log("TweakTech: ApplyChanges - Block " + Block.name.ToString() + " prefab changed to " + FDa.m_BulletPrefab.gameObject.name);
                    if (FDa.gameObject != Block.gameObject)
                        Debug.Log("TweakTech: FDBookmark - Instance failiure " + Block.name);
                }
                catch
                {
                    Debug.Log("TweakTech: FDBookmark - FIREDATA IS ILLEGALLY NULL " + Block.name);
                }

                //Debug.Log("TweakTech: FDBookmark - 1");
                var MWOG = prefab.GetComponent<ModuleWeapon>();
                var MW = Block.GetComponent<ModuleWeapon>();
                var MWGOG = prefab.GetComponent<ModuleWeaponGun>();
                if ((bool)MW && (bool)MWOG)
                {   // reduce firerates
                    //Debug.Log("TweakTech: FDBookmark - 2");
                    if ((bool)MWG && (bool)MWGOG)
                    {
                        //Debug.Log("TweakTech: FDBookmark - 3");
                        MWG.m_ShotCooldown = MWGOG.m_ShotCooldown;
                        MWG.m_BurstCooldown = MWGOG.m_BurstCooldown;
                        MW.m_ShotCooldown = MWOG.m_ShotCooldown;

                        //Debug.Log("TweakTech: FDBookmark - 4");
                        if (MWGOG.m_ShotCooldown >= ChangePatcher.FirerateReductionMin)
                            return;
                        float adjust = ChangePatcher.ProjectileReduction;
                        if (ChangePatcher.MaximumFireRateAdjust)
                        {
                            adjust = ChangePatcher.FirerateReductionMin / Mathf.Max(MWGOG.m_ShotCooldown, 0.01f);
                        }

                        //Debug.Log("TweakTech: FDBookmark - 7");

                        MWG.m_ShotCooldown *= adjust;
                        MWG.m_BurstCooldown *= adjust;
                        MW.m_ShotCooldown *= adjust;

                    }
                }
            }
            enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefab">Prefab</param>
        public void ResetChanges(TankBlock prefab)
        {
            var FD = prefab.GetComponent<FDBookmark>();
            if (Block == prefab)
            {
                //Debug.Log("TweakTech: FDBookmark - First call for block " + Block.name);
            }
            else
            {
                Block = GetComponent<TankBlock>();
                //Debug.Log("TweakTech: FDBookmark - Tweaked block " + Block.name);
                var MWG = Block.GetComponent<ModuleWeaponGun>();
                MWG.GetVelocity();
                try
                {
                    var FDa = (FireData)FDaG.GetValue(MWG);
                    FDa.m_BulletPrefab = prefab.GetComponent<FireData>().m_BulletPrefab;
                    Debug.Log("TweakTech: FDBookmark - Reset " + Block.name + " projectile to " + FDa.m_BulletPrefab.name);
                }
                catch
                {
                    Debug.Log("TweakTech: FDBookmark - FIREDATA IS ILLEGALLY NULL " + Block.name);
                }

                //Debug.Log("TweakTech: FDBookmark - 1");
                var MW = Block.GetComponent<ModuleWeapon>();
                var MWOG = prefab.GetComponent<ModuleWeapon>();
                var MWGOG = prefab.GetComponent<ModuleWeaponGun>();
                if ((bool)MW && (bool)MWOG)
                {   // reduce firerates
                    if ((bool)MWG && (bool)MWGOG)
                    {
                        MWG.m_ShotCooldown = MWGOG.m_ShotCooldown;
                        MWG.m_BurstCooldown = MWGOG.m_BurstCooldown;
                        MW.m_ShotCooldown = MWOG.m_ShotCooldown;

                        if (DoReset)
                        {
                            var BT = Tweaks.BlockTweaks.Find(delegate (BlockTweak cand) { return cand.Type == Block.BlockType; });
                            if (BT != null)
                            {
                                BT.ResetBlock(Block);
                            }
                        }
                        DoReset = false;
                    }
                }
            }
            enabled = false;
        }
    }
}
