using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogSystem : MonoBehaviour
{
    public int enemyHealth = 2;

    public Button m_FirstButton;
    public Text m_Reply;

    // Start is called before the first frame update
    void Start()
    {
        //m_FirstButton.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void HandleAction(int selection)
    {
        if (selection == 1)
        {
        }
    }

    public void TaskOnClick()
    {
        Debug.Log("TaskOnClick");
        m_Reply.text = "This is my reply";
        enemyHealth -= 1;
    }
}
