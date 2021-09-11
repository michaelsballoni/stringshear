#pragma once

__declspec(dllexport)
void StartSimulation();

__declspec(dllexport)
void ApplySimSettings(const char* settings);

__declspec(dllexport)
const char* GetSimState();

__declspec(dllexport)
const char* GetTimingSummary();
