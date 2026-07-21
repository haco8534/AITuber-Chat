using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneBuilder
{
    [MenuItem("Tools/Voice Chat/Build Debug Scene")]
    public static void BuildVoiceChatScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---- EventSystem ----
        var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // ---- Canvas ----
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 800);

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ---- Root vertical layout ----
        var root = CreateUIObject("Root", canvasGo.transform);
        StretchFull(root.GetComponent<RectTransform>());
        var rootLayout = root.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(16, 16, 16, 16);
        rootLayout.spacing = 8;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.09f, 0.10f, 0.12f);

        // ---- Status text ----
        var statusText = CreateText(root.transform, "StatusText", "STT: -  LLM: -  TTS: -", font, 16);
        AddLayoutElement(statusText.gameObject, -1, 24);

        // ---- Phase text ----
        var phaseText = CreateText(root.transform, "PhaseText", "フェーズ: IDLE", font, 20);
        phaseText.color = new Color(0.6f, 0.75f, 1f);
        AddLayoutElement(phaseText.gameObject, -1, 30);

        // ---- Params row ----
        var paramsRow = CreateUIObject("ParamsRow", root.transform);
        var paramsLayout = paramsRow.AddComponent<HorizontalLayoutGroup>();
        paramsLayout.spacing = 8;
        paramsLayout.childControlWidth = true;
        paramsLayout.childControlHeight = true;
        paramsLayout.childForceExpandWidth = false;
        paramsLayout.childForceExpandHeight = true;
        AddLayoutElement(paramsRow, -1, 60);

        var modelDropdown = CreateDropdown(paramsRow.transform, "ModelDropdown", font, 220);
        var speakerDropdown = CreateDropdown(paramsRow.transform, "SpeakerDropdown", font, 160);
        var languageInput = CreateInputField(paramsRow.transform, "LanguageInput", font, "japanese", 120);
        var instructInput = CreateInputField(paramsRow.transform, "InstructInput", font, "", 0, flexible: true);

        var tempPanel = CreateUIObject("TemperaturePanel", paramsRow.transform);
        AddLayoutElement(tempPanel, 200, 60);
        var tempLayout = tempPanel.AddComponent<VerticalLayoutGroup>();
        tempLayout.childControlWidth = true;
        tempLayout.childControlHeight = true;
        var tempValueText = CreateText(tempPanel.transform, "TemperatureValueText", "temperature 0.70", font, 14);
        AddLayoutElement(tempValueText.gameObject, -1, 20);
        var tempSliderGo = CreateUIObject("TemperatureSlider", tempPanel.transform);
        AddLayoutElement(tempSliderGo, -1, 24);
        var tempSlider = BuildSlider(tempSliderGo, 0f, 2f, 0.7f);

        // ---- Log scroll view ----
        var (scrollRect, logText) = CreateLogScrollView(root.transform, font);
        AddLayoutElement(scrollRect.gameObject, -1, -1, flexibleHeight: 1);

        // ---- Input row ----
        var inputRow = CreateUIObject("InputRow", root.transform);
        var inputLayout = inputRow.AddComponent<HorizontalLayoutGroup>();
        inputLayout.spacing = 8;
        inputLayout.childControlWidth = true;
        inputLayout.childControlHeight = true;
        inputLayout.childForceExpandWidth = false;
        inputLayout.childForceExpandHeight = true;
        AddLayoutElement(inputRow, -1, 44);

        var textInput = CreateInputField(inputRow.transform, "TextInput", font, "メッセージを入力(または録音)", 0, flexible: true);
        var sendButton = CreateButton(inputRow.transform, "SendButton", "送信", font, 90);
        var micButtonGo = CreateButton(inputRow.transform, "MicButton", "録音開始", font, 110);
        var resetButton = CreateButton(inputRow.transform, "ResetButton", "リセット", font, 90);

        // ---- ConversationController ----
        var controllerGo = new GameObject("ConversationController", typeof(AudioSource));
        var controller = controllerGo.AddComponent<ConversationController>();
        var audioSource = controllerGo.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        controller.statusText = statusText;
        controller.phaseText = phaseText;
        controller.modelDropdown = modelDropdown;
        controller.speakerDropdown = speakerDropdown;
        controller.languageInput = languageInput;
        controller.instructInput = instructInput;
        controller.temperatureSlider = tempSlider;
        controller.temperatureValueText = tempValueText;
        controller.logText = logText;
        controller.logScrollRect = scrollRect;
        controller.textInput = textInput;
        controller.sendButton = sendButton.GetComponent<Button>();
        controller.micButton = micButtonGo.GetComponent<Button>();
        controller.resetButton = resetButton.GetComponent<Button>();
        controller.micButtonLabel = micButtonGo.GetComponentInChildren<Text>();
        controller.audioSource = audioSource;

        EditorSceneManager.MarkSceneDirty(scene);
        var scenesDir = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(scenesDir))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        EditorSceneManager.SaveScene(scene, scenesDir + "/VoiceChat.unity");
        Debug.Log("VoiceChat scene built and saved.");
    }

    // ---------------- helpers ----------------

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void AddLayoutElement(GameObject go, float preferredWidth, float preferredHeight, float flexibleHeight = 0, float flexibleWidth = 0)
    {
        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        if (preferredWidth >= 0) le.preferredWidth = preferredWidth;
        if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
        le.flexibleHeight = flexibleHeight;
        le.flexibleWidth = flexibleWidth;
    }

    private static Text CreateText(Transform parent, string name, string content, Font font, int fontSize)
    {
        var go = CreateUIObject(name, parent);
        var text = go.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Dropdown CreateDropdown(Transform parent, string name, Font font, float width)
    {
        var go = CreateUIObject(name, parent);
        AddLayoutElement(go, width, 40);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.24f);
        var dropdown = go.AddComponent<Dropdown>();

        var labelGo = CreateUIObject("Label", go.transform);
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(10, 2);
        labelRt.offsetMax = new Vector2(-25, -2);
        var label = labelGo.AddComponent<Text>();
        label.font = font;
        label.fontSize = 14;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText = label;

        var templateGo = CreateUIObject("Template", go.transform);
        templateGo.SetActive(false);
        var templateRt = templateGo.GetComponent<RectTransform>();
        templateRt.anchorMin = new Vector2(0, 0);
        templateRt.anchorMax = new Vector2(1, 0);
        templateRt.pivot = new Vector2(0.5f, 1f);
        templateRt.anchoredPosition = new Vector2(0, 2);
        templateRt.sizeDelta = new Vector2(0, 150);
        var templateImage = templateGo.AddComponent<Image>();
        templateImage.color = new Color(0.15f, 0.15f, 0.18f);
        var scrollRect = templateGo.AddComponent<ScrollRect>();

        var viewportGo = CreateUIObject("Viewport", templateGo.transform);
        StretchFull(viewportGo.GetComponent<RectTransform>());
        var viewportImage = viewportGo.AddComponent<Image>();
        viewportGo.AddComponent<Mask>().showMaskGraphic = false;

        var contentGo = CreateUIObject("Content", viewportGo.transform);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0, 28);
        var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var itemGo = CreateUIObject("Item", contentGo.transform);
        var itemToggle = itemGo.AddComponent<Toggle>();
        var itemLe = itemGo.AddComponent<LayoutElement>();
        itemLe.preferredHeight = 28;
        var itemBgGo = CreateUIObject("Item Background", itemGo.transform);
        StretchFull(itemBgGo.GetComponent<RectTransform>());
        var itemBg = itemBgGo.AddComponent<Image>();
        itemBg.color = new Color(0.25f, 0.25f, 0.3f);
        var itemCheckGo = CreateUIObject("Item Checkmark", itemGo.transform);
        var itemCheck = itemCheckGo.AddComponent<Image>();
        var checkRt = itemCheckGo.GetComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0, 0.5f);
        checkRt.anchorMax = new Vector2(0, 0.5f);
        checkRt.sizeDelta = new Vector2(16, 16);
        checkRt.anchoredPosition = new Vector2(10, 0);
        var itemLabelGo = CreateUIObject("Item Label", itemGo.transform);
        var itemLabelRt = itemLabelGo.GetComponent<RectTransform>();
        itemLabelRt.anchorMin = Vector2.zero;
        itemLabelRt.anchorMax = Vector2.one;
        itemLabelRt.offsetMin = new Vector2(28, 1);
        itemLabelRt.offsetMax = new Vector2(-10, -1);
        var itemLabel = itemLabelGo.AddComponent<Text>();
        itemLabel.font = font;
        itemLabel.fontSize = 14;
        itemLabel.color = Color.white;
        itemLabel.alignment = TextAnchor.MiddleLeft;

        itemToggle.targetGraphic = itemBg;
        itemToggle.graphic = itemCheck;
        itemToggle.isOn = true;

        scrollRect.content = contentRt;
        scrollRect.viewport = viewportGo.GetComponent<RectTransform>();
        scrollRect.horizontal = false;

        dropdown.template = templateRt;
        dropdown.itemText = itemLabel;
        dropdown.targetGraphic = image;

        return dropdown;
    }

    private static InputField CreateInputField(Transform parent, string name, Font font, string placeholder, float width, bool flexible = false)
    {
        var go = CreateUIObject(name, parent);
        if (flexible)
            AddLayoutElement(go, -1, 40, flexibleWidth: 1);
        else
            AddLayoutElement(go, width, 40);

        var image = go.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.22f);
        var input = go.AddComponent<InputField>();

        var placeholderGo = CreateUIObject("Placeholder", go.transform);
        var placeholderRt = placeholderGo.GetComponent<RectTransform>();
        placeholderRt.anchorMin = Vector2.zero;
        placeholderRt.anchorMax = Vector2.one;
        placeholderRt.offsetMin = new Vector2(8, 4);
        placeholderRt.offsetMax = new Vector2(-8, -4);
        var placeholderText = placeholderGo.AddComponent<Text>();
        placeholderText.font = font;
        placeholderText.fontSize = 14;
        placeholderText.fontStyle = FontStyle.Italic;
        placeholderText.color = new Color(0.6f, 0.6f, 0.6f);
        placeholderText.text = placeholder;

        var textGo = CreateUIObject("Text", go.transform);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(8, 4);
        textRt.offsetMax = new Vector2(-8, -4);
        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 14;
        text.color = Color.white;
        text.supportRichText = false;

        input.textComponent = text;
        input.placeholder = placeholderText;
        input.targetGraphic = image;

        return input;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, Font font, float width)
    {
        var go = CreateUIObject(name, parent);
        AddLayoutElement(go, width, 40);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.25f, 0.4f, 0.85f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var textGo = CreateUIObject("Text", go.transform);
        StretchFull(textGo.GetComponent<RectTransform>());
        var text = textGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 15;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;

        return go;
    }

    private static Slider BuildSlider(GameObject go, float min, float max, float value)
    {
        var bgGo = CreateUIObject("Background", go.transform);
        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImage = bgGo.AddComponent<Image>();
        bgImage.color = new Color(0.25f, 0.25f, 0.3f);

        var fillAreaGo = CreateUIObject("Fill Area", go.transform);
        var fillAreaRt = fillAreaGo.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1, 0.75f);
        fillAreaRt.offsetMin = new Vector2(5, 0);
        fillAreaRt.offsetMax = new Vector2(-5, 0);

        var fillGo = CreateUIObject("Fill", fillAreaGo.transform);
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(0, 1);
        fillRt.sizeDelta = new Vector2(10, 0);
        var fillImage = fillGo.AddComponent<Image>();
        fillImage.color = new Color(0.4f, 0.6f, 1f);

        var handleAreaGo = CreateUIObject("Handle Slide Area", go.transform);
        StretchFull(handleAreaGo.GetComponent<RectTransform>());

        var handleGo = CreateUIObject("Handle", handleAreaGo.transform);
        var handleRt = handleGo.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(16, 16);
        var handleImage = handleGo.AddComponent<Image>();
        handleImage.color = Color.white;

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        return slider;
    }

    private static (ScrollRect, Text) CreateLogScrollView(Transform parent, Font font)
    {
        var scrollGo = CreateUIObject("LogScrollView", parent);
        var scrollImage = scrollGo.AddComponent<Image>();
        scrollImage.color = new Color(0.05f, 0.05f, 0.07f);
        var scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        var viewportGo = CreateUIObject("Viewport", scrollGo.transform);
        StretchFull(viewportGo.GetComponent<RectTransform>());
        viewportGo.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
        viewportGo.AddComponent<Mask>().showMaskGraphic = false;

        var contentGo = CreateUIObject("Content", viewportGo.transform);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0, 100);
        var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var text = contentGo.AddComponent<Text>();
        text.font = font;
        text.fontSize = 13;
        text.color = new Color(0.85f, 0.85f, 0.88f);
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = "";

        scrollRect.content = contentRt;
        scrollRect.viewport = viewportGo.GetComponent<RectTransform>();

        return (scrollRect, text);
    }
}
