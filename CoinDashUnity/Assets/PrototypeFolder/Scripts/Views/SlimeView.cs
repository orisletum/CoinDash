using CoinDash.Slime;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace CoinDash.Slime
{
    public class SlimeView : MonoBehaviour, IPoolable<SlimeView.Settings, IMemoryPool>
    {
        [SerializeField] private string     _score;
        private IMemoryPool                 _pool;

        [HideInInspector]
        public SlimeHeader                  Header;
        public string                       SessionID;

        public void DestroySlime() => _pool.Despawn(this);

        public void SetCurrentScore(int s) => _score = $"Score: {s}";

        public void SetNewPosition(float x, float y)
        {
            var vec = new Vector3(x, 0, y);
            transform.DOMove(vec, 0.3f);
            var vecHeader = vec;
            vecHeader.y += 2f;
            Vector3 curDirection = vec - transform.position;
            float targetAngle = Mathf.Atan2(curDirection.x, curDirection.z) * Mathf.Rad2Deg;
            transform.DOLocalRotate(new Vector3(0, targetAngle, 0), 0.3f);
            Header.transform.DOMove(vecHeader, 0.3f);

        }

        public void OnSpawned(Settings settings, IMemoryPool pool)
        {
            _pool = pool;
            SessionID = settings.SessionID;
            SetNewPosition(settings.Position.x, settings.Position.z);
        }

        public void OnDespawned() => gameObject.SetActive(false);

        private void OnDisable()
        {
            Header = null;
            _pool = null;
        }

        public class Factory : PlaceholderFactory<Settings, SlimeView> { }

        public class Settings
        {
            public string SessionID;
            public Vector3 Position;
        }
    }
}