using UnityEngine;

namespace Client.Gameplay
{
    public class Ship : MonoBehaviour
    {
        [SerializeField] private ParticleSystem m_SmokeParticles;
        
        [SerializeField] private ParticleSystem m_ExplosionParticles;
        [SerializeField] private int m_ExplosionParticlesMin;
        [SerializeField] private int m_ExplosionParticlesMax;
        
        public bool IsSmoking
        {
            get => m_SmokeParticles.isPlaying;
            set
            {
                if (value)
                    m_SmokeParticles.Play();
                else
                    m_SmokeParticles.Stop();
            }
        }

        public void Explode()
        {
            int randomParticlesCount = Random.Range(m_ExplosionParticlesMin, m_ExplosionParticlesMax);
            m_ExplosionParticles.Emit(randomParticlesCount);
        }
    }
}