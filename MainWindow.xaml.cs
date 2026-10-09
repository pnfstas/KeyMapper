using H.NotifyIcon.Core;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.System;
using Windows.UI;
using WinRT.Interop;

namespace KeyMapper
{
	public class BoolToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			Color color = value is bool isSelected && targetType == typeof(Brush) && isSelected ? Colors.LightBlue : Colors.White;
			return new SolidColorBrush(color);
		}
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
	public sealed partial class MainWindow : Window
    {
		private KeyMapper KeyMapper { get; }
        private HWND hwnd = nint.Zero;
        private bool isExiting = false;
		private Popup? ApplicationComboBoxPopup { get; set; }
		private HashSet<VirtualKey> PressedKeys { get; } = new HashSet<VirtualKey>();
		private bool isRecordingShortcut = false;
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern BOOL ShowWindow(nint HWND, int nCmdShow);
        public MainWindow()
        {
            InitializeComponent();
			isExiting = false;
			KeyMapper = new KeyMapper();
            KeyMapper.LoadSettings();
            Activated += OnActivated;
            AppWindow.Closing += OnAppWindowClosing;
        }
        private void HideWindow()
        {
            if(hwnd == nint.Zero)
            {
                hwnd = WindowNative.GetWindowHandle(this);
            }
            if(hwnd != nint.Zero)
            {
                ShowWindow(hwnd, CmdShow.SW_HIDE);
            }
        }
        private void OnActivated(object sender, WindowActivatedEventArgs args)
        {
            HideWindow();
            Activated -= OnActivated;
        }
        private void OnAppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if(!isExiting)
            {
                args.Cancel = true;
                AppWindow.Hide();
            }
            else
            {
				AppWindow.Closing -= OnAppWindowClosing;
            }
        }
		private void OnUpdateActiveWindowsCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
		{
			KeyMapper.ActiveWindows = KeyMapper.GetActiveWindows();
		}
		private void OnModifySettingsCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            AppWindow.Show(true);
        }
        private void OnStartMappingCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            KeyMapper.StartMapping();
        }
        private void OnStopMappingCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            KeyMapper.StopMapping();
        }
        private void OnExitCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
			KeyMapper.StopMapping();
			KeyMapper.SaveSettings();
			TrayIcon.Dispose();
            isExiting = true;
            Close();
        }
        private void OnCancelSettingsCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            HideWindow();
        }
		private void OnApplySettingsCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
		{
			HideWindow();
		}
		private void OnApplySettingsAndStartMappingCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
		{
			HideWindow();
			KeyMapper.StartMapping();
		}
		private void OnAddMappingCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            KeyMapper.ShortcutMap.Add(new ShortcutPair());
        }
        private void OnRemoveMappingCommandCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = KeyMapper?.SelectedShortcutPair != null;
        }
        private void OnRemoveMappingCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
			if(KeyMapper.SelectedShortcutPair != null)
			{
				KeyMapper.ShortcutMap.Remove(KeyMapper.SelectedShortcutPair);
			}
        }
        private void OnClearMappingCommandCanExecuteRequested(XamlUICommand sender, CanExecuteRequestedEventArgs args)
        {
            args.CanExecute = KeyMapper?.ShortcutMap.Count > 0;
        }
        private void OnClearMappingCommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            KeyMapper.ShortcutMap.Clear();
        }
		private void OnApplicationComboBoxLoaded(object sender, RoutedEventArgs args)
		{
			if(sender is ComboBox comboBox)
			{
				ApplicationComboBoxPopup = FindVisualChild<Popup>(comboBox, "Popup");
				if(ApplicationComboBoxPopup != null)
				{
					ApplicationComboBoxPopup.Opened += OnApplicationComboBoxPopupOpened;
				}
			}
		}
		private void OnApplicationComboBoxPopupOpened(object? sender, object args)
		{
			if(sender is Popup popup && popup == ApplicationComboBoxPopup)
			{
				ApplicationComboBoxPopup.PlacementTarget = ApplicationComboBox;
				ApplicationComboBoxPopup.DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedRight;
				ApplicationComboBoxPopup.VerticalOffset = 0;
				ApplicationComboBoxPopup.HorizontalOffset = 0;
			}
		}
		private void OnCurrentShortcutTextBoxGotFocus(object sender, RoutedEventArgs args)
		{
			SetSelectedShortcutPair(sender);
		}
		private void OnCurrentShortcutTextBoxPreviewKeyDown(object sender, KeyRoutedEventArgs args)
		{
			args.Handled = true;
			AddPressedKey(args.Key);
		}
		private void OnCurrentShortcutTextBoxPreviewKeyUp(object sender, KeyRoutedEventArgs args)
		{
			if(sender is TextBox textBox)
			{
				args.Handled = true;
				SaveShortcut(textBox, "CurrentShortcut");
			}
		}
		private void OnNewShortcutTextBoxGotFocus(object sender, RoutedEventArgs args)
		{
			SetSelectedShortcutPair(sender);
		}
		private void OnNewShortcutTextBoxPreviewKeyDown(object sender, KeyRoutedEventArgs args)
		{
			args.Handled = true;
			AddPressedKey(args.Key);
		}
		private void OnNewShortcutTextBoxPreviewKeyUp(object sender, KeyRoutedEventArgs args)
		{
			if(sender is TextBox textBox)
			{
				args.Handled = true;
				SaveShortcut(textBox, "NewShortcut");
			}
		}
		private void SetSelectedShortcutPair(object sender)
		{
			if(sender is TextBox textBox && textBox.DataContext is ShortcutPair shortcutPair)
			{
				if(KeyMapper.SelectedShortcutPair != null)
				{
					KeyMapper.SelectedShortcutPair.IsSelected = false;
				}
				KeyMapper.SelectedShortcutPair = shortcutPair;
				KeyMapper.SelectedShortcutPair.IsSelected = true;
			}
		}
		private void AddPressedKey(VirtualKey key)
		{
			if(!isRecordingShortcut)
			{
				PressedKeys.Clear();
				isRecordingShortcut = true;
			}
			PressedKeys.Add(key);
		}
		private void SaveShortcut(TextBox textBox, string? propertyName=null)
		{
			if(isRecordingShortcut)
			{
				isRecordingShortcut = false;
				if(PressedKeys.Count > 0 && textBox.DataContext is ShortcutPair currentShortcutPair)
				{
					VirtualKey? key = null;
					if(PressedKeys.Count > 1 || (key = PressedKeys.ElementAt(0)) != VirtualKey.Control
						&& key != VirtualKey.Shift && key != VirtualKey.Menu && key != VirtualKey.LeftWindows
						&& key != VirtualKey.RightWindows)
					{
						Shortcut? shortcut = propertyName switch
						{
							"CurrentShortcut" => currentShortcutPair.CurrentShortcut,
							"NewShortcut" => currentShortcutPair.NewShortcut,
							_ => null
						};
						if(shortcut != null)
						{
							shortcut.KeyCodes = [.. PressedKeys.Select(curKey => (int)curKey)];
						}
					}
				}
			}
		}
		public static T? FindVisualChild<T>(DependencyObject obj, string? name = null) where T : FrameworkElement
		{
			T? result = null;
			if(obj != null)
			{
				try
				{
					int count = VisualTreeHelper.GetChildrenCount(obj);
					for(int index = 0; result == null && index < count; index++)
					{
						DependencyObject curObj = VisualTreeHelper.GetChild(obj, index);
						if(curObj is T element && (string.IsNullOrWhiteSpace(name) || name.Equals(element.Name)))
						{
							result = element;
						}
						else
						{
							result = FindVisualChild<T>(curObj, name);
						}
					}
				}
				catch(Exception e)
				{
					result = null;
					Debug.WriteLine(e);
				}
			}
			return result;
		}
		public static T? FindVisualParent<T>(DependencyObject obj, string? name = null) where T : FrameworkElement
		{
			T? result = null;
			if(obj != null)
			{
				try
				{
					while(result == null)
					{
						DependencyObject curObj = VisualTreeHelper.GetParent(obj);
						if(curObj is T element && (string.IsNullOrWhiteSpace(name) || name.Equals(element.Name)))
						{
							result = element;
						}
						else
						{
							result = FindVisualParent<T>(curObj, name);
						}
					}
				}
				catch(Exception e)
				{
					result = null;
					Debug.WriteLine(e);
				}
			}
			return result;
		}
	}
}
