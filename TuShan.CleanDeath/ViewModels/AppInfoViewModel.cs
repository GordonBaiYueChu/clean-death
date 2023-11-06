using HandyControl.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TuShan.CleanDeath.Helps;
using TuShan.CleanDeath.Models;

namespace TuShan.CleanDeath.ViewModels
{
    public class AppInfoViewModel : Caliburn.Micro.Screen
    {
        public Action<CleanAppModel> AddAppInfo;

        private string _handAppExeFilePath;

        public string HandAppExeFilePath
        {
            get { return _handAppExeFilePath; }
            set
            {
                _handAppExeFilePath = value;
                NotifyOfPropertyChange(() => HandAppExeFilePath);
            }
        }

        private string _handAppDisplayName;

        public string HandAppDisplayName
        {
            get { return _handAppDisplayName; }
            set
            {
                _handAppDisplayName = value;
                if (_cleanAppModel != null)
                {
                    _cleanAppModel.AppDisplayName = _handAppDisplayName;
                }
                NotifyOfPropertyChange(() => HandAppDisplayName);
            }
        }

        private CleanAppModel _cleanAppModel;

        public void SelectedAppExeFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择App文件";
            openFileDialog.Filter = "快捷方式 (*.lnk)|*.lnk|可执行文件 (*.exe)|*.exe";
            openFileDialog.Multiselect = false;
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                SetAppInfoByPath(openFileDialog.FileName);
            }
        }

        public void AppExePathDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length == 1)
                {
                    string shortcutPath = files[0];
                    if (!shortcutPath.EndsWith(".exe") && !shortcutPath.EndsWith(".lnk"))
                    {
                        HandAppExeFilePath = "请拖放.lnk或.exe文件。";
                        return;
                    }
                    SetAppInfoByPath(shortcutPath);
                }
                else
                {
                    HandAppExeFilePath = "请拖放一个.lnk或.exe文件。";
                }
            }
        }

        private void SetAppInfoByPath(string selectedFilePath)
        {
            if (!selectedFilePath.EndsWith(".exe") && !selectedFilePath.EndsWith(".lnk"))
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(selectedFilePath))
            {
                if (selectedFilePath.EndsWith(".lnk"))
                {
                    selectedFilePath = PathHelp.GetShortcutTarget(selectedFilePath);
                }

                string exeName = selectedFilePath;
                if (exeName.Contains("\\"))
                {
                    exeName = exeName.Split('\\').Last();
                }
                _cleanAppModel = new CleanAppModel();
                _cleanAppModel.AppExePath = Path.GetDirectoryName(selectedFilePath);
                _cleanAppModel.AppExeName = exeName.Replace(".exe", "");
                _cleanAppModel.AppDisplayName = exeName.Replace(".exe", "");
                HandAppDisplayName = _cleanAppModel.AppDisplayName;
                HandAppExeFilePath = selectedFilePath;
            }
        }



        public void SaveCleanAppEvent()
        {
            AddAppInfo?.Invoke(_cleanAppModel);
            this.TryCloseAsync();
        }
    }
}
