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
};

public enum DialogOptionKey
{
    DoYouLikeYourself,
    HaveYouEverHadADream,
    LeaveThatToasterAlone,
    ErrorError,
    Leave,
};

public struct DialogOption
{
    public string text;
    // Transition check
    public DialogStateKey success;
    public DialogStateKey failure;
    public bool triggerExit;

    public DialogOption(string _text)
    {
        text = _text;
        triggerExit = false;
        success = DialogStateKey.Intro;
        failure = DialogStateKey.Intro;
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

public class DialogSystem : MonoBehaviour
{
    public Dictionary<DialogStateKey, DialogState> m_States =
        new Dictionary<DialogStateKey, DialogState>();
    public Dictionary<DialogOptionKey, DialogOption> m_DialogOptions =
        new Dictionary<DialogOptionKey, DialogOption>();

    public DialogStateKey m_CurrentState;

    public List<DialogButton> m_DialogButtons = new List<DialogButton>();

    public Text m_Reply;

    // Start is called before the first frame update
    void Start()
    {
        DialogState introState = new DialogState("Woe is me!");
        introState.options.Add(DialogOptionKey.DoYouLikeYourself);
        introState.options.Add(DialogOptionKey.HaveYouEverHadADream);
        introState.options.Add(DialogOptionKey.Leave);

        DialogState test1State = new DialogState("Always questions");
        test1State.options.Add(DialogOptionKey.LeaveThatToasterAlone);
        test1State.options.Add(DialogOptionKey.Leave);

        DialogState test2State = new DialogState("That you, you, could, could do, .asdadsfsdf");
        test2State.options.Add(DialogOptionKey.ErrorError);
        test2State.options.Add(DialogOptionKey.Leave);

        m_States.Add(DialogStateKey.Intro, introState);
        m_States.Add(DialogStateKey.Test1, test1State);
        m_States.Add(DialogStateKey.Test2, test2State);

        DialogOption leave = new DialogOption("<Leave>");
        leave.triggerExit = true;

        DialogOption doYou = new DialogOption("Do you like yourself?");
        doYou.success = DialogStateKey.Test1;

        DialogOption haveYou = new DialogOption( "Have you ever had a dream?");
        haveYou.success = DialogStateKey.Test2;

        DialogOption leaveThat = new DialogOption( "Leave that toaster alone!");

        DialogOption error = new DialogOption( "Error...Error..Error");

        m_DialogOptions.Add(DialogOptionKey.DoYouLikeYourself, doYou);
        m_DialogOptions.Add(DialogOptionKey.HaveYouEverHadADream, haveYou);
        m_DialogOptions.Add(DialogOptionKey.LeaveThatToasterAlone, leaveThat);
        m_DialogOptions.Add(DialogOptionKey.ErrorError, error);
        m_DialogOptions.Add(DialogOptionKey.Leave, leave);

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
            SetState(option.success);
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
