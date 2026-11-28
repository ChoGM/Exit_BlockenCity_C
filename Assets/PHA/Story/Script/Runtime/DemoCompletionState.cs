public static class DemoCompletionState
{
    public const string FinalStoryId = "Normal_12";

    public static bool IsCompleted(SaveData data)
    {
        return data != null && data.story != null && data.story.demoCompleted;
    }

    public static void Complete()
    {
        var manager = Datamanager.Instance;
        if (manager.saveData.story == null)
            manager.saveData.story = new StoryProgressData();
        manager.saveData.story.demoCompleted = true;
        manager.SaveGameData();
    }
}
