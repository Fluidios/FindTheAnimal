using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace MatchCardsGame
{
    public class MatchCardsGame : Game
    {
        [SerializeField] private CardView _cardViewPrefab;
        [SerializeField] private GameObject[] _cards;
        [SerializeField] private Transform _cardsParent;
        [SerializeField] private float _cardMaxSize = 1.0f;
        [SerializeField] private Vector2Int _levelSize = Vector2Int.one * 2;

        public Action<CardView, CardView> CorrectPairExposed;
        public Action<CardView, CardView> WrongPairExposed;
        public Action<CardView> CardExposed;
        public Action<CardView> CardHidden;
        private CardView[,] _cardViews;
        private CardView _exposedCard;
        private int _exposedCardPairsCount = 0;
        private Dictionary<string, List<Vector2Int>> _cardPairPositions = new Dictionary<string, List<Vector2Int>>();

        public override bool GameSaveExists 
        {
            get => PlayerPrefs.HasKey("MatchCardsGameSaved");
        }
        private Vector2Int GetSavedLevelSize()
        {
            var key = "MatchCardsGameSavedLevelWidth";
            if(PlayerPrefs.HasKey(key))
            {
                Vector2Int size = Vector2Int.zero;
                size.x = PlayerPrefs.GetInt(key);   
                key = "MatchCardsGameSavedLevelHeight";
                if(PlayerPrefs.HasKey(key))
                {
                    size.y = PlayerPrefs.GetInt(key);
                }
                return size;
            }
            return Vector2Int.zero;
        }
        private string GetSavedCard(Vector2Int position)
        {
            var key = string.Format("MatchCardsGameSavedCard[{0},{1}]", position.x, position.y);
            if(PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }
            return string.Empty;
        }
        public override void StartGameLevel()
        {
            _exposedCardPairsCount = 0;
            if(GenerateLevel(_levelSize))
            {
                StartCoroutine(HideAllCardsAtStart());
                PlayerPrefs.SetInt("MatchCardsGameSaved", 1);
                //save level size
                PlayerPrefs.SetInt("MatchCardsGameSavedLevelWidth", _levelSize.x);
                PlayerPrefs.SetInt("MatchCardsGameSavedLevelHeight", _levelSize.y);
                //save cards
                for(int y = 0; y < _levelSize.y; y++)
                {
                    for(int x = 0; x < _levelSize.x; x++)
                    {
                        PlayerPrefs.SetString(string.Format("MatchCardsGameSavedCard[{0},{1}]", x, y), _cardViews[x, y].CardName);
                        if(!_cardPairPositions.ContainsKey(_cardViews[x, y].CardName))_cardPairPositions.Add(_cardViews[x, y].CardName, new List<Vector2Int>());
                        _cardPairPositions[_cardViews[x, y].CardName].Add(new Vector2Int(x, y));
                    }
                }
                OnGameStarts?.Invoke();
            }
            else 
            {
                //in case the level generation failed, end the game as loose
                OnGameEnds?.Invoke(false);
            }
        }
        public override void LoadGameLevelFromSave()
        {
            if(GameSaveExists)
            {
                //load level size & init
                _levelSize = GetSavedLevelSize();
                _cardViews = new CardView[_levelSize.x, _levelSize.y];
                float appropriateCardSize = _cardMaxSize * 2 / Mathf.Max(_levelSize.x, _levelSize.y); //minimal game is 2x2, any size should match into the size of 2x2 game, by scalind cards down
                Vector2 cardsOrigin = new Vector2(-appropriateCardSize * (_levelSize.x/2), -appropriateCardSize * (_levelSize.y/2));
                if(_levelSize.x % 2 == 0) cardsOrigin += Vector2.right * appropriateCardSize/2;
                if(_levelSize.y % 2 == 0) cardsOrigin += Vector2.up * appropriateCardSize/2;
                //fill dictionary to handle fast access to card variants by name
                Dictionary<string, GameObject> cardVariants = new Dictionary<string, GameObject>();
                foreach(var card in _cards) cardVariants.Add(card.name, card);
                //generate level, skip already opened card pairs
                for(int y = 0; y < _levelSize.y; y++)
                {
                    for(int x = 0; x < _levelSize.x; x++)
                    {
                        var savedCard = GetSavedCard(new Vector2Int(x, y));
                        if(string.IsNullOrEmpty(savedCard) || !cardVariants.ContainsKey(savedCard)) continue; //save break or this card pair already was found by player
                        _cardViews[x, y] = Instantiate(_cardViewPrefab, Vector3.zero, Quaternion.identity, _cardsParent);
                        _cardViews[x, y].SetCardContent(cardVariants[savedCard]);
                        _cardViews[x, y].SetCardSize(appropriateCardSize);
                        _cardViews[x, y].transform.localPosition = new Vector3(cardsOrigin.x + x * appropriateCardSize, cardsOrigin.y + y * appropriateCardSize, 0);
                        _cardViews[x, y].OnCardExposed += OnCardExposed;
                        
                        if(!_cardPairPositions.ContainsKey(_cardViews[x, y].CardName))_cardPairPositions.Add(_cardViews[x, y].CardName, new List<Vector2Int>());
                        _cardPairPositions[_cardViews[x, y].CardName].Add(new Vector2Int(x, y));
                    }
                }
                StartCoroutine(HideAllCardsAtStart());
                OnGameStartsFromSave?.Invoke();
            }
            else 
            {
                //in case the level save not exist, end the game as loose
                OnGameEnds?.Invoke(false);
            }
        }
        public override void ClearGameLevel()
        {
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < _cardsParent.childCount; i++)
            {
                children.Add(_cardsParent.GetChild(i).gameObject);
            }
            while (children.Count > 0)
            {
                Destroy(children[0]);
                children.RemoveAt(0);
            }
            _exposedCardPairsCount = 0;
            PlayerPrefs.DeleteKey("MatchCardsGameSaved");
        }
        private bool GenerateLevel(Vector2Int levelSize)
        {
            //check if _cards variants is enough for the level
            if((_cards.Length * 2) < levelSize.x * levelSize.y)
            {
                Debug.LogError("Not enough card variant for the level");
                return false;
            }
            //check if required cards amount is even
            if((levelSize.x * levelSize.y) % 2 != 0)
            {
                Debug.LogError("Required cards amount should be even");
                return false;
            }

            //init
            _cardViews = new CardView[levelSize.x, levelSize.y];
            float appropriateCardSize = _cardMaxSize * 2 / Mathf.Max(levelSize.x, levelSize.y); //minimal game is 2x2, any size should match into the size of 2x2 game, by scalind cards down
            Vector2 cardsOrigin = new Vector2(-appropriateCardSize * (levelSize.x/2), -appropriateCardSize * (levelSize.y/2));
            if(levelSize.x % 2 == 0) cardsOrigin += Vector2.right * appropriateCardSize/2;
            if(levelSize.y % 2 == 0) cardsOrigin += Vector2.up * appropriateCardSize/2;
            int i = -1;

            // Shuffle the _cards array
            GameObject tempCardContent;
            for (int t = 0; t < _cards.Length; t++)
            {
                tempCardContent = _cards[t];
                int r = UnityEngine.Random.Range(t, _cards.Length);
                _cards[t] = _cards[r];
                _cards[r] = tempCardContent;
            }

            //spawn cards level
            for(int y = 0; y < levelSize.y; y++)
            {
                for(int x = 0; x < levelSize.x; x++)
                {
                    if((levelSize.x*y + x) % 2 == 0)
                    {
                        i++;
                    }
                    _cardViews[x, y] = Instantiate(_cardViewPrefab, Vector3.zero, Quaternion.identity, _cardsParent);
                    _cardViews[x, y].SetCardContent(_cards[i]);
                    _cardViews[x, y].SetCardSize(appropriateCardSize);
                    _cardViews[x, y].transform.localPosition = new Vector3(cardsOrigin.x + x * appropriateCardSize, cardsOrigin.y + y * appropriateCardSize, 0);
                    _cardViews[x, y].OnCardExposed += OnCardExposed;
                }
            }
            
            // Shuffle the cards level
            int randomX, randomY;
            CardView tempCardView;
            Vector3 tempPosition;
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    randomX = UnityEngine.Random.Range(0, levelSize.x);
                    randomY = UnityEngine.Random.Range(0, levelSize.y);

                    // Swap the elements
                    tempCardView = _cardViews[x, y];
                    tempPosition = _cardViews[x, y].transform.localPosition;
                    _cardViews[x, y].transform.localPosition = _cardViews[randomX, randomY].transform.localPosition;
                    _cardViews[x, y] = _cardViews[randomX, randomY];
                    _cardViews[randomX, randomY].transform.localPosition = tempPosition;
                    _cardViews[randomX, randomY] = tempCardView;
                }
            }
            return true;
        }
        private void OnCardExposed(CardView cardView)
        {
            CardExposed?.Invoke(cardView);
            if(_exposedCard == null)
            {
                _exposedCard = cardView;
            }
            else
            {
                if(_exposedCard.CardName == cardView.CardName)
                {
                    CorrectPairExposed?.Invoke(_exposedCard, cardView);
                    StartCoroutine(DestroyExposedCardsPair(_exposedCard, cardView));
                    _exposedCardPairsCount++;
                    //remove cards from save
                    Vector2Int cardPosition = _cardPairPositions[_exposedCard.CardName][0]; //asuming that there are only 2 cards with the same name on 1 level
                    PlayerPrefs.DeleteKey(string.Format("MatchCardsGameSavedCard[{0},{1}]", cardPosition.x, cardPosition.y));
                    cardPosition = _cardPairPositions[cardView.CardName][1];
                    PlayerPrefs.DeleteKey(string.Format("MatchCardsGameSavedCard[{0},{1}]", cardPosition.x, cardPosition.y));
                    _cardPairPositions[cardView.CardName].Clear();
                    _cardPairPositions.Remove(cardView.CardName);
                }
                else
                {
                    WrongPairExposed?.Invoke(_exposedCard, cardView);
                    StartCoroutine(HideExposedCardsPair(_exposedCard, cardView));
                }
                
                _exposedCard = null;
            }
        }
        private IEnumerator HideAllCardsAtStart()
        {
            yield return new WaitForSeconds(1);
            foreach(var card in _cardViews) {
                if(card == null) continue;
                card.Hide(true);
                CardHidden?.Invoke(card);
            }
        }
        private IEnumerator HideExposedCardsPair(CardView card1, CardView card2)
        {
            yield return new WaitForSeconds(1);//give time to show exposed cards
            card1.Hide();
            card2.Hide();
            CardHidden?.Invoke(card1);
            CardHidden?.Invoke(card2);
        }
        private IEnumerator DestroyExposedCardsPair(CardView card1, CardView card2)
        {
            yield return new WaitForSeconds(1);//give time to show exposed cards
            Destroy(card1.gameObject);
            Destroy(card2.gameObject);
            if(_cardPairPositions.Count <= 0)
            {
                OnGameEnds?.Invoke(true);
            }
        }

        public override void ClearSave()
        {
            PlayerPrefs.DeleteKey("MatchCardsGameSaved");
            int width = 0, height = 0;
            if(PlayerPrefs.HasKey("MatchCardsGameSavedLevelWidth"))
            {
                width = PlayerPrefs.GetInt("MatchCardsGameSavedLevelWidth");
                PlayerPrefs.DeleteKey("MatchCardsGameSavedLevelWidth");
            }
            if(PlayerPrefs.HasKey("MatchCardsGameSavedLevelHeight"))
            {
                height = PlayerPrefs.GetInt("MatchCardsGameSavedLevelHeight");
                PlayerPrefs.DeleteKey("MatchCardsGameSavedLevelHeight");
            }
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    PlayerPrefs.DeleteKey(string.Format("MatchCardsGameSavedCard[{0},{1}]", x, y));
                }
            }
        }
    }
}
