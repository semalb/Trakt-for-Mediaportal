using System;
using System.Threading;
using MediaPortal.Player;
using TvDatabase;
using TvPlugin;
using TraktPlugin.Extensions;
using TraktPlugin.Cache;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using TraktPlugin.GUI;
using TraktPlugin.TraktAPI.Extensions;
using TraktPlugin.TraktAPI.DataStructures;
using System.IO;
using MediaPortal.Configuration;


namespace TraktPlugin.TraktHandlers
{
   class MyETVLive : ITraktHandler
    {
        #region Variables
        Timer TraktTimer;
        bool TVJustTurnedOn = true;
        TraktEPGCacheRecord item = new TraktEPGCacheRecord();
        TraktScrobbleResponse response = new TraktScrobbleResponse();
        bool istStoppingScrobble = false;
        
        //private EVideoInfo CurrentProgram = null;
        #endregion

        #region Constructor

        public MyETVLive(int priority)
        {
            TraktLogger.Info("Initialising My TV plugin handler Enhanced With Localized EPG Search");
           
            Priority = priority;

        }


        #endregion

        #region ITraktHandler Members

        public string Name
        {
            get { return "My TV Live"; }
        }

        public int Priority { get; set; }

        public void SyncLibrary()
        {
            return;
        }

        public void setCurrentProgram(EVideoInfo program)
        {
            EbasicHandler.setCurrentProgram(program);
        }

        public bool Scrobble(string filename)
        {
            StopScrobble();

            if (!g_Player.IsTV) return false;

            EbasicHandler.setCurrentProgram(GetCurrentProgram());

            if (EbasicHandler.getCurrentProgram() == null) return false;
            EbasicHandler.SetCurrentProgramIsScrobbling(true);

            TVJustTurnedOn = true;
            if (EbasicHandler.getCurrentProgramType() == VideoType.Series)
            {
                TraktLogger.Info("Detected tv show playing on Live TV. Title = '{0}'", EbasicHandler.getCurrentProgram().Title.ToString());
            }
            else
            {
                TraktLogger.Info("Detected movie playing on Live TV. Title = '{0}'", EbasicHandler.getCurrentProgram().Title.ToString());
            }

            #region Scrobble Timer

            TraktTimer = new Timer(new TimerCallback((stateInfo) =>
            {
                Thread.CurrentThread.Name = "Scrobble";

                // get the current program airing on tv now
                // this may have changed since last status update on trakt
                EVideoInfo videoInfo = GetCurrentProgram();

                //Reinit all variables
                item = new TraktEPGCacheRecord();
                response = new TraktScrobbleResponse();

                // I have problems with GUI rendering most of the times. If i set the thread to sleep it really helps with GUI rendering.
                // This might also work well to handle zapping correctly.
                Thread.Sleep((TraktSettings.ETVScrobbleDelay)*1000);

                try
                { 
                    if (videoInfo != null)
                    {
                        // if we are watching something different, 
                        // check if we should mark previous as watched
                        //if (!videoInfo.Equals(CurrentProgram))

                        if (!videoInfo.Equals(EbasicHandler.getCurrentProgram()))
                        {
                            TraktLogger.Info("Detected new tv program has started. Previous Program = '{0}', New Program = '{1}'", EbasicHandler.getCurrentProgram().ToString(), videoInfo.ToString());
                            //The new program has changed. I should check if the active window is GUIShowSelect and eventually close it.
                            if (GUIWindowManager.ActiveWindow.Equals((int)TraktGUIWindows.EPGShowSelect))
                            {
                                GUIWindowManager.CloseCurrentWindow();
                            }
                            if (IsProgramWatched(EbasicHandler.getCurrentProgram()) && EbasicHandler.IsCurrentProgramScrobbling())
                            {
                                TraktLogger.Info("Playback of program on Live TV is considered watched. Title = '{0}'", EbasicHandler.getCurrentProgram().ToString());
                                BasicHandler.StopScrobble(EbasicHandler.getCurrentProgram(), true);
                            }
                            //The programs are different so we should start the whole scrobbling process.
                            //For that we set the new videoInfo scrobbling status to true
                            //later we're checking if videoInfo is scrobbling, and it should be false if we don't set it true here.
                            videoInfo.IsScrobbling = true;
                            //EbasicHandler.SetCurrentProgramIsScrobbling(true);
                        }

                        // continue watching new program
                        // dont try to scrobble if previous attempt failed
                        // If the current program is scrobbling, according to the APIARY there's no need to resend every 15 minutes, 
                        // it will expire after runtime has elapsed.

                        if ((videoInfo.IsScrobbling) | (TVJustTurnedOn))
                        {
                            TVJustTurnedOn = false;
                            //Starts the scrobble of the new program because it changed
                            //cache should search here because some shows have no name and could be in cache.

                            #region CACHE CHECK and SCROBBLE from CACHE
                            if (EPGCache.searchOnCache(videoInfo.Title, out item))
                            {
                                if (item.Type.Equals("nullRecord"))
                                {
                                    response.Code = 901;
                                    response.Description = "Manually set don't scrobble";
                                } 
                                else if ((item.Type.Equals("movie")) & (videoInfo.Type == VideoType.Movie))
                                {
                                    EbasicHandler.overrideVideoInfoProgram(ref videoInfo, item.Movie);
                                    response = EbasicHandler.StartScrobbleMovie(videoInfo);
                                }
                                else if ((item.Type.Equals("show")) & (videoInfo.Type == VideoType.Series))
                                { 
                                    EbasicHandler.overrideVideoInfoProgram(ref videoInfo, item.Show);
                                    response = EbasicHandler.StartScrobbleEpisode(videoInfo);
                                }
                                else
                                //The item type is different from the videoinfo type. Cache is authoritative here so overriding videoInfo with the cache.
                                //It's most likely that a show is detected wrong as a movie, because a movie with episode and season number is totally nonsense, but handles both.
                                {
                                    response.Code = 904; //This code will be used later.
                                    if (item.Type.Equals("show"))
                                    {
                                        EbasicHandler.overrideVideoInfoProgram(ref videoInfo, item.Show);
                                        response.Description = item.Show.Ids.Slug;
                                    }
                                    else EbasicHandler.overrideVideoInfoProgram(ref videoInfo, item.Movie);
                                }
                            }// end cache thingy if it was successful it should be scrobbled.
                            #endregion
                            #region CACHE MATCH UNSUCCESSFUL: TRY RETRIEVE ORIGINAL LANGUAGE AND SCROBBLE
                            else
                            {
                                #region FOUND ORIGINAL LANGUAGE TITLE WITH A SINGLE MATCH
                                // Try first to scrobble without changing anything. This is to avoid problems with shows that uses
                                // real non translated shows. In this case the name of the show usually is enough to succesfully scrobble.
                                if (videoInfo.Type == VideoType.Series)
                                {
                                    response = EbasicHandler.StartScrobbleEpisode(videoInfo);
                                }
                                else
                                {
                                    response = EbasicHandler.StartScrobbleMovie(videoInfo);
                                }
                                if (!(response.Code == 0) && EbasicHandler.setOriginalTitle(ref videoInfo))
                                {
                                    if ((videoInfo.Type == VideoType.Series))
                                    {
                                        response = EbasicHandler.StartScrobbleEpisode(videoInfo);
                                    }
                                    else
                                    {
                                        response = EbasicHandler.StartScrobbleMovie(videoInfo);
                                    }
                                }
                                #endregion
                                #region NO CACHE, NO ORIGINAL LANGUAGE SINGLE MATCH FOUND everything is very likely to fail.
                                else if (!response.Code.Equals(0))
                                {
                                    response.Code = 404;
                                    response.Description = "Not Found";
                                }
                                #endregion
                            }
                            #endregion
                            EbasicHandler.setCurrentProgram(videoInfo);
                            if (response.Code.Equals(901))
                            {
                                TraktLogger.Info("Program {0} skipped because manually marked to skip on cache", videoInfo.Title);
                            }
                            else if (response.Code.Equals(0))
                            {
                                if (TraktSettings.AllowPopUPOnSuccessfulScrobbling)
                                {
                                    GUIDialogNotify notification = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
                                    notification.Reset();
                                    notification.SetHeading(string.Format("{0} scrobbled!", EbasicHandler.getCurrentProgram().Type.ToString()));
                                    notification.SetText(string.Format("Scrobbling '{0}' as '{1}'", EbasicHandler.getCurrentProgram().getOriginalTitle(), EbasicHandler.getCurrentProgram().Title));
                                    notification.SetImage(Path.Combine(Config.GetFolder(Config.Dir.Skin), string.Format(@"{0}\Media\Logos\trakt.png", Config.SkinName)));
                                    notification.DoModal(GUIWindowManager.ActiveWindow);
                                    }
                            }
                            else // the response was bad! Everything gone worse than ever, again open the GUI
                            {
                                GUIDialogYesNo askManualSelection = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                                if (askManualSelection != null)
                                {
                                    askManualSelection.Reset();
                                    askManualSelection.SetHeading("Start manual matching?");
                                    askManualSelection.SetLine(1,string.Format("Cannot find a match for '{0}'", videoInfo.Title));
                                    askManualSelection.SetDefaultToYes(true);
                                    askManualSelection.DoModal(GUIWindowManager.ActiveWindow);
                                    if (askManualSelection.IsConfirmed)
                                    {
                                        if (!istStoppingScrobble) //maybe we can handle this better. If the user stops the program right before this the plugin crashes.
                                            EbasicHandler.StartGui(response.Code, item,TraktSettings.GUIAsDialog);
                                    }
                                    else
                                    {
                                        // Need to handle cancel button with a null on cache to avoid future scrobbling.
                                        EPGCache.addOnCache(videoInfo.Title);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (NullReferenceException exception)
                {
                    //handle the worst. This usually happens when the user stops the player during a dialog.
                    //We should at least log this in the mediaportal error.
                }
            }), null, 1000, 300000);

#endregion
            return true;
        }

        public void StopScrobble()
        {
            istStoppingScrobble = true;
            if (TraktTimer != null)
                TraktTimer.Dispose();


            if (EbasicHandler.getCurrentProgram() == null) return;

            if (IsProgramWatched(EbasicHandler.getCurrentProgram()) && EbasicHandler.IsCurrentProgramScrobbling())
            {
                TraktLogger.Info("Playback of program on Live TV is considered watched. Title = '{0}'", EbasicHandler.getCurrentProgram().ToString());
                BasicHandler.StopScrobble(EbasicHandler.getCurrentProgram(), true);
            }
            else
            {
                BasicHandler.StopScrobble(EbasicHandler.getCurrentProgram());
            }

            EbasicHandler.clearCurrentProgram();
        }

        public void SyncProgress()
        {
            return;
        }

#endregion

#region Helpers

        /// <summary>
        /// Checks if the current program is considered watched
        /// </summary>
        private bool IsProgramWatched(EVideoInfo program)
        {
            // check if we have watched atleast 80% of the program
            // this wont be an exact calculation +- 5mins due to the scrobble timer
            double durationPlayed = DateTime.Now.Subtract(program.StartTime).TotalMinutes;
            double percentPlayed = 0.0;
            if (program.Runtime > 0.0)
                percentPlayed = durationPlayed / program.Runtime;

            return percentPlayed >= 0.8;
        }
        /// <summary>
        /// Gets the current program
        /// </summary>
        /// <returns></returns>
        private EVideoInfo GetCurrentProgram()
        {
            EVideoInfo videoInfo = new EVideoInfo();

            // get current program details
            Program program = TVHome.Navigator.Channel.CurrentProgram;
            if (program == null || string.IsNullOrEmpty(program.Title))
            {
                TraktLogger.Info("Unable to get current program from database");
                return null;
            }
            else
            {
                string title = null;
                string year = null;
                BasicHandler.GetTitleAndYear(program.Title, out title, out year);

                videoInfo = new EVideoInfo
                {
                    Type = !string.IsNullOrEmpty(program.EpisodeNum) || !string.IsNullOrEmpty(program.SeriesNum) ? VideoType.Series : VideoType.Movie,
                    Title = title,
                    Year = year,
                    SeasonIdx = program.SeriesNum,
                    EpisodeIdx = program.EpisodeNum,
                    StartTime = program.StartTime,
                    Runtime = GetRuntime(program),
                };
                videoInfo.setVariables();
                TraktLogger.Info("Current program details. Title='{0}', Year='{1}', Season='{2}', Episode='{3}', StartTime='{4}', Runtime='{5}', isScrobbling='{6}'", videoInfo.Title, videoInfo.Year.ToLogString(), videoInfo.SeasonIdx.ToLogString(), videoInfo.EpisodeIdx.ToLogString(), videoInfo.StartTime == null ? "<empty>" : videoInfo.StartTime.ToString(), videoInfo.Runtime, videoInfo.IsScrobbling);
            }
            return videoInfo;
        }
        private double GetRuntime(Program program)
        {
            try
            {
                DateTime startTime = program.StartTime;
                DateTime endTime = program.EndTime;

                return endTime.Subtract(startTime).TotalMinutes;
            }
            catch
            {
                return 0.0;
            }
        }
#endregion
    }
}