using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum DialogStateKey
{
    FailedStatCheck,
    faxIntroState,
    faxIntroState2,
    faxState1, 
    faxEmpathyState, 
    faxSeductionState,
    faxMechanicalState, 
    faxEmpath1f,
    faxEmpath1s,
    faxEmpath2f,
    faxEmpath2s,
    faxSeduction1f, 
    faxSeduction1s, 
    faxSeduction2f, 
    faxSeduction2s, 
    faxMechanical1f, 
    faxMechanical1s, 
    faxMechanical2f, 
    faxMechanical2s
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

    public DialogOption(string _text, DialogStateKey  _next)
    {
        text = _text;
        triggerExit = false;
        success = _next;
        failure = DialogStateKey.faxIntroState;
        statCheck = IgnoreStatCheck;
    }
};

public delegate DialogRecord RecordGeneratorDelegate();
public delegate void DialogStateEntryDelegate(DialogSystem system);

public struct DialogState
{
    public string reply;
    public List<DialogOption> options;
    public RecordGeneratorDelegate recordGenerator;
    public DialogStateEntryDelegate onStateEntry;
    // Dialog options
    // Modifier
    public DialogState(string _reply)
    {
        reply = _reply;
        options = new List<DialogOption>();
        recordGenerator = null;
        onStateEntry = null;
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

public struct DialogRecord
{
    public string applianceName;
    public bool result;
    public int amount;

    public DialogRecord(string _applianceName, bool _result, int _amount)
    {
        applianceName = _applianceName;
        result = _result;
        amount = _amount;
    }
};

public enum ApplianceKey
{
    FaxMachine,
    Microwave,
    TV,
    Door,
};

public class DialogSystem : MonoBehaviour
{
    public Dictionary<DialogStateKey, DialogState> m_States =
        new Dictionary<DialogStateKey, DialogState>();
    // public Dictionary<DialogOptionKey, DialogOption> m_DialogOptions =
        // new Dictionary<DialogOptionKey, DialogOption>();

    public DialogStateKey m_CurrentState;

    public List<DialogButton> m_DialogButtons = new List<DialogButton>();

    public Text m_Reply;

    public static DialogSystem m_Instance = null;

    public DialogStats m_PlayerStats = new DialogStats(1, 2, 3);
    public DialogStats m_ApplianceStats = new DialogStats(0, 0, 0);

    public List<DialogRecord> m_Records = new List<DialogRecord>();

    private GameObject m_TaskLayoutGroup = null;
    private Dictionary<ApplianceKey, TaskUI> m_TaskUIList = new Dictionary<ApplianceKey, TaskUI>();
    private Dictionary<ApplianceKey, TaskState> m_TaskStates = new Dictionary<ApplianceKey, TaskState>();

    public GameObject m_TaskUIPrefab;

    public void AddTask(ApplianceKey key, string text)
    {
        if (m_TaskLayoutGroup == null)
        {
            m_TaskLayoutGroup = GameObject.FindWithTag("task_layout_group");
        }
        Debug.Assert(m_TaskLayoutGroup != null);
        GameObject task = Instantiate(m_TaskUIPrefab, m_TaskLayoutGroup.transform);

        TaskUI ui = task.GetComponent<TaskUI>();
        Debug.Assert(ui != null);
        ui.m_ApplianceName = text;
        m_TaskUIList.Add(key, ui);

        m_TaskStates.Add(key, TaskState.Incomplete);
    }

    public void SetTaskState(ApplianceKey key, TaskState state)
    {
        Debug.Assert(m_TaskStates.ContainsKey(key));
        Debug.Assert(m_TaskUIList.ContainsKey(key));

        m_TaskStates[key] = state;
        m_TaskUIList[key].SetState(state);

        if (AreAllTasksComplete())
        {
            Debug.Log("All tasks completed");
            if (!m_TaskStates.ContainsKey(ApplianceKey.Door))
            {
                AddTask(ApplianceKey.Door, "Leave");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("DialogSystem start");
        m_Instance = this;

        foreach (var button in m_DialogButtons)
        {
            button.Register(this);
        }

        m_TaskLayoutGroup = GameObject.FindWithTag("task_layout_group");
        Debug.Assert(m_TaskLayoutGroup != null);

        AddTask(ApplianceKey.FaxMachine, "Repair the fax machine");
        //AddTask(ApplianceKey.Microwave, "Repair the microwave");
        //AddTask(ApplianceKey.TV, "Repair the TV");

        GenerateFaxMachineDialogTree();

        SetState(DialogStateKey.faxIntroState);

    }

    public bool AreAllTasksComplete()
    {
        bool result = true;
        foreach (var taskState in m_TaskStates)
        {
            if (taskState.Value == TaskState.Incomplete)
            {
                result = false;
                Debug.Log("Task incomplete " + taskState.Key);
                break;
            }
        }

        return result;
    }

    public void HandleDialogOption(DialogOption option)
    {
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

    public void DumpRecords()
    {
        foreach(var record in m_Records)
        {
            string outcome = record.result ? "[FIXED]" : "[DAMAGED]";
            Debug.Log(record.applianceName + " - " + outcome + " - $" + record.amount);
        }
    }

    public void SetState(DialogStateKey key)
    {
        m_CurrentState = key;

        Debug.Assert(m_States.ContainsKey(key));
        DialogState state = m_States[key];
        if (state.recordGenerator != null)
        {
            m_Records.Add(state.recordGenerator());
            DumpRecords();
        }

        if (state.onStateEntry != null)
        {
            state.onStateEntry(this);
        }

        m_Reply.text = state.reply;
        UpdateDialogOptions();
    }

    private void GenerateFaxMachineDialogTree(){

        DialogState faxIntroState = new DialogState("Oh WOE is ME!");
        faxIntroState.recordGenerator = () => { return new DialogRecord("FAX MACHINE", false, -240); };

        DialogState faxIntroState2 = new  DialogState("T'was a time when Laura had eyes for only me..");
 

        DialogState faxState1 = new DialogState("[The fax machine is babbling and doesn't show any sign of stopping.]");

        DialogState faxEmpathyState = new DialogState("What does the fax need:");
        DialogState faxSeductionState = new DialogState("What is a fax machines deepest desire:");
        DialogState faxMechanicalState = new DialogState("Broken machines need fixin':");
        
        DialogState faxEmpath1f = new DialogState("Is this retirement I see before me? With laura to pull my plug");
        DialogState faxEmpath1s = new DialogState("Today I sh'ant rue the day"); 

        DialogState faxEmpath2s = new DialogState("Ah you saucy boy. The course of true love never did run smooth");
        DialogState faxEmpath2f = new DialogState("Frailty thy name is laura");

        DialogState faxSeduction1s = new DialogState("After a brief period of dial tones, the fax machine produces a trove of lacivious materials which you promptly store.");
        faxSeduction1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Completed); };
        DialogState faxSeduction1f = new DialogState("The fax machine begins dailing for the police and threatens to have you license revoked. You change the topic");

        DialogState faxSeduction2s = new DialogState("The fax machine hums its appreciation. \"If coilling be the food of love, coil on\". You feel uncomfortable.");
        faxSeduction2s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Completed); };
        DialogState faxSeduction2f = new DialogState("You accidentally yank the cable, pulling it from the port. You replace the handset, awkwardly stammering an apology while its stoic face stares on.");

        DialogState faxMechanical1s = new DialogState("Although you failed to affect any meaningful repair, the fax machine appreciates your consideration.");
        DialogState faxMechanical1f = new DialogState("You mistake the paper feeder as a toner repository, the paper is ruined and so is your ego.");

        DialogState faxMechanical2s = new DialogState("The fax machine feels well rested and wakes with a thankful expression on its grotesque face.");
        faxMechanical2s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Completed); };
        DialogState faxMechanical2f = new DialogState("You press the power button with excessive gusto, thereby installing and unintended speed hole. The fax machine's displeasure is obvious");

        
        m_States.Add(DialogStateKey.faxIntroState, faxIntroState);
        m_States.Add(DialogStateKey.faxIntroState2, faxIntroState2);
        m_States.Add(DialogStateKey.faxState1, faxState1);
        m_States.Add(DialogStateKey.faxEmpathyState, faxEmpathyState);
        m_States.Add(DialogStateKey.faxSeductionState, faxSeductionState);
        m_States.Add(DialogStateKey.faxMechanicalState, faxMechanicalState);
        m_States.Add(DialogStateKey.faxEmpath1f, faxEmpath1f);
        m_States.Add(DialogStateKey.faxEmpath1s, faxEmpath1s);
        m_States.Add(DialogStateKey.faxEmpath2f, faxEmpath2f);
        m_States.Add(DialogStateKey.faxEmpath2s, faxEmpath2s);
        m_States.Add(DialogStateKey.faxSeduction1f, faxSeduction1f);
        m_States.Add(DialogStateKey.faxSeduction1s, faxSeduction1s);
        m_States.Add(DialogStateKey.faxSeduction2f, faxSeduction2f);
        m_States.Add(DialogStateKey.faxSeduction2s, faxSeduction2s);
        m_States.Add(DialogStateKey.faxMechanical1f, faxMechanical1f);
        m_States.Add(DialogStateKey.faxMechanical1s, faxMechanical1s);
        m_States.Add(DialogStateKey.faxMechanical2f, faxMechanical2f);
        m_States.Add(DialogStateKey.faxMechanical2s, faxMechanical2s);

        DialogOption faxIntroContinue = new DialogOption("[Continue...]", DialogStateKey.faxIntroState2);
        DialogOption faxIntroContinue2 = new DialogOption("[Continue...]", DialogStateKey.faxState1);
        
        faxIntroState.options.Add(faxIntroContinue);
        faxIntroState2.options.Add(faxIntroContinue2);
        
        DialogOption faxEmpathyOption = new DialogOption("Empathize with the machine", DialogStateKey.faxEmpathyState);
        DialogOption faxSeductionOption = new DialogOption("Attempt a honeypot", DialogStateKey.faxSeductionState);
        DialogOption faxMechanicalOption = new DialogOption("Ignore it, fax machines cant talk", DialogStateKey.faxMechanicalState);
        
        faxState1.options.Add(faxEmpathyOption);
        faxState1.options.Add(faxSeductionOption);
        faxState1.options.Add(faxMechanicalOption);

        DialogOption faxChoiceReturn = new DialogOption("[Continue...]", DialogStateKey.faxState1);

        DialogOption faxEmpathy1Option = new DialogOption("Remind the fax machine that it is loved", DialogStateKey.faxEmpathyState);
        faxEmpathy1Option.success = DialogStateKey.faxEmpath1s;
        faxEmpathy1Option.failure = DialogStateKey.faxEmpath1f;
        faxEmpathy1Option.statCheck = DialogOption.CompareA;


        DialogOption faxEmpathy2Option = new DialogOption("Sympathize with fax machine about feelings for laura", DialogStateKey.faxEmpathyState);
        faxEmpathy2Option.success = DialogStateKey.faxEmpath2s;
        faxEmpathy2Option.failure = DialogStateKey.faxEmpath2f;
        faxEmpathy2Option.statCheck = DialogOption.CompareA;


        DialogOption faxSeduction1Option = new DialogOption("Make a subtle attempt at seduction", DialogStateKey.faxSeductionState);
        faxSeduction1Option.success = DialogStateKey.faxSeduction1s;
        faxSeduction1Option.failure = DialogStateKey.faxSeduction1f;
        faxSeduction1Option.statCheck = DialogOption.CompareA;

        DialogOption faxSeduction2Option = new DialogOption("Curl the phone cord around your finger", DialogStateKey.faxSeductionState);
        faxSeduction2Option.success = DialogStateKey.faxSeduction2s;
        faxSeduction2Option.failure = DialogStateKey.faxSeduction2s;
        faxSeduction2Option.statCheck = DialogOption.CompareA;


        DialogOption faxMechanical1Option = new DialogOption("Make a subtle attempt to check the toner", DialogStateKey.faxMechanicalState);
        faxMechanical1Option.success = DialogStateKey.faxMechanical1s;
        faxMechanical1Option.failure = DialogStateKey.faxMechanical1f;
        faxMechanical1Option.statCheck = DialogOption.CompareA;


        DialogOption faxMechanical2Option = new DialogOption("Try turning it on and off again", DialogStateKey.faxMechanicalState);
        faxMechanical2Option.success = DialogStateKey.faxMechanical2s;
        faxMechanical2Option.failure = DialogStateKey.faxMechanical2f;
        faxMechanical2Option.statCheck = DialogOption.CompareA;

        DialogOption leaveThat = new DialogOption( "Cheese it!", DialogStateKey.faxIntroState);
        leaveThat.triggerExit = true;

        faxState1.options.Add(leaveThat);

        faxEmpathyState.options.Add(faxEmpathy1Option);
        faxEmpathyState.options.Add(faxEmpathy2Option);

        faxSeductionState.options.Add(faxSeduction1Option);
        faxSeductionState.options.Add(faxSeduction2Option);

        faxMechanicalState.options.Add(faxMechanical1Option);
        faxMechanicalState.options.Add(faxMechanical2Option);

        faxEmpath1s.options.Add(faxChoiceReturn);
        faxEmpath1f.options.Add(faxChoiceReturn);

        faxEmpath2s.options.Add(faxChoiceReturn);
        faxEmpath2f.options.Add(faxChoiceReturn);

        faxSeduction1s.options.Add(faxChoiceReturn);
        faxSeduction1f.options.Add(faxChoiceReturn);

        faxSeduction2s.options.Add(faxChoiceReturn);
        faxSeduction2f.options.Add(faxChoiceReturn);
        
        faxMechanical1f.options.Add(faxChoiceReturn);
        faxMechanical1s.options.Add(faxChoiceReturn);
        
        faxMechanical2f.options.Add(faxChoiceReturn);
        faxMechanical2s.options.Add(faxChoiceReturn);

    }
    public void UpdateDialogOptions()
    {
        // Hide all dialog options objects
        foreach (var button in m_DialogButtons)
        {
            button.Hide();
        }

        // For the current state, set dialog options
        Debug.Assert(m_States.ContainsKey(m_CurrentState));
        DialogState state = m_States[m_CurrentState];
        for (var i = 0; i < state.options.Count; i++)
        {
            DialogOption option = state.options[i];

            DialogButton button = m_DialogButtons[i];
            button.SetOption(option.text, option);
        }
    }
}
