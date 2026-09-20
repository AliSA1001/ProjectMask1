using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.Timeline;
using UnityEngine.UI;

// Tools > Cutscenes: sets up the cutscene UI in the open scene and makes a ready-to-edit cutscene
// (object + Timeline + a camera + one fade and one subtitle line as an example). See Assets/Osama/Docs/Cutscene_AR.md
public static class CutsceneMenu
{
    private const string OsamaDir = "Assets/Osama";
    private const string CutsceneDir = OsamaDir + "/Cutscenes";

    [MenuItem("Tools/Cutscenes/Add Cutscene UI To Scene")]
    public static void AddCutsceneUi()
    {
        SubtitleUI subtitle;
        CanvasGroup fade;
        if (FindUi(out subtitle, out fade))
        {
            Selection.activeObject = subtitle.transform.parent.gameObject;
            Debug.Log("The scene already has a CutsceneUI");
            return;
        }
        BuildUi(out subtitle, out fade);
    }

    [MenuItem("Tools/Cutscenes/New Cutscene")]
    public static void NewCutscene()
    {
        SubtitleUI subtitle;
        CanvasGroup fade;
        if (!FindUi(out subtitle, out fade)) BuildUi(out subtitle, out fade);

        if (!AssetDatabase.IsValidFolder(CutsceneDir)) AssetDatabase.CreateFolder(OsamaDir, "Cutscenes");

        string path = AssetDatabase.GenerateUniqueAssetPath(CutsceneDir + "/NewCutscene.playable");
        string cutsceneName = System.IO.Path.GetFileNameWithoutExtension(path);

        TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        AssetDatabase.CreateAsset(timeline, path); // must exist as an asset before tracks are added so they get saved inside it

        GameObject root = new GameObject(cutsceneName);
        Undo.RegisterCreatedObjectUndo(root, "New Cutscene");
        root.AddComponent<Cutscene>(); // pulls in the PlayableDirector too
        PlayableDirector director = root.GetComponent<PlayableDirector>();
        director.playOnAwake = false;
        director.playableAsset = timeline;

        GameObject camera = BuildCamera(root.transform);

        // the camera stays switched off, this track turns it on for the length of the cutscene
        ActivationTrack cameraTrack = timeline.CreateTrack<ActivationTrack>(null, "Camera_1 On");
        director.SetGenericBinding(cameraTrack, camera);
        TimelineClip cameraClip = cameraTrack.CreateDefaultClip();
        cameraClip.start = 0.0;
        cameraClip.duration = 8.0;

        FadeTrack fadeTrack = timeline.CreateTrack<FadeTrack>(null, "Fade");
        director.SetGenericBinding(fadeTrack, fade);
        TimelineClip fadeClip = fadeTrack.CreateClip<FadeClip>();
        fadeClip.displayName = "Fade from black";
        fadeClip.start = 0.0;
        fadeClip.duration = 1.5;

        SubtitleTrack subtitleTrack = timeline.CreateTrack<SubtitleTrack>(null, "Subtitles");
        director.SetGenericBinding(subtitleTrack, subtitle);
        TimelineClip lineClip = subtitleTrack.CreateClip<SubtitleClip>();
        SubtitleClip line = (SubtitleClip)lineClip.asset;
        line.speaker = "Villager";
        line.text = "This is a test line. Change it in the Timeline window.";
        lineClip.displayName = "Test line";
        lineClip.start = 1.5;
        lineClip.duration = 4.0;

        EditorUtility.SetDirty(timeline);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(root.scene);

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(timeline);
        Debug.Log("Created '" + cutsceneName + "'. Open Window > Sequencing > Timeline with it selected to edit. It plays from a CutsceneTrigger, a UnityEvent, or Cutscene.Play() in code");
    }

    // ---- UI ----

    private static bool FindUi(out SubtitleUI subtitle, out CanvasGroup fade)
    {
        subtitle = Object.FindFirstObjectByType<SubtitleUI>(FindObjectsInactive.Include);
        fade = null;
        if (subtitle == null) return false;

        Transform fadePanel = subtitle.transform.parent != null ? subtitle.transform.parent.Find("FadePanel") : null;
        if (fadePanel != null) fade = fadePanel.GetComponent<CanvasGroup>();
        return fade != null;
    }

    private static void BuildUi(out SubtitleUI subtitle, out CanvasGroup fade)
    {
        GameObject canvasGo = new GameObject("CutsceneUI", typeof(Canvas), typeof(CanvasScaler));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Add Cutscene UI");

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500; // above the HUD

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // order = draw order. Fade at the back so the subtitles and skip hint stay readable while the screen is black
        GameObject fadeGo = NewUiObject("FadePanel", canvasGo.transform, typeof(Image), typeof(CanvasGroup));
        Stretch(fadeGo.GetComponent<RectTransform>());
        Image fadeImage = fadeGo.GetComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;
        fade = fadeGo.GetComponent<CanvasGroup>();
        fade.alpha = 0f;
        fade.interactable = false;
        fade.blocksRaycasts = false;

        GameObject subtitleGo = NewUiObject("SubtitlePanel", canvasGo.transform, typeof(Image), typeof(CanvasGroup), typeof(SubtitleUI));
        RectTransform subtitleRect = subtitleGo.GetComponent<RectTransform>();
        subtitleRect.anchorMin = subtitleRect.anchorMax = new Vector2(0.5f, 0f);
        subtitleRect.pivot = new Vector2(0.5f, 0f);
        subtitleRect.sizeDelta = new Vector2(1300f, 190f);
        subtitleRect.anchoredPosition = new Vector2(0f, 70f);
        Image subtitleBack = subtitleGo.GetComponent<Image>();
        subtitleBack.color = new Color(0f, 0f, 0f, 0.6f);
        subtitleBack.raycastTarget = false;
        CanvasGroup subtitleGroup = subtitleGo.GetComponent<CanvasGroup>();
        subtitleGroup.alpha = 0f;
        subtitleGroup.interactable = false;
        subtitleGroup.blocksRaycasts = false;

        TextMeshProUGUI speaker = NewText("Speaker", subtitleGo.transform, 34f, FontStyles.Bold, new Color(0.95f, 0.75f, 0.2f));
        RectTransform speakerRect = speaker.rectTransform;
        speakerRect.anchorMin = new Vector2(0f, 1f);
        speakerRect.anchorMax = new Vector2(1f, 1f);
        speakerRect.pivot = new Vector2(0.5f, 1f);
        speakerRect.sizeDelta = new Vector2(-60f, 44f);
        speakerRect.anchoredPosition = new Vector2(0f, -14f);

        TextMeshProUGUI body = NewText("Body", subtitleGo.transform, 32f, FontStyles.Normal, Color.white);
        body.enableAutoSizing = true;
        body.fontSizeMin = 22f;
        body.fontSizeMax = 32f;
        RectTransform bodyRect = body.rectTransform;
        bodyRect.anchorMin = Vector2.zero;
        bodyRect.anchorMax = Vector2.one;
        bodyRect.offsetMin = new Vector2(30f, 14f);
        bodyRect.offsetMax = new Vector2(-30f, -62f);

        subtitle = subtitleGo.GetComponent<SubtitleUI>();
        SetReference(subtitle, "group", subtitleGroup);
        SetReference(subtitle, "speakerText", speaker);
        SetReference(subtitle, "bodyText", body);

        BuildSkipHint(canvasGo.transform);

        Selection.activeGameObject = canvasGo;
        EditorSceneManager.MarkSceneDirty(canvasGo.scene);
    }

    private static void BuildSkipHint(Transform parent)
    {
        GameObject hintGo = NewUiObject("SkipHint", parent, typeof(CanvasGroup), typeof(CutsceneSkipHint));
        RectTransform hintRect = hintGo.GetComponent<RectTransform>();
        hintRect.anchorMin = hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(1f, 0f);
        hintRect.sizeDelta = new Vector2(360f, 60f);
        hintRect.anchoredPosition = new Vector2(-40f, 40f);
        CanvasGroup hintGroup = hintGo.GetComponent<CanvasGroup>();
        hintGroup.alpha = 0f;
        hintGroup.interactable = false;
        hintGroup.blocksRaycasts = false;

        TextMeshProUGUI label = NewText("Label", hintGo.transform, 26f, FontStyles.Normal, new Color(1f, 1f, 1f, 0.85f));
        label.text = "Hold SPACE to skip";
        label.alignment = TextAlignmentOptions.BottomRight;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(0f, 12f);
        labelRect.offsetMax = Vector2.zero;

        GameObject barBack = NewUiObject("BarBack", hintGo.transform, typeof(Image));
        RectTransform backRect = barBack.GetComponent<RectTransform>();
        backRect.anchorMin = Vector2.zero;
        backRect.anchorMax = new Vector2(1f, 0f);
        backRect.pivot = new Vector2(0.5f, 0f);
        backRect.sizeDelta = new Vector2(0f, 6f);
        backRect.anchoredPosition = Vector2.zero;
        Image backImage = barBack.GetComponent<Image>();
        backImage.color = new Color(1f, 1f, 1f, 0.25f);
        backImage.raycastTarget = false;

        GameObject barFill = NewUiObject("BarFill", barBack.transform, typeof(Image));
        RectTransform fillRect = barFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0f, 1f); // CutsceneSkipHint grows anchorMax.x
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = barFill.GetComponent<Image>();
        fillImage.color = Color.white;
        fillImage.raycastTarget = false;

        CutsceneSkipHint hint = hintGo.GetComponent<CutsceneSkipHint>();
        SetReference(hint, "group", hintGroup);
        SetReference(hint, "fillBar", fillRect);
    }

    // ---- camera ----

    private static GameObject BuildCamera(Transform parent)
    {
        GameObject go = new GameObject("Camera_1", typeof(Camera), typeof(AudioListener));
        go.transform.SetParent(parent, false);

        // start where you are looking in the scene view so the first shot isn't at the world origin
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
            go.transform.SetPositionAndRotation(sceneView.camera.transform.position, sceneView.camera.transform.rotation);

        // URP only draws Global Volume effects on cameras that have Post Processing ticked
        UniversalAdditionalCameraData data = go.GetComponent<UniversalAdditionalCameraData>();
        if (data == null) data = go.AddComponent<UniversalAdditionalCameraData>();
        data.renderPostProcessing = true;

        go.SetActive(false);
        return go;
    }

    // ---- small helpers ----

    private static GameObject NewUiObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        foreach (System.Type type in components) go.AddComponent(type);
        return go;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, float size, FontStyles style, Color color)
    {
        GameObject go = NewUiObject(name, parent, typeof(TextMeshProUGUI));
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // the components keep their fields private, this is the same as dragging the object into the slot in the inspector
    private static void SetReference(Object target, string field, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
