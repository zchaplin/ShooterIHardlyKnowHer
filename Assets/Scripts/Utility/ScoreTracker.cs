using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreTracker : NetworkBehaviour
{
    private NetworkVariable<int> score = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private TMP_Text scoreText;

    void Start()
    {
        scoreText = GetComponent<TMP_Text>();
        score.OnValueChanged += OnScoreChanged;
        UpdateScoreUI(score.Value);
    }

    private void OnScoreChanged(int oldScore, int newScore)
    {
        UpdateScoreUI(newScore);
    }

    public void addScore(int mod)
    {
        if (IsServer)
        {
            score.Value += mod;
        }
        else
        {
            SubmitScoreServerRpc(mod);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitScoreServerRpc(int mod)
    {
        score.Value += mod;
    }

    private void UpdateScoreUI(int newScore)
    {
        scoreText.text = newScore.ToString();
    }
}
