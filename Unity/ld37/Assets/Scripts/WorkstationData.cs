using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkstationData : MonoBehaviour
{
    public enum WorkstationType
    {
        None,
        water_1,
        water_2,
        water_3,
        coffee_1,
        coffee_2,
        coffee_3,
        tea_1,
        tea_2,
        tea_3,
        bakery_1,
        bakery_2,
        bakery_3,
        register
    }

    public WorkstationType StationType;
}