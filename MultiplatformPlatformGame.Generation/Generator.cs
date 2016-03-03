﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace MultiplatformPlatformGame.Generation
{
    public class Generator
    {
        private readonly Random _r;

        private List<Component> components;
        private List<Block> blocks;

        public Generator ()
        {
            this._r = new Random(DateTime.Now.Millisecond);
            components = new List<Component>();
            blocks = new List<Block>();
        }

        public void AddComponent(String filePath)
        {
            Component c = JsonConvert.DeserializeObject<Component>(System.IO.File.ReadAllText(filePath));
            components.Add(c);
        }

        public void AddBlock(String filePath)
        {
            Block b = JsonConvert.DeserializeObject<Block>(System.IO.File.ReadAllText(filePath));
            blocks.Add(b);
        }

        public List<Block> GetBlocks()
        {
            return blocks;
        }

        public void AnalyseBlocks()
        {
            foreach (Block b in blocks)
            {
                b.BindComponents(components);
                AnalyseBlock(b);
            }
        }

        protected void AnalyseBlock(Block b)
        {
            foreach (BlockComponent c in BorderNonSolidComponents(b))
            {
            }
        }

        protected IEnumerable<BlockComponent> BorderComponents(Block b)
        {
            for (int i = 0; i < b.BlockSize; i++)
            {
                if (i == 0 || i == b.BlockSize)
                {
                    foreach (BlockComponent c in b.ComponentMatrix[i])
                    {
                        yield return c;
                    }
                }
                else {
                    yield return b.ComponentMatrix[i][0];
                    yield return b.ComponentMatrix[i][b.BlockSize - 1];
                }
            }
        }

        protected IEnumerable<BlockComponent> BorderNonSolidComponents(Block b)
        {
            foreach (BlockComponent c in BorderComponents(b))
            {
                if (!c.Component.Solid)
                {
                    yield return c;
                }
            }
        }

        public byte[,] GenerateChunck(int width, int height)
        {
            // Le chunk est initialisé avec toutes les valeurs par défaut (0 -> aucune ouverture)
            // x représente les lignes, y les colonnes
            byte[,] result = new byte[height, width];

            // Liste des coordonnées à traiter
            Queue<Point> toTreat = new Queue<Point>();

            // On commence avec la position en haut à gauche
            toTreat.Enqueue(new Point(0, 0));

            // Génération des pièces
            bool endReached = false;
            // Tant qu'on a des pièces à traiter ou qu'on n'a pas atteint la fin
            while (toTreat.Count > 0 || !endReached)
            {
                Point currentCoord;
                if (toTreat.Count > 0)
                {
                    currentCoord = toTreat.Dequeue();
                }
                else
                {
                    currentCoord = this.FindTopRightNotNull(result);
                }

                // analyse de la faisabilité du tirage et initialisation des case adjacentes
                // 4 tests minimum
                this.CheckOpening(result, currentCoord, Opening.Up, toTreat);
                this.CheckOpening(result, currentCoord, Opening.Right, toTreat);
                this.CheckOpening(result, currentCoord, Opening.Down, toTreat);
                this.CheckOpening(result, currentCoord, Opening.Left, toTreat);

                if (currentCoord.Y == width - 1)
                    endReached = true;
            }
            return result;
        }

        private void CheckOpening(byte[,] array, Point currentCoord, Opening direction, Queue<Point> toTreat)
        {
            Opening oppositeOpening;
            bool isValidPosition;
            Point adjacentCoord;
            int creationRate;
            switch (direction)
            {
                case Opening.Up:
                    oppositeOpening = Opening.Down;
                    isValidPosition = currentCoord.X > 0;
                    adjacentCoord = new Point(currentCoord.X - 1, currentCoord.Y);
                    creationRate = 25;
                    break;
                case Opening.Right:
                    oppositeOpening = Opening.Left;
                    isValidPosition = currentCoord.Y < array.GetLength(1) - 1;
                    adjacentCoord = new Point(currentCoord.X, currentCoord.Y + 1);
                    creationRate = 50;
                    break;
                case Opening.Down:
                    oppositeOpening = Opening.Up;
                    isValidPosition = currentCoord.X < array.GetLength(0) - 1;
                    adjacentCoord = new Point(currentCoord.X + 1, currentCoord.Y);
                    creationRate = 25;
                    break;
                case Opening.Left:
                    oppositeOpening = Opening.Right;
                    isValidPosition = currentCoord.Y > 0;
                    adjacentCoord = new Point(currentCoord.X, currentCoord.Y - 1);
                    creationRate = 50;
                    break;
                default:
                    throw new Exception("Paramètre direction invalide");
            }

            // on peut créer une case dans cette direction
            if (isValidPosition && this._r.Next(100) < creationRate)
            {
                if (array[adjacentCoord.X, adjacentCoord.Y] == (byte)Opening.None)
                {
                    toTreat.Enqueue(adjacentCoord);
                }
                array[adjacentCoord.X, adjacentCoord.Y] |= (byte)oppositeOpening;
                array[currentCoord.X, currentCoord.Y] |= (byte)direction;
            }
        }

        private Point FindTopRightNotNull(byte[,] array)
        {
            for (int column = array.GetLength(1) - 1; column >= 0; column--)
            {
                for (int line = array.GetLength(0) - 1; line >= 0; line--)
                {
                    if (array[line, column] != 0)
                    {
                        Point found = new Point(line, column);
                        Console.WriteLine("Right : {0}", found);
                        return found;
                    }

                }
            }
            return new Point(0, 0);
        }
    }

    [Flags]
    public enum Opening : byte
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8
    }
}

