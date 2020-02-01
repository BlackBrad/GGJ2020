using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Appliance : MonoBehaviour
{
    public DialogStateKey m_StartingState;
    public DialogStats m_Stats;

    public int A;
    public int B;
    public int C;

    void Start()
    {
        m_Stats.A = A;
        m_Stats.B = B;
        m_Stats.C = C;
    }
}
