
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
namespace UnityEngine {
 public static class PlayerPrefs {
  static Dictionary<string,string> data=new Dictionary<string,string>();
  public static bool HasKey(string k){return data.ContainsKey(k);}
  public static string GetString(string k){return data[k];}
  public static void SetString(string k,string v){data[k]=v;}
  public static void Save(){}
 }
 public static class JsonUtility {
  public static T FromJson<T>(string s){return new JavaScriptSerializer().Deserialize<T>(s);}
  public static string ToJson(object o){return new JavaScriptSerializer().Serialize(o);}
 }
 public static class Debug { public static void LogError(string s){throw new Exception(s);} }
}
public class Datamanager {
 public static Datamanager Instance=new Datamanager();
 public void EnsureLoaded(){}
 public void DeleteGameData(){GameDataManager.Instance.Hongryeon=0;GameDataManager.Instance.Yaseo=0;}
}
public class GameDataManager {
 public static GameDataManager Instance=new GameDataManager();
 public float Hongryeon,Yaseo;
 public float GetRelationship(RelationshipType t){return t==RelationshipType.Hongryeon?Hongryeon:Yaseo;}
}
class Tests {
 static int count;
 static void Check(bool ok){count++;if(!ok)throw new Exception("Failed test "+count);}
 static bool Eval(string k){bool result;Check(StoryChoiceRules.TryEvaluate(k,"True",out result));return result;}
 static void Main(){
  Check(!Eval("history:AskedHongryeon"));
  Check(!Eval("history.any:AskedHongryeon|AskedYaseo"));
  foreach(float f in new[]{-1f,0f,0.01f}){
   GameDataManager.Instance.Hongryeon=f;
   Check(Eval("relationship.positive:Hongryeon")== (f>0));
   Check(!Eval("relationship.positive:Yaseo"));
  }
  StoryChoiceRules.RecordResult("history:AskedHongryeon","True");
  Check(Eval("history:AskedHongryeon"));Check(!Eval("history:AskedYaseo"));
  Check(Eval("history.any:AskedHongryeon|AskedYaseo"));
  Datamanager.Instance.DeleteGameData();
  Check(Eval("history:AskedHongryeon"));Check(!Eval("relationship.positive:Hongryeon"));
  StoryChoiceRules.RecordResult("history:AskedYaseo","True");
  Check(Eval("history:AskedYaseo"));Check(Eval("history:AskedHongryeon"));
  StoryChoiceRules.RecordResult("history:AskedHongryeon","False");
  Check(Eval("history:AskedHongryeon"));
  StoryChoiceRules.RecordResult("history:AskedHongryeon","True");
  Check(UnityEngine.JsonUtility.FromJson<StoryChoiceHistoryData>(UnityEngine.PlayerPrefs.GetString("Story.ChoiceHistory.v1")).results.Count==3);
  bool b;Check(!StoryChoiceRules.TryEvaluate("ordinaryKey","True",out b));
  Check(!Eval("relationship.positive:invalid"));
  Check(!Eval("relationship.positive:3"));
  Console.WriteLine("PASS: "+count+" assertions (real rule/history code, isolated storage/game adapters)");
 }
}
