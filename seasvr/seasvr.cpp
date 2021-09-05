#include "pch.h"
#include "Simulation.h"

#include <msclr\marshal.h>

#include <thread>

using namespace System;
using namespace System::IO;
using namespace System::Net;

using namespace msclr::interop;

std::string StringToString(String^ netStr)
{
    pin_ptr<const wchar_t> wstrPtr = PtrToStringChars(netStr);
    std::wstring wstr(wstrPtr);
    std::string str(wstr.begin(), wstr.end());
    return str;
}

double g_totalOutputElapsedMs = 0.0;
int g_outputsCount = 0;
CSection g_outputCs;
void SetOutputElapsedMs(double elapsedMs)
{
    //printf("%f\n", elapsedMs);
    CSLock lock(g_outputCs);
    g_totalOutputElapsedMs += elapsedMs;
    ++g_outputsCount;
}

double GetOutputElapsedMs()
{
    CSLock lock(g_outputCs);
    return g_outputsCount == 0 ? 0.0 : (g_totalOutputElapsedMs / g_outputsCount);
}

Simulation* g_sim = nullptr;

void RunStats()
{
    double totalComputeElapsed = 0.0;
    double totalOutputElapsed = 0.0;

    int statCycles = 0;
    
    bool isPaused = false;
    while (true)
    {
        std::this_thread::sleep_for(std::chrono::seconds(3));

        double computeElapsed = g_sim->GetElapsed();
        if (computeElapsed == 0.0) // paused
        {
            if (!isPaused)
            {
                printf("paused\n");
                isPaused = true;
            }
            continue;
        }

        totalComputeElapsed += computeElapsed;
        isPaused = false;

        ++statCycles;

        printf("compute: %f - output: %f\n",
               totalComputeElapsed / statCycles,
               GetOutputElapsedMs());
    }
}

int main(array<System::String ^> ^args)
{
    Console::Write("Starting simulation...");
    g_sim = new Simulation();
    Console::WriteLine("done!");

    Console::Write("Setting up server...");
    auto listener = gcnew HttpListener();
    listener->Prefixes->Add("http://localhost:9914/");
    listener->Start();
    Console::WriteLine("done!");

    std::thread statsThread(RunStats);

    Stopwatch sw;
    std::string state;
    while (true)
    {
#ifdef _DEBUG
        Console::Write("?");
#endif
        auto ctxt = listener->GetContext();
#ifdef _DEBUG
        Console::Write(".");
#endif
        if (ctxt->Request->HttpMethod == "GET")
        {
            sw.Start();
            g_sim->ToString(state);

            StreamWriter^ writer = gcnew StreamWriter(ctxt->Response->OutputStream);
            writer->Write(gcnew String(state.c_str()));
            delete writer;
            
            SetOutputElapsedMs(sw.ElapsedMs());
        }
        else
        {
            StreamReader^ reader = gcnew StreamReader(ctxt->Request->InputStream);
            g_sim->ApplySettings(StringToString(reader->ReadToEnd()));
            delete reader;
            delete ctxt->Response->OutputStream;
        }
#ifdef _DEBUG
        Console::Write("!");
#endif
    }

    return 0;
}
