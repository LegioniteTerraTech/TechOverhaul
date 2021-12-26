using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TweakTech
{
	/// <summary>
	/// this is a literal copy of what matters from ShotgunRound because ApplyDamageToObjectsInArc is nearly impossible to override
	///    please don't sue me
	/// </summary>
	public class ShotgunOverride : ShotgunRound
	{
		private struct ShotHit
		{
			public Visible visible;

			public Vector3 hitPoint;
		}
		private class RaycastHitDistanceComparer : IComparer<RaycastHit>
		{
			public Vector3 originPosition;

			public int Compare(RaycastHit hitA, RaycastHit hitB)
			{
				return (hitA.point - originPosition).sqrMagnitude.CompareTo((hitB.point - originPosition).sqrMagnitude);
			}
		}

		private Vector3 m_LastFireDirection;
		private Vector3 m_LastFireSpin;

		private static RaycastHit[] s_RaycastHits = new RaycastHit[32];
		private static Dictionary<Damageable, ShotHit> s_CollectedTargets = new Dictionary<Damageable, ShotHit>(); 
		private static HashSet<Damageable> s_HandledBlockers = new HashSet<Damageable>();
		private static RaycastHitDistanceComparer s_RaycastHitDistanceComparer = new RaycastHitDistanceComparer();
		
		/// <summary>
		/// Call this before ANYTHING happens in ShotgunRound
		/// </summary>
		/// <param name="fireDirection"></param>
		/// <param name="fireData"></param>
		/// <param name="weapon"></param>
		/// <param name="shooter"></param>
		/// <param name="seekingRounds"></param>
		/// <param name="replayRounds"></param>
		public override void Fire(Vector3 fireDirection, FireData fireData, ModuleWeapon weapon, Tank shooter = null, bool seekingRounds = false, bool replayRounds = false)
		{
			if (!replayRounds)
			{
				m_LastFireDirection = fireDirection;
			}
			else
			{
				fireDirection = m_LastFireDirection;
			}
			m_LastFireSpin = Vector3.zero;
			if (ManNetwork.IsHost)
			{
				FireDataShotgun fireDataShotgun = fireData as FireDataShotgun;
				ApplyDamageToObjectsInArc(fireDataShotgun, weapon, shooter);
			}
			this.Recycle();
		}

		// IGNORE SHIELDS DAMMIT
		private void ApplyDamageToObjectsInArc(FireDataShotgun shotgunFireData, ModuleWeapon weapon, Tank shooter)
		{
			Transform transform = base.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			float num = Mathf.Cos(shotgunFireData.m_ShotArc * ((float)Math.PI / 180f));
			float num2 = shotgunFireData.m_ShotFullDamageRange * shotgunFireData.m_ShotFullDamageRange;
			float num3 = (shotgunFireData.m_ShotMaxRange - shotgunFireData.m_ShotMinRange) / 3f;
			for (int i = 0; i < 3; i++)
			{
				float num4 = shotgunFireData.m_ShotMinRange + (float)i * num3;
				float num5 = num4 + num3;
				float num6 = 2f * num5 * num5;
				float num7 = Mathf.Sqrt(num6 - num6 * num) * 0.5f;
				num4 = Mathf.Min(num4 + num7, num5);
				float maxDistance = num5 - num4;
				int layerMask = Singleton.Manager<ManVisible>.inst.VisiblePickerMaskNoTechs;
				int num8;
				while (true)
				{
					num8 = Physics.SphereCastNonAlloc(transform.position + forward * num4, num7, forward, s_RaycastHits, maxDistance, layerMask, QueryTriggerInteraction.Collide);
					if (num8 < s_RaycastHits.Length)
					{
						break;
					}
					Array.Resize(ref s_RaycastHits, num8 * 2);
				}
				//int num9 = Globals.inst.layerShieldBulletsFilter;
				for (int j = 0; j < num8; j++)
				{
					RaycastHit raycastHit = s_RaycastHits[j];
					Visible visible = Singleton.Manager<ManVisible>.inst.FindVisible(raycastHit.collider);
					if (!visible.IsNotNull())
					{
						continue;
					}
					Damageable damageable = visible.damageable;
					ShotHit value;
					if (damageable.IsNotNull() && !s_CollectedTargets.ContainsKey(damageable))
					{
						Dictionary<Damageable, ShotHit> dictionary2 = s_CollectedTargets;
						Damageable key2 = damageable;
						value = new ShotHit
						{
							visible = visible,
							hitPoint = visible.centrePosition
						};
						dictionary2.Add(key2, value);
					}
				}
			}
			foreach (KeyValuePair<Damageable, ShotHit> s_CollectedTarget in s_CollectedTargets)
			{
				Damageable key3 = s_CollectedTarget.Key;
				Visible visible2 = s_CollectedTarget.Value.visible;
				if (visible2.IsNull())
				{
					continue;
				}
				bool flag = visible2.type == ObjectTypes.Block && visible2.damageable != key3;
				if (!(visible2.block == null) && !(visible2.block.tank == null) && !(visible2.block.tank != shooter) && !shooter.IsEnemy(visible2.block.tank.Team))
				{
					continue;
				}
				Vector3 vector = (flag ? s_CollectedTarget.Value.hitPoint : visible2.centrePosition);
				Vector3 damageDirection = vector - position;
				float sqrMagnitude = damageDirection.sqrMagnitude;
				float num10 = 1f;
				if (sqrMagnitude > num2)
				{
					num10 = Mathf.InverseLerp(shotgunFireData.m_ShotFullDamageRange, shotgunFireData.m_ShotMaxRange, Mathf.Sqrt(sqrMagnitude));
				}
				if (!(num10 > 0f))
				{
					continue;
				}
				float unblockedAOEDamageFractionAtTarget = GetUnblockedAOEDamageFractionAtTarget(weapon.block.visible, position, key3, vector);
				if (unblockedAOEDamageFractionAtTarget > 0f)
				{
					Vector3 hitPosition = vector;
					float num11 = Mathf.Lerp(shotgunFireData.m_MinDamagePercent, shotgunFireData.m_MaxDamagePercent, num10);
					float damage = (float)m_Damage * num11 * unblockedAOEDamageFractionAtTarget;
					float kickbackStrength = num10 * unblockedAOEDamageFractionAtTarget * shotgunFireData.m_TargetKickbackStrength;
					if (visible2.type == ObjectTypes.Scenery)
					{
						kickbackStrength = 0f;
					}
					Singleton.Manager<ManDamage>.inst.DealDamage(key3, damage, m_DamageType, weapon, shooter, hitPosition, damageDirection, kickbackStrength, shotgunFireData.m_TargetKickbackDuration);
				}
			}
			s_CollectedTargets.Clear();
		}

		private float GetUnblockedAOEDamageFractionAtTarget(Visible damageSource, Vector3 damageOrigin, Damageable target, Vector3 targetPosition)
		{
			float num = 1f;
			Vector3 vector = targetPosition - damageOrigin;
			float magnitude = vector.magnitude;
			int num2 = 0;
			if (magnitude > 0f)
			{
				num2 = Physics.RaycastNonAlloc(base.transform.position, vector / magnitude, s_RaycastHits, magnitude, -5, QueryTriggerInteraction.Collide);
			}
			if (num2 > 0)
			{
				s_RaycastHitDistanceComparer.originPosition = base.transform.position;
				Array.Sort(s_RaycastHits, 0, num2, s_RaycastHitDistanceComparer);
				for (int i = 0; i < num2; i++)
				{
					RaycastHit raycastHit = s_RaycastHits[i];
					if (raycastHit.collider.gameObject.IsTerrain())
					{
						num = 0f;
						break;
					}
					Damageable damageable = ((raycastHit.collider.gameObject.layer != (int)Globals.inst.layerShieldBulletsFilter) ? Singleton.Manager<ManVisible>.inst.FindVisible(raycastHit.collider)?.damageable : raycastHit.collider.GetComponentInParents<Damageable>(thisObjectFirst: true));
					if (damageable.IsNotNull())
					{
						if (damageable == target)
						{
							break;
						}
						if (!s_HandledBlockers.Contains(damageable) && damageable != null && damageable.GetComponent<Visible>() != damageSource)
						{
							s_HandledBlockers.Add(damageable);
							float aoEDamageBlockPercent = damageable.AoEDamageBlockPercent;
							num *= 1f - aoEDamageBlockPercent;
						}
					}
					if (num < 0.1f)
					{
						num = 0f;
						break;
					}
				}
				s_HandledBlockers.Clear();
			}
			return num;
		}
	}
}
