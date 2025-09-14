using CoinDash.Connection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace CoinDash.Coin
{
    public class UpdateCoinService
    {
        private List<CoinView>          _coinsList = new List<CoinView>();
        private  CoinView.Factory       _coinFactory;

        [Inject]
        public void Construct(CoinView.Factory coinFactory)
        {
            _coinFactory = coinFactory;
        }

        public void SpawnCoins(CoinState[] coinStates)
        {
            var coinIds = new HashSet<string>(coinStates.Select(c => c.Id));
            var currentCoinIds = new HashSet<string>(_coinsList.Select(c => c.ID));

            foreach (var coinView in _coinsList.Where(c => !coinIds.Contains(c.ID)).ToList())
            {
                coinView.DestroyCoin();
                _coinsList.Remove(coinView);
            }

            foreach (var coinState in coinStates)
                if (!currentCoinIds.Contains(coinState.Id))
                    SpawnNewCoin(coinState);

        }
        public void SpawnNewCoin(CoinState coinState)
        {
            var settings = new CoinView.Settings
            {
                Position = new Vector3(coinState.X, 0, coinState.Y),
                ID = coinState.Id

            };

            var coin = _coinFactory.Create(settings);
            _coinsList.Add(coin);
        }
    }
}