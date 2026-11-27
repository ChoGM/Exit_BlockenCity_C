using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BackgroundTestController : MonoBehaviour
{
    [SerializeField] private StoryBackgroundCatalog catalog;
    [Tooltip("스토리의 기본 배경 Image")]
    [SerializeField] private Image backgroundImage;
    [Tooltip("페이드용 Image. 기본 배경 바로 위에 같은 크기로 배치합니다.")]
    [SerializeField] private Image fadeLayer;

    [Header("기존 배경 테스트 (선택 사항)")]
    [SerializeField] private bool enableKeyboardTest = false;
    [SerializeField] private Sprite background1;
    [SerializeField] private Sprite background2;
    [SerializeField] private Sprite background3;
    [SerializeField] private GameObject blackOverlay;

    private void Start()
    {
        if (blackOverlay != null) blackOverlay.SetActive(false);
    }

    private void Update()
    {
        if (!enableKeyboardTest) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeBackground(background1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeBackground(background2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeBackground(background3);
        if (Input.GetKeyDown(KeyCode.Alpha4) && blackOverlay != null)
            blackOverlay.SetActive(!blackOverlay.activeSelf);
    }

    private void ChangeBackground(Sprite sprite)
    {
        if (sprite == null || !Initialize()) return;
        CompleteTransition();
        Apply(sprite);
    }
    private bool initialized;
    private bool sceneEnabled;
    private Sprite sceneSprite;
    private Color sceneColor;
    private bool transitioning;
    private Sprite destination;
    private int revision;

    private void Awake() => Initialize();

    private bool Initialize()
    {
        if (initialized) return backgroundImage != null;
        if (backgroundImage == null) return false;
        if (fadeLayer == backgroundImage)
        {
            Debug.LogError("기본 배경과 페이드 레이어에는 서로 다른 Image를 지정하세요.", this);
            fadeLayer = null;
        }
        sceneEnabled = backgroundImage.enabled;
        sceneSprite = backgroundImage.sprite;
        sceneColor = backgroundImage.color;
        if (fadeLayer != null)
        {
            fadeLayer.raycastTarget = false;
            fadeLayer.enabled = false;
        }
        initialized = true;
        return true;
    }

    public void BeginStory(StoryData story)
    {
        if (!Initialize())
        {
            Debug.LogError("BackgroundTestController에 기본 배경 Image를 지정하세요.", this);
            return;
        }
        CompleteTransition();
        backgroundImage.sprite = sceneSprite;
        backgroundImage.color = sceneColor;
        backgroundImage.enabled = sceneEnabled;
        if (!string.IsNullOrWhiteSpace(story.DefaultBackgroundId) &&
            TryResolve(story.DefaultBackgroundId, story.StoryId, out Sprite sprite))
            Apply(sprite);
    }

    public IEnumerator Show(StoryNodeData node, string storyId, bool skip)
    {
        if (string.IsNullOrWhiteSpace(node.BackgroundId)) yield break;
        if (!Initialize() || !TryResolve(node.BackgroundId, $"{storyId}/{node.NodeId}", out Sprite sprite))
            yield break;

        CompleteTransition();
        if (backgroundImage.sprite == sprite && backgroundImage.enabled == (sprite != null)) yield break;
        float duration = node.BackgroundTransitionDuration;
        if (skip || node.BackgroundTransition != StoryBackgroundTransition.Fade ||
            !(duration > 0f) || float.IsInfinity(duration))
        {
            Apply(sprite);
            yield break;
        }
        if (sprite != null && (fadeLayer == null || !fadeLayer.gameObject.activeInHierarchy))
        {
            Debug.LogWarning($"[{storyId}/{node.NodeId}] 페이드용 Image를 연결하고 활성화하세요. 배경을 즉시 교체합니다.", this);
            Apply(sprite);
            yield break;
        }

        destination = sprite;
        transitioning = true;
        int token = ++revision;
        if (sprite != null)
        {
            fadeLayer.sprite = sprite;
            fadeLayer.color = WithAlpha(0f);
            fadeLayer.enabled = true;
        }
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            if (token != revision) yield break;
            float t = elapsed / duration;
            if (sprite == null)
                backgroundImage.color = WithAlpha(1f - t);
            else
                fadeLayer.color = WithAlpha(t);
            yield return null;
        }
        if (token == revision) CompleteTransition();
    }

    private bool TryResolve(string id, string source, out Sprite sprite)
    {
        sprite = null;
        if (id.Trim() == StoryBackgroundCatalog.HideId) return true;
        if (catalog != null && catalog.TryGetSprite(id, out sprite)) return true;
        Debug.LogError($"[{source}] 배경 목록에서 '{id}'의 Sprite를 찾을 수 없습니다.", this);
        return false;
    }

    private Color WithAlpha(float fraction)
    {
        Color color = sceneColor;
        color.a *= fraction;
        return color;
    }

    private void Apply(Sprite sprite)
    {
        backgroundImage.sprite = sprite;
        backgroundImage.color = sceneColor;
        backgroundImage.enabled = sprite != null;
        if (fadeLayer != null)
        {
            fadeLayer.enabled = false;
            fadeLayer.sprite = null;
        }
    }

    public void CompleteTransition()
    {
        if (!transitioning) return;
        ++revision;
        transitioning = false;
        if (backgroundImage != null) Apply(destination);
        destination = null;
    }

    private void OnDisable() => CompleteTransition();
}
