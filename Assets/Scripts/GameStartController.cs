using System;
using PassengerScripts;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    [SerializeField] private AllPassengerController _suitcasePassengerController;
    [SerializeField] private AllPassengerController _passportPassengerController;
    [SerializeField] private XRayGame _xRayGame;
    private bool gameStarted = false;


    public void StartGame()
    {
        if (!gameStarted)
        {
            _xRayGame.StartMovingCase();
            _suitcasePassengerController.StartGame();
            _passportPassengerController.StartGame();
            gameStarted = true; 
        }
    }

    private void Update()
    {
        if(!gameStarted)
            if(Input.GetKeyDown(KeyCode.Return))StartGame();
    }
}
