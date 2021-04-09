using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WinStuff
{
    [ComImport()]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [GuidAttribute("000214e4-0000-0000-c000-000000000046")]
    public interface IContextMenu
    {
        // Adds commands to a shortcut menu
        [PreserveSig]
        Int32 QueryContextMenu(IntPtr hmenu, uint iMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

        // Carries out the command associated with a shortcut menu item
        [PreserveSig]
        Int32 InvokeCommand(ref CMINVOKECOMMANDINFOEX info);

        // Retrieves information about a shortcut menu command, 
        // including the help string and the language-independent, 
        // or canonical, name for the command
        [PreserveSig]
        Int32 GetCommandString(uint idcmd, GCS uflags, uint reserved, [MarshalAs(UnmanagedType.LPArray)] byte[] commandstring, int cch);
    }

    [ComImport, Guid("000214f4-0000-0000-c000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IContextMenu2
    {
        // Adds commands to a shortcut menu
        [PreserveSig]
        Int32 QueryContextMenu(IntPtr hmenu, uint iMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

        // Carries out the command associated with a shortcut menu item
        [PreserveSig]
        Int32 InvokeCommand(ref CMINVOKECOMMANDINFOEX info);

        // Retrieves information about a shortcut menu command, 
        // including the help string and the language-independent, 
        // or canonical, name for the command
        [PreserveSig]
        Int32 GetCommandString(uint idcmd, GCS uflags, uint reserved, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder commandstring, int cch);

        // Allows client objects of the IContextMenu interface to 
        // handle messages associated with owner-drawn menu items
        [PreserveSig]
        Int32 HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);
    }

    [ComImport, Guid("bcfce0a0-ec17-11d0-8d10-00a0c90f2719")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IContextMenu3
    {
        // Adds commands to a shortcut menu
        [PreserveSig]
        Int32 QueryContextMenu(IntPtr hmenu, uint iMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

        // Carries out the command associated with a shortcut menu item
        [PreserveSig]
        Int32 InvokeCommand(ref CMINVOKECOMMANDINFOEX info);

        // Retrieves information about a shortcut menu command, 
        // including the help string and the language-independent, 
        // or canonical, name for the command
        [PreserveSig]
        Int32 GetCommandString(uint idcmd, GCS uflags, uint reserved, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder commandstring, int cch);

        // Allows client objects of the IContextMenu interface to 
        // handle messages associated with owner-drawn menu items
        [PreserveSig]
        Int32 HandleMenuMsg(uint uMsg, IntPtr wParam, IntPtr lParam);

        // Allows client objects of the IContextMenu3 interface to 
        // handle messages associated with owner-drawn menu items
        [PreserveSig]
        Int32 HandleMenuMsg2(uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr plResult);
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // Flags specifying the information to return when calling IContextMenu::GetCommandString
    [Flags]
    public enum GCS : uint
    {
        VerbA = 0,
        HelpTextA = 1,
        ValidateA = 2,
        VerbW = 4,
        HelpTextW = 5,
        ValidateW = 6
    }
    
    // Specifies how the shortcut menu can be changed when calling IContextMenu::QueryContextMenu
    [Flags]
    public enum CMF : uint
    {
        Normal = 0x00000000,
        DefaultOnly = 0x00000001,
        VerbsOnly = 0x00000002,
        Explore = 0x00000004,
        NoVerbs = 0x00000008,
        CanRename = 0x00000010,
        NodeFault = 0x00000020,
        IncludeStatic = 0x00000040,
        ExtendedVerbs = 0x00000100,
        Reserved = 0xffff0000
    }
    
    
    // Contains extended information about a shortcut menu command
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CMINVOKECOMMANDINFOEX
    {
        public int Size;

        public CMIC Mask;

        public IntPtr Hwnd;

        public IntPtr Verb;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Parameters;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Directory;

        public ShowWindowCommands ShowType;

        public int HotKey;

        public IntPtr hIcon;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Title;

        public IntPtr VerbW;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string ParametersW;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string DirectoryW;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string TitleW;

        public WinNative.POINT InvokePoint;
    }
    
    // Flags used with the CMINVOKECOMMANDINFOEX structure
    [Flags]
    public enum CMIC : uint
    {
        Hotkey = 0x00000020,
        Icon = 0x00000010,
        FlagNoUi = 0x00000400,
        Unicode = 0x00004000,
        NoConsole = 0x00008000,
        Asyncok = 0x00100000,
        NoZoneChecks = 0x00800000,
        ShiftDown = 0x10000000,
        ControlDown = 0x40000000,
        FlagLogUsage = 0x04000000,
        PtInvoke = 0x20000000
    }
    
    
    
    
    
}