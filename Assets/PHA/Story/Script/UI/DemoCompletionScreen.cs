using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>12월 완료 및 완료 세이브의 이어하기에서 사용하는 전체 화면.</summary>
public class DemoCompletionScreen : MonoBehaviour
{
    private static DemoCompletionScreen instance;
    private int openedFrame;
    private bool returning;

    public static void Show()
    {
        if (instance != null) return;
        var root = new GameObject("DemoCompletionScreen", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        instance = root.AddComponent<DemoCompletionScreen>();
        instance.Build();
    }

    private void Build()
    {
        openedFrame = Time.frameCount;
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        var scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var panel = new GameObject("BlackBackground", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        panel.transform.SetParent(transform, false);
        Stretch(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        panel.GetComponent<UnityEngine.UI.Image>().color = Color.black;

        var font = Resources.Load<Font>("StoryDemo/DemoEndingFont");
        AddText(panel.transform, "Message", "데모 게임 플레이를 해주셔서 감사합니다", font,
            48, new Vector2(0.08f, 0.4f), new Vector2(0.92f, 0.6f), Color.white);
        AddText(panel.transform, "ClickHint", "클릭하면 타이틀로 돌아갑니다", font,
            24, new Vector2(0.1f, 0.26f), new Vector2(0.9f, 0.34f), new Color(0.65f, 0.65f, 0.65f));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void AddText(Transform parent, string name, string message, Font font,
        int size, Vector2 min, Vector2 max, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Text));
        obj.transform.SetParent(parent, false);
        Stretch(obj.GetComponent<RectTransform>(), min, max);
        var label = obj.GetComponent<UnityEngine.UI.Text>();
        label.font = font;
        label.text = message;
        label.fontSize = size;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.raycastTarget = false;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = size / 2;
        label.resizeTextMaxSize = size;
    }

    private void Update()
    {
        // 마지막 대사/이어하기를 누른 클릭으로 감사 화면까지 바로 닫히지 않게 한다.
        if (returning || Time.frameCount <= openedFrame + 1 || !Input.GetMouseButtonDown(0)) return;
        returning = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
