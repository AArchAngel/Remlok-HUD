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
    private string MissionType;

    private int ActiveMissionCount;

    private int FileNumber = 0;
    private int JournalStartMissions = 0;
    private int FoundMissions = 0;
    private int JournalLine = 0;

    private int LastLineNumber = 0;



    public List<DataDump> JournalContents = new List<DataDump>();
    public List<MissionAdd> ActiveMissionList = new List<MissionAdd>();
    public List<MissionEnd> EndedMissionList = new List<MissionEnd>();
    public List<Systems> EDDBData = new List<Systems>();
    public List<KillList> killlist = new List<KillList>();
    public List<PlayerInfo> playerinfo = new List<PlayerInfo>();

    public Transform distance;

    public Sprite Active;
    public Sprite Mission;
    public Sprite Blank;
    public Sprite ActiveMission;

    private void Start()
    {
        GetEDDB();
     //   GetFile();
        InvokeRepeating("UpdateLists", 0, 1);
    }

    void UpdateLists()
    {
        Debug.Log("active mission lists on update = " + ActiveMissionList.Count);
        if (ActiveMissionList.Count == 0)
        {
            Debug.Log("No missions!");
        }
        else
        {
         UpdateMissionList();

        }
    }

    // Gett EDDB info (only run once during startup)

    void GetEDDB()
    {
  
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
            GetFile();
            GetComponent<JournalWatcher>().runGrabLog = false;
        }
        if (Input.GetKeyDown("f"))
        {
            GetFile();
        }
        if (Input.GetKeyDown("1"))
        {
            Reward();
        }
        if (Input.GetKeyDown("2"))
        {
            Distance();
        }
        if (Input.GetKeyDown("3"))
        {
            Time();
        }
        if (Input.GetKeyDown("4"))
        {
            if (ActiveMissionCount == 0)
            {
                Debug.Log("No missions!");
            }
            else {
                foreach (var item in ActiveMissionList)
                {
                    item.active = false;
                }
                ActiveMissionList[0].active = true;
                UpdateMissionList();
            }
        }
        if (Input.GetKeyDown("5"))
        {
            if (ActiveMissionCount < 2)
            {
                Debug.Log("No missions!");
            }
            else
            {
                foreach (var item in ActiveMissionList)
                {
                    item.active = false;
                }
                ActiveMissionList[1].active = true;
                UpdateMissionList();
            }
        }
        if (Input.GetKeyDown("6"))
        {
            if (ActiveMissionCount < 3)
            {
                Debug.Log("No missions!");
            }
            else
            {
                foreach (var item in ActiveMissionList)
                {
                    item.active = false;
                }
                ActiveMissionList[2].active = true;
                UpdateMissionList();
            }
        }

    }

    //One off Read journal files and populate lists


    public void GetFile()
    {
        Debug.Log("Running Update JournalLine status is " + JournalLine);
        // Generate file information
        CurrentUser = System.Environment.UserName.ToString();
        directory = "C:/Users/" + CurrentUser + "/Saved Games/Frontier Developments/Elite Dangerous";

        DirectoryInfo dir = new DirectoryInfo(directory);
        FileInfo[] info = dir.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();

        JournalEndPath = info[FileNumber].ToString();



        // Read file lines using filestream to avoid access issues from readalllines

        FileStream fs = new FileStream(JournalEndPath, FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
        using (StreamReader read = new StreamReader(fs, true))
        {

            JournalDump = read.ReadToEnd();
        }



        string[] JournalData = JournalDump.Split("\n"[0]);
    //    Debug.Log(JournalData.Length + "JournalData items" + LastLineNumber + "Last line number");
        for (int j = LastLineNumber; j < JournalData.Length; j++)
        {

            //Handle extra blank line at end of file
            if (JournalData[j].Length == 0)
            { }
            else
            {

                // Json Deserialise
                DataDump datadump = JsonConvert.DeserializeObject<DataDump>(JournalData[j]);

                //Start up 0 = no file read


                if (JournalLine > 0)
                {
                 
                    //timestamp check
                    if (j > LastLineNumber || FileNumber > 0)
                    {
                     //   Debug.Log("Current line number is " + j + " Stored last line is " + LastLineNumber);
                        // Game load = 1 (when Commander event found)
                        if (JournalLine == 1 && FileNumber == 0)
                        {
                            if (datadump.@event == "Missions")
                            {
                                //Set to 2 when Missions event found
                                Debug.Log("Missions event found");
                                Debug.Log("Missions Count =" + datadump.Active.Length);


                                try
                                {
                                    if (JournalStartMissions == 0)
                                    {
                                        for (int i = 0; i < datadump.Active.Length; i++)
                                        {


                                             if (datadump.Active[i].Expires > 0)
                                             {
                                            JournalStartMissions = JournalStartMissions + 1;
                                            ActiveMissionList.Add(new MissionAdd { MissionID = datadump.Active[i].MissionID.ToString() });
                                             }


                                        }

                                    }
                                }

                                catch (Exception)
                                {

                                }

                            }
                        }

                        // Populate accepted missions

                        try
                        {

                            if (datadump.name.StartsWith("Mission_Massacre"))

                            {
                                MissionType = "Kill";
                            }
                            if (datadump.name.StartsWith("Mission_Delivery") || datadump.name.StartsWith("Mission_Collect"))

                            {
                                MissionType = "Deliver";
                            }
                            if (datadump.name.StartsWith("Mission_Assass"))
                            {
                                MissionType = "Assassinate";
                            }
                            if (datadump.name.StartsWith("Mission_Courier"))
                            {
                                MissionType = "Deliver";
                            }
                            if (datadump.name.StartsWith("Mission_Collect"))
                            {
                                MissionType = "Source";
                            }
                            if (datadump.name.StartsWith("Mission_Disable"))
                            {
                                MissionType = "Take out";
                            }

                        }
                        catch (Exception)
                        {

                        }

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
                            if (JournalLine == 2)
                            {
                        
                                ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID.ToString() });
                           }

                            if (datadump.name.StartsWith("Mission_Altr"))
                            {
                                ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID, LocalisedName = datadump.LocalisedName });
                            }

                            //****** ASSASSINATE MISSIONS*******
                            else if (datadump.name.StartsWith("Mission_Assass"))

                            {
                                if (FileNumber == 0)
                                       {

                                     ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID.ToString() });
                                     }

                                    foreach (var mission in ActiveMissionList)
                                {

                                    // Debug.Log(datadump.MissionID);
                                    if (mission.MissionID == datadump.MissionID)
                                    {
                                        Debug.Log("Mission " + datadump.name + "Added");
                                        FoundMissions = FoundMissions + 1;
                                        mission.AcceptedTime = datadump.timestamp;
                                        mission.LocalisedName = datadump.LocalisedName;
                                        mission.reward = datadump.Reward;
                                        mission.Target = datadump.Target;
                                        mission.TargetType_Localised = datadump.TargetType_Localised;
                                        mission.TargetFaction = datadump.TargetFaction;
                                        mission.DestinationSystem = datadump.DestinationSystem;
                                        mission.Expiry = datadump.Expiry;
                                        mission.type = MissionType;

                                    }
                                }

                            }
                            //****** MASSACRE MISSIONS*******
                            else if (datadump.name.StartsWith("Mission_Massa"))

                            {
                                if (JournalLine == 2)
                                {

                                    ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID.ToString() });
                                }

                                foreach (var mission in ActiveMissionList)
                                {

                                    // Debug.Log(datadump.MissionID);
                                    if (mission.MissionID == datadump.MissionID)
                                    {
                                        Debug.Log("Mission " + datadump.name + "Added");
                                        FoundMissions = FoundMissions + 1;
                                        mission.AcceptedTime = datadump.timestamp;
                                        mission.LocalisedName = datadump.LocalisedName;
                                        mission.reward = datadump.Reward;
                                        mission.Target = datadump.KillCount.ToString();
                                        mission.TargetType_Localised = datadump.TargetType_Localised;
                                        mission.TargetFaction = datadump.TargetFaction;
                                        mission.DestinationSystem = datadump.DestinationSystem;
                                        mission.Expiry = datadump.Expiry;
                                        mission.type = MissionType;

                                    }
                                }

                            }
                            else if (datadump.name.StartsWith("Mission_Courier"))

                            {
                                if (JournalLine == 2)
                                {

                                    ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID.ToString() });
                                }
                                foreach (var mission in ActiveMissionList)
                                {

                                    // Debug.Log(datadump.MissionID);
                                    if (mission.MissionID == datadump.MissionID)
                                    {
                                        Debug.Log("Mission " + datadump.name + "Added");
                                        FoundMissions = FoundMissions + 1;
                                        mission.AcceptedTime = datadump.timestamp;
                                        mission.LocalisedName = datadump.LocalisedName;
                                        mission.reward = datadump.Reward;
                                        mission.TargetFaction = datadump.TargetFaction;
                                        mission.DestinationSystem = datadump.DestinationSystem;
                                        mission.DestinationStation = datadump.DestinationStation;
                                        mission.Expiry = datadump.Expiry;
                                        mission.type = MissionType;

                                    }
                                }

                            }
                            else if (datadump.name.StartsWith("Mission_Deliver"))

                            {
                                if (JournalLine == 2)
                                {

                                    ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID.ToString() });
                                }
                                foreach (var mission in ActiveMissionList)
                                {

                                    // Debug.Log(datadump.MissionID);
                                    if (mission.MissionID == datadump.MissionID)
                                    {
                                        Debug.Log("Mission " + datadump.name + "Added");
                                        FoundMissions = FoundMissions + 1;
                                        mission.AcceptedTime = datadump.timestamp;
                                        mission.LocalisedName = datadump.LocalisedName;
                                        mission.reward = datadump.Reward;
                                        mission.TargetFaction = datadump.TargetFaction;
                                        mission.DestinationSystem = datadump.DestinationSystem;
                                        mission.DestinationStation = datadump.DestinationStation;
                                        mission.Expiry = datadump.Expiry;
                                        mission.type = MissionType;
                                        mission.Commodity = datadump.Commodity;
                                    }
                                }

                            }

                            else if (datadump.name.StartsWith("Mission_Disable"))

                            {
                                if (JournalLine == 2)
                                {

                                    ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID.ToString() });
                                }
                                foreach (var mission in ActiveMissionList)
                                {

                                    // Debug.Log(datadump.MissionID);
                                    if (mission.MissionID == datadump.MissionID)
                                    {
                                        Debug.Log("Mission " + datadump.name + "Added");
                                        FoundMissions = FoundMissions + 1;
                                        mission.AcceptedTime = datadump.timestamp;
                                        mission.LocalisedName = datadump.LocalisedName;
                                        mission.DestinationSystem = datadump.DestinationSystem;
                                        mission.reward = datadump.Reward;
                                        mission.TargetFaction = datadump.TargetFaction;
                                        mission.TargetType_Localised = datadump.Target_Localised;
                                        mission.Expiry = datadump.Expiry;
                                        mission.type = MissionType;
                                    }
                                }

                            }
                            else if (datadump.name.StartsWith("Mission_Collect"))

                            {
                                if (JournalLine == 2)
                                {

                                    ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID.ToString() });
                                }
                                foreach (var mission in ActiveMissionList)
                                {

                                    // Debug.Log(datadump.MissionID);
                                    if (mission.MissionID == datadump.MissionID)
                                    {
                                        Debug.Log("Mission " + datadump.name + "Added");
                                        FoundMissions = FoundMissions + 1;
                                        mission.AcceptedTime = datadump.timestamp;
                                        mission.LocalisedName = datadump.LocalisedName;
                                        mission.reward = datadump.Reward;
                                        mission.TargetFaction = datadump.TargetFaction;
                                        mission.Expiry = datadump.Expiry;
                                        mission.type = MissionType;
                                    }
                                }

                            }
                            Debug.Log(FoundMissions + " Missions Found");
                        }

                        // Populate ended missions list
                        if (datadump.@event == "MissionCompleted" || datadump.@event == "MissionAbandoned" || datadump.@event == "MissionFailed")
                        {

                            EndedMissionList.Add(new MissionEnd { MissionID = datadump.MissionID });
                        }
                        //End of timestamp check

                        Debug.Log("Timestamp updated");
                    }
                    else
                    {
                        Debug.Log("Line " + j + " not read as old");
                    }

                }
                else
                {
                    Debug.Log("game not loaded");
                    if (datadump.@event == "Commander" || datadump.part > 1)
                    {
                        Debug.Log("Commander event found!");
                        LastLineNumber = j;
                        JournalLine = 1;
                        Debug.Log("Journal Line is now " + JournalLine);
                        //GetFile();
                    }
                }
            }
            if (FileNumber == 0)
            {
                LastLineNumber = j;
            }
        }
        Debug.Log(" Active mission list = " + ActiveMissionList.Count);

        ScrollJournals();    

    }

    public void ScrollJournals()
    {
        if (FoundMissions < JournalStartMissions && JournalLine < 2)
        {
            Debug.Log("Mission counts " + FoundMissions + " " + JournalStartMissions);
            FileNumber = FileNumber+1;
            Debug.Log("Opening File " + FileNumber);
            GetFile();
        }
        else
        {
            Debug.Log("Mission counts " + FoundMissions + " " + JournalStartMissions);
            FileNumber = 0;
            JournalLine = 2;
            MissionCleanse();
            KillCountUpdate();
        }
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

            }
        }

        foreach (var sys in ActiveMissionList)
        {
            try
            {
                Systems location = EDDBData.Find(x => x.name.Contains(sys.DestinationSystem));
                sys.Location = new Vector3(float.Parse(location.x), float.Parse(location.y) , float.Parse(location.z));
                float dist = Vector3.Distance(sys.Location, PlayerLocation);
                sys.distance = dist;
            }
            catch(Exception)
            {
                
            }
        }
    }

    public void Reward()
    {
        ActiveMissionList = ActiveMissionList.OrderByDescending(x => x.reward).ToList();
        UpdateMissionList();
    }

    public void Distance()
    {
        ActiveMissionList = ActiveMissionList.OrderBy(x => x.distance).ToList();
        UpdateMissionList();
    }
    public void Time()
    {
        ActiveMissionList = ActiveMissionList.OrderBy(x => x.Expiry).ToList();
        UpdateMissionList();
    }

    public void UpdateMissionList()
    {
        GameObject MissionDetails;
        GameObject MissionImage;
        GameObject MissionActive;
        Debug.Log(ActiveMissionCount);
        ActiveMissionCount = ActiveMissionList.Count;

        if (ActiveMissionCount < 3)
        {
            for (int i = ActiveMissionCount; i < 3; i++)
            {
                Debug.Log("Active Missions are " + ActiveMissionCount);
                MissionDetails = GameObject.Find("Mission" + i);

                MissionDetails.GetComponent<Text>().text = "No mission available";
            }
        }
        
        if (ActiveMissionCount > 3)
        {
            ActiveMissionCount = 3;
        }

        for (int i = 0; i < ActiveMissionCount; i++)
        {
            TimeSpan countdown = ActiveMissionList[i].Expiry - DateTime.Now;
            
            MissionDetails = GameObject.Find("Mission" + i);
            MissionImage = GameObject.Find("MissionImage" + i);
            MissionActive= GameObject.Find("MissionActive" + i);
            Active = Resources.Load<Sprite>("Active");
            Blank = Resources.Load<Sprite>("Blank");

            if (ActiveMissionList[i].active == true)
            {
                MissionActive.GetComponent<Image>().overrideSprite = Active;
            }
            else
            {
                MissionActive.GetComponent<Image>().overrideSprite = Blank;
            }

            Mission = Resources.Load<Sprite>(ActiveMissionList[i].type);
            MissionImage.GetComponent<Image>().overrideSprite = Mission;

            MissionDetails.GetComponent<Text>().text = ActiveMissionList[i].type +  " " + ActiveMissionList[i].Target + " " +ActiveMissionList[i].TargetType_Localised +
                " System: " + ActiveMissionList[i].DestinationSystem + " - " + ActiveMissionList[i].DestinationStation + " " + ActiveMissionList[i].distance.ToString("f1") + " ly " +
                "\n"+ ActiveMissionList[i].reward.ToString("n0") + " cr " + countdown.Hours.ToString() +" hrs " + countdown.Minutes.ToString() + " Minutes "+ countdown.Seconds.ToString() + " Secs remaining";
        }
        SetActiveMission();
    }

    public void SetActiveMission()
    {
        GameObject ActiveMissionDetails;
        GameObject ImageActive;

        ActiveMissionDetails = GameObject.Find("ActiveMissionDetails");
        ImageActive = GameObject.Find("ImageActive");

        foreach (var mission in ActiveMissionList)
        {
            if(mission.active == true)
            {
     
                ActiveMission = Resources.Load<Sprite>(mission.type);
                ImageActive.GetComponent<Image>().overrideSprite = ActiveMission;
                ActiveMissionDetails.GetComponent<Text>().text = mission.KillCount-mission.TotalKills + "/" + mission.KillCount +
                    "\n" +mission.TargetType_Localised + " " + mission.TargetFaction;            
            }
        }

        

    }

}