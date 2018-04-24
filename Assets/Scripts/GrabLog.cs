using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GrabLog : MonoBehaviour
{

    public string JournalEndPath;
    private string JournalDump;
    private string SystemDump;
    public string directory;
    public string CurrentUser;
    private DateTime ExpiryTime;
    private string Countdown;
    private Vector3 PlayerLocation;
    private string[] location = new string[3];


    //Highest reward mission parameters
    private string MaxCreditsID;
    private string MaxCreditsTargetType;
    private int MaxCreditsKillCount;
    private string MaxCreditsTargetFaction;
    private string MaxCreditsDestination;
    private int MaxCreditsReward;
    private DateTime MaxCreditsExpiry;
    private string MaxCreditsCountDown;
    private float MaxCreditsDistance;

    //Nearest to time elapsing mission parameters
    private string MinTimeID;
    private string MinTimeTargetType;
    private int MinTimeKillCount;
    private string MinTimeTargetFaction;
    private string MinTimeDestination;
    private int MinTimeReward;
    private DateTime MinTimeExpiry;
    private string MinTimeCountDown;
    private float MinTimeDistance;


    //Closest mission parameters
    private string MinDistanceID;
    private string MinDistanceTargetType;
    private int MinDistanceKillCount;
    private string MinDistanceTargetFaction;
    private string MinDistanceDestination;
    private int MinDistanceReward;
    private DateTime MinDistanceExpiry;
    private string MinDistanceCountDown;
    private float MinDistanceDistance;

    public List<DataDump> JournalContents = new List<DataDump>();
    public List<MissionAdd> ActiveMissionList = new List<MissionAdd>();
    public List<MissionEnd> EndedMissionList = new List<MissionEnd>();
    public List<Systems> EDDBData = new List<Systems>();
    public List<KillList> killlist = new List<KillList>();
    public List<PlayerInfo> playerinfo = new List<PlayerInfo>();

    public Transform distance;

    private void Start()
    {
        GetEDDB();
        GetFile();
        InvokeRepeating("UpdateLists", 0, 1);
    }

    void UpdateLists()
    {
        if (ActiveMissionList.Count == 0)
        {
            Debug.Log("No missions!");
        }
        else
        {
            MissionDetails();
            UpdateCreditMission();
            UpdateTimeMission();
            UpdateDistanceMission();
        }
    }

    // Gett EDDB info (only run once during startup)

    void GetEDDB()
    {
        Debug.Log("Starting system data load");
        StartCoroutine(GetText());
    }
    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://eddb.io/archive/v5/systems_populated.jsonl");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            SystemDump = www.downloadHandler.text;
            string[] systemdata = SystemDump.Split("\n"[0]);
            foreach (string line in systemdata)
            {
                try
                {
                    Systems eddb = JsonConvert.DeserializeObject<Systems>(line);
                    EDDBData.Add(new Systems { name = eddb.name, x = eddb.x, y = eddb.y, z = eddb.z });
                }
                catch (Exception)
                {
                }
            }

        }
        MissionCleanse();
        Debug.Log("System Data loaded");
    }

    void Update()
    {

        if (GetComponent<JournalWatcher>().runGrabLog == true)
        {
            JournalUpdate();
            GetComponent<JournalWatcher>().runGrabLog = false;
        }
        if (Input.GetKeyDown("f"))
        {
            JournalUpdate();
        }

    }

    //One off Read journal files and populate lists

    public void GetFile()
    {
        // Generate file information
        CurrentUser = System.Environment.UserName.ToString();
        directory = "C:/Users/" + CurrentUser + "/Saved Games/Frontier Developments/Elite Dangerous";

        DirectoryInfo dir = new DirectoryInfo(directory);
        FileInfo[] info = dir.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();

        JournalEndPath = info[0].ToString();

        // Read file lines using filestream to avoid access issues from readalllines

        FileStream fs = new FileStream(JournalEndPath, FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
        using (StreamReader read = new StreamReader(fs, true))
        {
            JournalDump = read.ReadToEnd();
        }
        string[] JournalData = JournalDump.Split("\n"[0]);

        foreach (string line in JournalData)
        {

            //Handle extra blank line at end of file
            if (line.Length == 0)
            { }
            else
            {
                // Json Deserialise
                DataDump datadump = JsonConvert.DeserializeObject<DataDump>(line);
                // Populate accepted missions

                if (datadump.@event == "FSDJump")
                {
                    PlayerLocation = new Vector3(float.Parse(datadump.StarPos[0]), float.Parse(datadump.StarPos[1]), float.Parse(datadump.StarPos[2]));
                }

                if (datadump.@event == "Bounty")
                {
                    killlist.Add(new KillList { KillTime = datadump.timestamp, Faction = datadump.VictimFaction });
                }

                if (datadump.@event == "MissionAccepted")
                {
                    if (datadump.name.StartsWith("Mission_Altr"))
                    {
                        ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID, LocalisedName = datadump.LocalisedName });
                    }
                    else
                    {
                        ActiveMissionList.Add(new MissionAdd
                        {
                            AcceptedTime = datadump.timestamp,
                            MissionID = datadump.MissionID,
                            LocalisedName = datadump.LocalisedName,
                            reward = datadump.Reward,
                            KillCount = datadump.KillCount,
                            TargetType_Localised = datadump.TargetType_Localised,
                            TargetFaction = datadump.TargetFaction,
                            DestinationSystem = datadump.DestinationSystem,
                            Expiry = datadump.Expiry
                        });
                    }
                }
                // Populate ended missions list
                if (datadump.@event == "MissionCompleted" || datadump.@event == "MissionAbandoned" || datadump.@event == "MissionFailed")
                {
                    EndedMissionList.Add(new MissionEnd { MissionID = datadump.MissionID });
                }
            }

        }
        MissionCleanse();
        KillCountUpdate();
        
    }

    public void JournalUpdate()
    {
        Debug.Log(ActiveMissionList.Count);
        // Calculate file information
        CurrentUser = System.Environment.UserName.ToString();
        directory = "C:/Users/" + CurrentUser + "/Saved Games/Frontier Developments/Elite Dangerous";

        DirectoryInfo dir = new DirectoryInfo(directory);
        FileInfo[] info = dir.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();

        JournalEndPath = info[0].ToString();

        // Read file lines using filestream to avoid access issues from readalllines

        FileStream fs = new FileStream(JournalEndPath, FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
        using (StreamReader read = new StreamReader(fs, true))
        {
            SystemDump = read.ReadToEnd();
        }
        string[] SystemData = SystemDump.Split("\n"[0]);

        JournalDump = SystemData[SystemData.Length - 2];

        //Handle extra blank line at end of file
        if (JournalDump.Length == 0)
        { }
        else
        {
            // Json Deserialise
            DataDump datadump = JsonConvert.DeserializeObject<DataDump>(JournalDump);

            if (datadump.@event == "Bounty")
            {
                killlist.Add(new KillList { KillTime = datadump.timestamp, Faction = datadump.VictimFaction });
                foreach (var item in ActiveMissionList)
                {
                    if (item.TargetFaction == datadump.VictimFaction && item.AcceptedTime < datadump.timestamp)
                        {
                            item.TotalKills++;
                            Debug.Log(item.TargetFaction + item.TotalKills);
                        }
                    }
                }

            // Populate accepted missions
            if (datadump.@event == "MissionAccepted")
            {
                if (datadump.name.StartsWith("Mission_Altr"))
                {
                    ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID, LocalisedName = datadump.LocalisedName });
                }
                else
                {
                    ActiveMissionList.Add(new MissionAdd
                    {
                        MissionID = datadump.MissionID,
                        LocalisedName = datadump.LocalisedName,
                        reward = datadump.Reward,
                        KillCount = datadump.KillCount,
                        TargetType_Localised = datadump.TargetType_Localised,
                        TargetFaction = datadump.TargetFaction,
                        DestinationSystem = datadump.DestinationSystem,
                        Expiry = datadump.Expiry
                    });
                }
            }
            // Populate ended missions list
            if (datadump.@event == "MissionCompleted" || datadump.@event == "MissionAbandoned" || datadump.@event == "MissionFailed")
            {
                EndedMissionList.Add(new MissionEnd { MissionID = datadump.MissionID });
            }
        }
        MissionCleanse();
    }

    void KillCountUpdate()
    {
        foreach (var item in ActiveMissionList)
        {
            foreach (var kills in killlist)
            {
                if(item.TargetFaction == kills.Faction && item.AcceptedTime < kills.KillTime)
                {
                    item.TotalKills++;
                    Debug.Log(item.TargetFaction + item.TotalKills);
                }
            }
        }
    }

    void MissionCleanse()
        {
        // Mission List script

        foreach (var item in EndedMissionList)
        {
            //Compare mission ID in active missions to ID's in Ended missions
            MissionAdd matcheditem = ActiveMissionList.Find(x => x.MissionID.Contains(item.MissionID));
            try
            {
                //Remove any completed missions from active mission list
                ActiveMissionList.Remove(matcheditem);
            }
            catch (Exception)
            {
                Debug.Log(item.MissionID + "Mission Not yet complete");
            }
        }

        foreach (var sys in ActiveMissionList)
        {
            Debug.Log(sys.DestinationSystem);
            try
            {
                Systems location = EDDBData.Find(x => x.name.Contains(sys.DestinationSystem));
                sys.Location = new Vector3(float.Parse(location.x), float.Parse(location.y) , float.Parse(location.z));
                float dist = Vector3.Distance(sys.Location, PlayerLocation);
                sys.distance = dist;
                Debug.Log(PlayerLocation);
                Debug.Log(sys.Location);
                Debug.Log(sys.distance);
            }
            catch(Exception)
            {

            }
        }
    }
    
    void MissionDetails()
    {
        //Sort list by reward
        ActiveMissionList = ActiveMissionList.OrderByDescending(x => x.reward).ToList();
        MaxCreditsID = ActiveMissionList[0].MissionID;
        MaxCreditsTargetType = ActiveMissionList[0].TargetType_Localised;
        MaxCreditsKillCount = ActiveMissionList[0].KillCount;
        MaxCreditsTargetFaction = ActiveMissionList[0].TargetFaction;
        MaxCreditsDestination = ActiveMissionList[0].DestinationSystem;
        MaxCreditsReward = ActiveMissionList[0].reward;
        MaxCreditsExpiry = ActiveMissionList[0].Expiry;
        MaxCreditsCountDown = ActiveMissionList[0].Countdown;
        MaxCreditsDistance = ActiveMissionList[0].distance;
        

    ActiveMissionList = ActiveMissionList.OrderByDescending(x => x.Expiry).ToList();
        MinTimeID = ActiveMissionList[0].MissionID;
        MinTimeTargetType = ActiveMissionList[0].TargetType_Localised;
        MinTimeKillCount = ActiveMissionList[0].KillCount;
        MinTimeTargetFaction = ActiveMissionList[0].TargetFaction;
        MinTimeDestination = ActiveMissionList[0].DestinationSystem;
        MinTimeReward = ActiveMissionList[0].reward;
        MinTimeExpiry = ActiveMissionList[0].Expiry;
        MinTimeCountDown = ActiveMissionList[0].Countdown;
        MinTimeDistance = ActiveMissionList[0].distance;

        ActiveMissionList = ActiveMissionList.OrderBy(x => x.distance).ToList();
        MinDistanceID = ActiveMissionList[0].MissionID;
        MinDistanceTargetType = ActiveMissionList[0].TargetType_Localised;
        MinDistanceKillCount = ActiveMissionList[0].KillCount;
        MinDistanceTargetFaction = ActiveMissionList[0].TargetFaction;
        MinDistanceDestination = ActiveMissionList[0].DestinationSystem;
        MinDistanceReward = ActiveMissionList[0].reward;
        MinDistanceExpiry = ActiveMissionList[0].Expiry;
        MinDistanceCountDown = ActiveMissionList[0].Countdown;
        MinDistanceDistance = ActiveMissionList[0].distance;
    }


        void UpdateCreditMission()
        {
        GameObject MissionDetails;
        MissionDetails = GameObject.Find("CreditMissionDetails");
        TimeSpan countdown = MaxCreditsExpiry - DateTime.Now;
        MaxCreditsCountDown = (countdown.Hours.ToString() + " - " + countdown.Minutes.ToString() + " - " + countdown.Seconds.ToString());
        MissionDetails.GetComponent<Text>().text = "Kill " + MaxCreditsKillCount + " " + MaxCreditsTargetType +
            "\nTarget faction: " + MaxCreditsTargetFaction +
            "\nTarget system: " + MaxCreditsDestination +
            "\nPayout: " + MaxCreditsReward.ToString("n0") + " Credits" +
            "\nExpiry time: " + MaxCreditsExpiry.ToString("dd/MM/yyyy hh/mm/ss tt") +
            "\nRemaining time: " + MaxCreditsCountDown +
        "\nDistance to target " + MaxCreditsDistance + " LYR";

    }

    void UpdateTimeMission()
    {
        GameObject MissionDetails;
        MissionDetails = GameObject.Find("TimeMissionDetails");
        TimeSpan countdown = MinTimeExpiry - DateTime.Now;
        MinTimeCountDown = (countdown.Hours.ToString() + " - " + countdown.Minutes.ToString() + " - " + countdown.Seconds.ToString());
        MissionDetails.GetComponent<Text>().text = "Kill " + MinTimeKillCount + " " + MinTimeTargetType +
            "\nTarget faction: " + MinTimeTargetFaction +
            "\nTarget system: " + MinTimeDestination +
            "\nPayout: " + MinTimeReward.ToString("n0") + " Credits" +
            "\nExpiry time: " + MinTimeExpiry.ToString("dd/MM/yyyy hh/mm/ss tt") +
            "\nRemaining time: " + MinTimeCountDown +
            "\nDistance to target " + MinTimeDistance + " LYR";
    }

    void UpdateDistanceMission()
    {
        GameObject MissionDetails;
        MissionDetails = GameObject.Find("DistanceMissionDetails");
        TimeSpan countdown = MinDistanceExpiry - DateTime.Now;
        MinDistanceCountDown = (countdown.Hours.ToString() + " - " + countdown.Minutes.ToString() + " - " + countdown.Seconds.ToString());
        MissionDetails.GetComponent<Text>().text = "Kill " + MinDistanceKillCount + " " + MinDistanceTargetType +
            "\nTarget faction: " + MinDistanceTargetFaction +
            "\nTarget system: " + MinDistanceDestination +
            "\nPayout: " + MinDistanceReward.ToString("n0") + " Credits" +
            "\nExpiry time: " + MinDistanceExpiry.ToString("dd/MM/yyyy hh/mm/ss tt") +
            "\nRemaining time: " + MinDistanceCountDown +
            "\nDistance to target " + MinDistanceDistance + " LYR";
    }


   

}