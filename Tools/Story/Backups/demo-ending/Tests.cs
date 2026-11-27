
using System;
using System.Web.Script.Serialization;
[Serializable] public class SaveData {public StoryProgressData story=new StoryProgressData();}
public class Datamanager {
 public static Datamanager Instance=new Datamanager();
 public SaveData saveData=new SaveData(); public string saved;
 public void SaveGameData(){saved=new JavaScriptSerializer().Serialize(saveData);}
}
class Tests {
 static void Check(bool v){if(!v)throw new Exception("Failed");}
 static void Main(){
  Check(!DemoCompletionState.IsCompleted(null));
  Check(!DemoCompletionState.IsCompleted(new SaveData()));
  Check(!DemoCompletionState.IsCompleted(new SaveData{story=null}));
  var m=Datamanager.Instance;m.saveData.story=null;
  DemoCompletionState.Complete();Check(DemoCompletionState.IsCompleted(m.saveData));
  var loaded=new JavaScriptSerializer().Deserialize<SaveData>(m.saved);
  Check(DemoCompletionState.IsCompleted(loaded));
  var old=new JavaScriptSerializer().Deserialize<SaveData>("{\"story\":{\"completedStoryIds\":[]}}");
  Check(!DemoCompletionState.IsCompleted(old));
  m.saveData=new SaveData();Check(!DemoCompletionState.IsCompleted(m.saveData));
  Console.WriteLine("PASS: new/old/null saves, completion persistence, reload and new-game reset (7 assertions)");
 }
}
