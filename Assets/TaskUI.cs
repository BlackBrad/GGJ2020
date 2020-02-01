using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TaskState
{
    Incomplete,
    Completed,
    Failed,
};

public class TaskUI : MonoBehaviour
{
    public string m_ApplianceName;
    public TaskState m_State;
    public Color m_IncompleteColor;
    public Color m_CompleteColor;
    public Color m_FailedColor;

    private Text m_Text;

    // Start is called before the first frame update
    void Start()
    {
        m_Text = GetComponent<Text>();
        SetState(m_State);
    }

    public static string BuildString(string text, TaskState state)
    {
        string stateName = "";
        switch (state)
        {
            case TaskState.Incomplete:
                stateName = "";
                break;
            case TaskState.Completed:
                stateName = " - COMPLETED";
                break;
            case TaskState.Failed:
                stateName = " - FAILED";
                break;
            default:
                break;
        }

        return "- " + text + " " + stateName;
    }

    public void SetState(TaskState state)
    {
        m_State = state;
        m_Text.text = BuildString(m_ApplianceName, state);

        Color color = m_IncompleteColor;
        switch (state)
        {
            case TaskState.Completed:
                color = m_CompleteColor;
                break;
            case TaskState.Failed:
                color = m_FailedColor;
                break;
            default:
                break;
        }

        m_Text.color = color;
    }
}
