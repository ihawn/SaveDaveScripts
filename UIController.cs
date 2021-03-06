using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using UnityEngine.Diagnostics;
using TMPro;
using Michsky.UI.ModernUIPack;

public class UIController : MonoBehaviour
{
    public Vector3 normalFollowOffset, cutSceneFollowOffset;
    private Leaderboards theLeaderboard;

    public Text debugText;
    public Button[] levelButtons;
    private ScoreManager theScoreManager;
    private StackSpawner theStackSpawner;
    private LevelController theLevelController;
    private AudioManager theAudioManager;
    private QualityController theQualityController;
    public GameObject titleImage, startScreen, gamePlayOverlay;
    public Animator titleImageAnimator;
    public GameObject levelCompletionScreen, deathScreen, mainMenu, levelMenu, endGameMenu, pauseMenu, pauseButton, leaderboards, inputMenu;
    Animator lcAnim;
    public bool wasInDeathMenu;
    public bool winScreenBuffer;

    public Image[] starImages;
    public Image[] levelStarImages;

    public Sprite emptyStar, fullStar, lockSprite;

    public Text postCutsceneText, feedbackText, perfectsText;
    public Color badCol, medCol, goodCol;
    public string[] badText, medText, goodText;
    public TextMeshProUGUI statsText;

    public GameObject CutSceneUI;

    public bool paused;

    public HorizontalSelector resSelector, qualSelector, waterfallQualSelector;
    public string[] qualityNames, waterfallQualityNames;

    public Text fpsCounter;

    public float feedbackFadeSpeed;
    Coroutine co;

    public Text multiplierText;

    public static SliderManager powerupSlider;
    public Slider sl;
    public SliderManager slide;
    public static bool increasePower;
    public float increaseAmount, powerupDuration, barScaleSpeed, colorLerpSpeed;
    public Color barHitColor, barStartColor;
    public static float barContactScale = 1.15f;
    public Button powerupButton;
    public static Vector3 powerupSliderPosition;
    public static bool powerupActive;
    public float powerupTimer;
    bool canPlayPowerupSound;
    public Text tapPrompt;

    public SwitchManager volumeSwitch;
    public TextMeshProUGUI volumeText;

    public GameObject helmet;
    public float helmetHeight;

    private void Awake()
    {
     //  DeleteAndCrash();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("sound") == 1 || !PlayerPrefs.HasKey("sound"))
        {
            AudioListener.volume = 1f;
            volumeSwitch.isOn = true;
            volumeText.text = "Volume On";
        }
        else
        {
            AudioListener.volume = 0f;
            volumeSwitch.isOn = false;
            volumeText.text = "Volume Off";
        }
    

        tapPrompt.gameObject.SetActive(false);
        canPlayPowerupSound = true;
        sl = slide.GetComponent<Slider>();
        powerupSlider = slide;

        powerupActive = false;
        powerupButton.gameObject.SetActive(false);
        increasePower = false;

        theLeaderboard = FindObjectOfType<Leaderboards>();
        perfectsText.color = goodCol;

        co = StartCoroutine(FadeFeedback());
        feedbackText.gameObject.SetActive(false);

        theAudioManager = FindObjectOfType<AudioManager>();
        winScreenBuffer = true;
        debugText.text = "";

        theQualityController = FindObjectOfType<QualityController>();
        theScoreManager = FindObjectOfType<ScoreManager>();
        lcAnim = levelCompletionScreen.GetComponent<Animator>();
        theStackSpawner = FindObjectOfType<StackSpawner>();
        theLevelController = FindObjectOfType<LevelController>();

        titleImageAnimator = titleImage.GetComponent<Animator>();




        titleImageAnimator.SetBool("inGame", false);

        CutSceneUI.SetActive(false);

        // StartCoroutine(InitSettings());

        //set resolution options
        if (resSelector.itemList.Count == 0)
        {
            theQualityController.resolutions = new Vector2[theQualityController.resolutionCount];

            for (int i = theQualityController.resolutionCount; i > 0; i--)
            {
                theQualityController.resolutions[i - 1] = new Vector2(3 * Display.main.systemWidth / (i + 2), 3 * Display.main.systemHeight / (i + 2));
                resSelector.CreateNewItem("" + (int)theQualityController.resolutions[i - 1].x + " X " + (int)theQualityController.resolutions[i - 1].y);

            }
        }
        //set quality options
        if (qualSelector.itemList.Count == 0)
        {
            for (int i = 0; i < qualityNames.Length; i++)
            {
                qualSelector.CreateNewItem(qualityNames[i]);

            }
        }
        //set waterfall options
        if (waterfallQualSelector.itemList.Count== 0)
        {
            for(int i = 0; i < waterfallQualityNames.Length; i++)
            {
                waterfallQualSelector.CreateNewItem(waterfallQualityNames[i]);
            }
        }

        if(PlayerPrefs.HasKey("res"))
            theQualityController.SetResolution(PlayerPrefs.GetInt("res"));
        if(PlayerPrefs.HasKey("qual"))
            theQualityController.SetQuality(PlayerPrefs.GetInt("qual"));
        else
            QualitySettings.SetQualityLevel(5, true);
        if (PlayerPrefs.HasKey("waterfallQual"))
            theQualityController.SetWaterfallQuality(PlayerPrefs.GetInt("waterfallQual"));
        else
        {
            PlayerPrefs.SetInt("waterfallQual", 2);
            theQualityController.SetWaterfallQuality(2);
        }

        multiplierText.text = "";


        theStackSpawner.levelMarkers = new GameObject[theLevelController.levelRequirements.Length];

        if (!PlayerPrefs.HasKey("mode"))
        {
            theStackSpawner.StartCoroutine(theStackSpawner.SpawnLevelMarker());
            PlayerPrefs.SetString("mode", "level");
        }
        else
            theStackSpawner.StartCoroutine(theStackSpawner.SpawnLevelMarker());

    }

    public void DisableSounds()
    {
        AudioListener.volume = 0f;
        PlayerPrefs.SetInt("sound", 0);
        volumeText.text = "Volume Off";
    }

    public void EnableSounds()
    {
        AudioListener.volume = 1f;
        PlayerPrefs.SetInt("sound", 1);
        volumeText.text = "Volume On";
    }

    void UpdatePowerupSlider()
    {
        if(increasePower)
        {
            powerupTimer = powerupDuration;
            powerupSlider.mainSlider.value += increaseAmount;


            increasePower = false;
        }

        if (powerupSlider.mainSlider.value >= 1f && theLevelController.inGame)
        {
            //waiting to activate powerup
            powerupButton.gameObject.SetActive(true);
            if (canPlayPowerupSound)
            {
                canPlayPowerupSound = false;
                theAudioManager.PlayBonusFull();
            }

        }

        if (powerupActive)
        {
            powerupTimer -= Time.deltaTime;
            powerupSlider.mainSlider.value = powerupTimer / powerupDuration; 
        }

        powerupSlider.transform.localScale = Vector3.Lerp(powerupSlider.transform.localScale, Vector3.one, barScaleSpeed * Time.deltaTime);

        var colorBlock = sl.colors;
        colorBlock.disabledColor = Color.Lerp(sl.colors.disabledColor, barStartColor, Time.deltaTime * colorLerpSpeed);
        sl.colors = colorBlock;
    }

    void UpdateHelmet()
    {
        if(theStackSpawner.transform.position.y >= helmetHeight && !helmet.activeInHierarchy)
        {
            helmet.SetActive(true);
        }
        else if(theStackSpawner.transform.position.y < helmetHeight && helmet.activeInHierarchy)
        {
            helmet.SetActive(false);
        }
    }

    public void OnPowerupPress()
    {
    //    powerupSlider.mainSlider.value = 0f;
        powerupButton.gameObject.SetActive(false);
        ActivatePowerup();
        canPlayPowerupSound = true;
    }

    public void ActivatePowerup()
    {
        powerupSlider.mainSlider.value = 0.999f;
        theStackSpawner.multiplier = 0;
        powerupActive = true;
        theStackSpawner.perfectTolerance = 1f;
        tapPrompt.gameObject.SetActive(true);
        tapPrompt.color = goodCol;
        tapPrompt.text = "Tap Fast!";
        StartCoroutine(WaitToDeactivatePowerup1());
    }

    public void DeactivatePowerup()
    {
      //  powerupSlider.mainSlider.value = 0f;
        tapPrompt.gameObject.SetActive(false);
        powerupActive = false;
        theStackSpawner.perfectTolerance = theStackSpawner.startingPerfectTolerance;
    }

    IEnumerator WaitToDeactivatePowerup1()
    {
        yield return new WaitForSeconds(powerupDuration*0.5f);
        tapPrompt.text = "Careful!";
        tapPrompt.color = medCol;
        StartCoroutine(WaitToDeactivatePowerup2());
    }

    IEnumerator WaitToDeactivatePowerup2()
    {
        yield return new WaitForSeconds(powerupDuration * 0.3f);
        tapPrompt.text = "Stop!";
        tapPrompt.color = badCol;
        StartCoroutine(WaitToDeactivatePowerup3());

    }

    IEnumerator WaitToDeactivatePowerup3()
    {
        yield return new WaitForSeconds(powerupDuration * 0.2f);

        DeactivatePowerup();
    }


    public void ResetPowerup()
    {
        canPlayPowerupSound = true;
        powerupActive = false;
        theStackSpawner.perfectTolerance = theStackSpawner.startingPerfectTolerance;
        powerupSlider.mainSlider.value = 0f;
    }

    public void ShowStats(int rank)
    {
        if (rank > 0)
        {
            string rankText;

            if (rank > 999)
                rankText = "More than 1000th place";
            else
                rankText = rank.ToString() + "th place";

            if (PlayerPrefs.HasKey("username"))
            {
                statsText.text = PlayerPrefs.GetString("username") + "\nHigh Score: " + theScoreManager.highScore.ToString() + "\nRank: " + rankText;
            }
        }
    }

    public void OpenUsernameSubmission()
    {
        inputMenu.SetActive(true);
    }

    public void CloseUsernameSubmission()
    {
        inputMenu.SetActive(false);
     //  theLeaderboard.OnUserFieldSubmission();
    }

    private void Update()
    {
        fpsCounter.text = "" + 1f / Time.deltaTime;
        UpdateHelmet();
        powerupSliderPosition = Camera.main.ScreenToWorldPoint(powerupSlider.transform.position);
        UpdatePowerupSlider();
    }

    public void OpenLeaderboard()
    {
        mainMenu.SetActive(false);
        leaderboards.SetActive(true);

        ShowStats(theLeaderboard.place);
    }

    public void CloseLeaderboard()
    {
        mainMenu.SetActive(true);
        leaderboards.SetActive(false);
    }

    IEnumerator FadeFeedback()
    {
        Color col = feedbackText.color;
        float alpha = col.a;

        while(alpha > 0)
        {
            feedbackText.color = new Color(col.r, col.g, col.b, alpha);
            perfectsText.color = new Color(col.r, col.g, col.b, alpha);
            alpha -= Time.deltaTime*feedbackFadeSpeed;
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
    }

    public void ShowFeedbackText(string whichOne)
    {
        feedbackText.gameObject.SetActive(true);
        StopCoroutine(co);

        if (!powerupActive)
        {
            switch (whichOne)
            {
                case "bad":
                    feedbackText.color = badCol;
                    ShowBadText();
                    break;

                case "medium":
                    feedbackText.color = medCol;
                    ShowMediumText();
                    break;

                case "good":
                    feedbackText.color = goodCol;
                    ShowGoodText();
                    break;

                case "bonusGood":
                    feedbackText.color = goodCol;
                    ShowGoodBonusText();
                    break;

                case "bonusBad":
                    feedbackText.color = badCol;
                    ShowBadBonusText();
                    break;
            }
        }
        else
        {
            ShowFeedbackSubtext("");
            feedbackText.text = "";
        }

        co = StartCoroutine(FadeFeedback());
    }

    void ShowBadText()
    {
        feedbackText.text = badText[Random.Range(0, badText.Length)];
        ShowFeedbackSubtext("");
    }

    void ShowMediumText()
    {
        feedbackText.text = medText[Random.Range(0, medText.Length)];
        ShowFeedbackSubtext("");
    }

    void ShowGoodText()
    {
        feedbackText.text = goodText[Random.Range(0, goodText.Length)];
        if(theStackSpawner.perfectsInARow < 8 && !powerupActive)
            ShowFeedbackSubtext((theStackSpawner.perfectsInARow + 1).ToString() + "/8");
        else
            ShowFeedbackSubtext("");
    }

    void ShowGoodBonusText()
    {
        feedbackText.text = "Bonus!";
        if (theStackSpawner.perfectsInARow < 8)
            ShowFeedbackSubtext((theStackSpawner.perfectsInARow + 1).ToString() + "/8");
        else
            ShowFeedbackSubtext("");
    }

    void ShowBadBonusText()
    {
        feedbackText.text = "miss";
    }

    public void UnlockAll()
    {
        PlayerPrefs.SetInt("lastBeatenLevel", theLevelController.levelRequirements.Length - 1);
    }

    void ShowFeedbackSubtext(string txt)
    {
        perfectsText.text = txt;
    }

    //dont run this ever unless you are purposly trying to crash system
    public void DeleteAndCrash()
    {
        PlayerPrefs.DeleteAll();
        Utils.ForceCrash(ForcedCrashCategory.Abort);
    }

    IEnumerator InitSettings()
    {
        yield return new WaitForEndOfFrame();

        InitializeSettingsMenu();
    }


    void InitializeSettingsMenu()
    {

        resSelector.defaultIndex = 5 - PlayerPrefs.GetInt("res");
        resSelector.selectorEvent.AddListener(delegate { theQualityController.SetResolution(theQualityController.resolutions.Length - resSelector.index - 1); });

        qualSelector.defaultIndex = PlayerPrefs.GetInt("qual");
        qualSelector.selectorEvent.AddListener(delegate { theQualityController.SetQuality(qualSelector.index); });

        waterfallQualSelector.defaultIndex = PlayerPrefs.GetInt("waterfallQual");
        waterfallQualSelector.selectorEvent.AddListener(delegate { theQualityController.SetWaterfallQuality(waterfallQualSelector.index); });
    }

    public void StartEndless()
    {
        PlayerPrefs.SetString("mode", "endless");
        pauseButton.SetActive(true);
        theLevelController.ship.SetActive(false);

        //  debugText.text = "" + PlayerPrefs.GetInt("lastBeatenLevel");

        SelectLevel(1);

        mainMenu.SetActive(false);
       // theScoreManager.SpawnHighScoreMarker();
    }
    

    public void PauseGame()
    {
        InitializeSettingsMenu();
        paused = true;
        gamePlayOverlay.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        paused = false;
        gamePlayOverlay.SetActive(true);
        pauseButton.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void ShowCutsceneOnNextSession()
    {
        PlayerPrefs.SetInt("showCutscene", 1);
    }

    public void DontShowCutsceneOnNextSession()
    {
        PlayerPrefs.SetInt("showCutscene", 0);
    }

    public void HideStartScreen()
    {
        titleImage.SetActive(false);
        startScreen.SetActive(false);
    }

    public void ContinueFromCutScene()
    {
        theAudioManager.StartCoroutine(theAudioManager.FadeOutSound(theAudioManager.cutsceneMusic, 0.15f));
        CutSceneUI.SetActive(true);
        postCutsceneText.text = "Dave hopes you liked his drawings";

        StartCoroutine(AfterCutsceneTextProgression());
    }

    IEnumerator AfterCutsceneTextProgression()
    {
        yield return new WaitForSeconds(3.5f);

        postCutsceneText.text = "And that you can save him...";

        StartCoroutine(WaitForButton());
    }

    IEnumerator WaitForButton()
    {
        yield return new WaitForSeconds(2.5f);

        ShowStartScreen();
        theLevelController.inCutscene = false;
    }

    public void ShowStartScreen()
    {
        CutSceneUI.SetActive(false);
        titleImage.SetActive(true);
        startScreen.SetActive(true);
    }

    public void MMtoGame()
    {
        startScreen.SetActive(false);
        gamePlayOverlay.SetActive(true);
        theStackSpawner.davesAnim.SetBool("StartGame", true);
    }

    public void ShowLevelCompletionScreen()
    {
        lcAnim.SetBool("Restart", true);
        lcAnim.SetBool("StartToFade", false);
        pauseButton.SetActive(false);
    }

    public void HideLevelCompletionScreen()
    {
        pauseButton.SetActive(true);
        lcAnim.SetBool("Restart", false);
        lcAnim.SetBool("StartToFade", true);
    }

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void OpenMainMenu()
    {
        // titleImageAnimator.SetBool("inGame", true);
        if(PlayerPrefs.HasKey("mode"))
            theStackSpawner.StartCoroutine(theStackSpawner.DestroyLevelMarkers());
        endGameMenu.SetActive(false);
        titleImage.SetActive(false);
        HideLevelCompletionScreen();
        levelMenu.SetActive(false);
        deathScreen.SetActive(false);
        mainMenu.SetActive(true);
        theLevelController.inGame = false;
        wasInDeathMenu = false;
        theStackSpawner.davesAnim.SetBool("Restart", true);
        theStackSpawner.davesAnim.SetBool("StartGame", false);
        CloseUsernameSubmission();
    }

    public void OpenLevels()
    {
        CloseUsernameSubmission();
        PlayerPrefs.SetString("mode", "level");
        theLevelController.ship.SetActive(true);
        mainMenu.SetActive(false);
        deathScreen.SetActive(false);
        levelMenu.SetActive(true);
        RefreshUnlockedLevels();
        RefreshStarCount();
    }

    public void CloseLevels()
    {
        if(wasInDeathMenu)
        {
            deathScreen.SetActive(true);
        }
        else
        {
            mainMenu.SetActive(true);
        }
        levelMenu.SetActive(false);
        
    }

    public void Continue()
    {
        pauseButton.SetActive(true);
        PlayerPrefs.SetString("mode", "level");

        //  debugText.text = "" + PlayerPrefs.GetInt("lastBeatenLevel");

        if (PlayerPrefs.GetInt("lastBeatenLevel") < theLevelController.levelRequirements.Length)
            SelectLevel(PlayerPrefs.GetInt("lastBeatenLevel") + 1);
        else
            SelectLevel(PlayerPrefs.GetInt("lastBeatenLevel"));

        mainMenu.SetActive(false);
    }

    public void SelectLevel(int l)
    {
        if(PlayerPrefs.GetString("mode") == "level" || !PlayerPrefs.HasKey("mode"))
            theStackSpawner.StartCoroutine(theStackSpawner.SpawnLevelMarker());

        if (theScoreManager.marker != null)
            Destroy(theScoreManager.marker);

        theAudioManager.PlayLevelWhoosh();

        winScreenBuffer = false;
        levelMenu.SetActive(false);
        theLevelController.inGame = true;
        theLevelController.OpenLevel(l);
        theLevelController.level = l - 1;
        theLevelController.canCelebrate = false;
        theStackSpawner.RestartGame();

        gamePlayOverlay.SetActive(true);

    }

    public void SelectCurrentLevel()
    {
        SelectLevel(theLevelController.level + 1);
    }

    void RefreshUnlockedLevels()
    {
        for(int i = 0; i < levelButtons.Length; i++)
        {
            if(i < PlayerPrefs.GetInt("lastBeatenLevel") + 1)
            {
                levelButtons[i].interactable = true;
            }
            else
            {
                levelButtons[i].interactable = false;
            }
        }
    }

    void RefreshStarCount()
    {
        for (int i = 0; i < theLevelController.levelRequirements.Length; i++)
        {
            if (levelButtons[i].interactable)
            {
                for (int j = 0; j < 3; j++)
                {
                    levelStarImages[3 * i + j].gameObject.SetActive(true);
                    if (theScoreManager.stars[i] > j)
                    {
                        //filled star
                        levelStarImages[3 * i + j].sprite = fullStar;
                    }
                    else
                    {
                        //empty star
                        levelStarImages[3 * i + j].sprite = emptyStar;
                    }
                }
            }

            else
            {
                levelStarImages[3 * i].gameObject.SetActive(false);
                levelStarImages[3 * i + 1].sprite = lockSprite;
                levelStarImages[3 * i + 2].gameObject.SetActive(false);
            }

        }


    }

    public void ShowEndGameMenu()
    {
        endGameMenu.SetActive(true);
        HideLevelCompletionScreen();
        gamePlayOverlay.SetActive(false);
    }
}
