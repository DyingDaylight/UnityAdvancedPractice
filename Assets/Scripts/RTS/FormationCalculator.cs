using System.Collections.Generic;
using UnityEngine;

namespace RTS
{
    public static class FormationCalculator
    {
        public static List<Vector3> CalculatePositions(Vector3 center, int unitCount, float spacing)
        {
            if (unitCount <= 0)
                return new  List<Vector3>();
            
            List<Vector3> positions = new(unitCount);
            
            int columns = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
            int rows = Mathf.CeilToInt((float) unitCount / columns);

            for (int row = 0; row < rows; row++)
            {
                int unitsInRow = Mathf.Min(columns, unitCount - row * columns);

                for (int col = 0; col < unitsInRow; col++)
                {
                    float x = (col - (unitsInRow - 1) / 2f) * spacing;
                    float z = (row - (rows - 1) / 2f) * spacing;
                    
                    Vector3 offset = new Vector3(x, 0, z);
                    
                    positions.Add(offset + center);
                }
            }
            
            return positions;
        }
    }
}