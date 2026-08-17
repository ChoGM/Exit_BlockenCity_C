using UnityEngine;

public class ObstacleSoundManager : MonoBehaviour
{
    public static ObstacleSoundManager Instance { get; private set; }

    [SerializeField] private ObstacleSoundData soundData;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (soundData != null)
        {
            soundData.Initialize();
        }
    }

    /// <summary>
    /// 지정된 방해물 타입의 효과음을 1회 재생
    /// </summary>
    public void PlayObstacleSound(ObstacleType type)
    {
        if (soundData == null || audioSource == null) return;

        AudioClip clip = soundData.GetClip(type);
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}