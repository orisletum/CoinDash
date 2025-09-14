using TMPro;
using UnityEngine;
using Zenject;

namespace CoinDash.Slime
{
    public class SlimeHeader : MonoBehaviour, IPoolable<SlimeHeader.Settings, IMemoryPool>
    {
        [SerializeField] private TMP_Text   _nameText;
        private IMemoryPool                 _pool;
        private void Awake()
        {
            if (_nameText == null) _nameText = GetComponent<TMP_Text>();  
        }

        public void OnSpawned(Settings settings, IMemoryPool pool)
        {
            _pool = pool;
            _nameText.text = settings.PlayerName;
            transform.position = new Vector3(settings.Position.x, 2 , settings.Position.z);
        }

        public void OnDespawned() => gameObject.SetActive(false);

        public void DespawnHeader() => _pool.Despawn(this);

        private void OnDisable()
        {
            _pool = null;
        }

        public class Factory : PlaceholderFactory<Settings, SlimeHeader> { }

        public class Settings
        {
            public string PlayerName;
            public Vector3 Position;
            public Transform Parent;
        }
    }
}