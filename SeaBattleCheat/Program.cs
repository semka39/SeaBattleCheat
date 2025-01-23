using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SeaBattleCheat
{
    internal class Program
    {
        static int[] ships;
        static bool[,] map;
        static bool[,] knmap;
        static int[,] varMap;
        static bool[,] shMap;
        static Random rnd = new Random();

        static bool CheckCellForShip(int x, int y)
        {
            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                for (int ny = y - 1; ny <= y + 1; ny++)
                {
                    bool outOfMap = nx < 0 || ny < 0 || nx >= 10 || ny >= 10;
                    if (outOfMap && nx == x && ny == y)
                        return false;
                    else if (!outOfMap)
                    {
                        if (map[nx, ny])
                            return false;
                    }
                }

            }
            return true;
        }

        static bool CheckCellForVShip(int x, int y)
        {
            bool outOfMap = x < 0 || y < 0 || x >= 10 || y >= 10;

            return !outOfMap && !knmap[x, y];
        }

        static void FillMap()
        {
            int tries = 0;


            ships = new int[4];
            map = new bool[10, 10];
            knmap = new bool[10, 10];
            shMap = new bool[10, 10];
            fship = new List<(int, int)>();
            step = 0;
            goals = 0;

            for (int i = 0; i < ships.Length; i++)
            {
                ships[i] = 4 - i;
            }

            for (int shipLength = 0; shipLength < ships.Length; shipLength++)
            {
                for (int k = 0; k < ships[shipLength]; k++)
                {
                    int x, y, dirx, diry, cx, cy;
                    bool cansetship;
                    do
                    {
                        cansetship = true;

                        x = rnd.Next(10);
                        y = rnd.Next(10);

                        int a = rnd.Next(2) == 0 ? -1 : 1;
                        bool b = rnd.Next(2) == 0;
                        dirx = b ? a : 0;
                        diry = b ? 0 : a;

                        cx = x;
                        cy = y;
                        for (int j = 0; j < shipLength + 1; j++)
                        {
                            if (!CheckCellForShip(cx, cy))
                            {
                                cansetship = false;
                                break;
                            }
                            cx += dirx;
                            cy += diry;
                        }
                        tries++;
                    }
                    while (!cansetship && tries < 1000);

                    if (tries < 1000)
                    {
                        tries = 0;

                        cx = x;
                        cy = y;
                        for (int j = 0; j < shipLength + 1; j++)
                        {
                            map[cx, cy] = true;
                            cx += dirx;
                            cy += diry;
                        }
                    }

                }
            }

            if (tries >= 1000)
                FillMap();
        }


        static void GenerateVarMap()
        {
            varMap = new int[10, 10];

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {

                    for (int shipLength = 1; shipLength < ships.Length; shipLength++)
                    {
                        if (ships[shipLength] == 0)
                            continue;

                        for (int A = 0; A <= 1; A++)
                            for (int B = 0; B <= 1; B++)
                            {
                                // только с этого момента начинается логика для 1 крабля

                                bool cansetship = true;

                                int a = A == 0 ? -1 : 1;
                                bool b = B == 0;

                                int dirx = b ? a : 0;
                                int diry = b ? 0 : a;

                                int cx = x, cy = y;
                                for (int j = 0; j < shipLength + 1; j++)
                                {
                                    if (!CheckCellForVShip(cx, cy))
                                    {
                                        cansetship = false;
                                        break;
                                    }
                                    cx += dirx;
                                    cy += diry;
                                }

                                if (cansetship)
                                {
                                    cx = x;
                                    cy = y;
                                    for (int j = 0; j < shipLength + 1; j++)
                                    {
                                        varMap[cx, cy] += ships[shipLength];
                                        cx += dirx;
                                        cy += diry;
                                    }
                                }
                            }
                    }

                }
            }
        }

        static int step = 0;


        static bool sled = false;
        static bool slb = false, tslb = false;
        static List<(int, int)> fship;

        static int lmi = 0, lmsl = 1;

        static bool IsPointInMask(int x, int y, int i, int shipLength)
        {
            return (x - (y + i) % shipLength) % shipLength == 0;
        }


        static (int, int) MaskaPatern(List<(int, int)> maxes, int shipLength)
        {
            (int, int)[] masks = new (int, int)[shipLength];
            int[] masksL = new int[shipLength];

            foreach ((int, int) point in maxes)
            {
                int x = point.Item1;
                int y = point.Item2;
                for (int i = 0; i < shipLength; i++)
                {
                    if (IsPointInMask(x, y, i, shipLength))
                    {
                        masks[i] = (x, y);
                        masksL[i]++;
                    }
                }
            }

            /*
            for (int y = 0; y < 10; y++)
            {
                for (int i = 0; i < shipLength; i++)
                {
                    for (int x = (y + i) % shipLength; x < 10; x += shipLength)
                    {
                        if (maxes.Contains((x, y)))
                        {
                            masks[i] = (x, y);
                            masksL[i]++;
                        }
                    }
                }
            }
            */

            int minmaskL = -1;
            foreach (int maskL in masksL)
                if ((minmaskL == -1 || minmaskL > maskL) && maskL != 0)
                    minmaskL = maskL;

            int bestI = Array.IndexOf(masksL, minmaskL);
            lmi = bestI;
            lmsl = shipLength;

            (int, int) largestMasksP = masks[bestI];
            return largestMasksP;
        }

        static void MakeDesigion(out int finalX, out int finalY)
        {
            int maxI = -1, mX = -1, mY = -1;
            List<(int, int)> maxes = new List<(int, int)>();
            if (!sled)
            {
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        if (varMap[x, y] > maxI && !shMap[x, y])
                        {
                            maxI = varMap[x, y];
                            maxes.Clear();
                        }
                        if (varMap[x, y] == maxI && !shMap[x, y])
                            maxes.Add((x, y));
                    }
                }

                int[] shipswithoutone = ships.Skip(1).ToArray();
                int shipminL = -1;
                foreach (int shipL in shipswithoutone)
                    if ((shipminL == -1 || shipminL > shipL) && shipL != 0)
                        shipminL = shipL;

                int minshipLen = Array.IndexOf(shipswithoutone, shipminL) + 2;
                (int, int) bestP = MaskaPatern(maxes, minshipLen);
                mX = bestP.Item1;
                mY = bestP.Item2;

            }//zacepok net, ishem po karte
            else
            {
                foreach ((int, int) cell in fship)
                {
                    int x = cell.Item1;
                    int y = cell.Item2;

                    for (int A = 0; A <= 1; A++)
                        for (int B = 0; B <= 1; B++)
                        {
                            int a = A == 0 ? -1 : 1;
                            bool b = B == 0;
                            if (fship.Count > 1)
                                b = slb;

                            int nx = x + (b ? a : 0);
                            int ny = y + (b ? 0 : a);

                            bool outOfMap = nx < 0 || ny < 0 || nx >= 10 || ny >= 10;


                            if (!outOfMap && varMap[nx, ny] > maxI && !shMap[nx, ny])
                            {
                                maxI = varMap[nx, ny];
                                mX = nx;
                                mY = ny;
                                tslb = b;
                            }
                        }
                }
            }

            finalX = mX;
            finalY = mY;
        }

        static bool IsSlededKilled()
        {
            foreach ((int, int) cell in fship)
            {
                int x = cell.Item1;
                int y = cell.Item2;

                for (int px = x - 1; px <= x + 1; px++)
                {
                    for (int py = y - 1; py <= y + 1; py++)
                    {
                        bool outOfMap = px < 0 || py < 0 || px >= 10 || py >= 10;

                        if (!outOfMap && map[px, py] && !fship.Contains((px, py)))
                            return false;
                    }
                }
            }
            return true;
        }

        static void KilledObvodka()
        {
            foreach ((int, int) cell in fship)
            {
                int x = cell.Item1;
                int y = cell.Item2;

                for (int px = x - 1; px <= x + 1; px++)
                {
                    for (int py = y - 1; py <= y + 1; py++)
                    {
                        bool outOfMap = px < 0 || py < 0 || px >= 10 || py >= 10;

                        if (!outOfMap)
                        {
                            shMap[px, py] = true;
                            knmap[px, py] = true;
                        }

                    }
                }
            }
        }

        static int goals;

        static void Shoot(int x, int y)
        {
            shMap[x, y] = true;
            knmap[x, y] = !map[x, y];

            if (map[x, y])
            {
                goals++;
                sled = true;
                fship.Add((x, y));
                if (fship.Count > 1)
                    slb = tslb;

                //метагейм проверка на убит
                if (IsSlededKilled())
                {
                    KilledObvodka();
                    ships[fship.Count - 1]--;
                    fship.Clear();
                    sled = false;
                }

            }
        }

        static string PosToStr(int x, int y)
        {
            return "АБВГДЕЖЗИК"[x] + (y + 1).ToString();
        }

        static int games = 0, stepsum = 0, kpdsum = 0;

        static void Render(int sX, int sY)
        {
            Console.WriteLine($"Шаг: {step} ");
            Console.WriteLine("Стреляю " + PosToStr(sX, sY) + " ");

            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            if (sled)
            {
                string dir = slb ? "горизонтального" : "вертикального";
                Console.Write("Напал на след " + (fship.Count > 1 ? dir : "неизвестного") + " корабля, уже ранил в: ");
                foreach ((int, int) cell in fship)
                {
                    Console.Write(PosToStr(cell.Item1, cell.Item2) + " ");
                }
                Console.WriteLine();

            }
            else
                Console.WriteLine("Корабли не найдены, ищу по карте вероятностей");

            Console.Write("    А   Б   В   Г   Д   Е   Ж   З   И   К");
            for (int y = 0; y < 10; y++)
            {
                Console.WriteLine('\n');
                Console.Write($"{y + 1,2}  ");
                for (int x = 0; x < 10; x++)
                {

                    ConsoleColor bcolor, fcolor;
                    if (map[x, y])
                    {
                        bcolor = shMap[x, y] ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
                    }
                    else
                    {
                        bcolor = shMap[x, y] ? ConsoleColor.DarkBlue : ConsoleColor.Black;
                    }


                    fcolor = IsPointInMask(x, y, lmi, lmsl) ? ConsoleColor.Yellow : ConsoleColor.White;

                    Console.BackgroundColor = bcolor;
                    Console.ForegroundColor = fcolor;
                    Console.Write($"{varMap[x, y],2}");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("  ");
                }
            }

            Console.WriteLine();
            int length = (int)Math.Round(31 * (double)goals / step);
            Console.Write("\nКПД: ");
            Console.BackgroundColor = ConsoleColor.Green;
            Console.Write(new string(' ', length));
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write(new string(' ', 31 - length));

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($" - {(int)Math.Round(100 * (double)goals / step)}%   ");


        }

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            FillMap();

            while (true)
            {
                step++;
                Console.SetCursorPosition(0, 0);
                GenerateVarMap();

                int sX, sY;

                MakeDesigion(out sX, out sY);
                Shoot(sX, sY);

                Render(sX, sY);
                Console.ReadLine();
                //Thread.Sleep(1000);

                if (ships.Sum() == 0)
                {
                    stepsum += step;
                    kpdsum += (int)((double)goals * 100 / step);
                    games++;
                    Console.Write("\n\n");
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write($"Сыграно {games} игр. В среднем {(double)stepsum / games} ходов.\nСредний КПД: {kpdsum / games}%");
                    FillMap();
                }
            }
        }
    }
}
