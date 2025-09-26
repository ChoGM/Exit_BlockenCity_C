﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DialogManager : MonoBehaviour
{
    private Queue<DialogLine> dialogQueue = new Queue<DialogLine>();
    private List<DialogLine> allDialogs = new List<DialogLine>();

    public static DialogManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadDialogCSV("DialogDB"); // Resources/DialogDB.csv
    }

    public void LoadDialogCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        StringReader reader = new StringReader(csvFile.text);

        bool isFirstLine = true;

        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();

            // 첫 줄이 헤더면 건너뛴다
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            string[] values = line.Split(',');

            int branch = int.Parse(values[0]);
            string name = values[1];
            string dialog = values[2];

            int questionType = 0; // 기본값 0
            if (values.Length > 3 && !string.IsNullOrEmpty(values[3]))
                int.TryParse(values[3], out questionType);

            int acceptBranch = -1;
            int declineBranch = -1;

            if (values.Length > 4 && !string.IsNullOrEmpty(values[4]))
                int.TryParse(values[4], out acceptBranch);

            if (values.Length > 5 && !string.IsNullOrEmpty(values[5]))
                int.TryParse(values[5], out declineBranch);

            allDialogs.Add(new DialogLine(branch, name, dialog, questionType, acceptBranch, declineBranch));
        }
    }

    public void LoadDialogByBranch(int branchID)
    {
        dialogQueue.Clear();

        foreach (var dialog in allDialogs)
        {
            if (dialog.branch == branchID)
            {
                dialogQueue.Enqueue(dialog);
            }
        }
    }

    public DialogLine GetNextDialog()
    {
        if (dialogQueue.Count > 0)
        {
            return dialogQueue.Dequeue();
        }
        return null;
    }

    public bool HasMoreDialog()
    {
        return dialogQueue.Count > 0;
    }

    public void ClearQueue()  // ⬅️ 거절 등으로 대화 중단 시 사용
    {
        dialogQueue.Clear();
    }
}
