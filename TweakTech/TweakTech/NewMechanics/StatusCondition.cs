using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace TweakTech
{
    internal class ManStatus : MonoBehaviour
    {
        float time = 0;
        float timeFlash = 0;
        float timeAcid = 0;
        bool isBright = false;
        float reloop = Mathf.PI * 2;
        private static List<Action> pendingSubs = new List<Action>();

        public static EventNoParams acidUpdate = new EventNoParams();
        public static EventNoParams spreadUpdate = new EventNoParams();

        private void Update()
        {
            timeFlash = (timeFlash + (Time.deltaTime * 10)) % reloop;
            StatusCondition.t2d2.SetPixel(0, 17, new Color(1f, 1f, 1f, 0.7f + (0.25f * Mathf.Sin(timeFlash))));        // Scrambled Color
            if (time < Time.time)
            {
                   isBright = !isBright;
                if (isBright)
                    StatusCondition.t2d2.SetPixel(0, 14, new Color(1f, 1f, 0.5f, 0.85f));   // EMP Color
                else
                    StatusCondition.t2d2.SetPixel(0, 14, new Color(1f, 1f, 0.5f, 0.55f));   // EMP Color
                if (!ManPauseGame.inst.IsPaused)
                {
                    spreadUpdate.Send();
                    DelayedSubscribe();
                }
                time = Time.time + 0.5f;
            }
            StatusCondition.t2d2.Apply();
            if (timeAcid < Time.time)
            {
                if (!ManPauseGame.inst.IsPaused)
                {
                    acidUpdate.Send();
                }
                timeAcid = Time.time + 2;
            }
        }
        public static void PrepForSub(Action eventCase)
        {
            pendingSubs.Add(eventCase);
        }
        // Event does not like it when we are subscribing during a call
        private void DelayedSubscribe()
        {
            foreach (Action sub in pendingSubs)
                spreadUpdate.Subscribe(sub);
            pendingSubs.Clear();
        }
    }

    /// <summary>
    /// This is not applied consistantly between clients.  What the client sees on their own Tech is what they experience
    /// </summary>
    public class StatusCondition : MonoBehaviour
    {   // A Block cannot have more than two statuses at once

        internal static ManStatus man;

        public const float PryRecoveryPercentSec = 0.5f;
        public const float TempDisspationPercentSec = 0.1f;
        public const float UnJammingRateSec = 1250;
        public const float HackRecoveryPercentSec = 0.25f;
        public const float EMPGroundingRateSec = 250;
        public const float EMPGroundingRatePrecisePercentSec = 0.1f;
        public const float AcidVaporPercentSec = 50;
        private const float RedDelay = 0.5f;
        private const float RecoverDelay = 2.5f;
        private const int MinimumVolRqdToHack = 7;

        public const float FilmShieldDrain = 0.1f;

        private static int propID = -24;
        private static MaterialPropertyBlock propInitcache;
        public static Texture2D t2d2;
        public static float t2d2H = 1;
        private static ManDamage.DamageInfo acidPrefab;
        private static readonly FieldInfo prop = typeof(MaterialSwapper).GetField("s_matPropId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        private static readonly FieldInfo propInit = typeof(MaterialSwapper).GetField("s_matProp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        private static readonly FieldInfo colorsForMore = typeof(ManTechMaterialSwap).GetField("m_BlockDamageCLUT", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        private static readonly FieldInfo colorsForMoreH = typeof(ManTechMaterialSwap).GetField("m_BlockDamageScaleV", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        private static readonly FieldInfo skinVGet = typeof(MaterialSwapper).GetField("m_skinValueV", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        private static readonly FieldInfo skinUGet = typeof(MaterialSwapper).GetField("m_skinValueU", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);


        // Local Properties
        private TankBlock block;
        public Damageable damageable;
        public MaterialSwapper swap;
        public Renderer[] renders;
        public StatusType Status = StatusType.None;
        public float impactValue = 0;
        internal float impactPercent = 0;
        public float impactExcess = 0;

        private float skinV = 0;
        private float skinU = 0;

        private float redDelay = 0;
        private float warningRepeat = 0;
        private float lastDamageDelay = 0;
        private float originalFragility = 0;
        private bool disabledComponents = false;
        private bool precisionComponents = false;
        private byte spreadLastFrame = 0;
        public bool allowModuleUpdate = true;
        private static List<StatusCondition> cacheSpread = new List<StatusCondition>();


        public float GetOpPercent()
        {
            switch (Status)
            {
                case StatusType.EMF:
                    return impactPercent < 0.5f ? 1 : 0;
                case StatusType.Freezing:
                    return 1 - (impactPercent / 1.25f);
            }
            return 1;
        }

        public static bool IsSameTeam(Damageable dmg, ManDamage.DamageInfo info)
        {
            if (info.SourceTank)
            {
                int team = info.SourceTank.Team;
                Tank tank = dmg?.transform?.root?.GetComponent<Tank>();

                if (info.SourceTank == tank)
                    return true;

                if (tank)
                    if (tank.IsFriendly(team))
                        return true;
            }
            return false;
        }

        public static bool inited = false;
        public static void InitNewStatus()
        {
            if (!inited)
            {
                man = Instantiate(new GameObject("ManStatus"), null, true).AddComponent<ManStatus>();
                inited = true;
                Debug.Log("StatusCondition: InitNewStatus");
            }
            propID = (int)prop.GetValue(null);
            propInitcache = (MaterialPropertyBlock)propInit.GetValue(null);
            t2d2 = new Texture2D(1, 25);
            t2d2.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
            t2d2.SetPixel(0, 1, Globals.inst.moduleDamageParams.DamageColour);
            t2d2.SetPixel(0, 2, Globals.inst.moduleDamageParams.HealColour);
            t2d2.SetPixel(0, 3, new Color(1f, 1f, 1f, 0f));
            t2d2.SetPixel(0, 4, Globals.inst.moduleDamageParams.ScavengeColour);
            t2d2.SetPixel(0, 5, Globals.inst.moduleDamageParams.OutOfShieldColour);

            t2d2.SetPixel(0, 6, new Color(1f, 0.5f, 0f, 0.95f));        // Overheat Color
            t2d2.SetPixel(0, 7, new Color(1f, 0.5f, 0f, 0.75f));        // 
            t2d2.SetPixel(0, 8, new Color(1f, 0.5f, 0f, 0.55f));        // 
            t2d2.SetPixel(0, 9, new Color(1f, 0.5f, 0f, 0.35f));        // 
            t2d2.SetPixel(0, 10, new Color(0.4f, 0.95f, 0.9f, 0.35f));  // Freeze Color
            t2d2.SetPixel(0, 11, new Color(0.4f, 0.95f, 0.9f, 0.55f));  //
            t2d2.SetPixel(0, 12, new Color(0.4f, 0.95f, 0.9f, 0.75f));  //
            t2d2.SetPixel(0, 13, new Color(0.4f, 0.95f, 0.9f, 0.95f));  //
            t2d2.SetPixel(0, 14, new Color(1f, 1f, 0.5f, 0.85f));       // EMP Color
            t2d2.SetPixel(0, 15, new Color(1f, 1f, 0.5f, 0.45f));       // EMP Color
            t2d2.SetPixel(0, 16, new Color(1f, 1f, 1f, 0.25f));         // Scrambled Color
            t2d2.SetPixel(0, 17, new Color(1f, 1f, 1f, 0.65f));         // Scrambled Color
            t2d2.SetPixel(0, 18, new Color(0f, 0.4f, 0f, 0.99f));       // Hacked Color
            t2d2.SetPixel(0, 19, new Color(0.5f, 0.1f, 1f, 1f));        // Acid Color
            t2d2.SetPixel(0, 20, new Color(0.5f, 0.1f, 1f, 0.75f));     // 
            t2d2.SetPixel(0, 21, new Color(1f, 0.2f, 1f, 1f));          // Unset
            t2d2.SetPixel(0, 22, new Color(0.1f, 0.3f, 1f, 0.25f));     // Shield Color
            t2d2.SetPixel(0, 23, new Color(0.1f, 0.3f, 1f, 0.40f));     //
            t2d2.SetPixel(0, 24, new Color(0.1f, 0.3f, 1f, 0.55f));     //
            t2d2.Apply();
            colorsForMore.SetValue(ManTechMaterialSwap.inst, t2d2);
            Shader.SetGlobalTexture("_DamageCLUT", t2d2);
            t2d2H = 1f / (float)t2d2.height;
            colorsForMoreH.SetValue(ManTechMaterialSwap.inst, t2d2H);
            acidPrefab = new ManDamage.DamageInfo(25, (ManDamage.DamageType)DamageTypesExt.Acid, null);
        }
        public static StatusCondition InitOrGet(Damageable damageable, ManDamage.DamageInfo info, StatusType inflicted)
        {
            StatusCondition SC = damageable.Block?.GetComponent<StatusCondition>();
            if ((bool)SC)
            {
                if (SC.Status != inflicted)
                {
                    switch (SC.Status)
                    {
                        case StatusType.Overheat:
                            switch (inflicted)
                            {
                                case StatusType.Freezing:
                                    SC.SubFromVal(info.Damage * 4, inflicted);
                                    break;
                                case StatusType.Acid:
                                    SC.ChangeStat(info.Damage, inflicted);
                                    break;
                            }
                            break;
                        case StatusType.Freezing:
                            if (inflicted == StatusType.Overheat)
                            {
                                SC.SubFromVal(info.Damage * 2, inflicted);
                            }
                            break;
                    }
                    return null;
                }
                SC.CancelInvoke("Remove");
                return SC;
            }
            GameObject GO = damageable.Block?.gameObject;
            if (!(bool)GO)
                return null;
            var MS = GO.GetComponent<MaterialSwapper>();
            if (!(bool)MS)
                return null;
            if (!CanAddNewTo(GO, inflicted, out bool precision))
                return null;
            SC = GO.AddComponent<StatusCondition>();
            SC.precisionComponents = precision;
            SC.block = GO.GetComponent<TankBlock>();
            SC.swap = MS;
            SC.Status = inflicted;
            SC.damageable = damageable;
            SC.renders = GO.GetComponentsInChildren<Renderer>(true);
            SC.originalFragility = GO.GetComponent<ModuleDamage>().m_DamageDetachFragility;
            ManStatus.acidUpdate.Subscribe(SC.UpdateAcid);
            ManStatus.spreadUpdate.Subscribe(SC.UpdateSpread);
            return SC;
        }
        public static bool CanAddNewTo(GameObject GO, StatusType inflicted, out bool computing)
        {
            bool isWeapon = GO.GetComponent<ModuleWeapon>();
            bool isCab = GO.GetComponent<ModuleTechController>();
            bool antigrav = GO.GetComponent<ModuleAntiGravityEngine>();
            bool bubble = GO.GetComponent<ModuleShieldGenerator>();
            bool gyro = GO.GetComponent<ModuleGyro>();
            bool beam = GO.GetComponent<ModuleItemHolderBeam>();
            bool energy = GO.GetComponent<ModuleEnergy>();
            computing = isCab || bubble || antigrav || gyro || beam || (energy && !isWeapon);
            switch (inflicted)
            {
                case StatusType.EMF:
                    if (!isWeapon && !isCab && !antigrav && !bubble && !gyro &&
                        !beam && !energy && !GO.GetComponent<ModuleWheels>())
                        return false;
                    if (!computing && GO.GetComponent<Damageable>().MaxHealth < EMPGroundingRateSec)
                        return false;
                    break;
                case StatusType.Jamming:
                    if (!isWeapon)
                        return false;
                    break;
                case StatusType.Hacked:
                    if (!isWeapon && !isCab && !bubble && !gyro &&
                        !beam && !GO.GetComponent<ModuleRadar>())
                        return false;
                    if (isWeapon && !isCab && GO.GetComponent<TankBlock>().filledCells.Length <= MinimumVolRqdToHack)
                        return false;
                    break;
            }
            return true;
        }

        public static StatusCondition InitFilmShield(Damageable damageable, float value)
        {
            StatusCondition SC = damageable.GetComponent<StatusCondition>();
            if ((bool)SC)
            {
                SC.CancelInvoke("Remove");
                SC.AddToVal(value);
                return SC;
            }
            GameObject GO = damageable.gameObject;
            var MS = damageable.GetComponent<MaterialSwapper>();
            if (!(bool)MS)
                return null;

            SC = GO.AddComponent<StatusCondition>();
            SC.block = GO.GetComponent<TankBlock>();
            SC.swap = MS;
            SC.Status = StatusType.FilmShield;
            SC.damageable = damageable;
            SC.renders = GO.GetComponentsInChildren<Renderer>();
            SC.originalFragility = GO.GetComponent<ModuleDamage>().m_DamageDetachFragility;
            SC.AddToVal(value);
            return SC;
        }

        // On receiving damage
        public static float RunStatusPre(Damageable dmg, ManDamage.DamageInfo info)
        {
            if (!dmg)
                return 0;
            if (dmg.Invulnerable)
                return 0;
            StatusCondition SC = dmg.Block?.GetComponent<StatusCondition>();
            if (SC)
            {
                if (IsSameTeam(dmg, info))
                {
                    bool helpful = (SC.Status == StatusType.Freezing && (DamageTypesExt)info.DamageType == DamageTypesExt.Fire) ||
                        (SC.Status == StatusType.Overheat && (DamageTypesExt)info.DamageType == DamageTypesExt.Cryo) ? true : false;
                    if (!helpful)
                        return 1;
                }
                switch (SC.Status)
                {
                    case StatusType.Pry:
                        if (info.DamageType == ManDamage.DamageType.Cutting)
                        {
                            if (!(info.Source is Explosion))
                                SC.AddToValDetach(info.Damage);
                            return 1;
                        }
                        break;
                    case StatusType.Overheat:
                        if (info.DamageType == (ManDamage.DamageType)DamageTypesExt.Fire)
                        {
                            SC.AddToVal(info.Damage * 2, !(info.Source is Explosion));
                            return 1;
                        }
                        if (dmg.MaxHealth > 0)
                            info.ApplyDamageMultiplier(1 + (SC.impactValue / (dmg.MaxHealth * 2)));
                        break;
                    case StatusType.Freezing:
                        if (info.DamageType == (ManDamage.DamageType)DamageTypesExt.Cryo)
                        {
                            SC.AddToVal(info.Damage * 4, !(info.Source is Explosion));
                            return 0.1f;
                        }
                        break;
                    case StatusType.EMF:
                        switch (info.DamageType)
                        {
                            case (ManDamage.DamageType)DamageTypesExt.EMP:
                                SC.AddToVal(info.Damage * 12, !(info.Source is Explosion));
                                return 0;
                            case (ManDamage.DamageType)DamageTypesExt.Scramble:
                                SC.AddToVal(info.Damage);
                                return 0;
                        }
                        break;
                    case StatusType.Jamming:   // Cannot stack like normal EMP
                        switch (info.DamageType)
                        {
                            case (ManDamage.DamageType)DamageTypesExt.Scramble:
                                //SC.AddToVal(info.Damage * 8);
                                return 0;
                        }
                        break;
                    case StatusType.Hacked: 
                        switch (info.DamageType)
                        {
                            case (ManDamage.DamageType)DamageTypesExt.Hack:
                                if (info.SourceTank)
                                    SC.AddToVal(info.Damage * 0.5f, info.SourceTank.Team);
                                else
                                    SC.AddToVal(info.Damage * 0.5f);
                                return 0;
                        }
                        break;
                    case StatusType.Acid:
                        switch (info.DamageType)
                        {
                            case (ManDamage.DamageType)DamageTypesExt.Acid:
                                if (info.SourceTank)
                                    SC.AddToVal(info.Damage * 0.5f);
                                return 0.1f;
                        }
                        break;

                    case StatusType.FilmShield:
                        switch (info.DamageType)
                        {
                            case (ManDamage.DamageType)DamageTypesExt.EMP:
                                SC.SubFromVal(info.Damage * 5, (StatusType)((int)info.DamageType - 4));
                                return 0;
                            case ManDamage.DamageType.Fire:
                            case (ManDamage.DamageType)DamageTypesExt.Cryo:
                            case (ManDamage.DamageType)DamageTypesExt.Scramble:
                            case (ManDamage.DamageType)DamageTypesExt.Hack:
                                SC.SubFromVal(info.Damage * 3, (StatusType)((int)info.DamageType - 4));
                                return 0;
                        }
                        return SC.SubFromVal(info.Damage * 2);
                }
                switch (info.DamageType)
                {
                    case (ManDamage.DamageType)DamageTypesExt.Cryo:
                    case (ManDamage.DamageType)DamageTypesExt.Scramble:
                    case (ManDamage.DamageType)DamageTypesExt.Hack:
                        return 0;
                    case (ManDamage.DamageType)DamageTypesExt.EMP:
                        if (dmg.DamageableType == ManDamage.DamageableType.Shield)
                        {
                            if (SC.redDelay <= 0)
                                SC.redDelay = RedDelay;
                            return 1;
                        }
                        return 0;
                    case (ManDamage.DamageType)DamageTypesExt.Acid:
                        return 0.1f;
                    default:
                        if (SC.redDelay <= 0)
                            SC.redDelay = RedDelay;
                        break;
                }
            }
            else
            {
                switch (info.DamageType)
                {
                    case ManDamage.DamageType.Cutting:
                        SC = InitOrGet(dmg, info, StatusType.Pry);
                        if (SC)
                        {
                            SC.AddToValDetach(info.Damage);
                        }
                        break;
                    case ManDamage.DamageType.Fire:
                        SC = InitOrGet(dmg, info, StatusType.Overheat);
                        if (SC)
                        {
                            SC.AddToVal(info.Damage * 2, !(info.Source is Explosion));
                        }
                        break;
                    case (ManDamage.DamageType)DamageTypesExt.Cryo:
                        SC = InitOrGet(dmg, info, StatusType.Freezing);
                        if (SC)
                        {
                            SC.AddToVal(info.Damage * 4, !(info.Source is Explosion));
                        }
                        return 0.1f;
                    case (ManDamage.DamageType)DamageTypesExt.EMP:
                        SC = InitOrGet(dmg, info, StatusType.EMF);
                        if (SC)
                        {
                            SC.AddToVal(info.Damage * 8, !(info.Source is Explosion));
                        }
                        return 0;
                    case (ManDamage.DamageType)DamageTypesExt.Scramble:
                        SC = InitOrGet(dmg, info, StatusType.Jamming);
                        if (SC)
                        {
                            SC.AddToVal(info.Damage * 64);
                        }
                        return 0;
                    case (ManDamage.DamageType)DamageTypesExt.Hack:
                        SC = InitOrGet(dmg, info, StatusType.Hacked);
                        if (SC)
                        {   // Firewall
                            if (info.SourceTank)
                                SC.AddToVal(info.Damage * 0.25f, info.SourceTank.Team);
                            else
                                SC.AddToVal(info.Damage * 0.25f);
                        }
                        return 0;
                    case (ManDamage.DamageType)DamageTypesExt.Acid:
                        SC = InitOrGet(dmg, info, StatusType.Acid);
                        if (SC)
                        {
                            if (info.SourceTank)
                                SC.AddToVal(info.Damage);
                        }
                        return 0.1f;
                }
            }
            return 1;
        }
        public static void RunStatusPost(Damageable dmg, ManDamage.DamageInfo info)
        {
            if (IsSameTeam(dmg, info))
                return;
            //StatusCondition SC;
            switch (info.DamageType)
            {
                case ManDamage.DamageType.Fire:
                    return;
                case (ManDamage.DamageType)DamageTypesExt.Cryo:
                    return;
                case (ManDamage.DamageType)DamageTypesExt.EMP:
                    return;
            }
        }


        public void AddToVal(float addVal)
        {
            float dHealth = damageable.MaxHealth * 2;
            float change = impactValue + Mathf.Abs(addVal);
            if (change < dHealth)
                impactValue = change;
            else
            {
                impactValue = dHealth;
            }
            lastDamageDelay = RecoverDelay;
            //Debug.Log("TweakTech: StatusCondition - Value " + Status + " is now " + impactValue);
        }
        public void AddToVal(float addVal, bool spread)
        {
            float dHealth = damageable.MaxHealth * 2;
            float change = impactValue + Mathf.Abs(addVal);
            if (change < dHealth)
                impactValue = change;
            else
            {
                impactValue = dHealth;
                if (spread)
                    impactExcess += change - dHealth;
            }
            lastDamageDelay = RecoverDelay;
            //Debug.Log("TweakTech: StatusCondition - Value " + Status + " is now " + impactValue);
        }
        public void AddToVal(float addVal, int team)
        {
            float dHealth = damageable.MaxHealth * 2;
            float change = impactValue + Mathf.Abs(addVal);
            if (change < dHealth)
                impactValue = change;
            else
            {
                impactValue = dHealth;
                bool isValid = !ManNetwork.IsNetworked || ManNetwork.IsHost;
                if (block?.tank)
                {
                    if (block.tank.IsPopulation && block.tank.blockman.IterateBlockComponents<ModuleTechController>().Count() == 1)
                    {
                        if (isValid)
                            block.tank.SetTeam(team);
                        Debug.Log("TweakTech: StatusCondition - HACKED");
                        OnRemove();
                        return;
                    }
                    else
                    {
                        if (isValid)
                        {
                            ManLooseBlocks.inst.HostDetachBlock(block, false, true);
                            block.visible.SetInteractionTimeout(1 / HackRecoveryPercentSec);
                        }
                        ManSFX.inst.StopMiscLoopingSFX(ManSFX.MiscSfxType.PayloadIncoming, transform);
                    }
                }
            }
            lastDamageDelay = RecoverDelay;
            //Debug.Log("TweakTech: StatusCondition - Value " + Status + " is now " + impactValue);
        }
        public void AddToValDetach(float addVal)
        {
            float dHealth = damageable.MaxHealth * 2;
            float change = impactValue + Mathf.Abs(addVal);
            if (change < dHealth)
                impactValue = change;
            else
            {
                impactValue = dHealth;
                if (block?.tank)
                {
                    if (!ManNetwork.IsNetworked || ManNetwork.IsHost)
                    {
                        ManLooseBlocks.inst.HostDetachBlock(block, false, true);
                        block.visible.SetInteractionTimeout(1 / PryRecoveryPercentSec);
                    }
                }
            }
            lastDamageDelay = RecoverDelay;
            //Debug.Log("TweakTech: StatusCondition - Value " + Status + " is now " + impactValue);
        }
        public float SubFromVal(float subVal)
        {
            float change = impactValue - subVal;
            if (change >= 0)
                impactValue = change;
            else
            {
                impactValue = 0;
                OnRemove();
                return (-change) / subVal;
            }
            return 0;
        }
        public void SubFromVal(float subVal, StatusType changeStat)
        {
            float change = impactValue - subVal;
            if (change >= 0)
                impactValue = change;
            else
            {
                impactValue = -change;
                float dHealth = damageable.MaxHealth * 2;
                if (impactValue > dHealth)
                    impactValue = dHealth;
                Status = changeStat;
            }
        }
        public void ChangeStat(float startVal, StatusType changeStat)
        {
            impactValue = startVal;
            Status = changeStat;
        }


        // Nearby Spreading
        private void DiffuseNearby(float value)
        {
            if (block.NumConnectedAPs == 0)
                return;
            switch (Status)
            {
                case StatusType.Overheat:
                case StatusType.Freezing:
                case StatusType.EMF:
                    spreadLastFrame = 5;
                    foreach (TankBlock tb in block.ConnectedBlocksByAP)
                    {
                        if ((bool)tb)
                        {
                            if (SpreadStatus(tb, Status, out StatusCondition SC))
                                cacheSpread.Add(SC);
                        }
                    }
                    int blockTotal = cacheSpread.Count;
                    if (blockTotal < 1)
                        return;
                    cacheSpread.Shuffle();
                    value /= blockTotal;
                    foreach (StatusCondition SC in cacheSpread)
                    {
                        SC.HandleSpreadAdd(value, Status);
                    }
                    cacheSpread.Clear();
                    break;
            }
        }
        public bool SpreadStatus(TankBlock TB, StatusType inflicted, out StatusCondition SC)
        {
            SC = TB.GetComponent<StatusCondition>();
            if ((bool)SC)
            {
                if (SC.CanSpreadAdd(this, inflicted))
                {
                    SC.CancelInvoke("Remove");
                    return true;
                }
                return false;
            }
            GameObject GO = TB.gameObject;
            var MS = TB.GetComponent<MaterialSwapper>();
            if (!(bool)MS)
                return false;
            if (!CanAddNewTo(GO, inflicted, out bool precision))
                return false;
            SC = GO.AddComponent<StatusCondition>();
            SC.precisionComponents = precision;
            SC.block = GO.GetComponent<TankBlock>();
            SC.swap = MS;
            SC.Status = inflicted;
            SC.damageable = GO.GetComponent<Damageable>();
            SC.renders = GO.GetComponentsInChildren<Renderer>();
            SC.originalFragility = GO.GetComponent<ModuleDamage>().m_DamageDetachFragility;
            ManStatus.acidUpdate.Subscribe(SC.UpdateAcid);
            ManStatus.PrepForSub(SC.UpdateSpread);
            return true;
        }
        private bool CanSpreadAdd(StatusCondition origin, StatusType inflicted)
        {
            switch (Status)
            {
                case StatusType.Overheat:
                    switch (inflicted)
                    {
                        case StatusType.Overheat:
                            if (SpreadAddAllowed(origin))
                            {
                                return true;
                            }
                            else
                                return false;

                        case StatusType.Freezing:
                        case StatusType.Acid:
                            return true;
                    }
                    break;
                case StatusType.Freezing:
                    switch (inflicted)
                    {
                        case StatusType.Overheat:
                            return true;
                        case StatusType.Freezing:
                            if (SpreadAddAllowed(origin))
                            {
                                return true;
                            }
                            else
                                return false;
                    }
                    break;
                case StatusType.EMF:
                    switch (inflicted)
                    {
                        case StatusType.EMF:
                        case StatusType.Jamming:
                            if (SpreadAddAllowed(origin))
                            {
                                return true;
                            }
                            else
                                return false;
                    }
                    break;

                case StatusType.FilmShield:
                    switch (inflicted)
                    {
                        case StatusType.FilmShield:
                            if (SpreadAddAllowed(origin))
                            {
                                return true;
                            }
                            else
                                return false;
                    }
                    return true;
            }
            return false;
        }
        private bool SpreadAddAllowed(StatusCondition SCO)
        {
            //return impactPercent < 0.75f;
            return spreadLastFrame == 0;
        }
        private void HandleSpreadAdd(float value, StatusType inflicted)
        {
            switch (Status)
            {
                case StatusType.Overheat:
                    switch (inflicted)
                    {
                        case StatusType.Overheat:
                            AddToVal(value, true);
                            break;
                        case StatusType.Freezing:
                            SubFromVal(value, inflicted);
                            break;
                        case StatusType.Acid:
                            ChangeStat(value, inflicted);
                            break;
                    }
                    break;
                case StatusType.Freezing:
                    switch (inflicted)
                    {
                        case StatusType.Overheat:
                            SubFromVal(value, inflicted);
                            break;
                        case StatusType.Freezing:
                            AddToVal(value, true);
                            break;
                    }
                    break;
                case StatusType.EMF:
                    switch (inflicted)
                    {
                        case StatusType.EMF:
                            AddToVal(value, true);
                            break;
                        case StatusType.Jamming:
                            AddToVal(value);
                            break;
                    }
                    break;

                case StatusType.FilmShield:
                    switch (inflicted)
                    {
                        case StatusType.EMF:
                            SubFromVal(value, inflicted);
                            break;
                        case StatusType.Overheat:
                        case StatusType.Freezing:
                        case StatusType.Jamming:
                        case StatusType.Hacked:
                            SubFromVal(value, inflicted);
                            break;
                        case StatusType.FilmShield:
                            AddToVal(value, true);
                            break;
                    }
                    break;
            }
        }



        public void OnRemove()
        {
            impactValue = 0;
            ResetRenders();
            ManStatus.acidUpdate.Unsubscribe(UpdateAcid);
            ManStatus.spreadUpdate.Unsubscribe(UpdateSpread);
            ManSFX.inst.StopMiscLoopingSFX(ManSFX.MiscSfxType.PayloadIncoming, transform);
            swap.enabled = true;
            GetComponent<ModuleDamage>().m_DamageDetachFragility = originalFragility;
            swap.SwapMaterialTime(false);
            swap.SwapMaterialDamage(true);
            SetAllWorkingComponentsOff(false);
            Invoke("Remove", 0.001f);
        }
        public void Remove()
        {
            impactValue = 0;
            //Debug.Log("TweakTech: StatusCondition - Remove");
            swap.enabled = true;
            swap.SwapMaterialDamage(false);
            swap.SwapMaterialTime(ManTimeOfDay.inst.NightTime);
            Destroy(this);
        }

        public float UpdateRenders()
        {
            //Debug.Log("TweakTech: StatusCondition - UpdateRenders");
            swap.enabled = false;
            float emitVal = Mathf.Min(1, impactValue / (damageable.MaxHealth * 2f));
            //MaterialPropertyBlock MPB = propInitcache;
            float addVal = Mathf.Round(3 * emitVal);
            //Color caseC = MPB.GetColor("_EmissionColor");
            Vector4 vec4 = propInitcache.GetVector(propID);
            skinV = (float)(block.GetSkinIndex() % 8);
            skinU = (float)(block.GetSkinIndex() / 8);
            vec4.z = skinU;
            vec4.w = skinV;
            if (redDelay > 0.5f)
            {
                vec4.x = 1.5f * t2d2H;
                vec4.y = emitVal;
            }
            else
            {
                switch (Status)
                {
                    case StatusType.Pry:
                        vec4.x = 1.5f * t2d2H;
                        vec4.y = emitVal;
                        break;
                    case StatusType.Overheat:
                        vec4.x = (9.5f - addVal) * t2d2H;
                        vec4.y = emitVal;
                        break;
                    case StatusType.Freezing:
                        vec4.x = (10.5f + addVal) * t2d2H;
                        vec4.y = 0.1f;
                        break;
                    case StatusType.EMF:
                        if (0.5f < emitVal)
                        {
                            vec4.x = 14.5f * t2d2H;
                            vec4.y = 0.75f;
                        }
                        else
                        {
                            vec4.x = 15.5f * t2d2H;
                            vec4.y = 0.25f;
                        }
                        break;
                    case StatusType.Jamming:
                        if (0.5f < emitVal)
                        {
                            vec4.x = 17.5f * t2d2H;
                            vec4.y = 0.75f;
                        }
                        else
                        {
                            vec4.x = 16.5f * t2d2H;
                            vec4.y = 0.25f;
                        }
                        break;
                    case StatusType.Hacked:
                        vec4.x = 18.5f * t2d2H;
                        vec4.y = 0.5f;
                        break;
                    case StatusType.Acid:
                        if (0.5f < emitVal)
                        {
                            vec4.x = 19.5f * t2d2H;
                            vec4.y = 0.75f;
                        }
                        else
                        {
                            vec4.x = 20.5f * t2d2H;
                            vec4.y = 0.25f;
                        }
                        break;
                    case StatusType.FilmShield:
                        vec4.x = (22.5f + Mathf.Round(2 * emitVal)) * t2d2H;
                        vec4.y = emitVal;
                        break;
                }
            }
            propInitcache.SetVector(propID, vec4);
            foreach (Renderer ren in renders)
            {
                try
                {
                    ren.SetPropertyBlock(propInitcache);
                }
                catch { }
            }
            impactPercent = emitVal;
            return emitVal;
        }
        public void ResetRenders()
        {
            Vector4 vec4 = new Vector4(0.5f * t2d2H, swap.MinEmitScale, skinU, skinV);
            propInitcache.SetVector(propID, vec4);
            foreach (Renderer ren in renders)
            {
                try
                {
                    ren.SetPropertyBlock(propInitcache);
                }
                catch { }
            }
        }


        private static readonly FieldInfo dirtyGrav = typeof(ModuleAntiGravityEngine).GetField("m_GravityTargetsDirty", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        private static readonly FieldInfo switchGrav = typeof(ModuleAntiGravityEngine).GetField("m_SwitchedOn", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        public void SetAllWorkingComponentsOff(bool turnOff, bool forceFull = false)
        {
            if (turnOff)
            {
                if (forceFull)
                    GetComponent<ModuleDamage>().m_DamageDetachFragility = 0;
                else
                    GetComponent<ModuleDamage>().m_DamageDetachFragility = (impactPercent / 2) + originalFragility;
            }
            if (disabledComponents == turnOff)
                return;
            disabledComponents = turnOff;
            if (!block.tank && turnOff)
            {
                SetAllWorkingComponentsOff(false);
                return;
            }
            float Strength;
            if (forceFull)
                Strength = 1;
            else
                Strength = impactPercent / 2;
            if (turnOff)
            {
                if (forceFull)
                {
                    var aGrav = GetComponent<ModuleAntiGravityEngine>();
                    if (aGrav)
                    {
                        aGrav.enabled = false;
                        if (aGrav.SwitchedOn())
                        {
                            switchGrav.SetValue(aGrav, false);
                            dirtyGrav.SetValue(aGrav, true);
                        }
                    }
                    if (GetComponent<ModuleWing>())
                        GetComponent<ModuleWing>().enabled = false;
                }
            }
            else
            {
                GetComponent<ModuleDamage>().m_DamageDetachFragility = originalFragility;

                if (GetComponent<ModuleWing>())
                    GetComponent<ModuleWing>().enabled = true;

                if (GetComponent<ModuleAntiGravityEngine>())
                    GetComponent<ModuleAntiGravityEngine>().enabled = true;

                //BlockTweak.ApplyToBlockLocal(block);
            }
        }

        private static readonly FieldInfo MWGCooldown = typeof(ModuleWeaponGun).GetField("m_ShotTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo MWTCooldown = typeof(ModuleWeaponTeslaCoil).GetField("m_ChargeTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        public void MalfunctionUpdate()
        {
            allowModuleUpdate = UnityEngine.Random.Range(0, 100) >= impactPercent * 40;
            if (!allowModuleUpdate)
            {
                ModuleWeaponGun MWG = GetComponent<ModuleWeaponGun>();
                if (MWG)
                    MWGCooldown.SetValue(MWG, (float)MWGCooldown.GetValue(MWG) + Time.deltaTime);
                ModuleWeaponTeslaCoil MWT = GetComponent<ModuleWeaponTeslaCoil>();
                if (MWT)
                    MWTCooldown.SetValue(MWT, (float)MWTCooldown.GetValue(MWT) + Time.deltaTime);
            }
        }
        public void HaltUtils(bool Freeze)
        {
            allowModuleUpdate = !Freeze;
            if (Freeze)
            {
                ModuleWeaponGun MWG = GetComponent<ModuleWeaponGun>();
                if (MWG)
                    MWGCooldown.SetValue(MWG, (float)MWGCooldown.GetValue(MWG) + Time.deltaTime);
                ModuleWeaponTeslaCoil MWT = GetComponent<ModuleWeaponTeslaCoil>();
                if (MWT)
                    MWTCooldown.SetValue(MWT, (float)MWTCooldown.GetValue(MWT) + Time.deltaTime);
            }
        }

        internal void Update()
        { //obujecots sounds like a neat word
            float resistance = damageable.MaxHealth * 2;
            bool isRecovering;
            if (redDelay > 0)
                redDelay -= Time.deltaTime;
            if (lastDamageDelay > 0)
            {
                lastDamageDelay -= Time.deltaTime;
                isRecovering = true;
            }
            else
                isRecovering = false;
            bool frozen;
            switch (Status)
            {
                case StatusType.Pry:
                    if (!isRecovering)
                        impactValue -= Time.deltaTime * PryRecoveryPercentSec * resistance;
                    UpdateRenders();
                    SetAllWorkingComponentsOff(false);
                    allowModuleUpdate = true;
                    break;
                case StatusType.Overheat:
                    if (!isRecovering)
                        impactValue -= Time.deltaTime * TempDisspationPercentSec * resistance;
                    UpdateRenders();
                    SetAllWorkingComponentsOff(false);
                    allowModuleUpdate = true;
                    break;
                case StatusType.Freezing:
                    if (!isRecovering)
                        impactValue -= Time.deltaTime * TempDisspationPercentSec * resistance;
                    UpdateRenders();
                    SetAllWorkingComponentsOff(true);
                    MalfunctionUpdate();
                    break;
                case StatusType.EMF:
                    if (precisionComponents)
                        impactValue -= Time.deltaTime * EMPGroundingRatePrecisePercentSec * resistance;
                    else
                        impactValue -= Time.deltaTime * EMPGroundingRateSec;
                    frozen = 0.5f < UpdateRenders();
                    SetAllWorkingComponentsOff(frozen, true);
                    HaltUtils(frozen);
                    if (GetComponent<ModuleTechController>())
                    {
                        if (warningRepeat >= 3)
                        {
                            ManSFX.inst.StopMiscLoopingSFX(ManSFX.MiscSfxType.PayloadIncoming, block.trans);
                            ManSFX.inst.PlayMiscLoopingSFX(ManSFX.MiscSfxType.PayloadIncoming, block.trans);
                            warningRepeat = 0;
                        }
                        warningRepeat += Time.deltaTime;
                    }
                    break;
                case StatusType.Jamming:
                    impactValue -= Time.deltaTime * UnJammingRateSec;
                    frozen = 0.5f < UpdateRenders();
                    allowModuleUpdate = !frozen;
                    HaltUtils(frozen);
                    break;
                case StatusType.Hacked:
                    bool Cracked = 0.95f < UpdateRenders();
                    SetAllWorkingComponentsOff(false);
                    allowModuleUpdate = true;
                    if (Cracked)
                    {
                        impactValue -= Time.deltaTime * HackRecoveryPercentSec * resistance;
                    }
                    else
                    {
                        if (GetComponent<ModuleTechController>())
                        {
                            if (warningRepeat >= 3)
                            {
                                ManSFX.inst.StopMiscLoopingSFX(ManSFX.MiscSfxType.PayloadIncoming, block.trans);
                                ManSFX.inst.PlayMiscLoopingSFX(ManSFX.MiscSfxType.PayloadIncoming, block.trans);
                                warningRepeat = 0;
                            }
                            warningRepeat += Time.deltaTime;
                        }
                        if (!isRecovering)
                            impactValue -= Time.deltaTime * HackRecoveryPercentSec * resistance;
                    }
                    break;
                case StatusType.Acid:
                    if (!isRecovering)
                        impactValue -= Time.deltaTime * AcidVaporPercentSec * resistance;
                    UpdateRenders();
                    break;
                case StatusType.FilmShield:
                    impactValue -= Time.deltaTime * FilmShieldDrain * resistance;
                    UpdateRenders();
                    break;
            }
            if (impactValue <= 0)
            {
                impactValue = 0;
                OnRemove();
            }
        }

        internal void UpdateAcid()
        {
            if (Status != StatusType.Acid)
                return;
            if (ManNetwork.IsHost)
            {
                ManDamage.DamageInfo newDMG = acidPrefab.Clone();
                newDMG.ApplyDamageMultiplier(block.filledCells.Length * impactPercent);
                ManDamage.inst.DealDamage(newDMG, damageable);
            }
            else
            {
                block.damage.MultiplayerFakeDamagePulse();
            }
        }
        internal void UpdateSpread()
        {
            switch (Status)
            {
                case StatusType.Overheat:
                case StatusType.Freezing:
                case StatusType.EMF:
                    if (impactExcess > 0)
                        DiffuseNearby(impactExcess);
                    break;
            }
            if (spreadLastFrame > 0)
                spreadLastFrame--;
            impactExcess = 0;
        }
    }

    public enum StatusType
    {
        None,
        // Detrimental
        Overheat,
        Freezing,
        EMF,
        Jamming,
        Hacked,
        Acid,
        Pry,        // Cutting weapons
        // Beneficial
        FilmShield,
    }
}
