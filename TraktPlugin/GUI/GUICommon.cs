﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Video.Database;
using Trailers.Providers;
using TraktPlugin.Cache;
using TraktPlugin.Extensions;
using TraktPlugin.TraktAPI.DataStructures;
using TraktPlugin.TraktAPI.Enums;
using TraktPlugin.TraktAPI.Extensions;

namespace TraktPlugin.GUI
{
    #region Enums

    public enum Layout 
    {
        List = 0,
        SmallIcons = 1,
        LargeIcons = 2,
        Filmstrip = 3,
    }

    enum ActivityContextMenuItem
    {
        FilterTypes,
        FilterActions,
        ChangeView,
        ShowSeasonInfo,
        MarkAsWatched,
        MarkAsUnwatched,
        AddToCollection,
        RemoveFromCollection,
        AddToWatchList,
        RemoveFromWatchList,
        AddToList,
        RemoveFromList,
        Related,
        Rate,
        Shouts,
        UserProfile,
        FollowUser,
        Like,
        Unlike,
        Cast,
        Crew,
        Trailers,
        Spoilers,
        SearchWithMpNZB,
        SearchTorrent
    }

    public enum MediaContextMenuItem
    {
        MarkAsWatched,
        MarkAsUnWatched,
        AddToWatchList,
        RemoveFromWatchList,
        Filters,
        AddToList,
        AddToLibrary,
        RemoveFromLibrary,
        Related,
        Rate,
        Shouts,
        Cast,
        Crew,
        ChangeLayout,
        Trailers,
        SearchWithMpNZB,
        SearchTorrent,
        ShowSeasonInfo
    }

    public enum TraktGUIControls 
    {
        Layout = 2,
        Facade = 50,
    }

    enum TraktGUIWindows
    {
        Main = 87258,
        Calendar = 87259,
        Recommendations = 87261,
        RecommendationsShows = 87262,
        RecommendationsMovies = 87263,
        Trending = 87264,
        TrendingShows = 87265,
        TrendingMovies = 87266,
        WatchedList = 87267,
        WatchedListShows = 87268,
        WatchedListEpisodes = 87269,
        WatchedListMovies = 87270,
        Settings = 87271,
        SettingsAccount = 87272,
        SettingsPlugins = 87273,
        SettingsGeneral = 87274,
        CustomLists = 87275,
        CustomListItems = 87276,
        RelatedMovies = 87277,
        RelatedShows = 87278,
        Shouts = 87280,
        ShowSeasons = 87281,
        SeasonEpisodes = 87282,
        Network = 87283,
        RecentWatchedMovies = 87284,
        RecentWatchedEpisodes = 87285,
        RecentAddedMovies = 87286,
        RecentAddedEpisodes = 87287,
        RecentShouts = 87288,
        UserProfile = 87400,
        Search = 874001,
        SearchEpisodes = 874002,
        SearchShows = 874003,
        SearchMovies = 874004,
        SearchPeople = 874005,
        SearchUsers = 874006,
        Popular = 87100,
        PopularMovies = 87101,
        PopularShows = 87102,
        TV = 87500,
        Movies= 87501,
        Lists = 87502,
        PersonSummary = 87600,
        PersonCreditMovies = 87601,
        PersonCreditShows = 87602,
        CreditsMovie = 87603,
        CreditsShow = 87604,
        AnticipatedMovies = 87605,
        AnticipatedShows = 87606,
        BoxOffice = 87607,
        //Alberto83 Added
        EPGShowSelect = 87608,
        GUIShowDialogSelect = 87609
    }

    enum TraktDashboardControls
    {
        DashboardAnimation = 98299,
        ActivityFacade = 98300,
        TrendingShowsFacade = 98301,
        TrendingMoviesFacade = 98302
    }

    enum ExternalPluginWindows
    {
        OnlineVideos = 4755,
        VideoInfo = 2003,
        MovingPictures = 96742,
        TVSeries = 9811,
        MyVideosDb = 25,
        MyVideosShares = 6,
        MyFilms = 7986,
        MyAnime = 6001,
        MpNZB = 3847,
        MPEISettings = 803,
        MyTorrents = 5678,
        Showtimes = 7111992,
        MPSkinSettings = 705
    }

    enum ExternalPluginControls
    {
        WatchList = 97258,
        Rate = 97259,
        Shouts = 97260,
        CustomList = 97261,
        RelatedItems = 97262,
        SearchBy = 97263,
        TraktMenu = 97270
    }

    enum TraktMenuItems
    {
        AddToWatchList,
        AddToCustomList,
        Rate,
        Cast,
        Crew,
        Shouts,
        Related,
        ShowSeasonInfo,
        UserProfile,
        Calendar,
        Network,
        Recommendations,
        Trending,
        Popular,
        WatchList,
        Anticipated,
        Lists,
        SearchBy
    }

    enum TraktSearchByItems
    {
        Actors,
        Directors,
        Producers,
        Writers,
        GuestStars
    }

    public enum SortingFields
    {
        Title,
        ReleaseDate,
        Score,
        Votes,
        Runtime,
        PeopleWatching,
        WatchListInserted,
        Popularity,
        Anticipated
    }

    public enum SortingDirections
    {
        Ascending,
        Descending
    }

    public enum Filters
    {
        Watched,
        Watchlisted,
        Collected,
        Rated
    }

    public enum Credit
    {
        Cast,
        Crew,
        Production,
        Art,
        CostumeAndMakeUp,
        Directing,
        Writing,
        Sound,
        Camera
    }

    public enum MediaType
    {
        Show,
        Movie
    }

    #endregion

    public class GUICommon
    {
        public static TraktMovieSummary CurrentMovie = null;
        public static TraktShowSummary CurrentShow = null;
        public static MediaType CurrentMediaType = MediaType.Movie;

        #region Check Login
        public static bool CheckLogin()
        {
            return CheckLogin(true);
        }

        /// <summary>
        /// Checks if user is logged in, if not the user is presented with
        /// a choice to jump to Account settings and signup/login.
        /// </summary>
        public static bool CheckLogin(bool showPreviousWindow)
        {
            if (TraktSettings.AccountStatus != ConnectionState.Connected)
            {
                if (GUIUtils.ShowYesNoDialog(Translation.Login, Translation.NotLoggedIn, true))
                {
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.SettingsAccount);
                    return false;
                }
                if (showPreviousWindow) GUIWindowManager.ShowPreviousWindow();
                return false;
            }
            return true;
        }
        #endregion

        #region Play Movie
        /// <summary>
        /// Checks if a selected movie exists locally and plays movie or
        /// jumps to corresponding plugin details view
        /// </summary>
        /// <param name="jumpTo">false if movie should be played directly</param>
        internal static void CheckAndPlayMovie(bool jumpTo, TraktMovieSummary movie)
        {
            if (movie == null) return;

            CurrentMediaType = MediaType.Movie;
            CurrentMovie = movie;

            // check for parental controls
            if (PromptForPinCode)
            {
                if (!GUIUtils.ShowPinCodeDialog(TraktSettings.ParentalControlsPinCode))
                {
                    TraktLogger.Warning("Parental controls pin code has not successfully been entered. Window ID = {0}", GUIWindowManager.ActiveWindow);
                    return;
                }
            }

            TraktLogger.Info("Attempting to play movie. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", movie.Title, movie.Year.ToLogString(), movie.Ids.Imdb.ToLogString());
            bool handled = false;

            if (TraktHelper.IsMovingPicturesAvailableAndEnabled)
            {
                TraktLogger.Info("Checking if any movie to watch in MovingPictures");
                int? movieid = null;

                // Find Movie ID in MovingPictures
                // Movie list is now cached internally in MovingPictures so it will be fast
                bool movieExists = TraktHandlers.MovingPictures.FindMovieID(movie.Title, movie.Year.GetValueOrDefault(), movie.Ids.Imdb, ref movieid);

                if (movieExists)
                {
                    TraktLogger.Info("Found movie in MovingPictures with movie ID '{0}'", movieid.ToString());
                    if (jumpTo)
                    {
                        string loadingParameter = string.Format("movieid:{0}", movieid);
                        // Open MovingPictures Details view so user can play movie
                        GUIWindowManager.ActivateWindow((int)ExternalPluginWindows.MovingPictures, loadingParameter);
                    }
                    else
                    {
                        TraktHandlers.MovingPictures.PlayMovie(movieid);
                    }
                    handled = true;
                }
            }

            // check if its in My Videos database
            if (TraktSettings.MyVideos >= 0 && handled == false)
            {
                TraktLogger.Info("Checking if any movie to watch in My Videos");
                IMDBMovie imdbMovie = null;
                if (TraktHandlers.MyVideos.FindMovieID(movie.Title, movie.Year.GetValueOrDefault(), movie.Ids.Imdb, ref imdbMovie))
                {
                    // Open My Videos Video Info view so user can play movie
                    if (jumpTo)
                    {
                        GUIVideoInfo videoInfo = (GUIVideoInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_INFO);
                        videoInfo.Movie = imdbMovie;
                        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_VIDEO_INFO);
                    }
                    else
                    {
                        GUIVideoFiles.PlayMovie(imdbMovie.ID, false);
                    }
                    handled = true;
                }
            }

            // check if its in My Films database
            if (TraktHelper.IsMyFilmsAvailableAndEnabled && handled == false)
            {
                TraktLogger.Info("Checking if any movie to watch in My Films");
                int? movieid = null;
                string config = null;
                if (TraktHandlers.MyFilmsHandler.FindMovie(movie.Title, movie.Year.GetValueOrDefault(), movie.Ids.Imdb, ref movieid, ref config))
                {
                    // Open My Films Details view so user can play movie
                    if (jumpTo)
                    {
                        string loadingParameter = string.Format("config:{0}|movieid:{1}", config, movieid);
                        GUIWindowManager.ActivateWindow((int)ExternalPluginWindows.MyFilms, loadingParameter);
                    }
                    else
                    {
                        // TraktHandlers.MyFilms.PlayMovie(config, movieid); // TODO: Add Player Class to MyFilms
                        string loadingParameter = string.Format("config:{0}|movieid:{1}|play:{2}", config, movieid, "true");
                        GUIWindowManager.ActivateWindow((int)ExternalPluginWindows.MyFilms, loadingParameter);
                    }
                    handled = true;
                }
            }

            if (TraktHelper.IsTrailersAvailableAndEnabled && handled == false)
            {
                TraktLogger.Info("There were no movies found in local plugin databases. Attempting to search and/or play trailer(s) from the Trailers plugin");
                ShowMovieTrailersPluginMenu(movie);
                handled = true;
            }            
        }
        #endregion

        #region Play Episode
        internal static void CheckAndPlayEpisode(TraktShowSummary show, TraktEpisodeSummary episode)
        {
            if (show == null || episode == null) return;

            CurrentMediaType = MediaType.Show;
            CurrentShow = show;

            // check for parental controls
            if (PromptForPinCode)
            {
                if (!GUIUtils.ShowPinCodeDialog(TraktSettings.ParentalControlsPinCode))
                {
                    TraktLogger.Warning("Parental controls pin code has not successfully been entered. Window ID = {0}", GUIWindowManager.ActiveWindow);
                    return;
                }
            }

            bool handled = false;

            // check if plugin is installed and enabled
            if (TraktHelper.IsMPTVSeriesAvailableAndEnabled)
            {
                // Play episode if it exists
                handled = TraktHandlers.TVSeries.PlayEpisode(show.Ids.Tvdb.GetValueOrDefault(), episode.Season, episode.Number);
            }

            if (TraktHelper.IsTrailersAvailableAndEnabled && handled == false)
            {
                TraktLogger.Info("There were no episodes found in local plugin databases. Attempting to search and/or play trailer(s) from the Trailers plugin");
                ShowTVEpisodeTrailersPluginMenu(show, episode);
                handled = true;
            }
        }
        
        internal static void CheckAndPlayFirstUnwatchedEpisode(TraktShowSummary show, bool jumpTo)
        {
            if (show == null) return;

            CurrentMediaType = MediaType.Show;
            CurrentShow = show;

            // check for parental controls
            if (PromptForPinCode)
            {
                if (!GUIUtils.ShowPinCodeDialog(TraktSettings.ParentalControlsPinCode))
                {
                    TraktLogger.Warning("Parental controls pin code has not successfully been entered. Window ID = {0}", GUIWindowManager.ActiveWindow);
                    return;
                }
            }

            TraktLogger.Info("Attempting to play episodes for tv show. TVDb ID = '{0}', IMDb ID = '{1}'", show.Ids.Tvdb.ToLogString(), show.Ids.Imdb.ToLogString());
            bool handled = false;

            // check if plugin is installed and enabled
            if (TraktHelper.IsMPTVSeriesAvailableAndEnabled)
            {
                if (jumpTo)
                {
                    TraktLogger.Info("Looking for tv shows in MP-TVSeries database");
                    if (TraktHandlers.TVSeries.SeriesExists(show.Ids.Tvdb.GetValueOrDefault()))
                    {
                        string loadingParameter = string.Format("seriesid:{0}", show.Ids.Tvdb.GetValueOrDefault());
                        GUIWindowManager.ActivateWindow((int)ExternalPluginWindows.TVSeries, loadingParameter);
                        handled = true;
                    }
                }
                else
                {
                    // Play episode if it exists
                    TraktLogger.Info("Checking if any episodes to watch in MP-TVSeries");
                    handled = TraktHandlers.TVSeries.PlayFirstUnwatchedEpisode(show.Ids.Tvdb.GetValueOrDefault());
                }
            }

            if (TraktHelper.IsTrailersAvailableAndEnabled && handled == false)
            {
                TraktLogger.Info("There were no episodes found in local plugin databases. Attempting to search and/or play trailer(s) from the Trailers plugin");
                ShowTVShowTrailersPluginMenu(show);
                handled = true;
            }
        }
        #endregion

        #region Rate Movie

        internal static bool RateMovie(TraktMovieSummary movie)
        {
            var rateObject = new TraktSyncMovieRated
            {
                Ids = new TraktMovieId
                { 
                    Trakt = movie.Ids.Trakt,
                    Imdb = movie.Ids.Imdb.ToNullIfEmpty(),
                    Tmdb = movie.Ids.Tmdb
                },
                Title = movie.Title,
                Year = movie.Year,
                RatedAt = DateTime.UtcNow.ToISO8601()
            };

            int? prevRating = movie.UserRating();
            int newRating = 0;

            newRating = GUIUtils.ShowRateDialog<TraktSyncMovieRated>(rateObject);
            if (newRating == -1) return false;

            // If previous rating not equal to current rating then 
            // update skin properties to reflect changes
            if (prevRating == newRating)
                return false;

            if (prevRating == null || prevRating == 0)
            {
                // add to ratings
                TraktCache.AddMovieToRatings(movie, newRating);
                movie.Votes++;
            }
            else if (newRating == 0)
            {
                // remove from ratings
                TraktCache.RemoveMovieFromRatings(movie);
                movie.Votes--;
            }
            else
            {
                // rating changed, remove then add
                TraktCache.RemoveMovieFromRatings(movie);
                TraktCache.AddMovieToRatings(movie, newRating);
            }

            // update ratings until next online update
            // if we have the ratings distribution we could calculate correctly
            if (movie.Votes == 0)
            {
                movie.Rating = 0;
            }
            else if (movie.Votes == 1 && newRating > 0)
            {
                movie.Rating = newRating;
            }

            return true;
        }

        #endregion

        #region Rate Show

        internal static bool RateShow(TraktShowSummary show)
        {
            var rateObject = new TraktSyncShowRated
            {
                Ids = new TraktShowId
                {
                    Trakt = show.Ids.Trakt,
                    Imdb = show.Ids.Imdb.ToNullIfEmpty(),
                    Tmdb = show.Ids.Tmdb,
                    TvRage = show.Ids.TvRage,
                    Tvdb = show.Ids.Tvdb
                },
                Title = show.Title,
                Year = show.Year,
                RatedAt = DateTime.UtcNow.ToISO8601()
            };

            int? prevRating = show.UserRating();
            int newRating = 0;

            newRating = GUIUtils.ShowRateDialog<TraktSyncShowRated>(rateObject);
            if (newRating == -1) return false;

            // If previous rating not equal to current rating then 
            // update skin properties to reflect changes
            if (prevRating == newRating)
                return false;

            if (prevRating == null || prevRating == 0)
            {
                // add to ratings
                TraktCache.AddShowToRatings(show, newRating);
                show.Votes++;
            }
            else if (newRating == 0)
            {
                // remove from ratings
                TraktCache.RemoveShowFromRatings(show);
                show.Votes--;
            }
            else
            {
                // rating changed, remove then add
                TraktCache.RemoveShowFromRatings(show);
                TraktCache.AddShowToRatings(show, newRating);
            }

            // update ratings until next online update
            // if we have the ratings distribution we could calculate correctly
            if (show.Votes == 0)
            {
                show.Rating = 0;
            }
            else if (show.Votes == 1 && newRating > 0)
            {
                show.Rating = newRating;
            }

            return true;
        }

        #endregion

        #region Rate Season

        internal static bool RateSeason(TraktShowSummary show, TraktSeasonSummary season)
        {
            var rateObject = new TraktSyncSeasonRatedEx
            {
                Ids = show.Ids,
                Title = show.Title,
                Year = show.Year,
                Seasons = new List<TraktSyncSeasonRatedEx.Season>
                {
                    new TraktSyncSeasonRatedEx.Season
                    {
                        Number = season.Number,
                        RatedAt = DateTime.UtcNow.ToISO8601()
                    }
                }
            };

            int? prevRating = season.UserRating(show);
            int newRating = 0;

            newRating = GUIUtils.ShowRateDialog<TraktSyncSeasonRatedEx>(rateObject);
            if (newRating == -1) return false;

            // If previous rating not equal to current rating then 
            // update skin properties to reflect changes
            if (prevRating == newRating)
                return false;

            if (prevRating == null || prevRating == 0)
            {
                // add to ratings
                TraktCache.AddSeasonToRatings(show, season, newRating);
                season.Votes++;
            }
            else if (newRating == 0)
            {
                // remove from ratings
                TraktCache.RemoveSeasonFromRatings(show, season);
                season.Votes--;
            }
            else
            {
                // rating changed, remove then add
                TraktCache.RemoveSeasonFromRatings(show, season);
                TraktCache.AddSeasonToRatings(show, season, newRating);
            }

            // update ratings until next online update
            // if we have the ratings distribution we could calculate correctly
            if (season.Votes == 0)
            {
                season.Rating = 0;
            }
            else if (season.Votes == 1 && newRating > 0)
            {
                season.Rating = newRating;
            }

            return true;
        }

        #endregion

        #region Rate Episode

        internal static bool RateEpisode(TraktShowSummary show, TraktEpisodeSummary episode)
        {
            // this object will work without episode ids
            var rateObjectEx = new TraktSyncShowRatedEx
            {
                Ids = show.Ids,
                Title = show.Title,
                Year = show.Year,
                Seasons = new List<TraktSyncShowRatedEx.Season>
                {
                    new TraktSyncShowRatedEx.Season
                    {
                        Number = episode.Season,
                        Episodes = new List<TraktSyncShowRatedEx.Season.Episode>
                        {
                            new TraktSyncShowRatedEx.Season.Episode
                            {
                                Number = episode.Number,
                                RatedAt = DateTime.UtcNow.ToISO8601()
                            }
                        }
                    }
                }
            };

            // only use if we have episode ids
            var rateObject = new TraktSyncEpisodeRated
            {
                Ids = new TraktEpisodeId
                {
                    Trakt = episode.Ids.Trakt,
                    Imdb = episode.Ids.Imdb.ToNullIfEmpty(),
                    Tmdb = episode.Ids.Tmdb,
                    Tvdb = episode.Ids.Tvdb,
                    TvRage = episode.Ids.TvRage
                },
                Title = episode.Title,
                Season = episode.Season,
                Number = episode.Number,
                RatedAt = DateTime.UtcNow.ToISO8601()
            };

            int? prevRating = episode.UserRating(show);
            int newRating = 0;

            if (episode.Ids == null || episode.Ids.Trakt == null)
            {
                newRating = GUIUtils.ShowRateDialog<TraktSyncShowRatedEx>(rateObjectEx);
            }
            else
            {
                newRating = GUIUtils.ShowRateDialog<TraktSyncEpisodeRated>(rateObject);
            }

            if (newRating == -1) return false;

            // If previous rating not equal to current rating then 
            // update skin properties to reflect changes
            if (prevRating == newRating)
                return false;

            if (prevRating == null || prevRating == 0)
            {
                // add to ratings
                TraktCache.AddEpisodeToRatings(show, episode, newRating);
                episode.Votes++;
            }
            else if (newRating == 0)
            {
                // remove from ratings
                TraktCache.RemoveEpisodeFromRatings(episode);
                episode.Votes--;
            }
            else
            {
                // rating changed, remove then add
                TraktCache.RemoveEpisodeFromRatings(episode);
                TraktCache.AddEpisodeToRatings(show, episode, newRating);
            }

            // update ratings until next online update
            // if we have the ratings distribution we could calculate correctly
            if (episode.Votes == 0)
            {
                episode.Rating = 0;
            }
            else if (episode.Votes == 1 && newRating > 0)
            {
                episode.Rating = newRating;
            }

            return true;
        }

        #endregion

        #region Mark all Episodes in Show as Watched

        public static void MarkShowAsWatched(TraktShow show)
        {
            if (!GUICommon.CheckLogin(false)) return;

            var seenThread = new Thread(obj =>
            {
                var objShow = obj as TraktShow;

                var syncData = new TraktShow
                {
                    Ids = new TraktShowId
                    {
                        Trakt = objShow.Ids.Trakt,
                        Imdb = objShow.Ids.Imdb.ToNullIfEmpty(),
                        Tmdb = objShow.Ids.Tmdb,
                        Tvdb = objShow.Ids.Tvdb,
                        TvRage = objShow.Ids.TvRage
                    },
                    Title = show.Title,
                    Year = show.Year
                };

                TraktLogger.Info("Adding all episodes from show to trakt.tv watched history. Title = '{0}', Year = '{1}', IMDb ID = '{2}', TVDb ID = '{3}', TMDb ID = '{4}'", 
                                    show.Title, show.Year.ToLogString(), show.Ids.Imdb.ToLogString(), show.Ids.Tvdb.ToLogString(), show.Ids.Tmdb.ToLogString());

                var response = TraktAPI.TraktAPI.AddShowToWatchedHistory(syncData);
                TraktLogger.LogTraktResponse(response);
            })
            {
                IsBackground = true,
                Name = "MarkWatched"
            };

            seenThread.Start(show);
        }

        #endregion

        #region Mark all Episodes in a Shows Season as Watched

        public static void MarkSeasonAsWatched(TraktShow show, int season)
        {
            if (!GUICommon.CheckLogin(false)) return;

            var seenThread = new Thread(obj =>
            {
                var objShow = obj as TraktShow;

                var syncData = new TraktSyncShowEx
                {
                    Ids = new TraktShowId
                    {
                        Trakt = objShow.Ids.Trakt,
                        Imdb = objShow.Ids.Imdb.ToNullIfEmpty(),
                        Tmdb = objShow.Ids.Tmdb,
                        Tvdb = objShow.Ids.Tvdb,
                        TvRage = objShow.Ids.TvRage
                    },
                    Title = show.Title,
                    Year = show.Year,
                    Seasons = new List<TraktSyncShowEx.Season>()
                };

                var seasonObj = new TraktSyncShowEx.Season
                {
                    Number = season
                };
                syncData.Seasons.Add(seasonObj);

                TraktLogger.Info("Adding all episodes in season from show to trakt.tv watched history. Title = '{0}', Year = '{1}', IMDb ID = '{2}', TVDb ID = '{3}', TMDb ID = '{4}', Season = '{5}'", 
                                    show.Title, show.Year.ToLogString(), show.Ids.Imdb.ToLogString(), show.Ids.Tvdb.ToLogString(), show.Ids.Tmdb.ToLogString(), season);

                var response = TraktAPI.TraktAPI.AddShowToWatchedHistoryEx(syncData);
                TraktLogger.LogTraktResponse(response);
            })
            {
                IsBackground = true,
                Name = "MarkWatched"
            };

            seenThread.Start(show);
        }

        #endregion

        #region Add Show to Library

        public static void AddShowToCollection(TraktShow show)
        {
            if (!GUICommon.CheckLogin(false)) return;

            var collectionThread = new Thread(obj =>
            {
                var objShow = obj as TraktShow;

                var syncData = new TraktShow
                {
                    Ids = new TraktShowId
                    {
                        Trakt = objShow.Ids.Trakt,
                        Imdb = objShow.Ids.Imdb.ToNullIfEmpty(),
                        Tmdb = objShow.Ids.Tmdb,
                        Tvdb = objShow.Ids.Tvdb,
                        TvRage = objShow.Ids.TvRage
                    },
                    Title = show.Title,
                    Year = show.Year
                };

                TraktLogger.Info("Adding all episodes from show to trakt.tv collection. Title = '{0}', Year = '{1}', IMDb ID = '{2}', TVDb ID = '{3}', TMDb ID = '{4}'",
                                    show.Title, show.Year.ToLogString(), show.Ids.Imdb.ToLogString(), show.Ids.Tvdb.ToLogString(), show.Ids.Tmdb.ToLogString());

                var response = TraktAPI.TraktAPI.AddShowToCollection(syncData);
                TraktLogger.LogTraktResponse(response);
            })
            {
                IsBackground = true,
                Name = "AddCollection"
            };

            collectionThread.Start(show);
        }

        #endregion

        #region Add Season to Library

        public static void AddSeasonToLibrary(TraktShow show, int season)
        {
            if (!GUICommon.CheckLogin(false)) return;

            var seenThread = new Thread(obj =>
            {
                var objShow = obj as TraktShow;

                var syncData = new TraktSyncShowEx
                {
                    Ids = new TraktShowId
                    {
                        Trakt = objShow.Ids.Trakt,
                        Imdb = objShow.Ids.Imdb.ToNullIfEmpty(),
                        Tmdb = objShow.Ids.Tmdb,
                        Tvdb = objShow.Ids.Tvdb,
                        TvRage = objShow.Ids.TvRage
                    },
                    Title = show.Title,
                    Year = show.Year,
                    Seasons = new List<TraktSyncShowEx.Season>()
                };

                var seasonObj = new TraktSyncShowEx.Season
                {
                    Number = season
                };
                syncData.Seasons.Add(seasonObj);

                TraktLogger.Info("Adding all episodes in season from show to trakt.tv collection. Title = '{0}', Year = '{1}', IMDb ID = '{2}', TVDb ID = '{3}', TMDb ID = '{4}', Season = '{5}'",
                                    show.Title, show.Year.ToLogString(), show.Ids.Imdb.ToLogString(), show.Ids.Tvdb.ToLogString(), show.Ids.Tmdb.ToLogString(), season);

                var response = TraktAPI.TraktAPI.AddShowToCollectionEx(syncData);
                TraktLogger.LogTraktResponse(response);
            })
            {
                IsBackground = true,
                Name = "AddCollection"
            };

            seenThread.Start(show);
        }

        #endregion

        #region Likes

        public static void LikeComment(TraktComment comment)
        {
            var likeThread = new Thread((obj) =>
            {
                // add like to cache
                TraktCache.AddCommentToLikes((TraktComment)comment);

                TraktAPI.TraktAPI.LikeComment(((TraktComment)comment).Id);
            })
            {
                Name = "LikeComment",
                IsBackground = true
            };

            likeThread.Start(comment);
        }

        public static void UnLikeComment(TraktComment comment)
        {
            var unlikeThread = new Thread((obj) =>
            {
                // remove like from cache

                TraktCache.RemoveCommentFromLikes((TraktComment)comment);
                TraktAPI.TraktAPI.UnLikeComment(((TraktComment)comment).Id);
            })
            {
                Name = "LikeComment",
                IsBackground = true
            };

            unlikeThread.Start(comment);
        }

        public static void LikeList(TraktListDetail list, string username)
        {
            var likeThread = new Thread((obj) =>
            {
                // all list to likes cache
                TraktCache.AddListToLikes((TraktListDetail)obj);

                TraktAPI.TraktAPI.LikeList(username, ((TraktListDetail)obj).Ids.Trakt.Value);
            })
            {
                Name = "LikeList",
                IsBackground = true
            };

            likeThread.Start(list);
        }

        public static void UnLikeList(TraktListDetail list, string username)
        {
            var unlikeThread = new Thread((obj) =>
            {
                // remove list from likes cache
                TraktCache.RemoveListFromLikes((TraktListDetail)obj);

                TraktAPI.TraktAPI.UnLikeList(username, ((TraktListDetail)obj).Ids.Trakt.Value);
            })
            {
                Name = "UnLikeList",
                IsBackground = true
            };

            unlikeThread.Start(list);
        }

        #endregion

        #region Common Skin Properties

        internal static string GetProperty(string property)
        {
            string propertyVal = GUIPropertyManager.GetProperty(property);
            return propertyVal ?? string.Empty;
        }

        internal static void SetProperty(string property, string value)
        {
            string propertyValue = string.IsNullOrEmpty(value) ? "--" : value;
            GUIUtils.SetProperty(property, propertyValue);

            //TraktLogger.Debug("Property: {0}, Value = {1}", property, value);
        }

        internal static void SetProperty(string property, List<string> value)
        {
            string propertyValue = value == null ? "--" : string.Join(", ", value.ToArray());
            GUIUtils.SetProperty(property, propertyValue);
        }

        internal static void SetProperty(string property, int? value)
        {
            string propertyValue = value == null ? "--" : value.ToString();
            GUIUtils.SetProperty(property, propertyValue);
        }

        internal static void SetProperty(string property, bool value)
        {
            GUIUtils.SetProperty(property, value.ToString().ToLower());
        }

        internal static void ClearUserProperties()
        {
            GUIUtils.SetProperty("#Trakt.User.About", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.Age", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.Avatar", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.AvatarFileName", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.FullName", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.Gender", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.JoinDate", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.ApprovedDate", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.Location", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.Protected", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.Url", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.Username", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.VIP", string.Empty);
            GUIUtils.SetProperty("#Trakt.User.VIP_EP", string.Empty);
        }
        
        internal static void SetUserProperties(TraktUserSummary user)
        {
            if (user == null)
                return;

            SetProperty("#Trakt.User.Username", user.Username);
            SetProperty("#Trakt.User.Protected", user.IsPrivate.ToString().ToLower());
            SetProperty("#Trakt.User.VIP", user.IsVip.ToString().ToLower());
            SetProperty("#Trakt.User.VIP_EP", user.IsVipEP.ToString().ToLower());
            SetProperty("#Trakt.User.About", user.About.RemapHighOrderChars());
            SetProperty("#Trakt.User.Age", user.Age.ToString());
            SetProperty("#Trakt.User.FullName", user.FullName);
            SetProperty("#Trakt.User.Gender", string.IsNullOrEmpty(user.Gender) ? null : Translation.GetByName(string.Format("Gender{0}", user.Gender)));
            SetProperty("#Trakt.User.JoinDate", user.JoinedAt.FromISO8601().ToLongDateString());
            SetProperty("#Trakt.User.Location", user.Location);            
            SetProperty("#Trakt.User.Url", string.Format("http://trakt.tv/users/{0}", user.Username));
            if (user.Images != null)
            {
                SetProperty("#Trakt.User.Avatar", user.Images.Avatar.FullSize);
                SetProperty("#Trakt.User.AvatarFileName", user.Images.Avatar.LocalImageFilename(ArtworkType.Avatar));
            }
        }

        internal static void ClearListProperties()
        {
            GUIUtils.SetProperty("#Trakt.List.Name", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.Description", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.Privacy", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.Slug", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.Url", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.AllowShouts", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.ShowNumbers", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.UpdatedAt", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.ItemCount", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.Likes", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.Comments", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.Id", string.Empty);
            GUIUtils.SetProperty("#Trakt.List.Slug", string.Empty);
        }

        internal static void SetListProperties(TraktListDetail list, string username)
        {
            SetProperty("#Trakt.List.Name", list.Name);
            SetProperty("#Trakt.List.Description", list.Description);
            SetProperty("#Trakt.List.Privacy", list.Privacy);
            SetProperty("#Trakt.List.Slug", list.Ids.Slug);
            SetProperty("#Trakt.List.Url", string.Format("http://trakt.tv/users/{0}/lists/{1}", username, list.Ids.Trakt));
            SetProperty("#Trakt.List.AllowShouts", list.AllowComments);
            SetProperty("#Trakt.List.ShowNumbers", list.DisplayNumbers);
            SetProperty("#Trakt.List.UpdatedAt", list.UpdatedAt.FromISO8601().ToShortDateString());
            SetProperty("#Trakt.List.ItemCount", list.ItemCount);
            SetProperty("#Trakt.List.Comments", list.Comments);
            SetProperty("#Trakt.List.Likes", list.Likes);
            SetProperty("#Trakt.List.Id", list.Ids.Trakt);
            SetProperty("#Trakt.List.Slug", list.Ids.Slug);
        }

        internal static void ClearStatisticProperties()
        {
            #region Friends Statistics
            GUIUtils.SetProperty("#Trakt.Statistics.Friends", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Followers", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Following", string.Empty);
            #endregion

            #region Shows Statistics
            GUIUtils.SetProperty("#Trakt.Statistics.Shows.Library", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Shows.Watched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Shows.Collection", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Shows.Shouts", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Shows.Loved", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Shows.Hated", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Shows.Ratings", string.Empty);
            #endregion

            #region Episodes Statistics
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Checkins", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.CheckinsUnique", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Collection", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Ratings", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Hated", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Loved", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Scrobbles", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.ScrobblesUnique", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Seen", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Shouts", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.UnWatched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Plays", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.Watched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.WatchedElseWhere", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.WatchedTrakt", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.WatchedTraktUnique", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Episodes.WatchedUnique", string.Empty);
            #endregion

            #region Movies Statistics
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Checkins", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.CheckinsUnique", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Collection", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Ratings", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Hated", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Library", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Loved", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Scrobbles", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.ScrobblesUnique", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Seen", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Shouts", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.UnWatched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Plays", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.Watched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.WatchedElseWhere", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.WatchedTrakt", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.WatchedTraktUnique", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Movies.WatchedUnique", string.Empty);
            #endregion

            #region Ratings Statistics

            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Total", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.1", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.2", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.3", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.4", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.5", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.6", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.7", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.8", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.9", string.Empty);
            GUIUtils.SetProperty("#Trakt.Statistics.Ratings.Distribution.10", string.Empty);

            #endregion
        }

        internal static void SetStatisticProperties(TraktUserStatistics stats, string Username)
        {
            if (stats == null) return;

            // TODO
            // Can't work out the ratings loved/hated for other users, only current

            #region Network Statistics
            if (stats.Network != null)
            {
                SetProperty("#Trakt.Statistics.Friends", stats.Network.Friends);
                SetProperty("#Trakt.Statistics.Followers", stats.Network.Followers);
                SetProperty("#Trakt.Statistics.Following", stats.Network.Following);
            }
            #endregion

            #region Shows Statistics
            if (stats.Shows != null)
            {
                SetProperty("#Trakt.Statistics.Shows.Library", stats.Shows.Collected);
                SetProperty("#Trakt.Statistics.Shows.Collection", stats.Shows.Collected);
                SetProperty("#Trakt.Statistics.Shows.Watched", stats.Shows.Watched);
                SetProperty("#Trakt.Statistics.Shows.Shouts", stats.Shows.Comments);
                SetProperty("#Trakt.Statistics.Shows.Loved", Username == TraktSettings.Username ? TraktCache.StatsShowsLoved() : stats.Shows.Ratings);
                SetProperty("#Trakt.Statistics.Shows.Hated", Username == TraktSettings.Username ? TraktCache.StatsShowsHated() : 0);
                SetProperty("#Trakt.Statistics.Shows.Ratings", stats.Shows.Ratings);
            }
            #endregion

            #region Episodes Statistics
            if (stats.Episodes != null)
            {
                //SetProperty("#Trakt.Statistics.Episodes.Checkins", stats.Episodes.Checkins);
                //SetProperty("#Trakt.Statistics.Episodes.CheckinsUnique", stats.Episodes.CheckinsUnique);
                SetProperty("#Trakt.Statistics.Episodes.Library", stats.Episodes.Collected);
                SetProperty("#Trakt.Statistics.Episodes.Collection", stats.Episodes.Collected);
                SetProperty("#Trakt.Statistics.Episodes.Loved", Username == TraktSettings.Username ? TraktCache.StatsEpisodesLoved() : stats.Episodes.Ratings);
                SetProperty("#Trakt.Statistics.Episodes.Hated", Username == TraktSettings.Username ? TraktCache.StatsEpisodesHated() : 0);
                SetProperty("#Trakt.Statistics.Episodes.Ratings", stats.Episodes.Ratings);
                //SetProperty("#Trakt.Statistics.Episodes.Scrobbles", stats.Episodes.Scrobbles);
                //SetProperty("#Trakt.Statistics.Episodes.ScrobblesUnique", stats.Episodes.ScrobblesUnique);
                //SetProperty("#Trakt.Statistics.Episodes.Seen", stats.Episodes.Seen);
                SetProperty("#Trakt.Statistics.Episodes.Shouts", stats.Episodes.Comments);
                //SetProperty("#Trakt.Statistics.Episodes.UnWatched", stats.Episodes.UnWatched);
                SetProperty("#Trakt.Statistics.Episodes.Plays", stats.Episodes.Plays);
                SetProperty("#Trakt.Statistics.Episodes.Watched", stats.Episodes.Plays);
                //SetProperty("#Trakt.Statistics.Episodes.WatchedElseWhere", stats.Episodes.WatchedElseWhere);
                //SetProperty("#Trakt.Statistics.Episodes.WatchedTrakt", stats.Episodes.WatchedTrakt);
                //SetProperty("#Trakt.Statistics.Episodes.WatchedTraktUnique", stats.Episodes.WatchedTraktUnique);
                SetProperty("#Trakt.Statistics.Episodes.WatchedUnique", stats.Episodes.Watched);
            }
            #endregion

            #region Movies Statistics
            if (stats.Movies != null)
            {
                //SetProperty("#Trakt.Statistics.Movies.Checkins", stats.Movies.Checkins);
                //SetProperty("#Trakt.Statistics.Movies.CheckinsUnique", stats.Movies.CheckinsUnique);
                SetProperty("#Trakt.Statistics.Movies.Collection", stats.Movies.Collected);
                SetProperty("#Trakt.Statistics.Movies.Library", stats.Movies.Collected);
                SetProperty("#Trakt.Statistics.Movies.Ratings", stats.Movies.Ratings);
                SetProperty("#Trakt.Statistics.Movies.Loved", Username == TraktSettings.Username ? TraktCache.StatsMoviesLoved() : stats.Movies.Ratings);
                SetProperty("#Trakt.Statistics.Movies.Hated", Username == TraktSettings.Username ? TraktCache.StatsMoviesHated() : 0);
                //SetProperty("#Trakt.Statistics.Movies.Scrobbles", stats.Movies.Scrobbles);
                //SetProperty("#Trakt.Statistics.Movies.ScrobblesUnique", stats.Movies.ScrobblesUnique);
                //SetProperty("#Trakt.Statistics.Movies.Seen", stats.Movies.Seen);
                SetProperty("#Trakt.Statistics.Movies.Shouts", stats.Movies.Comments);
                //SetProperty("#Trakt.Statistics.Movies.UnWatched", stats.Movies.UnWatched);
                SetProperty("#Trakt.Statistics.Movies.Plays", stats.Movies.Plays);
                SetProperty("#Trakt.Statistics.Movies.Watched", stats.Movies.Plays);
                //SetProperty("#Trakt.Statistics.Movies.WatchedElseWhere", stats.Movies.WatchedElseWhere);
                //SetProperty("#Trakt.Statistics.Movies.WatchedTrakt", stats.Movies.WatchedTrakt);
                //SetProperty("#Trakt.Statistics.Movies.WatchedTraktUnique", stats.Movies.WatchedTraktUnique);
                SetProperty("#Trakt.Statistics.Movies.WatchedUnique", stats.Movies.Watched);
            }
            #endregion

            #region Ratings Statistics
            if (stats.Ratings != null)
            {
                SetProperty("#Trakt.Statistics.Ratings.Total", stats.Ratings.Total);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.1", stats.Ratings.Distribution.One);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.2", stats.Ratings.Distribution.Two);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.3", stats.Ratings.Distribution.Three);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.4", stats.Ratings.Distribution.Four);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.5", stats.Ratings.Distribution.Five);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.6", stats.Ratings.Distribution.Six);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.7", stats.Ratings.Distribution.Seven);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.8", stats.Ratings.Distribution.Eight);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.9", stats.Ratings.Distribution.Nine);
                SetProperty("#Trakt.Statistics.Ratings.Distribution.10", stats.Ratings.Distribution.Ten);
            }
            #endregion
        }

        internal static void ClearCommentProperties()
        {
            GUIUtils.SetProperty("#Trakt.Shout.Id", string.Empty);
            GUIUtils.SetProperty("#Trakt.Shout.Date", string.Empty);
            GUIUtils.SetProperty("#Trakt.Shout.Inserted", string.Empty);
            GUIUtils.SetProperty("#Trakt.Shout.Spoiler", "false");
            GUIUtils.SetProperty("#Trakt.Shout.Review", "false");
            GUIUtils.SetProperty("#Trakt.Shout.Text", string.Empty);
            GUIUtils.SetProperty("#Trakt.Shout.UserRating", string.Empty);
            GUIUtils.SetProperty("#Trakt.Shout.Type", string.Empty);            
            GUIUtils.SetProperty("#Trakt.Shout.Likes", string.Empty);
            GUIUtils.SetProperty("#Trakt.Shout.Replies", string.Empty);
        }

        internal static void SetCommentProperties(TraktComment comment, bool isWatched = false)
        {
            SetProperty("#Trakt.Shout.Id", comment.Id);
            SetProperty("#Trakt.Shout.Inserted", comment.CreatedAt.FromISO8601().ToLongDateString());
            SetProperty("#Trakt.Shout.Date", comment.CreatedAt.FromISO8601().ToShortDateString());
            SetProperty("#Trakt.Shout.Spoiler", comment.IsSpoiler);
            SetProperty("#Trakt.Shout.Review", comment.IsReview);
            SetProperty("#Trakt.Shout.Type", comment.IsReview ? "review" : "shout");
            SetProperty("#Trakt.Shout.Likes", comment.Likes);
            SetProperty("#Trakt.Shout.Replies", comment.Replies);
            SetProperty("#Trakt.Shout.UserRating", comment.UserRating);

            // don't hide spoilers if watched
            if (TraktSettings.HideSpoilersOnShouts && comment.IsSpoiler && !isWatched)
                SetProperty("#Trakt.Shout.Text", Translation.HiddenToPreventSpoilers);
            else
                SetProperty("#Trakt.Shout.Text", System.Web.HttpUtility.HtmlDecode(comment.Text.RemapHighOrderChars()).StripHTML());
        }

        internal static void ClearMovieProperties()
        {
            GUIUtils.SetProperty("#Trakt.Movie.Id", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Tmdb", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Slug", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Imdb", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Certification", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Language", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Overview", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Released", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Runtime", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Tagline", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Title", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Trailer", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Url", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Year", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Genres", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.PosterImageFilename", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.FanartImageFilename", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.InCollection", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.InWatchList", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Plays", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Watched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Rating", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Ratings.Icon", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Ratings.Percentage", string.Empty);
            GUIUtils.SetProperty("#Trakt.Movie.Ratings.Votes", string.Empty);
        }

        internal static void SetMovieProperties(TraktMovieSummary movie)
        {
            if (movie == null) return;

            SetProperty("#Trakt.Movie.Id", movie.Ids.Trakt);
            SetProperty("#Trakt.Movie.ImdbId", movie.Ids.Imdb);
            SetProperty("#Trakt.Movie.TmdbId", movie.Ids.Tmdb);
            SetProperty("#Trakt.Movie.Slug", movie.Ids.Slug);
            SetProperty("#Trakt.Movie.Certification", movie.Certification);
            SetProperty("#Trakt.Movie.Overview", movie.Overview.ToNullIfEmpty() == null ? Translation.NoMovieSummary : movie.Overview.RemapHighOrderChars());
            SetProperty("#Trakt.Movie.Released", movie.Released);
            SetProperty("#Trakt.Movie.Language", Translation.GetLanguageFromISOCode(movie.Language));
            SetProperty("#Trakt.Movie.Runtime", movie.Runtime);
            SetProperty("#Trakt.Movie.Tagline", movie.Tagline);
            SetProperty("#Trakt.Movie.Title", movie.Title.RemapHighOrderChars());
            SetProperty("#Trakt.Movie.Trailer", movie.Trailer);
            SetProperty("#Trakt.Movie.Url", string.Format("http://trakt.tv/movies/{0}", movie.Ids.Slug));
            SetProperty("#Trakt.Movie.Year", movie.Year);
            SetProperty("#Trakt.Movie.Genres", TraktGenres.Translate(movie.Genres));
            SetProperty("#Trakt.Movie.InCollection", movie.IsCollected());
            SetProperty("#Trakt.Movie.InWatchList", movie.IsWatchlisted());
            SetProperty("#Trakt.Movie.Plays", movie.Plays());
            SetProperty("#Trakt.Movie.Watched", movie.IsWatched());
            SetProperty("#Trakt.Movie.Rating", movie.UserRating());
            SetProperty("#Trakt.Movie.Ratings.Percentage", movie.Rating.ToPercentage());
            SetProperty("#Trakt.Movie.Ratings.Votes", movie.Votes);
            SetProperty("#Trakt.Movie.Ratings.Icon", (movie.Rating >= 6) ? "love" : "hate");
        }

        internal static void ClearSeasonProperties()
        {
            GUIUtils.SetProperty("#Trakt.Season.TmdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.TvdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.TvRageId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Number", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.EpisodeCount", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.EpisodeAiredCount", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Watched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.InCollection", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.InWatchList", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Collected", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Plays", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Rating", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Ratings.Icon", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Ratings.Percentage", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Ratings.Votes", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.Url", string.Empty);
            GUIUtils.SetProperty("#Trakt.Season.PosterImageFilename", string.Empty);
        }

        internal static void SetSeasonProperties(TraktShowSummary show, TraktSeasonSummary season)
        {
            SetProperty("#Trakt.Season.TmdbId", season.Ids.Tmdb);
            SetProperty("#Trakt.Season.TvdbId", season.Ids.Tvdb);
            SetProperty("#Trakt.Season.TvRageId", season.Ids.TvRage);
            SetProperty("#Trakt.Season.Number", season.Number);            
            SetProperty("#Trakt.Season.Url", string.Format("http://trakt.tv/shows/{0}/seasons/{1}", show.Ids.Slug, season.Number));
            //SetProperty("#Trakt.Season.PosterImageFilename", season.Images == null ? string.Empty : season.Images.Poster.LocalImageFilename(ArtworkType.SeasonPoster));
            SetProperty("#Trakt.Season.EpisodeCount", season.EpisodeCount);
            SetProperty("#Trakt.Season.EpisodeAiredCount", season.EpisodeAiredCount);
            SetProperty("#Trakt.Season.Overview", season.Overview ?? show.Overview);
            SetProperty("#Trakt.Season.Watched", season.IsWatched(show));
            SetProperty("#Trakt.Season.Plays", season.Plays(show));
            SetProperty("#Trakt.Season.InCollection", season.IsCollected(show));
            SetProperty("#Trakt.Season.InWatchList", season.IsWatchlisted(show));
            SetProperty("#Trakt.Season.Collected", season.Collected(show));
            SetProperty("#Trakt.Season.Rating", season.UserRating(show));
            SetProperty("#Trakt.Season.Ratings.Percentage", season.Rating.ToPercentage());
            SetProperty("#Trakt.Season.Ratings.Votes", season.Votes);
            SetProperty("#Trakt.Season.Ratings.Icon", (season.Rating >= 6) ? "love" : "hate");
        }

        internal static void ClearShowProperties()
        {
            GUIUtils.SetProperty("#Trakt.Show.Id", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.ImdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.TvdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.TmdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.TvRageId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Title", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Language", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Url", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.AirDay", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.AirDayLocalized", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.AirTime", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.AirTimeLocalized", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.AirTimezone", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.AirTimezoneWindows", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Certification", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Country", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.FirstAired", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.FirstAiredLocalized", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Network", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Overview", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Runtime", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Year", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Status", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Genres", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.InWatchList", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.InCollection", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Collected", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Watched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.AiredEpisodes", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Plays", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Rating", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Ratings.Icon", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Ratings.Percentage", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.Ratings.Votes", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.FanartImageFilename", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.PosterImageFilename", string.Empty);
            GUIUtils.SetProperty("#Trakt.Show.BannerImageFilename", string.Empty);
        }

        internal static void SetShowProperties(TraktShowSummary show)
        {
            if (show == null) return;

            SetProperty("#Trakt.Show.Id", show.Ids.Trakt);
            SetProperty("#Trakt.Show.ImdbId", show.Ids.Imdb);
            SetProperty("#Trakt.Show.TvdbId", show.Ids.Tvdb);
            SetProperty("#Trakt.Show.TmdbId", show.Ids.Tmdb);
            SetProperty("#Trakt.Show.TvRageId", show.Ids.TvRage);
            SetProperty("#Trakt.Show.Title", show.Title.RemapHighOrderChars());
            SetProperty("#Trakt.Show.Language", Translation.GetLanguageFromISOCode(show.Language));
            SetProperty("#Trakt.Show.Url", string.Format("http://trakt.tv/shows/{0}", show.Ids.Slug));
            if (show.Airs != null)
            {
                SetProperty("#Trakt.Show.AirDay", show.FirstAired.FromISO8601().ToLocalisedDayOfWeek());
                SetProperty("#Trakt.Show.AirDayLocalized", show.FirstAired.FromISO8601().ToLocalTime().ToLocalisedDayOfWeek());
                SetProperty("#Trakt.Show.AirTime", show.FirstAired.FromISO8601().ToShortTimeString());
                SetProperty("#Trakt.Show.AirTimeLocalized", show.FirstAired.FromISO8601().ToLocalTime().ToShortTimeString());
                SetProperty("#Trakt.Show.AirTimezone", show.Airs.Timezone);
                SetProperty("#Trakt.Show.AirTimezoneWindows", show.Airs.Timezone.OlsenToWindowsTimezone());
            }
            SetProperty("#Trakt.Show.Certification", show.Certification);
            SetProperty("#Trakt.Show.Country", show.Country.ToCountryName());
            SetProperty("#Trakt.Show.FirstAired", show.FirstAired.FromISO8601().ToShortDateString());
            SetProperty("#Trakt.Show.FirstAiredLocalized", show.FirstAired.FromISO8601().ToLocalTime().ToShortDateString());
            SetProperty("#Trakt.Show.Network", show.Network);
            SetProperty("#Trakt.Show.Overview", show.Overview.ToNullIfEmpty() == null ? Translation.NoShowSummary : show.Overview.RemapHighOrderChars());
            SetProperty("#Trakt.Show.Runtime", show.Runtime);
            SetProperty("#Trakt.Show.Year", show.Year);
            SetProperty("#Trakt.Show.Status", show.Status);
            SetProperty("#Trakt.Show.TranslatedStatus", (show.Status ?? "").Replace(" " ,"").Translate());
            SetProperty("#Trakt.Show.Genres", TraktGenres.Translate(show.Genres));
            SetProperty("#Trakt.Show.InWatchList", show.IsWatchlisted());
            SetProperty("#Trakt.Show.InCollection", show.IsCollected());
            SetProperty("#Trakt.Show.Collected", show.Collected());
            SetProperty("#Trakt.Show.Watched", show.IsWatched());
            SetProperty("#Trakt.Show.AiredEpisodes", show.AiredEpisodes);
            SetProperty("#Trakt.Show.Plays", show.Plays());
            SetProperty("#Trakt.Show.Rating", show.UserRating());
            SetProperty("#Trakt.Show.Ratings.Percentage", show.Rating.ToPercentage());
            SetProperty("#Trakt.Show.Ratings.Votes", show.Votes);
            SetProperty("#Trakt.Show.Ratings.Icon", (show.Rating > 6) ? "love" : "hate");
            //if (show.Images != null)
            //{
            //    SetProperty("#Trakt.Show.FanartImageFilename", show.Images.Fanart.LocalImageFilename(ArtworkType.ShowFanart));
            //    SetProperty("#Trakt.Show.PosterImageFilename", show.Images.Poster.LocalImageFilename(ArtworkType.ShowPoster));
            //    SetProperty("#Trakt.Show.BannerImageFilename", show.Images.Banner.LocalImageFilename(ArtworkType.ShowBanner));
            //}
        }

        internal static void ClearEpisodeProperties()
        {
            GUIUtils.SetProperty("#Trakt.Episode.Id", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.TvdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.ImdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.TmdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Number", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Season", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.FirstAired", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.FirstAiredLocalized", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.FirstAiredLocalizedDayOfWeek", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.FirstAiredLocalizedTime", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Title", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Url", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Overview", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Runtime", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.InWatchList", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.InCollection", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Plays", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Watched", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Rating", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Ratings.Icon", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Ratings.HatedCount", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Ratings.LovedCount", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Ratings.Percentage", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.Ratings.Votes", string.Empty);
            GUIUtils.SetProperty("#Trakt.Episode.EpisodeImageFilename", string.Empty);
        }

        internal static void SetEpisodeProperties(TraktShowSummary show, TraktEpisodeSummary episode)
        {
            if (episode == null) return;

            SetProperty("#Trakt.Episode.Id", episode.Ids.Trakt);
            SetProperty("#Trakt.Episode.TvdbId", episode.Ids.Tvdb);
            SetProperty("#Trakt.Episode.ImdbId", episode.Ids.Imdb);
            SetProperty("#Trakt.Episode.TmdbId", episode.Ids.Imdb);
            SetProperty("#Trakt.Episode.Number", episode.Number);
            SetProperty("#Trakt.Episode.Season", episode.Season);
            if (episode.FirstAired != null)
            {
                // FirstAired is converted to UTC from original countries timezone on trakt
                SetProperty("#Trakt.Episode.FirstAired", episode.FirstAired.FromISO8601().ToShortDateString());
                SetProperty("#Trakt.Episode.FirstAiredLocalized", episode.FirstAired.FromISO8601().ToLocalTime().ToShortDateString());
                SetProperty("#Trakt.Episode.FirstAiredLocalizedDayOfWeek", episode.FirstAired.FromISO8601().ToLocalTime().ToLocalisedDayOfWeek());
                SetProperty("#Trakt.Episode.FirstAiredLocalizedTime", episode.FirstAired.FromISO8601().ToLocalTime().ToShortTimeString());
            }
            SetProperty("#Trakt.Episode.Title", string.IsNullOrEmpty(episode.Title) ? string.Format("{0} {1}", Translation.Episode, episode.Number.ToString()) : episode.Title.RemapHighOrderChars());
            SetProperty("#Trakt.Episode.Url", string.Format("http://trakt.tv/shows/{0}/seasons/{1}/episodes/{2}", show.Ids.Slug, episode.Season, episode.Number));
            SetProperty("#Trakt.Episode.Overview", episode.Overview.ToNullIfEmpty() == null ? Translation.NoEpisodeSummary : episode.Overview.RemapHighOrderChars());
            SetProperty("#Trakt.Episode.Runtime", show.Runtime);
            SetProperty("#Trakt.Episode.InWatchList", episode.IsWatchlisted());
            SetProperty("#Trakt.Episode.InCollection", episode.IsCollected(show));
            SetProperty("#Trakt.Episode.Plays", episode.Plays(show));
            SetProperty("#Trakt.Episode.Watched", episode.IsWatched(show));
            SetProperty("#Trakt.Episode.Rating", episode.UserRating(show));
            SetProperty("#Trakt.Episode.Ratings.Percentage", episode.Rating.ToPercentage());
            SetProperty("#Trakt.Episode.Ratings.Votes", episode.Votes);
            SetProperty("#Trakt.Episode.Ratings.Icon", (episode.Rating >= 6) ? "love" : "hate");
            //if (episode.Images != null)
            //{
            //    SetProperty("#Trakt.Episode.EpisodeImageFilename", episode.Images.ScreenShot.LocalImageFilename(ArtworkType.EpisodeImage));
            //}
        }

        internal static void ClearPersonProperties()
        {
            GUIUtils.SetProperty("#Trakt.Person.Id", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.TmdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.ImdbId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.TvRageId", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.Name", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.HeadshotUrl", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.HeadshotFilename", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.FanartUrl", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.FanartFilename", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.Url", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.Biography", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.Birthday", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.Birthplace", string.Empty);
            GUIUtils.SetProperty("#Trakt.Person.Death", string.Empty);
        }
        
        internal static void SetPersonProperties(TraktPersonSummary person)
        {
            SetProperty("#Trakt.Person.Id", person.Ids.Trakt);
            SetProperty("#Trakt.Person.ImdbId", person.Ids.ImdbId);
            SetProperty("#Trakt.Person.TmdbId", person.Ids.TmdbId);
            SetProperty("#Trakt.Person.TvRageId", person.Ids.TvRageId);
            SetProperty("#Trakt.Person.Name", person.Name);
            //if (person.Images != null)
            //{
            //    SetProperty("#Trakt.Person.HeadshotUrl", person.Images.HeadShot.FullSize);
            //    SetProperty("#Trakt.Person.HeadshotFilename", person.Images.HeadShot.LocalImageFilename(ArtworkType.PersonHeadshot));
            //    if (person.Images.Fanart != null && System.IO.File.Exists(person.Images.Fanart.LocalImageFilename(ArtworkType.PersonFanart)))
            //    {
            //        SetProperty("#Trakt.Person.FanartUrl", person.Images.Fanart.FullSize);
            //        SetProperty("#Trakt.Person.FanartFilename", person.Images.Fanart.LocalImageFilename(ArtworkType.PersonFanart));
            //    }
            //}
            SetProperty("#Trakt.Person.Url", string.Format("http://trakt.tv/people/{0}", person.Ids.Slug));
            SetProperty("#Trakt.Person.Biography", person.Biography ?? Translation.NoPersonBiography.RemapHighOrderChars());
            SetProperty("#Trakt.Person.Birthday", person.Birthday);
            SetProperty("#Trakt.Person.Birthplace", person.Birthplace);
            SetProperty("#Trakt.Person.Death", person.Death);
        }

        #endregion

        #region GUI Context Menus

        #region Activity

        /// <summary>
        /// Returns a list of context menu items for a selected item in the Activity Dashboard
        /// </summary>
        internal static List<GUIListItem> GetContextMenuItemsForActivity(TraktActivity.Activity activity)
        {
            GUIListItem listItem = null;
            var listItems = new List<GUIListItem>();

            // Add Watchlist
            if (!activity.IsWatchlisted())
            {
                listItem = new GUIListItem(Translation.AddToWatchList);
                listItem.ItemId = (int)ActivityContextMenuItem.AddToWatchList;
                listItems.Add(listItem);
            }
            else if (activity.Type != ActivityType.list.ToString())
            {
                listItem = new GUIListItem(Translation.RemoveFromWatchList);
                listItem.ItemId = (int)ActivityContextMenuItem.RemoveFromWatchList;
                listItems.Add(listItem);
            }

            // Mark As Watched
            if (activity.Type == ActivityType.episode.ToString() || activity.Type == ActivityType.movie.ToString())
            {
                if (!activity.IsWatched())
                {
                    listItem = new GUIListItem(Translation.MarkAsWatched);
                    listItem.ItemId = (int)ActivityContextMenuItem.MarkAsWatched;
                    listItems.Add(listItem);
                }
                else
                {
                    listItem = new GUIListItem(Translation.MarkAsUnWatched);
                    listItem.ItemId = (int)ActivityContextMenuItem.MarkAsUnwatched;
                    listItems.Add(listItem);
                }
            }

            // Add To Collection
            if (activity.Type == ActivityType.episode.ToString() || activity.Type == ActivityType.movie.ToString())
            {
                if (!activity.IsCollected())
                {
                    listItem = new GUIListItem(Translation.AddToLibrary);
                    listItem.ItemId = (int)ActivityContextMenuItem.AddToCollection;
                    listItems.Add(listItem);
                }
                else
                {
                    listItem = new GUIListItem(Translation.RemoveFromLibrary);
                    listItem.ItemId = (int)ActivityContextMenuItem.RemoveFromCollection;
                    listItems.Add(listItem);
                }
            }

            // Add to Custom list
            listItem = new GUIListItem(Translation.AddToList);
            listItem.ItemId = (int)ActivityContextMenuItem.AddToList;
            listItems.Add(listItem);

            // Shouts
            listItem = new GUIListItem(Translation.Comments);
            listItem.ItemId = (int)ActivityContextMenuItem.Shouts;
            listItems.Add(listItem);

            // Rate
            listItem = new GUIListItem(Translation.Rate + "...");
            listItem.ItemId = (int)ActivityContextMenuItem.Rate;
            listItems.Add(listItem);

            // Cast and Crew
            listItem = new GUIListItem(Translation.Cast);
            listItem.ItemId = (int)ActivityContextMenuItem.Cast;
            listItems.Add(listItem);

            listItem = new GUIListItem(Translation.Crew);
            listItem.ItemId = (int)ActivityContextMenuItem.Crew;
            listItems.Add(listItem);

            // Trailers
            if (TraktHelper.IsTrailersAvailableAndEnabled)
            {
                listItem = new GUIListItem(Translation.Trailers);
                listItem.ItemId = (int)ActivityContextMenuItem.Trailers;
                listItems.Add(listItem);
            }

            return listItems;
        }

        internal static void CreateMoviesContextMenu(ref IDialogbox dlg, TraktMovie movie, bool dashboard)
        {
            GUIListItem listItem = null;

            // Mark As Watched
            if (!movie.IsWatched())
            {
                listItem = new GUIListItem(Translation.MarkAsWatched);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.MarkAsWatched;
            }

            // Mark As UnWatched
            if (movie.IsWatched())
            {
                listItem = new GUIListItem(Translation.MarkAsUnWatched);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.MarkAsUnWatched;
            }

            // Add/Remove Watchlist
            if (!movie.IsWatchlisted())
            {
                listItem = new GUIListItem(Translation.AddToWatchList);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.AddToWatchList;
            }
            else
            {
                listItem = new GUIListItem(Translation.RemoveFromWatchList);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.RemoveFromWatchList;
            }

            // Add to Custom list
            listItem = new GUIListItem(Translation.AddToList);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.AddToList;

            // Add to Library
            // Don't allow if it will be removed again on next sync
            // movie could be part of a DVD collection
            if (!movie.IsCollected() && !TraktSettings.KeepTraktLibraryClean)
            {
                listItem = new GUIListItem(Translation.AddToLibrary);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.AddToLibrary;
            }

            if (movie.IsCollected())
            {
                listItem = new GUIListItem(Translation.RemoveFromLibrary);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.RemoveFromLibrary;
            }

            // Filters
            if (TraktSettings.FilterTrendingOnDashboard || !dashboard)
            {
                listItem = new GUIListItem(Translation.Filters + "...");
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.Filters;
            }

            // Rate Movie
            listItem = new GUIListItem(Translation.RateMovie);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Rate;

            // Related Movies
            listItem = new GUIListItem(Translation.RelatedMovies);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Related;

            // Shouts
            listItem = new GUIListItem(Translation.Comments);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Shouts;

            // Cast & Crew
            listItem = new GUIListItem(Translation.Cast);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Cast;

            listItem = new GUIListItem(Translation.Crew);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Crew;

            // Trailers
            if (TraktHelper.IsTrailersAvailableAndEnabled)
            {
                listItem = new GUIListItem(Translation.Trailers);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.Trailers;
            }

            // Change Layout
            if (!dashboard)
            {
                listItem = new GUIListItem(Translation.ChangeLayout);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.ChangeLayout;
            }

            if (!movie.IsCollected() && TraktHelper.IsMpNZBAvailableAndEnabled)
            {
                // Search for movie with mpNZB
                listItem = new GUIListItem(Translation.SearchWithMpNZB);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.SearchWithMpNZB;
            }

            if (!movie.IsCollected() && TraktHelper.IsMyTorrentsAvailableAndEnabled)
            {
                // Search for movie with MyTorrents
                listItem = new GUIListItem(Translation.SearchTorrent);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.SearchTorrent;
            }

        }

        internal static void CreateShowsContextMenu(ref IDialogbox dlg, TraktShow show, bool dashboard)
        {
            GUIListItem listItem = null;

            // Add/Remove Watchlist            
            if (!show.IsWatchlisted())
            {
                listItem = new GUIListItem(Translation.AddToWatchList);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.AddToWatchList;
            }
            else
            {
                listItem = new GUIListItem(Translation.RemoveFromWatchList);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.RemoveFromWatchList;
            }

            // Show Season Information
            listItem = new GUIListItem(Translation.ShowSeasonInfo);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.ShowSeasonInfo;

            // Mark Show as Watched
            listItem = new GUIListItem(Translation.MarkAsWatched);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.MarkAsWatched;

            // Add Show to Library
            listItem = new GUIListItem(Translation.AddToLibrary);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.AddToLibrary;

            // Add to Custom List
            listItem = new GUIListItem(Translation.AddToList);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.AddToList;

            if (TraktHelper.IsTrailersAvailableAndEnabled)
            {
                listItem = new GUIListItem(Translation.Trailers);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.Trailers;
            }

            // Filters
            if (TraktSettings.FilterTrendingOnDashboard || !dashboard)
            {
                listItem = new GUIListItem(Translation.Filters + "...");
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.Filters;
            }

            // Related Shows
            listItem = new GUIListItem(Translation.RelatedShows);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Related;

            // Rate Show
            listItem = new GUIListItem(Translation.RateShow);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Rate;

            // Comments
            listItem = new GUIListItem(Translation.Comments);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Shouts;

            // Cast & Crew
            listItem = new GUIListItem(Translation.Cast);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Cast;

            listItem = new GUIListItem(Translation.Crew);
            dlg.Add(listItem);
            listItem.ItemId = (int)MediaContextMenuItem.Crew;

            // Change Layout
            if (!dashboard)
            {
                listItem = new GUIListItem(Translation.ChangeLayout);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.ChangeLayout;
            }

            if (TraktHelper.IsMpNZBAvailableAndEnabled)
            {
                // Search for show with mpNZB
                listItem = new GUIListItem(Translation.SearchWithMpNZB);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.SearchWithMpNZB;
            }

            if (TraktHelper.IsMyTorrentsAvailableAndEnabled)
            {
                // Search for show with MyTorrents
                listItem = new GUIListItem(Translation.SearchTorrent);
                dlg.Add(listItem);
                listItem.ItemId = (int)MediaContextMenuItem.SearchTorrent;
            }
        }

        #endregion

        #region Layout
        internal static Layout ShowLayoutMenu(Layout currentLayout, int itemToSelect)
        {
            Layout newLayout = currentLayout;

            IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(GetLayoutTranslation(currentLayout));

            foreach (Layout layout in Enum.GetValues(typeof(Layout)))
            {
                string menuItem = GetLayoutTranslation(layout);
                GUIListItem pItem = new GUIListItem(menuItem);
                if (layout == currentLayout) pItem.Selected = true;
                dlg.Add(pItem);
            }

            dlg.DoModal(GUIWindowManager.ActiveWindow);

            if (dlg.SelectedLabel >= 0)
            {
                var facade = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).GetControl((int)TraktGUIControls.Facade) as GUIFacadeControl;

                newLayout = (Layout)dlg.SelectedLabel;
                facade.SetCurrentLayout(Enum.GetName(typeof(Layout), newLayout));
                GUIControl.SetControlLabel(GUIWindowManager.ActiveWindow, (int)TraktGUIControls.Layout, GetLayoutTranslation(newLayout));
                // when loosing focus from the facade the current selected index is lost
                // e.g. changing layout from skin side menu
                facade.SelectIndex(itemToSelect);
            }
            return newLayout;
        }

        internal static string GetLayoutTranslation(Layout layout)
        {
            bool mp12 = TraktSettings.MPVersion <= new Version(1, 2, 0, 0);

            string strLine = string.Empty;
            switch (layout)
            {
                case Layout.List:
                    strLine = GUILocalizeStrings.Get(101);
                    break;
                case Layout.SmallIcons:
                    strLine = GUILocalizeStrings.Get(100);
                    break;
                case Layout.LargeIcons:
                    strLine = GUILocalizeStrings.Get(417);
                    break;
                case Layout.Filmstrip:
                    strLine = GUILocalizeStrings.Get(733);
                    break;
            }
            return mp12 ? strLine : GUILocalizeStrings.Get(95) + strLine;
        }
        #endregion

        #region SortBy

        internal static SortBy ShowSortMenu(SortBy currentSortBy)
        {
            var newSortBy = new SortBy();

            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null) return null;

            dlg.Reset();
            dlg.SetHeading(495); // Sort options

            // Add generic sortby fields
            GUIListItem pItem = new GUIListItem(Translation.Title);
            dlg.Add(pItem);
            pItem.ItemId = (int)SortingFields.Title;

            pItem = new GUIListItem(Translation.ReleaseDate);
            dlg.Add(pItem);
            pItem.ItemId = (int)SortingFields.ReleaseDate;

            pItem = new GUIListItem(Translation.Score);
            dlg.Add(pItem);
            pItem.ItemId = (int)SortingFields.Score;

            pItem = new GUIListItem(Translation.Votes);
            dlg.Add(pItem);
            pItem.ItemId = (int)SortingFields.Votes;

            pItem = new GUIListItem(Translation.Popularity);
            dlg.Add(pItem);
            pItem.ItemId = (int)SortingFields.Popularity;

            pItem = new GUIListItem(Translation.Runtime);
            dlg.Add(pItem);
            pItem.ItemId = (int)SortingFields.Runtime;

            // Trending
            if (GUIWindowManager.ActiveWindow == (int)TraktGUIWindows.TrendingMovies || 
                GUIWindowManager.ActiveWindow == (int)TraktGUIWindows.TrendingShows) {
                pItem = new GUIListItem(Translation.Watchers);
                dlg.Add(pItem);
                pItem.ItemId = (int)SortingFields.PeopleWatching;
            }

            // Watchlist
            if (GUIWindowManager.ActiveWindow == (int)TraktGUIWindows.WatchedListMovies || 
                GUIWindowManager.ActiveWindow == (int)TraktGUIWindows.WatchedListShows) {
                pItem = new GUIListItem(Translation.Inserted);
                dlg.Add(pItem);
                pItem.ItemId = (int)SortingFields.WatchListInserted;
            }

            // Anticipated
            if (GUIWindowManager.ActiveWindow == (int)TraktGUIWindows.AnticipatedMovies ||
                GUIWindowManager.ActiveWindow == (int)TraktGUIWindows.AnticipatedShows)
            {
                pItem = new GUIListItem(Translation.Anticipated);
                dlg.Add(pItem);
                pItem.ItemId = (int)SortingFields.Anticipated;
            }

            // set the focus to currently used sort method
            dlg.SelectedLabel = (int)currentSortBy.Field;

            // show dialog and wait for result
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            if (dlg.SelectedId == -1) return null;
            
            switch (dlg.SelectedId)
            {
                case (int)SortingFields.Title:
                    newSortBy.Field = SortingFields.Title;
                    break;

                case (int)SortingFields.ReleaseDate:
                    newSortBy.Field = SortingFields.ReleaseDate;
                    newSortBy.Direction = SortingDirections.Descending;
                    break;

                case (int)SortingFields.Score:
                    newSortBy.Field = SortingFields.Score;
                    newSortBy.Direction = SortingDirections.Descending;
                    break;

                case (int)SortingFields.Votes:
                    newSortBy.Field = SortingFields.Votes;
                    newSortBy.Direction = SortingDirections.Descending;
                    break;

                case (int)SortingFields.Popularity:
                    newSortBy.Field = SortingFields.Popularity;
                    newSortBy.Direction = SortingDirections.Descending;
                    break;

                case (int)SortingFields.Runtime:
                    newSortBy.Field = SortingFields.Runtime;
                    break;

                case (int)SortingFields.PeopleWatching:
                    newSortBy.Field = SortingFields.PeopleWatching;
                    newSortBy.Direction = SortingDirections.Descending;
                    break;

                case (int)SortingFields.WatchListInserted:
                    newSortBy.Direction = SortingDirections.Descending;
                    newSortBy.Field = SortingFields.WatchListInserted;
                    break;

                case (int)SortingFields.Anticipated:
                    newSortBy.Direction = SortingDirections.Descending;
                    newSortBy.Field = SortingFields.Anticipated;
                    break;

                default:
                    newSortBy.Field = SortingFields.Title;
                    break;
            }

            return newSortBy;
        }

        internal static string GetSortByString(SortBy currentSortBy)
        {
            string strLine = string.Empty;

            switch (currentSortBy.Field)
            {
                case SortingFields.Title:
                    strLine = Translation.Title;
                    break;

                case SortingFields.ReleaseDate:
                    strLine = Translation.ReleaseDate;
                    break;

                case SortingFields.Score:
                    strLine = Translation.Score;
                    break;

                case SortingFields.Votes:
                    strLine = Translation.Votes;
                    break;

                case SortingFields.Popularity:
                    strLine = Translation.Popularity;
                    break;

                case SortingFields.Runtime:
                    strLine = Translation.Runtime;
                    break;

                case SortingFields.PeopleWatching:
                    strLine = Translation.Watchers;
                    break;

                case SortingFields.WatchListInserted:
                    strLine = Translation.Inserted;
                    break;

                case SortingFields.Anticipated:
                    strLine = Translation.Anticipated;
                    break;
                
                default:
                    strLine = Translation.Title;
                    break;
            }

            return string.Format(Translation.SortBy, strLine);
        }

        #endregion

        #region Movie Trailers
        public static void ShowMovieTrailersPluginMenu(TraktMovieSummary movie)
        {
            var images = TmdbCache.GetMovieImages(movie.Ids.Tmdb, true);

            var trailerItem = new MediaItem
            {
                IMDb = movie.Ids.Imdb.ToNullIfEmpty(),
                TMDb = movie.Ids.Tmdb.ToString(),
                Plot = movie.Overview,
                Poster = TmdbCache.GetMoviePosterFilename(images),
                Title = movie.Title,
                Year = movie.Year.GetValueOrDefault(0)
            };
            Trailers.Trailers.SearchForTrailers(trailerItem);
        }

        public static void ShowMovieTrailersMenu(TraktMovieSummary movie)
        {
            if (TraktHelper.IsTrailersAvailableAndEnabled)
            {

                CurrentMediaType = MediaType.Movie;
                CurrentMovie = movie;

                // check for parental controls
                if (PromptForPinCode)
                {
                    if (!GUIUtils.ShowPinCodeDialog(TraktSettings.ParentalControlsPinCode))
                    {
                        TraktLogger.Warning("Parental controls pin code has not successfully been entered. Window ID = {0}", GUIWindowManager.ActiveWindow);
                        return;
                    }
                }

                ShowMovieTrailersPluginMenu(movie);
                return;
            }
        }

        #endregion

        #region TV Show Trailers
        public static void ShowTVShowTrailersPluginMenu(TraktShowSummary show)
        {
            var showImages = TmdbCache.GetShowImages(show.Ids.Tmdb, true);
            var trailerItem = new MediaItem
            {
                MediaType = MediaItemType.Show,
                IMDb = show.Ids.Imdb.ToNullIfEmpty(),
                TVDb = show.Ids.Tvdb.ToString(),
                TVRage = show.Ids.TvRage.ToString(),
                TMDb = show.Ids.Tmdb.ToString(),
                Plot = show.Overview,
                Poster = TmdbCache.GetShowPosterFilename(showImages),
                Title = show.Title,                
                Year = show.Year.GetValueOrDefault(0),
                AirDate = show.FirstAired.FromISO8601().ToString("yyyy-MM-dd")
            };
            Trailers.Trailers.SearchForTrailers(trailerItem);
        }

        public static void ShowTVShowTrailersMenu(TraktShowSummary show, TraktEpisodeSummary episode = null)
        {
            if (TraktHelper.IsTrailersAvailableAndEnabled)
            {
                CurrentMediaType = MediaType.Show;
                CurrentShow = show;

                // check for parental controls
                if (PromptForPinCode)
                {
                    if (!GUIUtils.ShowPinCodeDialog(TraktSettings.ParentalControlsPinCode))
                    {
                        TraktLogger.Warning("Parental controls pin code has not successfully been entered. Window ID = {0}", GUIWindowManager.ActiveWindow);
                        return;
                    }
                }

                if (episode == null)
                    ShowTVShowTrailersPluginMenu(show);
                else
                    ShowTVEpisodeTrailersPluginMenu(show, episode);

                return;
            }
        }
        #endregion

        #region TV Season Trailers
        public static void ShowTVSeasonTrailersPluginMenu(TraktShowSummary show, int season)
        {
            CurrentMediaType = MediaType.Show;
            CurrentShow = show;

            // check for parental controls
            if (PromptForPinCode)
            {
                if (!GUIUtils.ShowPinCodeDialog(TraktSettings.ParentalControlsPinCode))
                {
                    TraktLogger.Warning("Parental controls pin code has not successfully been entered. Window ID = {0}", GUIWindowManager.ActiveWindow);
                    return;
                }
            }

            var showImages = TmdbCache.GetShowImages(show.Ids.Tmdb, true);
            var trailerItem = new MediaItem
            {
                MediaType = MediaItemType.Season,
                IMDb = show.Ids.Imdb.ToNullIfEmpty(),
                TMDb = show.Ids.Tmdb.ToString(),
                TVDb = show.Ids.Tvdb.ToString(),
                TVRage = show.Ids.TvRage.ToString(),
                Plot = show.Overview,
                Poster = TmdbCache.GetShowPosterFilename(showImages),
                Title = show.Title,
                Year = show.Year.GetValueOrDefault(0),
                AirDate = show.FirstAired.FromISO8601().ToString("yyyy-MM-dd"),
                Season = season
            };
            Trailers.Trailers.SearchForTrailers(trailerItem);
        }
        #endregion

        #region TV Episode Trailers
        public static void ShowTVEpisodeTrailersPluginMenu(TraktShowSummary show, TraktEpisodeSummary episode)
        {
            var showImages = TmdbCache.GetShowImages(show.Ids.Tmdb, true);
            var trailerItem = new MediaItem
            {
                MediaType = MediaItemType.Episode,
                IMDb = show.Ids.Imdb.ToNullIfEmpty(),
                TMDb = show.Ids.Tmdb.ToString(),
                TVDb = show.Ids.Tvdb.ToString(),
                TVRage = show.Ids.TvRage.ToString(),
                Plot = show.Overview,
                Poster = TmdbCache.GetShowPosterFilename(showImages),
                Title = show.Title,
                Year = show.Year.GetValueOrDefault(0),
                AirDate = show.FirstAired.FromISO8601().ToString("yyyy-MM-dd"),
                Season = episode.Season,
                Episode = episode.Number,
                EpisodeName = episode.Title ?? string.Empty
            };
            Trailers.Trailers.SearchForTrailers(trailerItem);
        }
        #endregion

        #region Trakt External Menu

        #region SearchBy Menu
        public static bool ShowSearchByMenu(SearchPeople people, string title, string fanart)
        {
            IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(Translation.SearchBy);

            GUIListItem pItem = null;

            if (people.Actors.Count > 0)
            {
                pItem = new GUIListItem(Translation.Actors);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktSearchByItems.Actors;
                pItem.Label2 = people.Actors.Count.ToString();
            }

            if (people.Directors.Count > 0)
            {
                pItem = new GUIListItem(Translation.Directors);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktSearchByItems.Directors;
                pItem.Label2 = people.Directors.Count.ToString();
            }

            if (people.Producers.Count > 0)
            {
                pItem = new GUIListItem(Translation.Producers);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktSearchByItems.Producers;
                pItem.Label2 = people.Producers.Count.ToString();
            }

            if (people.Writers.Count > 0)
            {
                pItem = new GUIListItem(Translation.Writers);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktSearchByItems.Writers;
                pItem.Label2 = people.Writers.Count.ToString();
            }

            if (people.GuestStars.Count > 0)
            {
                pItem = new GUIListItem(Translation.GuestStars);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktSearchByItems.GuestStars;
                pItem.Label2 = people.GuestStars.Count.ToString();
            }

            // Show Context Menu
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            if (dlg.SelectedId < 0) return false;

            bool retCode = false;

            if (dlg.SelectedLabelText == Translation.Actors)
                retCode = ShowSearchByPersonMenu(people.Actors, title, fanart);
            if (dlg.SelectedLabelText == Translation.Directors)
                retCode = ShowSearchByPersonMenu(people.Directors, title, fanart);
            if (dlg.SelectedLabelText == Translation.Producers)
                retCode = ShowSearchByPersonMenu(people.Producers, title, fanart);
            if (dlg.SelectedLabelText == Translation.Writers)
                retCode = ShowSearchByPersonMenu(people.Writers, title, fanart);
            if (dlg.SelectedLabelText == Translation.GuestStars)
                retCode = ShowSearchByPersonMenu(people.GuestStars, title, fanart);

            return retCode;
        }

        public static bool ShowSearchByPersonMenu(List<string> people, string title, string fanart)
        {
            var dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(Translation.SearchBy);

            GUIListItem pItem = null;
            int itemId = 0;

            pItem = new GUIListItem(Translation.SearchAll);
            dlg.Add(pItem);
            pItem.ItemId = itemId++;

            foreach (var person in people)
            {
                pItem = new GUIListItem(person);
                dlg.Add(pItem);
                pItem.ItemId = itemId++;
            }

            // Show Context Menu
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            if (dlg.SelectedId < 0) return false;

            // Trigger Search
            // If Search By 'All', the parse along list of all people
            if (dlg.SelectedLabelText == Translation.SearchAll)
            {
                var peopleInItem = new PersonSearch { People = people, Title = title, Fanart = fanart };
                GUIWindowManager.ActivateWindow((int)TraktGUIWindows.SearchPeople, peopleInItem.ToJSON());
            }
            else
            {
                GUIWindowManager.ActivateWindow((int)TraktGUIWindows.PersonSummary, dlg.SelectedLabelText);
            }

            return true;
        }

        #endregion

        #region Movies
        public static bool ShowTraktExtMovieMenu(string title, string year, string imdbid, string fanart)
        {
            return ShowTraktExtMovieMenu(title, year, imdbid, fanart, false);
        }
        public static bool ShowTraktExtMovieMenu(string title, string year, string imdbid, string fanart, bool showAll)
        {
            return ShowTraktExtMovieMenu(title, year, imdbid, fanart, null, showAll);
        }
        public static bool ShowTraktExtMovieMenu(string title, string year, string imdbid, string fanart, SearchPeople people, bool showAll)
        {
            return ShowTraktExtMovieMenu(title, year, imdbid, false, fanart, people, showAll);
        }
        public static bool ShowTraktExtMovieMenu(string title, string year, string imdbid, bool isWatched, string fanart, SearchPeople people, bool showAll)
        {
            var dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(GUIUtils.PluginName());

            GUIListItem pItem = new GUIListItem(Translation.Comments);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Shouts;

            pItem = new GUIListItem(Translation.Rate);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Rate;

            pItem = new GUIListItem(Translation.RelatedMovies);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Related;

            pItem = new GUIListItem(Translation.AddToWatchList);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.AddToWatchList;

            pItem = new GUIListItem(Translation.AddToList);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.AddToCustomList;

            pItem = new GUIListItem(Translation.Cast);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Cast;

            pItem = new GUIListItem(Translation.Crew);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Crew;

            // Show Search By...
            if (people != null && people.Count != 0)
            {
                pItem = new GUIListItem(Translation.SearchBy + "...");
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.SearchBy;
            }

            // also show non-context sensitive items related to movies
            if (showAll)
            {
                // might want to check your recently watched, stats etc
                pItem = new GUIListItem(Translation.UserProfile);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.UserProfile;

                pItem = new GUIListItem(Translation.Network);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Network;

                pItem = new GUIListItem(Translation.Recommendations);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Recommendations;

                pItem = new GUIListItem(Translation.Trending);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Trending;

                pItem = new GUIListItem(Translation.Popular);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Popular;
                
                pItem = new GUIListItem(Translation.Anticipated);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Anticipated;

                pItem = new GUIListItem(Translation.WatchList);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.WatchList;

                pItem = new GUIListItem(Translation.Lists);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Lists;
            }

            // Show Context Menu
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            if (dlg.SelectedId < 0) return false;

            switch (dlg.SelectedId)
            {
                case ((int)TraktMenuItems.Rate):
                    TraktLogger.Info("Displaying rate dialog for movie. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    var movie = new TraktSyncMovieRated
                        {
                            Ids = new TraktMovieId { Imdb = imdbid.ToNullIfEmpty() },
                            Title = title,
                            Year = year.ToNullableInt32()
                        };

                    int rating = GUIUtils.ShowRateDialog<TraktSyncMovieRated>(movie);
                    
                    // update local databases
                    if (rating >= 0)
                    {
                        switch (GUIWindowManager.ActiveWindow)
                        {
                            case (int)ExternalPluginWindows.MovingPictures:
                                TraktHandlers.MovingPictures.SetUserRating(rating);
                                break;
                            case (int)ExternalPluginWindows.MyFilms:
                                TraktHandlers.MyFilmsHandler.SetUserRating(rating, title, year.ToNullableInt32(), imdbid.ToNullIfEmpty());
                                break;
                        }

                        if (rating == 0)
                            TraktCache.RemoveMovieFromRatings(movie);
                        else
                            TraktCache.AddMovieToRatings(movie, rating);
                    }
                    break;

                case ((int)TraktMenuItems.Shouts):
                    TraktLogger.Info("Displaying Shouts for movie. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    TraktHelper.ShowMovieShouts(imdbid, title, year, fanart);
                    break;

                case ((int)TraktMenuItems.Related):
                    TraktLogger.Info("Displaying Related Movies for. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    TraktHelper.ShowRelatedMovies(title, year, imdbid);
                    break;

                case ((int)TraktMenuItems.AddToWatchList):
                    TraktLogger.Info("Adding movie to Watchlist. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    TraktHelper.AddMovieToWatchList(title, year, imdbid, true);
                    break;

                case ((int)TraktMenuItems.AddToCustomList):
                    TraktLogger.Info("Adding movie to Custom List. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    TraktHelper.AddRemoveMovieInUserList(title, year, imdbid, false);
                    break;

                case ((int)TraktMenuItems.Cast):
                    TraktLogger.Info("Displaying Cast for movie. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    GUICreditsMovie.Movie = null;
                    GUICreditsMovie.Type = GUICreditsMovie.CreditType.Cast;
                    GUICreditsMovie.Fanart = fanart;
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.CreditsMovie, imdbid);
                    break;

                    case ((int)TraktMenuItems.Crew):
                    TraktLogger.Info("Displaying Crew for movie. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    GUICreditsMovie.Movie = null;
                    GUICreditsMovie.Type = GUICreditsMovie.CreditType.Crew;
                    GUICreditsMovie.Fanart = fanart;
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.CreditsMovie, imdbid);
                    break;

                case ((int)TraktMenuItems.SearchBy):
                    ShowSearchByMenu(people, title, fanart);
                    break;

                case ((int)TraktMenuItems.UserProfile):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.UserProfile);
                    break;

                case ((int)TraktMenuItems.Network):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.Network);
                    break;

                case ((int)TraktMenuItems.Recommendations):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.RecommendationsMovies);
                    break;

                case ((int)TraktMenuItems.Trending):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.TrendingMovies);
                    break;

                case ((int)TraktMenuItems.Popular):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.PopularMovies);
                    break;

                case ((int)TraktMenuItems.Anticipated):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.AnticipatedMovies);
                    break;

                case ((int)TraktMenuItems.WatchList):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.WatchedListMovies);
                    break;

                case ((int)TraktMenuItems.Lists):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.CustomLists);
                    break;
            }
            return true;
        }

        #endregion

        #region Shows
        
        public static bool ShowTraktExtTVShowMenu(string title, string year, string tvdbid, string fanart)
        {
            return ShowTraktExtTVShowMenu(title, year, tvdbid, fanart, false);
        }
        public static bool ShowTraktExtTVShowMenu(string title, string year, string tvdbid, string fanart, bool showAll)
        {
            return ShowTraktExtTVShowMenu(title, year, tvdbid, null, fanart, null, showAll);
        }
        public static bool ShowTraktExtTVShowMenu(string title, string year, string tvdbid, string fanart, SearchPeople people, bool showAll)
        {
            return ShowTraktExtTVShowMenu(title, year, tvdbid, null, fanart, people, showAll);
        }
        public static bool ShowTraktExtTVShowMenu(string title, string year, string tvdbid, string imdbid, string fanart, SearchPeople people, bool showAll)
        {
            var dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(GUIUtils.PluginName());

            GUIListItem pItem = new GUIListItem(Translation.Comments);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Shouts;

            pItem = new GUIListItem(Translation.Rate);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Rate;

            pItem = new GUIListItem(Translation.RelatedShows);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Related;

            pItem = new GUIListItem(Translation.ShowSeasonInfo);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.ShowSeasonInfo;

            pItem = new GUIListItem(Translation.AddToWatchList);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.AddToWatchList;

            pItem = new GUIListItem(Translation.AddToList);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.AddToCustomList;

            pItem = new GUIListItem(Translation.Cast);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Cast;

            pItem = new GUIListItem(Translation.Crew);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Crew;

            // Show SearchBy menu...
            if (people != null && people.Count != 0)
            {
                pItem = new GUIListItem(Translation.SearchBy + "...");
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.SearchBy;
            }

            // also show non-context sensitive items related to shows
            if (showAll)
            {
                // might want to check your recently watched, stats etc
                pItem = new GUIListItem(Translation.UserProfile);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.UserProfile;

                pItem = new GUIListItem(Translation.Network);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Network;

                pItem = new GUIListItem(Translation.Calendar);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Calendar;

                pItem = new GUIListItem(Translation.Recommendations);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Recommendations;

                pItem = new GUIListItem(Translation.Trending);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Trending;

                pItem = new GUIListItem(Translation.Popular);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Popular;

                pItem = new GUIListItem(Translation.Anticipated);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Anticipated;

                pItem = new GUIListItem(Translation.WatchList);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.WatchList;

                pItem = new GUIListItem(Translation.Lists);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Lists;
            }

            // Show Context Menu
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            if (dlg.SelectedId < 0) return false;

            switch (dlg.SelectedId)
            {
                case ((int)TraktMenuItems.Rate):
                    TraktLogger.Info("Displaying rate dialog for tv show. Title = '{0}', Year = '{1}', TVDb ID = '{2}'", title, year.ToLogString(), tvdbid.ToLogString());
                    var show = new TraktSyncShowRated
                    {
                        Ids = new TraktShowId { Tvdb = tvdbid.ToNullableInt32(), Imdb = imdbid.ToNullIfEmpty() },
                        Title = title,
                        Year = year.ToNullableInt32()
                    };
                    int rating = GUIUtils.ShowRateDialog<TraktSyncShowRated>(show);

                    // update local databases
                    if (rating >= 0)
                    {
                        switch (GUIWindowManager.ActiveWindow)
                        {
                            case (int)ExternalPluginWindows.TVSeries:
                                TraktHandlers.TVSeries.SetShowUserRating(rating);
                                break;
                        }

                        if (rating == 0)
                            TraktCache.RemoveShowFromRatings(show);
                        else
                            TraktCache.AddShowToRatings(show, rating);
                    }
                    break;

                case ((int)TraktMenuItems.Shouts):
                    TraktLogger.Info("Displaying Shouts for tv show. Title = '{0}', Year = '{1}', TVDb ID = '{2}', IMDb ID = '{3}'", title, year.ToLogString(), tvdbid.ToLogString(), imdbid.ToLogString());
                    TraktHelper.ShowTVShowShouts(title, year.ToNullableInt32(), tvdbid.ToNullableInt32(), null, imdbid, false, fanart);
                    break;

                case ((int)TraktMenuItems.Related):
                    TraktLogger.Info("Displaying Related shows for tv show. Title = '{0}', Year = '{1}', TVDb ID = '{2}'", title, year.ToLogString(), tvdbid.ToLogString());
                    TraktHelper.ShowRelatedShows(title, year.ToNullableInt32(), tvdbid.ToNullableInt32(), imdbid.ToNullIfEmpty(), null, null);
                    break;

                case ((int)TraktMenuItems.ShowSeasonInfo):
                    TraktLogger.Info("Displaying Season Info for tv show. Title = '{0}', Year = '{1}', TVDb ID = '{2}'", title, year.ToLogString(), tvdbid.ToLogString());
                    var showSummary = new TraktShowSummary
                    {
                        Ids = new TraktShowId
                        {
                            Imdb = imdbid.ToNullIfEmpty(),
                            Tvdb = tvdbid.ToNullableInt32()
                        },
                        Title = title,
                        Year = year.ToNullableInt32()
                    };
                    GUIShowSeasons.Fanart = fanart;
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.ShowSeasons, showSummary.ToJSON());
                    break;

                case ((int)TraktMenuItems.AddToWatchList):
                    TraktLogger.Info("Adding tv show to Watchlist. Title = '{0}', Year = '{1}', TVDb ID = '{2}'", title, year.ToLogString(), tvdbid.ToLogString());
                    TraktHelper.AddShowToWatchList(title, year.ToNullableInt32(), tvdbid.ToNullableInt32(), imdbid.ToNullIfEmpty(), null, null);
                    break;

                case ((int)TraktMenuItems.AddToCustomList):
                    TraktLogger.Info("Adding tv show to Custom List. Title = '{0}', Year = '{1}', TVDb ID = '{2}'", title, year.ToLogString(), tvdbid.ToLogString());
                    TraktHelper.AddRemoveShowInUserList(title, year, tvdbid, false);
                    break;

                case ((int)TraktMenuItems.Cast):
                    TraktLogger.Info("Displaying Cast for show. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    GUICreditsShow.Show = null;
                    GUICreditsShow.Type = GUICreditsShow.CreditType.Cast;
                    GUICreditsShow.Fanart = fanart;
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.CreditsShow, imdbid);
                    break;

                case ((int)TraktMenuItems.Crew):
                    TraktLogger.Info("Displaying Crew for show. Title = '{0}', Year = '{1}', IMDb ID = '{2}'", title, year.ToLogString(), imdbid.ToLogString());
                    GUICreditsShow.Show = null;
                    GUICreditsShow.Type = GUICreditsShow.CreditType.Crew;
                    GUICreditsShow.Fanart = fanart;
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.CreditsShow, imdbid);
                    break;
                case ((int)TraktMenuItems.SearchBy):
                    ShowSearchByMenu(people, title, fanart);
                    break;

                case ((int)TraktMenuItems.UserProfile):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.UserProfile);
                    break;

                case ((int)TraktMenuItems.Network):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.Network);
                    break;

                case ((int)TraktMenuItems.Calendar):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.Calendar);
                    break;

                case ((int)TraktMenuItems.Recommendations):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.RecommendationsShows);
                    break;

                case ((int)TraktMenuItems.Trending):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.TrendingShows);
                    break;

                case ((int)TraktMenuItems.Popular):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.PopularShows);
                    break;

                case ((int)TraktMenuItems.Anticipated):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.AnticipatedShows);

                    break;
                case ((int)TraktMenuItems.WatchList):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.WatchedListShows);
                    break;

                case ((int)TraktMenuItems.Lists):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.CustomLists);
                    break;
            }
            return true;
        }
        #endregion

        #region Seasons
        public static bool ShowTraktExtTVSeasonMenu(string title, string year, string tvdbid, string season, string seasonid, string fanart, SearchPeople people, bool showAll)
        {
            return ShowTraktExtTVSeasonMenu(title, year, tvdbid, null, season, seasonid, fanart, people, showAll);
        }
        public static bool ShowTraktExtTVSeasonMenu(string title, string year, string tvdbid, string imdbid, string season, string seasonid, string fanart, SearchPeople people, bool showAll)
        {
            var dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(GUIUtils.PluginName());

            GUIListItem pItem = new GUIListItem(Translation.Comments);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Shouts;

            pItem = new GUIListItem(Translation.Rate);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Rate;

            pItem = new GUIListItem(Translation.AddToWatchList);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.AddToWatchList;

            //pItem = new GUIListItem(Translation.AddToList);
            //dlg.Add(pItem);
            //pItem.ItemId = (int)TraktMenuItems.AddToCustomList;

            // also show non-context sensitive items related to shows
            if (showAll)
            {
                // might want to check your recently watched, stats etc
                pItem = new GUIListItem(Translation.UserProfile);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.UserProfile;

                pItem = new GUIListItem(Translation.Network);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Network;

                pItem = new GUIListItem(Translation.Calendar);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Calendar;

                pItem = new GUIListItem(Translation.Recommendations);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Recommendations;

                pItem = new GUIListItem(Translation.Trending);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Trending;

                pItem = new GUIListItem(Translation.WatchList);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.WatchList;

                pItem = new GUIListItem(Translation.Lists);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Lists;
            }

            int seasonNumber = 0;
            if (!int.TryParse(season, out seasonNumber))
                return false;

            // Show Context Menu
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            if (dlg.SelectedId < 0) return false;

            switch (dlg.SelectedId)
            {
                case ((int)TraktMenuItems.Rate):
                    TraktLogger.Info("Displaying rate dialog for tv season. Title = '{0}', Year = '{1}', TVDb ID = '{2}', Season = '{3}'", title, year.ToLogString(), tvdbid.ToLogString(), season);
                    GUIUtils.ShowRateDialog<TraktSyncSeasonRatedEx>(new TraktSyncSeasonRatedEx
                    {
                        Ids = new TraktShowId { Tvdb = tvdbid.ToNullableInt32(), Imdb = imdbid.ToNullIfEmpty() },
                        Title = title,
                        Year = year.ToNullableInt32(),
                        Seasons = new List<TraktSyncSeasonRatedEx.Season>
                        {
                            new TraktSyncSeasonRatedEx.Season
                            {
                                Number = seasonNumber,
                                RatedAt = DateTime.UtcNow.ToISO8601()
                            }
                        }
                    });
                    break;

                case ((int)TraktMenuItems.Shouts):
                    TraktLogger.Info("Displaying Shouts for tv season. Title = '{0}', Year = '{1}', TVDb ID = '{2}', IMDb ID = '{3}', Season = '{4}'", title, year.ToLogString(), tvdbid.ToLogString(), imdbid.ToLogString(), season);
                    TraktHelper.ShowTVSeasonShouts(title, year.ToNullableInt32(), tvdbid.ToNullableInt32(), null, imdbid, seasonNumber, false, fanart);
                    break;

                case ((int)TraktMenuItems.AddToWatchList):
                    TraktLogger.Info("Adding tv season to Watchlist. Title = '{0}', Year = '{1}', TVDb ID = '{2}' Season = '{3}'", title, year.ToLogString(), tvdbid.ToLogString(), season);
                    TraktHelper.AddSeasonToWatchList(title, year.ToNullableInt32(), seasonNumber, tvdbid.ToNullableInt32(), imdbid.ToNullIfEmpty(), null, null);
                    break;

                case ((int)TraktMenuItems.AddToCustomList):
                    TraktLogger.Info("Adding tv season to Custom List. Title = '{0}', Year = '{1}', TVDb ID = '{2}', Season = '{3}'", title, year.ToLogString(), tvdbid.ToLogString(), season);
                    //TraktHelper.AddRemoveSeasonInUserList(title, year, tvdbid, false);
                    break;

                case ((int)TraktMenuItems.UserProfile):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.UserProfile);
                    break;

                case ((int)TraktMenuItems.Network):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.Network);
                    break;

                case ((int)TraktMenuItems.Calendar):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.Calendar);
                    break;

                case ((int)TraktMenuItems.Recommendations):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.RecommendationsShows);
                    break;

                case ((int)TraktMenuItems.Trending):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.TrendingShows);
                    break;

                case ((int)TraktMenuItems.WatchList):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.WatchedListShows);
                    break;

                case ((int)TraktMenuItems.Lists):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.CustomLists);
                    break;
            }
            return true;
        }
        #endregion

        #region Episodes
        public static bool ShowTraktExtEpisodeMenu(string title, string year, string season, string episode, string tvdbid, string fanart)
        {
            return ShowTraktExtEpisodeMenu(title, year, season, episode, tvdbid, fanart, false);
        }
        public static bool ShowTraktExtEpisodeMenu(string title, string year, string season, string episode, string tvdbid, string fanart, bool showAll)
        {
            return ShowTraktExtEpisodeMenu(title, year, season, episode, tvdbid, fanart, null, showAll);
        }
        public static bool ShowTraktExtEpisodeMenu(string title, string year, string season, string episode, string tvdbid, string fanart, SearchPeople people, bool showAll)
        {
            return ShowTraktExtEpisodeMenu(title, year, season, episode, tvdbid, false, fanart, people, showAll);
        }
        public static bool ShowTraktExtEpisodeMenu(string title, string year, string season, string episode, string tvdbid, bool isWatched, string fanart, SearchPeople people, bool showAll)
        {
            return ShowTraktExtEpisodeMenu(title, year, season, episode, tvdbid, null, isWatched, fanart, people, showAll);
        }
        public static bool ShowTraktExtEpisodeMenu(string title, string year, string season, string episode, string tvdbid, string episodetvdbid, bool isWatched, string fanart, SearchPeople people, bool showAll)
        {
            return ShowTraktExtEpisodeMenu(title, year, season, episode, tvdbid, null, episodetvdbid, isWatched, fanart, people, showAll);
        }
        public static bool ShowTraktExtEpisodeMenu(string title, string year, string season, string episode, string tvdbid, string imdbid, string episodetvdbid, bool isWatched, string fanart, SearchPeople people, bool showAll)
        {
            var dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            dlg.Reset();
            dlg.SetHeading(GUIUtils.PluginName());

            var pItem = new GUIListItem(Translation.Comments);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Shouts;

            pItem = new GUIListItem(Translation.Rate);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.Rate;

            pItem = new GUIListItem(Translation.AddToWatchList);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.AddToWatchList;

            pItem = new GUIListItem(Translation.AddToList);
            dlg.Add(pItem);
            pItem.ItemId = (int)TraktMenuItems.AddToCustomList;

            // Show Search By menu...
            if (people != null && people.Count != 0)
            {
                pItem = new GUIListItem(Translation.SearchBy + "...");
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.SearchBy;
            }

            // also show non-context sensitive items related to episodes
            if (showAll)
            {
                // might want to check your recently watched, stats etc
                pItem = new GUIListItem(Translation.UserProfile);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.UserProfile;

                pItem = new GUIListItem(Translation.Network);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Network;

                pItem = new GUIListItem(Translation.Calendar);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Calendar;

                pItem = new GUIListItem(Translation.WatchList);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.WatchList;

                pItem = new GUIListItem(Translation.Lists);
                dlg.Add(pItem);
                pItem.ItemId = (int)TraktMenuItems.Lists;
            }

            // Show Context Menu
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            if (dlg.SelectedId < 0) return false;

            switch (dlg.SelectedId)
            {
                case ((int)TraktMenuItems.Rate):
                    TraktLogger.Info("Displaying rate dialog for tv episode. Title = '{0}', Year = '{1}', Season = '{2}', Episode = '{3}', Show ID = '{4}', Episode ID = '{5}'", title, year.ToLogString(), season, episode, tvdbid.ToLogString(), episodetvdbid.ToLogString());
                    var show = new TraktSyncShowRatedEx
                    {
                        Ids = new TraktShowId { Tvdb = tvdbid.ToNullableInt32(), Imdb = imdbid.ToNullIfEmpty() },
                        Title = title,
                        Year = year.ToNullableInt32(),
                        Seasons = new List<TraktSyncShowRatedEx.Season>
                        {
                            new TraktSyncShowRatedEx.Season
                            {
                                Number = season.ToInt(),
                                Episodes = new List<TraktSyncShowRatedEx.Season.Episode>
                                {
                                    new TraktSyncShowRatedEx.Season.Episode
                                    {
                                        Number = episode.ToInt(),
                                        RatedAt = DateTime.UtcNow.ToISO8601()
                                    }
                                }
                            }
                        }
                    };
                    int rating = GUIUtils.ShowRateDialog<TraktSyncShowRatedEx>(show);

                    // update local databases
                    if (rating >= 0)
                    {
                        switch (GUIWindowManager.ActiveWindow)
                        {
                            case (int)ExternalPluginWindows.TVSeries:
                                TraktHandlers.TVSeries.SetEpisodeUserRating(rating);
                                break;
                        }
                    }
                    break;

                case ((int)TraktMenuItems.Shouts):
                    TraktLogger.Info("Displaying Shouts for tv episode. Title = '{0}', Year = '{1}', Season = '{2}', Episode = '{3}'", title, year.ToLogString(), season, episode);
                    TraktHelper.ShowEpisodeShouts(title, year.ToNullableInt32(), tvdbid.ToNullableInt32(), null, imdbid.ToNullIfEmpty(), season.ToInt(), episode.ToInt(), isWatched, fanart);
                    break;

                case ((int)TraktMenuItems.AddToWatchList):
                    TraktLogger.Info("Adding tv episode to Watchlist. Title = '{0}', Year = '{1}', Season = '{2}', Episode = '{3}'", title, year.ToLogString(), season, episode);
                    TraktHelper.AddEpisodeToWatchList(title, season.ToInt(), episode.ToInt(), tvdbid.ToNullableInt32(), null, null, null);
                    break;

                case ((int)TraktMenuItems.AddToCustomList):
                    TraktLogger.Info("Adding tv episode to Custom List. Title = '{0}', Year = '{1}', Season = '{2}', Episode = '{3}', Episode ID = '{4}'", title, year.ToLogString(), season, episode, episodetvdbid.ToLogString());
                    if (string.IsNullOrEmpty(episodetvdbid))
                    {
                        TraktHelper.AddRemoveEpisodeInUserList(new TraktEpisode { Ids = new TraktEpisodeId { Tvdb = episodetvdbid.ToNullableInt32() } }, false);
                    }
                    break;

                case ((int)TraktMenuItems.SearchBy):
                    ShowSearchByMenu(people, title, fanart);
                    break;

                case ((int)TraktMenuItems.UserProfile):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.UserProfile);
                    break;

                case ((int)TraktMenuItems.Calendar):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.Calendar);
                    break;

                case ((int)TraktMenuItems.Network):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.Network);
                    break;

                case ((int)TraktMenuItems.WatchList):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.WatchedListEpisodes);
                    break;

                case ((int)TraktMenuItems.Lists):
                    GUIWindowManager.ActivateWindow((int)TraktGUIWindows.CustomLists);
                    break;
            }
            return true;
        }
        #endregion

        #endregion

        #endregion

        #region Filters

        internal static List<MultiSelectionItem> GetFilterListItems(Dictionary<Filters, bool> filters)
        {
            var selectItems = new List<MultiSelectionItem>();

            foreach (var filter in filters)
            {
                var multiSelectItem = new MultiSelectionItem
                {
                    ItemID = filter.Key.ToString(),
                    ItemTitle = Translation.GetByName(string.Format("Hide{0}", filter.Key)),
                    ItemTitle2 = filter.Value ? Translation.On : Translation.Off,
                    IsToggle = true,
                    Selected = false
                };
                selectItems.Add(multiSelectItem);
            }

            return selectItems;
        }

        internal static bool ShowMovieFiltersMenu()
        {
            Dictionary<Filters, bool> filters = new Dictionary<Filters, bool>();

            filters.Add(Filters.Watched, TraktSettings.TrendingMoviesHideWatched);
            filters.Add(Filters.Watchlisted, TraktSettings.TrendingMoviesHideWatchlisted);
            filters.Add(Filters.Collected, TraktSettings.TrendingMoviesHideCollected);
            filters.Add(Filters.Rated, TraktSettings.TrendingMoviesHideRated);

            var selectedItems = GUIUtils.ShowMultiSelectionDialog(Translation.Filters, GetFilterListItems(filters));
            if (selectedItems == null) return false;

            foreach (var item in selectedItems.Where(l => l.Selected == true))
            {
                // toggle state of all selected items
                switch ((Filters)Enum.Parse(typeof(Filters), item.ItemID, true))
                {
                    case Filters.Watched:
                        TraktSettings.TrendingMoviesHideWatched = !TraktSettings.TrendingMoviesHideWatched;
                        break;
                    case Filters.Watchlisted:
                        TraktSettings.TrendingMoviesHideWatchlisted = !TraktSettings.TrendingMoviesHideWatchlisted;
                        break;
                    case Filters.Collected:
                        TraktSettings.TrendingMoviesHideCollected = !TraktSettings.TrendingMoviesHideCollected;
                        break;
                    case Filters.Rated:
                        TraktSettings.TrendingMoviesHideRated = !TraktSettings.TrendingMoviesHideRated;
                        break;
                }
            }

            return true;
        }

        internal static bool ShowTVShowFiltersMenu()
        {
            Dictionary<Filters, bool> filters = new Dictionary<Filters, bool>();

            filters.Add(Filters.Watched, TraktSettings.TrendingShowsHideWatched);
            filters.Add(Filters.Watchlisted, TraktSettings.TrendingShowsHideWatchlisted);
            filters.Add(Filters.Collected, TraktSettings.TrendingShowsHideCollected);
            filters.Add(Filters.Rated, TraktSettings.TrendingShowsHideRated);

            var selectedItems = GUIUtils.ShowMultiSelectionDialog(Translation.Filters, GetFilterListItems(filters));
            if (selectedItems == null) return false;

            foreach (var item in selectedItems.Where(l => l.Selected == true))
            {
                // toggle state of all selected items
                switch ((Filters)Enum.Parse(typeof(Filters), item.ItemID, true))
                {
                    case Filters.Watched:
                        TraktSettings.TrendingShowsHideWatched = !TraktSettings.TrendingShowsHideWatched;
                        break;
                    case Filters.Watchlisted:
                        TraktSettings.TrendingShowsHideWatchlisted = !TraktSettings.TrendingShowsHideWatchlisted;
                        break;
                    case Filters.Collected:
                        TraktSettings.TrendingShowsHideCollected = !TraktSettings.TrendingShowsHideCollected;
                        break;
                    case Filters.Rated:
                        TraktSettings.TrendingShowsHideRated = !TraktSettings.TrendingShowsHideRated;
                        break;
                }
            }

            return true;
        }

        internal static IEnumerable<TraktMovieTrending> FilterTrendingMovies(IEnumerable<TraktMovieTrending> moviesToFilter)
        {
            if (TraktSettings.TrendingMoviesHideWatched)
                moviesToFilter = moviesToFilter.Where(t => !t.Movie.IsWatched());

            if (TraktSettings.TrendingMoviesHideWatchlisted)
                moviesToFilter = moviesToFilter.Where(t => !t.Movie.IsWatchlisted());

            if (TraktSettings.TrendingMoviesHideCollected)
                moviesToFilter = moviesToFilter.Where(t => !t.Movie.IsCollected());

            if (TraktSettings.TrendingMoviesHideRated)
                moviesToFilter = moviesToFilter.Where(t => t.Movie.UserRating() == null);

            return moviesToFilter;
        }

        internal static IEnumerable<TraktShowTrending> FilterTrendingShows(IEnumerable<TraktShowTrending> showsToFilter)
        {
            if (TraktSettings.TrendingShowsHideWatched)
                showsToFilter = showsToFilter.Where(t => !t.Show.IsWatched());

            if (TraktSettings.TrendingShowsHideWatchlisted)
                showsToFilter = showsToFilter.Where(t => !t.Show.IsWatchlisted());

            if (TraktSettings.TrendingShowsHideCollected)
                showsToFilter = showsToFilter.Where(t => !t.Show.IsCollected());

            if (TraktSettings.TrendingShowsHideRated)
                showsToFilter = showsToFilter.Where(t => t.Show.UserRating() == null);

            return showsToFilter;
        }

        #endregion

        #region Activity Helpers
        internal static string GetActivityListItemTitle(TraktActivity.Activity activity, ActivityView view = ActivityView.community)
        {
            if (activity == null)
                return null;

            string itemName = GetActivityItemName(activity);
            if (string.IsNullOrEmpty(itemName))
                return null;

            string userName = activity.User == null ? "" : activity.User.Username;
            string title = string.Empty;

            if (string.IsNullOrEmpty(activity.Action) || string.IsNullOrEmpty(activity.Type))
                return null;

            var action = (ActivityAction)Enum.Parse(typeof(ActivityAction), activity.Action);
            var type = (ActivityType)Enum.Parse(typeof(ActivityType), activity.Type);

            switch (action)
            {
                case ActivityAction.watching:
                    title = string.Format(Translation.ActivityWatching, userName, itemName);
                    break;

                case ActivityAction.scrobble:
                    if (activity.Plays() > 1)
                    {
                        title = string.Format(Translation.ActivityWatchedWithPlays, userName, itemName, activity.Plays());
                    }
                    else
                    {
                        title = string.Format(Translation.ActivityWatched, userName, itemName);
                    }
                    break;

                case ActivityAction.checkin:
                    title = string.Format(Translation.ActivityCheckedIn, userName, itemName);
                    break;

                case ActivityAction.seen:
                    if (type == ActivityType.episode && activity.Episodes.Count > 1)
                    {
                        title = string.Format(Translation.ActivitySeenEpisodes, userName, activity.Episodes.Count, itemName);
                    }
                    else
                    {
                        title = string.Format(Translation.ActivitySeen, userName, itemName);
                    }
                    break;

                case ActivityAction.collection:
                    if (type == ActivityType.episode && activity.Episodes.Count > 1)
                    {
                        title = string.Format(Translation.ActivityCollectedEpisodes, userName, activity.Episodes.Count, itemName);
                    }
                    else
                    {
                        title = string.Format(Translation.ActivityCollected, userName, itemName);
                    }
                    break;

                case ActivityAction.rating:
                    title = string.Format(Translation.ActivityRatingAdvanced, userName, itemName, activity.Rating);
                    break;

                case ActivityAction.watchlist:
                    if (view != ActivityView.me)
                    {
                        title = string.Format(Translation.ActivityWatchlist, userName, itemName);
                    }
                    else
                    {
                        title = string.Format(Translation.ActivityYourWatchlist, userName, itemName);
                    }
                    break;

                case ActivityAction.review:
                    title = string.Format(Translation.ActivityReview, userName, itemName);
                    break;

                case ActivityAction.shout:
                    title = string.Format(Translation.ActivityShouts, userName, itemName);
                    break;

                case ActivityAction.pause:
                    title = string.Format(Translation.ActivityPaused, userName, itemName, Math.Round(activity.Progress, 0));
                    break;

                case ActivityAction.created: // created list
                    title = string.Format(Translation.ActivityCreatedList, userName, itemName);
                    break;

                case ActivityAction.updated: // updated list
                    if (activity.List.ItemCount == 0)
                    {
                        title = string.Format(Translation.ActivityUpdatedList, userName, itemName);
                    }
                    else if (activity.List.ItemCount == 1)
                    {
                        title = string.Format(Translation.ActivityUpdatedListWithItemCountSingular, userName, itemName, 1);
                    }
                    else
                    {
                        title = string.Format(Translation.ActivityUpdatedListWithItemCount, userName, itemName, activity.List.ItemCount);
                    }
                    break;

                case ActivityAction.item_added: // added item to list
                    title = string.Format(Translation.ActivityAddToList, userName, itemName, activity.List.Name);
                    break;

                case ActivityAction.like:
                    if (type == ActivityType.comment)
                    {
                        title = string.Format(Translation.ActivityLikedComment, userName, activity.Shout.User.Username, itemName);
                    }
                    else if (type == ActivityType.list)
                    {
                        title = string.Format(Translation.ActivityLikedList, userName, itemName);
                    }
                    break;
            }

            // remove user name from your own feed, its not needed - you know who you are
            if ((ActivityView)TraktSettings.ActivityStreamView == ActivityView.me && title.StartsWith(TraktSettings.Username))
                title = title.Replace(TraktSettings.Username + " ", string.Empty);

            return title;
        }

        internal static string GetActivityItemName(TraktActivity.Activity activity)
        {
            string name = string.Empty;

            try
            {
                ActivityType type = (ActivityType)Enum.Parse(typeof(ActivityType), activity.Type);
                ActivityAction action = (ActivityAction)Enum.Parse(typeof(ActivityAction), activity.Action);

                switch (type)
                {
                    case ActivityType.episode:
                        if (action == ActivityAction.seen || action == ActivityAction.collection)
                        {
                            if (activity.Episodes.Count > 1)
                            {
                                // just return show name
                                name = activity.Show.Title;
                            }
                            else
                            {
                                //  get the first and only item in collection of episodes
                                string episodeIndex = activity.Episodes.First().Number.ToString();
                                string seasonIndex = activity.Episodes.First().Season.ToString();
                                string episodeName = activity.Episodes.First().Title;

                                if (!string.IsNullOrEmpty(episodeName))
                                    episodeName = string.Format(" - {0}", episodeName);

                                name = string.Format("{0} - {1}x{2}{3}", activity.Show.Title, seasonIndex, episodeIndex, episodeName);
                            }
                        }
                        else
                        {
                            string episodeName = activity.Episode.Title;

                            if (!string.IsNullOrEmpty(episodeName))
                                episodeName = string.Format(" - {0}", episodeName);

                            name = string.Format("{0} - {1}x{2}{3}", activity.Show.Title, activity.Episode.Season.ToString(), activity.Episode.Number.ToString(), episodeName);
                        }
                        break;

                    case ActivityType.show:
                        name = activity.Show.Title;
                        break;

                    case ActivityType.season:
                        name = string.Format("{0} - {1} {2}", activity.Show.Title, Translation.Season, activity.Season.Number);
                        break;

                    case ActivityType.movie:
                        name = string.Format("{0} ({1})", activity.Movie.Title, activity.Movie.Year);
                        break;

                    case ActivityType.person:
                        name = string.Format("{0}", activity.Person.Name);
                        break;

                    case ActivityType.list:
                        if (action == ActivityAction.item_added)
                        {
                            // return the name of the item added to the list
                            switch (activity.ListItem.Type)
                            {
                                case "show":
                                    name = activity.ListItem.Show.Title;
                                    break;

                                case "season":
                                    name = string.Format("{0} - {1} {2}", activity.ListItem.Show.Title, Translation.Season, activity.ListItem.Season.Number);
                                    break;

                                case "episode":
                                    string episodeIndex = activity.ListItem.Episode.Number.ToString();
                                    string seasonIndex = activity.ListItem.Episode.Season.ToString();
                                    string episodeName = activity.ListItem.Episode.Title;

                                    if (string.IsNullOrEmpty(episodeName))
                                        episodeName = string.Format("{0} {1}", Translation.Episode, episodeIndex);

                                    name = string.Format("{0} - {1}x{2} - {3}", activity.ListItem.Show.Title, seasonIndex, episodeIndex, episodeName);
                                    break;

                                case "movie":
                                    name = string.Format("{0} ({1})", activity.ListItem.Movie.Title, activity.ListItem.Movie.Year);
                                    break;

                                case "person":
                                    name = string.Format("{0}", activity.Person.Name);
                                    break;

                            }
                        }
                        else
                        {
                            // return the list name
                            name = activity.List.Name;
                        }
                        break;

                    case ActivityType.comment:
                        name = activity.Shout.Text.Truncate(30);
                        break;
                }
            }
            catch
            {
                // most likely trakt returned a null object
                name = string.Empty;
            }

            return name;
        }
        #endregion

        #region Parental Controls

        static bool PromptForPinCode
        {
            get
            {
                if (!TraktSettings.ParentalControlsEnabled)
                    return false;

                // check if we ignore parental controls after certain time
                if (TraktSettings.ParentalIgnoreAfterEnabled)
                {
                    // check if the current time is > that allowed time
                    if (Convert.ToDateTime(TraktSettings.ParentalIgnoreAfterTime) < DateTime.Now)
                        return false;
                }

                // check movie certification is allowed
                if (CheckRatingOnMovie())
                    return false;

                // check tv show certification is allowed
                if (CheckRatingOnShow())
                    return false;

                CurrentMovie = null;
                CurrentShow = null;

                return true;
            }
        }

        static bool CheckRatingOnMovie()
        {
            if (!TraktSettings.ParentalIgnoreMovieRatingEnabled)
                return false;

            if (CurrentMediaType != MediaType.Movie)
                return false;

            if (CurrentMovie == null || string.IsNullOrEmpty(CurrentMovie.Certification)) 
                return false;

            string allowedRating = TraktSettings.ParentalIgnoreMovieRating;

            switch (CurrentMovie.Certification)
            {
                case "R":
                    if (allowedRating == "R") return true;
                    break;

                case "PG-13":
                    if (allowedRating == "R")       return true;
                    if (allowedRating == "PG-13")   return true;
                    break;

                case "PG":
                    if (allowedRating == "R")       return true;
                    if (allowedRating == "PG-13")   return true;
                    if (allowedRating == "PG")      return true;
                    break;

                case "G":
                    if (allowedRating == "R")       return true;
                    if (allowedRating == "PG-13")   return true;
                    if (allowedRating == "PG")      return true;
                    if (allowedRating == "G")       return true;
                    break;
            }

            return false;
        }

        static bool CheckRatingOnShow()
        {
            if (!TraktSettings.ParentalIgnoreShowRatingEnabled)
                return false;

            if (CurrentMediaType != MediaType.Show)
                return false;

            if (CurrentShow == null || string.IsNullOrEmpty(CurrentShow.Certification))
                return false;

            string allowedRating = TraktSettings.ParentalIgnoreShowRating;

            switch (CurrentShow.Certification)
            {
                case "M":
                    if (allowedRating == "M") return true;
                    break;

                case "TV-14":
                    if (allowedRating == "M")       return true;
                    if (allowedRating == "TV-14")   return true;
                    break;

                case "TV-PG":
                    if (allowedRating == "M")       return true;
                    if (allowedRating == "TV-14")   return true;
                    if (allowedRating == "TV-PG")   return true;
                    break;

                case "TV-G":
                    if (allowedRating == "M")       return true;
                    if (allowedRating == "TV-14")   return true;
                    if (allowedRating == "TV-PG")   return true;
                    if (allowedRating == "TV-G")    return true;
                    break;

                case "TV-Y7":
                    if (allowedRating == "M")       return true;
                    if (allowedRating == "TV-14")   return true;
                    if (allowedRating == "TV-PG")   return true;
                    if (allowedRating == "TV-G")    return true;
                    if (allowedRating == "TV-Y7")   return true;
                    break;

                case "TV-Y":
                    if (allowedRating == "M")       return true;
                    if (allowedRating == "TV-14")   return true;
                    if (allowedRating == "TV-PG")   return true;
                    if (allowedRating == "TV-G")    return true;
                    if (allowedRating == "TV-Y7")   return true;
                    if (allowedRating == "TV-Y")    return true;
                    break;
            }

            return false;
        }

        #endregion

        #region Translation

        public static string GetTranslatedCreditJob(string job)
        {
            if (string.IsNullOrEmpty(job))
                return string.Empty;

            switch (job.ToLower().Trim())
            {
                case "additional photography":
                    return Translation.AdditionalPhotography;
                case "administration":
                    return Translation.Administration;
                case "adr & dubbing":
                    return Translation.ADRAndDubbing;
                case "animation":
                    return Translation.Animation;
                case "art department assistant":
                    return Translation.ArtDepartmentAssistant;
                case "art department coordinator":
                    return Translation.ArtDepartmentCoordinator;
                case "art department manager":
                    return Translation.ArtDepartmentManager;
                case "art direction":
                    return Translation.ArtDirection;
                case "assistant art director":
                    return Translation.AssistantArtDirector;
                case "assistant director":
                    return Translation.AssistantDirector;
                case "associate producer":
                    return Translation.AssociateProducer;
                case "best boy electric":
                    return Translation.BestBoyElectric;
                case "boom operator":
                    return Translation.BoomOperator;
                case "casting":
                    return Translation.Casting;
                case "camera department manager":
                    return Translation.CameraDepartmentManager;
                case "camera operator":
                    return Translation.CameraOperator;
                case "camera supervisor":
                    return Translation.CameraSupervisor;
                case "camera technician":
                    return Translation.CameraTechnician;
                case "characters":
                    return Translation.Characters;
                case "creature design":
                    return Translation.CreatureDesign;
                case "compositors":
                    return Translation.Compositors;
                case "conceptual design":
                    return Translation.ConceptualDesign;
                case "construction coordinator":
                    return Translation.ConstructionCoordinator;
                case "costume design":
                    return Translation.CostumeDesign;
                case "costume supervisor":
                    return Translation.CostumeSupervisor;
                case "dialogue editor":
                    return Translation.DialogueEditor;
                case "digital effects supervisor":
                    return Translation.DigitalEffectsSupervisor;
                case "digital intermediate":
                    return Translation.DigitalIntermediate;
                case "digital producer":
                    return Translation.DigitalProducer;
                case "director":
                    return Translation.Director;
                case "director of photography":
                    return Translation.DirectorOfPhotography;
                case "driver":
                    return Translation.Driver;
                case "editor":
                    return Translation.Editor;
                case "editorial coordinator":
                    return Translation.EditorialCoordinator;
                case "editorial manager":
                    return Translation.EditorialManager;
                case "editorial production assistant":
                    return Translation.EditorialProductionAssistant;
                case "executive in charge of post production ":
                    return Translation.ExecutiveInChargeOfPostProduction;
                case "executive in charge of production ":
                    return Translation.ExecutiveInChargeOfProduction;
                case "executive producer":
                    return Translation.ExecutiveProducer;
                case "electrician":
                    return Translation.Electrician;
                case "foley":
                    return Translation.Foley;
                case "gaffer":
                    return Translation.Gaffer;
                case "greensman":
                    return Translation.Greensman;
                case "grip":
                    return Translation.Grip;
                case "hair setup":
                    return Translation.HairSetup;
                case "hair stylist":
                case "hairstylist":
                    return Translation.HairStylist;
                case "helicopter camera":
                    return Translation.HelicopterCamera;
                case "lighting technician":
                    return Translation.LightingTechnician;
                case "line producer":
                    return Translation.LineProducer;
                case "layout":
                    return Translation.Layout;
                case "music":
                    return Translation.Music;
                case "modeling":
                    return Translation.Modeling;
                case "music editor":
                    return Translation.MusicEditor;
                case "MakeupArtist":
                    return Translation.MakeupArtist;
                case "novel":
                    return Translation.Novel;
                case "original music composer":
                    return Translation.OriginalMusicComposer;
                case "original story":
                    return Translation.OriginalStory;
                case "other":
                    return Translation.Other;
                case "post production supervisor":
                    return Translation.PostProductionSupervisor;
                case "producer":
                    return Translation.Producer;
                case "production design":
                    return Translation.ProductionDesign;
                case "production manager":
                    return Translation.ProductionManager;
                case "production office assistant":
                    return Translation.ProductionOfficeAssistant;
                case "production office coordinator":
                    return Translation.ProductionOfficeCoordinator;
                case "production supervisor":
                    return Translation.ProductionSupervisor;
                case "prop maker":
                case "propmaker":
                    return Translation.PropMaker;
                case "property master":
                    return Translation.PropertyMaster;
                case "recording supervision":
                    return Translation.RecordingSupervision;
                case "rigging gaffer":
                    return Translation.RiggingGaffer;
                case "rigging grip":
                    return Translation.RiggingGrip;
                case "screenplay":
                    return Translation.Screenplay;
                case "scenic artist":
                    return Translation.ScenicArtist;
                case "score engineer":
                    return Translation.ScoreEngineer;
                case "sculptor":
                    return Translation.Sculptor;
                case "second unit":
                    return Translation.SecondUnit;
                case "set costumer":
                    return Translation.SetCostumer;
                case "set decoration":
                    return Translation.SetDecoration;
                case "set dressing artist":
                    return Translation.SetDressingArtist;
                case "set designer":
                    return Translation.SetDesigner;
                case "sound designer":
                    return Translation.SoundDesigner;
                case "sound editor":
                    return Translation.SoundEditor;
                case "sound effects editor":
                    return Translation.SoundEffectsEditor;
                case "sound mixer":
                    return Translation.SoundMixer;
                case "sound re-recording mixer":
                    return Translation.SoundReRecordingMixer;
                case "special effects":
                    return Translation.SpecialEffects;
                case "special effects coordinator":
                    return Translation.SpecialEffectsCoordinator;
                case "still photographer":
                    return Translation.StillPhotographer;
                case "story":
                    return Translation.Story;
                case "storyboard":
                    return Translation.Storyboard;
                case "stunt coordinator":
                    return Translation.StuntCoordinator;
                case "stunts":
                    return Translation.Stunts;
                case "supervising sound editor":
                    return Translation.SupervisingSoundEditor;
                case "technical supervisor":
                    return Translation.TechnicalSupervisor;
                case "thanks":
                    return Translation.Thanks;
                case "transportation captain":
                    return Translation.TransportationCaptain;
                case "transportation co-captain":
                    return Translation.TransportationCoCaptain;
                case "transportation coordinator":
                    return Translation.TransportationCoordinator;
                case "video assist operator":
                    return Translation.VideoAssistOperator;
                case "vfx artist":
                    return Translation.VFXArtist;
                case "vfx production coordinator":
                    return Translation.VFXProductionCoordinator;
                case "vfx supervisor":
                    return Translation.VFXSupervisor;
                case "visual development":
                    return Translation.VisualDevelopment;
                case "visual effects":
                    return Translation.VisualEffects;
                case "visual effects design consultant":
                    return Translation.VisualEffectsDesignConsultant;
                case "visual effects editor":
                    return Translation.VisualEffectsEditor;
                case "visual effects producer":
                    return Translation.VisualEffectsProducer;
                case "visual effects supervisor":
                    return Translation.VisualEffectsSupervisor;
                case "writer":
                    return Translation.Writer;
                default:
                    return job;
            }
        }

        public static string GetTranslatedCreditType(Credit credit)
        {
            switch (credit)
            {
                case Credit.Art:
                    return Translation.Art;
                case Credit.Camera:
                    return Translation.Camera;
                case Credit.Cast:
                    return Translation.Cast;
                case Credit.CostumeAndMakeUp:
                    return Translation.CostumeAndMakeUp;
                case Credit.Crew:
                    return Translation.Crew;
                case Credit.Directing:
                    return Translation.Directing;
                case Credit.Production:
                    return Translation.Production;
                case Credit.Sound:
                    return Translation.Sound;
                case Credit.Writing:
                    return Translation.Writing;
                default:
                    return string.Empty;
            }
        }

        #endregion
    }

    /// <summary>
    /// Used to collect a list of people to SearchBy in External Plugins
    /// </summary>
    public class SearchPeople
    {
        public List<string> Actors = new List<string>();
        public List<string> Directors = new List<string>();
        public List<string> Producers = new List<string>();
        public List<string> Writers = new List<string>();
        public List<string> GuestStars = new List<string>();
        
        public int Count
        {
            get
            {
                int peopleCount = 0;
                peopleCount += Actors.Count();
                peopleCount += Directors.Count();
                peopleCount += Producers.Count();
                peopleCount += Writers.Count();
                peopleCount += GuestStars.Count();
                return peopleCount;
            }
        }
    }
}
