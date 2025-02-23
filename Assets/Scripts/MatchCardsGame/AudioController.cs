using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchCardsGame
{
    public class AudioController : MonoBehaviour
    {
        [SerializeField] private MatchCardsGame _game;
        [SerializeField] private AudioSource _sourcePrefab;
        [SerializeField] private AudioClip _correctPairClip;
        [SerializeField] private AudioClip _wrongPairClip;
        [SerializeField] private AudioClip _winClip;
        [SerializeField] private AudioClip _loseClip;
        [SerializeField] private AudioClip _exposeCardClip;
        private void Awake()
        {
            _game.CorrectPairExposed += (c1, c2) => PlayAudio(_correctPairClip);
            _game.WrongPairExposed += (c1, c2) => PlayAudio(_wrongPairClip);
            _game.OnGameEnds += (isWin) => PlayAudio(isWin ? _winClip : _loseClip);
            _game.CardExposed += (c) => PlayAudio(_exposeCardClip);
            _game.CardHidden += (c) => PlayAudio(_exposeCardClip, Random.Range(0, 0.5f));
        }

        private void PlayAudio(AudioClip clip, float delay = 0)
        {
            AudioSource source = Instantiate(_sourcePrefab, transform);
            source.clip = clip;
            StartCoroutine(PlaySource(source));
        }
        private IEnumerator PlaySource(AudioSource source, float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            source.Play();
            yield return new WaitForSeconds(source.clip.length);
            Destroy(source.gameObject);
        }
    }
}
