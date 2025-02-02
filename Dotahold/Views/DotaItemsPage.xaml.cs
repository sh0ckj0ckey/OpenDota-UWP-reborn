﻿using Dotahold.Models;
using Dotahold.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Dotahold.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DotaItemsPage : Page
    {
        // 用来抑制页面跳转时其他的动画的，这样可以避免其他动画和 Connected Animation 出现奇怪的冲突
        private SuppressNavigationTransitionInfo snti = new SuppressNavigationTransitionInfo();

        private DotaItemsViewModel ViewModel = null;
        private DotaViewModel MainViewModel = null;

        public DotaItemsPage()
        {
            try
            {
                this.InitializeComponent();
                ViewModel = DotaItemsViewModel.Instance;
                MainViewModel = DotaViewModel.Instance;

                ItemFrame.Navigate(typeof(BlankPage));
            }
            catch (Exception ex) { LogCourier.LogAsync(ex.Message, LogCourier.LogType.Error); }
        }

        /// <summary>
        /// 重写导航至此页面的代码,显示动画
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);

                if (e.Parameter is NavigationTransitionInfo transition)
                {
                    navigationTransition.DefaultNavigationTransitionInfo = transition;
                }

                bool load = await DotaItemsViewModel.Instance.LoadDotaItems();
                if (load) DotaItemsViewModel.Instance.LoadItemsImages();
            }
            catch (Exception ex) { LogCourier.LogAsync(ex.Message, LogCourier.LogType.Error); }
        }

        /// <summary>
        /// 输入文字搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox)
                {
                    string searching = textBox.Text.Replace(" ", "");
                    ViewModel.SearchItems(searching);
                }
            }
            catch (Exception ex) { LogCourier.LogAsync(ex.Message, LogCourier.LogType.Error); }
        }

        /// <summary>
        /// 点击列表显示信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListView ls && ls.SelectedItem is Core.Models.DotaItemModel item)
                {
                    ViewModel.CurrentItem = item;
                    ItemFrame.Navigate(typeof(ItemInfoPage), null, snti);
                }
            }
            catch (Exception ex) { LogCourier.LogAsync(ex.Message, LogCourier.LogType.Error); }
        }

        /// <summary>
        /// 切换搜索模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearchModeRadioChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton.Tag is string tag)
                {
                    if (tag == "fuzzy")
                    {
                        MainViewModel.AppSettings.bItemsSearchFuzzy = true;
                    }
                    else if (tag == "fullword")
                    {
                        MainViewModel.AppSettings.bItemsSearchFuzzy = false;
                    }
                }
            }
        }

        /// <summary>
        /// Unicode转换汉字
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string UnicodeToString(string text)
        {
            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(text, "\\\\u([\\w]{4})");
            if (mc != null && mc.Count > 0)
            {
                foreach (System.Text.RegularExpressions.Match m2 in mc)
                {
                    string v = m2.Value;
                    string word = v.Substring(2);
                    byte[] codes = new byte[2];
                    int code = Convert.ToInt32(word.Substring(0, 2), 16);
                    int code2 = Convert.ToInt32(word.Substring(2), 16);
                    codes[0] = (byte)code2;
                    codes[1] = (byte)code;
                    text = text.Replace(v, System.Text.Encoding.Unicode.GetString(codes));
                }
            }
            return text;
        }

    }
}
