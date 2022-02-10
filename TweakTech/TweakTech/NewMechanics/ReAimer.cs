using System;
using System.Collections.Generic;
using System.Linq;
using TAC_AI.AI;
using UnityEngine;
using System.Reflection;

namespace TweakTech
{
    public class ReAimer : MonoBehaviour
    {
        private TankBlock block;
        private ModuleWeapon Weap;
        private ModuleWeaponGun WeapG;
        public float GravSpeedModifier = 1;
        public Func<Vector3, Vector3> swatch;


        private static readonly FieldInfo SeekStrength = typeof(SeekingProjectile).GetField("m_TurnSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

        public static ReAimer ApplyToBlock(TankBlock TB)
        {
            if (TB.GetComponentInChildren<BeamWeapon>() || TB.GetComponent<FireDataShotgun>())
                return null; // Beams and shotguns do not lead!

            var WeapNew = TB.GetComponent<ModuleWeapon>();
            if ((bool)WeapNew)
            {
                var MWG = TB.GetComponent<ModuleWeaponGun>();
                if ((bool)MWG)
                {
                    var RA = TB.gameObject.AddComponent<ReAimer>();
                    RA.block = TB;
                    RA.Weap = WeapNew;
                    RA.WeapG = MWG;
                    if (TB.GetComponent<FireData>())
                    {
                        var BF = TB.GetComponent<FireData>().m_BulletPrefab;
                        if ((bool)BF)
                        {
                            var rbody = BF.GetComponent<Rigidbody>();
                            var missile = BF.GetComponent<MissileProjectile>();
                            if (!(bool)missile)
                            {
                                if ((bool)rbody?.useGravity)
                                {
                                    RA.swatch = RA.RoughPredictAim;
                                    return RA;
                                }
                            }
                            else
                            {
                                var seek = BF.GetComponent<SeekingProjectile>();
                                if (seek)
                                {
                                    if (25 >= (float)SeekStrength.GetValue(seek))
                                    {
                                        RA.swatch = RA.RoughPredictAim;
                                        return RA;
                                    }
                                }
                                else
                                {
                                    RA.swatch = RA.RoughPredictAim;
                                    return RA;
                                }
                            }
                            //Debug.Log("TweakTech: Changed TargetAimer(Arc) for " + TB.name);
                        }
                        //Debug.Log("TweakTech: Changed TargetAimer(Straight) for " + TB.name);
                    }
                    RA.swatch = RA.RoughPredictAimS;
                    return RA;
                }
            }
            return null;
        }
        public static ReAimer UpdateExisting(TankBlock TB)
        {
            if (TB.GetComponentInChildren<BeamWeapon>() || TB.GetComponent<FireDataShotgun>())
                return null; // Beams and shotguns do not lead!

            var WeapNew = TB.GetComponent<ModuleWeapon>();
            if ((bool)WeapNew)
            {
                var MWG = TB.GetComponent<ModuleWeaponGun>();
                if ((bool)MWG)
                {
                    var RA = TB.gameObject.GetComponent<ReAimer>();
                    RA.block = TB;
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
                    return RA;
                }
            }
            return null;
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
            Vector3 targPos = TargetPos;
            Vector3 posVec = TargetPos - WeapG.GetFireTransform().position;
            if (TankLazyAim.FetchVelocityDifference(block.tank, out Vector3 VeloDiff))
            {
                float MaxRangeVelo = velo * 0.7071f;
                float MaxTime = MaxRangeVelo / grav;
                float MaxDist = MaxTime * MaxRangeVelo;

                float veloVecMag = posVec.magnitude;
                float distDynamic = veloVecMag / MaxDist;
                if (distDynamic > 1)
                    distDynamic = 1;
                float roughTime = veloVecMag / (velo * (0.7071f + ((1 - distDynamic) * 0.2929f)));
                // this works I don't even know how
                Vector3 VeloDiffCorrected = VeloDiff;
                VeloDiffCorrected.y = 0;
                // The power of cos at 45 degrees compels thee
                VeloDiffCorrected = VeloDiffCorrected.magnitude * 0.7071f * VeloDiffCorrected.normalized;
                VeloDiffCorrected.y = VeloDiff.y;
                targPos = TargetPos + (VeloDiffCorrected * roughTime);
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
            Visible target = tank.Weapons.GetManualTarget();
            if (target == null)
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
            if ((bool)tank.rbody)
                VelocityDiff = -tank.rbody.velocity;// * Time.fixedDeltaTime;
            else
                VelocityDiff = Vector3.zero;
            if (!(bool)target)
            {
                Moving = !VelocityDiff.Approximately(Vector3.zero);
                return;
            }
            if ((bool)target.tank)
            {
                if ((bool)target.rbody)
                {
                    VelocityDiff += target.rbody.velocity;// * Time.fixedDeltaTime;
                    Moving = !VelocityDiff.Approximately(Vector3.zero);
                }
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
