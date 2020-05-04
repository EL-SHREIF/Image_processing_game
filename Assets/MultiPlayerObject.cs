using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MultiPlayerObject : NetworkBehaviour
{
    public static MultiPlayerObject INSTANCE;

    [SyncVar]
    public int clientScore = 0;
    [SyncVar]
    public int serverScore = 0;
    [SyncVar]
    public string clientLabel;
    [SyncVar]
    public string serverLabel;
    [SyncVar]
    public int clientLevel;
    [SyncVar]
    public int serverLevel;

    // Start is called before the first frame update
    void Start()
    {
        if(INSTANCE == null)
        {
            if (isServer && isLocalPlayer) return;
            if (!isServer && !isLocalPlayer) return;
            INSTANCE = this;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isServer && isLocalPlayer) return;
        if (!isServer && !isLocalPlayer) return;
    }

    public void updateClientScore(int score)
    {
        CmdUpdateClient(score, clientLevel, clientLabel);
    }
    public void updateServerScore(int score)
    {
        serverScore = score;
    }

    public void updateClientLevel(int level)
    {
        CmdUpdateClient(clientScore, level, clientLabel);
    }
    public void updateServerLevel(int level)
    {
        serverLevel= level;
    }

    public void updateClientLabel(string label)
    {
        CmdUpdateClient(clientScore, clientLevel, label);
    }
    public void updateServerLabel(string label)
    {
        serverLabel = label;
    }

    [Command]
    private void CmdUpdateClient(int score, int level, string label)
    {
        clientLevel = level;
        clientScore = score;
        clientLabel = label;
    }
}
