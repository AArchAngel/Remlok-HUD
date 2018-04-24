using UnityEngine;
using System.Collections;
using System.IO;

public class JournalWatcher : MonoBehaviour
{
    public string fileToWatch = "*.*";
    private FileSystemWatcher watcher;
    public bool runGrabLog;


    IEnumerator Start()
    {
        watcher = new FileSystemWatcher();
        watcher.Path = "C:\\Users\\Phil\\Saved Games\\Frontier Developments\\Elite Dangerous";
        watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName
                | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite
                | NotifyFilters.Size;
        watcher.Filter = fileToWatch;
        watcher.Changed += new FileSystemEventHandler(OnChanged);
        watcher.Created += new FileSystemEventHandler(OnChanged);


        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        yield return null;
    }
    
    private void OnChanged(object source, FileSystemEventArgs e)
    {
          runGrabLog = true;
     }
}