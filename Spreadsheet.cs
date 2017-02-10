using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SpreadsheetCalculator
{
    /// <summary>
    /// Main Class which holds the Solution
    /// </summary>
    public class Spreadsheet
    {
        /// <summary>
        /// Userdefine Data Structure to hold the Cell Value/Properties
        /// </summary>
        public class Cell
        {
            /// <summary>
            /// Holds the Value of the Cell
            /// </summary>
            public Double value;
            /// <summary>
            /// Cell Content can be Integer value or can be Expression
            /// </summary>
            public String cellContent;
            /// <summary>
            /// Variable to help check cyclic dependency
            /// </summary>
            public bool IsCurrentEvaluation;
            /// <summary>
            /// Flag to check if the Cell Value is evaluated
            /// </summary>
            public bool IsEvaluated;
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="cellContent">Content of the shell</param>
            public Cell(String cellContent)
            {
                this.cellContent = cellContent;
                this.IsCurrentEvaluation = false;
                this.IsEvaluated = false;
            }
        }

        #region Private Variables
        /// <summary>
        /// n*m cells
        /// </summary>
        private Cell[,] Cells;
        /// <summary>
        /// Columns
        /// </summary>
        private int Columns;
        /// <summary>
        /// Rows
        /// </summary>
        private int Rows;
        #endregion

        /// <summary>
        ///     Method Which will Read in the input from the Console (Standard Input) 
        ///     and it will populate the Spreadsheet object
        /// </summary>
        /// <param name="sheet">
        ///     Spreadsheet to be filled
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Throws Argument Exception if the Array Size doesn't match
        /// </exception>
        private static void ReadValuesFromConsole(Spreadsheet sheet)
        {
            sheet.Cells = null;

            try
            {
                #region variables
                // Read the Size of the two dimensional array
                string cellSize = Console.ReadLine();

                string[] WidthAndHeight = cellSize.Split(Convert.ToChar(" "));

                int[] size = new int[2];
                // Count to make sure user doesn't exceeds or miss few cell in n*m size array
                int cellCount = 0;
                #endregion

                #region Reading the Size of the Array
                // Check the Size of the Array , Throw exception if size mismatch
                if (WidthAndHeight.Length != 2)
                {
                    throw new ArgumentException("Invalid Size : Expected Size is n * m");
                }
                else
                {
                    for (int i = 0; i < WidthAndHeight.Length; i++)
                    {
                        //Holds the Columns and Row Size
                        size[i] = Int32.Parse(WidthAndHeight[i]);
                    }
                    sheet.Columns = size[0];
                    sheet.Rows = size[1];
                    sheet.Cells = new Cell[sheet.Rows, sheet.Columns];
                }
                #endregion

                #region Populating the Array
                for (int row = 0; row < sheet.Rows; row++)
                {
                    for (int column = 0; column < sheet.Columns; column++)
                    {
                        string line = Console.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        sheet.Cells[row, column] = new Cell(line);
                        cellCount++;
                    }
                }

                if (cellCount != (sheet.Rows * sheet.Columns))
                {
                    throw new ArgumentException("Number of Cells doesn't match the given size");
                }
                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occured in while reading the input");
                Environment.Exit(1);
            }

        }
        /// <summary>
        ///     Method which Prints Output on Console
        /// </summary>
        /// <param name="sheet">
        ///     sheet which contains cells
        /// </param>
        private static void PrintOutput(Spreadsheet sheet)
        {
            Console.WriteLine(sheet.Columns + " " + sheet.Rows);
            for (int i = 0; i < sheet.Rows; i++)
            {
                for (int j = 0; j < sheet.Columns; j++)
                {
                    Console.WriteLine("{0:0.00000}", sheet.Cells[i, j].value);
                }
            }
        }
        /// <summary>
        ///     Method to Evaluate the Cell's Value
        ///     Dijkstra's two-stack algorithm for expression evaluation
        /// </summary>
        /// <param name="cell">
        ///     Cell
        /// </param>
        /// <param name="currentEvalStack">
        ///     HashSet to check for the Cycling Redudency
        /// </param>
        /// <returns>
        ///     Returns value when evaluation is completed
        /// </returns>
        private double evaluateCell(Cell cell, HashSet<Cell> currentEvalStack)
        {
            if (currentEvalStack == null)
            {
                currentEvalStack = new HashSet<Cell>();
            }
            if (cell.IsEvaluated)
            {
                
            }
            else if (!cell.IsEvaluated && !currentEvalStack.Contains(cell))
            {
                currentEvalStack.Add(cell);

                string[] content = cell.cellContent.Split(Convert.ToChar(" "));
                //Stack to hold the Operands - 
                // if (Number - Push)
                // if (Operator - Pop ---> Evaluate and ----> Push back)
                // if (expression - Evaluate ---> Push)
                Stack<double> operandStack = new Stack<double>();

                for (int i = 0; i < content.Length; i++)
                {
                    string token = content[i];

                    if (token.Equals("+"))
                    {
                        operandStack.Push(operandStack.Pop() + operandStack.Pop());
                    }
                    else if (token.Equals("*"))
                    {
                        operandStack.Push(operandStack.Pop()*operandStack.Pop());
                    }
                    else if (token.Equals("/"))
                    {
                        double divisor = operandStack.Pop();
                        double dividend = operandStack.Pop();
                        operandStack.Push(dividend/divisor);
                    }
                    else if (token.Equals("-"))
                    {
                        double subtractor = operandStack.Pop();
                        double subtractee = operandStack.Pop();
                        operandStack.Push(subtractee - subtractor);
                    }
                    else if (token.Equals("++"))
                    {
                        double op = operandStack.Pop();
                        op++;
                        operandStack.Push(op);
                    } 
                    else if (token.Equals("--"))
                    {
                        double op = operandStack.Pop();
                        op--;
                        operandStack.Push(op);
                    }
                    else if (isNumeric(token))
                    {
                        operandStack.Push(double.Parse(token));
                    }
                    else
                    {
                        Cell anotherCell = getCell(token); // A1 / B2 etc.
                        operandStack.Push(evaluateCell(anotherCell, currentEvalStack));
                    }
                }

                cell.value = operandStack.Pop();
                cell.IsEvaluated = true;
            }
            else
            {
                Console.WriteLine("Cycle occured while evaluating Cell Value : {0}" , cell.cellContent );
                foreach (var cyclicCell in currentEvalStack)
                {
                    Console.WriteLine("Cell with Content : " + cyclicCell.cellContent);
                }
                Console.ReadLine();
                Environment.Exit(1);
            }

            return cell.value;
        }
        /// <summary>
        ///     Method to check if the current token is Real Number
        /// </summary>
        /// <param name="s">
        ///     token can be (A1 or 3 or +/-*)
        /// </param>
        /// <returns>
        ///     true if the given token is Numeric , false otherwise
        /// </returns>
        private static bool isNumeric(String s)
        {
            try
            {
                var d = Double.Parse(s);
                return true;
            }
            catch (FormatException e)
            {
                return false;
            }
        }


        private Cell getCell(String s)
        {
            try
            {
                int x = (int)s.ToCharArray()[0] % 65;
                int y = Int32.Parse(s.Substring(1, s.Length-1)) - 1;
                return Cells[x, y];
            }
            catch (FormatException e)
            {
                Console.WriteLine("Data format error occurred while evaluating Cell" + s);
                Console.ReadLine();
                Environment.Exit(1);
            }
            return null;

        }
        /// <summary>
        /// Main Method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Stopwatch stopwatch = null;
            try
            {
                // Create new stopwatch.
                stopwatch = new Stopwatch();
                // Begin timing.
                stopwatch.Start();

                //Object to hold the  Values
                Spreadsheet sheet = new Spreadsheet();

                //Read the values from Console
                ReadValuesFromConsole(sheet);

                //Evalute Cell Values
                for (int row = 0; row < sheet.Rows; row++)
                {
                    for (int column = 0; column < sheet.Columns; column++)
                    {
                        sheet.evaluateCell(sheet.Cells[row, column], null);
                    }
                }

                //Print the Values on Console (Standard Output)
                PrintOutput(sheet);
                // Stop.
                stopwatch.Stop();

                // Write hours, minutes and seconds.
                Console.WriteLine("Time elapsed: {0:hh\\:mm\\:ss}", stopwatch.Elapsed);
                Console.ReadLine();

            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error Occured in Main : {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occured in Main : {0}", ex.Message);
            }
        }


    }
}
