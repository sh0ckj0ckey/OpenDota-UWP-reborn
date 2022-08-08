﻿using Newtonsoft.Json;
using OpenDota_UWP.Helpers;
using OpenDota_UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OpenDota_UWP.ViewModels
{
    public class DotaMatchesViewModel : ViewModelBase
    {
        private static Lazy<DotaMatchesViewModel> _lazyVM = new Lazy<DotaMatchesViewModel>(() => new DotaMatchesViewModel());
        public static DotaMatchesViewModel Instance => _lazyVM.Value;

        // 用户ID
        private string _sSteamId = string.Empty;
        public string sSteamId
        {
            get { return _sSteamId; }
            set { Set("sSteamId", ref _sSteamId, value); }
        }

        // 缓存玩家名字和头像
        private Dictionary<string, string> dictPlayersNameCache = new Dictionary<string, string>();
        private Dictionary<string, string> dictPlayersPhotoCache = new Dictionary<string, string>();

        private Windows.Web.Http.HttpClient playerInfoHttpClient = new Windows.Web.Http.HttpClient();
        private Windows.Web.Http.HttpClient matchHttpClient = new Windows.Web.Http.HttpClient();
        private Windows.Web.Http.HttpClient matchInfoHttpClient = new Windows.Web.Http.HttpClient();

        // 用户信息
        private DotaMatchPlayerProfileModel _PlayerProfile = null;
        public DotaMatchPlayerProfileModel PlayerProfile
        {
            get { return _PlayerProfile; }
            set { Set("PlayerProfile", ref _PlayerProfile, value); }
        }

        // 用户胜负场数
        private DotaMatchWinLoseModel _PlayerWinLose = null;
        public DotaMatchWinLoseModel PlayerWinLose
        {
            get { return _PlayerWinLose; }
            set { Set("PlayerWinLose", ref _PlayerWinLose, value); }
        }

        // 在线玩家数量
        private string _sOnlilnePlayersCount = string.Empty;
        public string sOnlilnePlayersCount
        {
            get { return _sOnlilnePlayersCount; }
            set { Set("sOnlilnePlayersCount", ref _sOnlilnePlayersCount, value); }
        }

        // 最近的5场比赛
        public ObservableCollection<DotaRecentMatchModel> vRecentMatchesForFlip = new ObservableCollection<DotaRecentMatchModel>();

        // 最近的比赛
        public ObservableCollection<DotaRecentMatchModel> vRecentMatches = new ObservableCollection<DotaRecentMatchModel>();

        // 所有的比赛
        private List<DotaRecentMatchModel> vAllMatchesList = new List<DotaRecentMatchModel>();
        public ObservableCollection<DotaRecentMatchModel> vAllMatches = new ObservableCollection<DotaRecentMatchModel>();

        // 最常用的10个英雄
        public ObservableCollection<DotaMatchHeroPlayedModel> vMostPlayed10Heroes = new ObservableCollection<DotaMatchHeroPlayedModel>();

        // 所有的最常用英雄
        public ObservableCollection<DotaMatchHeroPlayedModel> vMostPlayedHeroes = new ObservableCollection<DotaMatchHeroPlayedModel>();

        // 玩家的统计数据
        public ObservableCollection<DotaMatchPlayerTotalModel> vPlayerTotals = new ObservableCollection<DotaMatchPlayerTotalModel>();

        // 是否正在加载常用英雄
        private bool _bLoadingPlayed = false;
        public bool bLoadingPlayed
        {
            get { return _bLoadingPlayed; }
            set { Set("bLoadingPlayed", ref _bLoadingPlayed, value); }
        }
        // 是否正在加载所有比赛
        private bool _bLoadingAllMatches = false;
        public bool bLoadingAllMatches
        {
            get { return _bLoadingAllMatches; }
            set { Set("bLoadingAllMatches", ref _bLoadingAllMatches, value); }
        }
        // 是否已经加载完所有的比赛
        private bool _bLoadedAllMatches = false;
        public bool bLoadedAllMatches
        {
            get { return _bLoadedAllMatches; }
            set { Set("bLoadedAllMatches", ref _bLoadedAllMatches, value); }
        }

        // 是否正在加载指定比赛
        private bool _bLoadingOneMatchInfo = false;
        public bool bLoadingOneMatchInfo
        {
            get { return _bLoadingOneMatchInfo; }
            set { Set("bLoadingOneMatchInfo", ref _bLoadingOneMatchInfo, value); }
        }

        // 刷新胜负场次的饼状图
        public Action<double, double> ActUpdatePieChart = null;

        // 是否已经拉取过所有比赛的列表
        private bool _bGottenAllMatchesList = false;

        public DotaMatchesViewModel()
        {
            InitialDotaMatches();
        }

        public async void InitialDotaMatches()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Going to load Matches ---> " + DateTime.Now.Ticks);
                bLoadingPlayed = true;
                PlayerProfile = null;
                PlayerWinLose = null;
                vRecentMatchesForFlip.Clear();
                vRecentMatches.Clear();
                vMostPlayed10Heroes.Clear();
                vMostPlayedHeroes.Clear();
                vPlayerTotals.Clear();

                _bGottenAllMatchesList = false;

                sSteamId = GetSteamID();

                bool triedLoadHeroes = await DotaHeroesViewModel.Instance.LoadDotaHeroes();
                bool triedLoadItems = await DotaItemsViewModel.Instance.LoadDotaItems();

                // 先等获取完英雄和物品列表
                if (!string.IsNullOrEmpty(sSteamId))
                {
                    if (triedLoadHeroes && triedLoadItems)
                    {
                        System.Diagnostics.Debug.WriteLine("Loading Matches ---> " + DateTime.Now.Ticks);

                        // 玩家信息
                        try
                        {
                            var profile = await GetPlayerProfileAsync(sSteamId);
                            if (profile != null)
                            {
                                if (profile.leaderboard_rank is int rank && rank > 0 && profile.rank_tier >= 80)
                                {
                                    if (rank == 1)
                                    {
                                        profile.rank_tier = 84;
                                    }
                                    else if (rank <= 10)
                                    {
                                        profile.rank_tier = 83;
                                    }
                                    else if (rank <= 100)
                                    {
                                        profile.rank_tier = 82;
                                    }
                                    else if (rank <= 1000)
                                    {
                                        profile.rank_tier = 81;
                                    }
                                    else
                                    {
                                        profile.rank_tier = 80;
                                    }
                                }
                            }
                            PlayerProfile = profile;
                            await PlayerProfile?.profile?.LoadIconAsync(72);
                        }
                        catch { }

                        // 胜率
                        try
                        {
                            var wl = await GetPlayerWLAsync(sSteamId);
                            if (wl != null && (wl.win + wl.lose) > 0)
                            {
                                double rate = wl.win / (wl.win + wl.lose);
                                wl.winRate = (Math.Floor(10000 * rate) / 100).ToString() + "%";

                                PlayerWinLose = wl;
                            }
                            if (PlayerWinLose != null)
                                ActUpdatePieChart?.Invoke(PlayerWinLose.win, PlayerWinLose.lose);
                        }
                        catch { }

                        // 统计数据
                        try
                        {
                            var totals = await GetTotalAsync(sSteamId);
                            if (totals != null && totals.Count > 0)
                            {
                                Dictionary<string, string> addingKeys = new Dictionary<string, string>()
                                {
                                    {"kills", "Kills"}, {"deaths", "Deaths"}, {"assists", "Assists"}, {"gold_per_min", "GPM"}, {"xp_per_min", "XPM"},
                                    {"last_hits", "Last Hits"}, {"denies", "Denies"}, {"level", "Level"}, {"hero_damage", "Hero Damage"}, {"tower_damage", "Tower Damage"},
                                    {"hero_healing", "Hero Healing"}
                                };

                                double kills = -1, deaths = -1, assists = -1;

                                foreach (var item in totals)
                                {
                                    if (addingKeys.ContainsKey(item.field))
                                    {
                                        item.field = addingKeys[item.field];
                                        item.n = Math.Floor((item.sum / item.n) * 10) / 10;
                                        vPlayerTotals.Add(item);

                                        if (item.field == "Kills") kills = item.n;
                                        if (item.field == "Deaths") deaths = item.n;
                                        if (item.field == "Assists") assists = item.n;
                                    }
                                }
                                if (kills >= 0 && deaths >= 0 && assists >= 0)
                                {
                                    double kda = 0;
                                    if (deaths <= 0) deaths = 1;
                                    kda = Math.Floor(((kills + assists) / deaths) * 100) / 100;

                                    vPlayerTotals.Insert(0, new DotaMatchPlayerTotalModel() { field = "KDA", n = kda });
                                }
                            }
                        }
                        catch { }

                        // 处理最近的比赛
                        try
                        {
                            var recentMatches = await GetRecentMatchAsync(sSteamId);
                            if (recentMatches != null)
                            {
                                foreach (var item in recentMatches)
                                {
                                    if (DotaHeroesViewModel.Instance.dictAllHeroes.ContainsKey(item.hero_id.ToString()))
                                    {
                                        item.sHeroCoverImage = string.Format("https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/crops/{0}.png",
                                            DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].name.Replace("npc_dota_hero_", ""));
                                        item.sHeroName = DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].localized_name;
                                        item.sHeroHorizonImage = DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].img;
                                        item.bWin = null;
                                        if (item.player_slot != null && item.radiant_win != null)
                                        {
                                            if (item.player_slot < 128)// 天辉
                                                item.bWin = item.radiant_win;
                                            else if (item.player_slot >= 128)// 夜魇
                                                item.bWin = !item.radiant_win;
                                        }
                                        vRecentMatches.Add(item);
                                        if (vRecentMatchesForFlip.Count < 5)
                                            vRecentMatchesForFlip.Add(item);
                                    }
                                }

                                foreach (var item in vRecentMatches)
                                {
                                    await item.LoadHorizonImageAsync(64);
                                }
                                foreach (var item in vRecentMatchesForFlip)
                                {
                                    await item.LoadCoverImageAsync(220);
                                }
                            }
                        }
                        catch { }

                        // 常用英雄
                        try
                        {
                            var heroes = await GetHeroesPlayedAsync(sSteamId);
                            if (heroes != null)
                            {
                                foreach (var item in heroes)
                                {
                                    if (DotaHeroesViewModel.Instance.dictAllHeroes.ContainsKey(item.hero_id.ToString()))
                                    {
                                        item.sHeroCoverImage = string.Format("https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/icons/{0}.png",
                                            DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].name.Replace("npc_dota_hero_", ""));
                                        item.sHeroName = DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].localized_name;

                                        double rate = 0;
                                        if (item.games > 0)
                                            rate = (item.win ?? 0) / (item.games ?? 1);
                                        else
                                            rate = 1;
                                        item.sWinRate = (Math.Floor(1000 * rate) / 10).ToString() + "%";

                                        vMostPlayedHeroes.Add(item);
                                        if (vMostPlayed10Heroes.Count < 10)
                                            vMostPlayed10Heroes.Add(item);
                                    }
                                }
                                foreach (var item in vMostPlayedHeroes)
                                {
                                    await item.LoadImageAsync(36);
                                }
                            }
                        }
                        catch { }
                        bLoadingPlayed = false;

                        // 在线玩家数
                        try
                        {
                            var online = await GetNumberOfCurrentPlayersAsync();
                            if (online?.response?.result == 1)
                            {
                                sOnlilnePlayersCount = online.response.player_count.ToString();
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
            finally
            {
                bLoadingPlayed = false;
            }
        }

        /// <summary>
        /// 绑定保存用户的SteamID
        /// </summary>
        /// <param name="input"></param>
        public void SetSteamID(string steamId)
        {
            try
            {
                // 我的Steam64位ID:76561198194624815
                if (steamId.Length > 14)
                {
                    // 说明输入的是64位的,要先转换成32位
                    decimal id64 = Convert.ToDecimal(steamId);
                    steamId = (id64 - 76561197960265728).ToString();
                }
                App.AppSettingContainer.Values["SteamID"] = steamId;
                sSteamId = steamId;
            }
            catch { }
        }

        /// <summary>
        /// 读取保存的用户的SteamID
        /// </summary>
        /// <returns></returns>
        public string GetSteamID()
        {
            try
            {
                if (App.AppSettingContainer?.Values["SteamID"] != null)
                {
                    return App.AppSettingContainer?.Values["SteamID"].ToString();
                }
            }
            catch { }
            return string.Empty;
        }

        public async Task<T> GetResponseAsync<T>(string url, Windows.Web.Http.HttpClient httpClient)
        {
            try
            {
                var response = await httpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(jsonMessage);
                return result;
            }
            catch { }
            return default(T);
        }


        /// <summary>
        /// 获得用户的个人信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<DotaMatchPlayerProfileModel> GetPlayerProfileAsync(string id)
        {
            try
            {
                string url = string.Format("https://api.opendota.com/api/players/{0}", id); // http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}

                var response = await playerInfoHttpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                var player = JsonConvert.DeserializeObject<DotaMatchPlayerProfileModel>(jsonMessage);
                return player;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 获得用户的胜局败局数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<DotaMatchWinLoseModel> GetPlayerWLAsync(string id)
        {
            try
            {
                string url = string.Format("https://api.opendota.com/api/players/{0}/wl", id);

                var response = await playerInfoHttpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                var wl = JsonConvert.DeserializeObject<DotaMatchWinLoseModel>(jsonMessage);
                return wl;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 获取比赛总数据统计
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<List<DotaMatchPlayerTotalModel>> GetTotalAsync(string id)
        {
            try
            {
                string url = string.Format("https://api.opendota.com/api/players/{0}/totals", id);

                var response = await playerInfoHttpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                var totals = JsonConvert.DeserializeObject<List<DotaMatchPlayerTotalModel>>(jsonMessage);
                return totals;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 获取最近20场比赛
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<List<DotaRecentMatchModel>> GetRecentMatchAsync(string id)
        {
            try
            {
                string url = string.Format("https://api.opendota.com/api/players/{0}/recentMatches", id);

                var response = await matchHttpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                var matches = JsonConvert.DeserializeObject<List<DotaRecentMatchModel>>(jsonMessage);
                return matches;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 获取玩家常用英雄数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<List<DotaMatchHeroPlayedModel>> GetHeroesPlayedAsync(string id)
        {
            try
            {
                string url = string.Format("https://api.opendota.com/api/players/{0}/heroes", id);

                var response = await matchHttpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                var heroes = JsonConvert.DeserializeObject<List<DotaMatchHeroPlayedModel>>(jsonMessage);

                return heroes;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 获取当前在线人数
        /// </summary>
        /// <returns></returns>
        private async Task<DotaOnlinePlayersModel> GetNumberOfCurrentPlayersAsync()
        {
            try
            {
                string url = "http://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1?appid=570&format=json";

                var response = await playerInfoHttpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                var online = JsonConvert.DeserializeObject<DotaOnlinePlayersModel>(jsonMessage);
                return online;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 加载所有比赛的列表
        /// </summary>
        public async void LoadAllMatches()
        {
            try
            {
                if (string.IsNullOrEmpty(sSteamId)) return;
                if (_bGottenAllMatchesList) return;

                bLoadingAllMatches = true;
                bLoadedAllMatches = true;
                vAllMatchesList.Clear();
                vAllMatches.Clear();

                var matches = await GetAllMatchAsync(sSteamId);

                if (matches == null) return;

                _bGottenAllMatchesList = true;

                foreach (var item in matches)
                {
                    if (DotaHeroesViewModel.Instance.dictAllHeroes.ContainsKey(item.hero_id.ToString()))
                    {
                        item.sHeroCoverImage = string.Format("https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/crops/{0}.png",
                            DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].name.Replace("npc_dota_hero_", ""));
                        item.sHeroName = DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].localized_name;
                        item.sHeroHorizonImage = DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].img;
                        item.bWin = null;
                        if (item.player_slot != null && item.radiant_win != null)
                        {
                            if (item.player_slot < 128)// 天辉
                                item.bWin = item.radiant_win;
                            else if (item.player_slot >= 128)// 夜魇
                                item.bWin = !item.radiant_win;
                        }

                        vAllMatchesList.Add(item);
                        if (vAllMatches.Count < 40)
                        {
                            double kda = 0;
                            if (item.kills != null && item.assists != null && item.deaths != null)
                            {
                                if (item.deaths <= 0)
                                    kda = (double)item.kills + (double)item.assists;
                                else
                                    kda = ((double)item.kills + (double)item.assists) / (double)item.deaths;
                            }
                            item.sKda = kda.ToString("f2");
                            vAllMatches.Add(item);
                        }
                    }
                }

                if (vAllMatchesList.Count <= vAllMatches.Count)
                {
                    bLoadedAllMatches = true;
                }
                else
                {
                    bLoadedAllMatches = false;
                }

                foreach (var item in vAllMatches)
                {
                    await item.LoadHorizonImageAsync(64);
                }
            }
            catch { }
            finally { bLoadingAllMatches = false; }
        }

        /// <summary>
        /// 从所有比赛中再取出20条显示
        /// </summary>
        public async void IncreaseFromAllMatches()
        {
            try
            {
                // 未绑定账号或者还没有拉到列表时就不处理
                if (string.IsNullOrEmpty(sSteamId)) return;
                if (!_bGottenAllMatchesList) return;

                int index = vAllMatches.Count;
                for (int i = index; i < index + 30 && i < vAllMatchesList.Count; i++)
                {
                    if (i >= vAllMatchesList.Count) break;

                    var item = vAllMatchesList[i];

                    if (DotaHeroesViewModel.Instance.dictAllHeroes.ContainsKey(item.hero_id.ToString()))
                    {
                        item.sHeroCoverImage = string.Format("https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/crops/{0}.png",
                            DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].name.Replace("npc_dota_hero_", ""));
                        item.sHeroName = DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].localized_name;
                        item.sHeroHorizonImage = DotaHeroesViewModel.Instance.dictAllHeroes[item.hero_id.ToString()].img;
                        item.bWin = null;
                        if (item.player_slot != null && item.radiant_win != null)
                        {
                            if (item.player_slot < 128)// 天辉
                                item.bWin = item.radiant_win;
                            else if (item.player_slot >= 128)// 夜魇
                                item.bWin = !item.radiant_win;
                        }

                        double kda = 0;
                        if (item.kills != null && item.assists != null && item.deaths != null)
                        {
                            if (item.deaths <= 0)
                                kda = (double)item.kills + (double)item.assists;
                            else
                                kda = ((double)item.kills + (double)item.assists) / (double)item.deaths;
                        }
                        item.sKda = kda.ToString("f2");

                        await item.LoadHorizonImageAsync(64);

                        vAllMatches.Add(item);
                    }
                }

                if (vAllMatchesList.Count <= vAllMatches.Count)
                {
                    bLoadedAllMatches = true;
                }
                else
                {
                    bLoadedAllMatches = false;
                }
            }
            catch { }
        }

        /// <summary>
        /// 获取所有比赛
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<List<DotaRecentMatchModel>> GetAllMatchAsync(string id)
        {
            try
            {
                string url = string.Format("https://api.opendota.com/api/players/{0}/matches", id);

                var response = await matchHttpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                var matches = JsonConvert.DeserializeObject<List<DotaRecentMatchModel>>(jsonMessage);
                return matches;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 加载指定比赛
        /// </summary>
        public async void LoadMatchInfo(long matchId, Models.DotaRecentMatchModel currentMatch)
        {
            try
            {
                if (matchId == 0) return;

                bLoadingOneMatchInfo = true;
            }
            catch { }
            finally { bLoadingOneMatchInfo = false; }
        }

        /// <summary>
        /// 获取某场比赛详情
        /// </summary>
        /// <param name="matchId"></param>
        private async void GetMatchInfoAsync(long matchid)
        {
            try
            {
                string url = string.Format("https://api.opendota.com/api/matches/{0}", matchid);    //e.g.3792271763

                var response = await matchInfoHttpClient.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                var matches = JsonConvert.DeserializeObject<object>(jsonMessage);


                //Match first_blood_timeMatch = Regex.Match(jsonMessage, "\\\"first_blood_time\\\":([\\d\\D]*?),");
                ////Match start_timeMatch = Regex.Match(jsonMessage, "\\\"start_time\\\":([\\d\\D]*?),");
                //Match durationMatch = Regex.Match(jsonMessage, "\\\"duration\\\":([\\d\\D]*?),");
                ////Match levelMatch = Regex.Match(jsonMessage, "\\\"skill\\\":([\\d\\D]*?),");
                //Match game_modeMatch = Regex.Match(jsonMessage, "\\\"game_mode\\\":([\\d\\D]*?),");
                //Match radiant_scoreMatch = Regex.Match(jsonMessage, "\\\"radiant_score\\\":([\\d\\D]*?),");
                //Match dire_scoreMatch = Regex.Match(jsonMessage, "\\\"dire_score\\\":([\\d\\D]*?),");
                //Match lobby_typeMatch = Regex.Match(jsonMessage, "\\\"lobby_type\\\":([\\d\\D]*?),");
                //Match radiant_winMatch = Regex.Match(jsonMessage, "\\\"radiant_win\\\":([\\d\\D]*?),");
                //Match radiant_gold_advMatch = Regex.Match(jsonMessage, "\\\"radiant_gold_adv\\\":\\[([\\d\\D]*?)\\],");
                //Match radiant_xp_advMatch = Regex.Match(jsonMessage, "\\\"radiant_xp_adv\\\":\\[([\\d\\D]*?)\\],");
            }
            catch { }
        }

        /// <summary>
        /// 请求更新数据
        /// </summary>
        /// <param name="id"></param>
        public async void PostRefreshAsync(string id)
        {
            try
            {
                string url = string.Format("https://api.opendota.com/api/players/{0}/refresh", id);
                await playerInfoHttpClient.PostAsync(new Uri(url), null);
            }
            catch { }
        }

    }
}
