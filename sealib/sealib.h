#pragma once

//
// NOTE: Access to this API must not be multi-threaded
//		 The app program is single-threaded (timer + UI), so this isn't a problem with that usage
//
extern "C"
{
	__declspec(dllexport)
		void StartSimulation();

	__declspec(dllexport)
		void ApplySimSettings(const char* settings);

	__declspec(dllexport)
		const char* GetSimState();

	__declspec(dllexport)
		const char* GetTimingSummary();
}