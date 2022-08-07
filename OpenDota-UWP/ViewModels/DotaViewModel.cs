﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace OpenDota_UWP.ViewModels
{
    public class DotaViewModel : ViewModelBase
    {
        private static Lazy<DotaViewModel> _lazyVM = new Lazy<DotaViewModel>(() => new DotaViewModel());
        public static DotaViewModel Instance => _lazyVM.Value;

        // 当前选中的Tab: 0-Heroes 1-Items 2-Matches
        private int _iSideMenuTabIndex = 0;
        public int iSideMenuTabIndex
        {
            get { return _iSideMenuTabIndex; }
            set { Set("iSideMenuTabIndex", ref _iSideMenuTabIndex, value); }
        }

        // 应用程序的主题
        private ElementTheme _eAppTheme = ElementTheme.Light;
        public ElementTheme eAppTheme
        {
            get { return _eAppTheme; }
            set { Set("eAppTheme", ref _eAppTheme, value); }
        }

        // 设置的启动页面: 0-Heroes 1-Items 2-Matches
        private int _iStartupTabIndex = 0;
        public int iStartupTabIndex
        {
            get { return _iStartupTabIndex; }
            set
            {
                if (value > 2 || value < 0)
                {
                    return;
                }
                Set("iStartupTabIndex", ref _iStartupTabIndex, value);
            }
        }

        // 设置的语言: 0-english 1-schinese 2-russian
        private int _iLanguageIndex = 0;
        public int iLanguageIndex
        {
            get { return _iLanguageIndex; }
            set
            {
                if (value > 2 || value < 0)
                {
                    return;
                }
                Set("iLanguageIndex", ref _iLanguageIndex, value);
            }
        }

        // 物品-搜索模式, true:模糊匹配, false:全字匹配
        private bool _bSearchFuzzy = true;
        public bool bSearchFuzzy
        {
            get { return _bSearchFuzzy; }
            set { Set("bSearchFuzzy", ref _bSearchFuzzy, value); }
        }

        public DotaViewModel()
        {
            try
            {
                LoadSettingContainer();
            }
            catch { }
        }

        public void LoadSettingContainer()
        {
            try
            {
                // 读取设置的应用程序主题
                try
                {
                    if (App.AppSettingContainer?.Values["Theme"] == null)
                    {
                        this.eAppTheme = ElementTheme.Dark;
                    }
                    else if (App.AppSettingContainer?.Values["Theme"]?.ToString() == "Light")
                    {
                        this.eAppTheme = ElementTheme.Light;
                    }
                    else if (App.AppSettingContainer?.Values["Theme"]?.ToString() == "Dark")
                    {
                        this.eAppTheme = ElementTheme.Dark;
                    }
                    else
                    {
                        this.eAppTheme = ElementTheme.Dark;
                    }
                }
                catch { }

                // 读取设置的启动页面
                try
                {
                    if (App.AppSettingContainer?.Values["StartupPage"] == null ||
                        App.AppSettingContainer?.Values["StartupPage"].ToString() == "0")
                    {
                        iStartupTabIndex = 0;
                    }
                    else if (App.AppSettingContainer?.Values["StartupPage"].ToString() == "1")
                    {
                        iStartupTabIndex = 1;
                    }
                    else if (App.AppSettingContainer?.Values["StartupPage"].ToString() == "2")
                    {
                        iStartupTabIndex = 2;
                    }
                    else
                    {
                        iStartupTabIndex = 0;
                    }
                }
                catch { }

                // 读取设置的语言
                try
                {
                    if (App.AppSettingContainer?.Values["Language"] == null ||
                        App.AppSettingContainer?.Values["Language"].ToString() == "0")
                    {
                        iLanguageIndex = 0;
                    }
                    else if (App.AppSettingContainer?.Values["Language"].ToString() == "1")
                    {
                        iLanguageIndex = 1;
                    }
                    else if (App.AppSettingContainer?.Values["Language"].ToString() == "2")
                    {
                        iLanguageIndex = 2;
                    }
                    else
                    {
                        iLanguageIndex = 0;
                    }
                }
                catch { }

                // 读取设置的物品搜索模式
                try
                {
                    if (App.AppSettingContainer?.Values["ItemsSearchFuzzy"] == null)
                    {
                        this.bSearchFuzzy = true;
                    }
                    else if (App.AppSettingContainer?.Values["ItemsSearchFuzzy"]?.ToString() == "True")
                    {
                        this.bSearchFuzzy = true;
                    }
                    else if (App.AppSettingContainer?.Values["ItemsSearchFuzzy"]?.ToString() == "False")
                    {
                        this.bSearchFuzzy = false;
                    }
                    else
                    {
                        this.bSearchFuzzy = true;
                    }
                }
                catch { }
            }
            catch { }
        }
    }
}
