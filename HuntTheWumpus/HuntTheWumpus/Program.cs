using System;

namespace HuntTheWumpus
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcom to hunt the Wumpus!");
            Console.WriteLine("    Avoid the squeeky vampire bats");
            Console.WriteLine("    Avoid the bone chewing Wumpus");
            Console.WriteLine("    Shoot the Wumpus from an adjacent room");

            WumpusMaze maze = new WumpusMaze();
            while (!(maze.Won || maze.Lost))
            {
                Console.WriteLine();
                Console.WriteLine("You are in room " + maze.PlayerRoom.ToString());

                var cw = maze.GetAdjacentRoom(maze.PlayerRoom, Direction.Clockwise);
                var ccw = maze.GetAdjacentRoom(maze.PlayerRoom, Direction.CounterClockwise);
                var inout = maze.GetAdjacentRoom(maze.PlayerRoom, Direction.InOut);
                Console.WriteLine("   You are adjacent to rooms: " + cw.ToString() + ", " + ccw.ToString() + ", and " + inout.ToString());

                if (maze.AdjacentToBat())
                {
                    Console.WriteLine("    You hear high pitched squeeks");
                }

                if (maze.AdjacentToPit())
                {
                    Console.WriteLine("    You whistling wind");
                }

                if (maze.AdjacentToWumpus())
                {
                    Console.WriteLine("    You something gnawing on bones");
                }

                Console.WriteLine("Do you want to move or shoot ? ");
                string moveShoot = Console.ReadLine();

                if (moveShoot == "move")
                {
                    int room = GetPlayerMove("Where do you want to move?");
                    string result = maze.Move(room);
                    Console.WriteLine(result);
                }
                else if (moveShoot == "shoot")
                {
                    int room = GetPlayerMove("Where do you want to shoot?");
                    string result = maze.Shoot(room);
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("I didn't understand");
                }
            }
        }

        public static int GetPlayerMove(string prompt)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var d = Console.ReadLine();

                int newRoom;
                if (int.TryParse(d, out newRoom))
                    return newRoom;

                Console.WriteLine("I did not understand.");
            }
        }
    }
}
