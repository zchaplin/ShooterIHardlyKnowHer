using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTracker : MonoBehaviour
{
    public static int score;
    private TMP_Text scoreText;
    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        scoreText = GetComponent<TMP_Text>();
    }

    public void addScore(int mod) {
        score += mod;
        scoreText.text = score.ToString();
    }
}
