using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float gameTime = 180f;

    [HideInInspector] public ScoreUIBinder scoreManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TimeOver()
    {
        Debug.Log("일차 종료!");

        //점수창 활성화
        scoreManager.ToggleScoreUI(true);
    }
}
