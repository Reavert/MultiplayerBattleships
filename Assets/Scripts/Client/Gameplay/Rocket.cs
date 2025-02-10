using UnityEngine;

namespace Client.Gameplay
{
    public class Rocket : MonoBehaviour
    {
        private Vector3 m_Source;
        private Vector3 m_Destination;
        private float m_Height;
        
        public void SetPath(Vector3 source, Vector3 destination, float height)
        {
            m_Source = source;
            m_Destination = destination;
            m_Height = height;
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            
            // Parabola argument.
            float x = Mathf.Lerp(-1.0f, 1.0f, progress);
            
            // Calculate height from parabola equation.
            float height = -(m_Height * x * x) + m_Height;
            
            transform.position = new Vector3(
                Mathf.Lerp(m_Source.x, m_Destination.x, progress),
                height,
                Mathf.Lerp(m_Source.z, m_Destination.z, progress));
            
            // Calculate tilt from parabola derivative.
            float tilt = Mathf.Atan(2.0f * x);
            
            transform.rotation = Quaternion.LookRotation(m_Destination - m_Source,  Vector3.up) * 
                                 Quaternion.Euler(tilt * Mathf.Rad2Deg, 0.0f, 0.0f);
        }
    }
}