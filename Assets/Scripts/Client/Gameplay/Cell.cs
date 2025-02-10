using UnityEngine;

namespace Client.Gameplay
{
    public class Cell : MonoBehaviour
    {
        private static readonly int ColorPropertyID = Shader.PropertyToID("_BaseColor");
        
        [SerializeField] private Color m_NormalColor;
        [SerializeField] private Color m_SelectedColor;

        [SerializeField, Range(0.0f, 1.0f)] private float m_NormalAlpha;
        [SerializeField, Range(0.0f, 1.0f)] private float m_HoveredAlpha;

        [SerializeField] private Renderer m_Renderer;
        
        private MaterialPropertyBlock m_PropertyBlock;

        public Vector2Int Position { get; set; }
        public Battleground Owner { get; set; }
        
        private bool m_Selected;
        public bool Selected
        {
            get => m_Selected;
            set
            {
                if (m_Selected == value)
                    return;
                
                m_Selected = value;
                Refresh();
            }
            
        }

        private bool m_Hovered;
        public bool Hovered
        {
            get => m_Hovered;
            set
            {
                if (m_Hovered == value)
                    return;

                m_Hovered = value;
                Refresh();
            }
        }
        
        private void Awake()
        {
            m_PropertyBlock = new MaterialPropertyBlock();
        }
        
        public void SetSize(Vector2 size)
        {
            transform.localScale = new Vector3(size.x, 1.0f, size.y);
        }

        public void Refresh()
        {
            var color = Selected ? m_SelectedColor : m_NormalColor;
            var alpha = Hovered ? m_HoveredAlpha : m_NormalAlpha;
            
            SetColor(new Color(color.r, color.g, color.b, alpha));
        }
        
        private void SetColor(Color color)
        {
            m_Renderer.GetPropertyBlock(m_PropertyBlock);
            m_PropertyBlock.SetColor(ColorPropertyID, color);
            m_Renderer.SetPropertyBlock(m_PropertyBlock);
        }
    }
}