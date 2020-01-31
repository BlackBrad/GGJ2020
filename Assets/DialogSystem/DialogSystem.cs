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
};

public struct DialogOption
{
    public string text;
    // Transition check
    public DialogStateKey success;
    public DialogStateKey failure;
};

public struct DialogState
{
    public string reply;
    public List<DialogOptionKey> options;
    // Dialog options
    // Modifier
};

public class DialogSystem : MonoBehaviour
{
    public Dictionary<DialogStateKey, DialogState> m_States =
        new Dictionary<DialogStateKey, DialogState>();
    public Dictionary<DialogOptionKey, DialogOption> m_DialogOptions =
        new Dictionary<DialogOptionKey, DialogOption>();

    public DialogStateKey m_CurrentState;

    public List<DialogButton> m_DialogButtons = new List<DialogButton>();

    public int enemyHealth = 2;

    public Button m_FirstButton;
    public Text m_Reply;

    // Start is called before the first frame update
    void Start()
    {
        DialogState introState = new DialogState();
        introState.options = new List<DialogOptionKey>();
        introState.reply = "Woe is me!";
        introState.options.Add(DialogOptionKey.DoYouLikeYourself);
        introState.options.Add(DialogOptionKey.HaveYouEverHadADream);

        DialogState test1State = new DialogState();
        test1State.options = new List<DialogOptionKey>();
        test1State.reply = "Always questions";
        test1State.options.Add(DialogOptionKey.LeaveThatToasterAlone);

        DialogState test2State = new DialogState();
        test2State.options = new List<DialogOptionKey>();
        test2State.reply = "That you, you, could, could do, .asdadsfsdf";
        test2State.options.Add(DialogOptionKey.ErrorError);

        m_States.Add(DialogStateKey.Intro, introState);
        m_States.Add(DialogStateKey.Test1, test1State);
        m_States.Add(DialogStateKey.Test2, test2State);

        DialogOption doYou = new DialogOption();
        doYou.text = "Do you like yourself?";
        doYou.success = DialogStateKey.Test1;

        DialogOption haveYou = new DialogOption();
        haveYou.text = "Have you ever had a dream?";
        haveYou.success = DialogStateKey.Test2;

        DialogOption leaveThat = new DialogOption();
        leaveThat.text = "Leave that toaster alone!";

        DialogOption error = new DialogOption();
        error.text = "Error...Error..Error";

        m_DialogOptions.Add(DialogOptionKey.DoYouLikeYourself, doYou);
        m_DialogOptions.Add(DialogOptionKey.HaveYouEverHadADream, haveYou);
        m_DialogOptions.Add(DialogOptionKey.LeaveThatToasterAlone, leaveThat);
        m_DialogOptions.Add(DialogOptionKey.ErrorError, error);

        foreach (var button in m_DialogButtons)
        {
            button.Register(this);
        }

        SetState(DialogStateKey.Intro);
    }

    public void HandleDialogOption(DialogOptionKey key)
    {
        DialogOption option = m_DialogOptions[key];
        SetState(option.success);
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
