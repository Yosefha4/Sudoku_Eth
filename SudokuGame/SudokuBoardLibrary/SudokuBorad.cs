using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ValidationBoardLibrary;
using Newtonsoft.Json;

namespace SudokuBoardLibrary
{
    public class SudokuBorad
    {
        public readonly int[,] _array;
        private string filePath = "borads_game\\boards.json";
        public ValidationSudokuBoard validSudoku;
        //  string[] lines;
        private Dictionary<string, int[][,]> difficultyBorads;
        GCHandle handle;

        public unsafe SudokuBorad(int[,] array, string select_difficulty)
        {
            _array = array;
            int row = array.GetLength(0);
            int col = array.GetLength(1);
            ReadJsonOfBorad();
            //  lines = File.ReadAllLines(filePath);

            handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            fixed (int* pArray = array)
            {

                int** cArray = (int**)Marshal.AllocHGlobal(array.GetLength(0) * sizeof(int*));
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    cArray[i] = pArray + i * array.GetLength(1);
                }
                validSudoku = new ValidationSudokuBoard(cArray, array.GetLength(0), array.GetLength(1));
            }
        }
        public void ReleaseHandle()
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }


        private void ReadJsonOfBorad()
        {

            // Read the JSON file content
            string jsonContent = File.ReadAllText(filePath);

            // Deserialize the JSON into a dictionary
            difficultyBorads = JsonConvert.DeserializeObject<Dictionary<string, int[][,]>>(jsonContent);

        }

        public void FillBorad(int[,] arrayBorad, string levelDifficulty = "Extreme")
        {
            var borads = difficultyBorads[levelDifficulty];
            int boradRandom = new Random().Next(0, borads.Length);
            // arrayBorad = borads[boradRandom];

            int rows = borads[boradRandom].GetLength(0);
            int col = borads[boradRandom].GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    arrayBorad[i, j] = borads[boradRandom][i, j];
                }
            }

        }


        //public void FillBorad(int[,] arrayBorad, string levelDifficulty = "Extreme")
        //{


        //    int index = Array.FindIndex(lines, line => line.Equals(levelDifficulty, StringComparison.OrdinalIgnoreCase));

        //    List<int> validIndices = new List<int>();
        //    if (index != -1)
        //    {
        //        int currentIndex = index + 1;
        //        while (currentIndex < lines.Length && !lines[currentIndex - 1].StartsWith("="))
        //        {
        //            validIndices.Add(currentIndex);
        //            currentIndex += 10;
        //        }
        //    }

        //    int randomIndex = 0;
        //    if (validIndices.Count > 0)
        //    {
        //        randomIndex = validIndices[new Random().Next(validIndices.Count)];
        //    }

        //    int rows = arrayBorad.GetLength(0);
        //    int columns = arrayBorad.GetLength(1);

        //    for (int i = 0; i < rows; i++)
        //    {
        //        string[] rowValues = lines[i + randomIndex]
        //            .TrimStart('{').TrimEnd('}')
        //            .Split(' ');

        //        for (int j = 0; j < columns; j++)
        //        {
        //            arrayBorad[i, j] = int.Parse(rowValues[j]);
        //        }
        //    }


        //}

    }
}