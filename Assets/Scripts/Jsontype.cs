using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jsontype
{
    public float betAmount;
    public string token;
    public string userName;
    public float totalAmount;
    public float earnAmount;
    public int sideFlag;
}

public class ReceiveJsonObject
{
    public bool isStarted;
    public bool gameResult;
    public float odd;
    public float earnAmount;
    public int level;
    public float amount;
    public int sideFlag;
    public string errMessage;
    public ReceiveJsonObject()
    {
    }
    public static ReceiveJsonObject CreateFromJSON(string data)
    {
        return JsonUtility.FromJson<ReceiveJsonObject>(data);
    }
}
