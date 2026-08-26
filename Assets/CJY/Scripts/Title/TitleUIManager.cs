using UnityEngine;
using TMPro;

public class TitleUIManager : MonoBehaviour
{
    [Header("버전 정보")]
    public string UID = "UHD";
    public string version = "Ver. 1.0.0";

    [Header("UI")]
    [SerializeField] private TMP_Text tmpUID;
    [SerializeField] private TMP_Text tmpVersion;

    private void Start()
    {
        tmpUID.text = UID;
        tmpVersion.text = version;
    }
}