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
    [SerializeField] private TextMeshProUGUI completedLevelsMap3Text;
    [SerializeField] private TextMeshProUGUI starMap1Text;
    [SerializeField] private TextMeshProUGUI starMap2Text;
    [SerializeField] private TextMeshProUGUI starMap3Text;


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
    private int starMap3;


    private void Awake()
    {
        TransferAndDeletePlayerPrefs();
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
        int completedLevels3Count = GetCompletedLevels3Count();

        completedLevelsMap1Text.text = completedLevels1Count.ToString() + "/" + "192";
        completedLevelsMap2Text.text = completedLevels2Count.ToString() + "/" + "32";
        completedLevelsMap3Text.text = completedLevels3Count.ToString() + "/" + "32";
        starMap1Text.text = starMap1.ToString();
        starMap2Text.text = starMap2.ToString();
        starMap3Text.text = starMap3.ToString();
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
                    for (int lv = 1; lv <= 192; lv++)
                    {
                        totalStars0 += PlayerPrefs.GetInt("Lv" + lv);
                    }
                    int maxStars = (mapSelection[i].endLevel - mapSelection[i].startLevel + 1) * 3;
                    unlockedStarsText[i].text = totalStars0.ToString();// + "/" + maxStars;
                    starMap1 = totalStars0;
                    break;

                case 1:
                    int totalStars1 = 0;
                    for (int lv = 1; lv <= 32; lv++)
                    {
                        totalStars1 += PlayerPrefs.GetInt("HLv" + lv);
                    }
                    unlockedStarsText[i].text = totalStars1.ToString();// + "/" + (mapSelection[i].endLevel - mapSelection[i].startLevel + 1) * 3;
                    starMap2 = totalStars1;
                    break;
                case 2:
                    int totalStars2 = 0;
                    for (int lv = 1; lv <= 32; lv++)
                    {
                        totalStars2 += PlayerPrefs.GetInt("NLv" + lv);
                    }
                    unlockedStarsText[i].text = totalStars2.ToString();// + "/" + (mapSelection[i].endLevel - mapSelection[i].startLevel + 1) * 3;
                    starMap3 = totalStars2;
                    break;
            }
        }
    }
    public void UpdateStarUI()
    {
        stars = 0;
        for (int i = 1; i <= 192; i++)
        {
            stars += PlayerPrefs.GetInt("Lv" + i);
        }
        for (int i = 1; i <= 32; i++)
        {
            stars += PlayerPrefs.GetInt("HLv" + i);
        }
        startText.text = stars.ToString() + "/" + 576;
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
        LoadHighestUnlockedLevel(1, 192);
    }

    public void LoadHighestUnlockedLevelMode2()
    {
        LoadHighestUnlockedLevel(1, 32);
    }

    private void LoadHighestUnlockedLevel(int start, int end)
    {
        int highestUnlockedLevel = start;

        for (int i = start; i <= end; i++)
        {
            if (PlayerPrefs.GetInt("Lv" + i) >= 0 && PlayerPrefs.GetInt("Level" + i + "_Win") == 1)
            {
                highestUnlockedLevel = i+1;
            }
            else
            {
                break; // stop if level lock
            }
        }

        //string levelName = "Level" + highestUnlockedLevel;
        Debug.Log("Loading highest unlocked level: " + highestUnlockedLevel);
        //SceneManager.LoadScene(levelName);
        PlayerPrefs.SetInt("SelectedLevel", highestUnlockedLevel);
        PlayerPrefs.SetString("SelectedMode", "Classic");
        SceneManager.LoadScene(1);
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

        for (int i = 1; i <= 192; i++)
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

        for (int i = 1; i <= 32; i++)
        {
            if (PlayerPrefs.GetInt("HLevel" + i + "_Win") == 1)
            {
                completedLevels2Count++;
            }
        }

        return completedLevels2Count;
    }
    public int GetCompletedLevels3Count()
    {
        int completedLevels3Count = 0;

        for (int i = 1; i <= 32; i++)
        {
            if (PlayerPrefs.GetInt("NLevel" + i + "_Win") == 1)
            {
                completedLevels3Count++;
            }
        }

        return completedLevels3Count;
    }
    void ClearPlayerPrefsForLevels()
    {
        if (PlayerPrefs.GetInt("GameUpdated", 0) == 0)
        {
            for (int levelIndex = 64; levelIndex <= 96; levelIndex++)
            {
                PlayerPrefs.DeleteKey("Lv" + levelIndex);
                PlayerPrefs.SetInt("Level" + levelIndex + "_Win", 0);
            }
            PlayerPrefs.SetInt("GameUpdated", 1);
            PlayerPrefs.Save();
        }
    }
    void TransferAndDeletePlayerPrefs()
    {
        if (PlayerPrefs.GetInt("DataTransferred", 0) == 0)
        {
            for (int oldLevelIndex = 65, newLevelIndex = 1; oldLevelIndex <= 96; oldLevelIndex++, newLevelIndex++)
            {
                int stars = PlayerPrefs.GetInt("Lv" + oldLevelIndex, 0);
                PlayerPrefs.SetInt("HLv" + newLevelIndex, stars);

                int winStatus = PlayerPrefs.GetInt("Level" + oldLevelIndex + "_Win", 0);
                PlayerPrefs.SetInt("HLevel" + newLevelIndex + "_Win", winStatus);

                PlayerPrefs.DeleteKey("Lv" + oldLevelIndex);
                PlayerPrefs.DeleteKey("Level" + oldLevelIndex + "_Win");
            }

            PlayerPrefs.SetInt("DataTransferred", 1);
            PlayerPrefs.Save();
        }
    }


}
