using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RandomAdditions;

namespace TweakTech
{
    internal class BTBookmark : MonoBehaviour
    {
        internal static bool startWorking = false;
        internal static bool Adding = true;
        internal static List<BlockTypes> reformatted;
        internal TankBlock Block;
        internal string MainType;
        internal BlockTweak Main;
        private short delay = 2;
        public static void EnableAll()
        {
            Adding = true;
            reformatted = new List<BlockTypes>();
            foreach (BTBookmark BTB in Resources.FindObjectsOfTypeAll<BTBookmark>())
            {
                try
                {
                    TankBlock Block = BTB.GetComponent<TankBlock>();
                    TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Block.BlockType);
                    if (prefab == Block)
                    {
                        BTB.enabled = true;
                        return;
                    }
                    BTB.Block = Block;
                    BTB.Main.ChangeBlock(Block);
                    //var FDB = BTB.GetComponent<FDBookmark>();
                    //if (FDB)
                    //    FDB.ApplyChanges(prefab);
                }
                catch { }
            }
            Debug.Log("TweakTech: BTBookmark.EnableAll - changed " + ChangePatcher.timesFired);
            ChangePatcher.timesFired = 0;
            reformatted = null;
        }
        public static void DisableAll()
        {
            Adding = false;
            reformatted = new List<BlockTypes>();
            foreach (BTBookmark BTB in Resources.FindObjectsOfTypeAll<BTBookmark>())
            {
                try
                {
                    TankBlock Block = BTB.GetComponent<TankBlock>();
                    TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Block.BlockType);
                    if (prefab == Block)
                    {
                        return;
                    }
                    BTB.Block = Block;
                    BTB.Main.ResetBlock(Block);
                    //var FDB = BTB.GetComponent<FDBookmark>();
                    //if (FDB)
                    //    FDB.ResetChanges(prefab);
                }
                catch { }
            }
            Debug.Log("TweakTech: BTBookmark.EnableAll - changed " + ChangePatcher.timesFired);
            ChangePatcher.timesFired = 0;
            reformatted = null;
        }

        public void OnPool()
        {
            //DelayedTrigger();
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
            //DelayedTrigger();
        }
        public void DelayedTrigger()
        {
            if (!startWorking || !gameObject.activeSelf)
                return;
            Block = GetComponent<TankBlock>();
            TankBlock blockF = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Block.BlockType);

            var BT = blockF.GetComponent<BTBookmark>();
            if ((bool)BT)
            {
                if (Block == null)
                {
                    //Debug.Log("TweakTech: BTBookmark - BLOCK IS NULL!!!");
                    return;
                }
                if (BT.Main == null)
                {
                    //Debug.Log("TweakTech: BTBookmark - Init for block " + Block.name);
                    BT.Main = Tweaks.BlockTweaks.Find(delegate (BlockTweak cand) { return cand.Type == Block.BlockType; });
                }
                /*
                Debug.Log("TweakTech: BTBookmark - Setting up for modded block " + Block.name + " based on " + BT.MainType);
                if (Block.name != BT.MainType)
                {
                    Debug.Log("TweakTech: BTBookmark - Setting up for modded block " + Block.name + " based on " + BT.MainType);
                    Destroy(BT);
                    return;
                }*/
                if (Block == blockF)
                {
                    //Debug.Log("TweakTech: BTBookmark - First call for block " + Block.name);
                }
                else
                {
                    Debug.Log("TweakTech: BTBookmark - Tweaked block " + Block.name);
                    if (Adding)
                        BT.Main.ChangeBlock(Block);
                }
            }
            else
                Destroy(this);
        }

    }

    internal class BlockTweak
    {
        internal BlockTypes Type = BlockTypes.GSOAIController_111;

        internal float HPChange = 0;
        internal float FragilityReplace = -1;

        internal bool ChangeDamageableType = false;
        internal ManDamage.DamageableType NewDamageableType = ManDamage.DamageableType.Standard;

        internal WeaponTweak WT;
        internal ScannerTweak ST;
        /// <summary>
        /// Prefab, AlreadyDid, the BlockType
        /// </summary>
        internal Action<TankBlock, bool, BlockTypes> MiscChanges;

        internal TankBlock ApplyToBlockProjectile()
        {
            TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Type);
            if ((bool)prefab)
            {
                BTBookmark BTB = prefab.GetComponent<BTBookmark>();
                if (!BTB)
                {
                    BTB = prefab.gameObject.AddComponent<BTBookmark>();
                    BTB.Main = this;
                    BTB.MainType = prefab.gameObject.name;
                }
                int stepError = 0;
                try
                {
                    //Debug.Log("TweakTech: ApplyToBlock.PrepBlock - Loading for type " + Type);
                    var FD = prefab.gameObject.GetComponent<FireData>();
                    if ((bool)FD)
                        if (FD.m_BulletPrefab != null)
                            WeaponTweak.GetOrSetBPrefab(prefab, Type);
                    stepError++;
                    if (WT != null)
                        WT.ChangeWeapon(prefab, Type);
                    stepError++;
                    // Additional changes (technical)\
                    MiscChanges?.Invoke(prefab, false, Type);
                }
                catch (Exception e)
                {
                    Debug.Log("TweakTech: BlockTweak.ApplyToBlock.PrepBlock - error " + stepError + " | " + e);
                }
            }
            return prefab;
        }
        internal TankBlock ApplyToBlock()
        {
            TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Type);
            if ((bool)prefab)
            {
                BTBookmark BTB = prefab.GetComponent<BTBookmark>();
                if (!BTB)
                {
                    BTB = prefab.gameObject.AddComponent<BTBookmark>();
                    BTB.Main = this;
                    BTB.MainType = prefab.gameObject.name;
                }
                int stepError = 0;
                try
                {
                    //Debug.Log("TweakTech: ApplyToBlock.PrepBlock - Loading for type " + Type);
                    var FD = prefab.gameObject.GetComponent<FireData>();
                    if ((bool)FD)
                        if (FD.m_BulletPrefab != null)
                            WeaponTweak.GetOrSetBPrefab(prefab, Type);
                }
                catch (Exception e)
                {
                    Debug.Log("TweakTech: BlockTweak.ApplyToBlock.PrepBlock - error " + stepError + " | " + e);
                }
            }
            return prefab;
        }
        internal bool ChangeBlock(TankBlock blockI)
        {
            int stepError = 0;
            TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Type);
            Debug.Log("TweakTech: ApplyBlockTweaks - Changing " + blockI.name);
            try
            {
                var HPOG = prefab.GetComponent<Damageable>();
                var HP = blockI.GetComponent<Damageable>();
                if (HPChange != 0)
                {
                    float HPChanged = HPOG.MaxHealth + HPChange;
                    if (HPChanged > 0)
                        HP.SetMaxHealth(HPChanged);
                }
                stepError++;
                if (ChangeDamageableType)
                    HP.DamageableType = NewDamageableType;
                stepError++;
                if (FragilityReplace != -1)
                    blockI.damage.m_DamageDetachFragility = FragilityReplace;
                stepError++;
                if (WT != null)
                    WT.ChangeWeapon(blockI);
                stepError++;
                if (ST != null)
                    ST.ChangeScanner(blockI);
                stepError++;
                // Additional changes (technical)\
                MiscChanges?.Invoke(blockI, true, BlockTypes.GSOAIController_111);
                var FD = blockI.GetComponent<FireData>();
                if ((bool)FD)
                    if (FD.m_BulletPrefab != null)
                        FD.m_BulletPrefab = WeaponTweak.GetOrSetBPrefab(blockI);
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: BlockTweak.ChangeBlock - error " + stepError + " " + e);
            }
            return blockI;
        }
        /// <summary>
        /// Resets
        /// </summary>
        /// <returns>Returns the prefab</returns>
        internal TankBlock ResetApplyToBlock()
        {
            TankBlock block = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Type);
            if ((bool)block)
            {
                try
                {
                    if (WT != null)
                        WT.ResetWeapon(block, Type);
                }
                catch (Exception e)
                {
                    Debug.Log("TweakTech: BlockTweak.ResetApplyToBlock - " + e);
                }
            }
            return block;
        }
        internal bool ResetBlock(TankBlock blockI)
        {
            int stepError = 0;
            TankBlock prefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(Type);
            //Debug.Log("TweakTech: ApplyBlockTweaks - Changing " + blockI.name);
            try
            {
                var HPOG = prefab.GetComponent<Damageable>();
                var HP = blockI.GetComponent<Damageable>();
                if (HPChange != 0)
                {
                    float HPChanged = HPOG.MaxHealth;
                    if (HPChanged > 0)
                        HP.SetMaxHealth(HPChanged);
                }
                stepError++;
                if (ChangeDamageableType)
                    HP.DamageableType = HPOG.DamageableType;
                stepError++;
                if (FragilityReplace != -1)
                    blockI.damage.m_DamageDetachFragility = prefab.damage.m_DamageDetachFragility;
                stepError++;
                if (WT != null)
                    WT.ResetWeapon(blockI, prefab);
                stepError++;
                if (ST != null)
                    ST.ResetScanner(blockI, prefab);
                var FD = blockI.GetComponent<FireData>();
                if ((bool)FD)
                    if (FD.m_BulletPrefab != null)
                        FD.m_BulletPrefab = prefab.GetComponent<FireData>().m_BulletPrefab;
            }
            catch
            {
                Debug.Log("TweakTech: BlockTweak.ChangeBlock(RESET) - error " + stepError);
            }
            return blockI;
        }


        internal static void ApplyToBlockLocal(TankBlock TB)
        {
            BlockTweak BT = Tweaks.BlockTweaks.Find(delegate(BlockTweak BT1) { return BT1.Type == TB.BlockType; });
            if (BT != null)
            {
                BTBookmark BTB = TB.GetComponent<BTBookmark>();
                if (!BTB)
                {
                    BTB = TB.gameObject.AddComponent<BTBookmark>();
                    BTB.Main = BT;
                    BTB.MainType = TB.gameObject.name;
                }
                //BT.ChangeBlock(TB);
            }
        }
    }

    internal class ScannerTweak
    {
        internal float OverrideVision = 0;

        internal TankBlock ChangeScanner(TankBlock block)
        {
            try
            {
                var MV = block.GetComponent<ModuleVision>(); 
                if ((bool)MV)
                {
                    if (OverrideVision > 0)
                        MV.visionRange = OverrideVision;
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: ApplyBlockTweaks - fail in changing " + block.name + "  | " + e);
            }
            return block;
        }
        internal TankBlock ResetScanner(TankBlock block, TankBlock refBlock)
        {
            try
            {
                var MV = block.GetComponent<ModuleVision>();
                if ((bool)MV)
                {
                    MV.visionRange = refBlock.GetComponent<ModuleVision>().visionRange;
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: ApplyBlockTweaks - fail in changing " + block.name + "  | " + e);
            }
            return block;
        }
    }

}
