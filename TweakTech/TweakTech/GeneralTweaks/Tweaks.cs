using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RandomAdditions;

namespace TweakTech
{
    internal static class Tweaks
    {
        private static float BFLaserSpedBuff = 2;

        internal static List<BlockTweak> BlockTweaks = new List<BlockTweak>() { 
            // WEAPON BUFFS
            // GSO
            new BlockTweak {
                Type = BlockTypes.GSOAnchorFixed_111,
                HPChange = -1250,
            },
            new BlockTweak {
                Type = BlockTypes.GSOBigBertha_845,
                WT = new WeaponTweak {
                    OverrideTraverse = 40,
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 2,
                    EditExplosion = true,
                    ExploDamgMulti = 1.5f,
                    ExploPushMulti = 1.75f,
                    ExploRadMulti = 1.75f,
                }
            },
            new BlockTweak {
                Type = BlockTypes.GSOMGunFixed_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 1.75f,
                },
                //MiscChanges = SpecChanges.PointDefense,
            },
            new BlockTweak {
                Type = BlockTypes.GSOLaserFixed_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 1.6f,
                }
            },
            new BlockTweak {
                Type = BlockTypes.GSOLaserForward_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 1.75f,
                }
            },
            new BlockTweak {
                Type = BlockTypes.GSOCannonTurret_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 2,//5
                    OverrideTraverse = 125,
                }
            },
            new BlockTweak {
                Type = BlockTypes.GSOMortarFixed_211,
                WT = new WeaponTweak {
                    //ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1f,
                    DirectDamageMulti = 2.0f,
                    EditExplosion = true,
                    ExploDamgMulti = 2.0f,
                    ExploPushMulti = 1.5f,
                    ExploRadMulti = 1.5f,
                },
                //MiscChanges = SpecChanges.DoubleHoming,
            },
            new BlockTweak {
                Type = BlockTypes.GSOMegatonLong_242,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1,//3
                    OverrideTraverse = 65,
                }
            },
            new BlockTweak {
                Type = BlockTypes.GSOMediumCannon_222,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1,//2
                    OverrideTraverse = 75, // yes it's better than the Megaton
                }
            },
            new BlockTweak {
                Type = BlockTypes.GSOMiniMissile_222,
                WT = new WeaponTweak {
                    // Better against aircraft like in Sam Site
                    //ProjSpeedChange = ProjSpeedChange.Fast,
                    //ProjSpeedChangeMulti = 1.25f,
                    OverrideTraverse = 145,
                    //OverrideCooldown = 0.5f,
                    //OverrideBurstCooldown = 0.5f,
                }
            },
            // GC
            new BlockTweak {
                Type = BlockTypes.GC_NailGun_122,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 2,
                    DirectDamageMulti = 5,
                    OverrideBurstCount = 2,
                    OverrideCooldown = 0.01f,
                    OverrideBurstCooldown = 0.5f,
                }
            },
            new BlockTweak {
                Type = BlockTypes.GC_Catapult_234,
                WT = new WeaponTweak {
                    ChangeSeeking = 1,
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1,
                    DirectDamageMulti = 2.5f,
                    EditExplosion = true,
                    ExploDamgMulti = 2,
                    ExploPushMulti = 1.25f,
                    ExploRadMulti = 1.75f,
                }
            },
            new BlockTweak {
                Type = BlockTypes.GC_Mortar_424,
                WT = new WeaponTweak {
                    ChangeSeeking = 1,
                    //ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1,
                    DirectDamageMulti = 2.5f,
                    EditExplosion = true,
                    ExploDamgMulti = 1.5f,
                    ExploPushMulti = 2,
                    ExploRadMulti = 1.5f,
                },
                MiscChanges = SpecChanges.DoubleHoming,
            },
            // VEN
            new BlockTweak {
                Type = BlockTypes.VENMGun_111,
                //MiscChanges = SpecChanges.PointDefense,
            },
            new BlockTweak {
                Type = BlockTypes.VENLaserTurret_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 2.25f,
                }
            },
            new BlockTweak {
                Type = BlockTypes.VENLaserMachineGun_112,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 2.25f,
                }
            },
            /*
            new BlockTweak {
                Type = BlockTypes.VENTurretRoundabout_111,
                WT = new WeaponTweak {
                    //ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1f,
                    DirectDamageMulti = 2.0f,
                    EditExplosion = true,
                    ExploDamgMulti = 2.0f,
                    ExploPushMulti = 1.5f,
                    ExploRadMulti = 1.5f,
                },
                //MiscChanges = SpecChanges.DoubleHoming,
            },
            new BlockTweak {
                Type = BlockTypes.VENMGun_112,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 1.5f,
                }
            },*/
            new BlockTweak {
                Type = BlockTypes.VENFlameThrower_121,
                HPChange = 175, // Really weak as-is for a volitile melee weapon
            },
            /*
            new BlockTweak {
                Type = BlockTypes.VENRPGLauncher_122,
                HPChange = 125, // Buff due to weakening of tracking
                WT = new WeaponTweak {
                    OverrideCooldown = 0.4f,
                    OverrideBurstCooldown = 0.4f,
                    //ChangeSeeking = 0, // no seeking

                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 3,// FAST
                    OverrideTraverse = 120, //Kiting gun
                    
                    EditExplosion = true,
                    ExploDamgMulti = 2,
                    ExploPushMulti = 2,
                    ExploRadMulti = 1.75f,
                },
                MiscChanges = SpecChanges.NerfTracking,
            },*/
            new BlockTweak {
                Type = BlockTypes.VENCannonRapid_313,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1,
                    OverrideTraverse = 160, //Kiting gun
                }
            },
            new BlockTweak {
                Type = BlockTypes.VENMachineGunForward_311,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 2,
                    OverrideTraverse = 350, //AIM FAST
                    DirectDamageMulti = 1.5f,
                }
            },
            // HE
            new BlockTweak {
                Type = BlockTypes.HE_Suppressed_MGun_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 1.75f, //faster
                },
                //MiscChanges = SpecChanges.PointDefense,
            },
            new BlockTweak {
                Type = BlockTypes.HE_Chain_Gun_29_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 2f, //faster
                    OverrideTraverse = 90, //slower
                }
            },
            new BlockTweak {
                Type = BlockTypes.HE_MachineGun_33_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 1.25f, //faster
                    OverrideTraverse = 80, //slower
                }
            },
            /*
            new BlockTweak {
                Type = BlockTypes.HE_TurretRoundabout_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1f,
                    DirectDamageMulti = 2.0f,
                    EditExplosion = true,
                    ExploDamgMulti = 2.0f,
                    ExploPushMulti = 1.5f,
                    ExploRadMulti = 1.5f,
                },
                //MiscChanges = SpecChanges.DoubleHoming,
            },*/
            new BlockTweak {
                Type = BlockTypes.HE_Mortar_232,
                WT = new WeaponTweak {
                    //ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1f,
                    DirectDamageMulti = 2.0f,
                    EditExplosion = true,
                    ExploDamgMulti = 2.0f,
                    ExploPushMulti = 1.5f,
                    ExploRadMulti = 1.5f,
                },
                //MiscChanges = SpecChanges.DoubleHoming,
            },
            new BlockTweak {
                Type = BlockTypes.HE_CannonTurret_Short_525,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1,//3
                    OverrideTraverse = 65,
                }
            },
            new BlockTweak {
                Type = BlockTypes.HE_CannonBattleship_216,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1f,//1.25f
                    OverrideTraverse = 30,
                }
            },
            new BlockTweak {
                Type = BlockTypes.HE_Cannon_Naval_826,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.SlowedFast,
                    ProjSpeedChangeMulti = 1,
                    OverrideTraverse = 50,
                    OverrideBurstCount = 3,
                    OverrideBurstCooldown = 1.75f,
                    OverrideCooldown = 0.1f,
                }
            },
            /*
            new BlockTweak {
                Type = BlockTypes.HE_MissilePod_122,
                WT = new WeaponTweak {
                    ChangeSeeking = 0, // For first phase
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 1.75f,
                },
                MiscChanges = SpecChanges.MultiStageWeapon,
            },*/
            new BlockTweak {
                // Overhaul cruise missile to have same range, but slower travel speed to permit dodge
                Type = BlockTypes.HE_Cruise_Missile_51_121,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = 1.25f, // need more launching power
                },
                MiscChanges = SpecChanges.NerfCruiseMissile,
            },
            // BF
            new BlockTweak {
                Type = BlockTypes.BF_Laser_112,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = BFLaserSpedBuff,
                },
                //MiscChanges = SpecChanges.PointDefense,
            },
            new BlockTweak {
                Type = BlockTypes.BF_Laser_Cannon_112,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = BFLaserSpedBuff,
                }
            },
            new BlockTweak {
                Type = BlockTypes.BF_Laser_Cannon_112,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = BFLaserSpedBuff,
                }
            },
            new BlockTweak {
                Type = BlockTypes.BF_Laser_Streamlined_111,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = BFLaserSpedBuff,
                }
            },
            new BlockTweak {
                Type = BlockTypes.BF_Laser_Streamlined_112,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = BFLaserSpedBuff,
                }
            },
            new BlockTweak {
                Type = BlockTypes.BF_Laser_Streamlined_113,
                WT = new WeaponTweak {
                    ProjSpeedChange = ProjSpeedChange.Fast,
                    ProjSpeedChangeMulti = BFLaserSpedBuff,
                }
            },
            new BlockTweak {
                Type = BlockTypes.BF_Laser_Turret_212,
                WT = new WeaponTweak {
                    OverrideTraverse = 120,// Orbiting harassment weapon
                }
            },
            new BlockTweak {
                Type = BlockTypes.BF_Laser_Turret_313,
                WT = new WeaponTweak {
                    OverrideTraverse = 90,// Orbiting harassment weapon
                }
            },
            new BlockTweak {
                Type = BlockTypes.BF_MissilePod_323,
                WT = new WeaponTweak {
                    ChangeSeeking = 0, // For first phase
                    OverrideTraverse = 65,
                },
                MiscChanges = SpecChanges.MultiStageWeapon2,
            },
            // RR
            new BlockTweak {
                Type = BlockTypes.EXP_Flares_121,
                //MiscChanges = SpecChanges.PointDefenseFlare,
            },
            new BlockTweak {
                Type = BlockTypes.EXP_LaserGun_323,
                WT = new WeaponTweak {
                    OverrideTraverse = 110,
                },
            },

            // ARMOR
            new BlockTweak {
                Type = BlockTypes.GCArmourPlate_221,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCArmourPlate_421,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GC_ArmourPlate_121,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GSOArmourPlateCab_111,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GSOArmourPlateLarge_222,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GSOArmourPlateMedium_211,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GSOArmourPlateSmall_111,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_10_111,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_114,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_11_121,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_122,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_123,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_124,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_12_131,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_CornerInt_112,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_CornerInt_113,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_CornerInt_114,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Corner_112,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Corner_113,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Corner_114,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Corner_212,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Edge_111,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Edge_211,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Edge_InsideCorner_111,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Edge_OutsideCorner_111,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmouredBlock_Edge_OutsideCorner_212,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmourPlate_111,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmourPlate_211,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmourPlate_221,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.HE_ArmourPlate_Extended_221,
                MiscChanges = SpecChanges.MakeArmored,
            },

            // ETC Buffs
            //  GC Crash resistance
            new BlockTweak {
                Type = BlockTypes.GCArticulatedDrill_423,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCBuzzSaw_246,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCDiggerScoop_433,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCJackHammer_244,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCJackHammer_Straight_226,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCTripleBore_234,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCMiningFlail_442,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCMiningFlail_663,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCFlail_446,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCBuzzSaw_312,
                MiscChanges = SpecChanges.MakeArmored,
            },
            new BlockTweak {
                Type = BlockTypes.GCRamSpike_223,
                MiscChanges = SpecChanges.MakeArmored,
            },
        };

        internal static List<DamageTweak> DMGTweaks = new List<DamageTweak>(){
            // Cryogenic
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Cryo,
                Taker = ManDamage.DamageableType.Standard,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Cryo,
                Taker = ManDamage.DamageableType.Armour,
            },
            new DamageTweak {
                Multiplier = 0.25f,
                Dealer = DamageTypesExt.Cryo,
                Taker = ManDamage.DamageableType.Rock,
            },
            new DamageTweak {
                Multiplier = 0.5f,
                Dealer = DamageTypesExt.Cryo,
                Taker = ManDamage.DamageableType.Rubber,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Cryo,
                Taker = ManDamage.DamageableType.Shield,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Cryo,
                Taker = ManDamage.DamageableType.Volatile,
            },
            new DamageTweak {
                Multiplier = 1.5f,
                Dealer = DamageTypesExt.Cryo,
                Taker = ManDamage.DamageableType.Wood,
            },
            // EMP
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.EMP,
                Taker = ManDamage.DamageableType.Standard,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.EMP,
                Taker = ManDamage.DamageableType.Armour,
            },
            new DamageTweak {
                Multiplier = 0.1f, // Strong resistance
                Dealer = DamageTypesExt.EMP,
                Taker = ManDamage.DamageableType.Rock,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.EMP,
                Taker = ManDamage.DamageableType.Rubber,
            },
            new DamageTweak {
                Multiplier = 4,
                Dealer = DamageTypesExt.EMP,
                Taker = ManDamage.DamageableType.Shield,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.EMP,
                Taker = ManDamage.DamageableType.Volatile,
            },
            new DamageTweak {
                Multiplier = 0, // Can't zap wood
                Dealer = DamageTypesExt.EMP,
                Taker = ManDamage.DamageableType.Wood,
            },
            // Scramble
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Scramble,
                Taker = ManDamage.DamageableType.Standard,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Scramble,
                Taker = ManDamage.DamageableType.Armour,
            },
            new DamageTweak {
                Multiplier = 0.1f, // Strong resistance
                Dealer = DamageTypesExt.Scramble,
                Taker = ManDamage.DamageableType.Rock,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Scramble,
                Taker = ManDamage.DamageableType.Rubber,
            },
            new DamageTweak {
                Multiplier = 4,
                Dealer = DamageTypesExt.Scramble,
                Taker = ManDamage.DamageableType.Shield,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Scramble,
                Taker = ManDamage.DamageableType.Volatile,
            },
            new DamageTweak {
                Multiplier = 0, // Can't zap wood
                Dealer = DamageTypesExt.Scramble,
                Taker = ManDamage.DamageableType.Wood,
            },
            // Acid
            new DamageTweak {
                Multiplier = 0.5f,
                Dealer = DamageTypesExt.Acid,
                Taker = ManDamage.DamageableType.Standard,
            },
            new DamageTweak {
                Multiplier = 0.5f,
                Dealer = DamageTypesExt.Acid,
                Taker = ManDamage.DamageableType.Armour,
            },
            new DamageTweak {
                Multiplier = 0.25f, // Strong resistance
                Dealer = DamageTypesExt.Acid,
                Taker = ManDamage.DamageableType.Rock,
            },
            new DamageTweak {
                Multiplier = 0.75f,
                Dealer = DamageTypesExt.Acid,
                Taker = ManDamage.DamageableType.Rubber,
            },
            new DamageTweak {
                Multiplier = 0.25f,
                Dealer = DamageTypesExt.Acid,
                Taker = ManDamage.DamageableType.Shield,
            },
            new DamageTweak {
                Multiplier = 1,
                Dealer = DamageTypesExt.Acid,
                Taker = ManDamage.DamageableType.Volatile,
            },
            new DamageTweak {
                Multiplier = 1.25f,
                Dealer = DamageTypesExt.Acid,
                Taker = ManDamage.DamageableType.Wood,
            },

        };

    }

    internal static class SpecChanges
    {
        internal static void MakeArmored(TankBlock TB, bool AlreadyDid, BlockTypes Override)
        {
            if (KickStart.RandomAdditionsAvail)
            {
                var endurance = TB.GetComponent<ModuleReinforced>();
                if (!endurance)
                    endurance = TB.gameObject.AddComponent<ModuleReinforced>();
                endurance.DenyExplosion = true;
            }
        }


        private static FieldInfo lifeTime = typeof(Projectile).GetField("m_LifeTime", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo boostJ = typeof(BoosterJet).GetField("m_Force", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo boostS = typeof(MissileProjectile).GetField("m_BoosterActivationDelay", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo boost = typeof(MissileProjectile).GetField("m_MaxBoosterLifetime", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo armTime = typeof(MissileProjectile).GetField("m_ArmDelay", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo seekDist = typeof(SeekingProjectile).GetField("m_VisionRange", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo seekSped = typeof(SeekingProjectile).GetField("m_TurnSpeed", BindingFlags.NonPublic | BindingFlags.Instance); 
        /// <summary>
        /// Delay missile control
        /// </summary>
        /// <param name="TB"></param>
        internal static void MultiStageWeapon(TankBlock TB, bool AlreadyDid, BlockTypes Override)
        {
            MultiStageWeaponBase(TB, AlreadyDid, 0.75f, Override);
        }

        /// <summary>
        /// Delay missile control shorter
        /// </summary>
        /// <param name="TB"></param>
        internal static void MultiStageWeapon2(TankBlock TB, bool AlreadyDid, BlockTypes Override)
        {
            MultiStageWeaponBase(TB, AlreadyDid, 0.5f, Override);
        }

        private static void MultiStageWeaponBase(TankBlock TB, bool AlreadyDid, float val, BlockTypes Override) 
        {
            if (AlreadyDid)
                return;
            if (!(bool)TB || !KickStart.RandomAdditionsAvail)
                return;
            bool worked = false;
            try
            {
                var FD = TB.GetComponent<FireData>();
                if ((bool)FD)
                {
                    WeaponRound BP = WeaponTweak.GetOrSetBPrefab(TB, Override);
                    if ((bool)BP)
                    {
                        var GO = BP.gameObject;
                        if ((bool)GO)
                        {
                            GameObject GO2 = null;
                            GameObject GO3 = null;

                            var SP = GO.GetComponent<SeekingProjectile>();
                            float SeD = (float)seekDist.GetValue(SP);
                            float SeS = (float)seekSped.GetValue(SP);

                            var FD2 = GO.GetComponent<FireData>();
                            if (!FD2)
                            {
                                // Stage 2 - Turn
                                GO2 = UnityEngine.Object.Instantiate(GO, null);
                                GO2.SetActive(false);
                                FD2 = GO.AddComponent<FireData>();
                                FD2.m_KickbackStrength = 0;
                                FD2.m_MuzzleVelocity = 1;
                                FD2.m_BulletSpin = 0;
                                if (!GO2.GetComponent<WeaponRound>())
                                    Debug.Log("TweakTech: MultiStageWeapon - Could not fetch duplicate WR for " + TB.name);
                                FD2.m_BulletPrefab = GO2.GetComponent<WeaponRound>();

                                var stage3 = GO2.GetComponent<SpiltProjectile>();
                                if (!stage3)
                                    stage3 = GO2.AddComponent<SpiltProjectile>();
                                stage3.SpawnAmount = 1;
                                stage3.DeployOnExplode = true;
                                stage3.UseSeeking = true;

                                boostS.SetValue(GO2.GetComponent<MissileProjectile>(), 0.0f);
                                boost.SetValue(GO2.GetComponent<MissileProjectile>(), 0.25f);
                                lifeTime.SetValue(GO2.GetComponent<Projectile>(), 1.75f);
                                var SP2 = GO2.GetComponent<SeekingProjectile>();
                                seekDist.SetValue(SP2, SeD * 4f);
                                seekSped.SetValue(SP2, SeS / 1.5f); // One-turn
                            }
                            else
                            {
                                GO2 = FD2.gameObject;
                            }

                            var FD3 = GO2.GetComponent<FireData>();
                            if (!FD3)
                            { // Stage 3 - Home in
                                GO3 = UnityEngine.Object.Instantiate(GO, null);
                                GO3.SetActive(false);

                                FD3 = GO2.AddComponent<FireData>();
                                FD3.m_KickbackStrength = 0;
                                FD3.m_MuzzleVelocity = 1;
                                FD3.m_BulletSpin = 0;
                                if (!GO3.GetComponent<WeaponRound>())
                                    Debug.Log("TweakTech: MultiStageWeapon - Could not fetch duplicate WR(2) for " + TB.name);
                                FD3.m_BulletPrefab = GO3.GetComponent<WeaponRound>();

                                //float prevVal = (float)boost.GetValue(GO.GetComponent<MissileProjectile>());
                                float prevValL = (float)lifeTime.GetValue(GO.GetComponent<Projectile>());

                                var BJ = GO3.GetComponentInChildren<BoosterJet>();
                                boostJ.SetValue(BJ, (float)boostJ.GetValue(BJ) * 0.75f);

                                boostS.SetValue(GO3.GetComponent<MissileProjectile>(), 0);
                                boost.SetValue(GO3.GetComponent<MissileProjectile>(), prevValL);
                                lifeTime.SetValue(GO3.GetComponent<Projectile>(), prevValL);
                                var SP3 = GO3.GetComponent<SeekingProjectile>();
                                seekDist.SetValue(SP3, SeD * 2.5f);
                                seekSped.SetValue(SP3, SeS / 6f); // After one-turn it suffers
                            }
                            else
                            {
                                GO3 = FD3.gameObject;
                            }
                            Debug.Log("TweakTech: MultiStageWeapon - " + TB.name + "  | Part 0");
                            DisableExplode(GO2.GetComponent<Projectile>());

                            Debug.Log("TweakTech: MultiStageWeapon - " + TB.name + "  | Part 1");
                            // Stage 1 - Launch
                            var stage2 = GO.GetComponent<SpiltProjectile>();
                            if (!stage2)
                                stage2 = GO.AddComponent<SpiltProjectile>();
                            Debug.Log("TweakTech: MultiStageWeapon - " + TB.name + "  | Part 2");
                            stage2.SpawnAmount = 1;
                            stage2.DeployOnExplode = true;
                            stage2.UseSeeking = true;

                            Debug.Log("TweakTech: MultiStageWeapon - " + TB.name + "  | Part 3");
                            boost.SetValue(GO.GetComponent<MissileProjectile>(), 0.1f);
                            var proj = BP.GetComponent<Projectile>();
                            Debug.Log("TweakTech: MultiStageWeapon - " + TB.name + "  | Part 4");
                            lifeTime.SetValue(proj, val);
                            Debug.Log("TweakTech: MultiStageWeapon - " + TB.name + "  | Part 5");
                            explode.SetValue(BP.GetComponent<Projectile>(), null);
                            Debug.Log("TweakTech: MultiStageWeapon - There is an explosion in " + TB.name + "  | " + (bool)((Transform)explode.GetValue(BP.GetComponent<Projectile>())));

                            worked = true;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: MultiStageWeapon - fail in changing " + TB.name + "  | " + e);
            }
            if (!worked)
                Debug.Log("TweakTech: MultiStageWeapon - fail in changing " + TB.name);
        }
        internal static void NerfTracking(TankBlock TB, bool AlreadyDid, BlockTypes Override)
        {
            if (AlreadyDid)
                return;
            if (!(bool)TB || !KickStart.RandomAdditionsAvail)
                return;
            bool worked = false;
            try
            {
                var FD = TB.GetComponent<FireData>();
                if ((bool)FD)
                {
                    WeaponRound BP = WeaponTweak.GetOrSetBPrefab(TB, Override);
                    if ((bool)BP)
                    {
                        var GO = BP.gameObject;
                        if ((bool)GO)
                        {
                            var SP = GO.GetComponent<SeekingProjectile>();
                            seekSped.SetValue(SP, (float)seekSped.GetValue(SP) / 2.25f); 
                            worked = true;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: NerfTracking - fail in changing " + TB.name + "  | " + e);
            }
            if (!worked)
                Debug.Log("TweakTech: NerfTracking - fail in changing " + TB.name);
        }

        internal static void DoubleHoming(TankBlock TB, bool AlreadyDid, BlockTypes Override)
        {
            if (AlreadyDid)
                return;
            if (!(bool)TB)
                return;
            bool worked = false;
            try
            {
                var FD = TB.GetComponent<FireData>();
                if ((bool)FD)
                {
                    WeaponRound BP = WeaponTweak.GetOrSetBPrefab(TB, Override);
                    if ((bool)BP)
                    {
                        var SP = BP.GetComponent<SeekingProjectile>();
                        if ((bool)SP)
                        {
                            seekDist.SetValue(SP, (float)seekDist.GetValue(SP) * 2.25f);
                            seekSped.SetValue(SP, (float)seekSped.GetValue(SP) * 2.25f);
                            worked = true;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: DoubleHoming - fail in changing " + TB.name + "  | " + e);
            }
            if (!worked)
                Debug.Log("TweakTech: DoubleHoming - fail in changing " + TB.name);
        }

        internal static void PointDefense(TankBlock TB, bool AlreadyDid, BlockTypes Override)
        {
            PointDefenseBase(TB, AlreadyDid, Override);
        }
        internal static void PointDefenseFlare(TankBlock TB, bool AlreadyDid, BlockTypes Override)
        {
            PointDefenseBase(TB, AlreadyDid, Override, true);
        }
        private static void PointDefenseBase(TankBlock TB, bool AlreadyDid, BlockTypes Override, bool Flares = false)
        {
            if (!(bool)TB || !KickStart.RandomAdditionsAvail)
                return;
            bool worked = false;
            if (AlreadyDid)
            {
                var MPD = TB.GetComponent<ModulePointDefense>();
                if (!(bool)MPD)
                {
                    MPD = TB.gameObject.AddComponent<ModulePointDefense>();
                    MPD.CanInterceptFast = false;
                    MPD.OverrideEnemyAiming = false;
                    MPD.DefenseEnergyCost = 0;
                    MPD.DefendRange = 75;
                    var MW = TB.GetComponent<ModuleWeapon>();
                    if ((bool)MW)
                    {
                        MPD.RotateRate = MW.m_RotateSpeed * 2f;
                        MPD.FireSFXType = MW.m_FireSFXType;
                    }
                    else
                    {
                        MPD.RotateRate = 100;
                    }
                    if (TB.GetComponent<ModuleWeaponGun>())
                    {
                        if (Flares)
                            MPD.DefenseCooldown = TB.GetComponent<ModuleWeaponGun>().m_ShotCooldown * 2;
                        else
                            MPD.DefenseCooldown = TB.GetComponent<ModuleWeaponGun>().m_ShotCooldown / 2;
                    }
                    else
                        MPD.DefenseCooldown = 0.25f;
                }
                return;
            }
            try
            {
                var FD = TB.GetComponent<FireData>();
                if ((bool)FD)
                {
                    WeaponRound BP = WeaponTweak.GetOrSetBPrefab(TB, Override);
                    if ((bool)BP)
                    {
                        var IP = BP.gameObject.AddComponent<InterceptProjectile>();
                        if ((bool)IP)
                        {
                            if (Flares)
                            {
                                IP.IsFlare = true;
                                IP.ConstantDistract = true;
                                IP.DistractChance = 16;
                                IP.DistractsMoreThanOne = true;
                                IP.InterceptRange = 2;
                            }
                            else
                            {
                                IP.InterceptRange = 4;
                            }
                            worked = true;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: PointDefenseBase - fail in changing " + TB.name + "  | " + e);
            }
            if (!worked)
                Debug.Log("TweakTech: PointDefenseBase - fail in changing " + TB.name);
        }

        internal static void NerfCruiseMissile(TankBlock TB, bool AlreadyDid, BlockTypes Override)
        {   // We slow it down to half speed because siege weapon
            if (!(bool)TB || !KickStart.RandomAdditionsAvail)
                return;
            if (AlreadyDid)
            {
                return;
            }
            bool worked = false;
            try
            {
                var FD = TB.GetComponent<FireData>();
                if ((bool)FD)
                {
                    WeaponRound BP = WeaponTweak.GetOrSetBPrefab(TB, Override);
                    if ((bool)BP)
                    {
                        var GP = BP.gameObject.AddComponent<GravitateProjectile>();
                        if ((bool)GP)
                        {
                            GP.WorldGravitateStrength = 0;
                            GP.WorldAugmentedDragEnabled = true;
                            GP.WorldAugmentedDragStrength = 0.025f;
                            worked = true;
                        }
                        var Proj = BP.gameObject.GetComponent<Projectile>();
                        if ((bool)Proj)
                            lifeTime.SetValue(Proj, (float)lifeTime.GetValue(Proj) * 2.5f);
                        var Miss = BP.gameObject.GetComponent<MissileProjectile>();
                        if ((bool)Miss)
                            boost.SetValue(Miss, (float)boost.GetValue(Miss) * 2.5f);
                        var Seek = BP.gameObject.GetComponent<SeekingProjectile>();
                        if ((bool)Seek)
                            seekSped.SetValue(Seek, (float)seekSped.GetValue(Seek) / 2.5f);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: NerfCruiseMissile - fail in changing " + TB.name + "  | " + e);
            }
            if (!worked)
                Debug.Log("TweakTech: NerfCruiseMissile - fail in changing " + TB.name);
        }

        private static FieldInfo explode = typeof(Projectile).GetField("m_Explosion", BindingFlags.NonPublic | BindingFlags.Instance);
        private static void DisableExplode(Projectile Proj)
        {
            try
            {
                if ((bool)Proj)
                {
                    explode.SetValue(Proj, null);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.Log("TweakTech: DisableExplode - fail in changing " + Proj.name + "  | " + e);
            }
            //Debug.Log("TweakTech: DisableExplode - fail in changing " + Proj.name);
        }
    }
}
