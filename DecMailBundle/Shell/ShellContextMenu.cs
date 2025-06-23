using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;
using Windows.Win32.UI.WindowsAndMessaging;

namespace DecMailBundle.Shell;

public unsafe class ShellContextMenu : NativeWindow
{
    private const uint CMD_FIRST = 1;
    private const uint CMD_LAST = 30000;

    private IContextMenu2? _oContextMenu2;
    private IContextMenu3? _oContextMenu3;

    public ShellContextMenu()
    {
        Init();
    }
    
    private void Init()
    {
        CreateHandle(new CreateParams());
    }

    public void Show(FileInfo fileInfo, Point pointOnScreen)
    {
        var desktop = GetDesktopFolder();

        IShellFolder? parentFolder = null;
        ITEMIDLIST* idList = null;
        DestroyMenuSafeHandle? menuHandle = null;
        IContextMenu? contextMenu = null;
        IntPtr? unkownId = null;
        try
        {
            (parentFolder, var parentName) = GetParentFolder(desktop, fileInfo.DirectoryName!);

            // Get the file relative to folder
            fixed (char* str = &fileInfo.Name.GetPinnableReference())
            {
                uint attributs = 0;
                parentFolder.ParseDisplayName(HWND.Null, null, str, null, &idList, ref attributs);
            }
   
            var iid = typeof(IContextMenu).GUID;
            parentFolder.GetUIObjectOf(HWND.Null, 1, &idList, &iid, null, out var ppv);
            contextMenu = (IContextMenu)ppv;

            _oContextMenu2 = (IContextMenu2)ppv;
            _oContextMenu3 = (IContextMenu3)ppv;

            menuHandle = PInvoke.CreatePopupMenu_SafeHandle();
            var hMenu = new HMENU(menuHandle.DangerousGetHandle());
            
            var flags = PInvoke.CMF_EXPLORE | PInvoke.CMF_NORMAL |
                        ((Control.ModifierKeys & Keys.Shift) != 0 ? PInvoke.CMF_EXTENDEDVERBS : 0);
          
            contextMenu.QueryContextMenu((HMENU)hMenu, 0, CMD_FIRST, CMD_LAST, flags);
            
            var nSelected = (uint)(int)PInvoke.TrackPopupMenuEx(menuHandle, (uint)TRACK_POPUP_MENU_FLAGS.TPM_RETURNCMD,
                pointOnScreen.X, pointOnScreen.Y, new HWND(Handle), null);

            if (nSelected != 0)
            {
                InvokeCommand(contextMenu, nSelected, parentName);
            }
        }
        finally
        {
            menuHandle?.Dispose();
            ComRelease(contextMenu);

            if (unkownId.HasValue)
                Marshal.Release(unkownId.Value);

            ComRelease(parentFolder);
            ComRelease(desktop);

            _oContextMenu2 = null;
            _oContextMenu3 = null;
        }
    }

    private static void ComRelease(object? obj)
    {
        if (obj != null)
        {
            Marshal.ReleaseComObject(obj);
        }
    }

    protected override void WndProc(ref Message m)
    {
        var w = new WPARAM((UIntPtr)m.WParam.ToPointer());
        var l = new LPARAM(m.LParam);

        
        if (_oContextMenu3 != null && m.Msg == PInvoke.WM_MENUCHAR)
        {
            try
            {
                _oContextMenu3.HandleMenuMsg2((uint)m.Msg, w, l);
            }
            catch (NotImplementedException)
            {
                // ignore
            }
            catch (COMException)
            {
                // ignore
            }
        }

        if (_oContextMenu2 != null &&
            m.Msg is (int)PInvoke.WM_INITMENUPOPUP or (int)PInvoke.WM_MEASUREITEM or (int)PInvoke.WM_DRAWITEM)
        {
            try
            {
                _oContextMenu2.HandleMenuMsg((uint)m.Msg, w, l);
                return;
            }
            catch (NotImplementedException)
            {
                // ignore
            }
            catch (COMException)
            {
                // ignore
            }
        }


        base.WndProc(ref m);
    }

    private static void InvokeCommand(IContextMenu oContextMenu, uint nCmd, string? strFolder)
    {
        if (strFolder == null)
        {
            return;
        }

        var mask = PInvoke.SEE_MASK_UNICODE | PInvoke.CMIC_MASK_PTINVOKE |
                   ((Control.ModifierKeys & Keys.Control) != 0 ? PInvoke.CMIC_MASK_CONTROL_DOWN : 0) |
                   ((Control.ModifierKeys & Keys.Shift) != 0 ? PInvoke.CMIC_MASK_SHIFT_DOWN : 0);

        fixed (char* pStrFolder = &strFolder.GetPinnableReference())
        {
            var q = new CMINVOKECOMMANDINFOEX
            {
                cbSize = (uint)Marshal.SizeOf<CMINVOKECOMMANDINFOEX>(),
                lpVerb = new PCSTR((byte*)(nCmd - CMD_FIRST)),
                lpDirectory = new PCSTR((byte*)pStrFolder),
                lpVerbW = new PCWSTR((char*)(nCmd - CMD_FIRST)),
                lpDirectoryW = new PCWSTR(pStrFolder),
                fMask = mask,
                nShow = (int)SHOW_WINDOW_CMD.SW_SHOWNORMAL
            };
            oContextMenu.InvokeCommand((CMINVOKECOMMANDINFO*)&q);
        }
    }
    
    private static IShellFolder GetDesktopFolder()
    {
        var result = PInvoke.SHGetDesktopFolder(out var shellFolder);
        result.ThrowOnFailure();
        return shellFolder;
    }
    
    private static (IShellFolder shellFolder, string displayName) GetParentFolder(IShellFolder desktop, string folderName)
    {
        ITEMIDLIST* pidl = null;
        fixed (char* fFolderName = &folderName.GetPinnableReference())
        {
            uint attribs = 0;
            desktop.ParseDisplayName(HWND.Null, null, new PWSTR(fFolderName), null, &pidl, ref attribs);
        }
        
        STRRET pName;
        desktop.GetDisplayNameOf(pidl, SHGDNF.SHGDN_FORPARSING, &pName);

        var pNameStr = new string(pName.Anonymous.pOleStr.AsSpan());

        var shell = typeof(IShellFolder).GUID;
        desktop.BindToObject(pidl, null, &shell, out var ppv);

        return ((IShellFolder)ppv, pNameStr);
    }
}