using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallSimulator
{
    public interface ICollidable
    {
        double TimeOfCollision(Ball other);
        void ResolveCollision(Ball other);
    }
}
