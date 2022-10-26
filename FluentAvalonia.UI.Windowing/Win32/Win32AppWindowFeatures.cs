﻿using Avalonia.Logging;
using Avalonia.Media;
using FluentAvalonia.Interop;
using FluentAvalonia.UI.Windowing.Win32;
using static FluentAvalonia.Interop.Win32Interop;

namespace FluentAvalonia.UI.Windowing;

internal class Win32AppWindowFeatures : IAppWindowPlatformFeatures
{
    public Win32AppWindowFeatures(AppWindow owner)
    {
        _owner = owner;
    }

    public void SetTaskBarProgressBarState(TaskBarProgressBarState state)
    {
        if (_taskBarList == null)
        {
            CreateTaskBarList();

            // If creation fails, return 
            if (_taskBarList == null)
                return;
        }

        // Enum values are mapped to TMPF_ from Win32
        _taskBarList.SetProgressState(_owner.PlatformImpl.Handle.Handle, (int)state);
    }

    public void SetTaskBarProgressBarValue(ulong currentValue, ulong totalValue)
    {
        if (_taskBarList == null)
        {
            CreateTaskBarList();

            // If creation fails, return 
            if (_taskBarList == null)
                return;
        }

        _taskBarList.SetProgressValue(_owner.PlatformImpl.Handle.Handle, currentValue, totalValue);
    }

    public unsafe void SetWindowBorderColor(Color color)
    {
        // This is only available on Windows 11 right now, but don't bring the whole app down
        // if called on Win 10
        if (OSVersionHelper.IsWindows11())
        {
            COLORREF cr = color.ToUint32();
            var hr = (HRESULT)DwmSetWindowAttribute(_owner.PlatformImpl.Handle.Handle, DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR,
                &cr, sizeof(COLORREF));
            if (!hr.SUCCEEDED)
            {
                Logger.TryGet(LogEventLevel.Debug, "AppWindow")?
                    .Log("SetWindowBorderColor", "Failed to set the border color of the window with hr: {hr}", hr);
            }
        }
    }

    private void CreateTaskBarList()
    {
        try
        {
            Guid clsid = Guid.Parse("56FDF344-FD6D-11D0-958A-006097C9A090");
            Guid iid = Guid.Parse("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf");
            _taskBarList = Win32.Win32Interop.CreateInstance<ITaskbarList3>(ref clsid, ref iid);
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Debug, "AppWindow")?
                    .Log("SetWindowBorderColor", "Unable to create instance of ITaskbarList3", ex);
        }
    }

    private AppWindow _owner;
    private ITaskbarList3 _taskBarList;
}
