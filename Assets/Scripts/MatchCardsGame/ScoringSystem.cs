using System.Collections;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MatchCardsGame
{
    public class ScoringSystem : MonoBehaviour
    {
        [SerializeField] private MatchCardsGame _game;
        [SerializeField] private Timer _timer;
        [SerializeField] private int _defaultScore = 100;
        [SerializeField] private float _comboTimer = 3f;

        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _comboDisplayer;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private Image _comboTimerDisplay;
        [SerializeField] private TextMeshProUGUI _addedScoreTextPrefab;
        [SerializeField] private RectTransform _canvasTransform;
        [SerializeField] private GameObject _endLevelScreen;
        [SerializeField] private TextMeshProUGUI _endLevelScoreText;
        [SerializeField] private TextMeshProUGUI _endLevelBestScoreText;

        private int _score;
        private int _combo = 1;
        private Core.Timer.TimerProcess _comboTimerProcess;
        private int SavedScore
        {
            get => PlayerPrefs.GetInt(string.Format("{0}_score", _game.GetType().Name), 0);
            set => PlayerPrefs.SetInt(string.Format("{0}_score", _game.GetType().Name), value);
        }
        private void Awake()
        {
            _game.OnGameStarts += OnGameStarts;
            _game.OnGameStartsFromSave += OnGameStartsFromSave;
            _game.OnGameEnds += OnGameEnds;

            _game.CorrectPairExposed += OnCorrectPairExposed;
            _game.WrongPairExposed += OnWrongPairExposed;
            _scoreText.gameObject.SetActive(false);
            _comboDisplayer.SetActive(false);
            StartCoroutine(UpdateComboTimerDisplay());
        }
        private void OnGameStarts()
        {
            _score = 0;
            SavedScore = 0;
            _combo = 1;
            _comboText.text = string.Format("x{0}", _combo);
            _comboTimerProcess = null;
            _comboTimerDisplay.fillAmount = 0;
            _scoreText.gameObject.SetActive(true);
            _comboDisplayer.SetActive(true);
            StartCoroutine(UpdateScore());
        }
        private void OnGameStartsFromSave()
        {
            _score = SavedScore;
            _combo = 1;
            _comboText.text = string.Format("x{0}", _combo);
            _comboTimerProcess = null;
            _comboTimerDisplay.fillAmount = 0;
            _scoreText.gameObject.SetActive(true);
            _comboDisplayer.SetActive(true);
            StartCoroutine(UpdateScore());
        }
        private void OnGameEnds(bool isWin)
        {
            var bestScore = PlayerPrefs.GetInt(string.Format("{0}_best_score", _game.GetType().Name), 0);
            if(_score > bestScore)
            {
                PlayerPrefs.SetInt(string.Format("{0}_best_score", _game.GetType().Name), _score);
                bestScore = _score;
            }

            _endLevelScreen.SetActive(true);
            _endLevelScoreText.text = string.Format("Current score: {0}", _score);
            _endLevelBestScoreText.text = string.Format("Best Score: {0}", bestScore);

            _score = 0;
            SavedScore = 0;
            _combo = 1;
            if(_comboTimerProcess != null) _comboTimerProcess.Stop();
            _comboTimerProcess = null;
            _comboTimerDisplay.fillAmount = 0;
            _comboText.text = string.Format("x{0}", _combo);
            _scoreText.gameObject.SetActive(false);
            _comboDisplayer.SetActive(false);
        }

        private void OnCorrectPairExposed(CardView c1, CardView c2)
        {
            _score += _defaultScore * _combo;
            StartCoroutine(UpdateScore());
            StartCoroutine(ShowRaisingScore(c1, _defaultScore * _combo / 2));
            StartCoroutine(ShowRaisingScore(c2, _defaultScore * _combo / 2));
            if(_comboTimerProcess == null)
            {
                _comboTimerProcess = _timer.StartNewTimer(_comboTimer);
                _comboTimerProcess.OnTimerEnds += OnComboTimerExposed;
            }
            else
            {
                _comboTimerProcess.Reset(_comboTimer);
            }
            _combo++;
            _comboText.text = string.Format("x{0}", _combo);
        }
        private void OnWrongPairExposed(CardView c1, CardView c2)
        {
            _combo = 1;
            _comboText.text = string.Format("x{0}", _combo);
            if(_comboTimerProcess != null) _comboTimerProcess.Stop();
            _comboTimerProcess = null;
            _comboTimerDisplay.fillAmount = 0;
        }
        private void OnComboTimerExposed()
        {
            _combo = 1;
            _comboText.text = string.Format("x{0}", _combo);
            if(_comboTimerProcess != null) _comboTimerProcess.Stop();
            _comboTimerProcess = null;
            _comboTimerDisplay.fillAmount = 0;
        }
        private IEnumerator UpdateScore()
        {
            _scoreText.text = _score.ToString();
            float timer = 1;
            while(timer > 0)
            {
                timer -= Time.deltaTime;
                _scoreText.rectTransform.localScale = Vector3.one * (1 + Mathf.Sin(timer * Mathf.PI));
                yield return null;
            }
        }
        private IEnumerator UpdateComboTimerDisplay()
        {
            while(true)
            {
                if(_comboTimerProcess != null)
                {
                    _comboTimerDisplay.fillAmount = _comboTimerProcess.SecondsPassed / _comboTimer;
                }
                yield return null;
            }
        }
        private IEnumerator ShowRaisingScore(CardView card, int score)
        {
            var addedScoreText = Instantiate(_addedScoreTextPrefab, _canvasTransform);
            addedScoreText.text = string.Format("+{0}", score);
            addedScoreText.gameObject.SetActive(true);
            addedScoreText.rectTransform.position = Camera.main.WorldToScreenPoint(card.transform.position);
        
            yield return new WaitForSeconds(2f);

            Destroy(addedScoreText.gameObject);
        }
    }
}
