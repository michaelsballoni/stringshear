#include "pch.h"
#include "sealib.h"
#include "Simulation.h"
#include "ScopeTiming.h"

Simulation* sim = nullptr;

void StartSimulation()
{
	sim = new Simulation();
#ifdef TIMING
	ScopeTiming::GetObj().Init(true);
#endif
}

void ApplySimSettings(const char* settings)
{
	sim->ApplySettings(settings);
}

std::string simState;
const char* GetSimState()
{
	sim->ToString(simState);
	return simState.c_str();
}

std::string timingSummary;
const char* GetTimingSummary()
{
	timingSummary = ScopeTiming::GetObj().GetSummary();
	return timingSummary.c_str();
}

std::string simSummary;
const char* GetSimSummary()
{
	simSummary = sim->ToSummary();
	return simSummary.c_str();
}
