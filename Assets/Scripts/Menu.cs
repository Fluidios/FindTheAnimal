using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Game _game;


        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _game.OnGameEnds += OnGameEnds;
        }
        private void OnPlayButtonClicked()
        {
            _game.StartGameLevel();
            _playButton.gameObject.SetActive(false);
        }
        private void OnGameEnds(bool isWin)
        {
            if(!isWin) _game.ClearGameLevel();
            _playButton.gameObject.SetActive(true);
        }
    }
}
