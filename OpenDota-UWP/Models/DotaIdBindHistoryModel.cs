﻿using OpenDota_UWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace OpenDota_UWP.Models
{
    public class DotaIdBindHistoryModel : ViewModels.ViewModelBase
    {
        public string PlayerName { get; set; } = string.Empty;
        public string AvatarImage { get; set; } = string.Empty;
        public string SteamId { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonIgnore]
        public BitmapImage _ImageSource = null;
        public BitmapImage ImageSource
        {
            get { return _ImageSource; }
            set { Set("ImageSource", ref _ImageSource, value); }
        }
        public async Task LoadImageAsync(int decodeWidth)
        {
            try
            {
                ImageSource = await ImageLoader.LoadImageAsync(AvatarImage);
                ImageSource.DecodePixelType = DecodePixelType.Logical;
                ImageSource.DecodePixelWidth = decodeWidth;
            }
            catch { }
        }
    }
}