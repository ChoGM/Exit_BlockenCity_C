using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryNews", menuName = "NewsSystem/StoryNewsData")]
public class StoryNewsData : ScriptableObject
{
    [Header("고유 식별자")]
    public string id; // 예: "NEWS_01_01"

    [Header("매칭 조건")]
    public int targetMonth; // 예: 1 (1월에 등장할 기사)

    [Header("기사 내용")]
    public string title;

    [TextArea(3, 10)]
    public string content;

    public Sprite icon;
}
