using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JsonStartupConfig : MonoBehaviour
{
    public int defaultStartLevel = 0;
    public bool autoStart;  //  automatically start as server or client, depending upon whether I have a display or not.
    public const string kConfigJsonBasename = "MHConfig";
    public const string kConfigJson = kConfigJsonBasename + ".json";
    public bool isConfigLoaded;
    public StartupConfig startupConfig;
    public class StartupConfig
    {
        public string serverIP;
        public string serverPort;
        public int startingLevel;
    }

    void CreateDummyJson()
    {
#if UNITY_EDITOR
        Debug.LogWarning("Automatically created " + kConfigJson + ". Move this file to a Resources/ subfolder.\n");
        StartupConfig cfg = new StartupConfig();
        //  set up defaults here.
        cfg.startingLevel = defaultStartLevel;  //  this is the multiplayer map.
        cfg.serverIP = "35.236.114.54";   //  this is the address Graeme gave me.
        cfg.serverPort = "7777";    //  this is sort of the default for MechaHamster for whatever reason.
        string jsonOutStr = JsonUtility.ToJson(cfg);
        using (FileStream fs = new FileStream(kConfigJson, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(jsonOutStr);
            }
        }
#endif
    }

    public void ReadJsonStartupConfig()
    {
        var jsonAsset = Resources.Load<TextAsset>(kConfigJsonBasename); //  yep, Unity MUST drop off the extension. it won't find the file if you specify the correct filename explicitly. Ugh!
        TextAsset json = jsonAsset as TextAsset;

        if (json == null)
        {
            if (Application.isEditor)
            {
                CreateDummyJson();
            }
            if (autoStart)
                Debug.LogWarning("cannot autostart because File not found in Resources/: " + kConfigJson + "\n");
        }
        else
        {
            startupConfig = JsonUtility.FromJson<StartupConfig>(json.ToString());
            isConfigLoaded = true;
        }
    }

    private void Awake()
    {
        isConfigLoaded = false;
        DontDestroyOnLoad(this.gameObject); //  because NetworkManager has been set to DontDestroyOnLoad, it will be in a separate scene hierarchy/memory segment that cannot interact with this. Thus we must be in the same "zone" as the NetworkManager! Ugh, Unity!
    }
    // Start is called before the first frame update
    void Start()
    {
        if (this.isActiveAndEnabled)    //  allows this to be disabled so that we can easily test without loading the config file.
            ReadJsonStartupConfig();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
