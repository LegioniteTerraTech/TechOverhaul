using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace TweakTech
{
    public class StatusCondition : MonoBehaviour
    {   // A Block cannot have more than two statuses at once
        public static float HeatDisspationPercentSec = 0.25f;
        public static float EMPGroundingRateGroundSec = 100;
        public static float EMPGroundingRateAirSec = 50;

        private static int propID = -24;
        private static MaterialPropertyBlock propInitcache;
        public static Texture2D t2d2;
        public static float t2d2H = 1;
        private static FieldInfo prop = typeof(MaterialSwapper).GetField("s_matPropId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        private static FieldInfo propInit = typeof(MaterialSwapper).GetField("s_matProp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        private static FieldInfo colorsForMore = typeof(ManTechMaterialSwap).GetField("m_BlockDamageCLUT", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        private static FieldInfo colorsForMoreH = typeof(ManTechMaterialSwap).GetField("m_BlockDamageScaleV", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);


        public Damageable damageable;
        public MaterialSwapper swap;
        public Renderer[] renders;
        public StatusType Status = StatusType.None;
        public float impactValue = 0;

        public static void InitNewStatus()
        {
            propID = (int)prop.GetValue(null);
            propInitcache = (MaterialPropertyBlock)propInit.GetValue(null);
            t2d2 = new Texture2D(1, 15);
            t2d2.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
            t2d2.SetPixel(0, 1, Globals.inst.moduleDamageParams.DamageColour);
            t2d2.SetPixel(0, 2, Globals.inst.moduleDamageParams.HealColour);
            t2d2.SetPixel(0, 3, new Color(1f, 1f, 1f, 0f));
            t2d2.SetPixel(0, 4, Globals.inst.moduleDamageParams.ScavengeColour);
            t2d2.SetPixel(0, 5, Globals.inst.moduleDamageParams.OutOfShieldColour);

            t2d2.SetPixel(0, 6, new Color(1f, 0.5f, 0f, 0.8f));        // Overheat Color
            t2d2.SetPixel(0, 7, new Color(1f, 0.5f, 0f, 0.6f));        // 
            t2d2.SetPixel(0, 8, new Color(1f, 0.5f, 0f, 0.4f));        // 
            t2d2.SetPixel(0, 9, new Color(1f, 0.5f, 0f, 0.2f));        // 
            t2d2.SetPixel(0, 10, new Color(0.4f, 0.95f, 0.9f, 0.2f));  // Freeze Color
            t2d2.SetPixel(0, 11, new Color(0.4f, 0.95f, 0.9f, 0.4f));  //
            t2d2.SetPixel(0, 12, new Color(0.4f, 0.95f, 0.9f, 0.6f));  //
            t2d2.SetPixel(0, 13, new Color(0.4f, 0.95f, 0.9f, 0.8f));  //
            t2d2.SetPixel(0, 14, new Color(1f, 1f, 0.75f, 0.75f));     // EMP Color
            t2d2.Apply();
            colorsForMore.SetValue(ManTechMaterialSwap.inst, t2d2);
            Shader.SetGlobalTexture("_DamageCLUT", t2d2);
            t2d2H = 1f / (float)t2d2.height;
            colorsForMoreH.SetValue(ManTechMaterialSwap.inst, t2d2H);
        }
        public static StatusCondition InitOrGet(Damageable damageable, StatusType inflicted)
        {
            StatusCondition SC = damageable.GetComponent<StatusCondition>();
            if ((bool)SC)
            {
                if (SC.Status != inflicted)
                    return null;
                SC.CancelInvoke("Remove");
                return SC;
            }
            GameObject GO = damageable.gameObject;
            var MS = damageable.GetComponent<MaterialSwapper>();
            if (!(bool)MS)
                return null;
            SC = GO.AddComponent<StatusCondition>();
            SC.swap = MS;
            SC.Status = inflicted;
            SC.damageable = damageable;
            SC.renders = GO.GetComponentsInChildren<Renderer>();
            SC.swap.enabled = false;
            return SC;
        }

        public void AddToVal(float addVal)
        {
            float dHealth = damageable.MaxHealth * 2;
            if (impactValue < dHealth)
                impactValue += addVal;
            else
            {
                impactValue = dHealth;
            }
        }
        public void ChangeGlobalTex()
        {
            ResetRenders();
            swap.enabled = true;
            swap.SwapMaterialDamage(false);
            enabled = false;
        }


        public void OnRemove()
        {
            ResetRenders();
            swap.enabled = true;
            swap.SwapMaterialDamage(true);
            Invoke("Remove", 0.001f);
        }
        public void Remove()
        {
            swap.enabled = true;
            swap.SwapMaterialDamage(false);
            swap.SwapMaterialTime(ManTimeOfDay.inst.NightTime);
            Destroy(this);
        }

        public void UpdateRenders()
        {
            MaterialPropertyBlock MPB = propInitcache;
            float emitVal = Mathf.Min(1, impactValue / (damageable.MaxHealth * 2));
            float addVal = Mathf.Round(3 * emitVal);
            //Color caseC = MPB.GetColor("_EmissionColor");
            Vector4 vec4= MPB.GetVector(propID);
            switch (Status)
            {
                case StatusType.Overheat:
                    vec4.x = (9.5f - addVal) * t2d2H;
                    vec4.y = emitVal;
                    break;
                case StatusType.Freezing:
                    vec4.x = (10.5f + addVal) * t2d2H;
                    vec4.y = 0.1f;
                    break;
                case StatusType.EMF:
                    vec4.x = 14.5f * t2d2H;
                    vec4.y = 0.75f;
                    break;
            }
            MPB.SetVector(propID, vec4);
            foreach (Renderer ren in renders)
            {
                try
                {
                    ren.SetPropertyBlock(MPB);
                }
                catch { }
            }
        }
        public void ResetRenders()
        {
            MaterialPropertyBlock MPB = propInitcache;
            foreach (Renderer ren in renders)
            {
                try
                {
                    ren.SetPropertyBlock(MPB);
                }
                catch { }
            }
        }

        private void Update()
        {
            swap.enabled = false;
            switch (Status)
            {
                case StatusType.Overheat:
                    UpdateRenders();
                    impactValue -= Time.deltaTime * (HeatDisspationPercentSec * damageable.MaxHealth);
                    if (impactValue <= 0)
                    {
                        impactValue = 0;
                        OnRemove();
                    }
                    return;
                case StatusType.Freezing:
                    return;
                case StatusType.EMF:
                    return;
            }
        }
    }

    public enum StatusType
    {
        None,
        Overheat,
        Freezing,
        EMF,
    }
}
