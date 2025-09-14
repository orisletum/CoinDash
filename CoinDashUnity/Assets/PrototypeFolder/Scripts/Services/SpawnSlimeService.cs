using CoinDash.Connection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace CoinDash.Slime
{
    public class SpawnSlimeService
    {
        private SlimeView.Factory                   _slimeFactory;
        private SlimeHeader.Factory                 _slimeHeaderFactory;
        private Dictionary<SlimeView, SlimeHeader>  _nameTexts = new();
        private List<SlimeView>                     _slimes = new List<SlimeView>();
        public List<SlimeView>                      Slimes => _slimes;

        [Inject]
        public void Construct( SlimeView.Factory slimeFactory, SlimeHeader.Factory slimeHeaderFactory)
        {
            _slimeHeaderFactory = slimeHeaderFactory;
            _slimeFactory = slimeFactory;
        }

        public void AddHeader(SlimeView slime, string name)
        {
            if (_nameTexts.ContainsKey(slime)) return;
            var settings = new SlimeHeader.Settings
            {
                PlayerName = name,
                Position = slime.transform.position
            };
            var header = _slimeHeaderFactory.Create(settings);
            slime.Header = header;
            _nameTexts[slime] = header;
        }

        public void RemoveHeader(SlimeView slime)
        {
            if (_nameTexts.TryGetValue(slime, out SlimeHeader header))
            {
                header.DespawnHeader();
                _nameTexts.Remove(slime);
            }
        }
      
        public void CreateNewSlimes(PlayerState[] players)
        {
            var currentSlimeSessionIds = new HashSet<string>(_slimes.Select(s => s.SessionID));
            foreach (var player in players)
            {
                if (!currentSlimeSessionIds.Contains(player.SessionId))
                {
                    var settings = new SlimeView.Settings
                    {
                        Position = new Vector3(player.X, 0, player.Y),
                        SessionID = player.SessionId
                    };

                    var slime = _slimeFactory.Create(settings);
                    _slimes.Add(slime);
                    AddHeader(slime, player.Name);
                }
            }
        }

        public void RemoveNonExSlimes(PlayerState[] players)
        {
            var playerSessionIds = new HashSet<string>(players.Select(p => p.SessionId));
            foreach (var slime in _slimes.Where(s => !playerSessionIds.Contains(s.SessionID)).ToList())
            {
                slime.DestroySlime();
                _slimes.Remove(slime);
                RemoveHeader(slime);
            }
        }
    }
}