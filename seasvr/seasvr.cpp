#include "pch.h"
#include "Simulation.h"

#include <thread>

using namespace System;
using namespace System::IO;
using namespace System::Net;

std::string StringToString(String^ netStr)
{
    pin_ptr<const wchar_t> wstrPtr = PtrToStringChars(netStr);
    std::wstring wstr(wstrPtr);
    std::string str(wstr.begin(), wstr.end());
    return str;
}

void RunStats(Simulation& sim)
{
    double totalComputeElapsed = 0.0;
    double totalOutputElapsed = 0.0;

    int statCycles = 0;
    
    while (true)
    {
        std::this_thread::sleep_for(std::chrono::seconds(3));
        double computeElapsed, outputElapsed;
        sim.GetElapsed(computeElapsed, outputElapsed);

        totalComputeElapsed += computeElapsed;
        totalOutputElapsed += outputElapsed;

        ++statCycles;
        printf("%f compute (%f avg) - %f output (%f avg)\n",
               computeElapsed, totalComputeElapsed / statCycles,
               outputElapsed, totalOutputElapsed / statCycles);
    }
}

int main(array<System::String ^> ^args)
{
    Console::Write("Starting simulation...");
    Simulation sim;
    std::thread statsThread(RunStats, sim);
    Console::WriteLine("done!");

    Console::Write("Setting up server...");
    auto listener = gcnew HttpListener();
    listener->Prefixes->Add("http://localhost:9914/");
    listener->Start();
    Console::WriteLine("done!");
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
            std::string state = sim.ToString();
            StreamWriter^ writer = gcnew StreamWriter(ctxt->Response->OutputStream);
            writer->Write(state.c_str());
            delete writer;
        }
        else
        {
            StreamReader^ reader = gcnew StreamReader(ctxt->Request->InputStream);
            sim.ApplySettings(StringToString(reader->ReadToEnd()));
            delete reader;
            delete ctxt->Response->OutputStream;
        }
#ifdef _DEBUG
        Console::Write("!");
#endif
    }

    return 0;
}
