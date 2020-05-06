using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class finalScene : MonoBehaviour
{
    public GameObject task_msg;
    public GameObject press_here;
    string score;
    // Start is called before the first frame update
    void Start()
    {
        score = PlayerPrefs.GetString("player_score");  //get the name from last Scene
        Debug.Log(score);
        task_msg.GetComponent<Text>().text = score;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ON_move_click()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);

    }
}
