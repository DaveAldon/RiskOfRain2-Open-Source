﻿using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Toolbot
{
	// Token: 0x020000DE RID: 222
	public class ToolbotDash : BaseCharacterMain
	{
		// Token: 0x06000457 RID: 1111 RVA: 0x00011FE4 File Offset: 0x000101E4
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = this.baseDuration;
			if (base.isAuthority)
			{
				if (base.inputBank)
				{
					this.idealDirection = base.inputBank.aimDirection;
					this.idealDirection.y = 0f;
				}
				this.UpdateDirection();
			}
			if (base.modelLocator)
			{
				base.modelLocator.normalizeToFloor = true;
			}
			if (this.startEffectPrefab && base.characterBody)
			{
				EffectManager.instance.SpawnEffect(this.startEffectPrefab, new EffectData
				{
					origin = base.characterBody.corePosition
				}, false);
			}
			if (base.characterDirection)
			{
				base.characterDirection.forward = this.idealDirection;
			}
			Util.PlaySound(ToolbotDash.startSoundString, base.gameObject);
			base.PlayCrossfade("Body", "BoxModeEnter", 0.1f);
			base.modelAnimator.SetFloat("aimWeight", 0f);
			if (NetworkServer.active)
			{
				base.characterBody.AddBuff(BuffIndex.ArmorBoost);
			}
			HitBoxGroup hitBoxGroup = null;
			Transform modelTransform = base.GetModelTransform();
			if (modelTransform)
			{
				hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Charge");
			}
			this.attack = new OverlapAttack();
			this.attack.attacker = base.gameObject;
			this.attack.inflictor = base.gameObject;
			this.attack.teamIndex = TeamComponent.GetObjectTeam(this.attack.attacker);
			this.attack.damage = ToolbotDash.chargeDamageCoefficient * this.damageStat;
			this.attack.hitEffectPrefab = ToolbotDash.impactEffectPrefab;
			this.attack.forceVector = Vector3.up * ToolbotDash.upwardForceMagnitude;
			this.attack.pushAwayForce = ToolbotDash.awayForceMagnitude;
			this.attack.hitBoxGroup = hitBoxGroup;
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x000121E8 File Offset: 0x000103E8
		public override void OnExit()
		{
			if (base.characterBody)
			{
				if (!this.outer.destroying && this.endEffectPrefab)
				{
					EffectManager.instance.SpawnEffect(this.endEffectPrefab, new EffectData
					{
						origin = base.characterBody.corePosition
					}, false);
				}
				base.PlayAnimation("Body", "BoxModeExit");
				base.characterBody.isSprinting = false;
				if (NetworkServer.active)
				{
					base.characterBody.RemoveBuff(BuffIndex.ArmorBoost);
				}
			}
			if (base.characterMotor && !base.characterMotor.disableAirControlUntilCollision)
			{
				base.characterMotor.velocity += this.GetIdealVelocity();
			}
			if (base.modelLocator)
			{
				base.modelLocator.normalizeToFloor = false;
			}
			Util.PlaySound(ToolbotDash.endSoundString, base.gameObject);
			base.modelAnimator.SetFloat("aimWeight", 1f);
			base.OnExit();
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x000122ED File Offset: 0x000104ED
		private float GetDamageBoostFromSpeed()
		{
			return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x00012310 File Offset: 0x00010510
		private void UpdateDirection()
		{
			if (base.inputBank)
			{
				Vector2 vector = Util.Vector3XZToVector2XY(base.inputBank.moveVector);
				if (vector != Vector2.zero)
				{
					vector.Normalize();
					this.idealDirection = new Vector3(vector.x, 0f, vector.y).normalized;
				}
			}
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x00012373 File Offset: 0x00010573
		private Vector3 GetIdealVelocity()
		{
			return base.characterDirection.forward * base.characterBody.moveSpeed * this.speedMultiplier;
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x0001239C File Offset: 0x0001059C
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration)
			{
				this.outer.SetNextStateToMain();
				return;
			}
			if (base.isAuthority)
			{
				if (base.characterBody)
				{
					base.characterBody.isSprinting = true;
				}
				if (base.skillLocator.special && base.inputBank.skill4.down)
				{
					base.skillLocator.special.ExecuteIfReady();
				}
				this.UpdateDirection();
				if (!this.inHitPause)
				{
					if (base.characterDirection)
					{
						base.characterDirection.moveVector = this.idealDirection;
						if (base.characterMotor && !base.characterMotor.disableAirControlUntilCollision)
						{
							base.characterMotor.rootMotion += this.GetIdealVelocity() * Time.fixedDeltaTime;
						}
					}
					this.attack.damage = this.damageStat * (ToolbotDash.chargeDamageCoefficient * this.GetDamageBoostFromSpeed());
					if (this.attack.Fire(this.victimsStruck))
					{
						Util.PlaySound(ToolbotDash.impactSoundString, base.gameObject);
						this.inHitPause = true;
						this.hitPauseTimer = ToolbotDash.hitPauseDuration;
						base.AddRecoil(-0.5f * ToolbotDash.recoilAmplitude, -0.5f * ToolbotDash.recoilAmplitude, -0.5f * ToolbotDash.recoilAmplitude, 0.5f * ToolbotDash.recoilAmplitude);
						base.PlayAnimation("Gesture, Additive", "BoxModeImpact", "BoxModeImpact.playbackRate", ToolbotDash.hitPauseDuration);
						for (int i = 0; i < this.victimsStruck.Count; i++)
						{
							float num = 0f;
							HealthComponent healthComponent = this.victimsStruck[i];
							CharacterMotor component = healthComponent.GetComponent<CharacterMotor>();
							if (component)
							{
								num = component.mass;
							}
							else
							{
								Rigidbody component2 = healthComponent.GetComponent<Rigidbody>();
								if (component2)
								{
									num = component2.mass;
								}
							}
							if (num >= ToolbotDash.massThresholdForKnockback)
							{
								healthComponent.TakeDamage(new DamageInfo
								{
									attacker = base.gameObject,
									damage = this.damageStat * ToolbotDash.knockbackDamageCoefficient * this.GetDamageBoostFromSpeed(),
									crit = this.attack.isCrit,
									procCoefficient = 1f,
									damageColorIndex = DamageColorIndex.Item,
									damageType = DamageType.Stun1s,
									position = base.characterBody.corePosition
								});
								base.AddRecoil(-0.5f * ToolbotDash.recoilAmplitude * 3f, -0.5f * ToolbotDash.recoilAmplitude * 3f, -0.5f * ToolbotDash.recoilAmplitude * 8f, 0.5f * ToolbotDash.recoilAmplitude * 3f);
								base.healthComponent.TakeDamageForce(this.idealDirection * -ToolbotDash.knockbackForce, true);
								EffectManager.instance.SimpleImpactEffect(ToolbotDash.knockbackEffectPrefab, base.characterBody.corePosition, base.characterDirection.forward, true);
								this.outer.SetNextStateToMain();
								return;
							}
						}
						return;
					}
				}
				else
				{
					base.characterMotor.velocity = Vector3.zero;
					this.hitPauseTimer -= Time.fixedDeltaTime;
					if (this.hitPauseTimer < 0f)
					{
						this.inHitPause = false;
					}
				}
			}
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x0000BBE7 File Offset: 0x00009DE7
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}

		// Token: 0x04000419 RID: 1049
		[SerializeField]
		public float baseDuration;

		// Token: 0x0400041A RID: 1050
		[SerializeField]
		public float speedMultiplier;

		// Token: 0x0400041B RID: 1051
		public static float chargeDamageCoefficient;

		// Token: 0x0400041C RID: 1052
		public static float awayForceMagnitude;

		// Token: 0x0400041D RID: 1053
		public static float upwardForceMagnitude;

		// Token: 0x0400041E RID: 1054
		public static GameObject impactEffectPrefab;

		// Token: 0x0400041F RID: 1055
		public static float hitPauseDuration;

		// Token: 0x04000420 RID: 1056
		public static string impactSoundString;

		// Token: 0x04000421 RID: 1057
		public static float recoilAmplitude;

		// Token: 0x04000422 RID: 1058
		public static string startSoundString;

		// Token: 0x04000423 RID: 1059
		public static string endSoundString;

		// Token: 0x04000424 RID: 1060
		public static GameObject knockbackEffectPrefab;

		// Token: 0x04000425 RID: 1061
		public static float knockbackDamageCoefficient;

		// Token: 0x04000426 RID: 1062
		public static float massThresholdForKnockback;

		// Token: 0x04000427 RID: 1063
		public static float knockbackForce;

		// Token: 0x04000428 RID: 1064
		[SerializeField]
		public GameObject startEffectPrefab;

		// Token: 0x04000429 RID: 1065
		[SerializeField]
		public GameObject endEffectPrefab;

		// Token: 0x0400042A RID: 1066
		private float duration;

		// Token: 0x0400042B RID: 1067
		private float hitPauseTimer;

		// Token: 0x0400042C RID: 1068
		private Vector3 idealDirection;

		// Token: 0x0400042D RID: 1069
		private OverlapAttack attack;

		// Token: 0x0400042E RID: 1070
		private bool inHitPause;

		// Token: 0x0400042F RID: 1071
		private List<HealthComponent> victimsStruck = new List<HealthComponent>();
	}
}
