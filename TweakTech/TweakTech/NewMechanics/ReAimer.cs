using System;
using System.Collections.Generic;
using System.Linq;
using TAC_AI.AI;
using UnityEngine;
using System.Reflection;

namespace TweakTech
{
    /// <summary>
    /// Note: this is automatically disabled when WeaponAimMod is installed as that mod has better target leading
    /// </summary>
    public class ReAimer : MonoBehaviour
    {
        private TankBlock block;
        private ModuleWeapon Weap;
        private ModuleWeaponGun WeapG;
        public float GravSpeedModifier = 1;
        private Func<Vector3, Vector3> swatch;
        public Func<Vector3, Vector3> swatchGet { 
            get 
            {
                if (KickStart.EnableThis)
                    return swatch;
                return swatchDefault;
            } 
        }
        public Func<Vector3, Vector3> swatchDefault;


        private static readonly FieldInfo SeekStrength = typeof(SeekingProjectile).GetField("m_TurnSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void CreateOrUpdateForBlock(TankBlock TB)
        {
            if (TB.GetComponentInChildren<BeamWeapon>() || TB.GetComponent<FireDataShotgun>())
                return; // Beams and shotguns do not lead!
            //Debug.Log("TweakTech: CreateOrUpdateForBlock");
            var aimer = TB.GetComponent<ReAimer>();
            if (!aimer) 
                ApplyToBlock(TB);
            else
                aimer.UpdateExisting(TB);
        }
        private static void ApplyToBlock(TankBlock TB)
        {
            var WeapNew = TB.GetComponent<ModuleWeapon>();
            if ((bool)WeapNew)
            {
                var MWG = TB.GetComponent<ModuleWeaponGun>();
                if ((bool)MWG)
                {
                    //Debug.Log("TweakTech: Created ReAimer for block " + TB.name);
                    var RA = TB.gameObject.AddComponent<ReAimer>();
                    RA.block = TB;
                    RA.Weap = WeapNew;
                    RA.WeapG = MWG;
                    if (TB.GetComponent<FireData>())
                    {
                        var BF = TB.GetComponent<FireData>().m_BulletPrefab;
                        if ((bool)BF)
                        {
                            var WP = BF.GetComponent<WeightedProjectile>();
                            var rbody = BF.GetComponent<Rigidbody>();
                            var missile = BF.GetComponent<MissileProjectile>();
                            if (!(bool)missile)
                            {
                                if ((bool)rbody?.useGravity)
                                {
                                    if (WP)
                                        RA.GravSpeedModifier = WP.GravityAndSpeedScale;
                                    else
                                        RA.GravSpeedModifier = 1;
                                    RA.Attach();
                                    RA.swatch = RA.RoughPredictAim;
                                    return;
                                }
                            }
                            else
                            {
                                var seek = BF.GetComponent<SeekingProjectile>();
                                if (seek)
                                {
                                    if (25 >= (float)SeekStrength.GetValue(seek))
                                    {
                                        if (WP)
                                            RA.GravSpeedModifier = WP.GravityAndSpeedScale;
                                        else
                                            RA.GravSpeedModifier = 1;
                                        RA.Attach();
                                        RA.swatch = RA.RoughPredictAim;
                                        return;
                                    }
                                }
                                else
                                {
                                    if (WP)
                                        RA.GravSpeedModifier = WP.GravityAndSpeedScale;
                                    else
                                        RA.GravSpeedModifier = 1;
                                    RA.Attach();
                                    RA.swatch = RA.RoughPredictAim;
                                    return;
                                }
                            }
                            if (WP)
                                RA.GravSpeedModifier = WP.GravityAndSpeedScale;
                            else
                                RA.GravSpeedModifier = 1;
                            //Debug.Log("TweakTech: Changed TargetAimer(Arc) for " + TB.name);
                        }
                        //Debug.Log("TweakTech: Changed TargetAimer(Straight) for " + TB.name);
                    }
                    RA.Attach();
                    RA.swatch = RA.RoughPredictAimS;
                    return;
                }
            }
            //Debug.Log("TweakTech: Failed to determine block as cand");
        }

        static FieldInfo aimD = typeof(TargetAimer).GetField("AimDelegate", BindingFlags.NonPublic | BindingFlags.Instance);
        public void Attach()
        {
            var TA = block.GetComponent<TargetAimer>();
            swatchDefault = (Func<Vector3, Vector3>)aimD.GetValue(TA);
            aimD.SetValue(TA, swatchGet);
            //Debug.Log("TweakTech: Attached ReAimer for block " + block.name);
        }

        private void UpdateExisting(TankBlock TB)
        {
            var WeapNew = TB.GetComponent<ModuleWeapon>();
            if ((bool)WeapNew)
            {
                var MWG = TB.GetComponent<ModuleWeaponGun>();
                if ((bool)MWG)
                {
                    //Debug.Log("TweakTech: Updating ReAimer for block " + TB.name + " enabled " + KickStart.EnableThis);
                    block = TB;
                    Weap = WeapNew;
                    WeapG = MWG;
                    if (MWG.AimWithTrajectory())
                    {
                        swatch = RoughPredictAim;
                        //Debug.Log("TweakTech: Changed TargetAimer(Arc) for " + TB.name);
                    }
                    else
                    {
                        swatch = RoughPredictAimS;
                        //Debug.Log("TweakTech: Changed TargetAimer(Straight) for " + TB.name);
                    }
                    var BF = TB.GetComponent<FireData>().m_BulletPrefab;
                    if (BF)
                    {
                        var WP = BF.GetComponent<WeightedProjectile>();
                        if (WP)
                            GravSpeedModifier = WP.GravityAndSpeedScale;
                        else
                            GravSpeedModifier = 1;
                    }
                    else
                        GravSpeedModifier = 1;
                    Attach();
                    return;
                }
            }
            Debug.Log("TweakTech: ASSERT - block " + TB.name + " has ReAimer but with no valid reason to have one!!!");
        }
        public void ReCheckAiming()
        {
            if (block.GetComponentInChildren<BeamWeapon>() || block.GetComponent<FireDataShotgun>())
                return; // Beams and shotguns do not lead!

            var WeapNew = block.GetComponent<ModuleWeapon>();
            if ((bool)WeapNew)
            {
                var MWG = block.GetComponent<ModuleWeaponGun>();
                if ((bool)MWG)
                {
                    var RA = block.gameObject.GetComponent<ReAimer>();
                    RA.block = block;
                    RA.Weap = WeapNew;
                    RA.WeapG = MWG;
                    if (MWG.AimWithTrajectory())
                    {
                        RA.swatch = RA.RoughPredictAim;
                        //Debug.Log("TweakTech: Changed TargetAimer(Arc) for " + TB.name);
                    }
                    else
                    {
                        RA.swatch = RA.RoughPredictAimS;
                        //Debug.Log("TweakTech: Changed TargetAimer(Straight) for " + TB.name);
                    }
                }
            }
        }

        public Vector3 RoughPredictAimS(Vector3 aimPoint)
        {   
            if (!TankLazyAim.FetchVelocityDifference(block.tank, out Vector3 VeloDiff))
                return aimPoint;
            float velo = WeapG.GetVelocity();
            if (velo < 1)
                velo = 1;
            Vector3 posVec = aimPoint - WeapG.GetFireTransform().position;
            float roughDist = posVec.magnitude / velo;
            return aimPoint + (VeloDiff * roughDist);
        }

        public Vector3 RoughPredictAim(Vector3 TargetPos)
        {
            float velo = WeapG.GetVelocity();
            if (velo < 1)
                velo = 1;
            //Debug.Log("TweakTech: RoughPredictAim - " + GravSpeedModifier);

            float grav = -(Physics.gravity.y * GravSpeedModifier);
            //Debug.Log("TweakTech: RoughPredictAim - Grav " + grav);
            Vector3 targPos = TargetPos;
            Vector3 posVec = TargetPos - WeapG.GetFireTransform().position;
            if (TankLazyAim.FetchVelocityDifference(block.tank, out Vector3 VeloDiff))
            {
                float VeloMaxDistArc = velo * 0.7071f;    // Velocity at 45 degrees
                float MaxFlightTime = VeloMaxDistArc / grav;    // Divide max upwards range velocity by gravity 
                float MaxDist = MaxFlightTime * VeloMaxDistArc; // Get the max possible range this can aim at

                float distVecMag = posVec.magnitude;
                float distDynamic = distVecMag / MaxDist; // Get the percent of how far we are aiming
                if (distDynamic > 1)
                    distDynamic = 1;
                // So we get approx forwards velocity
                // The power of cos at 45 degrees compels thee
                float approxVelo = velo * (0.7071f + ((1 - distDynamic) * 0.2929f));
                float roughTimeToHit = distVecMag / approxVelo;

                Vector3 VeloDiffCorrected = VeloDiff * 0.7071f;
                VeloDiffCorrected.y = VeloDiff.y;
                /*
                // We are getting the x and z info here and flattening it
                Vector3 VeloDiffCorrected = VeloDiff; 
                VeloDiffCorrected *= 0.7071f;
                VeloDiffCorrected.y = VeloDiff.y;
                */
                targPos = TargetPos + (VeloDiffCorrected * roughTimeToHit);
            }
            // Aim with rough predictive trajectory
            
            velo *= velo;
            Vector3 direct = targPos - WeapG.GetFireTransform().position;
            Vector3 directFlat = direct;
            directFlat.y = 0;
            float distFlat = directFlat.sqrMagnitude;
            float height = direct.y + direct.y;

            float vertOffset = (velo * velo) - grav * (grav * distFlat + (height * velo));
            if (vertOffset < 0)
                targPos.y += (velo / grav) - direct.y;
            else
                targPos.y += ((velo - Mathf.Sqrt(vertOffset)) / grav) - direct.y;
            return targPos;
        }

        public Vector3 RoughPredictAimEXP(Vector3 TargetPos)
        {
            float velo = WeapG.GetVelocity();
            if (velo < 1)
                velo = 1;
            //Debug.Log("TweakTech: RoughPredictAim - " + GravSpeedModifier);

            float grav = -(Physics.gravity.y * GravSpeedModifier);
            //Debug.Log("TweakTech: RoughPredictAim - Grav " + grav);
            Vector3 targPos = TargetPos;

            // Aim with rough trajectory
            /*
            float velo2 = velo * velo;
            Vector3 direct = TargetPos - WeapG.GetFireTransform().position;
            Vector3 directFlat = direct;
            directFlat.y = 0;
            float distFlat = directFlat.sqrMagnitude;
            float height = direct.y + direct.y;

            float vertOffset = (velo2 * velo2) - grav * (grav * distFlat + (height * velo2));
            if (vertOffset < 0)
                targPos.y += (velo2 / grav) - direct.y;
            else
                targPos.y += ((velo2 - Mathf.Sqrt(vertOffset)) / grav) - direct.y;
            */
            if (TankLazyAim.FetchVelocityDifference(block.tank, out Vector3 VeloDiff))
            {
                // d = D + V*T + A*T^2

                float VeloMaxArc = velo * 0.7071f;    // Velocity at 45 degrees
                float MaxFlightTime = VeloMaxArc / grav;    // Divide max upwards range velocity by gravity 
                float MaxDist = MaxFlightTime * VeloMaxArc; // Get the max possible range this can aim at

                float distVecMag = targPos.ToVector2XZ().magnitude;
                float distDynamic = distVecMag / MaxDist; // Get the percent of how far we are aiming
                if (distDynamic > 1)
                    distDynamic = 1;
                // So we get approx forwards velocity
                // The power of cos at 45 degrees compels thee
                float approxForwardVelo = velo * (0.7071f + ((1 - distDynamic) * 0.2929f));
                float roughTimeToHit = distVecMag / approxForwardVelo;
                float gravY = roughTimeToHit * (grav * grav); // Get the max possible range this can aim at

                Vector3 VeloDiffCorrected = VeloDiff;
                /*
                // We are getting the x and z info here and flattening it
                Vector3 VeloDiffCorrected = VeloDiff; 
                VeloDiffCorrected *= 0.7071f;
                VeloDiffCorrected.y = VeloDiff.y;
                */
                targPos.x = TargetPos.x + (VeloDiffCorrected.x * roughTimeToHit);
                targPos.z = TargetPos.z + (VeloDiffCorrected.z * roughTimeToHit);
                targPos.y = TargetPos.y + gravY + (VeloDiffCorrected.y * roughTimeToHit);
                return TargetPos;
            }

            return swatchDefault(TargetPos);
        }
    }

    public class TankLazyAim : MonoBehaviour
    {
        private List<ModuleWeapon> SWeapons = new List<ModuleWeapon>();
        private Vector3 VelocityDiff = Vector3.zero;
        private bool requestedUpdate = false;
        public bool Moving = false;

        public static bool FetchVelocityDifference(Tank tank, out Vector3 veloDiff)
        {
            veloDiff = Vector3.zero;
            if (!(bool)tank)
                return false;
            var thisInst = tank.GetComponent<TankLazyAim>();
            if (!(bool)thisInst)
                return false;
            if (!thisInst.requestedUpdate)
                thisInst.GetVelocityDifference(tank);
            veloDiff = thisInst.VelocityDiff;
            return thisInst.Moving;
        }


        /// <summary>
        /// Get the velocity of the target to this tech and the difference between both
        /// </summary>
        /// <param name="tank"></param>
        private void GetVelocityDifference(Tank tank)
        {
            Debug.Assert(!tank, "TANK ON CALL IS NULL");
            Visible target = tank.Weapons.GetManualTarget();
            if (!target)
            {
                try
                {
                    if (KickStart.TACAIModAvail)
                    {
                        var helper = tank.GetComponent<AIECore.TankAIHelper>();
                        if (helper.AIState != 0)
                            target = helper.lastEnemy;
                        else
                            target = tank.Vision.GetFirstVisibleTechIsEnemy(tank.Team);
                    }
                    else
                        target = tank.Vision.GetFirstVisibleTechIsEnemy(tank.Team);
                }
                catch { }
            }
            if (tank.rbody)
                VelocityDiff = -tank.rbody.velocity;// * Time.fixedDeltaTime;
            else
                VelocityDiff = Vector3.zero;
            if (!target)
            {
                Moving = !VelocityDiff.Approximately(Vector3.zero);
                return;
            }
            if (target.rbody)
            {
                VelocityDiff += target.rbody.velocity;// * Time.fixedDeltaTime;
                Moving = !VelocityDiff.Approximately(Vector3.zero);
                return;
            }
            Moving = !VelocityDiff.Approximately(Vector3.zero);
        }

        public static void Add(Tank tank, ModuleWeapon weap)
        {
            var TLA = tank.GetComponent<TankLazyAim>();
            if (!(bool)TLA)
                TLA = tank.gameObject.AddComponent<TankLazyAim>();
            TLA.SWeapons.Add(weap);
        }
        public static void Remove(Tank tank, ModuleWeapon weap)
        {
            var TLA = tank.GetComponent<TankLazyAim>();
            if ((bool)TLA)
            {
                TLA.SWeapons.Remove(weap);
                if (TLA.SWeapons.Count == 0)
                    Destroy(TLA);
            }
        }

        private void FixedUpdate()
        {
            requestedUpdate = false;
        }
    }
}
