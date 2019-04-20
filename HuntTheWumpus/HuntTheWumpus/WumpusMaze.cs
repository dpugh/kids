using System;
using System.Collections.Generic;

namespace HuntTheWumpus
{
    public class WumpusMaze
    {
        public int PlayerRoom { get; private set; }
        public bool Won { get; private set; }
        public bool Lost { get; private set; }

        public WumpusMaze()
        {
            this.PlayerRoom = 1;
            _wumpusRoom = this.PickRandomStartingRoom();

            int numberOfBats = 1;// _random.Next(2) + 1;
            int numberOfPits = 1;// 3 - numberOfBats;

            for (int i = 0; (i < numberOfBats); ++i)
            {
                _batRooms.Add(this.PickRandomStartingRoom());
            }

            for (int i = 0; (i < numberOfPits); ++i)
            {
                _pitRooms.Add(this.PickRandomStartingRoom());
            }

            // Relocate the first pit if all access to the Wumpus is blocked
            while (!(this.IsSafe(this.GetAdjacentRoom(_wumpusRoom, Direction.Clockwise)) ||
                     this.IsSafe(this.GetAdjacentRoom(_wumpusRoom, Direction.CounterClockwise)) ||
                     this.IsSafe(this.GetAdjacentRoom(_wumpusRoom, Direction.InOut))))
            {
                _pitRooms[0] = this.PickRandomStartingRoom();
            }
        }

        public string Move(int newRoom)
        {
            string result = "";
            if (!this.IsAdjacent(newRoom, this.PlayerRoom))
            {
                result = "You can't move there!";
            }
            else
            {
                this.PlayerRoom = newRoom;

                while (true)
                {
                    if (this.PlayerRoom == _wumpusRoom)
                    {
                        result = result + "You were eaten by the Wumpus!";
                        this.Lost = true;
                        break;
                    }

                    if (_pitRooms.Contains(this.PlayerRoom))
                    {
                        result = result + "You fell into a bottomless pit!";
                        this.Lost = true;
                        break;
                    }

                    if (_batRooms.Contains(this.PlayerRoom))
                    {
                        result = result + "You were carried away by bats and ....";
                        this.PlayerRoom = _random.Next(10);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public string Shoot(int targetRoom)
        {
            string result;
            if (!this.IsAdjacent(targetRoom, this.PlayerRoom))
            {
                result = "You can't shoot there!";
            }
            else
            {
                if (targetRoom == _wumpusRoom)
                {
                    result = "You got the Wumpus!";
                    this.Won = true;
                }
                else
                {
                    result = "You missed the Wumpus and are out of arrows!";
                    this.Lost = true;
                }
            }

            return result;
        }

        public bool AdjacentToWumpus()
        {
            return (_wumpusRoom == this.GetAdjacentRoom(this.PlayerRoom, Direction.Clockwise)) ||
                   (_wumpusRoom == this.GetAdjacentRoom(this.PlayerRoom, Direction.CounterClockwise)) ||
                   (_wumpusRoom == this.GetAdjacentRoom(this.PlayerRoom, Direction.InOut));
        }

        public bool AdjacentToBat()
        {
            return _batRooms.Contains(this.GetAdjacentRoom(this.PlayerRoom, Direction.Clockwise)) ||
                   _batRooms.Contains(this.GetAdjacentRoom(this.PlayerRoom, Direction.CounterClockwise)) ||
                   _batRooms.Contains(this.GetAdjacentRoom(this.PlayerRoom, Direction.InOut));
        }

        public bool AdjacentToPit()
        {
            return _pitRooms.Contains(this.GetAdjacentRoom(this.PlayerRoom, Direction.Clockwise)) ||
                   _pitRooms.Contains(this.GetAdjacentRoom(this.PlayerRoom, Direction.CounterClockwise)) ||
                   _pitRooms.Contains(this.GetAdjacentRoom(this.PlayerRoom, Direction.InOut));
        }

        #region privates
        private Random _random = new Random();
        private int _wumpusRoom = -1;
        private List<int> _batRooms = new List<int>();
        private List<int> _pitRooms = new List<int>();

        private bool IsAdjacent(int room1, int room2)
        {
            return (this.GetAdjacentRoom(room1, Direction.Clockwise) == room2) ||
                   (this.GetAdjacentRoom(room1, Direction.CounterClockwise) == room2) ||
                   (this.GetAdjacentRoom(room1, Direction.InOut) == room2);
        }

        private bool IsSafe(int room)
        {
            if ((_wumpusRoom == room) || _batRooms.Contains(room) || _pitRooms.Contains(room))
                return false;

            return true;
        }

        public int GetAdjacentRoom(int room, Direction d)
        {
            int newRoom;

            switch (d)
            {
                case Direction.Clockwise:
                    if (room == 4)
                        newRoom = 0;
                    else if (room == 9)
                        newRoom = 5;
                    else
                        newRoom = room + 1;
                    break;
                case Direction.CounterClockwise:
                    if (room == 0)
                        newRoom = 4;
                    else if (room == 5)
                        newRoom = 9;
                    else
                        newRoom = room - 1;
                    break;
                default:
                    if (room < 5)
                        newRoom = room + 5;
                    else
                        newRoom = room - 5;
                    break;
            }

            return newRoom;
        }

        private int PickRandomStartingRoom()
        {
            while (true)
            {
                int room = _random.Next(7) + 3;  // gives a room of 3 .... 9
                if ((room != 6) && this.IsSafe(room))
                {
                    return room;
                }
            }
        }
        #endregion
    }
}
