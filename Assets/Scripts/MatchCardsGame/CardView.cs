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
        private bool _cardIsExposed = false;
        public Action<CardView> OnCardExposed;
        public string CardName { get; private set; }

        public void SetCardContent(GameObject cardContent)
        {
            CardName = cardContent.name;
            Instantiate(cardContent, _cardContentTransform);
        }
        public void SetCardSize(float size)
        {
            transform.localScale = new Vector3(size, size, 1);
        }
        public void Show()
        {
            _animator.SetTrigger("Show");
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
            _cardIsExposed = false;
        }
        public void OnMouseDown()
        {
            if(!_cardIsExposed) Show();
        }
    }
}
