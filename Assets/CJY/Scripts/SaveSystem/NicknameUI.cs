using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NicknameUI : MonoBehaviour
{
    public TMP_InputField nicknameInput;
    public TMP_Text warningText;

    //public GameObject nicknamePanel;

    private Coroutine warningCoroutine;

    private const int MIN_LENGTH = 1;
    private const int MAX_LENGTH = 8;

    public void OnClickConfirm()
    {
        string inputName = nicknameInput.text.Trim();

        // 빈 값 검사
        if (string.IsNullOrEmpty(inputName))
        {
            ShowWarning("닉네임을 입력해주세요.");
            return;
        }

        // 글자 수 제한
        if (inputName.Length < MIN_LENGTH || inputName.Length > MAX_LENGTH)
        {
            ShowWarning($"닉네임은 {MIN_LENGTH}~{MAX_LENGTH}자 사이여야 합니다.");
            return;
        }

        // 저장
        Datamanager.Instance.saveData.player.playerName = inputName;
        Datamanager.Instance.SaveGameData();

        //nicknamePanel.SetActive(false);

        gameObject.SetActive(false);
    }

    private void ShowWarning(string message)
    {
        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        warningCoroutine = StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        warningText.gameObject.SetActive(false);
    }
}