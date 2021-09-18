#include "pch.h"
#include "sealib.h"
#include "Simulation.h"
#include "ScopeTiming.h"

Simulation* sim = nullptr;
auto particlesBuffer = new std::array<char, 1024 * 1024>();

void StartSimulation()
{
	sim = new Simulation();
	ScopeTiming::GetObj().Init(false);
}

void ApplySimSettings(const char* settings)
{
	sim->ApplySettings(settings);
}

std::string simState;
const char* GetSimState()
{
	sim->ToString(simState, *particlesBuffer);
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
