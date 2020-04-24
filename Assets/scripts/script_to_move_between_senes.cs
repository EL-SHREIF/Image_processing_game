using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class script_to_move_between_senes : MonoBehaviour
{
    public GameObject the_text;
    public GameObject the_panel;
    // Start is called before the first frame update
    void Start()
    {
        the_panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void click_on_ok() {
        the_panel.SetActive(false);
    }

    public void ON_move_click(){

        // here I should store my last name before move to level two
        string nick_name=the_text.GetComponent<InputField>().text;
        PlayerPrefs.SetString("player_name", nick_name);
        if (nick_name.Length < 3 || nick_name.Length > 6)
        {
            the_panel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }
}
