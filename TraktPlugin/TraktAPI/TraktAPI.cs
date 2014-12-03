﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using TraktPlugin.TraktAPI.DataStructures;
using TraktPlugin.TraktAPI.Enums;
using TraktPlugin.TraktAPI.Extensions;

namespace TraktPlugin.TraktAPI
{
    public static class TraktAPI
    {
        private const string ApplicationId = "d8aed1748b971261dadabba705d85348567579f44ffcec22f8eb8cb982964c78";

        #region Web Events

        // these events can be used to log data sent / received from trakt
        internal delegate void OnDataSendDelegate(string url, string postData);
        internal delegate void OnDataReceivedDelegate(string response);
        internal delegate void OnDataErrorDelegate(string error);

        internal static event OnDataSendDelegate OnDataSend;
        internal static event OnDataReceivedDelegate OnDataReceived;
        internal static event OnDataErrorDelegate OnDataError;

        #endregion

        #region Settings

        // these settings should be set before sending data to trakt
        // exception being the UserToken which is set after logon

        internal static string Username { get; set; }
        internal static string Password { get; set; }
        internal static string UserToken { get; set; }
        internal static string UserAgent { get; set; }
        
        #endregion

        #region Trakt Methods

        #region Authentication

        /// <summary>
        /// Login to trakt and to request a user token for all subsequent requests
        /// </summary>
        /// <returns></returns>
        public static TraktUserToken Login()
        {
            var response = PostToTrakt(TraktURIs.Login, GetUserLogin(), false);
            return response.FromJSON<TraktUserToken>();
        }

        /// <summary>
        /// Gets a User Login object
        /// </summary>       
        /// <returns>The User Login json string</returns>
        private static string GetUserLogin()
        {
            return new TraktAuthentication { Username = TraktAPI.Username, Password = TraktAPI.Password }.ToJSON();
        }

        #endregion

        #region GET Methods

        #region Sync

        public static TraktLastSyncActivities GetLastSyncActivities()
        {
            var response = GetFromTrakt(TraktURIs.SyncLastActivities);
            return response.FromJSON<TraktLastSyncActivities>();
        }

        #endregion

        #region Collection

        public static IEnumerable<TraktMovieCollected> GetCollectedMovies()
        {
            var response = GetFromTrakt(TraktURIs.SyncCollectionMovies);
            
            if (response == null) return null;
            return response.FromJSONArray<TraktMovieCollected>();
        }

        public static IEnumerable<TraktEpisodeCollected> GetCollectedEpisodes()
        {
            var response = GetFromTrakt(TraktURIs.SyncCollectionEpisodes);

            if (response == null) return null;
            return response.FromJSONArray<TraktEpisodeCollected>();
        }

        #endregion

        #region Watched History

        public static IEnumerable<TraktMovieWatched> GetWatchedMovies()
        {
            var response = GetFromTrakt(TraktURIs.SyncWatchedMovies);

            if (response == null) return null;
            return response.FromJSONArray<TraktMovieWatched>();
        }

        public static IEnumerable<TraktEpisodeWatched> GetWatchedEpisodes()
        {
            var response = GetFromTrakt(TraktURIs.SyncWatchedEpisodes);

            if (response == null) return null;
            return response.FromJSONArray<TraktEpisodeWatched>();
        }

        #endregion

        #region Ratings

        public static IEnumerable<TraktMovieRated> GetRatedMovies()
        {
            var response = GetFromTrakt(TraktURIs.SyncRatedMovies);

            if (response == null) return null;
            return response.FromJSONArray<TraktMovieRated>();
        }

        public static IEnumerable<TraktEpisodeRated> GetRatedEpisodes()
        {
            var response = GetFromTrakt(TraktURIs.SyncRatedEpisodes);

            if (response == null) return null;
            return response.FromJSONArray<TraktEpisodeRated>();
        }

        public static IEnumerable<TraktShowRated> GetRatedShows()
        {
            var response = GetFromTrakt(TraktURIs.SyncRatedShows);

            if (response == null) return null;
            return response.FromJSONArray<TraktShowRated>();
        }

        #endregion

        #region Recommendations

        public static IEnumerable<TraktMovieSummary> GetRecommendedMovies(string extendedInfoParams = "min")
        {
            var response = GetFromTrakt(string.Format(TraktURIs.RecommendedMovies, extendedInfoParams));
            return response.FromJSONArray<TraktMovieSummary>();
        }

        public static IEnumerable<TraktShowSummary> GetRecommendedShows()
        {
            var response = GetFromTrakt(TraktURIs.RecommendedShows);
            return response.FromJSONArray<TraktShowSummary>();
        }

        #endregion

        #region User

        /// <summary>
        /// Gets a list of follower requests for the current user
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TraktFollowerRequest> GetFollowerRequests()
        {
            var response = GetFromTrakt(TraktURIs.UserFollowerRequests);
            return response.FromJSONArray<TraktFollowerRequest>();
        }

        /// <summary>
        /// Returns a list of Friends for current user
        /// Friends are a two-way relationship ie. both following each other
        /// </summary>
        public static IEnumerable<TraktNetworkFriend> GetNetworkFriends()
        {
            return GetNetworkFriends(TraktSettings.Username);
        }
        public static IEnumerable<TraktNetworkFriend> GetNetworkFriends(string user)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.NetworkFriends, user));
            return response.FromJSONArray<TraktNetworkFriend>();
        }

        /// <summary>
        /// Returns a list of people the current user follows
        /// </summary>
        public static IEnumerable<TraktNetworkUser> GetNetworkFollowing()
        {
            return GetNetworkFollowing(TraktSettings.Username);
        }
        public static IEnumerable<TraktNetworkUser> GetNetworkFollowing(string user)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.NetworkFollowing, user));
            return response.FromJSONArray<TraktNetworkUser>();
        }

        /// <summary>
        /// Returns a list of people that follow the current user
        /// </summary>
        public static IEnumerable<TraktNetworkUser> GetNetworkFollowers()
        {
            return GetNetworkFollowers(TraktSettings.Username);
        }
        public static IEnumerable<TraktNetworkUser> GetNetworkFollowers(string user)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.NetworkFollowers, user));
            return response.FromJSONArray<TraktNetworkUser>();
        }

        public static TraktNetworkUser NetworkApproveFollower(int id)
        {
            string response = PostToTrakt(string.Format(TraktURIs.NetworkFollowRequest, id), string.Empty);
            return response.FromJSON<TraktNetworkUser>();
        }

        public static bool NetworkDenyFollower(int id)
        {
            return DeleteFromTrakt(string.Format(TraktURIs.NetworkFollowRequest, id));
        }

        public static TraktNetworkApproval NetworkFollowUser(string username)
        {
            string response = PostToTrakt(string.Format(TraktURIs.NetworkFollowUser, username), string.Empty);
            return response.FromJSON<TraktNetworkApproval>();
        }

        public static bool NetworkUnFollowUser(string username)
        {
            return DeleteFromTrakt(string.Format(TraktURIs.NetworkFollowUser, username));
        }

        #endregion

        #region Lists
        
        public static IEnumerable<TraktListDetail> GetUserLists(string username, string extendedInfoParams = "min")
        {
            var response = GetFromTrakt(string.Format(TraktURIs.UserLists, username, extendedInfoParams));
            return response.FromJSONArray<TraktListDetail>();
        }

        public static IEnumerable<TraktListItem> GetUserListItems(string username, string listId, string extendedInfoParams = "min")
        {
            var response = GetFromTrakt(string.Format(TraktURIs.UserListItems, username, listId, extendedInfoParams));
            return response.FromJSONArray<TraktListItem>();
        }

        #endregion

        #region Watchlists

        public static IEnumerable<TraktMovieWatchList> GetWatchListMovies(string username, string extendedInfoParams = "min")
        {
            var response = GetFromTrakt(string.Format(TraktURIs.UserWatchlistMovies, username, extendedInfoParams));
            return response.FromJSONArray<TraktMovieWatchList>();
        }

        public static IEnumerable<TraktShowWatchList> GetWatchListShows(string username)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.UserWatchlistShows, username));
            return response.FromJSONArray<TraktShowWatchList>();
        }

        public static IEnumerable<TraktEpisodeWatchList> GetWatchListEpisodes(string username)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.UserWatchlistEpisodes, username));
            return response.FromJSONArray<TraktEpisodeWatchList>();
        }

        #endregion

        #region Movies

        #region Related

        public static IEnumerable<TraktMovieSummary> GetRelatedMovies(string id, bool hideWatched = false)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.RelatedMovies, id));
            return response.FromJSONArray<TraktMovieSummary>();
        }

        #endregion

        #region Comments

        public static IEnumerable<TraktComment> GetMovieComments(string id)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.MovieComments, id));
            return response.FromJSONArray<TraktComment>();
        }

        #endregion

        #region Trending

        public static IEnumerable<TraktMovieTrending> GetTrendingMovies(int page = 1, int maxItems = 100)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.TrendingMovies, page, maxItems));
            return response.FromJSONArray<TraktMovieTrending>();
        }

        #endregion

        #endregion

        #region Shows

        #region Related

        public static IEnumerable<TraktShowSummary> GetRelatedShows(string id, bool hideWatched = false)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.RelatedShows, id));
            return response.FromJSONArray<TraktShowSummary>();
        }

        #endregion

        #region Seasons

        public static IEnumerable<TraktSeasonSummary> GetShowSeasons(string id)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.ShowSeasons, id));
            return response.FromJSONArray<TraktSeasonSummary>();
        }

        #endregion

        #region Comments

        public static IEnumerable<TraktComment> GetShowComments(string id)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.ShowComments, id));
            return response.FromJSONArray<TraktComment>();
        }

        #endregion

        #region Trending

        public static IEnumerable<TraktShowTrending> GetTrendingShows(int page = 1, int maxItems = 100)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.TrendingShows, page, maxItems));
            return response.FromJSONArray<TraktShowTrending>();
        }

        #endregion

        #endregion

        #region Episodes

        #region Comments

        public static IEnumerable<TraktComment> GetEpisodeComments(string id, int season, int episode)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.EpisodeComments, id, season, episode));
            return response.FromJSONArray<TraktComment>();
        }

        #endregion

        #region Season Episodes

        public static IEnumerable<TraktEpisodeSummary> GetSeasonEpisodes(string showId, string seasonId)
        {
            var response = GetFromTrakt(string.Format(TraktURIs.SeasonEpisodes, showId, seasonId));
            return response.FromJSONArray<TraktEpisodeSummary>();
        }

        #endregion

        #endregion

        #endregion

        #region Activity TODO

        public static TraktActivity GetFriendActivity()
        {
            return GetFriendActivity(null, null, false);
        }
        public static TraktActivity GetFriendActivity(bool includeMe)
        {
            return GetFriendActivity(null, null, includeMe);
        }
        public static TraktActivity GetFriendActivity(List<ActivityType> types, List<ActivityAction> actions, bool includeMe)
        {
            return GetFriendActivity(types, actions, 0, 0, includeMe);
        }
        public static TraktActivity GetFriendActivity(List<ActivityType> types, List<ActivityAction> actions, long start, long end, bool includeMe)
        {
            //// get comma seperated list of types and actions (if more than one)
            //string activityTypes = types == null ? "all" : string.Join(",", types.Select(t => t.ToString()).ToArray());
            //string activityActions = actions == null ? "all" : string.Join(",", actions.Select(a => a.ToString()).ToArray());

            //string startEnd = (start == 0 || end == 0) ? string.Empty : string.Format("/{0}/{1}", start, end);
            //string apiUrl = includeMe ? TraktURIs.ActivityFriendsMe : TraktURIs.ActivityFriends;

            //string activity = Transmit(string.Format(apiUrl, activityTypes, activityActions, startEnd), GetUserAuthentication());
            //return activity.FromJSON<TraktActivity>();
            return null;
        }

        public static TraktActivity GetFollowingActivity()
        {
            return GetFollowingActivity(null, null);
        }
        public static TraktActivity GetFollowingActivity(List<ActivityType> types, List<ActivityAction> actions)
        {
            return GetFollowingActivity(types, actions, 0, 0);
        }
        public static TraktActivity GetFollowingActivity(List<ActivityType> types, List<ActivityAction> actions, long start, long end)
        {
            //// get comma seperated list of types and actions (if more than one)
            //string activityTypes = types == null ? "all" : string.Join(",", types.Select(t => t.ToString()).ToArray());
            //string activityActions = actions == null ? "all" : string.Join(",", actions.Select(a => a.ToString()).ToArray());

            //string startEnd = (start == 0 || end == 0) ? string.Empty : string.Format("/{0}/{1}", start, end);

            //string activity = Transmit(string.Format(TraktURIs.ActivityFollowing, activityTypes, activityActions, startEnd), GetUserAuthentication());
            //return activity.FromJSON<TraktActivity>();
            return null;
        }

        public static TraktActivity GetFollowersActivity()
        {
            return GetFollowersActivity(null, null);
        }
        public static TraktActivity GetFollowersActivity(List<ActivityType> types, List<ActivityAction> actions)
        {
            return GetFollowersActivity(types, actions, 0, 0);
        }
        public static TraktActivity GetFollowersActivity(List<ActivityType> types, List<ActivityAction> actions, long start, long end)
        {
            //// get comma seperated list of types and actions (if more than one)
            //string activityTypes = types == null ? "all" : string.Join(",", types.Select(t => t.ToString()).ToArray());
            //string activityActions = actions == null ? "all" : string.Join(",", actions.Select(a => a.ToString()).ToArray());

            //string startEnd = (start == 0 || end == 0) ? string.Empty : string.Format("/{0}/{1}", start, end);

            //string activity = Transmit(string.Format(TraktURIs.ActivityFollowers, activityTypes, activityActions, startEnd), GetUserAuthentication());
            //return activity.FromJSON<TraktActivity>();
            return null;
        }

        public static TraktActivity GetCommunityActivity()
        {
            return GetCommunityActivity(null, null);
        }
        public static TraktActivity GetCommunityActivity(List<ActivityType> types, List<ActivityAction> actions)
        {
            return GetCommunityActivity(types, actions, 0, 0);
        }
        public static TraktActivity GetCommunityActivity(List<ActivityType> types, List<ActivityAction> actions, long start, long end)
        {
            //// get comma seperated list of types and actions (if more than one)
            //string activityTypes = types == null ? "all" : string.Join(",", types.Select(t => t.ToString()).ToArray());
            //string activityActions = actions == null ? "all" : string.Join(",", actions.Select(a => a.ToString()).ToArray());

            //string startEnd = (start == 0 || end == 0) ? string.Empty : string.Format("/{0}/{1}", start, end);

            //string activity = Transmit(string.Format(TraktURIs.ActivityCommunity, activityTypes, activityActions, startEnd), GetUserAuthentication());
            //return activity.FromJSON<TraktActivity>();
            return null;
        }

        public static TraktActivity GetUserActivity(string username, List<ActivityType> types, List<ActivityAction> actions)
        {
            //// get comma seperated list of types and actions (if more than one)
            //string activityTypes = string.Join(",", types.Select(t => t.ToString()).ToArray());
            //string activityActions = string.Join(",", actions.Select(a => a.ToString()).ToArray());

            //string activity = Transmit(string.Format(TraktURIs.ActivityUser, username, activityTypes, activityActions), GetUserAuthentication());
            //return activity.FromJSON<TraktActivity>();
            return null;
        }

        #endregion

        #region Search

        static readonly Object searchLock = new Object();

        /// <summary>
        /// Search from one or more types, movies, episodes, shows etc...
        /// </summary>
        /// <param name="searchTerm">string to search for</param>
        /// <param name="types">a list of search types</param>
        /// <returns>returns results from multiple search types</returns>
        public static IEnumerable<TraktSearchResult> Search(string searchTerm, HashSet<SearchType> types, int maxResults)
        {
            // collect all the results from each type in this list
            List<TraktSearchResult> results = new List<TraktSearchResult>();

            // run all search types in parallel
            List<Thread> threads = new List<Thread>();

            foreach (var type in types)
            {
                switch (type)
                {
                    case SearchType.movies:
                        var tMovieSearch = new Thread(obj => 
                        { 
                            var response = SearchMovies(obj as string, maxResults);
                            lock (searchLock)
                            {
                                results.AddRange(response);
                            }

                        });
                        tMovieSearch.Start(searchTerm);
                        tMovieSearch.Name = "Search";
                        threads.Add(tMovieSearch);
                        break;

                    case SearchType.shows:
                        var tShowSearch = new Thread(obj =>
                        {
                            var response = SearchMovies(obj as string, maxResults);
                            lock (searchLock)
                            {
                                results.AddRange(response);
                                lock (searchLock)
                                {
                                    results.AddRange(response);
                                }
                            }
                        });
                        tShowSearch.Start(searchTerm);
                        tShowSearch.Name = "Search";
                        threads.Add(tShowSearch);
                        break;

                    case SearchType.episodes:
                        var tEpisodeSearch = new Thread(obj =>
                        {
                            var response = SearchMovies(obj as string, maxResults);
                            lock (searchLock)
                            {
                                results.AddRange(response);
                            }
                        });
                        tEpisodeSearch.Start(searchTerm);
                        tEpisodeSearch.Name = "Search";
                        threads.Add(tEpisodeSearch);
                        break;

                    case SearchType.people:
                        var tPeopleSearch = new Thread(obj =>
                        {
                            var response = SearchMovies(obj as string, maxResults);
                            lock (searchLock)
                            {
                                results.AddRange(response);
                            }
                        });
                        tPeopleSearch.Start(searchTerm);
                        tPeopleSearch.Name = "Search";
                        threads.Add(tPeopleSearch);
                        break;

                    case SearchType.users:
                        var tUserSearch = new Thread(obj =>
                        {
                            var response = SearchMovies(obj as string, maxResults);
                            lock (searchLock)
                            {
                                results.AddRange(response);
                            }
                        });
                        tUserSearch.Start(searchTerm);
                        tUserSearch.Name = "Search";
                        threads.Add(tUserSearch);
                        break;

                    case SearchType.lists:
                        var tListSearch = new Thread(obj =>
                        {
                            var response = SearchMovies(obj as string, maxResults);
                            lock (searchLock)
                            {
                                results.AddRange(response);
                            }
                        });
                        tListSearch.Start(searchTerm);
                        tListSearch.Name = "Search";
                        threads.Add(tListSearch);
                        break;
                }
            }

            // wait until all search results are back
            threads.ForEach(t => t.Join());

            // now we have everything we need
            return results;
        }

        /// <summary>
        /// Returns a list of users found using search term
        /// </summary>
        public static IEnumerable<TraktSearchResult> SearchForUsers(string searchTerm)
        {
            return SearchUsers(searchTerm, 30);
        }
        public static IEnumerable<TraktSearchResult> SearchUsers(string searchTerm, int maxResults)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.SearchUsers, HttpUtility.UrlEncode(searchTerm), 1, maxResults));
            return response.FromJSONArray<TraktSearchResult>();
        }

        /// <summary>
        /// Returns a list of movies found using search term
        /// </summary>
        public static IEnumerable<TraktSearchResult> SearchMovies(string searchTerm)
        {
            return SearchMovies(searchTerm, 30);
        }
        public static IEnumerable<TraktSearchResult> SearchMovies(string searchTerm, int maxResults)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.SearchMovies, HttpUtility.UrlEncode(searchTerm), 1, maxResults));
            return response.FromJSONArray<TraktSearchResult>();
        }

        /// <summary>
        /// Returns a list of shows found using search term
        /// </summary>
        public static IEnumerable<TraktSearchResult> SearchShows(string searchTerm)
        {
            return SearchShows(searchTerm, 30);
        }
        public static IEnumerable<TraktSearchResult> SearchShows(string searchTerm, int maxResults)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.SearchShows, HttpUtility.UrlEncode(searchTerm), 1, maxResults));
            return response.FromJSONArray<TraktSearchResult>();
        }

        /// <summary>
        /// Returns a list of episodes found using search term
        /// </summary>
        public static IEnumerable<TraktSearchResult> SearchEpisodes(string searchTerm)
        {
            return SearchEpisodes(searchTerm, 30);
        }
        public static IEnumerable<TraktSearchResult> SearchEpisodes(string searchTerm, int maxResults)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.SearchEpisodes, HttpUtility.UrlEncode(searchTerm), 1, maxResults));
            return response.FromJSONArray<TraktSearchResult>();
        }

        /// <summary>
        /// Returns a list of people found using search term
        /// </summary>
        public static IEnumerable<TraktSearchResult> SearchPeople(string searchTerm)
        {
            return SearchPeople(searchTerm, 30);
        }
        public static IEnumerable<TraktSearchResult> SearchPeople(string searchTerm, int maxResults)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.SearchPeople, HttpUtility.UrlEncode(searchTerm), 1, maxResults));
            return response.FromJSONArray<TraktSearchResult>();
        }

        /// <summary>
        /// Returns a list of lists found using search term
        /// </summary>
        public static IEnumerable<TraktSearchResult> SearchLists(string searchTerm)
        {
            return SearchLists(searchTerm, 30);
        }
        public static IEnumerable<TraktSearchResult> SearchLists(string searchTerm, int maxResults)
        {
            string response = GetFromTrakt(string.Format(TraktURIs.SearchLists, HttpUtility.UrlEncode(searchTerm), 1, maxResults));
            return response.FromJSONArray<TraktSearchResult>();
        }

        #endregion

        #region POST Methods

        #region Collection

        public static TraktSyncResponse AddMoviesToCollecton(TraktSyncMoviesCollected movies)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionAdd, movies.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveMoviesFromCollecton(TraktSyncMovies movies)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionRemove, movies.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddShowsToCollectonEx(TraktSyncShowsEx shows)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddShowsToCollecton(TraktSyncShows shows)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveShowsFromCollecton(TraktSyncShows shows)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddEpisodesToCollecton(TraktSyncEpisodesCollected episodes)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionAdd, episodes.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveEpisodesFromCollecton(TraktSyncEpisodes episodes)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionRemove, episodes.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddShowsToCollectonEx(TraktSyncShowsCollectedEx shows)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }
        
        public static TraktSyncResponse RemoveShowsFromCollectonEx(TraktSyncShowsEx shows)
        {
            var response = PostToTrakt(TraktURIs.SyncCollectionRemove, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        #endregion

        #region Collection (Single)

        public static TraktSyncResponse AddMovieToCollection(TraktSyncMovieCollected movie)
        {
            var movies = new TraktSyncMoviesCollected
            {
                Movies = new List<TraktSyncMovieCollected>() { movie }
            };

            return AddMoviesToCollecton(movies);
        }

        public static TraktSyncResponse RemoveMovieFromCollection(TraktMovie movie)
        {
            var movies = new TraktSyncMovies
            {
                Movies = new List<TraktMovie>() { movie }
            };

            return RemoveMoviesFromCollecton(movies);
        }

        public static TraktSyncResponse AddShowToCollection(TraktShow show)
        {
            var shows = new TraktSyncShows
            {
                Shows = new List<TraktShow>() { show }
            };

            return AddShowsToCollecton(shows);
        }

        public static TraktSyncResponse RemoveShowFromCollection(TraktShow show)
        {
            var shows = new TraktSyncShows
            {
                Shows = new List<TraktShow>() { show }
            };

            return RemoveShowsFromCollecton(shows);
        }

        public static TraktSyncResponse AddShowToCollectionEx(TraktSyncShowEx show)
        {
            var shows = new TraktSyncShowsEx
            {
                Shows = new List<TraktSyncShowEx>() { show }
            };

            return AddShowsToCollectonEx(shows);
        }

        public static TraktSyncResponse RemoveShowFromCollectionEx(TraktSyncShowEx show)
        {
            var shows = new TraktSyncShowsEx
            {
                Shows = new List<TraktSyncShowEx>() { show }
            };

            return RemoveShowsFromCollectonEx(shows);
        }

        public static TraktSyncResponse AddEpisodeToCollection(TraktSyncEpisodeCollected episode)
        {
            var episodes = new TraktSyncEpisodesCollected
            {
                Episodes = new List<TraktSyncEpisodeCollected>() { episode }
            };

            return AddEpisodesToCollecton(episodes);
        }

        public static TraktSyncResponse RemoveEpisodeFromCollection(TraktEpisode episode)
        {
            var episodes = new TraktSyncEpisodes
            {
                Episodes = new List<TraktEpisode>() { episode }
            };

            return RemoveEpisodesFromCollecton(episodes);
        }

        #endregion

        #region Watched History

        public static TraktSyncResponse AddMoviesToWatchedHistory(TraktSyncMoviesWatched movies)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchedHistoryAdd, movies.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveMoviesFromWatchedHistory(TraktSyncMovies movies)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchedHistoryRemove, movies.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddShowsToWatchedHistory(TraktSyncShows shows)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchedHistoryAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveShowsFromWatchedHistory(TraktSyncShows shows)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchedHistoryRemove, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddEpisodesToWatchedHistory(TraktSyncEpisodesWatched episodes)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchedHistoryAdd, episodes.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveEpisodesFromWatchedHistory(TraktSyncEpisodes episodes)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchedHistoryRemove, episodes.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddShowsToWatchedHistoryEx(TraktSyncShowsEx shows)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchedHistoryAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveShowsFromWatchedHistoryEx(TraktSyncShowsEx shows)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchedHistoryRemove, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        #endregion

        #region Watched History (Single)

        public static TraktSyncResponse AddMovieToWatchedHistory(TraktSyncMovieWatched movie)
        {
            var movies = new TraktSyncMoviesWatched
            {
                Movies = new List<TraktSyncMovieWatched>() { movie }
            };

            return AddMoviesToWatchedHistory(movies);
        }

        public static TraktSyncResponse RemoveMovieFromWatchedHistory(TraktMovie movie)
        {
            var movies = new TraktSyncMovies
            {
                Movies = new List<TraktMovie>() { movie }
            };

            return RemoveMoviesFromWatchedHistory(movies);
        }

        public static TraktSyncResponse AddShowToWatchedHistory(TraktShow show)
        {
            var shows = new TraktSyncShows
            {
                Shows = new List<TraktShow>() { show }
            };

            return AddShowsToWatchedHistory(shows);
        }

        public static TraktSyncResponse RemoveShowFromWatchedHistory(TraktShow show)
        {
            var shows = new TraktSyncShows
            {
                Shows = new List<TraktShow>() { show }
            };

            return RemoveShowsFromWatchedHistory(shows);
        }

        public static TraktSyncResponse AddShowToWatchedHistoryEx(TraktSyncShowEx show)
        {
            var shows = new TraktSyncShowsEx
            {
                Shows = new List<TraktSyncShowEx>() { show }
            };

            return AddShowsToWatchedHistoryEx(shows);
        }

        public static TraktSyncResponse RemoveShowFromWatchedHistoryEx(TraktSyncShowEx show)
        {
            var shows = new TraktSyncShowsEx
            {
                Shows = new List<TraktSyncShowEx>() { show }
            };

            return RemoveShowsFromWatchedHistoryEx(shows);
        }

        public static TraktSyncResponse AddEpisodeToWatchedHistory(TraktSyncEpisodeWatched episode)
        {
            var episodes = new TraktSyncEpisodesWatched
            {
                Episodes = new List<TraktSyncEpisodeWatched>() { episode }
            };

            return AddEpisodesToWatchedHistory(episodes);
        }

        public static TraktSyncResponse RemoveEpisodeFromWatchedHistory(TraktEpisode episode)
        {
            var episodes = new TraktSyncEpisodes
            {
                Episodes = new List<TraktEpisode>() { episode }
            };

            return RemoveEpisodesFromWatchedHistory(episodes);
        }

        #endregion

        #region Ratings

        public static TraktSyncResponse AddMoviesToRatings(TraktSyncMoviesRated movies)
        {
            var response = PostToTrakt(TraktURIs.SyncRatingsAdd, movies.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveMoviesFromRatings(TraktSyncMovies movies)
        {
            var response = PostToTrakt(TraktURIs.SyncRatingsRemove, movies.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddShowsToRatings(TraktSyncShowsRated shows)
        {
            var response = PostToTrakt(TraktURIs.SyncRatingsAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveShowsFromRatings(TraktSyncShows shows)
        {
            var response = PostToTrakt(TraktURIs.SyncRatingsRemove, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddEpisodesToRatings(TraktSyncEpisodesRated episodes)
        {
            var response = PostToTrakt(TraktURIs.SyncRatingsAdd, episodes.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddShowsToRatingsEx(TraktSyncShowsRatedEx shows)
        {
            var response = PostToTrakt(TraktURIs.SyncRatingsAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveEpisodesFromRatings(TraktSyncEpisodes episodes)
        {
            var response = PostToTrakt(TraktURIs.SyncRatingsRemove, episodes.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveShowsFromRatingsEx(TraktSyncShowsRatedEx shows)
        {
            var response = PostToTrakt(TraktURIs.SyncRatingsRemove, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        #endregion

        #region Ratings (Single)

        /// <summary>
        /// Rate a single episode on trakt.tv
        /// </summary>
        public static TraktSyncResponse AddEpisodeToRatings(TraktSyncEpisodeRated episode)
        {
            var episodes = new TraktSyncEpisodesRated
            {
                Episodes = new List<TraktSyncEpisodeRated>() { episode }
            };

            return AddEpisodesToRatings(episodes);
        }

        /// <summary>
        /// UnRate a single episode on trakt.tv
        /// </summary>
        public static TraktSyncResponse RemoveEpisodeFromRatings(TraktEpisode episode)
        {
            var episodes = new TraktSyncEpisodes
            {
                Episodes = new List<TraktEpisode>() { episode }
            };

            return RemoveEpisodesFromRatings(episodes);
        }

        /// <summary>
        /// Rate a single episode on trakt.tv (with show info)
        /// </summary>
        public static TraktSyncResponse AddEpisodeToRatingsEx(TraktSyncShowRatedEx item)
        {
            var episodes = new TraktSyncShowsRatedEx
            {
                Shows = new List<TraktSyncShowRatedEx>() { item }
            };

            return AddShowsToRatingsEx(episodes);
        }

        /// <summary>
        /// UnRate a single episode on trakt.tv (with show info)
        /// </summary>
        public static TraktSyncResponse RemoveEpisodeFromRatingsEx(TraktSyncShowRatedEx item)
        {
            var episodes = new TraktSyncShowsRatedEx
            {
                Shows = new List<TraktSyncShowRatedEx>() { item }
            };

            return RemoveShowsFromRatingsEx(episodes);
        }

        /// <summary>
        /// Rate a single show on trakt.tv
        /// </summary>
        public static TraktSyncResponse AddShowToRatings(TraktSyncShowRated show)
        {
            var shows = new TraktSyncShowsRated
            {
                Shows = new List<TraktSyncShowRated>() { show }
            };

            return AddShowsToRatings(shows);
        }

        /// <summary>
        /// UnRate a single show on trakt.tv
        /// </summary>
        public static TraktSyncResponse RemoveShowFromRatings(TraktShow show)
        {
            var shows = new TraktSyncShows
            {
                Shows = new List<TraktShow>() { show }
            };

            return RemoveShowsFromRatings(shows);
        }

        /// <summary>
        /// Rate a single movie on trakt.tv
        /// </summary>
        public static TraktSyncResponse AddMovieToRatings(TraktSyncMovieRated movie)
        {
            var movies = new TraktSyncMoviesRated
            {
                Movies = new List<TraktSyncMovieRated>() { movie }
            };

            return AddMoviesToRatings(movies);
        }

        /// <summary>
        /// UnRate a single movie on trakt.tv
        /// </summary>
        public static TraktSyncResponse RemoveMovieFromRatings(TraktMovie movie)
        {
            var movies = new TraktSyncMovies
            {
                Movies = new List<TraktMovie>() { movie }
            };

            return RemoveMoviesFromRatings(movies);
        }

        #endregion

        #region Scrobble

        public static TraktScrobbleResponse StartMovieScrobble(TraktScrobbleMovie movie)
        {
            var response = PostToTrakt(TraktURIs.ScrobbleStart, movie.ToJSON());
            return response.FromJSON<TraktScrobbleResponse>();
        }

        public static TraktScrobbleResponse StartEpisodeScrobble(TraktScrobbleEpisode episode)
        {
            var response = PostToTrakt(TraktURIs.ScrobbleStart, episode.ToJSON());
            return response.FromJSON<TraktScrobbleResponse>();
        }

        public static TraktScrobbleResponse PauseMovieScrobble(TraktScrobbleMovie movie)
        {
            var response = PostToTrakt(TraktURIs.ScrobblePause, movie.ToJSON());
            return response.FromJSON<TraktScrobbleResponse>();
        }

        public static TraktScrobbleResponse PauseEpisodeScrobble(TraktScrobbleEpisode episode)
        {
            var response = PostToTrakt(TraktURIs.ScrobblePause, episode.ToJSON());
            return response.FromJSON<TraktScrobbleResponse>();
        }

        public static TraktScrobbleResponse StopMovieScrobble(TraktScrobbleMovie movie)
        {
            var response = PostToTrakt(TraktURIs.ScrobbleStop, movie.ToJSON());
            return response.FromJSON<TraktScrobbleResponse>();
        }

        public static TraktScrobbleResponse StopEpisodeScrobble(TraktScrobbleEpisode episode)
        {
            var response = PostToTrakt(TraktURIs.ScrobbleStop, episode.ToJSON());
            return response.FromJSON<TraktScrobbleResponse>();
        }

        #endregion

        #region Watchlist

        public static TraktSyncResponse AddMoviesToWatchlist(TraktSyncMovies movies)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchlistAdd, movies.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveMoviesFromWatchlist(TraktSyncMovies movies)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchlistRemove, movies.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddShowsToWatchlist(TraktSyncShows shows)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchlistAdd, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveShowsFromWatchlist(TraktSyncShows shows)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchlistRemove, shows.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse AddEpisodesToWatchlist(TraktSyncEpisodes episodes)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchlistAdd, episodes.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        public static TraktSyncResponse RemoveEpisodesFromWatchlist(TraktSyncEpisodes episodes)
        {
            var response = PostToTrakt(TraktURIs.SyncWatchlistRemove, episodes.ToJSON());
            return response.FromJSON<TraktSyncResponse>();
        }

        #endregion

        #region Watchlist (Single)

        public static TraktSyncResponse AddMovieToWatchlist(TraktMovie movie)
        {
            var movies = new TraktSyncMovies
            {
                Movies = new List<TraktMovie>() { movie }
            };

            return AddMoviesToWatchlist(movies);
        }

        public static TraktSyncResponse RemoveMovieFromWatchlist(TraktMovie movie)
        {
            var movies = new TraktSyncMovies
            {
                Movies = new List<TraktMovie>() { movie }
            };

            return RemoveMoviesFromWatchlist(movies);
        }

        public static TraktSyncResponse AddShowToWatchlist(TraktShow show)
        {
            var shows = new TraktSyncShows
            {
                Shows = new List<TraktShow>() { show }
            };

            return AddShowsToWatchlist(shows);
        }

        public static TraktSyncResponse RemoveShowFromWatchlist(TraktShow show)
        {
            var shows = new TraktSyncShows
            {
                Shows = new List<TraktShow>() { show }
            };

            return RemoveShowsFromWatchlist(shows);
        }

        public static TraktSyncResponse AddEpisodeToWatchlist(TraktEpisode episode)
        {
            var episodes = new TraktSyncEpisodes
            {
                Episodes = new List<TraktEpisode>() { episode }
            };

            return AddEpisodesToWatchlist(episodes);
        }

        public static TraktSyncResponse RemoveEpisodeFromWatchlist(TraktEpisode episode)
        {
            var episodes = new TraktSyncEpisodes
            {
                Episodes = new List<TraktEpisode>() { episode }
            };

            return RemoveEpisodesFromWatchlist(episodes);
        }

        #endregion

        #endregion

        #region DELETE Methods

        #region Dismiss Recommendations

        public static bool DismissRecommendedMovie(string movieId)
        {
            return DeleteFromTrakt(string.Format(TraktURIs.DismissRecommendedMovie, movieId));
        }

        public static bool DismissRecommendedShow(string showId)
        {
            return DeleteFromTrakt(string.Format(TraktURIs.DismissRecommendedShow, showId));
        }

        #endregion

        #region Delete List

        public static bool DeleteUserList(string username, string listId)
        {
            return DeleteFromTrakt(string.Format(TraktURIs.DeleteList, username, listId));
        }

        #endregion

        #endregion

        #region Web Helpers

        public static bool DeleteFromTrakt(string address)
        {
            var response = GetFromTrakt(address, "DELETE");
            return response != null;
        }

        public static string GetFromTrakt(string address, string method = "GET")
        {
            if (OnDataSend != null)
                OnDataSend(address, null);

            var request = WebRequest.Create(address) as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = method;
            request.ContentLength = 0;
            request.Timeout = 120000;
            request.ContentType = "application/json";
            request.UserAgent = UserAgent;

            // add required headers for authorisation
            request.Headers.Add("trakt-api-version", "2");
            request.Headers.Add("trakt-api-key", ApplicationId);
            request.Headers.Add("trakt-user-login", Username);
            request.Headers.Add("trakt-user-token", UserToken ?? string.Empty);

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response == null) return null;

                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string strResponse = reader.ReadToEnd();

                if (method == "DELETE")
                {
                    strResponse = response.StatusCode.ToString();
                }
                
                if (OnDataReceived != null)
                    OnDataReceived(strResponse);

                stream.Close();
                reader.Close();
                response.Close();

                return strResponse;
            }
            catch (WebException ex)
            {
                string errorMessage = ex.Message;
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    errorMessage = string.Format("The API responded to the request with the following error. Code = '{0}', Description = '{1}'", (int)response.StatusCode, response.StatusDescription);
                }

                if (OnDataError != null)
                    OnDataError(ex.Message);

                return null;
            }
        }

        public static string PostToTrakt(string address, string postData, bool logRequest = true)
        {
            if (OnDataSend != null && logRequest)
                OnDataSend(address, postData);

            byte[] data = new UTF8Encoding().GetBytes(postData);

            var request = WebRequest.Create(address) as HttpWebRequest;
            request.KeepAlive = true;

            request.Method = "POST";
            request.ContentLength = data.Length;
            request.Timeout = 120000;
            request.ContentType = "application/json";
            request.UserAgent = UserAgent;

            // add required headers for authorisation
            request.Headers.Add("trakt-api-version", "2");
            request.Headers.Add("trakt-api-key", ApplicationId);
            request.Headers.Add("trakt-user-login", Username);
            request.Headers.Add("trakt-user-token", UserToken);

            try
            {
                // post to trakt
                Stream postStream = request.GetRequestStream();
                postStream.Write(data, 0, data.Length);

                // get the response
                var response = (HttpWebResponse)request.GetResponse();
                if (response == null) return null;

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string strResponse = reader.ReadToEnd();

                if (OnDataReceived != null)
                    OnDataReceived(strResponse);

                // cleanup
                postStream.Close();
                responseStream.Close();
                reader.Close();
                response.Close();

                return strResponse;
            }
            catch (WebException ex)
            {
                string errorMessage = ex.Message;
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    errorMessage = string.Format("The API responded to the request with the following error. Code = '{0}', Description = '{1}'", (int)response.StatusCode, response.StatusDescription);
                }

                if (OnDataError != null)
                    OnDataError(ex.Message);

                return null;
            }
        }

        #endregion

        #endregion
    }
}
