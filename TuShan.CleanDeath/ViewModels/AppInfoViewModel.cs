﻿using HandyControl.Controls;
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
using TuShan.CleanDeath.Models;

namespace TuShan.CleanDeath.ViewModels
{
    public class AppInfoViewModel : Caliburn.Micro.Screen
    {
        /// <summary>
        /// 应用程序信息
        /// </summary>
        public Action<CleanAppModel> AddAppInfo;

        private string _handAppExeFilePath;

        /// <summary>
        /// 软件运行地址
        /// </summary>
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

        /// <summary>
        /// 程序名称
        /// </summary>
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

        /// <summary>
        /// 用户选择文件
        /// </summary>
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

        /// <summary>
        /// 拖动exe或者lnk文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AppExePathDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // 检查是否是.lnk文件
                if (files != null && files.Length == 1)
                {
                    string shortcutPath = files[0];
                    if (!shortcutPath.EndsWith(".exe") && !shortcutPath.EndsWith(".lnk"))
                    {
                        HandAppExeFilePath = "请拖放一个.lnk或.exe文件。";
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

        /// <summary>
        /// 通过选择的文件来设置app对象信息
        /// </summary>
        /// <param name="selectedFilePath"></param>
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
                    selectedFilePath = GetShortcutTarget(selectedFilePath);
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

        /// <summary>
        /// 获取lnk文件的目标文件地址
        /// </summary>
        /// <param name="shortcutPath"></param>
        /// <returns></returns>
        string GetShortcutTarget(string shortcutPath)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                string targetPath = (string)shortcut.GetType().InvokeMember("TargetPath", BindingFlags.GetProperty, null, shortcut, null);
                return targetPath;
            }
            catch (Exception ex)
            {
                // 处理异常，如快捷方式文件不存在或无法读取
                Console.WriteLine("发生异常：" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 添加回调
        /// </summary>
        public void SaveCleanAppEvent()
        {
            AddAppInfo?.Invoke(_cleanAppModel);
            this.TryCloseAsync();
        }
    }
}
