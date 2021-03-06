﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI
{
	// Token: 0x020005E4 RID: 1508
	[RequireComponent(typeof(Canvas))]
	public class HighlightRect : MonoBehaviour
	{
		// Token: 0x060021C4 RID: 8644 RVA: 0x0009F794 File Offset: 0x0009D994
		static HighlightRect()
		{
			RoR2Application.onLateUpdate += HighlightRect.UpdateAll;
		}

		// Token: 0x060021C5 RID: 8645 RVA: 0x0009F7BC File Offset: 0x0009D9BC
		private void Awake()
		{
			this.canvas = base.GetComponent<Canvas>();
		}

		// Token: 0x060021C6 RID: 8646 RVA: 0x0009F7CA File Offset: 0x0009D9CA
		private void OnEnable()
		{
			HighlightRect.instancesList.Add(this);
		}

		// Token: 0x060021C7 RID: 8647 RVA: 0x0009F7D7 File Offset: 0x0009D9D7
		private void OnDisable()
		{
			HighlightRect.instancesList.Remove(this);
		}

		// Token: 0x060021C8 RID: 8648 RVA: 0x0009F7E8 File Offset: 0x0009D9E8
		private void Start()
		{
			this.highlightState = HighlightRect.HighlightState.Expanding;
			this.bottomLeftImage = this.bottomLeftRectTransform.GetComponent<Image>();
			this.bottomRightImage = this.bottomRightRectTransform.GetComponent<Image>();
			this.topLeftImage = this.topLeftRectTransform.GetComponent<Image>();
			this.topRightImage = this.topRightRectTransform.GetComponent<Image>();
			this.bottomLeftImage.sprite = this.cornerImage;
			this.bottomRightImage.sprite = this.cornerImage;
			this.topLeftImage.sprite = this.cornerImage;
			this.topRightImage.sprite = this.cornerImage;
			this.bottomLeftImage.color = this.highlightColor;
			this.bottomRightImage.color = this.highlightColor;
			this.topLeftImage.color = this.highlightColor;
			this.topRightImage.color = this.highlightColor;
			if (this.nametagRectTransform)
			{
				this.nametagText = this.nametagRectTransform.GetComponent<TextMeshProUGUI>();
				this.nametagText.color = this.highlightColor;
				this.nametagText.text = this.nametagString;
			}
		}

		// Token: 0x060021C9 RID: 8649 RVA: 0x0009F908 File Offset: 0x0009DB08
		private static void UpdateAll()
		{
			for (int i = HighlightRect.instancesList.Count - 1; i >= 0; i--)
			{
				HighlightRect.instancesList[i].DoUpdate();
			}
		}

		// Token: 0x060021CA RID: 8650 RVA: 0x0009F93C File Offset: 0x0009DB3C
		private void DoUpdate()
		{
			if (!this.targetRenderer)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			switch (this.highlightState)
			{
			case HighlightRect.HighlightState.Expanding:
				this.time += Time.deltaTime;
				if (this.time >= this.expandTime)
				{
					this.time = this.expandTime;
					this.highlightState = HighlightRect.HighlightState.Holding;
				}
				break;
			case HighlightRect.HighlightState.Holding:
				if (this.destroyOnLifeEnd)
				{
					this.time += Time.deltaTime;
					if (this.time > this.maxLifeTime)
					{
						this.highlightState = HighlightRect.HighlightState.Contracting;
						this.time = this.expandTime;
					}
				}
				break;
			case HighlightRect.HighlightState.Contracting:
				this.time -= Time.deltaTime;
				if (this.time <= 0f)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				break;
			}
			Rect rect = HighlightRect.GUIRectWithObject(this.sceneCam, this.targetRenderer);
			Vector2 a = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, 0.5f), Mathf.Lerp(rect.yMin, rect.yMax, 0.5f));
			float t = this.curve.Evaluate(this.time / this.expandTime);
			this.bottomLeftRectTransform.anchoredPosition = Vector2.LerpUnclamped(a, new Vector2(rect.xMin, rect.yMin), t);
			this.bottomRightRectTransform.anchoredPosition = Vector2.LerpUnclamped(a, new Vector2(rect.xMax, rect.yMin), t);
			this.topLeftRectTransform.anchoredPosition = Vector2.LerpUnclamped(a, new Vector2(rect.xMin, rect.yMax), t);
			this.topRightRectTransform.anchoredPosition = Vector2.LerpUnclamped(a, new Vector2(rect.xMax, rect.yMax), t);
			if (this.nametagRectTransform)
			{
				this.nametagRectTransform.anchoredPosition = Vector2.LerpUnclamped(a, new Vector2(rect.xMin, rect.yMax), t);
			}
		}

		// Token: 0x060021CB RID: 8651 RVA: 0x0009FB48 File Offset: 0x0009DD48
		public static Rect GUIRectWithObject(Camera cam, Renderer rend)
		{
			Vector3 center = rend.bounds.center;
			Vector3 extents = rend.bounds.extents;
			HighlightRect.extentPoints[0] = HighlightRect.WorldToGUIPoint(cam, new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z));
			HighlightRect.extentPoints[1] = HighlightRect.WorldToGUIPoint(cam, new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z));
			HighlightRect.extentPoints[2] = HighlightRect.WorldToGUIPoint(cam, new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z));
			HighlightRect.extentPoints[3] = HighlightRect.WorldToGUIPoint(cam, new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z));
			HighlightRect.extentPoints[4] = HighlightRect.WorldToGUIPoint(cam, new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z));
			HighlightRect.extentPoints[5] = HighlightRect.WorldToGUIPoint(cam, new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z));
			HighlightRect.extentPoints[6] = HighlightRect.WorldToGUIPoint(cam, new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z));
			HighlightRect.extentPoints[7] = HighlightRect.WorldToGUIPoint(cam, new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z));
			Vector2 vector = HighlightRect.extentPoints[0];
			Vector2 vector2 = HighlightRect.extentPoints[0];
			foreach (Vector2 rhs in HighlightRect.extentPoints)
			{
				vector = Vector2.Min(vector, rhs);
				vector2 = Vector2.Max(vector2, rhs);
			}
			return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
		}

		// Token: 0x060021CC RID: 8652 RVA: 0x0009FDD7 File Offset: 0x0009DFD7
		public static Vector2 WorldToGUIPoint(Camera cam, Vector3 world)
		{
			return cam.WorldToScreenPoint(world);
		}

		// Token: 0x060021CD RID: 8653 RVA: 0x0009FDE8 File Offset: 0x0009DFE8
		public static void CreateHighlight(GameObject viewerBodyObject, Renderer targetRenderer, GameObject highlightPrefab, float overrideDuration = -1f, bool visibleToAll = false)
		{
			ReadOnlyCollection<CameraRigController> readOnlyInstancesList = CameraRigController.readOnlyInstancesList;
			int i = 0;
			int count = readOnlyInstancesList.Count;
			while (i < count)
			{
				CameraRigController cameraRigController = readOnlyInstancesList[i];
				if (!(cameraRigController.target != viewerBodyObject) || visibleToAll)
				{
					HighlightRect component = UnityEngine.Object.Instantiate<GameObject>(highlightPrefab).GetComponent<HighlightRect>();
					component.targetRenderer = targetRenderer;
					component.canvas.worldCamera = cameraRigController.uiCam;
					component.uiCam = cameraRigController.uiCam;
					component.sceneCam = cameraRigController.sceneCam;
					if (overrideDuration > 0f)
					{
						component.maxLifeTime = overrideDuration;
					}
				}
				i++;
			}
		}

		// Token: 0x040024A7 RID: 9383
		public AnimationCurve curve;

		// Token: 0x040024A8 RID: 9384
		public Color highlightColor;

		// Token: 0x040024A9 RID: 9385
		public Sprite cornerImage;

		// Token: 0x040024AA RID: 9386
		public string nametagString;

		// Token: 0x040024AB RID: 9387
		private Image bottomLeftImage;

		// Token: 0x040024AC RID: 9388
		private Image bottomRightImage;

		// Token: 0x040024AD RID: 9389
		private Image topLeftImage;

		// Token: 0x040024AE RID: 9390
		private Image topRightImage;

		// Token: 0x040024AF RID: 9391
		private TextMeshProUGUI nametagText;

		// Token: 0x040024B0 RID: 9392
		public Renderer targetRenderer;

		// Token: 0x040024B1 RID: 9393
		public GameObject cameraTarget;

		// Token: 0x040024B2 RID: 9394
		public RectTransform nametagRectTransform;

		// Token: 0x040024B3 RID: 9395
		public RectTransform bottomLeftRectTransform;

		// Token: 0x040024B4 RID: 9396
		public RectTransform bottomRightRectTransform;

		// Token: 0x040024B5 RID: 9397
		public RectTransform topLeftRectTransform;

		// Token: 0x040024B6 RID: 9398
		public RectTransform topRightRectTransform;

		// Token: 0x040024B7 RID: 9399
		public float expandTime = 1f;

		// Token: 0x040024B8 RID: 9400
		public float maxLifeTime;

		// Token: 0x040024B9 RID: 9401
		public bool destroyOnLifeEnd;

		// Token: 0x040024BA RID: 9402
		private float time;

		// Token: 0x040024BB RID: 9403
		public HighlightRect.HighlightState highlightState;

		// Token: 0x040024BC RID: 9404
		private static List<HighlightRect> instancesList = new List<HighlightRect>();

		// Token: 0x040024BD RID: 9405
		private Canvas canvas;

		// Token: 0x040024BE RID: 9406
		private Camera uiCam;

		// Token: 0x040024BF RID: 9407
		private Camera sceneCam;

		// Token: 0x040024C0 RID: 9408
		private static readonly Vector2[] extentPoints = new Vector2[8];

		// Token: 0x020005E5 RID: 1509
		public enum HighlightState
		{
			// Token: 0x040024C2 RID: 9410
			Expanding,
			// Token: 0x040024C3 RID: 9411
			Holding,
			// Token: 0x040024C4 RID: 9412
			Contracting
		}
	}
}
