using GUITree;
using KKAPI.Studio;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FKHeightAdjustUI
{
    public class FKHeightAdjustUI
    {

        private static Slider HeightAdjustSlider;
        private static Button HeightAdjustButton;
        private static TextMeshProUGUI HeightAdjustButtonText;
        private static InputField HeightAdjustInputField;

        private static OCIChar selectedChar;
        private static bool UIInitialized = false;

        public static void UpdateSliderRange()
        {
            float hipHeightConst = -11.435f;
            HeightAdjustSlider.minValue = hipHeightConst - (hipHeightConst * (0 - ((float)FKHeightAdjustUIPlugin.MinSliderHeightPercent.Value / 100f)));
            HeightAdjustSlider.maxValue = (hipHeightConst * -1) + hipHeightConst * ((float)FKHeightAdjustUIPlugin.MaxSliderHeightPercent.Value / 100f);

#if DEBUG
            FKHeightAdjustUIPlugin.Instance.Log.LogInfo($"Applying new slider ranges Min: {HeightAdjustSlider.minValue} Max: {HeightAdjustSlider.maxValue}");
#endif
        }

        internal static void InitUI()
        {
            FKHeightAdjustUIPlugin.Instance.StartCoroutine(DoInitUI());
        }

        private static IEnumerator DoInitUI()
        {
            // Wait a few frames before monkeying with the UI, let FKIK Plugin clone this thing so I don't have to manually delete stuff to clean up.
            yield return null;
            yield return null;
            yield return null;

            // Find and Resize FK Panel
            GameObject fkPanel = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/00_FK");
            fkPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(fkPanel.GetComponent<RectTransform>().sizeDelta.x, fkPanel.GetComponent<RectTransform>().sizeDelta.y + 50);

            // Clone Slider
            Slider sizeSlider = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/00_FK/Slider Size").GetComponent<Slider>();
            HeightAdjustSlider = GameObject.Instantiate(sizeSlider, sizeSlider.transform.parent);
            HeightAdjustSlider.name = "Slider HeightAdj";
            HeightAdjustSlider.transform.Translate(new Vector3(-80, -60, 0), Space.Self);
            HeightAdjustSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 20);

            UpdateSliderRange();

            HeightAdjustSlider.onValueChanged.RemoveAllListeners();
            HeightAdjustSlider.onValueChanged.AddListener(delegate (float value)
            {
                if (updating)
                    return;

                foreach (FKHeightAdjustUICharaController controller in StudioAPI.GetSelectedControllers<FKHeightAdjustUICharaController>())
                {
                    controller.HeightAdjust = value;
                    SyncValues(controller);
                }
            });

            // Clone Text
            GameObject camRotXField = GameObject.Find("StudioScene/Canvas Main Menu/04_System/02_Option/Option/Viewport/Content/Camera/Rot Speed X/InputField");

            HeightAdjustInputField = GameObject.Instantiate(camRotXField.GetComponent<InputField>(), fkPanel.transform);
            HeightAdjustInputField.name = "InputField HeightAdj";
            HeightAdjustInputField.transform.localPosition = new Vector3(100, HeightAdjustSlider.transform.localPosition.y + 15, 0);
            HeightAdjustInputField.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 20);
            HeightAdjustInputField.onValueChanged = new InputField.OnChangeEvent();
            HeightAdjustInputField.onEndEdit = new InputField.SubmitEvent();
            HeightAdjustInputField.onEndEdit.AddListener((string s) => { 
                if (float.TryParse(s, out float value))
                {
                    if (updating)
                        return;

                    foreach (FKHeightAdjustUICharaController controller in StudioAPI.GetSelectedControllers<FKHeightAdjustUICharaController>())
                    {
                        controller.HeightAdjust = value;
                        SyncValues(controller);
                    }
                }
            });


            // Clone Button
            GameObject fkButton = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/Viewport/Content/FK");
            HeightAdjustButton = GameObject.Instantiate(fkButton.GetComponent<Button>(), fkPanel.transform);
            HeightAdjustButton.name = "Button HeightAdj";
            HeightAdjustButton.transform.localPosition = new Vector3(HeightAdjustSlider.transform.localPosition.x + 55, HeightAdjustSlider.transform.localPosition.y + 15, HeightAdjustSlider.transform.localPosition.z);
            HeightAdjustButton.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 20);
            HeightAdjustButton.GetComponent<PreferredSizeFitter>().preferredWidth = 90;

            HeightAdjustButtonText = HeightAdjustButton.transform.GetChild(0).GetComponentInChildren(typeof(TextMeshProUGUI)) as TextMeshProUGUI;
            HeightAdjustButtonText.text = "Height Adj:";
            HeightAdjustButtonText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 70);

            HeightAdjustButton.onClick = new Button.ButtonClickedEvent();
            HeightAdjustButton.image.color = Color.white;
            HeightAdjustButton.image.rectTransform.sizeDelta = new Vector2(90, 20);

            HeightAdjustButton.onClick.AddListener(() =>
            {
                foreach (FKHeightAdjustUICharaController controller in StudioAPI.GetSelectedControllers<FKHeightAdjustUICharaController>())
                {
                    controller.HeightAdjust = 0.0f;
                    SyncValues(controller);
                }
            });

            UIInitialized = true;
        }

        private static bool updating;
        public static void SyncValues(FKHeightAdjustUICharaController controller)
        {
            if (updating || !UIInitialized)
                return;

            updating = true;
            float value = controller.HeightAdjust;
#if DEBUG
            FKHeightAdjustUIPlugin.Instance.Log.LogInfo($"Syncing Value: {value} {controller}");
#endif
            HeightAdjustInputField.text = value.ToString("0.00");
            HeightAdjustSlider.value = value;                

            updating = false;
        }

        internal static void UpdateUI(OCIChar _char)
        {
            selectedChar = _char;

            if (selectedChar != null)
            {
                FKHeightAdjustUICharaController fKHeightAdjController = selectedChar.charInfo.gameObject.GetComponent<FKHeightAdjustUICharaController>();
                SyncValues(fKHeightAdjController);
            }
        }
    }
}
