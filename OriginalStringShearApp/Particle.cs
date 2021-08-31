// Define a particle as its x position along the string,
// its y position above or below the string,
// its velocity, its acceleration, and its punch (velocity of acceleration)
using System;

namespace StringShear
{
    public struct Particle
    {
        public double x;
        public double y;
        public double vel;
        public double acl;
        public double punch;
        public double nextNeighborFactor;

        public Particle(double _x)
        {
            x = _x;
            y = 0;
            vel = 0;
            acl = 0;
            punch = 0;
            nextNeighborFactor = 0;
        }

        public double GetVal(int index)
        {
            switch (index)
            {
                case 0: return x;
                case 1: return y;
                case 2: return vel;
                case 3: return acl;
                case 4: return punch;
                default: throw new Exception($"Invalid index to get: {index}");
            }
        }

        public void Reset()
        {
            // NOTE: Leave x position alone
            y = vel = acl = punch = nextNeighborFactor = 0.0;
        }

        // Update the y value for this, computing vel and acl and punch,
        // and returning the work performed (acl * distance)
        // This is used for endpoints of the string
        public double SetPosY(double newPosY, double elapsedTime, double time)
        {
            double newDisplacement = (newPosY - y);

            double newVel = newDisplacement / elapsedTime;

            double newAcl = (newVel - vel) / elapsedTime;
            if (time <= elapsedTime)
                newAcl = 0.0;

            double newPunch = (newAcl - acl) / elapsedTime;
            if (time <= elapsedTime * 2.0)
                newPunch = 0.0;

            double workDone = newDisplacement * acl;

            y = newPosY;
            vel = newVel;
            acl = newAcl;
            punch = newPunch;

            return Math.Abs(workDone);
        }
    }
}
