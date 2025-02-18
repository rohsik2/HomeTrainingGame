﻿using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

enum Mode{
    READY, RUNNING, END
}
public class PlankMainController : MonoBehaviour
{
    public PlankPlayer player; // 현재 게임을 진행중인 플레이어.
    public GameObject shield;
    public SkinnedMeshRenderer hairband;

    public Text healthText;
    public Text timeText;
    public Text announceText;
    public Text positionScoreText;
    public GameObject panel;
    public GameObject button;
    public VNectBarracudaRunner runner;
    public GameObject particles;
    public LaserSoundPlayer SoundPlayer;
    public PhotonView PV;

    private Mode _gameMode;
    private float _time;
    private int _health;
    private WifiSessionController wifiController;
    private float _prevtime;
    
    public void Start()
    {
        wifiController = WifiSessionController.getInstance();
        DontDestroyOnLoad(this.gameObject);
        StartGame();
        button.SetActive(false);
    }

    public void Update()
    {

        if (_gameMode == Mode.END)
        {
            return;
        }

        if (_gameMode == Mode.RUNNING)
        {
            PlayingGame();
        }
        
        TimeChecker();
        ShieldTrackHairBand();

    }

    private void ShowVariables()
    {
        healthText.text =string.Format( "점수 : {0:F1}" , GetPlayerScore());
    }
    private void PlayingGame()
    {
        SoundPlayer.enabled = true;
        panel.SetActive(false);
        int posScore = player.GetPositionScore();
        positionScoreText.text = "플랭크 자세점수 : " + posScore;
        if((int)_prevtime != (int)_time)
            _health -= (5 - posScore);
        if (_health < 0 || posScore == -1)
        {
            EndGame();
        }
        ShowVariables();
    }
    private void TimeChecker()
    {
        _prevtime = _time;
        if (_time > 0)
        {
            _time -= Time.deltaTime;
            if(_gameMode == Mode.RUNNING)
                timeText.text = string.Format("남은시간 {0:F1}", _time);
            if(_gameMode == Mode.READY)
                timeText.text = string.Format("대기시간 {0:F1}", _time);
        }
        else if (_gameMode == Mode.RUNNING)
        {
            EndGame();
        }
        else if (_gameMode == Mode.READY)
        {
            _time = 60;
            _gameMode = Mode.RUNNING;
            announceText.text = "";
        }
    }
    
    private void StartGame()
    {
        
        panel.SetActive(true);
        announceText.text = "플랭크 자세를 취해주세요";
        healthText.text = "";
        _time = 10;
        timeText.text = _time + "s";
        _gameMode = Mode.READY;
        _health = 100;
        
        Debug.Log("Game Started");
    }

    private void EndGame()
    {
        SoundPlayer.enabled = false;

        panel.SetActive(true);
        Debug.Log("플랭크 종료");
        _gameMode = Mode.END;
        announceText.text = "플랭크 게임 종료. \n당신의 점수는 " + GetPlayerScore();
        button.SetActive(true);
        runner.enabled = false;
        particles.SetActive(false);
    }

    private void ShieldTrackHairBand()
    {
        if(_gameMode == Mode.RUNNING)
        {
            shield.SetActive(true);
            Vector3 hairBendPos = hairband.bones[0].position + Vector3.left * 0.5f;
            shield.transform.position = hairBendPos;
        } 
        else
        {
            shield.SetActive(false);
        }
    }
    
    /// <summary>
    /// 게임 결과의 점수를 float 형태로 리턴.
    /// </summary>
    /// <returns>플레이어의 스쿼트 게임 수행 점수</returns>
    public float GetPlayerScore()
    {
        return _health - _time;
    }


    /// <summary>
    /// 게임이 끝나고 누를 버튼.
    /// </summary>
    public void FinishBtnOnClick()
    {
        PV.RPC("SetScore", RpcTarget.All);
        PV.RPC("LoadBoardGame", RpcTarget.MasterClient);
        this.Invoke(() => PV.RPC("ShowResult", RpcTarget.All), 0.5f);
    }

    [PunRPC]
    private void SetScore()
    {
        float score = GetPlayerScore();
        GameManager.Instance.localPlayer.lastScore = score;
    }

    [PunRPC]
    private void LoadBoardGame()
    {
        PhotonNetwork.LoadLevel("MainBoardGame");
    }

    [PunRPC]
    private void ShowResult()
    {
        BoardGameUIManager uiManager = GameObject.Find("MainCanvas").GetComponent<BoardGameUIManager>();
        uiManager.ResultUIOpen();
        Destroy(this.gameObject);
    }
}
