using System;
using System.Collections.Generic;

/// <summary>
/// 완료한 스토리, 소개된 세력, 선택 결과를 저장
/// </summary>

[Serializable]
public class StoryProgressData
{
    public List<string> completedStoryIds = new();

    public List<string> introducedFactionIds = new();

    public List<StoryChoiceResultData> choiceResults = new();
}

[Serializable]
public class StoryChoiceResultData
{
    public string key;
    public string value;
}