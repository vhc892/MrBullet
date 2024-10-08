﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hapiga.Ads;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI completedLevelsMap1Text;
    [SerializeField] private TextMeshProUGUI completedLevelsMap2Text;
    [SerializeField] private TextMeshProUGUI starMap1Text;
    [SerializeField] private TextMeshProUGUI starMap2Text;

    public static UIManager Instance;
    public GameObject mapSelectionPanel;
    public GameObject[] levelSelectionPanels;
    public GameObject settingPanel;
    public GameObject shopPanel;
    public int stars;
    public MapSelection[] mapSelection;
    public TextMeshProUGUI[] questStarsText;
    public TextMeshProUGUI[] lockedStarsText;
    public TextMeshProUGUI[] unlockedStarsText;
    public TextMeshProUGUI startText;
    public Image[] checkmarks;
    private Vector3[] initialPositions;
    private int starMap1;
    private int starMap2;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if(Instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);
        }
        initialPositions = new Vector3[levelSelectionPanels.Length];
        for (int i = 0; i < levelSelectionPanels.Length; i++)
        {
            initialPositions[i] = levelSelectionPanels[i].GetComponent<RectTransform>().anchoredPosition;
        }
    }
    private bool[] buttonStates;

    void Start()
    {
        //AdManager.Instance.ShowBanner();
        EconomyManager.Instance.AssignMoneyText();
    }
    
    private void Update()
    {
        UpdateLockedStarUI();
        UpdateUnlockedStarUI();
        UpdateStarUI();
        UpdateCompletedLevelsText();
    }
    private void UpdateCompletedLevelsText()
    {
        int completedLevels1Count = GetCompletedLevels1Count();
        int completedLevels2Count = GetCompletedLevels2Count();

        completedLevelsMap1Text.text = completedLevels1Count.ToString() + "/" + "64";
        completedLevelsMap2Text.text = completedLevels2Count.ToString() + "/" + "32";
        starMap1Text.text = starMap1.ToString();
        starMap2Text.text = starMap2.ToString();
    }
    public void UpdateLockedStarUI()
    {
        for(int i = 0;i < mapSelection.Length;i++)
        {
            questStarsText[i].text = stars.ToString() + "/" + mapSelection[i].questNum.ToString();
            if (mapSelection[i].isUnlock == false)
            {
                lockedStarsText[i].text = stars.ToString() + "/" + mapSelection[i].endLevel * 3;
            }
        }
    }
    public void UpdateUnlockedStarUI()
    {
        for(int i = 0; i < mapSelection.Length; i++)
        {
            unlockedStarsText[i].text = stars.ToString();// + "/" + mapSelection[i].endLevel * 3;
            switch (i)
            {
                case 0:
                    int totalStars0 = 0;
                    for (int lv = 1; lv <= 64; lv++)
                    {
                        totalStars0 += PlayerPrefs.GetInt("Lv" + lv);
                    }
                    int maxStars = (mapSelection[i].endLevel - mapSelection[i].startLevel + 1) * 3;
                    unlockedStarsText[i].text = totalStars0.ToString();// + "/" + maxStars;
                    starMap1 = totalStars0;
                    break;

                case 1:
                    int totalStars1 = 0;
                    for (int lv = 65; lv <= 96; lv++)
                    {
                        totalStars1 += PlayerPrefs.GetInt("Lv" + lv);
                    }
                    unlockedStarsText[i].text = totalStars1.ToString();// + "/" + (mapSelection[i].endLevel - mapSelection[i].startLevel + 1) * 3;
                    starMap2 = totalStars1;
                    break;
                case 2:
                    unlockedStarsText[i].text = 0.ToString(); ;// + "/" + (mapSelection[i].endLevel - mapSelection[i].startLevel + 1) * 3;
                    break;
            }
        }
    }
    public void UpdateStarUI()
    {
        stars = 0;
        for (int i = 1; i <= 96; i++)
        {
            stars += PlayerPrefs.GetInt("Lv" + i);
        }
        startText.text = stars.ToString() + "/" + 288;
    }
    public void PressMapButton(int mapIndex)
    {
        if (mapSelection[mapIndex].isUnlock == true)
        {
            mapSelectionPanel.gameObject.SetActive(false);
            StartCoroutine(ShowPanel(levelSelectionPanels[mapIndex], mapIndex));

            LevelSelection[] levels = levelSelectionPanels[mapIndex].GetComponentsInChildren<LevelSelection>();
            foreach (LevelSelection level in levels)
            {
                level.UpdateLevelStatus();
                level.UpdateLevelImage();
            }
        }
        else
        {
            Debug.Log("You cannot open this map now");
        }
    }

    private IEnumerator ShowPanel(GameObject panel, int mapIndex)
    {
        panel.SetActive(true);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        Vector3 startPosition = new Vector3(0, -Screen.height, 0);
        Vector3 targetPosition = initialPositions[mapIndex];

        float elapsedTime = 0f;
        float duration = 0.5f;

        rectTransform.anchoredPosition = startPosition;

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }

    public void BackMapButton()
    {
        foreach (MapSelection map in mapSelection)
        {
            map.UpdateMapStatus();
            map.UnlockMap();
        }
        mapSelectionPanel.gameObject.SetActive(true);
        for(int i = 0;i < mapSelection.Length; i++)
        {
            levelSelectionPanels[i].gameObject.SetActive(false);
        }
    }
    public void LoadHighestUnlockedLevelMode1()
    {
        LoadHighestUnlockedLevel(1, 63);
    }

    public void LoadHighestUnlockedLevelMode2()
    {
        LoadHighestUnlockedLevel(65, 95);
    }

    private void LoadHighestUnlockedLevel(int start, int end)
    {
        int highestUnlockedLevel = start;

        for (int i = start; i <= end; i++)
        {
            if (PlayerPrefs.GetInt("Lv" + i) >= 0 && PlayerPrefs.GetInt("Level" + i + "_Win") == 1)
            {
                highestUnlockedLevel = i + 1;
            }
            else
            {
                break; // stop if level lock
            }
        }

        string levelName = "Level" + highestUnlockedLevel;
        Debug.Log("Loading highest unlocked level: " + levelName);
        SceneManager.LoadScene(levelName);
    }
    public void TurnOnSettingPanel()
    {
        mapSelectionPanel.gameObject.SetActive(false);
        settingPanel.SetActive(true);
    }
    public void TurnOffSettingPanel()
    {
        mapSelectionPanel.gameObject.SetActive(true);
        settingPanel.SetActive(false);
    }
    public void TurnOnShopPanel()
    {
        mapSelectionPanel.gameObject.SetActive(false);
        shopPanel.SetActive(true);
    }
    public void TurnOffShopPanel()
    {
        mapSelectionPanel.gameObject.SetActive(true);
        shopPanel.SetActive(false);
    }
    public int GetCompletedLevels1Count()
    {
        int completedLevels1Count = 0;

        for (int i = 1; i <= 64; i++)
        {
            if (PlayerPrefs.GetInt("Level" + i + "_Win") == 1)
            {
                completedLevels1Count++;
            }
        }

        return completedLevels1Count;
    }
    public int GetCompletedLevels2Count()
    {
        int completedLevels2Count = 0;

        for (int i = 65; i <= 96; i++)
        {
            if (PlayerPrefs.GetInt("Level" + i + "_Win") == 1)
            {
                completedLevels2Count++;
            }
        }

        return completedLevels2Count;
    }
}
