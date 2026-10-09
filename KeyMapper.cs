using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Timers;
using Windows.System;
using static KeyMapper.CmdShow;
using static KeyMapper.ExtendedWindowStyles;
using static KeyMapper.GetWindowLongOffset;
using static KeyMapper.HookFlags;
using static KeyMapper.HookType;
using static KeyMapper.IputEventTypes;
using static KeyMapper.KeyboardEventFlags;
using static KeyMapper.KeyboardMsg;
using static KeyMapper.UCmd;
using static KeyMapper.VirtualKeys;
using static KeyMapper.Win32Helper;
using static KeyMapper.WindowStyles;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KeyMapper
{
    public struct UCmd
    {
        public const int GW_HWNDFIRST		= 0;
        public const int GW_HWNDLAST		= 1;
        public const int GW_HWNDNEXT		= 2;
        public const int GW_HWNDPREV		= 3;
        public const int GW_OWNER		    = 4;
        public const int GW_CHILD		    = 5;
        public const int GW_ENABLEDPOPUP	= 6;
        public const int GW_MAX		        = 6;
	}
	public struct GetWindowLongOffset
    {
        public const int GWL_WNDPROC		= -4;
        public const int GWL_HINSTANCE		= -6;
        public const int GWL_HWNDPARENT		= -8;
        public const int GWL_STYLE		    = -16;
        public const int GWL_EXSTYLE		= -20;
        public const int GWL_USERDATA		= -21;
        public const int GWL_ID		        = -12;
    }
    public struct WindowStyles
    {
        public const uint WS_OVERLAPPED		    = 0x00000000;
        public const uint WS_POPUP		        = 0x80000000;
        public const uint WS_CHILD		        = 0x40000000;
        public const uint WS_MINIMIZE		    = 0x20000000;
        public const uint WS_VISIBLE		    = 0x10000000;
        public const uint WS_DISABLED		    = 0x08000000;
        public const uint WS_CLIPSIBLINGS		= 0x04000000;
        public const uint WS_CLIPCHILDREN		= 0x02000000;
        public const uint WS_MAXIMIZE		    = 0x01000000;
        public const uint WS_CAPTION		    = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
        public const uint WS_BORDER		        = 0x00800000;
        public const uint WS_DLGFRAME		    = 0x00400000;
        public const uint WS_VSCROLL		    = 0x00200000;
        public const uint WS_HSCROLL		    = 0x00100000;
        public const uint WS_SYSMENU		    = 0x00080000;
        public const uint WS_THICKFRAME		    = 0x00040000;
        public const uint WS_GROUP		        = 0x00020000;
        public const uint WS_TABSTOP		    = 0x00010000;
        public const uint WS_MINIMIZEBOX		= 0x00020000;
        public const uint WS_MAXIMIZEBOX		= 0x00010000;
        public const uint WS_TILED		        = WS_OVERLAPPED;
        public const uint WS_ICONIC		        = WS_MINIMIZE;
        public const uint WS_SIZEBOX		    = WS_THICKFRAME;
        public const uint WS_TILEDWINDOW		= WS_OVERLAPPEDWINDOW;
        public const uint WS_OVERLAPPEDWINDOW	= WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        public const uint WS_POPUPWINDOW		= WS_POPUP | WS_BORDER | WS_SYSMENU;
        public const uint WS_CHILDWINDOW		= WS_CHILD;
    }
    public struct ExtendedWindowStyles
    {
        public const uint WS_EX_DLGMODALFRAME		    = 0x00000001;
        public const uint WS_EX_NOPARENTNOTIFY		    = 0x00000004;
        public const uint WS_EX_TOPMOST		            = 0x00000008;
        public const uint WS_EX_ACCEPTFILES		        = 0x00000010;
        public const uint WS_EX_TRANSPARENT		        = 0x00000020;
        public const uint WS_EX_MDICHILD		        = 0x00000040;
        public const uint WS_EX_TOOLWINDOW		        = 0x00000080;
        public const uint WS_EX_WINDOWEDGE		        = 0x00000100;
        public const uint WS_EX_CLIENTEDGE		        = 0x00000200;
        public const uint WS_EX_CONTEXTHELP		        = 0x00000400;
        public const uint WS_EX_RIGHT		            = 0x00001000;
        public const uint WS_EX_LEFT		            = 0x00000000;
        public const uint WS_EX_RTLREADING		        = 0x00002000;
        public const uint WS_EX_LTRREADING		        = 0x00000000;
        public const uint WS_EX_LEFTSCROLLBAR		    = 0x00004000;
        public const uint WS_EX_RIGHTSCROLLBAR		    = 0x00000000;
        public const uint WS_EX_CONTROLPARENT		    = 0x00010000;
        public const uint WS_EX_STATICEDGE		        = 0x00020000;
        public const uint WS_EX_APPWINDOW		        = 0x00040000;
        public const uint WS_EX_OVERLAPPEDWINDOW		= WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE;
        public const uint WS_EX_PALETTEWINDOW		    = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
        public const uint WS_EX_LAYERED		            = 0x00080000;
        public const uint WS_EX_NOINHERITLAYOUT		    = 0x00100000; // Disable inheritence of mirroring by children
        public const uint WS_EX_NOREDIRECTIONBITMAP		= 0x00200000;
        public const uint WS_EX_LAYOUTRTL		        = 0x00400000; // Right to left mirroring
        public const uint WS_EX_COMPOSITED		        = 0x02000000;
        public const uint WS_EX_NOACTIVATE		        = 0x08000000;
    }
	public struct HookType
    {
        public const int WH_MIN				= -1;
        public const int WH_MSGFILTER		= -1;
        public const int WH_JOURNALRECORD	= 0;   // OBSOLETE: discontinued
        public const int WH_JOURNALPLAYBACK	= 1;   // OBSOLETE: discontinued
        public const int WH_KEYBOARD		= 2;
        public const int WH_GETMESSAGE		= 3;
        public const int WH_CALLWNDPROC		= 4;
        public const int WH_CBT				= 5;
        public const int WH_SYSMSGFILTER	= 6;
        public const int WH_MOUSE			= 7;
        public const int WH_HARDWARE		= 8;
        public const int WH_DEBUG			= 9;
        public const int WH_SHELL			= 10;
        public const int WH_FOREGROUNDIDLE	= 11;
        public const int WH_CALLWNDPROCRET	= 12;
        public const int WH_KEYBOARD_LL		= 13;
        public const int WH_MOUSE_LL		= 14;
        public const int WH_MAX				= 14;
        public const int WH_MINHOOK			= WH_MIN;
        public const int WH_MAXHOOK			= WH_MAX;
    }
	public struct HookFlags
	{
		public const int LLKHF_EXTENDED				= 0x00000001;
		public const int LLKHF_INJECTED				= 0x00000010;
		public const int LLKHF_ALTDOWN				= 0x00000020;
		public const int LLKHF_UP					= 0x00000080;
		public const int LLKHF_LOWER_IL_INJECTED	= 0x00000002;
		public const int LLMHF_INJECTED				= 0x00000001;
		public const int LLMHF_LOWER_IL_INJECTED	= 0x00000002;
	}
	public struct KeyboardMsg
    {
        public const int WM_KEYFIRST	= 0x0100;
        public const int WM_KEYDOWN		= 0x0100;
        public const int WM_KEYUP		= 0x0101;
        public const int WM_CHAR		= 0x0102;
        public const int WM_DEADCHAR	= 0x0103;
        public const int WM_SYSKEYDOWN	= 0x0104;
        public const int WM_SYSKEYUP	= 0x0105;
        public const int WM_SYSCHAR		= 0x0106;
        public const int WM_SYSDEADCHAR	= 0x0107;
        public const int WM_UNICHAR		= 0x0109;
        public const int WM_KEYLAST		= 0x0109;
        public const int UNICODE_NOCHAR	= 0xFFFF;
    }
    public struct VirtualKeys
    {
        public const int VK_LBUTTON					        = 0x01;
        public const int VK_RBUTTON					        = 0x02;
        public const int VK_CANCEL					        = 0x03;
        public const int VK_MBUTTON					        = 0x04;    // NOT contiguous with L & RBUTTON
        public const int VK_XBUTTON1					    = 0x05;    // NOT contiguous with L & RBUTTON
        public const int VK_XBUTTON2					    = 0x06;    // NOT contiguous with L & RBUTTON
        public const int VK_BACK					        = 0x08;
        public const int VK_TAB					            = 0x09;
        public const int VK_CLEAR					        = 0x0C;
        public const int VK_RETURN					        = 0x0D;
        public const int VK_SHIFT					        = 0x10;
        public const int VK_CONTROL					        = 0x11;
        public const int VK_MENU					        = 0x12;
        public const int VK_PAUSE					        = 0x13;
        public const int VK_CAPITAL					        = 0x14;
        public const int VK_KANA					        = 0x15;
        public const int VK_HANGEUL					        = 0x15;  // old name - should be here for compatibility
        public const int VK_HANGUL					        = 0x15;
        public const int VK_IME_ON					        = 0x16;
        public const int VK_JUNJA					        = 0x17;
        public const int VK_FINAL					        = 0x18;
        public const int VK_HANJA					        = 0x19;
        public const int VK_KANJI					        = 0x19;
        public const int VK_IME_OFF					        = 0x1A;
        public const int VK_ESCAPE					        = 0x1B;
        public const int VK_CONVERT					        = 0x1C;
        public const int VK_NONCONVERT					    = 0x1D;
        public const int VK_ACCEPT					        = 0x1E;
        public const int VK_MODECHANGE					    = 0x1F;
        public const int VK_SPACE					        = 0x20;
        public const int VK_PRIOR					        = 0x21;
        public const int VK_NEXT					        = 0x22;
        public const int VK_END					            = 0x23;
        public const int VK_HOME					        = 0x24;
        public const int VK_LEFT					        = 0x25;
        public const int VK_UP					            = 0x26;
        public const int VK_RIGHT					        = 0x27;
        public const int VK_DOWN					        = 0x28;
        public const int VK_SELECT					        = 0x29;
        public const int VK_PRINT					        = 0x2A;
        public const int VK_EXECUTE					        = 0x2B;
        public const int VK_SNAPSHOT					    = 0x2C;
        public const int VK_INSERT					        = 0x2D;
        public const int VK_DELETE					        = 0x2E;
        public const int VK_HELP					        = 0x2F;
        public const int VK_LWIN					        = 0x5B;
        public const int VK_RWIN					        = 0x5C;
        public const int VK_APPS					        = 0x5D;
        public const int VK_SLEEP					        = 0x5F;
        public const int VK_NUMPAD0					        = 0x60;
        public const int VK_NUMPAD1					        = 0x61;
        public const int VK_NUMPAD2					        = 0x62;
        public const int VK_NUMPAD3					        = 0x63;
        public const int VK_NUMPAD4					        = 0x64;
        public const int VK_NUMPAD5					        = 0x65;
        public const int VK_NUMPAD6					        = 0x66;
        public const int VK_NUMPAD7					        = 0x67;
        public const int VK_NUMPAD8					        = 0x68;
        public const int VK_NUMPAD9					        = 0x69;
        public const int VK_MULTIPLY					    = 0x6A;
        public const int VK_ADD					            = 0x6B;
        public const int VK_SEPARATOR					    = 0x6C;
        public const int VK_SUBTRACT					    = 0x6D;
        public const int VK_DECIMAL					        = 0x6E;
        public const int VK_DIVIDE					        = 0x6F;
        public const int VK_F1					            = 0x70;
        public const int VK_F2					            = 0x71;
        public const int VK_F3					            = 0x72;
        public const int VK_F4					            = 0x73;
        public const int VK_F5					            = 0x74;
        public const int VK_F6					            = 0x75;
        public const int VK_F7					            = 0x76;
        public const int VK_F8					            = 0x77;
        public const int VK_F9					            = 0x78;
        public const int VK_F10					            = 0x79;
        public const int VK_F11					            = 0x7A;
        public const int VK_F12					            = 0x7B;
        public const int VK_F13					            = 0x7C;
        public const int VK_F14					            = 0x7D;
        public const int VK_F15					            = 0x7E;
        public const int VK_F16					            = 0x7F;
        public const int VK_F17					            = 0x80;
        public const int VK_F18					            = 0x81;
        public const int VK_F19					            = 0x82;
        public const int VK_F20					            = 0x83;
        public const int VK_F21					            = 0x84;
        public const int VK_F22					            = 0x85;
        public const int VK_F23					            = 0x86;
        public const int VK_F24					            = 0x87;
        public const int VK_NAVIGATION_VIEW				    = 0x88; // reserved
        public const int VK_NAVIGATION_MENU				    = 0x89; // reserved
        public const int VK_NAVIGATION_UP				    = 0x8A; // reserved
        public const int VK_NAVIGATION_DOWN				    = 0x8B; // reserved
        public const int VK_NAVIGATION_LEFT				    = 0x8C; // reserved
        public const int VK_NAVIGATION_RIGHT				= 0x8D; // reserved
        public const int VK_NAVIGATION_ACCEPT				= 0x8E; // reserved
        public const int VK_NAVIGATION_CANCEL				= 0x8F; // reserved
        public const int VK_NUMLOCK				            = 0x90;
        public const int VK_SCROLL				            = 0x91;
        public const int VK_OEM_NEC_EQUAL				    = 0x92;   // '=' key on numpad
        public const int VK_OEM_FJ_JISHO				    = 0x92;   // 'Dictionary' key
        public const int VK_OEM_FJ_MASSHOU				    = 0x93;   // 'Unregister word' key
        public const int VK_OEM_FJ_TOUROKU				    = 0x94;   // 'Register word' key
        public const int VK_OEM_FJ_LOYA				        = 0x95;   // 'Left OYAYUBI' key
        public const int VK_OEM_FJ_ROYA				        = 0x96;   // 'Right OYAYUBI' key
        public const int VK_LSHIFT				            = 0xA0;
        public const int VK_RSHIFT				            = 0xA1;
        public const int VK_LCONTROL				        = 0xA2;
        public const int VK_RCONTROL				        = 0xA3;
        public const int VK_LMENU				            = 0xA4;
        public const int VK_RMENU				            = 0xA5;
        public const int VK_BROWSER_BACK				    = 0xA6;
        public const int VK_BROWSER_FORWARD				    = 0xA7;
        public const int VK_BROWSER_REFRESH				    = 0xA8;
        public const int VK_BROWSER_STOP				    = 0xA9;
        public const int VK_BROWSER_SEARCH				    = 0xAA;
        public const int VK_BROWSER_FAVORITES				= 0xAB;
        public const int VK_BROWSER_HOME				    = 0xAC;
        public const int VK_VOLUME_MUTE				        = 0xAD;
        public const int VK_VOLUME_DOWN				        = 0xAE;
        public const int VK_VOLUME_UP				        = 0xAF;
        public const int VK_MEDIA_NEXT_TRACK				= 0xB0;
        public const int VK_MEDIA_PREV_TRACK				= 0xB1;
        public const int VK_MEDIA_STOP				        = 0xB2;
        public const int VK_MEDIA_PLAY_PAUSE				= 0xB3;
        public const int VK_LAUNCH_MAIL				        = 0xB4;
        public const int VK_LAUNCH_MEDIA_SELECT				= 0xB5;
        public const int VK_LAUNCH_APP1				        = 0xB6;
        public const int VK_LAUNCH_APP2				        = 0xB7;
        public const int VK_OEM_1				            = 0xBA;   // ';:' for US
        public const int VK_OEM_PLUS				        = 0xBB;   // '+' any country
        public const int VK_OEM_COMMA				        = 0xBC;   // ',' any country
        public const int VK_OEM_MINUS				        = 0xBD;   // '-' any country
        public const int VK_OEM_PERIOD				        = 0xBE;   // '.' any country
        public const int VK_OEM_2				            = 0xBF;   // '/?' for US
        public const int VK_OEM_3				            = 0xC0;   // '`~' for US
        public const int VK_GAMEPAD_A				        = 0xC3; // reserved
        public const int VK_GAMEPAD_B				        = 0xC4; // reserved
        public const int VK_GAMEPAD_X				        = 0xC5; // reserved
        public const int VK_GAMEPAD_Y				        = 0xC6; // reserved
        public const int VK_GAMEPAD_RIGHT_SHOULDER			= 0xC7; // reserved
        public const int VK_GAMEPAD_LEFT_SHOULDER			= 0xC8; // reserved
        public const int VK_GAMEPAD_LEFT_TRIGGER			= 0xC9; // reserved
        public const int VK_GAMEPAD_RIGHT_TRIGGER			= 0xCA; // reserved
        public const int VK_GAMEPAD_DPAD_UP				    = 0xCB; // reserved
        public const int VK_GAMEPAD_DPAD_DOWN				= 0xCC; // reserved
        public const int VK_GAMEPAD_DPAD_LEFT				= 0xCD; // reserved
        public const int VK_GAMEPAD_DPAD_RIGHT				= 0xCE; // reserved
        public const int VK_GAMEPAD_MENU				    = 0xCF; // reserved
        public const int VK_GAMEPAD_VIEW				    = 0xD0; // reserved
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON	= 0xD1; // reserved
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON	= 0xD2; // reserved
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_UP		= 0xD3; // reserved
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_DOWN	= 0xD4; // reserved
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT	= 0xD5; // reserved
        public const int VK_GAMEPAD_LEFT_THUMBSTICK_LEFT	= 0xD6; // reserved
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_UP		= 0xD7; // reserved
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN	= 0xD8; // reserved
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT	= 0xD9; // reserved
        public const int VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT	= 0xDA; // reserved
        public const int VK_OEM_4				            = 0xDB;  //  '[{' for US
        public const int VK_OEM_5				            = 0xDC;  //  '\|' for US
        public const int VK_OEM_6				            = 0xDD;  //  ']}' for US
        public const int VK_OEM_7				            = 0xDE;  //  ''"' for US
        public const int VK_OEM_8				            = 0xDF;
        public const int VK_OEM_AX				            = 0xE1;  //  'AX' key on Japanese AX kbd
        public const int VK_OEM_102				            = 0xE2;  //  "<>" or "\|" on RT 102-key kbd.
        public const int VK_ICO_HELP				        = 0xE3;  //  Help key on ICO
        public const int VK_ICO_00				            = 0xE4;  //  00 key on ICO
        public const int VK_PROCESSKEY				        = 0xE5;
        public const int VK_ICO_CLEAR				        = 0xE6;
        public const int VK_PACKET				            = 0xE7;
        public const int VK_OEM_RESET				        = 0xE9;
        public const int VK_OEM_JUMP				        = 0xEA;
        public const int VK_OEM_PA1				            = 0xEB;
        public const int VK_OEM_PA2				            = 0xEC;
        public const int VK_OEM_PA3				            = 0xED;
        public const int VK_OEM_WSCTRL				        = 0xEE;
        public const int VK_OEM_CUSEL				        = 0xEF;
        public const int VK_OEM_ATTN				        = 0xF0;
        public const int VK_OEM_FINISH				        = 0xF1;
        public const int VK_OEM_COPY				        = 0xF2;
        public const int VK_OEM_AUTO				        = 0xF3;
        public const int VK_OEM_ENLW				        = 0xF4;
        public const int VK_OEM_BACKTAB				        = 0xF5;
        public const int VK_ATTN				            = 0xF6;
        public const int VK_CRSEL				            = 0xF7;
        public const int VK_EXSEL				            = 0xF8;
        public const int VK_EREOF				            = 0xF9;
        public const int VK_PLAY				            = 0xFA;
        public const int VK_ZOOM				            = 0xFB;
        public const int VK_NONAME				            = 0xFC;
        public const int VK_PA1				                = 0xFD;
        public const int VK_OEM_CLEAR                       = 0xFE;
    }
    public struct CmdShow
    {
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_MAX = 11;
    }
    public struct IputEventTypes
    {
        public const int INPUT_MOUSE    = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;
    }
    public struct KeyboardEventFlags
    {
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP       = 0x0002;
        public const uint KEYEVENTF_UNICODE     = 0x0004;
        public const uint KEYEVENTF_SCANCODE    = 0x0008;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public DWORD vkCode;
        public DWORD scanCode;
        public DWORD flags;
        public DWORD time;
        public ULONG_PTR dwExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public DWORD uMsg;
        public WORD wParamL;
        public WORD wParamH;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public LONG dx;
        public LONG dy;
        public DWORD mouseData;
        public DWORD dwFlags;
        public DWORD time;
        public ULONG_PTR dwExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public WORD wVk;
        public WORD wScan;
        public DWORD dwFlags;
        public DWORD time;
        public ULONG_PTR dwExtraInfo;
	}
    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_UNION
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public DWORD type;
        //public INPUT_UNION DUMMYUNIONNAME;
        public KEYBDINPUT ki;
		public void SetKeyboardInput(INT vkCode, DWORD dwFlags = 0)
		{
			type = INPUT_KEYBOARD;
			ki = new KEYBDINPUT()
			{
				wVk = (WORD)vkCode,
				wScan = 0,
				dwFlags = dwFlags,
				time = 0,
				dwExtraInfo = (ULONG_PTR)GetMessageExtraInfo()
			};
		}
	}
	public class WindowInfo(HWND hwnd, Process process, string? title)
    {
        public HWND Hwnd { get; } = hwnd;
        public Process Process { get; } = process;
        public string Title { get; } = title ?? "";
    }
    public class Win32Helper
    {
        public delegate LRESULT LowLevelKeyboardProcDlgt(int nCode, WPARAM wParam, LPARAM lParam);
        public delegate BOOL EnumWindowsProcDlgt(HWND hwnd, LPARAM lParam);
		public static int[] ModifierKeys { get; } = [VK_LCONTROL, VK_RCONTROL, VK_CONTROL, VK_LSHIFT, VK_RSHIFT, VK_SHIFT, VK_LWIN, VK_RWIN, VK_LMENU, VK_RMENU, VK_MENU];
		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern HMODULE GetModuleHandle([In, Optional] LPCTSTR lpModuleName);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern BOOL EnumWindows(
            [In] WNDENUMPROC lpEnumFunc,
            [In] LPARAM lParam);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern HWND GetWindow(
            [In] HWND hWnd,
            [In] UINT uCmd);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern HWND GetShellWindow();
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern HWND GetForegroundWindow();
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern DWORD GetWindowThreadProcessId(
            [In] HWND hWnd,
            [Out, Optional] out DWORD lpdwProcessId);
        [DllImport("User32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern LONG GetWindowLong32(
            [In] HWND hWnd,
            [In] int nIndex);
        [DllImport("User32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern LONG_PTR GetWindowLong64(
            [In] HWND hWnd,
            [In] int nIndex);
        public static LONG GetWindowLong(HWND hWnd, int nIndex) => nint.Size == 8 ? (LONG)GetWindowLong64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowTextLength([In] HWND hWnd);
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(
            [In] HWND hWnd,
            [In, Out] StringBuilder lpString,
            [In] int nMaxCount);
        public static string? GetWindowTitle(HWND hWnd)
        {
            string? title = null;
            try
            {
                int length = GetWindowTextLength(hWnd);
                if(length > 0)
                {
                    length++;
					StringBuilder builder = new StringBuilder(length);
                    if(GetWindowText(hWnd, builder, length) > 0)
                    {
                        title = builder.ToString();
					}
				}
			}
            catch(Exception e)
            {
				title = null;
				Debug.WriteLine(e);
            }
            return title;
        }
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern HHOOK SetWindowsHookEx(
            [In] int idHook,
            [In] HOOKPROC lpfn,
            [In] HINSTANCE hmod,
            [In] DWORD dwThreadId);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern BOOL UnhookWindowsHookEx([In] HHOOK hhk);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern LRESULT CallNextHookEx(
            [In, Optional] HHOOK hhk,
            [In] int nCode,
            [In] WPARAM wParam,
            [In] LPARAM lParam);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern UINT SendInput(
            [In] UINT cInputs,
            [In] INPUT[] pInputs,
            [In] int cbSize);
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern void keybd_event(
			[In] BYTE bVk,
			[In] BYTE bScan,
			[In] DWORD dwFlags,
			[In] ULONG_PTR dwExtraInfo);
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern LPARAM GetMessageExtraInfo();
	}
	public class Shortcut : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged = delegate { };
		public HashSet<int> KeyCodes
		{
			get;
			set
			{
				if(field != value)
				{
					field = value;
					OnPropertyChanged("KeyCodes");
					OnPropertyChanged("Description");
					OnPropertyChanged("IsModifierKeysOnly");
				}
			}
		} = new HashSet<int>();
		public string? Description
		{
			get
			{
				return string.Join('+', KeyCodes.Select(vKey => Enum.GetName((VirtualKey)vKey)));
			}
		}
		public bool IsModifierKeysOnly
		{
			get
			{
				return KeyCodes.All(vKey => ModifierKeys.Contains(vKey));
			}
		}
		private void OnPropertyChanged([CallerMemberName] string PropertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
		}
	}
	public class ShortcutPair : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged = delegate { };
		public Shortcut CurrentShortcut
		{
			get;
			set
			{
				if(field != value)
				{
					field = value;
					OnPropertyChanged("CurrentShortcut");
				}
			}
		}
		public Shortcut NewShortcut
		{
			get;
			set
			{
				if(field != value)
				{
					field = value;
					OnPropertyChanged("NewShortcut");
				}
			}
		}
		public bool IsSelected
		{
			get;
			set
			{
				if(field != value)
				{
					field = value;
					OnPropertyChanged("IsSelected");
				}
			}
		}
		public ShortcutPair()
		{
			CurrentShortcut = new Shortcut();
			NewShortcut = new Shortcut();
		}
		private void OnPropertyChanged([CallerMemberName] string PropertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
		}
	}
	public class KeyMapper : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
		private string SettingsFilePath { get; }
        private HWND ShellWindowHandler { get; }
        private HHOOK hHook = 0;
        private HOOKPROC hookProc;
        private WNDENUMPROC wndEnumProc;
		private HashSet<int> PressedKeys { get; } = [];
		public List<WindowInfo> ActiveWindows 
		{ 
			get;
			set
			{
				if(field != value)
				{
					field = value;
					OnPropertyChanged("ActiveWindows");
				}
			}
		}
		public ObservableCollection<ShortcutPair> ShortcutMap { get; } = new ObservableCollection<ShortcutPair>();
        public WindowInfo? SelectedWindowInfo
		{
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    OnPropertyChanged("SelectedWindowInfo");
                }
            }
        }
        public ShortcutPair? SelectedShortcutPair
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    OnPropertyChanged("SelectedShortcutPair");
                }
            }
        }
        public KeyMapper()
        {
            SettingsFilePath = GetSettingsFileName();
            ShellWindowHandler = GetShellWindow();
            hookProc = LowLevelKeyboardProc;
            wndEnumProc = EnumWindowsProc;
			ActiveWindows = GetActiveWindows();
		}
		public List<WindowInfo> GetActiveWindows()
		{
			List<WindowInfo> listWindows = new List<WindowInfo>();
			GCHandle handle = GCHandle.Alloc(listWindows, GCHandleType.Normal);
			try
			{
				nint ptr = GCHandle.ToIntPtr(handle);
				EnumWindows(wndEnumProc, ptr);
			}
			finally
			{
				if(handle.IsAllocated)
				{
					handle.Free();
				}
				listWindows = listWindows.OrderBy(curWindowInfo => curWindowInfo.Title).ToList();
			}
			Debug.WriteLine($"listWindows.Count: {listWindows.Count}");
			return listWindows;
		}
		private BOOL EnumWindowsProc(HWND hwnd, LPARAM lParam)
        {
            try
            {
                if(hwnd != nint.Zero && hwnd != ShellWindowHandler && lParam != nint.Zero)
                {
                    GCHandle handle = GCHandle.FromIntPtr(lParam);
                    if(handle.Target is List<WindowInfo> listWindows)
                    {
                        DWORD pid = 0;
                        GetWindowThreadProcessId(hwnd, out pid);
                        if(pid > 0)
                        {
                            using(Process process = Process.GetProcessById((int)pid))
                            {
                                if(!string.IsNullOrWhiteSpace(process?.ProcessName) && !process.ProcessName.Equals("system", StringComparison.OrdinalIgnoreCase)
                                    && !process.ProcessName.Equals("idle", StringComparison.OrdinalIgnoreCase))
                                {
                                    HWND owner = GetWindow(hwnd, GW_OWNER);
                                    ULONG style = (ULONG)GetWindowLong(hwnd, GWL_STYLE);
                                    ULONG extStyle = (ULONG)GetWindowLong(hwnd, GWL_EXSTYLE);
                                    if((style & WS_CHILD) == 0 && ((extStyle & WS_EX_APPWINDOW) != 0 || (extStyle & WS_EX_TOOLWINDOW) == 0 && (owner == nint.Zero)))
                                    {
                                        string? title = GetWindowTitle(hwnd) ?? process.ProcessName;
                                        if(!listWindows.Any(winInfo => string.Equals(winInfo.Title, title)))
                                        {
                                            WindowInfo windowInfo = new WindowInfo(hwnd, process, title);
                                            listWindows.Add(windowInfo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
            }
            return true;
        }
        private LRESULT LowLevelKeyboardProc(int nCode, WPARAM wParam, LPARAM lParam)
        {
			LRESULT result = 0;
			KBDLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
			if(nCode >= 0 && (hookStruct.flags & LLKHF_INJECTED) == 0)
            {
                try
                {
					int vkCode = NormalizeKey((int)hookStruct.vkCode);
					bool isKeyDown = wParam == (nint)WM_KEYDOWN || wParam == (nint)WM_SYSKEYDOWN;
                    bool isKeyUp = wParam == (nint)WM_KEYUP || wParam == (nint)WM_SYSKEYUP;
					if(isKeyDown)
					{
						if(IsTargetApplicationActive())
						{
							bool isNewKey = PressedKeys.Add(vkCode);
							if(isNewKey)
							{
								ShortcutPair? shortcutPair = FindShortcut();
								if(shortcutPair != null)
								{
									Task.Run(async () =>
									{
										await Task.Delay(15);
										MapShortcut(shortcutPair);
									});
									result = 1;
								}
							}
						}
					}
					else if(isKeyUp)
					{
						ShortcutPair? shortcutPair = FindShortcut();
						PressedKeys.Remove(vkCode);
						if(IsTargetApplicationActive() && shortcutPair != null)
						{
							result = 1;
						}
					}
                }
                catch(Exception e)
                {
					result = 0;
					Debug.WriteLine(e);
                    //throw;
                }
            }
            return (INT)result > 0 ? result : CallNextHookEx(nint.Zero, nCode, wParam, lParam);
        }
		private static int NormalizeKey(int vkCode)
		{
			return vkCode switch
			{
				VK_LCONTROL or VK_RCONTROL => VK_CONTROL,
				VK_LSHIFT or VK_RSHIFT => VK_SHIFT,
				VK_LMENU or VK_RMENU => VK_MENU,
				_ => vkCode
			};
		}
		private bool IsTargetApplicationActive()
        {
            bool isActive = false;
            try
            {
                if(SelectedWindowInfo != null && SelectedWindowInfo.Hwnd != nint.Zero)
                {
                    HWND hwnd = GetForegroundWindow();
                    isActive = hwnd != nint.Zero && hwnd == SelectedWindowInfo.Hwnd;
                }
            }
            catch(Exception e)
            {
                isActive = false;
                Debug.WriteLine(e);
                //throw;
            }
            return isActive;
        }
		private ShortcutPair? FindShortcut()
		{
			ShortcutPair? shortcutPair = null;
			try
			{
				shortcutPair = ShortcutMap.OrderByDescending(pair => pair.NewShortcut.KeyCodes.Count)
					.FirstOrDefault(pair => PressedKeys.SetEquals(pair.NewShortcut.KeyCodes.Select(NormalizeKey)));
				if(shortcutPair != null && (shortcutPair.CurrentShortcut.KeyCodes.Count == 0 || shortcutPair.NewShortcut.KeyCodes.Count == 0))
				{
					shortcutPair = null;
				}
			}
			catch(Exception e)
			{
				shortcutPair = null;
				Debug.WriteLine(e);
				throw;
			}
			return shortcutPair;
		}
		private void MapShortcut(ShortcutPair shortcutPair)
		{
			try
			{
				bool isModifierKeysOnlyShortcut = shortcutPair.NewShortcut.IsModifierKeysOnly;
				int[] arrCurrentKeyCodes = [.. shortcutPair.CurrentShortcut.KeyCodes];
				int inputCount = arrCurrentKeyCodes.Length * 2 + (isModifierKeysOnlyShortcut ? PressedKeys.Count + 2 : 0);
				int index = 0;
				INPUT[] arrInputs = new INPUT[inputCount];
				if(isModifierKeysOnlyShortcut)
				{
					arrInputs[index++].SetKeyboardInput(VK_NONAME);
					arrInputs[index++].SetKeyboardInput(VK_NONAME, KEYEVENTF_KEYUP);
					foreach(int vkCode in PressedKeys)
					{
						arrInputs[index++].SetKeyboardInput(vkCode, KEYEVENTF_KEYUP);
					}
				}
				foreach(int vkCode in arrCurrentKeyCodes)
				{
					arrInputs[index++].SetKeyboardInput(vkCode);
				}
				int[] arrCurrentKeyCodesReversed = arrCurrentKeyCodes.Reverse().ToArray();
				foreach(int vkCode in arrCurrentKeyCodesReversed)
				{
					arrInputs[index++].SetKeyboardInput(vkCode, KEYEVENTF_KEYUP);
				}
				SendInput((uint)inputCount, arrInputs, Marshal.SizeOf<INPUT>());
			}
			catch(Exception e)
			{
				Debug.WriteLine(e);
				throw;
			}
		}
		public void StartMapping()
        {
            using(Process process = Process.GetCurrentProcess())
            {
                if(!string.IsNullOrWhiteSpace(process.MainModule?.ModuleName))
                {
                    HMODULE hmod = GetModuleHandle(process.MainModule.ModuleName);
                    hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, hmod, 0);
                }
            }
        }
        public void StopMapping()
        {
            if(hHook != nint.Zero)
            {
                UnhookWindowsHookEx(hHook);
            }
        }
        public void LoadSettings()
        {
            ShortcutMap.Clear();
            if(File.Exists(SettingsFilePath))
            {
                string jsonSettings = File.ReadAllText(SettingsFilePath);
                ShortcutPair[]? arrShortcutMap = JsonSerializer.Deserialize<ShortcutPair[]>(jsonSettings);
                if(arrShortcutMap?.Length > 0)
                {
                    foreach(ShortcutPair pair in arrShortcutMap)
                    {
                        ShortcutMap.Add(pair);
                    }
                }
            }
        }
        public void SaveSettings()
        {
            if(ShortcutMap.Count > 0)
            {
                ShortcutPair[] arrShortcutMap = ShortcutMap.ToArray();
                string jsonSettings = JsonSerializer.Serialize(arrShortcutMap);
                File.WriteAllText(SettingsFilePath, jsonSettings);
            }
        }
        public void Dispose()
        {
            hHook = 0;
            PressedKeys.Clear();
        }
        private string GetSettingsFileName([CallerFilePath] string sourceFilePath = "")
        {
            string fileName = "";
            string[] arrPath = sourceFilePath.Split('\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if(arrPath.Length > 2)
            {
                arrPath[arrPath.Length - 1] = "settings\\Settings.json";
                fileName = string.Join('\\', arrPath);
            }
            else
            {
                string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string userFolderName = Path.GetFileName(userProfilePath);
                fileName = Path.Combine(userFolderName, "KeyMapperSettings.json");
            }
            return fileName;
        }
        private void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}