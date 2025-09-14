using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace CoinDash.Coin
{
    public class CoinView : MonoBehaviour, IPoolable<CoinView.Settings, IMemoryPool>
    {
        private IDisposable     _subscription;
        private float           _rotationSpeed = 360;
        private IMemoryPool     _pool;

        public string           ID;


        void OnEnable()
        {
            Vector3 direction = new Vector3(1, 1, 2);
            _subscription = Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    transform.RotateAround(
                        transform.position,
                        direction,
                        _rotationSpeed * Time.deltaTime
                    );
                });
        }
        public void OnSpawned(Settings settings, IMemoryPool memoryPool)
        {
            _pool = memoryPool;
            ID = settings.ID;
            SetNewPosition(settings.Position.x, settings.Position.z);
            gameObject.SetActive(true);
        }

        public void OnDespawned() => gameObject.SetActive(false);

        public void DestroyCoin() => _pool?.Despawn(this);

        public void SetNewPosition(float x, float z) => transform.position = new Vector3(x, 0, z);

        private void OnDisable()
        {
            _pool = null;
            _subscription?.Dispose();
            _subscription = null;
        }

        public class Settings
        {
            public string ID;
            public Vector3 Position;
        }

        public class Factory : PlaceholderFactory<Settings, CoinView> { }
    }
}