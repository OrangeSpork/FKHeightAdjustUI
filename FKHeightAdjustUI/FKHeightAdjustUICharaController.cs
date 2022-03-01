using ExtensibleSaveFormat;
using KKABMX.Core;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if KK || KKS
using static ChaFileDefine;
#endif

namespace FKHeightAdjustUI
{
    public class FKHeightAdjustUICharaController : CharaCustomFunctionController
    {
        private ChangeAmount hipsChangeAmount;
#if AI || HS2
        private const string HeightAdjustBone = "cf_J_Hips";
#elif KK || KKS
        private const string HeightAdjustBone = "cf_j_hips";
#endif

        public float HeightAdjust
        {
            get
            {
                if (hipsChangeAmount == null)
                    hipsChangeAmount = FindHipsChangeAmount();

                return hipsChangeAmount == null ? 0.0f : hipsChangeAmount.pos.y;
            }
            set
            {
                if (hipsChangeAmount == null)
                    hipsChangeAmount = FindHipsChangeAmount();

                if (hipsChangeAmount != null)
                {
                    hipsChangeAmount.pos = new Vector3(hipsChangeAmount.pos.x, value, hipsChangeAmount.pos.z);
#if DEBUG
                    FKHeightAdjustUIPlugin.Instance.Log.LogInfo($"Updated Height Adjust to {hipsChangeAmount.pos.y}");
#endif
                }
                else
                {
#if DEBUG
                    FKHeightAdjustUIPlugin.Instance.Log.LogInfo($"Hips Change Amount not available");
#endif

                }
            }
        }

        private HeightAdjustBoneEffect HeightAdjustBoneEffectInstance;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (HeightAdjustBoneEffectInstance == null)
                StartCoroutine(InitializeHeightAdjustment());
        }

        private ChangeAmount FindHipsChangeAmount()
        {
            OCIChar.BoneInfo bone = ChaControl.GetOCIChar().listBones.Find((bi) => { return String.Equals(bi?.guideObject?.transformTarget?.name, HeightAdjustBone); });
            if (bone != null)
                return bone.guideObject.changeAmount;
            else
                return null;
        }

        private IEnumerator InitializeHeightAdjustment()
        {
            yield return new WaitUntil(() => ChaControl != null && ChaControl.GetComponent<BoneController>() != null && ChaControl.objAnim != null);

            if (HeightAdjustBoneEffectInstance == null)
            {
                BoneController boneController = ChaControl.GetComponent<BoneController>();
                HeightAdjustBoneEffectInstance = new HeightAdjustBoneEffect(this);
                boneController.AddBoneEffect(HeightAdjustBoneEffectInstance);
            }
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            var data = new PluginData();

            data.data["HeightAdjust"] = HeightAdjust;

            SetExtendedData(data);
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (maintainState)
                return;

            var data = GetExtendedData();
            if (data != null)
            {
                if (data.data.TryGetValue("HeightAdjust", out var val1)) HeightAdjust = (float)val1;
            }

            FKHeightAdjustUI.UpdateUI(ChaControl.GetOCIChar());
        }

        private class HeightAdjustBoneEffect : BoneEffect
        {
            private static string[] HeightAdjustBones = { HeightAdjustBone };
            private BoneModifierData HeightAdjustModifier;
            private FKHeightAdjustUICharaController controller;

            public HeightAdjustBoneEffect(FKHeightAdjustUICharaController controller)
            {
                this.controller = controller;
            }

            public override IEnumerable<string> GetAffectedBones(BoneController origin)
            {
                return HeightAdjustBones;
            }

            public override BoneModifierData GetEffect(string bone, BoneController origin, CoordinateType coordinate)
            {
                if (HeightAdjustModifier == null)
                    HeightAdjustModifier = new BoneModifierData();

                if (!(controller.ChaControl.GetOCIChar().oiCharInfo.enableFK && controller.ChaControl.GetOCIChar().oiCharInfo.activeFK[3]))
                    return null;

                if (!HeightAdjustBones.Contains(bone))
                    return null;

                if (HeightAdjustModifier.PositionModifier.y != controller.HeightAdjust)
                {
#if DEBUG
                    FKHeightAdjustUIPlugin.Instance.Log.LogInfo($"Applying Height Adjust {controller.HeightAdjust}");
#endif
                    HeightAdjustModifier.PositionModifier = new UnityEngine.Vector3(0, controller.HeightAdjust, 0);
                }

                return HeightAdjustModifier;
            }
        }
    }
}
