using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutScVFXController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource typingSFX;
    public AudioSource typingEndSFX;
    [Space(5f)]

    [Header("Typing Settings")]
    
    [Space(5f)]
    public Text text1;
   [TextArea] public string message1;
    
    [Space(5f)]
    public Text text2;
    [TextArea] public string message2;

    [Space(5f)]
    public Text text3;
    [TextArea] public string message3;

    [Space(5f)]
    public float typingSpeed = 0.1f;
    public string cursorChar = "|";
    public float cursorBlinkRate = 0.5f;

    private Coroutine blinkCoroutine;

    public void PlaySequence()
    {
        StopAllCoroutines();

        if (typingSFX != null)
            typingSFX.Stop();

        if (typingEndSFX != null)
            typingEndSFX.Stop();

        text1.text = "";
        text2.text = "";
        text3.text = "";

        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        yield return StartCoroutine(TypeSingle(text1, message1, false));
        yield return StartCoroutine(TypeSingle(text2, message2, false));
        yield return StartCoroutine(TypeSingle(text3, message3, true));
    }

    private IEnumerator TypeSingle(Text target, string message, bool allowBlink)
    {
        string currentText = "";

        if (typingSFX != null)
        {
            typingSFX.Play();
        }

        for (int i = 0; i < message.Length; i++)
        {
            char c = message[i];

            if (IsKorean(c))
            {
                foreach (string step in DecomposeKorean(c))
                {
                    target.text = currentText + step + cursorChar;
                    yield return new WaitForSeconds(typingSpeed);
                }

                currentText += c;
            }
            else
            {
                currentText += c;
                target.text = currentText + cursorChar;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        
        if (typingSFX != null)
        {
            typingSFX.Stop();
        }

        if (typingEndSFX != null)
        {
            typingEndSFX.Play();
        }

        if (allowBlink)
        {
            blinkCoroutine = StartCoroutine(CursorBlink(target, currentText));
        }
        else
        {
            target.text = currentText;
        }
    }

    private IEnumerator CursorBlink(Text target, string finalMessage)
    {
        bool visible = true;

        while (true)
        {
            if (visible)
            {
                target.text = finalMessage + cursorChar;
            }
            else
            {
                target.text = finalMessage +
                    "<color=#00000000>" + cursorChar + "</color>";
            }

            visible = !visible;
            yield return new WaitForSeconds(cursorBlinkRate);
        }
    }

    private bool IsKorean(char c)
    {
        return c >= 0xAC00 && c <= 0xD7A3;
    }

    private IEnumerable<string> DecomposeKorean(char syllable)
    {
        List<string> steps = new List<string>();

        int unicode = syllable - 0xAC00;

        int cho = unicode / 588;
        int jung = (unicode % 588) / 28;
        int jong = unicode % 28;

        string[] CHO = {
            "ぁ","あ","い","ぇ","え","ぉ","け","げ","こ","さ",
            "ざ","し","じ","す","ず","せ","ぜ","そ","ぞ"
        };

        steps.Add(CHO[cho]);

        char jungCombined =
            (char)(0xAC00 + (cho * 588) + (jung * 28));
        steps.Add(jungCombined.ToString());

        if (jong != 0)
        {
            char final =
                (char)(0xAC00 + (cho * 588) + (jung * 28) + jong);
            steps.Add(final.ToString());
        }

        return steps;
    }
}
