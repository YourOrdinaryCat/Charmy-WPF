#define WIN32_LEAN_AND_MEAN
#include <stdlib.h>
#include <windows.h>

#pragma comment(lib, "USER32")
#pragma comment(linker, "/SUBSYSTEM:WINDOWS")

#define KEYDOWN(k) ((k) & 0x80)

// This is a **very** minimal hotcorner app, written in C. Maybe its not the
// optimal way to do this, but it works for me.
//
// Zero state is stored anywhere, no registry keys or configuration files.
//
// - If you want to configure something, edit the code.
// - If you want to uninstall it, just delete it.
//
// Tavis Ormandy <taviso@cmpxchg8b.com> December, 2016
//
// https://github.com/taviso/hotcorner
//

extern "C" {
	// Hot corner area offsets relative to top left corner of given monitor.
	static const RECT kHotCornerOffsets = {
		-10,
		-10,
		+10,
		+10,
	};

#define MAX_MONITORS 4
	// If the mouse enters any of these rectangles, activate the hot corner function.
	static RECT gHotCorners[MAX_MONITORS];
	static INT gMonitorsCnt = 0;

	// Input to inject when corner activated (Win+Tab by default).
	static INPUT kCornerInput[] = {
		{ INPUT_KEYBOARD, { VK_LWIN, 0 } },
		{ INPUT_KEYBOARD, { VK_TAB, 0 } },
		{ INPUT_KEYBOARD, { VK_TAB, KEYEVENTF_KEYUP } },
		{ INPUT_KEYBOARD, { VK_LWIN, KEYEVENTF_KEYUP } },
	};

	// How long cursor has to linger in the kHotCorner RECT to trigger input.
	static UINT kHotDelay = 0;

	static HANDLE CornerThread = INVALID_HANDLE_VALUE;

	static BOOL IsPointWithinHotCorner(POINT pt)
	{
		for (int i = 0; i < gMonitorsCnt; i++) {
			if (PtInRect(&gHotCorners[i], pt)) {
				return TRUE;
			}
		}
		return FALSE;
	}

	// This thread runs when the cursor enters the hot corner, and waits to see if the cursor stays in the corner.
	// If the mouse leaves while we're waiting, the thread is just terminated.
	static ULONG WINAPI CornerHotFunc(LPVOID lpParameter)
	{
		BYTE KeyState[256];
		POINT Point;

		Sleep(kHotDelay);

		// Check if a mouse putton is pressed, maybe a drag operation?
		if (GetKeyState(VK_LBUTTON) < 0 || GetKeyState(VK_RBUTTON) < 0) {
			return 0;
		}

		// Check if any modifier keys are pressed.
		if (GetKeyboardState(KeyState)) {
			if (KEYDOWN(KeyState[VK_SHIFT]) || KEYDOWN(KeyState[VK_CONTROL])
				|| KEYDOWN(KeyState[VK_MENU]) || KEYDOWN(KeyState[VK_LWIN])
				|| KEYDOWN(KeyState[VK_RWIN])) {
				return 0;
			}
		}

		// Verify the corner is still hot
		if (GetCursorPos(&Point) == FALSE) {
			return 1;
		}

		// Check co-ordinates.
		if (IsPointWithinHotCorner(Point)) {
			if (SendInput(_countof(kCornerInput), kCornerInput, sizeof(INPUT)) != _countof(kCornerInput)) {
				return 1;
			}
		}

		return 0;
	}

	// Iterates over monitors.
	static BOOL CALLBACK MonitorEnumProc(HMONITOR hMonitor, HDC hdcMonitor, LPRECT lprcMonitor, LPARAM dwData)
	{
		gHotCorners[gMonitorsCnt].top = lprcMonitor->top + kHotCornerOffsets.top;
		gHotCorners[gMonitorsCnt].left = lprcMonitor->left + kHotCornerOffsets.left;
		gHotCorners[gMonitorsCnt].bottom = lprcMonitor->top + kHotCornerOffsets.bottom;
		gHotCorners[gMonitorsCnt].right = lprcMonitor->left + kHotCornerOffsets.right;
		return ++gMonitorsCnt < MAX_MONITORS;
	}

	static LRESULT CALLBACK MouseHookCallback(int nCode, WPARAM wParam, LPARAM lParam)
	{
		MSLLHOOKSTRUCT *evt = (MSLLHOOKSTRUCT *)lParam;
		POINT Point;

		// If the mouse hasn't moved, we're done.
		if (wParam != WM_MOUSEMOVE)
			goto finish;

		// If no hot corner is active, update monitors positions (preventing data race)
		if (CornerThread == INVALID_HANDLE_VALUE) {
			gMonitorsCnt = 0;
			EnumDisplayMonitors(NULL, NULL, MonitorEnumProc, 0);
		}

		// Need position compatible with EnumDisplayMonitors.
		if (GetCursorPos(&Point) == FALSE)
			goto finish;

		// Check if the cursor is hot or cold.
		if (!IsPointWithinHotCorner(Point)) {

			// The corner is cold, and was cold before.
			if (CornerThread == INVALID_HANDLE_VALUE)
				goto finish;

			// The corner is cold, but was previously hot.
			TerminateThread(CornerThread, 0);

			CloseHandle(CornerThread);

			// Reset state.
			CornerThread = INVALID_HANDLE_VALUE;

			goto finish;
		}

		// The corner is hot, check if it was already hot.
		if (CornerThread != INVALID_HANDLE_VALUE) {
			goto finish;
		}

		// Check if a mouse putton is pressed, maybe a drag operation?
		if (GetKeyState(VK_LBUTTON) < 0 || GetKeyState(VK_RBUTTON) < 0) {
			goto finish;
		}

		// The corner is hot, and was previously cold. Here we start a thread to
		// monitor if the mouse lingers.
		CornerThread = CreateThread(NULL, 0, CornerHotFunc, NULL, 0, NULL);

	finish:
		return CallNextHookEx(NULL, nCode, wParam, lParam);
	}

	static HANDLE Proc;

	__declspec(dllexport) int CALLBACK BeginTracking()
	{
		Proc = GetCurrentProcess();

		// Setting higher priority for process not to lag when system busy
		SetPriorityClass(Proc, REALTIME_PRIORITY_CLASS);

		MSG Msg;
		HHOOK MouseHook;

		if (!(MouseHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookCallback, NULL, 0)))
			return 1;

		while (GetMessage(&Msg, NULL, 0, 0)) {
			if (Msg.message == WM_HOTKEY) {
				break;
			}
			DispatchMessage(&Msg);
		}

		UnhookWindowsHookEx(MouseHook);

		return Msg.wParam;
	}

	__declspec(dllexport) void CALLBACK StopTracking()
	{
		TerminateProcess(Proc, 1);
		CloseHandle(Proc);
	}

	__declspec(dllexport) UINT CALLBACK GetDelay()
	{
		return kHotDelay;
	}

	__declspec(dllexport) void CALLBACK SetDelay(UINT delay)
	{
		kHotDelay = delay;
	}
}