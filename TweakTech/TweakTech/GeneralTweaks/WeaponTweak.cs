using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TweakTech
{
    internal class WeaponTweak
    {
        internal static Dictionary<BlockTypes, WeaponRound> cacheProjEdits = new Dictionary<BlockTypes, WeaponRound>();
        internal static Dictionary<BlockTypes, Transform> cacheExplEdits = new Dictionary<BlockTypes, Transform>();


        internal ProjSpeedChange ProjSpeedChange = ProjSpeedChange.Default;
        internal float ProjSpeedChangeMulti = 1;
        internal float OverrideTraverse = 0;
        internal float OverrideCooldown = 0;
        internal float OverrideBurstCooldown = 0;
        internal int OverrideBurstCount = -1;

        internal float DirectDamageMulti = -1;

        internal sbyte ChangeSeeking = -1;
        internal bool EditExplosion = false;
        internal float ExploDamgMulti = -1;
        internal float ExploPushMulti = -1;
        internal float ExploRadMulti = -1;


        internal static FieldInfo deals = typeof(WeaponRound).GetField("m_Damage", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static FieldInfo explode = typeof(Projectile).GetField("m_Explosion", BindingFlags.NonPublic | BindingFlags.Instance);
        /// <summary>
        /// Active
        /// </summary>
        /// <param name="block"></param>
        /// <param name="PrefabSet"></param>
        internal void ChangeWeapon(TankBlock block)
        {
            int stepError = 0;
            try
            {
                if (ProjSpeedChange != ProjSpeedChange.Default)
                    ChangeProjectileSpeed(block);
                stepError++;
                if (OverrideTraverse != 0)
                    OverrideTurretTraverse(block);
                stepError++;
                if (OverrideBurstCooldown > 0 || OverrideBurstCount >= 0 || OverrideCooldown > 0)
                    OverrideCooldowns(block);
                stepError++;
                if (ChangeSeeking == 0)
                    if (block.GetComponent<ModuleWeaponGun>())
                        block.GetComponent<ModuleWeaponGun>().m_SeekingRounds = false;
                stepError++;
                if (ChangeSeeking == 1)
                    if (block.GetComponent<ModuleWeaponGun>())
                        block.GetComponent<ModuleWeaponGun>().m_SeekingRounds = true;
            }
            catch (Exception e)
            {
                Debug.LogError("TweakTech: WeaponTweak.ChangeWeapon - error " + stepError + " " + e);
            }

        }
        internal void ResetWeapon(TankBlock block, TankBlock refBlock)
        {
            var MW = block.GetComponent<ModuleWeapon>();
            var OGMW = refBlock.GetComponent<ModuleWeapon>();
            if (ProjSpeedChange != ProjSpeedChange.Default)
            {
                var FD = block.GetComponent<FireData>();
                var FDOG = refBlock.GetComponent<FireData>();
                if ((bool)FD && (bool)FDOG)
                {
                    FD.m_MuzzleVelocity = FDOG.m_MuzzleVelocity;
                    ReAimer.CreateOrUpdateForBlock(block);
                }
                else
                {
                    Debug.Info("TweakTech: ChangeProjectileSpeed - Found no FireData in " + block.name + " to change");
                }
            }
            if (OverrideTraverse != 0)
            {
                if ((bool)MW && (bool)OGMW)
                {
                    MW.m_RotateSpeed = OGMW.m_RotateSpeed;
                    //Debug.Info("TweakTech: OverrideTurretTraverse - changed traverse in " + block.name + " to " + OverrideTraverse);
                }
                else
                {
                    Debug.Info("TweakTech: ResetWeapon - Found no ModuleWeapon in " + block.name + " to change");
                }
            }
            if (OverrideBurstCooldown > 0 || OverrideBurstCount >= 0 || OverrideCooldown > 0)
            {
                var MWG = block.GetComponent<ModuleWeaponGun>();
                var OGMWG = refBlock.GetComponent<ModuleWeaponGun>();
                if ((bool)MW && (bool)OGMW)
                {
                    MW.m_ShotCooldown = OGMW.m_ShotCooldown;
                    if ((bool)MWG && (bool)OGMWG)
                    {
                        MWG.m_ShotCooldown = OGMWG.m_ShotCooldown;
                        MWG.m_BurstCooldown = OGMWG.m_BurstCooldown;
                        MWG.m_BurstShotCount = OGMWG.m_BurstShotCount;
                        MWG.m_SeekingRounds = OGMWG.m_SeekingRounds;
                    }
                }
                else
                {
                    Debug.Log("TweakTech: ResetWeapon - Found no ModuleWeapon in " + block.name + " to change");
                }
            }
        }
        /// <summary>
        /// Setup only, does not change values
        /// </summary>
        internal void ChangeWeapon(TankBlock block, BlockTypes Override)
        {
            if (ProjSpeedChange != ProjSpeedChange.Default)
                ChangeProjectileSpeed(block, Override);
            try
            {
                var FD = block.GetComponent<FireData>();
                if ((bool)FD)
                {
                    if (FD.m_BulletPrefab)
                    {
                        WeaponRound BP = GetOrSetBPrefab(block, Override);
                        if ((bool)BP)
                        {
                            if (DirectDamageMulti != -1)
                                ApplyDirectDamageChange(BP, Override);
                            if (EditExplosion)
                                ApplyExplodeChange(block, Override);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("TweakTech: ApplyBlockTweaks - fail in changing " + block.name + "  | " + e);
            }
            return;
        }
        internal void ResetWeapon(TankBlock block, BlockTypes Override)
        {
            if (ProjSpeedChange != ProjSpeedChange.Default)
            {
                var FD = block.GetComponent<FireData>();
                if ((bool)FD)
                {
                    if (!KickStart.RandomAdditionsAvail)
                        return;
                    ReAimer.CreateOrUpdateForBlock(block);
                }
                else
                {
                    Debug.Log("TweakTech: ChangeProjectileSpeed - Found no FireData in " + block.name + " to change");
                }
            }
            try
            {
                var FD = block.GetComponent<FireData>();
                if ((bool)FD)
                {
                    TankBlock refBlock = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Override);
                    FD.m_BulletPrefab = refBlock.GetComponent<FireData>().m_BulletPrefab;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("TweakTech: ApplyBlockTweaks - fail in changing " + block.name + "  | " + e);
            }
            return;
        }
        internal void ApplyDirectDamageChange(WeaponRound WR, BlockTypes Override)
        {
            TankBlock refBlock = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Override);
            var WROG = refBlock.GetComponent<FireData>().m_BulletPrefab;
            deals.SetValue(WR, (int)(DirectDamageMulti * (int)deals.GetValue(WROG)));
        }
        internal void ApplyExplodeChange(TankBlock PJ, BlockTypes Override)
        {
            try
            {
                TankBlock refBlock = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Override);
                var refSplosion = GetExplosion(refBlock);
                var trans = GetOrSetEPrefab(PJ, Override);
                var retrofit = trans.GetComponent<Explosion>();
                if (!(bool)retrofit)
                    return;
                if (ExploDamgMulti != -1)
                    retrofit.m_MaxDamageStrength = refSplosion.m_MaxDamageStrength * ExploDamgMulti;
                if (ExploPushMulti != -1)
                    retrofit.m_MaxImpulseStrength = refSplosion.m_MaxImpulseStrength * ExploPushMulti;
                if (ExploRadMulti != -1)
                    retrofit.m_EffectRadius = refSplosion.m_EffectRadius * ExploRadMulti;
                explode.SetValue(GetOrSetBPrefab(PJ, Override), trans);
            }
            catch { }
        }
        private void OverrideTurretTraverse(TankBlock block)
        {
            if (!KickStart.RandomAdditionsAvail)
                return;
            var MW = block.GetComponent<ModuleWeapon>();
            if ((bool)MW)
            {
                MW.m_RotateSpeed = OverrideTraverse;
                Debug.Info("TweakTech: OverrideTurretTraverse - changed traverse in " + block.name + " to " + OverrideTraverse);
            }
            else
            {
                Debug.Info("TweakTech: OverrideTurretTraverse - Found no ModuleWeapon in " + block.name + " to change");
            }
        }
        private void OverrideCooldowns(TankBlock block)
        {
            if (!KickStart.RandomAdditionsAvail)
                return;
            var MW = block.GetComponent<ModuleWeapon>();
            var MWG = block.GetComponent<ModuleWeaponGun>();
            if ((bool)MW)
            {
                if (OverrideCooldown > 0 && OverrideCooldown < MW.m_ShotCooldown)
                    MW.m_ShotCooldown = OverrideCooldown;
                if ((bool)MWG)
                {
                    if (OverrideCooldown > 0)
                        MWG.m_ShotCooldown = OverrideCooldown;
                    if (OverrideBurstCooldown > 0)
                        MWG.m_BurstCooldown = OverrideBurstCooldown;
                    if (OverrideBurstCount >= 0)
                        MWG.m_BurstShotCount = OverrideBurstCount;
                }
            }
            else
            {
                Debug.Log("TweakTech: OverrideCooldowns - Found no ModuleWeapon in " + block.name + " to change");
            }
        }

        private static FieldInfo lifeTime = typeof(Projectile).GetField("m_LifeTime", BindingFlags.NonPublic | BindingFlags.Instance);
        private void ChangeProjectileSpeed(TankBlock block)
        {
            TankBlock refBlock = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(block.BlockType);
            var FDOG = refBlock.GetComponent<FireData>();
            var FD = block.GetComponent<FireData>();
            if ((bool)FD && (bool)FDOG)
            {
                switch (ProjSpeedChange)
                {
                    case ProjSpeedChange.Fast:
                        FD.m_MuzzleVelocity = FDOG.m_MuzzleVelocity * ProjSpeedChangeMulti;
                        return;
                    case ProjSpeedChange.Slow:
                        FD.m_MuzzleVelocity = FDOG.m_MuzzleVelocity * ProjSpeedChangeMulti;
                        ReAimer.CreateOrUpdateForBlock(block);
                        return;
                    case ProjSpeedChange.SlowedFast:
                        FD.m_MuzzleVelocity = FDOG.m_MuzzleVelocity * ProjSpeedChangeMulti;
                        FD.m_MuzzleVelocity *= ChangePatcher.HighExplosiveSpeedMulti;
                        ReAimer.CreateOrUpdateForBlock(block);
                        return;
                }
            }
            else
            {
                Debug.Log("TweakTech: ChangeProjectileSpeed - Found no FireData in " + block.name + " to change");
            }
        }
        /// <summary>
        /// Setup only, does not change values
        /// </summary>
        private void ChangeProjectileSpeed(TankBlock block, BlockTypes Override)
        {
            var FD = block.GetComponent<FireData>();
            if ((bool)FD)
            {
                WeaponRound BP = GetOrSetBPrefab(block, Override);
                WeightedProjectile WProj = BP.gameObject.GetComponent<WeightedProjectile>();
                switch (ProjSpeedChange)
                {
                    case ProjSpeedChange.Slow:
                        if (KickStart.RandomAdditionsAvail)
                        {
                            if (!(bool)WProj)
                            {
                                WProj = BP.gameObject.AddComponent<WeightedProjectile>();
                                //Debug.Log("TweakTech: ChangeProjectileSpeed - Added WeightedProjectile to " + block.name);
                            }
                            WProj.CustomGravity = true;
                            WProj.CustomGravityFractionSpeed = false;
                            WProj.ProjectileMass = 0.00123f;
                            //FD.m_MuzzleVelocity *= ProjSpeedChangeMulti;
                            WProj.GravityAndSpeedScale = ProjSpeedChangeMulti;
                            ReAimer.CreateOrUpdateForBlock(block);
                            if ((bool)BP)
                            {
                                var Proj = BP.GetComponent<Projectile>();
                                if ((bool)Proj)
                                    lifeTime.SetValue(Proj, (float)lifeTime.GetValue(Proj) / ProjSpeedChangeMulti);
                            }
                            //Debug.Log("TweakTech: AddSlowProjectile - Could not fetch ReAimer in " + block.name + " to change");
                        }
                        return;
                    case ProjSpeedChange.Fast:
                        //FD.m_MuzzleVelocity *= ProjSpeedChangeMulti;
                        if ((bool)BP)
                        {
                            var Proj = BP.GetComponent<Projectile>();
                            if ((bool)Proj)
                            {
                                lifeTime.SetValue(Proj, (float)lifeTime.GetValue(Proj) / ProjSpeedChangeMulti);
                                //Debug.Log("TweakTech: ChangeProjectileSpeed - Changed LifeTime in " + FD.gameObject.name);
                            }
                        }
                        return;
                    case ProjSpeedChange.SlowedFast:
                        //FD.m_MuzzleVelocity *= ProjSpeedChangeMulti;
                        if ((bool)BP)
                        {
                            var Proj = BP.GetComponent<Projectile>();
                            if ((bool)Proj)
                                lifeTime.SetValue(Proj, ((float)lifeTime.GetValue(Proj) / ProjSpeedChangeMulti) / ChangePatcher.HighExplosiveSpeedMulti);
                        }

                        if (KickStart.RandomAdditionsAvail)
                        {
                            if (!(bool)WProj)
                            {
                                WProj = BP.gameObject.AddComponent<WeightedProjectile>();
                                //Debug.Log("TweakTech: ChangeProjectileSpeed - Added WeightedProjectile to " + block.name);
                            }
                            WProj.CustomGravity = true;
                            WProj.CustomGravityFractionSpeed = false;
                            WProj.ProjectileMass = 0.00123f;
                            //FD.m_MuzzleVelocity *= ChangePatcher.HighExplosiveSpeedMulti;
                            WProj.GravityAndSpeedScale = ChangePatcher.HighExplosiveSpeedMulti;
                            ReAimer.CreateOrUpdateForBlock(block);
                            //Debug.Log("TweakTech: AddSlowProjectile - Could not fetch ReAimer in " + block.name + " to change");
                        }
                        return;
                }
            }
            else
            {
                Debug.Log("TweakTech: ChangeProjectileSpeed - Found no FireData in " + block.name + " to change");
            }
        }

        /// <summary>
        /// Checks if there is a bullet prefab stored for this mod
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        internal static bool IsPrefabSet(TankBlock block)
        {
            BlockTypes type = block.BlockType;
            return cacheProjEdits.ContainsKey(type);
        }
        /// <summary>
        /// Creates a seperate bullet prefab for this mod to tweak from existing bullet prefabs and then sets that to the block.
        /// DO NOT USE THIS TO SET THE BASE PREFAB
        /// </summary>
        /// <param name="block">Where to get the default prefab</param>
        /// <param name="Override">In case we can't easily get the BlockType</param>
        /// <returns></returns>
        internal static WeaponRound GetOrSetBPrefab(TankBlock originalPrefab, BlockTypes Override = BlockTypes.GSOAnchorAI_121)
        {
            if (cacheProjEdits.TryGetValue(Override, out WeaponRound val))
            {
                return GetOGBPrefab(val);
            }
            TankBlock refBlock = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Override);
            var FD2 = refBlock.GetComponent<FireData>();
            if ((bool)FD2)
            {
                var GON = UnityEngine.Object.Instantiate(GetOGBPrefab(FD2.m_BulletPrefab).gameObject, null);
                GON.SetActive(false);
                GON.name = GON.name + "_TWEAKTECH_EDIT";
                var newBullet = GON.GetComponent<WeaponRound>();
                //var Inst = WR.Spawn(Singleton.dynamicContainer, Vector3.zero, Quaternion.identity);
                //Inst.gameObject.SetActive(false);

                /*
                bool hadToEnable = false;
                if (Singleton.Manager<ComponentPool>.inst.DisableInitPools)
                {
                    Singleton.Manager<ComponentPool>.inst.DisableInitPools = false;
                    hadToEnable = true;
                }
                WR.CreatePool(8);
                if (hadToEnable)
                    Singleton.Manager<ComponentPool>.inst.DisableInitPools = true;
                */
                cacheProjEdits.Add(Override, newBullet);
                //Debug.Log("TweakTech: GetOrSetBPrefab - Registered " + Override + " | " + WR.name);
                //Set = false;
                if (FD2.m_BulletPrefab == newBullet)
                {
                    Debug.LogError("TweakTech: ASSERT - PREFAB WAS OVERWRITTEN!!! " + originalPrefab.name);
                }
                return newBullet;
            }
            else
            {
                //Debug.Log("TweakTech: GetOrSetBPrefab - NULL FireData in " + originalPrefab.name);
                return null;
            }
        }

        /// <summary>
        /// Replaces this mod's prefab pool with the specified bullet prefab
        /// </summary>
        /// <param name="BT">The BlockType this prefab is assigned to</param>
        /// <param name="WR">The bullet prefab to replace the previous with</param>
        internal static void SetBPrefab(BlockTypes BT, WeaponRound WR)
        {
            if (WR == null)
            {
                Debug.Log("TweakTech: SetBPrefab - Input WeaponRound for BlockType " + BT.ToString() + " is null!");
                return;
            }
            if (cacheProjEdits.ContainsKey(BT))
            {
                cacheProjEdits.Remove(BT);
                cacheProjEdits.Add(BT, GetOGBPrefab(WR));
                return;
            }
            Debug.Log("TweakTech: ASSERT - ILLEGAL INPUT EXCEPTION");
            foreach (KeyValuePair<BlockTypes, WeaponRound> check in cacheProjEdits)
            {
                Debug.Log(check.Key + " | " + check.Value.name.ToString());
            }
        }

        internal static Projectile GetProjectile(TankBlock block)
        {
            var FD2 = block.GetComponent<FireData>();
            if ((bool)FD2)
            {
                return FD2.m_BulletPrefab.GetComponent<Projectile>();
            }
            else
            {
                Debug.Log("TweakTech: GetProjectile - NULL FireData in " + block.name);
                return null;
            }
        }
        internal static Explosion GetExplosion(TankBlock block)
        {
            var FD2 = block.GetComponent<FireData>();
            if ((bool)FD2)
            {
                var Trans = (Transform)explode.GetValue(FD2.m_BulletPrefab.GetComponent<Projectile>());
                return Trans.GetComponent<Explosion>();
            }
            else
            {
                Debug.Log("TweakTech: GetExplosion - NULL FireData in " + block.name);
                return null;
            }
        }
        internal static Transform GetOrSetEPrefab(TankBlock block, BlockTypes Override = BlockTypes.GSOAnchorAI_121)
        {
            if (cacheExplEdits.TryGetValue(Override, out Transform val))
            {
                return GetOGEPrefab(val);
            }
            try
            {
                TankBlock refBlock = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Override);
                var FD2 = refBlock.GetComponent<FireData>();
                if ((bool)FD2)
                {
                    var PJ = FD2.m_BulletPrefab.GetComponent<Projectile>();
                    var GON = UnityEngine.Object.Instantiate(GetOGEPrefab((Transform)explode.GetValue(PJ)), null);
                    GON.gameObject.SetActive(false);
                    GON.name = GON.name + "_TWEAKTECH_EDIT";
                    //var Inst = GON.Spawn(Singleton.dynamicContainer, Vector3.zero, Quaternion.identity);
                    //Inst.gameObject.SetActive(false);
                    /*
                    bool hadToEnable = false;
                    if (Singleton.Manager<ComponentPool>.inst.DisableInitPools)
                    {
                        Singleton.Manager<ComponentPool>.inst.DisableInitPools = false;
                        hadToEnable = true;
                    }
                    GON.CreatePool(8);
                    if (hadToEnable)
                        Singleton.Manager<ComponentPool>.inst.DisableInitPools = true;
                    */
                    cacheExplEdits.Add(Override, GON);
                    //Set = false;
                    return GON;
                }
                else
                {
                    Debug.Log("TweakTech: GetOrSetEPrefab - NULL FireData in " + block.name);
                    return null;
                }
            }
            catch { }
            return null;
        }
        internal static WeaponRound GetOrSetBPrefab(TankBlock block)
        {
            //Set = true;
            BlockTypes type = block.BlockType;
            if (cacheProjEdits.TryGetValue(type, out WeaponRound val))
            {
                return GetOGBPrefab(val);
            }
            Debug.Log("TweakTech: GetOrSetBPrefab - FireData did not set properly for " + block.name);

            TankBlock refBlock = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(type);
            var FD = refBlock.GetComponent<FireData>();
            if ((bool)FD)
            {
                var GON = UnityEngine.Object.Instantiate(GetOGBPrefab(FD.m_BulletPrefab).gameObject, null);
                GON.SetActive(false);
                GON.name = GON.name + "_TWEAKTECH_EDIT";
                Debug.Log("TweakTech: GetOrSetBPrefab - Setup " + GON.name);
                var WR = GON.GetComponent<WeaponRound>();

                /*
                bool hadToEnable = false;
                if (Singleton.Manager<ComponentPool>.inst.DisableInitPools)
                {
                    Singleton.Manager<ComponentPool>.inst.DisableInitPools = false;
                    hadToEnable = true;
                }
                WR.CreatePool(8);
                if (hadToEnable)
                    Singleton.Manager<ComponentPool>.inst.DisableInitPools = true;
                */
                cacheProjEdits.Add(type, WR);
                Debug.Log("TweakTech: GetOrSetBPrefab - Registered " + type + " | " + WR.name);
                //Set = false;
                return WR;
            }
            else
            {
                //Debug.Log("TweakTech: GetOrSetBPrefab - NULL FireData in " + block.name);
                return null;
            }
        }
        internal static void ResetBPrefab(TankBlock block, BlockTypes type)
        {
            //Set = true;
            TankBlock refBlock = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(type);
            var FD = refBlock.GetComponent<FireData>();
            if (FD)
            {
                WeaponRound ogPrefab = GetOGBPrefab(FD.m_BulletPrefab);
                if (!ogPrefab)
                    return;
                /*
                if (cacheProjEdits.TryGetValue(type, out WeaponRound val))
                {
                    var WPj = val.GetComponent<WeightedProjectile>();
                    if (WPj)
                        UnityEngine.Object.Destroy(WPj);
                }*/
            }
            else
            {
                //Debug.Log("TweakTech: GetOrSetBPrefab - NULL FireData in " + block.name);
            }
        }
        private static WeaponRound GetOGBPrefab(WeaponRound val)
        {
            if (val == null)
            {
                //Debug.Log("TweakTech: GetOGBPrefab - Input value is null! " + StackTraceUtility.ExtractStackTrace());
                return null;
            }
            WeaponRound inst = Singleton.Manager<ComponentPool>.inst.GetOriginalPrefab(val);
            if (inst != null)
                return inst;
            else
            {
                /*
                bool hadToEnable = false;
                if (Singleton.Manager<ComponentPool>.inst.DisableInitPools)
                {
                    Singleton.Manager<ComponentPool>.inst.DisableInitPools = false;
                    hadToEnable = true;
                }
                val.CreatePool(8);
                if (hadToEnable)
                    Singleton.Manager<ComponentPool>.inst.DisableInitPools = true;
                */
            }
            return val;
        }
        private static Transform GetOGEPrefab(Transform val)
        {
            if (val == null)
            {
                Debug.Log("TweakTech: GetOGEPrefab - Input value is null!");
                return null;
            }
            //Set = true;
            Transform inst = Singleton.Manager<ComponentPool>.inst.GetOriginalPrefab(val);
            if (inst != null)
                return inst;
            else
            {
                /*
                bool hadToEnable = false;
                if (Singleton.Manager<ComponentPool>.inst.DisableInitPools)
                {
                    Singleton.Manager<ComponentPool>.inst.DisableInitPools = false;
                    hadToEnable = true;
                }
                val.CreatePool(8);
                if (hadToEnable)
                    Singleton.Manager<ComponentPool>.inst.DisableInitPools = true;
                */
            }
            return val;
        }
    }
}
