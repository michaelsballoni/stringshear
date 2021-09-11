#include "pch.h"
#include "sealib.h"
#include "Simulation.h"
#include "ScopeTiming.h"

Simulation* sim = nullptr;
auto particlesBuffer = new std::array<char, 1024 * 1024>();

void StartSimulation()
{
	sim = new Simulation();
	ScopeTiming::GetObj().Init(true);
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
	//printf("Enabled: %d - len: %u\n", (int)ScopeTiming::GetObj().m_enabled, ScopeTiming::GetObj().GetSummary().size());
	timingSummary = ScopeTiming::GetObj().GetSummary();
	return timingSummary.c_str();
}
