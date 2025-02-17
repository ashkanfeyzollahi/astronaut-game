using System;
using System.Collections.Generic;
using System.Threading;

namespace AstronautGame
{
    class Entity
    {
        public int left { get; set; }
        public int top { get; set; }

        public Entity(int left, int top)
        {
            this.left = left;
            this.top = top;
        }
    }
    class Player : Entity
    {
        public char sign = '^';

        public Player(int left, int top) : base(left, top)
        { }

        public char[] Render(char[] textBuffer)
        {
            textBuffer[top * Console.WindowWidth + left] = sign;
            return textBuffer;
        }
    }

    class Enemy : Entity
    {
        public char sign = '*';

        public Enemy(int left, int top) : base(left, top)
        { }

        public char[] Render(char[] textBuffer)
        {
            textBuffer[top * Console.WindowWidth + left] = sign;
            return textBuffer;
        }
    }

    class PlayerManager
    {
        public static void ListenToArrowKeys()
        {
            while (true)
            {
                ConsoleKeyInfo console_key_info = Console.ReadKey();

                switch (console_key_info.Key)
                {
                    case ConsoleKey.UpArrow:
                        Program.player.top = Math.Max(
                            0, --Program.player.top
                        );
                        break;

                    case ConsoleKey.LeftArrow:
                        Program.player.left = Math.Max(
                            0, --Program.player.left
                        );
                        break;

                    case ConsoleKey.DownArrow:
                        Program.player.top = Math.Min(
                            Console.WindowHeight - 1, ++Program.player.top
                        );
                        break;

                    case ConsoleKey.RightArrow:
                        Program.player.left = Math.Min(
                            Console.WindowWidth - 1, ++Program.player.left
                         );
                        break;
                }
            }
        }

        public static void CheckCollisionWithEnemy(Enemy enemy)
        {
            if (enemy.top == Program.player.top && enemy.left == Program.player.left)
                Program.lostTheGame = true;
        }

        public static void CheckCollisionsWithEnemies()
        {
            try
            {
                Program.enemies.ForEach(new Action<Enemy>(CheckCollisionWithEnemy));
            }
            catch {}
        }

        public static void RenderPlayer()
        {
            CheckCollisionsWithEnemies();
            Program.player.Render(Program.textBuffer);
        }

        public static void ManagePlayer()
        {
            ListenToArrowKeys();
        }
    }

    class EnemyManager
    {
        public static void AddEnemy(Enemy enemy)
        {
            Program.enemies.Add(enemy);
        }

        public static void RenderEnemy(Enemy enemy)
        {
            enemy.Render(Program.textBuffer);
        }

        public static void RenderEnemies()
        {
            try
            {
                Program.enemies.ForEach(new Action<Enemy>(RenderEnemy));
            }
            catch {}
        }

        public static void MoveEnemyDown(Enemy enemy)
        {
            enemy.top++;

            if (enemy.top > Console.WindowHeight - 1)
                Program.enemies.Remove(enemy);
        }

        public static void MoveEnemiesDown()
        {
            try
            {
                Program.enemies.ForEach(new Action<Enemy>(MoveEnemyDown));
            }
            catch {}
        }

        public static void ManageEnemies()
        {
            Random rand = new Random();

            while (true)
            {
                MoveEnemiesDown();

                Enemy new_enemy = new Enemy(
                    rand.Next(0, Console.WindowWidth),
                    0
                );

                AddEnemy(new_enemy);

                Thread.Sleep(100);
            }
        }
    }

    class Program
    {
        public static char[] textBuffer = new char[Console.WindowHeight * Console.WindowWidth];

        public static bool lostTheGame = false;

        public static Player player;
        public static List<Enemy> enemies = new List<Enemy>();

        static void ClearTextBuffer()
        {
            textBuffer = new char[Console.WindowHeight * Console.WindowWidth];
        }

        static void RenderTextBuffer()
        {
            Console.Write(new string(textBuffer));

            ClearTextBuffer();
        }

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.DarkBlue;

            player = new Player(Console.WindowWidth / 2, Console.WindowHeight / 2);

            Thread _PlayerManager = new Thread(
                new ThreadStart(PlayerManager.ManagePlayer)
            );
            _PlayerManager.Start();

            Thread _EnemyManager = new Thread(
                new ThreadStart(EnemyManager.ManageEnemies)
            );
            _EnemyManager.Start();

            while (!lostTheGame)
            {
                Console.Clear();

                EnemyManager.RenderEnemies();
                PlayerManager.RenderPlayer();

                RenderTextBuffer();
                Thread.Sleep(20);
            }

            _PlayerManager.Abort();
            _EnemyManager.Abort();

            Console.Clear();

            Thread.Sleep(500);

            Console.Write("You lost! Press any key to continue...");
            Console.ReadKey();
        }
    }
}
