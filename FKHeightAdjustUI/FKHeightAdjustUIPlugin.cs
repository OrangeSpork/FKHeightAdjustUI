using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using Studio;
using System;
using System.Collections.Generic;
using System.Text;

namespace FKHeightAdjustUI
{
#if AI || HS2
    [BepInProcess("StudioNEOV2")]
#else
    [BepInProcess("CharaStudio")]
#endif
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(KKABMX.Core.KKABMX_Core.GUID, KKABMX.Core.KKABMX_Core.Version)]
    public class FKHeightAdjustUIPlugin : BaseUnityPlugin
    {
        public const string GUID = "orange.spork.fkheightadjustuiplugin";
        public const string PluginName = "FK Height Adjust UI";
        public const string Version = "1.0.2";

        public static FKHeightAdjustUIPlugin Instance { get; set; }

        public static ConfigEntry<int> MinSliderHeightPercent { get; set; }
        public static ConfigEntry<int> MaxSliderHeightPercent { get; set; }

        internal BepInEx.Logging.ManualLogSource Log => Logger;

        public FKHeightAdjustUIPlugin()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Singleton Only.");
            }

            Instance = this;

            MinSliderHeightPercent = Config.Bind("Options", "Minimum Height Adj (% of character height)", 0, new ConfigDescription("0 is origin, negative below origin, positive above origin", new AcceptableValueRange<int>(-100, 100)));
            MaxSliderHeightPercent = Config.Bind("Options", "Maximum Height Adj (% of character height)", 50, new ConfigDescription("0 is char height, negative below char height, positive above char height", new AcceptableValueRange<int>(-100, 200)));

            MinSliderHeightPercent.SettingChanged += SliderSettingsChanged;
            MaxSliderHeightPercent.SettingChanged += SliderSettingsChanged;

            var harmony = new Harmony(GUID);
            harmony.Patch(typeof(MPCharCtrl).GetNestedType("FKInfo", AccessTools.all).GetMethod("Init"), null, new HarmonyMethod(typeof(FKHeightAdjustUI).GetMethod(nameof(FKHeightAdjustUI.InitUI), AccessTools.all)));
            harmony.Patch(typeof(MPCharCtrl).GetNestedType("FKInfo", AccessTools.all).GetMethod("UpdateInfo"), null, new HarmonyMethod(typeof(FKHeightAdjustUI).GetMethod(nameof(FKHeightAdjustUI.UpdateUI), AccessTools.all)));

#if DEBUG
            Log.LogInfo("FK Height Adjust UI Plugin Loaded");
#endif
        }

        private void SliderSettingsChanged(object sender, EventArgs e)
        {
            FKHeightAdjustUI.UpdateSliderRange();
        }

        private void Start()
        {
            CharacterApi.RegisterExtraBehaviour<FKHeightAdjustUICharaController>(GUID);
#if DEBUG
            Log.LogInfo("FK Height Adjust UI Plugin Started");
#endif
        }
    }
}
