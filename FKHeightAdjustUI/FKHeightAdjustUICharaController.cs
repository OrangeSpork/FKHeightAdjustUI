using ExtensibleSaveFormat;
using KKABMX.Core;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FKHeightAdjustUI
{
    public class FKHeightAdjustUICharaController : CharaCustomFunctionController
    {
        public float HeightAdjust { get; set; } = 0.0f;

        private HeightAdjustBoneEffect HeightAdjustBoneEffectInstance;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (HeightAdjustBoneEffectInstance == null)
                StartCoroutine(InitializeHeightAdjustment());
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
            private static string[] HeightAdjustBones = { "cf_J_Hips"};
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
