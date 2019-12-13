using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.UI.ModernUIPack;
using System;
using OuterRimStudios.Utilities;

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager Instance;
    [Header("Start Page")]
    public GameObject startPage;
    public TMP_InputField testerCountField;

    [Space, Header("Quest Page")]
    public GameObject questPage;
    public QuestUser questUserPrefab;
    public Transform contentTransform;

    [Space, Header("End Page")]
    public GameObject endPage;
    public TextMeshProUGUI warning;
    public GameObject questOnlinePanel;
    public Transform iconParent;
    public HeadsetIconManager iconPrefab;
    public GameObject networkErrorText;

    public float networkErrorDelay = 30;

    const string MISSING_ID = "One or more userIDs were left empty!";
    const string DUPLICATE_ID = "One or more users have duplicate userIDs!";
    const string URL = "ftp://ftpupload.net/htdocs/ProjectPerspective/appControl.json";
    NetworkCredential credential = new NetworkCredential("epiz_24876763", "Wr6f38F0XBubb");

    //Parallel Lists
    List<UserInfo> pageInfos = new List<UserInfo>();
    List<QuestUser> questUsers = new List<QuestUser>();

    Dictionary<int, bool> questsOnline = new Dictionary<int, bool>();
    List<HeadsetIconManager> headsetIcons = new List<HeadsetIconManager>();

    int pageIndex;
    TestGroup data;
    bool canStart;
    Coroutine waitForResponse;
    float responseStartTime;

    private void Awake()
    {
        Instance = this;
    }

    public void SetNumberOfTesters()
    {
        if (int.TryParse(testerCountField.text, out int numberOfTester))
        {
            if (numberOfTester <= 0) return;

            pageInfos.Clear();

            if (questUsers.Count > 0)
            {
                foreach (QuestUser questUser in questUsers)
                    Destroy(questUser.gameObject);

                questUsers.Clear();
            }

            for (int i = 0; i < numberOfTester; i++)
            {
                pageInfos.Add(new UserInfo());
                pageInfos[i].questID = i + 1;
                pageInfos[i].groupID = "A";
                QuestUser questUser = Instantiate(questUserPrefab, contentTransform);
                questUsers.Add(questUser);
            }

            for (int i = 0; i < questUsers.Count; i++)
            {
                questUsers[i].questIDField.text = "Oculus Quest " + pageInfos[i].questID;
                questUsers[i].userIDField.text = pageInfos[i].userID == "" ? "" : pageInfos[i].userID;
                questUsers[i].groupIDField.SetSwitch(true);
            }
        }
    }

    public void SaveFields()
    {
        Debug.Log("Save Fields");
        for(int i = 0; i < questUsers.Count; i++)
        {
            pageInfos[i].userID = questUsers[i].userIDField.text;
            pageInfos[i].groupID = questUsers[i].groupIDField.isOn ? "A" : "B";
            pageInfos[i].questReady = false;
        }
    }

    public void SetGroups()
    {
        for (int i = 0; i < questUsers.Count; i++)
            questUsers[i].groupIDField.SetSwitch(pageInfos[i].groupID == "A" ? true : false);
    }

    public void CheckUsers()
    {
        Debug.LogError("Checking Users");
        bool isMissing = false;
        foreach (UserInfo pageInfo in pageInfos)
        {
            if (pageInfo.userID == "" || pageInfo.userID == null)
            {
                isMissing = true;
                break;
            }
            else
                Debug.Log(pageInfo.userID);

            foreach (UserInfo _pageInfo in pageInfos)
            {
                if (pageInfo != _pageInfo && pageInfo.userID == _pageInfo.userID)
                {
                    warning.text = DUPLICATE_ID;
                    warning.gameObject.SetActive(true);
                }
            }
        }

        if (isMissing)
        {
            warning.text = MISSING_ID;
            warning.gameObject.SetActive(true);
        }
        else
            warning.gameObject.SetActive(false);
    }

    public void BeginExperience()
    {
        canStart = true;
        Post();
        endPage.SetActive(false);
        foreach (UserInfo pageInfo in pageInfos)
        {
            if (questsOnline.ContainsKey(pageInfo.questID))
                questsOnline[pageInfo.questID] = false;
            else
                questsOnline.Add(pageInfo.questID, false);

            HeadsetIconManager headset = Instantiate(iconPrefab, iconParent);
            headsetIcons.Add(headset);
            headset.Initialize(pageInfo.questID);
        }

        questOnlinePanel.SetActive(true);
        waitForResponse = StartCoroutine(WaitForCallback());
    }

    IEnumerator WaitForCallback()
    {
        responseStartTime = Time.time;
        for (; ; )
        {
            yield return new WaitForSeconds(1);
            if (Time.time - responseStartTime >= networkErrorDelay)
                networkErrorText.SetActive(true);

            Get();

            if (data == null) yield break;
            foreach(UserInfo user in data.users)
            {
                if (questsOnline[user.questID] != true)
                {
                    questsOnline[user.questID] = user.questReady;
                    headsetIcons[user.questID - 1].Ready(user.questReady);
                }
            }

            if (!questsOnline.ContainsValue(false))
                break;
        }

        Reset();
        questOnlinePanel.SetActive(false);
        startPage.SetActive(true);
    }

    //Reads data from the remote file
    void Get()
    {
        WebClient request = new WebClient();
        request.Credentials = credential;
        try
        {
            byte[] newFileData = request.DownloadData(URL);
            string fileString = System.Text.Encoding.UTF8.GetString(newFileData);
            data = JsonUtility.FromJson(fileString, typeof(TestGroup)) as TestGroup;
        }
        catch (WebException e)
        {
            Debug.LogError($"This was caused when getting {e.Message} {e.Status}");
        }
    }

    //sends data to the remote file
    void Post()
    {
        WebClient client = new WebClient();
        client.Credentials = credential;
        client.Headers[HttpRequestHeader.ContentType] = "application/json";
        try
        {
            string jsonData = JsonUtility.ToJson(new TestGroup { canStart = canStart, users = pageInfos });
            client.UploadString(URL, jsonData);
        }
        catch (WebException e)
        {
            Debug.LogError($"This was caused when posting {e.Message} {e.Status}");
        }
    }

    public void CancelStart()
    {
        endPage.SetActive(true);
        questOnlinePanel.SetActive(false);
        networkErrorText.SetActive(false);
        responseStartTime = 0;

        if (waitForResponse != null)
            StopCoroutine(waitForResponse);
        
        DestroyHeadsets();
        canStart = false;
        Post();
    }

    private void Reset()
    {
        networkErrorText.SetActive(false);
        questsOnline.Clear();
        DestroyHeadsets();
        pageInfos.Clear();

        foreach (QuestUser questUser in questUsers)
            Destroy(questUser.gameObject);

        questUsers.Clear();

        canStart = false;
        pageIndex = 0;
        Post();
    }

    void DestroyHeadsets()
    {
        foreach (HeadsetIconManager headset in headsetIcons)
            Destroy(headset.gameObject);
        headsetIcons.Clear();
    }

    private void OnApplicationQuit()
    {
        Reset();
    }
}

[Serializable]
public class TestGroup
{
    public bool canStart;
    public List<UserInfo> users;
}

[Serializable]
public class UserInfo
{
    public int questID;
    public string userID;
    public string groupID;
    public bool questReady;
}