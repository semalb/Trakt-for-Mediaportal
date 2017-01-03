using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using TraktPlugin.TraktAPI.DataStructures;
using TraktPlugin.Cache;
using Action = MediaPortal.GUI.Library.Action;
using TraktPlugin.TraktHandlers;

namespace TraktPlugin.GUI
{
    public class GUIDialogShowSelect : GUIDialogMenu
    {
        [SkinControlAttribute(3)]
        protected GUIListControl resultListControl = null;
        [SkinControlAttribute(4)]
        protected GUILabelControl heading = null;
        [SkinControlAttribute(5)]
        protected GUILabelControl headingGFX = null;
        [SkinControlAttribute(2)]
        protected GUIButtonControl exitButton = null;

        public IEnumerable<TraktSearchResult> currentSearch;
        public TraktShowSummary currentTvShow;
        private int searchlevel = 0;
        private Stack<IEnumerable<TraktSearchResult>> searchStack = new Stack<IEnumerable<TraktSearchResult>>();


        #region Overrides
        public override bool Init()
        {
            return Load(GUIGraphicsContext.GetThemedSkinFile(@"\Trakt.ShowSelectDialog.xml"));
        }
        public GUIDialogShowSelect()
        {
            GetID = 87609;
        }

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            //if (GUIBackgroundTask.Instance.IsBusy) return;
            switch (controlId)
            {
                #region SEARCH LIST CONTROL SELECTED
                case (3):
                    if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
                    {

                        GUIListItem item = resultListControl.SelectedListItem as GUIListItem;
#if DEBUG
                        TraktLogger.Info("The button pressed has a tvTAG as: {0}, ad has a string value of: {1}", item.TVTag.GetType(), item.TVTag.ToString());
#endif
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
                        }
                        #endregion  
                        else if (item.TVTag is TraktShowSummary)//search for seasons
                        {
                            TraktLogger.Info("TraktShowSummary Detected");
                            currentTvShow = (TraktShowSummary)item.TVTag;
                            searchStack.Push(currentSearch);
                            PopulateListControl(EbasicHandler.SearchShowSeasons(((TraktShowSummary)item.TVTag).Ids.Slug), searchlevel++);
                            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
                            OnMessage(msg);

                        }
                        else if (item.TVTag is TraktSeasonSummary) //Episode for episodes
                        {
#if DEBUG
                            TraktLogger.Info("SeasonSummary Detected");
                            TraktLogger.Info("Season '{0}' for show {1} selected. Getting Episodes.", ((TraktSeasonSummary)item.TVTag).Number, currentTvShow.Ids.Slug);
#endif
                            searchStack.Push(currentSearch);
                            PopulateListControl(EbasicHandler.SearchSeasonEpisodes(currentTvShow.Ids.Slug, ((TraktSeasonSummary)item.TVTag).Number.ToString()), searchlevel++);
                            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
                            OnMessage(msg);
                        }
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
#if DEBUG
                            TraktLogger.Info(currentTvShow.ToString());
#endif
                            EPGCache.addOnCache(EbasicHandler.getCurrentProgram().getOriginalTitle(), EPGCache.createData(currentTvShow));
                            EbasicHandler.overrideCurrentProgram(currentTvShow, data.Episode);
                            EbasicHandler.StartScrobble(data);
                        }
                        #endregion

                        else if (item.TVTag.ToString() == "FirstButton")
                        {
                            if ((item.Label).ToString() == "Manual Search")
                            {
                                //popup the input dialog to search. I need to call the full GUI here.
#if DEBUG
                                TraktLogger.Info("Manual search selected");
#endif
                                VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
                                //keyboard.IsSearchKeyboard = true;
                                keyboard.Reset();
                                keyboard.Text = EbasicHandler.getCurrentProgram().Title;
                                keyboard.DoModal(GUIWindowManager.ActiveWindow);
                                if (keyboard.IsConfirmed)
                                {
                                    //PopulateControlWithShows(keyboard.Text);
                                    //PopulateListControl(TraktAPI.TraktAPI.SearchShowByName(keyboard.Text));
                                    //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
                                    //OnMessage(msg);
                                    //GUIShowSelectGUI.setCurrentSearch(TraktAPI.TraktAPI.SearchByName(keyboard.Text));
                                    //GUIWindowManager.ActivateWindow((int)TraktGUIWindows.EPGShowSelect);
                                    GUIShowSelectGUI.callSearchGUI(TraktAPI.TraktAPI.SearchByName(keyboard.Text));
                                }
                            }
                            else //this handles back button
                            {
                                TraktLogger.Info("BackButtonDetected");
                                PopulateListControl(searchStack.Pop(), searchlevel--);
                                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
                                OnMessage(msg);
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
        public override void DoModal(int dwParentId)
        {
            PopulateListControl(currentSearch);
           // LoadSkin();
            base.DoModal(dwParentId);
        }
        public void SetHeading(string text, string text2)
        {
            headingGFX.Label = text;
            heading.Label = text2;
        }
        public override void Reset()
        {
            ClearControls();
            base.Reset();
        }
        #endregion

        private void ClearControls()
        {
            //resultListControl.Clear();
            currentSearch = null;
            searchlevel = 0;
            currentTvShow = null;
        }
        private void PopulateListControl(IEnumerable<TraktSearchResult> search, int searchLevel = 0)
        {
            resultListControl.Clear();
            listItems.Clear();
            GUIListItem item;
            if (searchlevel < 1)
            {
                //manualSearch button, it's first search
                item = new GUIListItem("Manual Search");
                item.TVTag = "FirstButton";
                Add(item);
            }
            else
            {
                item = new GUIListItem("Back");
                item.TVTag = "FirstButton";
                Add(item);
            }
            //Now populating items           
            IEnumerator<TraktSearchResult> p = search.GetEnumerator();
            while (p.MoveNext())
            {
                if (p.Current.Type == "show")
                {
                    item = new GUIListItem(p.Current.Show.Title);
                    //item.Label = p.Current.Show.Title;
                    //item.Label2 = p.Current.Type;
                    item.TVTag = p.Current.Show;
#if DEBUG
                    TraktLogger.Info("Adding '{0}' to item '{1}'. Type detected: '{2}'", p.Current.Show.Title, item.Label, p.Current.Type);
#endif
                    Add(item);
                }
                if (p.Current.Type == "season")
                {
                    item = new GUIListItem(string.Format("Season {0}", p.Current.Season.Number));
                    //item.Label = "Season " + p.Current.Season.Number;
                    // item.Label2 = p.Current.Season.Number.ToString();
                    item.TVTag = p.Current.Season;
#if DEBUG
                    TraktLogger.Info("adding Season '{0}' to the list as {1}. Object type: {2}", p.Current.Season.Number, p.Current.Type, p.Current.Season.ToString());
#endif
                    Add(item);
                }
                if (p.Current.Type == "episode")
                {
                    item = new GUIListItem(string.Format("Episode {0}: {1} ", p.Current.Episode.Number, p.Current.Episode.Title));
                    //item.Label = "Episode " + p.Current.Episode.Number + ": " + p.Current.Episode.Title;
                    item.TVTag = p.Current.Episode;
#if DEBUG
                    TraktLogger.Info("adding Episode '{0}' to the list", p.Current.Episode.Number);
#endif
                    Add(item);
                }
                if (p.Current.Type == "movie")
                {
                    item = new GUIListItem(p.Current.Movie.Title);
                    //   item.Label = p.Current.Movie.Title;
                    // item.Label2 = p.Current.Type;
                    item.TVTag = p.Current.Movie;
                    Add(item);
                }
            }
            currentSearch = search;
        }
    }
}