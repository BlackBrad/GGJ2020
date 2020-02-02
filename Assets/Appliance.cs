using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Appliance : MonoBehaviour
{
    public DialogStateKey m_StartingState;
    public DialogStats m_Stats;
    public ApplianceKey m_Key;

    public int empathy;
    public int seduction;
    public int mechanical;

    void Start()
    {
        m_Stats.empathy = empathy;
        m_Stats.seduction = seduction;
        m_Stats.mechanical = mechanical;
    }
}
