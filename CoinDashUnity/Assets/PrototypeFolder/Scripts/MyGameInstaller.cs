using CoinDash.Coin;
using CoinDash.Connection;
using CoinDash.Slime;
using CoinDash.UI;
using UnityEngine;
using Zenject;

namespace CoinDash
{
    public class MyGameInstaller : MonoInstaller
    {
        private string _worldCanvas = "WorldCanvas";
        private string _header = "Prefabs/Header";
        private string _coin = "Prefabs/Coin";
        private string _slime = "Prefabs/Slime";
        private string _gameView = "Prefabs/GameView";
        private string _joystiñk = "Prefabs/Joystick";
        public override void InstallBindings()
        {

            var worldCanvas = GameObject.FindGameObjectWithTag(_worldCanvas);

            Container.BindFactory<SlimeHeader.Settings, SlimeHeader, SlimeHeader.Factory>()
            .FromMonoPoolableMemoryPool(
                x => x.WithInitialSize(1)
                    .FromComponentInNewPrefabResource(_header)
                    .UnderTransform(worldCanvas.transform));

            Container.BindFactory<SlimeView.Settings, SlimeView, SlimeView.Factory>()
             .FromMonoPoolableMemoryPool(
                 x => x.WithInitialSize(1)
                     .FromComponentInNewPrefabResource(_slime)
                     .UnderTransformGroup("SlimePool"));

            Container.BindFactory<CoinView.Settings, CoinView, CoinView.Factory>()
              .FromMonoPoolableMemoryPool(
                  x => x.WithInitialSize(1)
                      .WithMaxSize(10)
                      .FromComponentInNewPrefabResource(_coin)
                      .UnderTransformGroup("CoinPool"));

            Container.Bind<JoystiñkForMovement>().FromComponentInNewPrefabResource(_joystiñk).AsSingle().NonLazy();
            Container.Bind<GameView>().FromComponentInNewPrefabResource(_gameView).AsSingle().NonLazy();

            Container.Bind<SpawnSlimeService>().AsSingle().NonLazy();
            Container.Bind<UpdateSlimeService>().AsSingle().NonLazy();
            Container.Bind<UpdateCoinService>().AsSingle().NonLazy();
            Container.Bind<WebSocketConnectionService>().AsSingle().NonLazy();
            Container.Bind<ServerConnectionService>().AsSingle().NonLazy();
        }
    }
}