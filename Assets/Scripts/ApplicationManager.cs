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
    [Header("Start Page")]
    public GameObject startPage;
    public TMP_InputField testerCountField;

    [Space, Header("Quest Page")]
    public GameObject questPage;
    public TextMeshProUGUI questIDField;
    public TMP_InputField userIDField;
    public SwitchManager groupIDField;

    [Space, Header("End Page")]
    public GameObject endPage;
    public TextMeshProUGUI warning;
    public GameObject questOnlinePanel;
    public TextMeshProUGUI waitingText;

    const string MISSING_ID = "One or more userIDs were left empty!";
    const string DUPLICATE_ID = "One or more users have duplicate userIDs!";
    const string URL = "ftp://ftpupload.net/htdocs/ProjectPerspective/appControl.json";
    NetworkCredential credential = new NetworkCredential("epiz_24876763", "Wr6f38F0XBubb");

    List<UserInfo> pageInfos = new List<UserInfo>();
    Dictionary<string, bool> questsOnline = new Dictionary<string, bool>();

    int pageIndex;
    TestGroup data;
    bool canStart;

    private void Start()
    {
        Get();
    }

    public void SetNumberOfTesters()
    {
        if (int.TryParse(testerCountField.text, out int numberOfTester))
        {
            if (numberOfTester <= 0) return;

            if(numberOfTester < pageInfos.Count)
            {
                int count = pageInfos.Count - numberOfTester;

                for (int i = 0; i < count; i++)
                    pageInfos.RemoveAt(pageInfos.Count - 1);
            }
            for (int i = pageInfos.Count; i < numberOfTester; i++)
            {
                pageInfos.Add(new UserInfo());
                pageInfos[i].questID = "Oculus Quest " + (i + 1);
            }

            Next();
        }
    }

    public void Next()
    {
        SaveFields();
        pageIndex = pageIndex.IncrementClamped(pageInfos.Count + 1);
        SetUpPage();
    }

    public void Previous()
    {
        SaveFields();
        pageIndex = pageIndex.DecrementClamped();
        SetUpPage();
    }

    void SaveFields()
    {
        if(pageIndex != 0 && pageIndex != pageInfos.Count + 1)
        {
            pageInfos[pageIndex - 1].userID = userIDField.text;
            pageInfos[pageIndex - 1].groupID = groupIDField.isOn ? "A" : "B";
            pageInfos[pageIndex - 1].questReady = false;
        }
    }

    void SetUpPage()
    {
        startPage.SetActive(pageIndex == 0);
        endPage.SetActive(pageIndex == pageInfos.Count + 1);

        if (!startPage.activeInHierarchy && !endPage.activeInHierarchy)
        {
            questPage.SetActive(true);

            questIDField.text = pageInfos[pageIndex - 1].questID;
            userIDField.text = pageInfos[pageIndex - 1].userID == "" ? "" : pageInfos[pageIndex - 1].userID;
            groupIDField.SetSwitch(pageInfos[pageIndex - 1].groupID != "B");
        }
        else
            questPage.SetActive(false);

        if(endPage.activeInHierarchy)
        {
            foreach(UserInfo pageInfo in pageInfos)
            {
                if (pageInfo.userID == "")
                {
                    warning.text = MISSING_ID;
                    warning.gameObject.SetActive(true);
                    break;
                }

                foreach(UserInfo _pageInfo in pageInfos)
                {
                    if (pageInfo != _pageInfo && pageInfo.userID == _pageInfo.userID)
                    {
                        warning.text = DUPLICATE_ID;
                        warning.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
            warning.gameObject.SetActive(false);
    }

    public void BeginExperience()
    {
        canStart = true;
        Post();
        endPage.SetActive(false);
        foreach(UserInfo pageInfo in pageInfos)
            questsOnline.Add(pageInfo.questID, false);

        waitingText.text = $"(0/{questsOnline.Count}) Quests Online. ";
        questOnlinePanel.SetActive(true);
        StartCoroutine(WaitForCallback());
    }

    IEnumerator WaitForCallback()
    {
        int dotCounter = 0;
        for (; ; )
        {
            yield return new WaitForSeconds(1);

            //Read from google sheets here
            Get();

            foreach(UserInfo user in data.users)
            {
                if (questsOnline[user.questID] != true)
                    questsOnline[user.questID] = user.questReady;
            }

            int count = 0;
            foreach(KeyValuePair<string, bool> keyValuePair in questsOnline)
            {
                if (keyValuePair.Value)
                    count++;
            }

            dotCounter = dotCounter.IncrementLoop(2);

            switch (dotCounter)
            {
                case 0:
                    waitingText.text = $"({count}/{questsOnline.Count}) Quests Online. ";
                    break;
                case 1:
                    waitingText.text = $"({count}/{questsOnline.Count}) Quests Online.. ";
                    break;
                case 2:
                    waitingText.text = $"({count}/{questsOnline.Count}) Quests Online... ";
                    break;
            }

            if (!questsOnline.ContainsValue(false))
                break;
        }

        Reset();
        questOnlinePanel.SetActive(false);
        startPage.SetActive(true);
    }

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
            Debug.LogError(e.Message);
        }
    }

    void Post()
    {
        WebClient client = new WebClient();
        client.Credentials = credential;
        client.Headers[HttpRequestHeader.ContentType] = "application/json";
        try
        {
            string jsonData = JsonUtility.ToJson(new TestGroup {canStart = canStart, users = pageInfos });
            client.UploadString(URL, jsonData);
        }
        catch (WebException e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void Reset()
    {
        questsOnline.Clear();
        pageInfos.Clear();
        canStart = false;
        Post();
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
    public string questID;
    public string userID;
    public string groupID;
    public bool questReady;
}