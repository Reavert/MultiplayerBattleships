using System;
using UnityEngine;

namespace Client.Gameplay
{
    public class Battleground : MonoBehaviour
    {
        [SerializeField] private Cell m_CellPrefab;
        [SerializeField] private Ship m_ShipPrefab;
        [SerializeField] private ParticleSystem m_WaterSplashPrefab;
        
        [SerializeField] private Vector2 m_CellSize;
        [SerializeField] private LayerMask m_CellsLayerMask;

        private Vector2 m_CellHalfSize;
        private Vector2 m_TotalSize;

        private bool m_Created;

        private int m_Width;
        private int m_Height;

        private Ship[] m_Ships;
        private Cell[] m_CellPreviews;

        private Ray m_Ray;
        private bool m_IsRaySelective;
        private readonly RaycastHit[] m_CellRaycastHits = new RaycastHit[1];

        private bool m_Interactive;

        public bool Interactive
        {
            get => m_Interactive;
            set
            {
                m_Interactive = value;
                RefreshCellsVisibility();
            }
        }

        public Vector2 CellSize => m_CellSize;
        public Vector2 TotalSize => m_TotalSize;

        public event Action<Cell> CellSelected;
        
        public void Create(int width, int height)
        {
            if (m_Created)
                return;

            m_Width = width;
            m_Height = height;

            m_CellPreviews = new Cell[width * height];
            m_Ships = new Ship[width * height];

            m_CellHalfSize = m_CellSize / 2.0f;
            m_TotalSize = m_CellSize * new Vector2(width, height);

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var position = new Vector2Int(x, y);

                Cell cell = Instantiate(m_CellPrefab);
                cell.SetSize(m_CellSize);
                cell.Selected = false;
                cell.Hovered = false;
                cell.Position = position;
                cell.Owner = this;
                
                m_CellPreviews[y * width + x] = cell;

                Place(cell.transform, position);
            }

            m_Created = true;
        }

        public void SendInteractionRay(Ray ray, bool selective)
        {
            if (!m_Interactive)
                return;
            
            m_Ray = ray;
            m_IsRaySelective = selective;

            Cell hitCell = null;

            int hits = Physics.RaycastNonAlloc(m_Ray, m_CellRaycastHits, 100.0f, m_CellsLayerMask);
            
            if (hits > 0) 
                m_CellRaycastHits[0].transform.TryGetComponent(out hitCell);
            
            if (hitCell != null && selective)
                CellSelected?.Invoke(hitCell);
            
            foreach (var cell in m_CellPreviews)
            {
                if (cell == hitCell)
                {
                    if (m_IsRaySelective)
                        cell.Selected = !cell.Selected;
                }

                cell.Hovered = cell == hitCell;
            }
            
        }

        public Cell[] GetCells()
        {
            return m_CellPreviews;
        }

        public Ship PlaceShip(Vector2Int cellPosition)
        {
            int index = CalculateCellArrayIndex(cellPosition);
            var ship = m_Ships[index];
            if (ship != null)
                return ship;

            ship = Instantiate(m_ShipPrefab, transform);
            ship.IsSmoking = false;
            
            Place(ship.transform, cellPosition);

            m_Ships[index] = ship;

            return ship;
        }

        public void RemoveShip(Vector2Int cellPosition)
        {
            int index = CalculateCellArrayIndex(cellPosition);
            var ship = m_Ships[index];
            if (ship == null)
                return;

            Destroy(ship.gameObject);
            
            m_Ships[index] = null;
        }

        public void WaterSplash(Vector2Int cellPosition)
        {
            var waterSplash = Instantiate(m_WaterSplashPrefab, transform);
            waterSplash.transform.position = GetCellWorldCenter(cellPosition);
        }
        
        public Vector3 GetCellWorldCenter(Vector2Int cellPosition)
        {
            var localPosition = cellPosition * CellSize;
            return transform.position + new Vector3(
                x: localPosition.x - m_TotalSize.x / 2.0f + m_CellHalfSize.x,
                y: 0.0f,
                z: localPosition.y - m_TotalSize.y / 2.0f + m_CellHalfSize.y);
        }
        
        private int CalculateCellArrayIndex(Vector2Int cellPosition)
        {
            return cellPosition.y * m_Width + cellPosition.x;
        }
        
        private void Place(Transform targetTransform, Vector2Int cellPosition)
        {
            targetTransform.SetParent(transform);

            var localPosition = cellPosition * CellSize;
            targetTransform.localPosition = new Vector3(
                x: localPosition.x - m_TotalSize.x / 2.0f + m_CellHalfSize.x,
                y: 0.0f,
                z: localPosition.y - m_TotalSize.y / 2.0f + m_CellHalfSize.y);
        }
        
        private void RefreshCellsVisibility()
        {
            foreach (Cell cellPreview in m_CellPreviews) 
                cellPreview.gameObject.SetActive(m_Interactive);
        }
    }
}
