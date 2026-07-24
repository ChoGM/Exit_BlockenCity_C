using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public PlayerData player = new PlayerData();
    public RelationshipData relationship = new RelationshipData();
    public ProgressData progress = new ProgressData();
    public FriendlinessData friendlinessData = new FriendlinessData();

    public NewsData news = new NewsData();
}

