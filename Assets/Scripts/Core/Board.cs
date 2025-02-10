using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class Board
    {
        private readonly CellState[,] m_Cells;

        public int Height => m_Cells.GetLength(0);
        public int Width => m_Cells.GetLength(1);
    
        public Board(int width, int height)
        {
            m_Cells = new CellState[height, width];
        }

        public bool IsAliveShipInCell(int x, int y)
        {
            return GetCellState(x, y) == CellState.Ship;
        }

        public bool IsDestroyedShipInCell(int x, int y)
        {
            return GetCellState(x, y) == CellState.DestroyedShip;
        }

        public bool IsCellEmpty(int x, int y)
        {
            return GetCellState(x, y) == CellState.Empty;
        }

        public bool PlaceShip(int x, int y)
        {
            CellState currentCellState = GetCellState(x, y);
            if (currentCellState != CellState.Empty)
                return false;
        
            SetCellState(x, y, CellState.Ship);
            return true;
        }

        public bool AttackCell(int x, int y)
        {
            CellState currentCellState = GetCellState(x, y);
            if (currentCellState != CellState.Ship)
                return false;
        
            SetCellState(x, y, CellState.DestroyedShip);
            return true;
        }

        public bool HasAliveShips()
        {
            return ForeachCell().Any(cellState => cellState == CellState.Ship);
        }

        public CellState At(int x, int y)
        {
            return GetCellState(x, y);
        }
    
        private CellState GetCellState(int x, int y)
        {
            return m_Cells[y, x];
        }

        private void SetCellState(int x, int y, CellState newCellState)
        {
            m_Cells[y, x] = newCellState;
        }

        private IEnumerable<CellState> ForeachCell()
        {
            int width = Width;
            int height = Height;
        
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                yield return m_Cells[y, x];
            }
        }
    }
}