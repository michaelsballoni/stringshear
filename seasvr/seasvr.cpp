#include "pch.h"
#include "../sealib/sealib.h"

#include <msclr\marshal.h>

#include <thread>

using namespace System;
using namespace System::IO;
using namespace System::Net;
using namespace System::Diagnostics;

using namespace msclr::interop;

std::string StringToString(String^ netStr)
{
    pin_ptr<const wchar_t> wstrPtr = PtrToStringChars(netStr);
    std::wstring wstr(wstrPtr);
    std::string str(wstr.begin(), wstr.end());
    return str;
}

void RunStats()
{
    while (true)
    {
        std::this_thread::sleep_for(std::chrono::seconds(3));
        
        std::string seaTiming = GetTimingSummary();
        std::string svrTiming = StringToString(StringShear::ScopeTiming::Summary);

        std::string timing;
        if (!seaTiming.empty())
            timing += seaTiming;

        if (!svrTiming.empty())
        {
            if (!timing.empty())
                timing += '\n';
            timing += svrTiming;
        }

        if (!timing.empty())
            timing += '\n';

        printf("%s\n", timing.c_str());
    }
}

int main(array<System::String ^> ^args)
{
    Console::Write("Starting simulation...");
    StartSimulation();
    Console::WriteLine("done!");

    Console::Write("Setting up server...");
    auto listener = gcnew HttpListener();
    listener->Prefixes->Add("http://localhost:9914/");
    listener->Start();
    Console::WriteLine("done!");

    std::thread statsThread(RunStats);

    Stopwatch^ sw = gcnew Stopwatch();
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
            sw->Restart();

            std::string state = GetSimState();
            StringShear::ScopeTiming::RecordScope("Output.ToString", sw);

            StreamWriter^ writer = gcnew StreamWriter(ctxt->Response->OutputStream);
            writer->Write(gcnew String(state.c_str()));
            delete writer;
            StringShear::ScopeTiming::RecordScope("Output.StreamWriter", sw);
        }
        else
        {
            sw->Restart();

            StreamReader^ reader = gcnew StreamReader(ctxt->Request->InputStream);
            std::string settings = StringToString(reader->ReadToEnd());
            delete reader;
            StringShear::ScopeTiming::RecordScope("Settings.StreamReader", sw);

            ApplySimSettings(settings.c_str());
            StringShear::ScopeTiming::RecordScope("Settings.Apply", sw);
        }
        delete ctxt->Response->OutputStream;
#ifdef _DEBUG
        Console::Write("!");
#endif
    }

    return 0;
}
