#include "pch.h"
#include "ScopeTiming.h"
#include "Global.h"

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

void RunStats()
{
    while (true)
    {
        std::this_thread::sleep_for(std::chrono::seconds(3));
        printf("%s\n\n", ScopeTiming::GetObj().GetSummary().c_str());
    }
}

int main(array<System::String ^> ^args)
{
    ScopeTiming::GetObj().Init(true);

    Console::Write("Starting simulation...");
    StartSimulation();
    Console::WriteLine("done!");

    Console::Write("Setting up server...");
    auto listener = gcnew HttpListener();
    listener->Prefixes->Add("http://localhost:9914/");
    listener->Start();
    Console::WriteLine("done!");

    std::thread statsThread(RunStats);

    Stopwatch sw;
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

            std::string state = GetSimState();
            ScopeTiming::GetObj().RecordScope("Output.ToString", sw);

            StreamWriter^ writer = gcnew StreamWriter(ctxt->Response->OutputStream);
            writer->Write(gcnew String(state.c_str()));
            delete writer;
            ScopeTiming::GetObj().RecordScope("Output.StreamWriter", sw);
        }
        else
        {
            sw.Start();

            StreamReader^ reader = gcnew StreamReader(ctxt->Request->InputStream);
            std::string settings = StringToString(reader->ReadToEnd());
            delete reader;
            ScopeTiming::GetObj().RecordScope("Settings.StreamReader", sw);

            ApplySimSettings(settings);
            ScopeTiming::GetObj().RecordScope("Settings.Apply", sw);
        }
        delete ctxt->Response->OutputStream;
#ifdef _DEBUG
        Console::Write("!");
#endif
    }

    return 0;
}
