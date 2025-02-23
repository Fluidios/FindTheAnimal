using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchCardsGame
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private Transform _cardContentTransform;
        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _fx;
        private bool _cardIsExposed = true;
        private bool _rejectClicks = false;
        public Action<CardView> OnCardExposed;
        public string CardName { get; private set; }
        public ParticleSystem Fx => _fx;

        public void SetCardContent(GameObject cardContent)
        {
            CardName = cardContent.name;
            Instantiate(cardContent, _cardContentTransform);
        }
        public void SetCardSize(float size)
        {
            transform.localScale = new Vector3(size, size, 1);
            var shape = _fx.shape;
            shape.scale = new Vector3(size, size, 1);
        }
        public void Show()
        {
            _animator.SetTrigger("Show");
            StartCoroutine(HandleClickRejectingWhileAnimating());
            _cardIsExposed = true;
            OnCardExposed?.Invoke(this);
        }
        public void Hide(bool longAnimation = false)
        {
            if (longAnimation)
            {
                _animator.SetTrigger("HideLong");
            }
            else
            {
                _animator.SetTrigger("Hide");
            }
            StartCoroutine(HandleClickRejectingWhileAnimating());
            _cardIsExposed = false;
        }
        public void OnMouseDown()
        {
            if(_rejectClicks) return;
            if(!_cardIsExposed) Show();
        }
        private IEnumerator HandleClickRejectingWhileAnimating()
        {
            _rejectClicks = true;
            var currentAnimationClip = _animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            yield return new WaitForSeconds(currentAnimationClip.length);
            _rejectClicks = false;
        }
    }
}
