using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health = 3;
    public GameObject[] stages;
    public Transform[] spawnPoints;
    public PlayerMove player;
    public Image[] UIHealth;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public TextMeshProUGUI UIResult;
    public GameObject UIRestartBtn;

    public bool IsFinished { get; private set; }

    private void Start()
    {
        Time.timeScale = 1;
        for (int i = 0; i < stages.Length; i++)
            stages[i].SetActive(i == stageIndex);

        UIRestartBtn.SetActive(false);
        PlayerReposition();
        UpdateUI();
    }

    public void AddPoint(int point)
    {
        if (IsFinished)
            return;

        stagePoint += point;
        UpdateUI();
    }

    public void NextStage()
    {
        if (IsFinished)
            return;

        totalPoint += stagePoint;
        stagePoint = 0;

        if (stageIndex < stages.Length - 1)
        {
            stages[stageIndex].SetActive(false);
            stageIndex++;
            stages[stageIndex].SetActive(true);
            PlayerReposition();
        }
        else
        {
            IsFinished = true;
            player.SetVelocityZero();
            UIResult.text = "GAME CLEAR";
            UIRestartBtn.SetActive(true);
            Time.timeScale = 0;
        }

        UpdateUI();
    }

    public void HealthDown()
    {
        if (IsFinished || health <= 0)
            return;

        health--;
        UpdateUI();
        if (health == 0)
        {
            IsFinished = true;
            UIResult.text = "GAME OVER";
            UIRestartBtn.SetActive(true);
            player.OnDie();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || IsFinished)
            return;

        HealthDown();
        if (health > 0)
            PlayerReposition();
    }

    private void PlayerReposition()
    {
        player.Respawn(spawnPoints[stageIndex].position);
    }

    private void UpdateUI()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
        UIStage.text = "STAGE " + (stageIndex + 1);
        for (int i = 0; i < UIHealth.Length; i++)
            UIHealth[i].color = i < health ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.35f);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
