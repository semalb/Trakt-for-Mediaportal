using System; // i need this for stringcomparer.
using System.Collections.Generic;
using TraktPlugin.GUI;
using TraktPlugin.Extensions;
using TraktPlugin.TraktAPI;
using TraktPlugin.TraktAPI.DataStructures;
using TraktPlugin.TraktAPI.Enums;
using TraktPlugin.TraktAPI.Extensions;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using System.IO;
using System.Threading;
using System.Web;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace TraktPlugin.TraktHandlers
{
    public class EVideoInfo : VideoInfo
    {
        //public bool isLiveTV { get; set; } = false;
        private string OriginalTitle;
        private string OriginalSeasonIdx;
        private string OriginalEpisodeIdx;
        private string OriginalYear;
        private VideoType OriginalType;

        public EVideoInfo()
        {
        }


        public void setVariables()
        {
            OriginalTitle = Title;
            OriginalSeasonIdx = SeasonIdx;
            OriginalEpisodeIdx = EpisodeIdx;
            OriginalYear = Year;
            OriginalType = Type;
        }
        #region Overrides
        public override string ToString()
        {
            if (this.Type == VideoType.Series)
                return string.Format("{0} - {1}x{2}", this.OriginalTitle, this.OriginalSeasonIdx, this.OriginalEpisodeIdx);
            else
                return string.Format("{0}{1}", this.OriginalTitle, string.IsNullOrEmpty(this.OriginalYear) ? string.Empty : " (" + this.OriginalYear + ")");
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return (this.ToString().Equals(((EVideoInfo)obj).ToString()));
        }
        public string getOriginalTitle()
        {
            return OriginalTitle;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        #endregion
    }


    public class EbasicHandler
    {
        private static EVideoInfo currentProgram = null; // use lock on videoinfo to avoid cucurrent actions.
        //private static IEnumerable<TraktSearchResult> search;
       // public static IEnumerable<TraktSearchResult> firstGUISearch;
        public static TraktShowSummary summary;

        public static void StartGui(int responseCode, TraktEPGCacheRecord data, bool doModal)
        {
            //try
            //{
                if (doModal)
                {
                    GUIDialogShowSelect dialog = (GUIDialogShowSelect)GUIWindowManager.GetWindow((int)TraktGUIWindows.GUIShowDialogSelect);
                    if (dialog == null) TraktLogger.Info("Cannot create the dialog");
                    else
                    {
                        dialog.Reset();
                        dialog.SetHeading("Choose what to scrobble", "nameoftheprogram");
                        if ((responseCode == 904) && (getCurrentProgram().Type.ToString().Equals("Series")))
                        {
                            dialog.currentSearch = SearchShowSeasons(data.Show.Ids.Slug);
                            dialog.currentTvShow = data.Show;
                        }
                        else dialog.currentSearch = TraktAPI.TraktAPI.SearchByName(getCurrentProgram().Title);
                        TraktLogger.Info("Activating dialog");
                        dialog.ShowQuickNumbers = false;
                        dialog.DoModal(GUIWindowManager.ActiveWindow);
                    }
                }
                else
                {
                    if ((responseCode == 904) && (getCurrentProgram().Type.ToString().Equals("Series")))
                    {
                        GUIShowSelectGUI.callSearchGUI(SearchShowSeasons(data.Show.Ids.Slug), data.Show);
                    }
                    else
                    {
                        GUIShowSelectGUI.callSearchGUI(TraktAPI.TraktAPI.SearchByName(getCurrentProgram().Title));
                    }
                }
            //}
            //catch (NullReferenceException exception)
            //{
            //    TraktLogger.Info("An exception has been generated");
            //}
        }

        public static TraktScrobbleMovie GetOriginalMovieTitle(EVideoInfo info, double playerprogress)
        {
            IEnumerable<TraktSearchResult> movieMatch = TraktAPI.TraktAPI.SearchByName(info.Title, "movie","Title");
            IEnumerator<TraktSearchResult> p = movieMatch.GetEnumerator();
            if (!(p.MoveNext()))
            {
#if DEBUG
                TraktLogger.Info("No result found for '{0}'", info.Title);
#endif
                return null;
            }
#if DEBUG
            TraktLogger.Info("Fetching informations for '{0}'. Matched to '{1}'", info.Title, p.Current.Movie.Title);
#endif
            var scrobbleData = new TraktScrobbleMovie
            {
                Movie = new TraktMovie
                {
                    Ids = new TraktMovieId(),
                    Title = p.Current.Movie.Title.ToString(),
                    Year = info.Year.ToNullableInt32()
                },
                Progress = playerprogress,
                AppDate = TraktSettings.BuildDate,
                AppVersion = TraktSettings.Version
            };

            return scrobbleData;
        }

        //Queries trakt with the title of the show.
        //returns true if finds something and is unique.
        //returns false and the result otherwise.
        public static bool QueryOriginalTitle(string title, out IEnumerable<TraktSearchResult> result, string typeOfQuery = "movie,show", string fields = "title,translations,aliases")
        {
            result = TraktAPI.TraktAPI.SearchByName(title,typeOfQuery,fields);
            IEnumerator<TraktSearchResult> p = result.GetEnumerator();
            while (p.MoveNext())
            {
                TraktLogger.Info(string.Format("Current title: {0}", p.Current.Type.Equals("show") ? p.Current.Show.Title : p.Current.Movie.Title));    
            }
            p.Reset();
            if (p.MoveNext())
            {
                //there's a first element               
                if (p.MoveNext()) return false; //there's a second element. Return false, not a single match
                return true; //there is a single match
            }
            return false; //returns false, there are no elements.
        }

        // Tries to match the videoinfo with the corresponding show or movie
        // If it finds a single result, and matches the tipe of videoinfo data
        // modifies the program and returns true. Otherwise returns false.
        // Stores the search for subsequent actions.
        public static bool setOriginalTitle(ref EVideoInfo info)
        {
            IEnumerable<TraktSearchResult> search;
            if (QueryOriginalTitle(info.Title, out search))
            {
                IEnumerator<TraktSearchResult> p = search.GetEnumerator();
                if (p.MoveNext())
                {
                    if ((p.Current.Type.Equals("show")) & (info.Type == VideoType.Series))
                    {
                        //overrideCurrentProgram(p.Current.Show);
                        info.Title = p.Current.Show.Title;
                        info.Year = p.Current.Show.Year.ToString();
                        return true;
                    }
                    if ((p.Current.Type.Equals("movie")) & (info.Type == VideoType.Movie))
                    {
                        //overrideCurrentProgram(p.Current.Movie);
                        info.Title = p.Current.Movie.Title;
                        info.Year = p.Current.Movie.Year.ToString();
                        return true;
                    }
                }
            }
            return false;
        }


        #region Scrobbling methods
        internal static void StartScrobble(object scrobbledata)
        {
            var scrobbleThread = new Thread((objVideoInfo) =>
            {
                if (scrobbledata is TraktScrobbleEpisode)
                {
                    var info = objVideoInfo as TraktScrobbleEpisode;
#if DEBUG
                    TraktLogger.Info("Starting new thread to manually scrobble show: {0}, S{1}xE{2}", info.Show.Title, info.Episode.Season, info.Episode.Number);
                    TraktLogger.Info(info.ToString());
#endif
                    var response = TraktAPI.TraktAPI.StartEpisodeScrobble(info);
                    //TraktLogger.LogTraktResponse(response);
                }
                else if (scrobbledata is TraktScrobbleMovie)
                {
                    var info = objVideoInfo as TraktScrobbleMovie;
#if DEBUG
                    TraktLogger.Info("Starting new thread to manually scrobble movie: {0},{1}", info.Movie.Title, info.Movie.Year);
#endif
                    var response = TraktAPI.TraktAPI.StartMovieScrobble(info);
                    TraktLogger.LogTraktResponse(response);
                }
                else
                {
                    TraktLogger.Info("Bad data passed {0}", objVideoInfo.ToString());
                }

            })
            {
                IsBackground = true,
                Name = "Scrobble"
            };

            scrobbleThread.Start(scrobbledata);
        }

        internal static TraktScrobbleResponse StartScrobbleEpisode(EVideoInfo videoInfo)
        {
            // get scrobble data to send to api
            var scrobbleData = CreateEpisodeScrobbleData(videoInfo);
            if (scrobbleData == null) return null;
            TraktLogger.Info("Trying to scrobble '{0}' season {1} episode {2}", scrobbleData.Show.Title, scrobbleData.Episode.Season, scrobbleData.Episode.Number);
            var response = TraktAPI.TraktAPI.StartEpisodeScrobble(scrobbleData);
            TraktLogger.LogTraktResponse(response);
            return response;

        }
        internal static TraktScrobbleResponse StartScrobbleMovie(EVideoInfo videoInfo)
        {
            // get scrobble data to send to api
            var scrobbleData = CreateMovieScrobbleData(videoInfo);
            if (scrobbleData == null) return null;

            var response = TraktAPI.TraktAPI.StartMovieScrobble(scrobbleData);
            TraktLogger.LogTraktResponse(response);
            return response;
        }

        internal static TraktScrobbleEpisode CreateEpisodeScrobbleData(EVideoInfo info)
        {
            // create scrobble data
            var scrobbleData = new TraktScrobbleEpisode
            {
                Show = new TraktShow
                {
                    Ids = new TraktShowId(),
                    Title = info.Title,
                    Year = info.Year.ToNullableInt32()
                },
                Episode = new TraktEpisode
                {
                    Ids = new TraktEpisodeId(),
                    Number = info.EpisodeIdx.ToInt(),
                    Season = info.SeasonIdx.ToInt()
                },
                Progress = GetPlayerProgress(info),
                AppDate = TraktSettings.BuildDate,
                AppVersion = TraktSettings.Version
            };
            return scrobbleData;
        }
        internal static TraktScrobbleMovie CreateMovieScrobbleData(EVideoInfo info)
        {
            //create scrobble data
            var scrobbleData = new TraktScrobbleMovie
            {
                Movie = new TraktMovie
                {
                    Ids = new TraktMovieId(),
                    Title = info.Title,
                    Year = info.Year.ToNullableInt32()
                },
                Progress = GetPlayerProgress(info),
                AppDate = TraktSettings.BuildDate,
                AppVersion = TraktSettings.Version
            };
            return scrobbleData;
        }
        internal static double GetPlayerProgress(EVideoInfo videoInfo)
        {
            //get duration/ position in minutes
            double duration = videoInfo.Runtime > 0.0 ? videoInfo.Runtime : g_Player.Duration / 60;
            double position = g_Player.CurrentPosition / 60;
            double progress = 0.0;

            if (duration > 0.0)
                progress = (position / duration) * 100.0;

            if (progress > 100) progress = 100;

            return Math.Round(progress, 2);
        }
        #endregion

        #region EVideoInfo Access methods with locks
        public static void setCurrentProgram(EVideoInfo videoInfoIn)
        {
            if (currentProgram == null) currentProgram = videoInfoIn;
            else
            {
                lock (currentProgram)
                {
#if DEBUG
                    TraktLogger.Info("Current Program is a {0}: '{1}' and its scrobbling status is {2}", currentProgram.Type, currentProgram.Title, currentProgram.IsScrobbling);
#endif
                    currentProgram = null;
                    currentProgram = videoInfoIn;
#if DEBUG
                    TraktLogger.Info("New program set is a {0}: '{1}' and its scrobbling status is {2}", currentProgram.Type, currentProgram.Title, currentProgram.IsScrobbling);
#endif
                }
            }
        }
        public static EVideoInfo getCurrentProgram()
        {
            if (currentProgram == null) return null;
            lock (currentProgram)
            {
                return currentProgram;
            }
        }
        public static string getOriginalTitle()
        {
            lock (currentProgram)
            {
                return currentProgram.getOriginalTitle();
            }
        }
        public static void SetCurrentProgramIsScrobbling(bool status)
        {
            lock (currentProgram)
            {
                currentProgram.IsScrobbling = status;
            }
        }
        public static bool IsCurrentProgramScrobbling()
        {
            lock (currentProgram)
            {
                return currentProgram.IsScrobbling;
            }
        }
        public static VideoType getCurrentProgramType()
        {
            lock (currentProgram)
            {
                return currentProgram.Type;
            }
        }
        public static VideoInfo returnVideoInfo()
        {
            lock (currentProgram)
            {
                return currentProgram;
            }
        }
        public static void clearCurrentProgram()
        {
            lock (currentProgram)
            {
                currentProgram = null;
            }
        }

        public static void overrideCurrentProgramTitle(string newTitle)
        {
            if (currentProgram != null)
                lock (currentProgram)
                {
                    currentProgram.Title = newTitle;
                }
        }
        public static void overrideCurrentProgram(TraktShowSummary originalShow, TraktEpisode episode = null)
        {
            if (currentProgram != null)
                lock (currentProgram)
                {
                    currentProgram.Title = originalShow.Title;
                    currentProgram.Year = originalShow.Year.ToString();
                    currentProgram.Type = VideoType.Series;
                    if (episode != null)
                    {
                        currentProgram.EpisodeIdx = episode.Number.ToString();
                        currentProgram.SeasonIdx = episode.Season.ToString();
                    }
                }
        }
        public static void overrideCurrentProgram(TraktMovieSummary originalMovie)
        {
            {
                if (currentProgram != null)
                    lock (currentProgram)
                    {
                        currentProgram.Title = originalMovie.Title;
                        currentProgram.EpisodeIdx = null;
                        currentProgram.SeasonIdx = null;
                        currentProgram.Year = originalMovie.Year.ToString();
                        currentProgram.Type = VideoType.Movie;
                    }
            }
        }

        public static void overrideVideoInfoProgram(ref EVideoInfo info, TraktMovieSummary originalMovie)
        {
            {
                info.Title = originalMovie.Title;
                info.EpisodeIdx = null;
                info.SeasonIdx = null;
                info.Year = originalMovie.Year.ToString();
                info.Type = VideoType.Movie;
            }
        }
        public static void overrideVideoInfoProgram(ref EVideoInfo info, TraktShowSummary originalShow, TraktEpisode episode = null)
        {
            info.Title = originalShow.Title;
            info.Year = originalShow.Year.ToString();
            info.Type = VideoType.Series;
            if (episode != null)
            {
                info.EpisodeIdx = episode.Number.ToString();
                info.SeasonIdx = episode.Season.ToString();
            }
        }


        #endregion

        #region EXTENSION TO TRAKTAPI
        public static IEnumerable<TraktSearchResult> SearchShowSeasons(string id, string extendedParameter = "full")
        {
            //IEnumerable<TraktSearchResult> searchResults;
            List<TraktSearchResult> resultLIst = new List<TraktSearchResult>();
            IEnumerable<TraktSeasonSummary> seasons = TraktAPI.TraktAPI.GetShowSeasons(id);
            IEnumerator<TraktSeasonSummary> p = seasons.GetEnumerator();
            while (p.MoveNext())
            {
                resultLIst.Add(new TraktSearchResult { Type = "season", Season = p.Current });
            }
            return resultLIst;
        }
        public static IEnumerable<TraktSearchResult> SearchSeasonEpisodes(string showId, string seasonId)
        {
            //IEnumerable<TraktSearchResult> searchResults;
            List<TraktSearchResult> resultLIst = new List<TraktSearchResult>();
            IEnumerable<TraktEpisodeSummary> episodes = TraktAPI.TraktAPI.GetSeasonEpisodes(showId,seasonId);
            IEnumerator<TraktEpisodeSummary> p = episodes.GetEnumerator();
            while (p.MoveNext())
            {
                resultLIst.Add(new TraktSearchResult { Type = "episode", Episode = p.Current });
            }
            return resultLIst;
        }

        #endregion
    }
}