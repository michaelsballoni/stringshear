// Define a particle as its x position along the string,
// its y position above or below the string,
// its velocity, its acceleration, and its punch (velocity of acceleration)
using System;
using System.Linq;

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

        // NOTE: Default constructor zeroes out all fields which is what we want

        public Particle(double _x, double _y = 0, double _vel = 0, double _acl = 0, double _punch = 0, double _next = 0)
        {
            x = _x;
            y = _y;
            vel = _vel;
            acl = _acl;
            punch = _punch;
            nextNeighborFactor = _next;
        }

        public override string ToString()
        {
            return $"{x},{y},{vel},{acl},{punch},{nextNeighborFactor}";
        }

        public static Particle FromString(string str)
        {
            double[] vals = 
                str
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => double.Parse(x))
                .ToArray();
            if (vals.Length != 6)
                throw new Exception("Particle parsing fails: " + str);

            return new Particle(vals[0], vals[1], vals[2], vals[3], vals[4], vals[5]);
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
