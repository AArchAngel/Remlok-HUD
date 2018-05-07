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
            JournalUpdate();
            GetComponent<JournalWatcher>().runGrabLog = false;
        }
        if (Input.GetKeyDown("f"))
        {
            JournalUpdate();
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


        foreach (string line in JournalData)
        {

            //Handle extra blank line at end of file
            if (line.Length == 0)
            { }
            else

            {
                // Json Deserialise
                DataDump datadump = JsonConvert.DeserializeObject<DataDump>(line);
    

                if (datadump.@event == "Missions")
                {
                

                    try
                    {
                        if (JournalStartMissions == 0)
                        {
                     
                            JournalStartMissions = datadump.Active.Length;
                            Debug.Log(JournalStartMissions);
                            for (int i = 0; i < datadump.Active.Length; i++)
                            {
                                ActiveMissionList.Add(new MissionAdd { MissionID = datadump.Active[i].MissionID.ToString() });
                              //  Debug.Log(ActiveMissionList[0]);
                            }
                        }
                    }
                    catch(Exception)
                    {

                    }
            
                }


                // Populate accepted missions

                try
                {
                    if (datadump.name.StartsWith("Mission_Massacre"))

                    {
                        MissionType = "Kill";
                    }
                    if (datadump.name.StartsWith("delivery"))

                    {
                        MissionType = "delivery";
                    }
                   
                }
                catch(Exception)
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
                   
                    if (datadump.name.StartsWith("Mission_Altr"))
                    {
                        ActiveMissionList.Add(new MissionAdd { MissionID = datadump.MissionID, LocalisedName = datadump.LocalisedName });
                    }
                    else
                    
                    {
                        
                        foreach (var mission in ActiveMissionList)
                        {

                           // Debug.Log(datadump.MissionID);
                            if (mission.MissionID == datadump.MissionID)
                            {
                                FoundMissions = FoundMissions+1;
                                mission.AcceptedTime = datadump.timestamp;
                                mission.LocalisedName = datadump.LocalisedName;
                                mission.reward = datadump.Reward;
                                mission.KillCount = datadump.KillCount;
                                mission.TargetType_Localised = datadump.TargetType_Localised;
                                mission.TargetFaction = datadump.TargetFaction;
                                mission.DestinationSystem = datadump.DestinationSystem;
                                mission.Expiry = datadump.Expiry;
                                mission.type = MissionType;

                           

                            }
                        }
                       
                    }
                }
                // Populate ended missions list
                if (datadump.@event == "MissionCompleted" || datadump.@event == "MissionAbandoned" || datadump.@event == "MissionFailed")
                {
                  
                    EndedMissionList.Add(new MissionEnd { MissionID = datadump.MissionID });
                }
            }

        }
  

        ScrollJournals();    

    }

    public void ScrollJournals()
    {
        if (FoundMissions < JournalStartMissions)
        {
            FileNumber = FileNumber+1;
          
            GetFile();
        }
        else
        {
            MissionCleanse();
            KillCountUpdate();
        }
    }


    public void JournalUpdate()
    {

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

        ActiveMissionCount = ActiveMissionList.Count;

        if (ActiveMissionCount < 3)
        {
            for (int i = ActiveMissionCount; i < 3; i++)
            {
                MissionDetails = GameObject.Find("Mission" + i);
                MissionDetails.GetComponent<Text>().text = "No mission available";
            }
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

            MissionDetails.GetComponent<Text>().text = "Kill " + ActiveMissionList[i].KillCount + " " +ActiveMissionList[i].TargetType_Localised +
                " System: " + ActiveMissionList[i].DestinationSystem + " - "+ ActiveMissionList[i].distance.ToString("f1") + " ly " +
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