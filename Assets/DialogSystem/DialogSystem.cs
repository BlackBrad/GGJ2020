using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum DialogStateKey
{
    Intro,
    Test1,
    Test2,
    FailedStatCheck,
};

public enum DialogOptionKey
{
    DoYouLikeYourself,
    HaveYouEverHadADream,
    LeaveThatToasterAlone,
    ErrorError,
    Leave,
    FailStatCheck,
};

public delegate bool DialogStatCheck(DialogStats playerStats, DialogStats applianceStats);

public struct DialogOption
{
    public string text;
    public DialogStatCheck statCheck;
    public DialogStateKey success;
    public DialogStateKey failure;
    public bool triggerExit;

    public static bool IgnoreStatCheck(DialogStats playerStats, DialogStats applianceStats)
    {
        return true;
    }

    public static bool FailStatCheck(DialogStats playerStats, DialogStats applianceStats)
    {
        return false;
    }

    public static bool CompareA(DialogStats playerStats, DialogStats applianceStats)
    {
        return playerStats.A >= applianceStats.A;
    }

    public DialogOption(string _text)
    {
        text = _text;
        triggerExit = false;
        success = DialogStateKey.Intro;
        failure = DialogStateKey.Intro;
        statCheck = IgnoreStatCheck;
    }
};

public struct DialogState
{
    public string reply;
    public List<DialogOptionKey> options;
    // Dialog options
    // Modifier
    public DialogState(string _reply)
    {
        reply = _reply;
        options = new List<DialogOptionKey>();
    }
};

public struct DialogStats
{
    public int A;
    public int B;
    public int C;

    public DialogStats(int _A, int _B, int _C)
    {
        A = _A;
        B = _B;
        C = _C;
    }
};

public class DialogSystem : MonoBehaviour
{
    public Dictionary<DialogStateKey, DialogState> m_States =
        new Dictionary<DialogStateKey, DialogState>();
    public Dictionary<DialogOptionKey, DialogOption> m_DialogOptions =
        new Dictionary<DialogOptionKey, DialogOption>();

    public DialogStateKey m_CurrentState;

    public List<DialogButton> m_DialogButtons = new List<DialogButton>();

    public Text m_Reply;

    public static DialogSystem m_Instance = null;

    public DialogStats m_PlayerStats = new DialogStats(1, 2, 3);
    public DialogStats m_ApplianceStats = new DialogStats(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        m_Instance = this;

        DialogState introState = new DialogState("Woe is me!");
        introState.options.Add(DialogOptionKey.DoYouLikeYourself);
        introState.options.Add(DialogOptionKey.HaveYouEverHadADream);
        introState.options.Add(DialogOptionKey.Leave);

        DialogState test1State = new DialogState("Always questions");
        test1State.options.Add(DialogOptionKey.LeaveThatToasterAlone);
        test1State.options.Add(DialogOptionKey.FailStatCheck);
        test1State.options.Add(DialogOptionKey.Leave);

        DialogState test2State = new DialogState("That you, you, could, could do, .asdadsfsdf");
        test2State.options.Add(DialogOptionKey.ErrorError);
        test2State.options.Add(DialogOptionKey.Leave);

        DialogState failStatCheckState = new DialogState("FAILED STAT CHECK!");
        failStatCheckState.options.Add(DialogOptionKey.Leave);

        m_States.Add(DialogStateKey.Intro, introState);
        m_States.Add(DialogStateKey.Test1, test1State);
        m_States.Add(DialogStateKey.Test2, test2State);
        m_States.Add(DialogStateKey.FailedStatCheck, failStatCheckState);

        DialogOption leave = new DialogOption("<Leave>");
        leave.triggerExit = true;

        DialogOption doYou = new DialogOption("Do you like yourself?");
        doYou.success = DialogStateKey.Test1;

        DialogOption haveYou = new DialogOption( "Have you ever had a dream?");
        haveYou.success = DialogStateKey.Test2;

        DialogOption leaveThat = new DialogOption( "Leave that toaster alone!");

        DialogOption error = new DialogOption( "Error...Error..Error");

        DialogOption failStatCheck = new DialogOption("Fail stat check");
        failStatCheck.statCheck = DialogOption.CompareA;
        failStatCheck.success = DialogStateKey.Test2;
        failStatCheck.failure = DialogStateKey.FailedStatCheck;

        m_DialogOptions.Add(DialogOptionKey.DoYouLikeYourself, doYou);
        m_DialogOptions.Add(DialogOptionKey.HaveYouEverHadADream, haveYou);
        m_DialogOptions.Add(DialogOptionKey.LeaveThatToasterAlone, leaveThat);
        m_DialogOptions.Add(DialogOptionKey.ErrorError, error);
        m_DialogOptions.Add(DialogOptionKey.Leave, leave);
        m_DialogOptions.Add(DialogOptionKey.FailStatCheck, failStatCheck);

        foreach (var button in m_DialogButtons)
        {
            button.Register(this);
        }

        SetState(DialogStateKey.Intro);
    }

    public void HandleDialogOption(DialogOptionKey key)
    {
        DialogOption option = m_DialogOptions[key];
        if (option.triggerExit)
        {
            PlayerStateManager.m_Instance.SetState(PlayerState.Moving);
        }
        else
        {
            if (option.statCheck(m_PlayerStats, m_ApplianceStats))
            {
                Debug.Log("Stat check passed!");
                SetState(option.success);
            }
            else
            {
                Debug.Log("Stat check failed!");
                SetState(option.failure);
            }
        }
    }

    public void SetState(DialogStateKey key)
    {
        m_CurrentState = key;
        DialogState state = m_States[key];
        m_Reply.text = state.reply;
        UpdateDialogOptions();
    }

    public void UpdateDialogOptions()
    {
        // Hide all dialog options objects
        foreach (var button in m_DialogButtons)
        {
            button.Hide();
        }

        // For the current state, set dialog options
        DialogState state = m_States[m_CurrentState];
        for (var i = 0; i < state.options.Count; i++)
        {
            DialogOptionKey optionKey = state.options[i];
            DialogOption option = m_DialogOptions[optionKey];

            DialogButton button = m_DialogButtons[i];
            button.SetOption(option.text, optionKey);
        }
    }
}
