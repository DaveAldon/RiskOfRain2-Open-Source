﻿using System;
using System.Collections.Generic;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.AI.Walker
{
	// Token: 0x020001E6 RID: 486
	public class Wander : BaseAIState
	{
		// Token: 0x0600097B RID: 2427 RVA: 0x0002FAE4 File Offset: 0x0002DCE4
		private void PickNewTargetLookPosition()
		{
			if (base.bodyInputBank)
			{
				float num = 0f;
				Vector3 aimOrigin = base.bodyInputBank.aimOrigin;
				Vector3 vector = base.bodyInputBank.moveVector;
				if (vector == Vector3.zero)
				{
					vector = UnityEngine.Random.onUnitSphere;
				}
				for (int i = 0; i < 1; i++)
				{
					Vector3 direction = Util.ApplySpread(vector, 0f, 60f, 0f, 0f, 0f, 0f);
					float num2 = 25f;
					Ray ray = new Ray(aimOrigin, direction);
					RaycastHit raycastHit;
					if (Physics.Raycast(ray, out raycastHit, 25f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
					{
						num2 = raycastHit.distance;
					}
					if (num2 > num)
					{
						num = num2;
						this.targetLookPosition = ray.GetPoint(num2);
					}
				}
			}
			this.lookTimer = UnityEngine.Random.Range(0.5f, 4f);
		}

		// Token: 0x0600097C RID: 2428 RVA: 0x0002FBD8 File Offset: 0x0002DDD8
		public override void OnEnter()
		{
			base.OnEnter();
			if (base.ai && base.body)
			{
				this.targetPosition = base.bodyTransform.position;
				NodeGraph nodeGraph = base.ai.GetNodeGraph();
				NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(nodeGraph, (HullMask)(1 << (int)base.body.hullClassification));
				nodeGraphSpider.AddNodeForNextStep(nodeGraph.FindClosestNode(base.bodyTransform.position, base.body.hullClassification));
				for (int i = 0; i < 6; i++)
				{
					nodeGraphSpider.PerformStep();
				}
				List<NodeGraphSpider.StepInfo> collectedSteps = nodeGraphSpider.collectedSteps;
				if (collectedSteps.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, collectedSteps.Count);
					NodeGraph.NodeIndex node = collectedSteps[index].node;
					nodeGraph.GetNodePosition(node, out this.targetPosition);
					base.ai.pathFollower.targetPosition = this.targetPosition;
				}
				this.PickNewTargetLookPosition();
			}
		}

		// Token: 0x0600097D RID: 2429 RVA: 0x0002FCCC File Offset: 0x0002DECC
		public override void OnExit()
		{
			if (base.bodyInputBank)
			{
				base.bodyInputBank.skill1.PushState(false);
				base.bodyInputBank.skill2.PushState(false);
				base.bodyInputBank.skill3.PushState(false);
				base.bodyInputBank.skill4.PushState(false);
				base.bodyInputBank.jump.PushState(false);
				base.bodyInputBank.moveVector = Vector3.zero;
			}
			base.OnExit();
		}

		// Token: 0x0600097E RID: 2430 RVA: 0x0002FD54 File Offset: 0x0002DF54
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.ai && base.body)
			{
				if (base.ai.skillDriverEvaluation.dominantSkillDriver)
				{
					this.outer.SetNextState(new Combat());
				}
				Vector3 position = base.bodyTransform.position;
				base.ai.pathFollower.UpdatePosition(base.body.footPosition);
				base.ai.localNavigator.targetPosition = base.ai.pathFollower.GetNextPosition();
				base.ai.localNavigator.Update(Time.fixedDeltaTime);
				if (base.bodyInputBank)
				{
					base.bodyInputBank.skill1.PushState(false);
					base.bodyInputBank.skill2.PushState(false);
					base.bodyInputBank.skill3.PushState(false);
					base.bodyInputBank.skill4.PushState(false);
					base.bodyInputBank.jump.PushState(base.ai.localNavigator.jumpSpeed > 0f);
					base.bodyInputBank.moveVector = base.ai.localNavigator.moveVector * 0.25f;
					base.ai.desiredAimDirection = (this.targetLookPosition - base.bodyInputBank.aimOrigin).normalized;
				}
				this.lookTimer -= Time.fixedDeltaTime;
				if (this.lookTimer <= 0f)
				{
					this.PickNewTargetLookPosition();
				}
				if ((base.body.footPosition - this.targetPosition).sqrMagnitude <= base.body.radius * base.body.radius * 4f)
				{
					this.outer.SetNextState(new LookBusy());
					return;
				}
			}
		}

		// Token: 0x04000CD4 RID: 3284
		private Vector3 targetPosition;

		// Token: 0x04000CD5 RID: 3285
		private float lookTimer;

		// Token: 0x04000CD6 RID: 3286
		private const float minLookDuration = 0.5f;

		// Token: 0x04000CD7 RID: 3287
		private const float maxLookDuration = 4f;

		// Token: 0x04000CD8 RID: 3288
		private const int lookTries = 1;

		// Token: 0x04000CD9 RID: 3289
		private const float lookRaycastLength = 25f;

		// Token: 0x04000CDA RID: 3290
		private Vector3 targetLookPosition;
	}
}
