#pragma once

#include <stdio.h>
#include <math.h>

#include <exception>

struct Particle
{
    double x;
    double y;
    double vel;
    double acl;
    double punch;
    double nextNeighborFactor;

    Particle()
    {
        // Let it ride
        //memset(this, 0, sizeof(this));
    }

    Particle(double _x, double _y = 0, double _vel = 0, double _acl = 0, double _punch = 0, double _next = 0)
    {
        x = _x;
        y = _y;
        vel = _vel;
        acl = _acl;
        punch = _punch;
        nextNeighborFactor = _next;
    }

    void ToString(char* buffer, size_t bufLen) const
    {
        snprintf(buffer, bufLen, 
                 "%f,%f,%f,%f,%f,%f", 
                 x, y, vel, acl, punch, nextNeighborFactor);
    }

    double GetVal(unsigned index) const
    {
        switch (index)
        {
        case 0: return x;
        case 1: return y;
        case 2: return vel;
        case 3: return acl;
        case 4: return punch;
        default: throw std::exception("Particle::GetVal invalid index");
        }
    }

    void Reset()
    {
        // NOTE: Leave x position alone
        y = vel = acl = punch = nextNeighborFactor = 0.0;
    }

    // Update the y value for this, computing vel and acl and punch,
    // and returning the work performed (acl * distance)
    // This is used for endpoints of the string
    double SetPosY(double newPosY, double elapsedTime, double time)
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

        return fabs(workDone);
    }
};
