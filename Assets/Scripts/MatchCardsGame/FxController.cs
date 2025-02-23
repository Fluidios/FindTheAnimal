using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchCardsGame
{
    public class FxController : MonoBehaviour
    {
        [SerializeField] private MatchCardsGame _game;
        [SerializeField] private Color _correctPairColor;
        [SerializeField] private Color _wrongPairColor;

        private void Awake()
        {
            _game.CorrectPairExposed += OnCorrectCardsPairExposed;
            _game.WrongPairExposed += OnWrongCardsPairExposed;
        }

        private void OnCorrectCardsPairExposed(CardView c1, CardView c2)
        {
            var settingsBlock = c1.Fx.main;
            settingsBlock.startColor = _correctPairColor;
            StartCoroutine(PlayFx(c1.Fx));

            settingsBlock = c2.Fx.main;
            settingsBlock.startColor = _correctPairColor;
            StartCoroutine(PlayFx(c2.Fx));
        }
        private void OnWrongCardsPairExposed(CardView c1, CardView c2)
        {
            var settingsBlock = c1.Fx.main;
            settingsBlock.startColor = _wrongPairColor;
            StartCoroutine(PlayFx(c1.Fx));

            settingsBlock = c2.Fx.main;
            settingsBlock.startColor = _wrongPairColor;
            StartCoroutine(PlayFx(c2.Fx));
        }

        private IEnumerator PlayFx(ParticleSystem fx)
        {
            yield return new WaitForSeconds(0.5f); //give time for the card rotation animation
            if(fx != null) fx.Play();
        }
    }
}
