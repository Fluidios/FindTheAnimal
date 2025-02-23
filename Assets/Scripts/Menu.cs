using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _clearSaveButton;
        [SerializeField] private Game _game;


        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            _clearSaveButton.onClick.AddListener(OnClearSaveButtonClicked);
            _game.OnGameEnds += OnGameEnds;
            SetMenu(true);
        }
        private void OnPlayButtonClicked()
        {
            _game.StartGameLevel();
            SetMenu(false);
        }
        private void OnContinueButtonClicked()
        {
            _game.LoadGameLevelFromSave();
            SetMenu(false);
        }
        private void OnClearSaveButtonClicked()
        {
            _game.ClearSave();
            SetMenu(true);
        }
        private void OnGameEnds(bool isWin)
        {
            _game.ClearGameLevel();
            SetMenu(true);
        }
        private void SetMenu(bool enabled)
        {
            _playButton.gameObject.SetActive(enabled && !_game.GameSaveExists);
            _continueButton.gameObject.SetActive(enabled && _game.GameSaveExists);
            _clearSaveButton.gameObject.SetActive(enabled && _game.GameSaveExists);
        }
    }
}
