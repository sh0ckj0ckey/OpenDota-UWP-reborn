﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace OpenDota_UWP.Helpers
{
    /// <summary>
    /// 这是一只信使，帮你运送你需要的本地文件
    /// </summary>
    public class StorageFilesCourier
    {
        //存储数据的文件夹名称
        private const string DataFolderName = "OpenDotaData";

        //存储数据的文件夹对象(单例，见下面的GetDataFolder方法)
        private static StorageFolder DataFolder = null;

        /// <summary>
        /// 获取存储数据的文件夹的对象
        /// </summary>
        /// <returns></returns>
        public static async Task<StorageFolder> GetDataFolder()
        {
            if (DataFolder == null)
            {
                DataFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(DataFolderName, CreationCollisionOption.OpenIfExists);
            }
            return DataFolder;
        }

        /// <summary>
        /// 读取本地文件夹根目录的文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<string> ReadFileAsync(string fileName)
        {
            string text = string.Empty;
            try
            {
                StorageFolder applicationFolder = await GetDataFolder();
                var storageFile = await applicationFolder.GetFileAsync(fileName);
                if (storageFile != null)
                {
                    IRandomAccessStream accessStream = await storageFile.OpenReadAsync();
                    using (StreamReader streamReader = new StreamReader(accessStream.AsStreamForRead((int)accessStream.Size)))
                    {
                        text = streamReader.ReadToEnd();
                    }
                }
            }
            catch { }
            return text;
        }

        /// <summary>
        /// 写入本地文件夹根目录的文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task<bool> WriteFileAsync(string fileName, string content)
        {
            try
            {
                StorageFolder applicationFolder = await GetDataFolder();
                StorageFile storageFile = await applicationFolder.CreateFileAsync(fileName + "Tmp", CreationCollisionOption.ReplaceExisting);

                int retryAttempts = 3;
                const int ERROR_ACCESS_DENIED = unchecked((int)0x80070005);
                const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);

                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.WriteTextAsync(storageFile, content);
                        await storageFile.RenameAsync(fileName, NameCollisionOption.ReplaceExisting);
                        return true;
                    }
                    catch (Exception ex) when ((ex.HResult == ERROR_ACCESS_DENIED) || (ex.HResult == ERROR_SHARING_VIOLATION))
                    {
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    catch { }
                }
            }
            catch { }
            return false;
        }
    }
}
