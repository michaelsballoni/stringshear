#include "pch.h"
#include "sealib.h"
#include "Simulation.h"
#include "ScopeTiming.h"

Simulation* g_pSim = nullptr;

void StartSimulation()
{
	g_pSim = new Simulation();
#ifdef TIMING
	ScopeTiming::GetObj().Init(true);
#endif
}

bool g_anyReset = false;
void ApplySimSettings(const char* settings)
{
	if (strstr(settings, "reset") == settings) // starts with
		g_anyReset = true;

	g_pSim->ApplySettings(settings);
}

std::string g_simStateStr;
const char* GetSimState()
{
	g_pSim->ToString(g_simStateStr);
	return g_simStateStr.c_str();
}

std::string g_timingSummaryStr;
const char* GetTimingSummary()
{
	g_timingSummaryStr = ScopeTiming::GetObj().GetSummary();
	return g_timingSummaryStr.c_str();
}

std::string g_simSummaryStr;
const char* GetSimSummary()
{
	g_simSummaryStr = g_pSim->ToSummary();
	if (g_anyReset)
	{
		g_simSummaryStr += "\nreset: 1";
		g_anyReset = false;
	}
	return g_simSummaryStr.c_str();
}
