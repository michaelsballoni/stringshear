#pragma once

#ifdef WIN32
#include <windows.h>

class CSection
{
public:
	CSection() { ::InitializeCriticalSection(&m_cs); }
	~CSection() { ::DeleteCriticalSection(&m_cs); }

	void Lock() { ::EnterCriticalSection(&m_cs); }
	void Unlock() { ::LeaveCriticalSection(&m_cs); }

private:
	CRITICAL_SECTION m_cs;
};

class CSLock
{
public:
	CSLock(CSection& cs) : m_cs(cs) { m_cs.Lock(); }
	~CSLock() { m_cs.Unlock(); }

private:
	CSection& m_cs;
};
#else
#include <mutex>
typedef std::recursive_mutex CSection;
typedef std::lock_guard<std::recursive_mutex> CSLock;
#endif