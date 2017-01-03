using TraktPlugin.TraktAPI.DataStructures;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using TraktPlugin.TraktHandlers;
using Action = MediaPortal.GUI.Library.Action;
using TraktPlugin.Cache;
using TvPlugin;
using System.IO;

namespace TraktPlugin.GUI
{
    public class GUIShowSelectGUI : GUIWindow
    {
        [SkinControlAttribute(873008)]
        protected GUIListControl resultListControl;
        [SkinControlAttribute(36)]
        protected GUITextControl AiringNow;
        [SkinControlAttribute(99)]
        protected GUIVideoControl video;
        [SkinControlAttribute(38)]
        protected GUITextScrollUpControl plot;
        [SkinControlAttribute(37)]
        protected GUIImage channelLogo;
        [SkinControlAttribute(14)]
        protected GUILabelControl channelName;


        private static IEnumerable<TraktSearchResult> currentSearch = null;
        private static TraktShowSummary currentTvShow = null;
        private static bool wasFullScreenVideo = false;
        private int searchlevel = 0;
        private Stack<IEnumerable<TraktSearchResult>> searchStack = new Stack<IEnumerable<TraktSearchResult>>();

        public GUIShowSelectGUI()
        {
        }
        public override int GetID
        {
            get
            {
                return (int)TraktGUIWindows.EPGShowSelect;
            }          
        }
        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\Trakt.ShowSelectGUI.xml");
        }
        protected override void OnPageLoad()
        {
            PopulateListControl(currentSearch);
            PopulateChannelData();
            base.OnPageLoad();
        }
        protected override void OnPageDestroy(int new_windowId)
        {
           // ClearWindow();
            base.OnPageDestroy(new_windowId);
        }
        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            //if (GUIBackgroundTask.Instance.IsBusy) return;
            switch (controlId)
            {
                #region SEARCH LIST CONTROL SELECTED
                case (873008):
                    if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
                    {
                        
                        GUIListItem item = resultListControl.SelectedListItem as GUIListItem;

                        #region SCROBBLEMOVIE
                        if (item.TVTag is TraktMovieSummary)
                        {
                            // Get movie from traktmoviesummary
                            TraktMovie currentMovieSelected = new TraktMovie
                            {
                                Title = ((TraktMovieSummary)item.TVTag).Title,
                                Year = ((TraktMovieSummary)item.TVTag).Year,
                                Ids = ((TraktMovieSummary)item.TVTag).Ids
                            };
                            var data = new TraktScrobbleMovie
                            {
                                Movie = currentMovieSelected,
                                Progress = EbasicHandler.GetPlayerProgress(EbasicHandler.getCurrentProgram()),
                                AppDate = TraktSettings.BuildDate,
                                AppVersion = TraktSettings.Version                                
                            };
                            EPGCache.addOnCache(EbasicHandler.getCurrentProgram().getOriginalTitle(), EPGCache.createData((TraktMovieSummary)item.TVTag));
                            EbasicHandler.overrideCurrentProgram((TraktMovieSummary)item.TVTag);
                            EbasicHandler.StartScrobble(data);
                            exitGUI();
                        }
                        #endregion
                        #region SCROBBLESHOW
                        else if (item.TVTag is TraktEpisodeSummary)//found!
                        {
                            TraktLogger.Info("EpisodeSummary Detected");
                            var data = new TraktScrobbleEpisode
                            {
                                Show = new TraktShow
                                {
                                    Title = currentTvShow.Title,
                                    Year = currentTvShow.Year,
                                    Ids = currentTvShow.Ids
                                },
                                Episode = new TraktEpisode
                                {
                                    Season = ((TraktEpisodeSummary)item.TVTag).Season,
                                    Number = ((TraktEpisodeSummary)item.TVTag).Number,
                                    Title = ((TraktEpisodeSummary)item.TVTag).Title,
                                    Ids = ((TraktEpisodeSummary)item.TVTag).Ids
                                },
                                Progress = EbasicHandler.GetPlayerProgress(EbasicHandler.getCurrentProgram()),
                                AppDate = TraktSettings.BuildDate,
                                AppVersion = TraktSettings.Version
                            };
                            EPGCache.addOnCache(EbasicHandler.getCurrentProgram().getOriginalTitle(), EPGCache.createData(currentTvShow));
                            EbasicHandler.overrideCurrentProgram(currentTvShow, data.Episode);
                            EbasicHandler.StartScrobble(data);
                            exitGUI();
                        }
                        #endregion

                        else if (item.TVTag is TraktShowSummary)
                        {
                            TraktLogger.Info("TraktShowSummary Detected");
                            currentTvShow = (TraktShowSummary)item.TVTag;
                            searchStack.Push(currentSearch);
                            PopulateListControl(EbasicHandler.SearchShowSeasons(((TraktShowSummary)item.TVTag).Ids.Slug), searchlevel++);
                        }
                        else if (item.TVTag is TraktSeasonSummary)
                        {
                            searchStack.Push(currentSearch);
                            PopulateListControl(EbasicHandler.SearchSeasonEpisodes(currentTvShow.Ids.Slug, ((TraktSeasonSummary)item.TVTag).Number.ToString()),searchlevel++);
                        }
                        else if (item.TVTag.ToString() == "FirstButton" )
                        {
                            if ((item.Label).ToString() == "Manual Search")
                            {
                                //popup the input dialog to search.
                                VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
                                keyboard.Reset();
                                keyboard.Text = EbasicHandler.getCurrentProgram().Title;
                                keyboard.DoModal(GUIWindowManager.ActiveWindow);
                                if (keyboard.IsConfirmed)
                                {
                                    PopulateListControl(TraktAPI.TraktAPI.SearchByName(keyboard.Text, "show,movie", "title"));
                                }
                            }
                            else //this handles back button
                            {
                                PopulateListControl(searchStack.Pop(), searchlevel--);
                            }

                        }
                    }
                    break;
                #endregion 
                default:
                    TraktLogger.Info("I don't know what's the object.");
                    base.OnClicked(controlId, control, actionType);
                    break;
            }
        }
        private void ClearWindow()
        {
            resultListControl.Clear();
            currentSearch = null;
            currentTvShow = null;
        }

        public override void OnAction(Action action)
        {
            switch (action.wID)
            {
                case Action.ActionType.ACTION_PREVIOUS_MENU:
                    exitGUI();
                    base.OnAction(action);
                    break;
                case Action.ActionType.ACTION_STOP:
                    exitGUI();
                    //GUIWindowManager.CloseCurrentWindow();
                    base.OnAction(action);
                    break;
                default:
                    base.OnAction(action);
                    break;
            }
        }

        /// <summary>
        /// This calls the searchGUI handling the fullscreen video replacement.
        /// This allows to stop the video from the search GUI and gracefully go to the previous window.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="show"></param>
        public static void callSearchGUI(IEnumerable<TraktSearchResult> search, TraktShowSummary show = null)
        {
            wasFullScreenVideo = GUIGraphicsContext.IsFullScreenVideo;
            currentSearch = search;
            currentTvShow = show;
            GUIWindowManager.ActivateWindow((int)TraktGUIWindows.EPGShowSelect, GUIGraphicsContext.IsFullScreenVideo);
        }

        /// <summary>
        /// Populates GUIListControl with titles from the search. the searchLevel defines if it's the first search, thus the manual search button is needed
        /// or a subsequent search, inserting the back button instead.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="searchLevel"></param>
        private void PopulateListControl(IEnumerable<TraktSearchResult> search, int searchLevel = 0)
        {
            resultListControl.Clear();
            GUIListItem item;
            if (searchlevel < 1)
            {
                //manualSearch button, it's first search
                item = new GUIListItem("Manual Search");
                item.TVTag = "FirstButton";
                resultListControl.Add(item);
            }
            else
            {
                item = new GUIListItem("Back");
                item.TVTag = "FirstButton";
                resultListControl.Add(item);
            }
            //Now populating items           
            IEnumerator<TraktSearchResult> p = search.GetEnumerator();
            while (p.MoveNext())
            {
                if (p.Current.Type == "show")
                {
                    item = new GUIListItem(p.Current.Show.Title);
                    item.TVTag = p.Current.Show;
#if DEBUG
                    TraktLogger.Info("Adding '{0}' to item '{1}'. Type detected: '{2}'", p.Current.Show.Title, item.Label, p.Current.Type);
#endif
                    resultListControl.Add(item);
                }
                if (p.Current.Type == "season")
                {
                    item = new GUIListItem(string.Format("Season {0}", p.Current.Season.Number));
                    item.TVTag = p.Current.Season;
#if DEBUG
                    TraktLogger.Info("adding Season '{0}' to the list as {1}. Object type: {2}", p.Current.Season.Number, p.Current.Type, p.Current.Season.ToString());
#endif
                    resultListControl.Add(item);
                }
                if (p.Current.Type == "episode")
                {
                    item = new GUIListItem(string.Format("Episode {0}: {1} ", p.Current.Episode.Number, p.Current.Episode.Title));
                    item.TVTag = p.Current.Episode;
#if DEBUG
                    TraktLogger.Info("adding Episode '{0}' to the list", p.Current.Episode.Number);
#endif
                    resultListControl.Add(item);
                }
                if (p.Current.Type == "movie")
                {
                    item = new GUIListItem(p.Current.Movie.Title);
                    item.TVTag = p.Current.Movie;
                    resultListControl.Add(item);
                }
            }
            currentSearch = search;
        }

        /// <summary>
        /// We need to know what to do when exiting GUI.
        /// If the GUI started with the video FULLSCREEN it replaced the fullscreen window. We have to handle that gracefully.
        /// This method does that for you.
        /// </summary>
        public static void exitGUI()
        {
            if (wasFullScreenVideo && GUIGraphicsContext.IsPlayingVideo)
            {
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO, true);
            }
            else
            {
                GUIWindowManager.ShowPreviousWindow();
            }
        }

        private void PopulateChannelData()
        {
            channelName.Label = TVHome.Navigator.CurrentChannel;
            AiringNow.Label = TVHome.Navigator.Channel.CurrentProgram.Title;
            plot.Label = TVHome.Navigator.Channel.CurrentProgram.Description;
            channelLogo.FileName = Path.Combine(Config.GetFolder(Config.Dir.Thumbs), string.Format(@"TV\logos\{0}.png", TVHome.Navigator.CurrentChannel));
        }
    }
}