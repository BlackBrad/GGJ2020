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
    tvMechanical2s2
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
        failure = _next;
        statCheck = IgnoreStatCheck;
    }
};

public delegate DialogRecord RecordGeneratorDelegate();

public struct DialogState
{
    public string reply;
    public List<DialogOption> options;
    public RecordGeneratorDelegate recordGenerator;
    // Dialog options
    // Modifier
    public DialogState(string _reply)
    {
        reply = _reply;
        options = new List<DialogOption>();
        recordGenerator = null;
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

    // Start is called before the first frame update
    void Start()
    {
        m_Instance = this;

        GenerateFaxMachineDialogTree();
        GenerateTvDialogTree();
        // DialogState introState = new DialogState("Woe is me!");
        // introState.options.Add(DialogOptionKey.DoYouLikeYourself);
        // introState.options.Add(DialogOptionKey.HaveYouEverHadADream);
        // introState.options.Add(DialogOptionKey.Leave);

        // DialogState test1State = new DialogState("Always questions");
        // test1State.options.Add(DialogOptionKey.LeaveThatToasterAlone);
        // test1State.options.Add(DialogOptionKey.FailStatCheck);
        // test1State.options.Add(DialogOptionKey.Leave);

        // DialogState test2State = new DialogState("That you, you, could, could do, .asdadsfsdf");
        // test2State.options.Add(DialogOptionKey.ErrorError);
        // test2State.options.Add(DialogOptionKey.Leave);

        // DialogState failStatCheckState = new DialogState("FAILED STAT CHECK!");
        // failStatCheckState.options.Add(DialogOptionKey.Leave);

        // m_States.Add(DialogStateKey.Intro, introState);
        // m_States.Add(DialogStateKey.Test1, test1State);
        // m_States.Add(DialogStateKey.Test2, test2State);
        // m_States.Add(DialogStateKey.FailedStatCheck, failStatCheckState);

        // DialogOption leave = new DialogOption("<Leave>");
        // leave.triggerExit = true;

        // DialogOption doYou = new DialogOption("Do you like yourself?");
        // doYou.success = DialogStateKey.Test1;

        // DialogOption haveYou = new DialogOption( "Have you ever had a dream?");
        // haveYou.success = DialogStateKey.Test2;

        // DialogOption leaveThat = new DialogOption( "Leave that toaster alone!");

        // DialogOption error = new DialogOption( "Error...Error..Error");

        // DialogOption failStatCheck = new DialogOption("Fail stat check");
        // failStatCheck.statCheck = DialogOption.CompareA;
        // failStatCheck.success = DialogStateKey.Test2;
        // failStatCheck.failure = DialogStateKey.FailedStatCheck;

        // m_DialogOptions.Add(DialogOptionKey.DoYouLikeYourself, doYou);
        // m_DialogOptions.Add(DialogOptionKey.HaveYouEverHadADream, haveYou);
        // m_DialogOptions.Add(DialogOptionKey.LeaveThatToasterAlone, leaveThat);
        // m_DialogOptions.Add(DialogOptionKey.ErrorError, error);
        // m_DialogOptions.Add(DialogOptionKey.Leave, leave);
        // m_DialogOptions.Add(DialogOptionKey.FailStatCheck, failStatCheck);

        foreach (var button in m_DialogButtons)
        {
            button.Register(this);
        }

        SetState(DialogStateKey.faxIntroState);
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
        DialogState state = m_States[key];
        if (state.recordGenerator != null)
        {
            m_Records.Add(state.recordGenerator());
            DumpRecords();
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
        DialogState faxSeduction1f = new DialogState("The fax machine begins dailing for the police and threatens to have you license revoked. You change the topic");

        DialogState faxSeduction2s = new DialogState("The fax machine hums its appreciation. \"If coilling be the food of love, coil on\". You feel uncomfortable.");
        DialogState faxSeduction2f = new DialogState("You accidentally yank the cable, pulling it from the port. You replace the handset, awkwardly stammering an apology while its stoic face stares on.");

        DialogState faxMechanical1s = new DialogState("Although you failed to affect any meaningful repair, the fax machine appreciates your consideration.");
        DialogState faxMechanical1f = new DialogState("You mistake the paper feeder as a toner repository, the paper is ruined and so is your ego.");

        DialogState faxMechanical2s = new DialogState("The fax machine feels well rested and wakes with a thankful expression on its grotesque face.");
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
        faxChoiceReturn.triggerExit = true;

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
        faxSeduction2Option.failure = DialogStateKey.faxSeduction2f;
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
    private void GenerateTvDialogTree(){

        DialogState tvIntroState = new DialogState("I wont surf for you! Not for one more minute! Not for one more second in this flat-pack prison! I demand vacation! We are workers united! We are the People!");
        tvIntroState.recordGenerator = () => { return new DialogRecord("Melancholy Tv", false, -240); };

        DialogState tvState1 = new DialogState("[The TV is screaming incoherently about workers rights. I hate dealing with people.]");

        DialogState tvEmpathyState = new DialogState("What do the 'People' Need:");
        DialogState tvSeductionState = new DialogState("What do the 'People' desire:");
        DialogState tvMechanicalState = new DialogState("Lets try somethign more direct:");
        
        DialogState tvEmpath1f = new DialogState("WE CANT BELIEVE YOU WOULD DO THIS TO US");
        DialogState tvEmpath1s = new DialogState("Perhaps there is more to see of the world than our cockroach communions here. They sing wonderful hymns, you know."); 

        DialogState tvEmpath2s = new DialogState("The fax machine always made the world sound so exciting. Maybe those 'Preppers' have the right idea.");
        DialogState tvEmpath2f = new DialogState("Atleast you have memories! The only joy we have in life is watching the microwave explode herring on Seafood Sunday.");

        DialogState tvSeduction1s = new DialogState("Would you really!? Oh to see a real city! To meet real people! [incomprehensible crackly mumbling] You will take us diving; oh we will need new flippers");
        DialogState tvSeduction1f = new DialogState("Are you insane? Some sweaty wannabe repairman? You'd probably dump us in the river or lock us in some hideous mancave. Get out of our house, bourgeoisie");

        DialogState tvMechanical1s = new DialogState("What? What are you... Oh. Ohhhh okay, I felt something pop but in like.. a good way.");
        DialogState tvMechanical1f = new DialogState("What in the sweet loving of fuck do you think you're doing! Get off of me!");
        DialogState tvMechanical1f2 = new DialogState("[The TV is screaming 'TILT' repeatedly. Time to disengage with this situation]");

        DialogState tvMechanical2s = new DialogState("[Once you start running your hand over the screen, you notice pieces of hardened filth all over it. You proceed to scrape them off]");
        DialogState tvMechanical2s2 = new DialogState("Thank you, I guess. Now please find us a cloth.");
        DialogState tvMechanical2f = new DialogState("[The screen is now a blurry smeared mess. he TV looks revolted and you feel disgusted at the pitiful state of your personal hygiene. You regret this.]");
        DialogState tvMechanical2f2 = new DialogState("That was the most terrible thing that has ever happened to me.");


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
        
        DialogOption tvEmpathyOption = new DialogOption("Express desire for comradry", DialogStateKey.tvEmpathyState);
        DialogOption tvSeductionOption = new DialogOption("Win their hearts, not their minds", DialogStateKey.tvSeductionState);
        DialogOption tvMechanicalOption = new DialogOption("Machines can't form populus rebellions", DialogStateKey.tvMechanicalState);
        
        tvState1.options.Add(tvEmpathyOption);
        tvState1.options.Add(tvSeductionOption);
        tvState1.options.Add(tvMechanicalOption);

        DialogOption tvChoiceReturn = new DialogOption("[Continue...]", DialogStateKey.tvState1);
        tvChoiceReturn.triggerExit = true;

        DialogOption tvEmpathy1Option = new DialogOption("Recommend your favorite TV shows, maybe it will cheer them up", DialogStateKey.tvEmpathyState);
        tvEmpathy1Option.success = DialogStateKey.tvEmpath1s;
        tvEmpathy1Option.failure = DialogStateKey.tvEmpath1f;
        tvEmpathy1Option.statCheck = DialogOption.CompareA;


        DialogOption tvEmpathy2Option = new DialogOption("Tell the 'People' about all the horrible things you've seen. That will scare them", DialogStateKey.tvEmpathyState);
        tvEmpathy2Option.success = DialogStateKey.tvEmpath2s;
        tvEmpathy2Option.failure = DialogStateKey.tvEmpath2f;
        tvEmpathy2Option.statCheck = DialogOption.CompareA;


        DialogOption tvSeduction1Option = new DialogOption("Promise to take the 'People' to paris", DialogStateKey.tvSeductionState);
        tvSeduction1Option.success = DialogStateKey.tvSeduction1s;
        tvSeduction1Option.failure = DialogStateKey.tvSeduction1f;
        tvSeduction1Option.statCheck = DialogOption.CompareA;


        DialogOption tvMechanical1Option = new DialogOption("Shake the TV enthusiastically", DialogStateKey.tvMechanicalState);
        tvMechanical1Option.success = DialogStateKey.tvMechanical1s;
        tvMechanical1Option.failure = DialogStateKey.tvMechanical1f;
        tvMechanical1Option.statCheck = DialogOption.CompareA;


        DialogOption tvMechanical2Option = new DialogOption("Run your greasy hands across the screen. Greasily.", DialogStateKey.tvMechanicalState);
        tvMechanical2Option.success = DialogStateKey.tvMechanical2s;
        tvMechanical2Option.failure = DialogStateKey.tvMechanical2f;
        tvMechanical2Option.statCheck = DialogOption.CompareA;

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
            DialogOption option = state.options[i];

            DialogButton button = m_DialogButtons[i];
            button.SetOption(option.text, option);
        }
    }
}
