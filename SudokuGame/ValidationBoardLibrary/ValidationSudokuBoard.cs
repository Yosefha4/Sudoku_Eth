using System;
using System.Collections.Generic;

namespace ValidationBoardLibrary
{
    public class ValidationSudokuBoard
    {
        private unsafe int** loadedArray = null;
        private int rows, cols;
        public unsafe ValidationSudokuBoard(int** arr, int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            this.loadedArray = arr;
        }
        public unsafe bool IsBoardValid(int value, int[] indexes)
        {

            var lambdaIsRowColSubMatrix = new Func<int, int, bool>((row, col) =>
            {
                if (indexes[0] == row && indexes[1] == col || value != loadedArray[row][col])
                {
                    return true;
                }
                return false;
            });

            return IsRowOrColunmValid(0, indexes[1], lambdaIsRowColSubMatrix, false) &&
                 IsRowOrColunmValid(indexes[0], 0, lambdaIsRowColSubMatrix) &&
                 IsSubMatrixValid(indexes, lambdaIsRowColSubMatrix);

        }

        public int GetMoveValidInBorad(int row_or_col)
        {
            //A function that returns the legal position of a slot
            //This function is actually used to test the arrow keys pressed by the user
            if (row_or_col % rows == 0)
            {
                return 0;
            }
            if (row_or_col < 0)
            {
                return rows - 1;
            }
            return row_or_col % rows;
        }

        void Swap(ref int r, ref int c)
        {
            int temp = r;
            r = c;
            c = temp;
        }

        private unsafe bool IsRowOrColunmValid(int row, int col, Func<int, int, bool> lambda, bool isRow = true)
        {
            int r = 1, c = 0;
            if (isRow)
            {
                Swap(ref r, ref c);
            }

            for (int i = 0; i < rows; i++)
            {

                if (!lambda(row, col))
                {
                    return false;
                }
                row += r;
                col += c;
            }
            return true;
        }

        private unsafe bool IsSubMatrixValid(int[] indexes, Func<int, int, bool> lambda)
        {
            int r = indexes[0] % 3, c = indexes[1] % 3;
            for (int i = 0; i < 3; i++)
            {
                int x = indexes[0] - r, y = indexes[1] - c;

                for (int j = 0; j < 3; j++)
                {
                    if (!lambda(x + i, y + j))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public List<int[]> GetIndexesToChnageColor(params int[] para)
        {
            return GetIndexesToChnageColor(new int[] { para[0], para[1], para[2] }, para[3]);
        }

        public unsafe List<int[]> GetIndexesToChnageColor(int[] indexes, int value)
        {

            List<int[]> liIndexesChnage = new List<int[]>();
            int row_current = indexes[0], col_current = indexes[1];

            int potentialIndexRow = -1, potentialIndexCol = -1;

            var lambdaPotentialIndex = new Func<int, int, bool>((r, c) =>
            {
                if (loadedArray[r][c] == value)
                {

                    if (potentialIndexRow == -1)
                    {
                        potentialIndexRow = r;
                        potentialIndexCol = c;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            });

            void AddIndexToList()
            {
                // check if index potential valid 
                if (potentialIndexRow != -1 && IsBoardValid(value, new int[] { potentialIndexRow, potentialIndexCol }))
                {
                    liIndexesChnage.Add(new int[] { potentialIndexRow, potentialIndexCol });
                }
                potentialIndexRow = potentialIndexCol = -1;
            }

            //Check is find index potential to chnage in col
            IsRowOrColunmValid(0, col_current, lambdaPotentialIndex, false);
            AddIndexToList();

            //Check is find index potential to chnage in row
            IsRowOrColunmValid(row_current, 0, lambdaPotentialIndex);
            AddIndexToList();

            //Check is find index potential to chnage in sub matrix
            IsSubMatrixValid(indexes, lambdaPotentialIndex);
            AddIndexToList();

            return liIndexesChnage;
        }

    }
}