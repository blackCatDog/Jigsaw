using UnityEngine;
#if NO_GPGS
using GooglePlayGames;
#endif

public class GPGSController : MonoBehaviour
{
    public static GPGSController instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
#if NO_GPGS
        PlayGamesPlatform.Activate();
#endif
    }

    public void ShowLeaderboard()
    {
#if NO_GPGS
        Social.localUser.Authenticate((bool success) => {
            if (success)
            {
                PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_high_score);
                CFirebase.LogEvent("leaderboard", "show");
            }
        });

        CFirebase.LogEvent("leaderboard", "button_click");
#endif
    }

    public void ReportScore(int score)
    {
#if NO_GPGS
        if (Social.localUser.authenticated)
        {
            Social.ReportScore(score, GPGSIds.leaderboard_high_score, (bool postSuccess) => {
                // handle success or failure
            });
        }
#endif
    }

    public void ReportScoreAndShowLeaderboard(int score)
    {
#if NO_GPGS
        Social.localUser.Authenticate((bool success) => 
        {
            if (success)
            {
                Social.ReportScore(score, GPGSIds.leaderboard_high_score, (bool postSuccess) => 
                {
                    PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_high_score);
                    CFirebase.LogEvent("leaderboard", "show");
                });
            }
        });
#endif
    }
}