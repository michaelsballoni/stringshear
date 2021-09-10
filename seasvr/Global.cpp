#include "pch.h"
#include "Global.h"
#include "Simulation.h"

Simulation* sim = nullptr;
auto particlesBuffer = new std::array<char, 1024 * 1024>();;

void StartSimulation()
{
	sim = new Simulation();
}

void ApplySimSettings(const std::string& settings)
{
	sim->ApplySettings(settings);
}

std::string GetSimState()
{
	return sim->ToString(*particlesBuffer);
}
