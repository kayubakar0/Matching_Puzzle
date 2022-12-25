using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace _2048FALLING
{
    public class UIManager : MonoBehaviour
    {
        [Header("Object References")]
        public GameObject mainCanvas;
        public GameObject header;
        public GameObject title;
        public Text score;
        public Text bestScore;
        public GameObject newBestScore;
        public GameObject playBtn;
        public GameObject restartBtn;
        public GameObject menuButtons;
        public GameObject rewardUI;
        public GameObject settingsUI;
        public GameObject soundOnBtn;
        public GameObject soundOffBtn;
        public GameObject musicOnBtn;
        public GameObject musicOffBtn;
        public GameObject broadGame;
        public GameObject whiteCurtain;

        [Header("Premium Features Buttons")]
        public GameObject achievementBtn;
        public GameObject shareBtn;

        [Header("Sharing-Specific")]
        public GameObject shareUI;
        public ShareUIController shareUIController;

        Animator scoreAnimator;
        Animator dailyRewardAnimator;
        bool isWatchAdsForCoinBtnActive;
        static bool wasShowStartUI = false;

        void OnEnable()
        {
            GameManager.GameStateChanged += GameManager_GameStateChanged;
            ScoreManager.ScoreUpdated += OnScoreUpdated;
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= GameManager_GameStateChanged;
            ScoreManager.ScoreUpdated -= OnScoreUpdated;
        }

        // Use this for initialization
        void Start()
        {
            scoreAnimator = score.GetComponent<Animator>();
            // dailyRewardAnimator = dailyRewardBtn.GetComponent<Animator>();

            Reset();

            if (!wasShowStartUI)
            {
                ShowStartUI();
                wasShowStartUI = true;
            }
        }

        void returnHome()
        {
            wasShowStartUI = false;
        }

        // Update is called once per frame
        void Update()
        {
            score.text = ScoreManager.Instance.Score.ToString();
            bestScore.text = ScoreManager.Instance.HighScore.ToString();

            if (settingsUI.activeSelf)
            {
                UpdateSoundButtons();
                UpdateMusicButtons();
            }
        }

        void GameManager_GameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                whiteCurtain.SetActive(true);
                Invoke("ShowGameUI", 0.3f);
                //GameBroadManager.Instance.NewGame();
            }
            else if (newState == GameState.PreGameOver)
            {
                // Before game over, i.e. game potentially will be recovered
            }
            else if (newState == GameState.GameOver)
            {
                Invoke("ShowGameOverUI", 1f);
            }
        }

        void OnScoreUpdated(int newScore)
        {
            scoreAnimator.Play("NewScore");
        }

        void HideWhiteCurtain()
        {
            whiteCurtain.SetActive(false);
        }

        void Reset()
        {
            mainCanvas.SetActive(true);
            // characterSelectionUI.SetActive(false);
            header.SetActive(false);
            title.SetActive(false);
            score.gameObject.SetActive(false);
            newBestScore.SetActive(false);
            playBtn.SetActive(false);
            menuButtons.SetActive(false);
            // dailyRewardBtn.SetActive(false);
            whiteCurtain.SetActive(false);

            settingsUI.SetActive(false);
        }

        public void StartGame()
        {
            GameManager.Instance.StartGame();
        }

        public void EndGame()
        {
            GameManager.Instance.GameOver();
        }

        public void RestartGame()
        {

            // set game play again
            GameManager.Instance.RestartGame(0.1f);
            Time.timeScale = 1f;
            //GameBroadManager.Instance.ResetGameShare();
        }

        public void HomeGame()
        {

            // set game play again
            returnHome();
            // GameManager.Instance.RestartGame(0.1f);
            SceneManager.LoadScene("coba");
            Time.timeScale = 1f;
            //GameBroadManager.Instance.ResetGameShare();
        }

        public void ShowStartUI()
        {
            settingsUI.SetActive(false);

            header.SetActive(true);
            title.SetActive(true);
            playBtn.SetActive(true);
            restartBtn.SetActive(false);
            menuButtons.SetActive(true);
            shareBtn.SetActive(false);
            whiteCurtain.SetActive(false);  

            // If first launch: show "WatchForCoins" and "DailyReward" buttons if the conditions are met
            if (GameManager.GameCount == 0)
            {
                //ShowWatchForCoinsBtn();
                //ShowDailyRewardBtn();
            }
        }

        public void ShowGameUI()    // show main game play 
        {
            whiteCurtain.SetActive(false);
            header.SetActive(true);
            title.SetActive(false);
            score.gameObject.SetActive(true);
            playBtn.SetActive(false);
            menuButtons.SetActive(false);
            // dailyRewardBtn.SetActive(false);
            // watchRewardedAdBtn.SetActive(false);
            //if (broadGame.activeSelf == false)
            //    broadGame.SetActive(true);
        }

        public void ShowGameOverUI()
        {
            header.SetActive(true);
            title.SetActive(false);
            score.gameObject.SetActive(true);
            newBestScore.SetActive(ScoreManager.Instance.HasNewHighScore);

            playBtn.SetActive(false);
            restartBtn.SetActive(true);
            menuButtons.SetActive(true);
            settingsUI.SetActive(false);
        }

        void ShowWatchForCoinsBtn()
        {
            // Only show "watch for coins button" if a rewarded ad is loaded and premium features are enabled
#if EASY_MOBILE
            if (IsPremiumFeaturesEnabled() && AdDisplayer.Instance.CanShowRewardedAd() && AdDisplayer.Instance.watchAdToEarnCoins)
            {
                watchRewardedAdBtn.SetActive(true);
                watchRewardedAdBtn.GetComponent<Animator>().SetTrigger("activate");
            }
            else
            {
                watchRewardedAdBtn.SetActive(false);
            }
#endif
        }

        public void ShowSettingsUI()
        {
            settingsUI.SetActive(true);
        }

        public void HideSettingsUI()
        {
            settingsUI.SetActive(false);
        }

        public void WatchRewardedAd()
        {
#if EASY_MOBILE
            // Hide the button
            watchRewardedAdBtn.SetActive(false);

            AdDisplayer.CompleteRewardedAdToEarnCoins += OnCompleteRewardedAdToEarnCoins;
            AdDisplayer.Instance.ShowRewardedAdToEarnCoins();
#endif
        }

        void OnCompleteRewardedAdToEarnCoins()
        {
#if EASY_MOBILE
            // Unsubscribe
            AdDisplayer.CompleteRewardedAdToEarnCoins -= OnCompleteRewardedAdToEarnCoins;

            // Give the coins!
            ShowRewardUI(AdDisplayer.Instance.rewardedCoins);
#endif
        }

        public void ShowLeaderboardUI()
        {
#if EASY_MOBILE
            if (GameServices.IsInitialized())
            {
                GameServices.ShowLeaderboardUI();
            }
            else
            {
#if UNITY_IOS
            NativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
#elif UNITY_ANDROID
                GameServices.Init();
#endif
            }
#endif
        }

        public void ShowAchievementsUI()
        {
#if EASY_MOBILE
            if (GameServices.IsInitialized())
            {
                GameServices.ShowAchievementsUI();
            }
            else
            {
#if UNITY_IOS
            NativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
#elif UNITY_ANDROID
                GameServices.Init();
#endif
            }
#endif
        }

        public void PurchaseRemoveAds()
        {
#if EASY_MOBILE
            InAppPurchaser.Instance.Purchase(InAppPurchaser.Instance.removeAds);
#endif
        }

        public void RestorePurchase()
        {
#if EASY_MOBILE
            InAppPurchaser.Instance.RestorePurchase();
#endif
        }

        public void ShowShareUI()
        {
            if (!ScreenshotSharer.Instance.disableSharing)
            {
                Texture2D texture = ScreenshotSharer.Instance.CapturedScreenshot;
                shareUIController.ImgTex = texture;

#if EASY_MOBILE
                AnimatedClip clip = ScreenshotSharer.Instance.RecordedClip;
                shareUIController.AnimClip = clip;
#endif

                shareUI.SetActive(true);
            }
        }

        public void HideShareUI()
        {
            shareUI.SetActive(false);
        }

        public void ToggleSound()
        {
            SoundManager.Instance.ToggleSound();
        }

        public void ToggleMusic()
        {
            SoundManager.Instance.ToggleMusic();
        }


        public void ButtonClickSound()
        {
            Utilities.ButtonClickSound();
        }

        void UpdateSoundButtons()
        {
            if (SoundManager.Instance.IsSoundOff())
            {
                soundOnBtn.gameObject.SetActive(false);
                soundOffBtn.gameObject.SetActive(true);
            }
            else
            {
                soundOnBtn.gameObject.SetActive(true);
                soundOffBtn.gameObject.SetActive(false);
            }
        }

        void UpdateMusicButtons()
        {
            if (SoundManager.Instance.IsMusicOff())
            {
                musicOffBtn.gameObject.SetActive(true);
                musicOnBtn.gameObject.SetActive(false);
            }
            else
            {
                musicOffBtn.gameObject.SetActive(false);
                musicOnBtn.gameObject.SetActive(true);
            }
        }
    }
}