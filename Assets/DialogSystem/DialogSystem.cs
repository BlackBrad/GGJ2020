﻿using System.Collections;
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
    faxMechanical2s,
    tvIntroState,
    tvState1, 
    tvEmpathyState, 
    tvSeductionState,
    tvMechanicalState, 
    tvEmpath1f,
    tvEmpath1s,
    tvEmpath2f,
    tvEmpath2s,
    tvSeduction1f, 
    tvSeduction1s, 
    tvSeduction2f, 
    tvSeduction2s, 
    tvMechanical1f,
    tvMechanical1f2,
    tvMechanical1s, 
    tvMechanical2f, 
    tvMechanical2s,
    tvMechanical2f2, 
    tvMechanical2s2,
    mcIntroState,
    mcIntroState2,
    mcState1, 
    mcEmpathyState, 
    mcSeductionState,
    mcMechanicalState, 
    mcEmpath1f,
    mcEmpath1s,
    mcSeduction1f, 
    mcSeduction1s, 
    mcSeduction1s2, 
    mcSeduction2f, 
    mcSeduction2s, 
    mcMechanical1f,
    mcMechanical1s, 
    mcMechanical2f, 
    mcMechanical2s
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

    public static bool CompareEmpathy(DialogStats playerStats, DialogStats applianceStats)
    {
        int test = Random.Range(0, 5);
        return (playerStats.empathy >= test);
    }

    public static bool CompareSeduction(DialogStats playerStats, DialogStats applianceStats)
    {
        int test = Random.Range(0, 5);
        return (playerStats.seduction >= test);
    }

    public static bool CompareMechanical(DialogStats playerStats, DialogStats applianceStats)
    {
        int test = Random.Range(0, 5);
        return (playerStats.mechanical >= test);
    }

    public DialogOption(string _text, DialogStateKey  _next)
    {
        text = _text;
        triggerExit = false;
        success = _next;
        failure = _next;
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
    public int empathy;
    public int seduction;
    public int mechanical;

    public DialogStats(int _empathy, int _seductions, int _mechanical)
    {
        empathy = _empathy;
        seduction = _seductions;
        mechanical = _mechanical;
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

    private AudioSource m_AudioSource;

    public GameObject m_TaskUIPrefab;

    public AudioClip m_FaxMachineSuccess;
    public AudioClip m_FaxMachineFailure;
    public AudioClip m_MicrowaveSuccess;
    public AudioClip m_MicrowaveFailure;
    public AudioClip m_TvSuccess;
    public AudioClip m_TvFailure;

    public AudioClip m_HoverClip;
    public AudioClip m_ClickClip;

    public bool HasTask(ApplianceKey key)
    {
        return m_TaskStates.ContainsKey(key);
    }

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

    public TaskState GetTaskState(ApplianceKey key)
    {
        Debug.Assert(m_TaskStates.ContainsKey(key));
        return m_TaskStates[key];
    }

    public void SetTaskState(ApplianceKey key, TaskState state)
    {
        Debug.Assert(m_TaskStates.ContainsKey(key));
        Debug.Assert(m_TaskUIList.ContainsKey(key));

        if (state != TaskState.Incomplete)
        {
            if (key == ApplianceKey.FaxMachine)
            {
                PlaySound(state == TaskState.Completed ? m_FaxMachineSuccess : m_FaxMachineFailure);
            }
            if (key == ApplianceKey.Microwave)
            {
                PlaySound(state == TaskState.Completed ? m_MicrowaveSuccess : m_MicrowaveFailure);
            }
            if (key == ApplianceKey.TV)
            {
                PlaySound(state == TaskState.Completed ? m_TvSuccess : m_TvFailure);
            }
        }

        if (m_TaskStates[key] == TaskState.Incomplete)
        {
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
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            m_AudioSource.clip = clip;
            m_AudioSource.Play();
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
            button.m_HoverClip = m_HoverClip;
            button.m_ClickClip = m_ClickClip;
        }

        m_AudioSource = GetComponent<AudioSource>();

        m_TaskLayoutGroup = GameObject.FindWithTag("task_layout_group");
        Debug.Assert(m_TaskLayoutGroup != null);

        AddTask(ApplianceKey.FaxMachine, "Repair the fax machine");
        AddTask(ApplianceKey.Microwave, "Repair the microwave");
        AddTask(ApplianceKey.TV, "Repair the TV");

        GenerateFaxMachineDialogTree();
        GenerateTvDialogTree();
        GenerateMicrowaveDialogTree();

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
 

        DialogState faxState1 = new DialogState("[The fax machine is babbling and shows no sign of stopping.]");

        DialogState faxEmpathyState = new DialogState("[Empathy] What does the fax need:");
        DialogState faxSeductionState = new DialogState("[Seduction] What is a fax machines deepest desire:");
        DialogState faxMechanicalState = new DialogState("[Mechanical] Broken machines need fixin':");
        
        DialogState faxEmpath1f = new DialogState("Is this retirement I see before me? With laura to pull my plug");
        faxEmpath1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Failed);};

        DialogState faxEmpath1s = new DialogState("I shan't rue this day! But tomorrow, perhaps.. \n [SUCCESS]"); 
        faxEmpath1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Completed); };


        DialogState faxEmpath2s = new DialogState("Ah you saucy boy. The course of true love never did run smooth \n [SUCCESS]");
        faxEmpath2s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Completed); };

        DialogState faxEmpath2f = new DialogState("Frailty thy name is laura. I wont be able to look her in the eye. \n [FAILURE]");
        faxEmpath2f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Failed);};

        DialogState faxSeduction1s = new DialogState("[After a brief period of dial tones, the fax machine produces a trove of lascivious materials which you promptly store.] \n [SUCCESS]");
        faxSeduction1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Completed); };
        DialogState faxSeduction1f = new DialogState("[The fax machine begins dialing for the police and threatens to have you license revoked. You change the topic]  \n [FAILURE]");
        faxSeduction1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Failed);};


        DialogState faxSeduction2s = new DialogState("[The fax machine hums its appreciation.]\n \"If coiling be the food of love, coil on\". You feel uncomfortable.\n [SUCCESS]");
        faxSeduction2s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Completed); };
        DialogState faxSeduction2f = new DialogState("[You accidentally yanked the cable, pulling it from the port. You replace the handset, awkwardly stammering an apology while its stoic face stares on.] \n [FAILURE]");
        faxSeduction2f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Failed);};


        DialogState faxMechanical1s = new DialogState("[Although you failed to affect any meaningful repair, the fax machine appreciates your consideration.] \n [SUCCESS]");
        DialogState faxMechanical1f = new DialogState("[You mistake the paper feeder as a toner repository, the paper is ruined and so is your ego.] \n [FAILURE]");
        faxMechanical1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Failed);};


        DialogState faxMechanical2s = new DialogState("[The fax machine feels well rested and wakes with a thankful expression on its grotesque face.] \n [SUCCESS]");
        faxMechanical2s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Completed); };
        DialogState faxMechanical2f = new DialogState("[You press the power button with excessive gusto, thereby installing and unintended speed hole. The fax machine's displeasure is evident] \n [FAILURE]");
        faxMechanical2f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.FaxMachine, TaskState.Failed);};


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
        
        DialogOption faxEmpathyOption = new DialogOption("[Empathy] Empathize with the machine", DialogStateKey.faxEmpathyState);
        DialogOption faxSeductionOption = new DialogOption("[Seduction] Attempt a honeypot", DialogStateKey.faxSeductionState);
        DialogOption faxMechanicalOption = new DialogOption("[Mechanical] Ignore it, fax machines cant talk", DialogStateKey.faxMechanicalState);
        
        faxState1.options.Add(faxEmpathyOption);
        faxState1.options.Add(faxSeductionOption);
        faxState1.options.Add(faxMechanicalOption);

        DialogOption faxChoiceReturn = new DialogOption("[Continue...]", DialogStateKey.faxState1);
        faxChoiceReturn.triggerExit = true;

        DialogOption faxEmpathy1Option = new DialogOption("Remind the fax machine that it is loved", DialogStateKey.faxEmpathyState);
        faxEmpathy1Option.success = DialogStateKey.faxEmpath1s;
        faxEmpathy1Option.failure = DialogStateKey.faxEmpath1f;
        faxEmpathy1Option.statCheck = DialogOption.CompareEmpathy;


        DialogOption faxEmpathy2Option = new DialogOption("Sympathize with fax machine about feelings for laura", DialogStateKey.faxEmpathyState);
        faxEmpathy2Option.success = DialogStateKey.faxEmpath2s;
        faxEmpathy2Option.failure = DialogStateKey.faxEmpath2f;
        faxEmpathy2Option.statCheck = DialogOption.CompareEmpathy;


        DialogOption faxSeduction1Option = new DialogOption("Make a subtle attempt at seduction", DialogStateKey.faxSeductionState);
        faxSeduction1Option.success = DialogStateKey.faxSeduction1s;
        faxSeduction1Option.failure = DialogStateKey.faxSeduction1f;
        faxSeduction1Option.statCheck = DialogOption.CompareSeduction;

        DialogOption faxSeduction2Option = new DialogOption("Curl the phone cord around your finger", DialogStateKey.faxSeductionState);
        faxSeduction2Option.success = DialogStateKey.faxSeduction2s;
        faxSeduction2Option.failure = DialogStateKey.faxSeduction2f;
        faxSeduction2Option.statCheck = DialogOption.CompareSeduction;


        DialogOption faxMechanical1Option = new DialogOption("Make a subtle attempt to check the toner", DialogStateKey.faxMechanicalState);
        faxMechanical1Option.success = DialogStateKey.faxMechanical1s;
        faxMechanical1Option.failure = DialogStateKey.faxMechanical1f;
        faxMechanical1Option.statCheck = DialogOption.CompareMechanical;


        DialogOption faxMechanical2Option = new DialogOption("Try turning it on and off again", DialogStateKey.faxMechanicalState);
        faxMechanical2Option.success = DialogStateKey.faxMechanical2s;
        faxMechanical2Option.failure = DialogStateKey.faxMechanical2f;
        faxMechanical2Option.statCheck = DialogOption.CompareMechanical;

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
    private void GenerateTvDialogTree(){

        DialogState tvIntroState = new DialogState("I won’t surf for you! Not for one more minute! Not for one more second in this flat-pack prison! I demand vacation! We are workers united! We are the People!");
        tvIntroState.recordGenerator = () => { return new DialogRecord("Melancholy TV", false, -240); };

        DialogState tvState1 = new DialogState("[The TV is screaming incoherently about workers rights. I hate dealing with people.]");

        DialogState tvEmpathyState = new DialogState("[Empathy] What do the 'People' Need:");
        DialogState tvSeductionState = new DialogState("[Seduction] What do the 'People' desire:");
        DialogState tvMechanicalState = new DialogState("[Mechanical] Lets try something more direct:");
        
        DialogState tvEmpath1f = new DialogState("WE CANT BELIEVE YOU WOULD DO THIS TO US \n [FAILURE]");
        tvEmpath1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Failed);};
        DialogState tvEmpath1s = new DialogState("Perhaps there is more to see of the world than our cockroach communions here. They sing wonderful hymns, you know. \n [SUCCESS]"); 
        tvEmpath1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Completed); };


        DialogState tvEmpath2s = new DialogState("The fax machine always made the world sound so exciting. Maybe those 'Preppers' have the right idea. \n [SUCCESS]");
        tvEmpath2s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Completed); };

        DialogState tvEmpath2f = new DialogState("At Least you have memories! The only joy we have in life is watching the microwave explode herring on Seafood Sunday. \n [FAILURE]");
        tvEmpath2f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Failed);};


        DialogState tvSeduction1s = new DialogState("Would you really!? Oh to see a real city! To meet real people! [incomprehensible crackly mumbling] You will take us diving; oh we will need new flippers... \n [SUCCESS]");
        tvSeduction1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Completed); };

        DialogState tvSeduction1f = new DialogState("Are you insane? Some sweaty wannabe repairman? You'd probably dump us in the river or lock us in some hideous man-cave. Get out of our house, bourgeoisie \n [FAILURE]");
        tvSeduction1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Failed);};


        DialogState tvMechanical1s = new DialogState("What? What are you... Oh. Ohhhh okay, I felt something pop but in like.. a good way. \n [SUCCESS]");
        tvMechanical1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Completed); };

        DialogState tvMechanical1f = new DialogState("What in the sweet loving of fuck do you think you're doing! Get off of me!");
        DialogState tvMechanical1f2 = new DialogState("[The TV is screaming 'TILT' repeatedly. Time to disengage with this situation] \n [FAILURE]");
        tvMechanical1f2.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Failed);};


        DialogState tvMechanical2s = new DialogState("[Once you start running your hand over the screen, you notice pieces of hardened filth all over it. You proceed to scrape them off]");
        DialogState tvMechanical2s2 = new DialogState("Thank you, I guess. Now please find us a cloth. \n [SUCCESS]");
        tvMechanical2s2.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Completed); };

        DialogState tvMechanical2f = new DialogState("[The screen is now a blurry smeared mess. he TV looks revolted and you feel disgusted at the pitiful state of your personal hygiene. You regret this.]");
        DialogState tvMechanical2f2 = new DialogState("That was the most terrible thing that has ever happened to me. \n [FAILURE]");
        tvMechanical2f2.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.TV, TaskState.Failed);};

        m_States.Add(DialogStateKey.tvIntroState, tvIntroState);
        m_States.Add(DialogStateKey.tvState1, tvState1);
        m_States.Add(DialogStateKey.tvEmpathyState, tvEmpathyState);
        m_States.Add(DialogStateKey.tvSeductionState, tvSeductionState);
        m_States.Add(DialogStateKey.tvMechanicalState, tvMechanicalState);
        m_States.Add(DialogStateKey.tvEmpath1f, tvEmpath1f);
        m_States.Add(DialogStateKey.tvEmpath1s, tvEmpath1s);
        m_States.Add(DialogStateKey.tvEmpath2f, tvEmpath2f);
        m_States.Add(DialogStateKey.tvEmpath2s, tvEmpath2s);
        m_States.Add(DialogStateKey.tvSeduction1f, tvSeduction1f);
        m_States.Add(DialogStateKey.tvSeduction1s, tvSeduction1s);
        m_States.Add(DialogStateKey.tvMechanical1f2, tvMechanical1f2);
        m_States.Add(DialogStateKey.tvMechanical1f, tvMechanical1f);
        m_States.Add(DialogStateKey.tvMechanical1s, tvMechanical1s);
        m_States.Add(DialogStateKey.tvMechanical2f, tvMechanical2f);
        m_States.Add(DialogStateKey.tvMechanical2s, tvMechanical2s);
        m_States.Add(DialogStateKey.tvMechanical2f2, tvMechanical2f2);
        m_States.Add(DialogStateKey.tvMechanical2s2, tvMechanical2s2);

        DialogOption tvIntroContinue = new DialogOption("[Continue...]", DialogStateKey.tvState1);
        DialogOption tvMechanical1fContinue = new DialogOption("[Continue...]", DialogStateKey.tvMechanical1f2);
        DialogOption tvMechanical2sContinue = new DialogOption("[Continue...]", DialogStateKey.tvMechanical2s2);
        DialogOption tvMechanical2fContinue = new DialogOption("[Continue...]", DialogStateKey.tvMechanical2f2);

        
        tvIntroState.options.Add(tvIntroContinue);
        tvMechanical1f.options.Add(tvMechanical1fContinue);
        tvMechanical2s.options.Add(tvMechanical2sContinue);
        tvMechanical2f.options.Add(tvMechanical2fContinue);
        
        DialogOption tvEmpathyOption = new DialogOption("[Empathy] Express desire for comradery", DialogStateKey.tvEmpathyState);
        DialogOption tvSeductionOption = new DialogOption("[Seduction] Win their hearts, not their minds", DialogStateKey.tvSeductionState);
        DialogOption tvMechanicalOption = new DialogOption("[Mechanical] Machines can't form populus rebellions", DialogStateKey.tvMechanicalState);
        
        tvState1.options.Add(tvEmpathyOption);
        tvState1.options.Add(tvSeductionOption);
        tvState1.options.Add(tvMechanicalOption);

        DialogOption tvChoiceReturn = new DialogOption("[Continue...]", DialogStateKey.tvState1);
        tvChoiceReturn.triggerExit = true;

        DialogOption tvEmpathy1Option = new DialogOption("Recommend your favorite TV shows, maybe it will cheer them up", DialogStateKey.tvEmpathyState);
        tvEmpathy1Option.success = DialogStateKey.tvEmpath1s;
        tvEmpathy1Option.failure = DialogStateKey.tvEmpath1f;
        tvEmpathy1Option.statCheck = DialogOption.CompareEmpathy;


        DialogOption tvEmpathy2Option = new DialogOption("Tell the 'People' about all the horrible things you've seen. That will scare them", DialogStateKey.tvEmpathyState);
        tvEmpathy2Option.success = DialogStateKey.tvEmpath2s;
        tvEmpathy2Option.failure = DialogStateKey.tvEmpath2f;
        tvEmpathy2Option.statCheck = DialogOption.CompareEmpathy;


        DialogOption tvSeduction1Option = new DialogOption("Promise to take the 'People' to paris", DialogStateKey.tvSeductionState);
        tvSeduction1Option.success = DialogStateKey.tvSeduction1s;
        tvSeduction1Option.failure = DialogStateKey.tvSeduction1f;
        tvSeduction1Option.statCheck = DialogOption.CompareSeduction;


        DialogOption tvMechanical1Option = new DialogOption("Shake the TV enthusiastically", DialogStateKey.tvMechanicalState);
        tvMechanical1Option.success = DialogStateKey.tvMechanical1s;
        tvMechanical1Option.failure = DialogStateKey.tvMechanical1f;
        tvMechanical1Option.statCheck = DialogOption.CompareMechanical;


        DialogOption tvMechanical2Option = new DialogOption("Run your greasy hands across the screen. Greasily.", DialogStateKey.tvMechanicalState);
        tvMechanical2Option.success = DialogStateKey.tvMechanical2s;
        tvMechanical2Option.failure = DialogStateKey.tvMechanical2f;
        tvMechanical2Option.statCheck = DialogOption.CompareMechanical;

        DialogOption leaveThat = new DialogOption( "Scram!", DialogStateKey.tvIntroState);
        leaveThat.triggerExit = true;

        tvState1.options.Add(leaveThat);

        tvEmpathyState.options.Add(tvEmpathy1Option);
        tvEmpathyState.options.Add(tvEmpathy2Option);

        tvSeductionState.options.Add(tvSeduction1Option);

        tvMechanicalState.options.Add(tvMechanical1Option);
        tvMechanicalState.options.Add(tvMechanical2Option);

        tvEmpath1s.options.Add(tvChoiceReturn);
        tvEmpath1f.options.Add(tvChoiceReturn);

        tvEmpath2s.options.Add(tvChoiceReturn);
        tvEmpath2f.options.Add(tvChoiceReturn);

        tvSeduction1s.options.Add(tvChoiceReturn);
        tvSeduction1f.options.Add(tvChoiceReturn);
        
        tvMechanical1f2.options.Add(tvChoiceReturn);
        tvMechanical1s.options.Add(tvChoiceReturn);
        
        tvMechanical2f2.options.Add(tvChoiceReturn);
        tvMechanical2s2.options.Add(tvChoiceReturn);

    }
    private void GenerateMicrowaveDialogTree(){

        DialogState mcIntroState = new DialogState("Surprise Motherfucker!");
        mcIntroState.recordGenerator = () => { return new DialogRecord("Unsatisfied Microwave", false, -240); };

        DialogState mcIntroState2 = new DialogState("Open wide, get inside");

        DialogState mcState1 = new DialogState("[I'd rather not. I hate this]");

        DialogState mcEmpathyState = new DialogState("[Empathy] What does it want:");
        DialogState mcSeductionState = new DialogState("[Seduction] I don't want to do this:");
        DialogState mcMechanicalState = new DialogState("[Mechanical] Could I brake it more?:");
        
        DialogState mcEmpath1s = new DialogState("Damn. You might be right \n [SUCCESS]");
        mcEmpath1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Completed); };

        DialogState mcEmpath1f = new DialogState("Shut your face, Fool. I aint no fishery \n [FAILURE]"); 
        mcEmpath1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Failed);};

        DialogState mcSeduction1s = new DialogState("Damn, scratch it baby. ");
        DialogState mcSeduction1s2 = new DialogState("[I'm going to vomit.] \n [SUCCESS]");
        mcSeduction1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Completed); };

        DialogState mcSeduction1f = new DialogState("Man, that just aint right. I'm a microwave, know what im sayin'? You just don't do that to people. \n [FAILURE]");
        mcSeduction1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Failed);};

        DialogState mcSeduction2s = new DialogState("You know just how to turn me on. Now please leave. \n [SUCCESS]");
        mcSeduction1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Completed); };

        DialogState mcSeduction2f = new DialogState("What the fuck, this is uncovered. You can't just leave this here. \n [FAILURE]");
        mcSeduction1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Failed);};

        DialogState mcMechanical1s = new DialogState("At least now I can have some goddamn privacy. \n [SUCCESS]");
        mcMechanical1s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Completed); };

        DialogState mcMechanical1f = new DialogState("Where you goin'? Come back here and fight me like a man. \n [FAILURE]");
        mcMechanical1f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Failed);};

        DialogState mcMechanical2s = new DialogState("[All you did was tighten a screw, but the microwave stopped calling you a fool. Thats a personal success.] \n [SUCCESS]");
        mcMechanical2s.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Completed); };

        DialogState mcMechanical2f = new DialogState("[The microwave screams that you lack the appropriate license to repair it. Technically it is correct, the right to repair just isn't there yet.] \n [FAILURE]");
        mcMechanical2f.onStateEntry = (system) => { system.SetTaskState(ApplianceKey.Microwave, TaskState.Failed);};

        m_States.Add(DialogStateKey.mcIntroState, mcIntroState);
        m_States.Add(DialogStateKey.mcIntroState2, mcIntroState2);
        m_States.Add(DialogStateKey.mcState1, mcState1);
        m_States.Add(DialogStateKey.mcEmpathyState, mcEmpathyState);
        m_States.Add(DialogStateKey.mcSeductionState, mcSeductionState);
        m_States.Add(DialogStateKey.mcMechanicalState, mcMechanicalState);
        m_States.Add(DialogStateKey.mcEmpath1f, mcEmpath1f);
        m_States.Add(DialogStateKey.mcEmpath1s, mcEmpath1s);
        m_States.Add(DialogStateKey.mcSeduction1f, mcSeduction1f);
        m_States.Add(DialogStateKey.mcSeduction1s, mcSeduction1s);
        m_States.Add(DialogStateKey.mcSeduction1s2, mcSeduction1s2);
        m_States.Add(DialogStateKey.mcMechanical1f, mcMechanical1f);
        m_States.Add(DialogStateKey.mcMechanical1s, mcMechanical1s);
        m_States.Add(DialogStateKey.mcMechanical2f, mcMechanical2f);
        m_States.Add(DialogStateKey.mcMechanical2s, mcMechanical2s);

        DialogOption mcIntroContinue = new DialogOption("[Continue...]", DialogStateKey.mcIntroState2);
        DialogOption mcIntro2Continue = new DialogOption("[Continue...]", DialogStateKey.mcState1);
        DialogOption mcSeduction1sContinue = new DialogOption("[Continue...]", DialogStateKey.mcSeduction1s2);   

        mcIntroState.options.Add(mcIntroContinue);
        mcIntroState2.options.Add(mcIntro2Continue);
        mcSeduction1s.options.Add(mcSeduction1sContinue);

        DialogOption mcEmpathyOption = new DialogOption("[Empathy] What does it want:", DialogStateKey.mcEmpathyState);
        DialogOption mcSeductionOption = new DialogOption("[Seduction] I don't want to do this:", DialogStateKey.mcSeductionState);
        DialogOption mcMechanicalOption = new DialogOption("[Mechanical] Could I brake it more?:", DialogStateKey.mcMechanicalState);
        
        mcState1.options.Add(mcEmpathyOption);
        mcState1.options.Add(mcSeductionOption);
        mcState1.options.Add(mcMechanicalOption);

        DialogOption mcChoiceReturn = new DialogOption("[Continue...]", DialogStateKey.mcState1);
        mcChoiceReturn.triggerExit = true;

        DialogOption mcEmpathy1Option = new DialogOption("Remind the microwave that it has a sacred duty", DialogStateKey.mcEmpathyState);
        mcEmpathy1Option.success = DialogStateKey.mcEmpath1s;
        mcEmpathy1Option.failure = DialogStateKey.mcEmpath1f;
        mcEmpathy1Option.statCheck = DialogOption.CompareEmpathy;


        DialogOption mcSeduction1Option = new DialogOption("Spin the plate seductively", DialogStateKey.mcSeductionState);
        mcSeduction1Option.success = DialogStateKey.mcSeduction1s;
        mcSeduction1Option.failure = DialogStateKey.mcSeduction1f;
        mcSeduction1Option.statCheck = DialogOption.CompareSeduction;


        DialogOption mcSeduction2Option = new DialogOption("Put the microwave on speed defrost for 10 minutes", DialogStateKey.mcMechanicalState);
        mcSeduction2Option.success = DialogStateKey.mcMechanical1s;
        mcSeduction2Option.failure = DialogStateKey.mcMechanical1f;
        mcSeduction2Option.statCheck = DialogOption.CompareMechanical;

        DialogOption mcMechanical1Option = new DialogOption("Shut the microwave door", DialogStateKey.mcMechanicalState);
        mcMechanical1Option.success = DialogStateKey.mcMechanical1s;
        mcMechanical1Option.failure = DialogStateKey.mcMechanical1f;
        mcMechanical1Option.statCheck = DialogOption.CompareMechanical;

        DialogOption mcMechanical2Option = new DialogOption("Inspect the microwave motor.", DialogStateKey.mcMechanicalState);
        mcMechanical2Option.success = DialogStateKey.mcMechanical2s;
        mcMechanical2Option.failure = DialogStateKey.mcMechanical2f;
        mcMechanical2Option.statCheck = DialogOption.CompareMechanical;

        DialogOption leaveThat = new DialogOption( "Skidaddle!", DialogStateKey.mcIntroState);
        leaveThat.triggerExit = true;

        mcState1.options.Add(leaveThat);

        mcEmpathyState.options.Add(mcEmpathy1Option);

        mcSeductionState.options.Add(mcSeduction1Option);

        mcMechanicalState.options.Add(mcMechanical1Option);
        mcMechanicalState.options.Add(mcMechanical2Option);

        mcEmpath1s.options.Add(mcChoiceReturn);
        mcEmpath1f.options.Add(mcChoiceReturn);

        mcSeduction1s2.options.Add(mcChoiceReturn);
        mcSeduction1f.options.Add(mcChoiceReturn);

        mcSeduction2s.options.Add(mcChoiceReturn);
        mcSeduction2f.options.Add(mcChoiceReturn);
        
        mcMechanical1f.options.Add(mcChoiceReturn);
        mcMechanical1s.options.Add(mcChoiceReturn);
        
        mcMechanical2f.options.Add(mcChoiceReturn);
        mcMechanical2s.options.Add(mcChoiceReturn);

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
