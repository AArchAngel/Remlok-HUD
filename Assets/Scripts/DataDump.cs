using System;
using UnityEngine;
[System.Serializable]
public class DataDump
{
    public DateTime timestamp { get; set; }
    public string @event { get; set; }
    public string Commander { get; set; }
    public string LocalisedName { get; set; }
    public string TargetType_Localised { get; set; }
    public string TargetFaction { get; set; }
    public int KillCount { get; set; }
    public string DestinationSystem { get; set; }
    public int Reward { get; set; }
    public string MissionID { get; set; }
    public string name { get; set; }
    public DateTime Expiry { get; set; }
    public string VictimFaction { get; set; }
    public string[] StarPos { get; set; }
    //public string Name;
}
public class MissionAdd
{
    public DateTime AcceptedTime;
    public string MissionID;
    public bool MissionActive = true;
    public int reward;
    public string LocalisedName;
    public string TargetType_Localised;
    public string TargetFaction;
    public int KillCount;
    public string DestinationSystem;
    public DateTime Expiry;
    public string Countdown;
    public float distance;
    public Vector3 Location;
    public string x;
    public string y;
    public string z;
    public int TotalKills = 0;
    public string type;
    public bool active = false;

}
public class MissionEnd
{
    public string MissionID;
}

public class PlayerInfo
{
    public string Location;
    public string X;
    public string Y;
    public string Z;
    public Vector3 PlayerLocation;
    public string PlayerLocation1;
    public string Name;
    public string Credits;
}

public class Systems
{
    public string name;
    public string x;
    public string y;
    public string z;
}

public class KillList
{
    public string Faction;
    public DateTime KillTime;
}


