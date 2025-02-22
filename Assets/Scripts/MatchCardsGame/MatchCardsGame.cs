using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchCardsGame : Game
{
    [SerializeField] private CardView _cardViewPrefab;
    [SerializeField] private GameObject[] _cards;
    [SerializeField] private Transform _cardsParent;
    [SerializeField] private float _cardMaxSize = 1.0f;
    [SerializeField] private Vector2Int _levelSize = Vector2Int.one * 2;

    private CardView[,] _cardViews;
    private CardView _exposedCard;
    private int _exposedCardPairsCount = 0;

    public override void StartGameLevel()
    {
        _exposedCardPairsCount = 0;
        GenerateLevel(_levelSize);
        StartCoroutine(HideAllCardsAtStart());
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
    }
    private void GenerateLevel(Vector2Int levelSize)
    {
        //check if _cards variants is enough for the level
        if((_cards.Length * 2) < levelSize.x * levelSize.y)
        {
            Debug.LogError("Not enough card variant for the level");
            return;
        }
        //check if required cards amount is even
        if((levelSize.x * levelSize.y) % 2 != 0)
        {
            Debug.LogError("Required cards amount should be even");
            return;
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
            int r = Random.Range(t, _cards.Length);
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
                randomX = Random.Range(0, levelSize.x);
                randomY = Random.Range(0, levelSize.y);

                // Swap the elements
                tempCardView = _cardViews[x, y];
                tempPosition = _cardViews[x, y].transform.localPosition;
                _cardViews[x, y].transform.localPosition = _cardViews[randomX, randomY].transform.localPosition;
                _cardViews[x, y] = _cardViews[randomX, randomY];
                _cardViews[randomX, randomY].transform.localPosition = tempPosition;
                _cardViews[randomX, randomY] = tempCardView;
            }
        }
    }
    private void OnCardExposed(CardView cardView)
    {
        if(_exposedCard == null)
        {
            _exposedCard = cardView;
        }
        else
        {
            if(_exposedCard.CardName == cardView.CardName)
            {
                StartCoroutine(DestroyExposedCardsPair(_exposedCard, cardView));
                _exposedCardPairsCount++;
                if(_exposedCardPairsCount == _cardViews.Length/2)
                {
                    OnGameEnds?.Invoke(true);
                }
            }
            else
            {
                StartCoroutine(HideExposedCardsPair(_exposedCard, cardView));
            }
            
            _exposedCard = null;
        }
    }
    private IEnumerator HideAllCardsAtStart()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("HideAllCardsAtStart");
        foreach(var card in _cardViews) card.Hide(true);
    }
    private IEnumerator HideExposedCardsPair(CardView card1, CardView card2)
    {
        yield return new WaitForSeconds(1);//give time to show exposed cards
        card1.Hide();
        card2.Hide();
    }
    private IEnumerator DestroyExposedCardsPair(CardView card1, CardView card2)
    {
        yield return new WaitForSeconds(1);//give time to show exposed cards
        Destroy(card1.gameObject);
        Destroy(card2.gameObject);
    }
}
