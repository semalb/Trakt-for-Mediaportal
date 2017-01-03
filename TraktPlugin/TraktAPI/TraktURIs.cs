﻿
namespace TraktPlugin.TraktAPI
{
    /// <summary>
    /// List of URIs for the Trakt API
    /// Staging:    https://api.staging.trakt.tv
    /// Production: https://api-v2launch.trakt.tv
    /// </summary>
    public static class TraktURIs
    {
        public const string Login = "https://api-v2launch.trakt.tv/auth/login";

        public const string SyncLastActivities = "https://api-v2launch.trakt.tv/sync/last_activities";

        public const string SyncPausedMovies = "https://api-v2launch.trakt.tv/sync/playback/movies";
        public const string SyncPausedEpisodes = "https://api-v2launch.trakt.tv/sync/playback/episodes";

        public const string SyncCollectionMovies = "https://api-v2launch.trakt.tv/sync/collection/movies";
        public const string SyncWatchedMovies = "https://api-v2launch.trakt.tv/sync/watched/movies";
        public const string SyncRatedMovies = "https://api-v2launch.trakt.tv/sync/ratings/movies";

        public const string SyncCollectionEpisodes = "https://api-v2launch.trakt.tv/sync/collection/shows";
        public const string SyncWatchedEpisodes = "https://api-v2launch.trakt.tv/sync/watched/shows";
        public const string SyncRatedEpisodes = "https://api-v2launch.trakt.tv/sync/ratings/episodes";
        public const string SyncRatedSeasons = "https://api-v2launch.trakt.tv/sync/ratings/seasons";
        public const string SyncRatedShows = "https://api-v2launch.trakt.tv/sync/ratings/shows";

        public const string SyncCollectionAdd = "https://api-v2launch.trakt.tv/sync/collection";
        public const string SyncCollectionRemove = "https://api-v2launch.trakt.tv/sync/collection/remove";
        public const string SyncWatchedHistoryAdd = "https://api-v2launch.trakt.tv/sync/history";
        public const string SyncWatchedHistoryRemove = "https://api-v2launch.trakt.tv/sync/history/remove";
        public const string SyncRatingsAdd = "https://api-v2launch.trakt.tv/sync/ratings";
        public const string SyncRatingsRemove = "https://api-v2launch.trakt.tv/sync/ratings/remove";
        public const string SyncWatchlistAdd = "https://api-v2launch.trakt.tv/sync/watchlist";
        public const string SyncWatchlistRemove = "https://api-v2launch.trakt.tv/sync/watchlist/remove";

        public const string UserLists = "https://api-v2launch.trakt.tv/users/{0}/lists";
        public const string UserListItems = "https://api-v2launch.trakt.tv/users/{0}/lists/{1}/items?extended={2}";
        
        public const string UserListAdd = "https://api-v2launch.trakt.tv/users/{0}/lists";
        public const string UserListEdit = "https://api-v2launch.trakt.tv/users/{0}/lists/{1}";

        public const string UserListItemsAdd = "https://api-v2launch.trakt.tv/users/{0}/lists/{1}/items";
        public const string UserListItemsRemove = "https://api-v2launch.trakt.tv/users/{0}/lists/{1}/items/remove";

        public const string UserListLike = "https://api-v2launch.trakt.tv/users/{0}/lists/{1}/like";

        public const string UserWatchlistMovies = "https://api-v2launch.trakt.tv/users/{0}/watchlist/movies?extended={1}";
        public const string UserWatchlistShows = "https://api-v2launch.trakt.tv/users/{0}/watchlist/shows?extended={1}";
        public const string UserWatchlistSeasons = "https://api-v2launch.trakt.tv/users/{0}/watchlist/seasons?extended={1}";
        public const string UserWatchlistEpisodes = "https://api-v2launch.trakt.tv/users/{0}/watchlist/episodes?extended={1}";

        public const string UserProfile = "https://api-v2launch.trakt.tv/users/{0}?extended=full,images";
        public const string UserFollowerRequests = "https://api-v2launch.trakt.tv/users/requests?extended=full,images";
        public const string UserStats = "https://api-v2launch.trakt.tv/users/{0}/stats";

        public const string UserWatchedHistoryMovies = "https://api-v2launch.trakt.tv/users/{0}/history/movies?extended=full&page={1}&limit={2}";
        public const string UserWatchedHistoryEpisodes = "https://api-v2launch.trakt.tv/users/{0}/history/episodes?extended=full&page={1}&limit={2}";
        
        public const string UserCollectedMovies = "https://api-v2launch.trakt.tv/users/{0}/collection/movies?extended=full";
        public const string UserCollectedShows = "https://api-v2launch.trakt.tv/users/{0}/collection/shows?extended=full";

        public const string UserComments = "https://api-v2launch.trakt.tv/users/{0}/comments/{1}/{2}?extended={3}&page={4}&limit={5}";

        public const string UserLikedItems = "https://api-v2launch.trakt.tv/users/likes/{0}?extended={1}&page={2}&limit={3}";

        public const string RecommendedMovies = "https://api-v2launch.trakt.tv/recommendations/movies?extended={0}";
        public const string RecommendedShows = "https://api-v2launch.trakt.tv/recommendations/shows?extended=full";

        public const string RelatedMovies = "https://api-v2launch.trakt.tv/movies/{0}/related?extended=full&limit={1}";
        public const string RelatedShows = "https://api-v2launch.trakt.tv/shows/{0}/related?extended=full&limit={1}";

        public const string TrendingMovies = "https://api-v2launch.trakt.tv/movies/trending?extended=full&page={0}&limit={1}";
        public const string TrendingShows = "https://api-v2launch.trakt.tv/shows/trending?extended=full&page={0}&limit={1}";

        public const string PopularMovies = "https://api-v2launch.trakt.tv/movies/popular?extended=full&page={0}&limit={1}";
        public const string PopularShows = "https://api-v2launch.trakt.tv/shows/popular?extended=full&page={0}&limit={1}";

        public const string AnticipatedMovies = "https://api-v2launch.trakt.tv/movies/anticipated?extended=full&page={0}&limit={1}";
        public const string AnticipatedShows = "https://api-v2launch.trakt.tv/shows/anticipated?extended=full&page={0}&limit={1}";

        public const string BoxOffice = "https://api-v2launch.trakt.tv/movies/boxoffice?extended=full";

        public const string MovieComments = "https://api-v2launch.trakt.tv/movies/{0}/comments?extended=full&page={1}&limit={2}";
        public const string ShowComments = "https://api-v2launch.trakt.tv/shows/{0}/comments?extended=full&page={1}&limit={2}";
        public const string SeasonComments = "https://api-v2launch.trakt.tv/shows/{0}/seasons/{1}/comments?extended=full&page={2}&limit={3}";
        public const string EpisodeComments = "https://api-v2launch.trakt.tv/shows/{0}/seasons/{1}/episodes/{2}/comments?extended=full&page={3}&limit={4}";

        public const string CommentLike = "https://api-v2launch.trakt.tv/comments/{0}/like";
        public const string CommentReplies = "https://api-v2launch.trakt.tv/comments/{0}/replies";

        public const string SearchMovies = "https://api-v2launch.trakt.tv/search?query={0}&type=movie&page={1}&limit={2}?extended=full";
        public const string SearchShows = "https://api-v2launch.trakt.tv/search?query={0}&type=show&page={1}&limit={2}?extended=full";
        public const string SearchEpisodes = "https://api-v2launch.trakt.tv/search?query={0}&type=episode&page={1}&limit={2}?extended=full";
        public const string SearchPeople = "https://api-v2launch.trakt.tv/search?query={0}&type=person&page={1}&limit={2}?extended=full";
        public const string SearchUsers = "https://api-v2launch.trakt.tv/search?query={0}&type=user&page={1}&limit={2}?extended=full";
        public const string SearchLists = "https://api-v2launch.trakt.tv/search?query={0}&type=list&page={1}&limit={2}?extended=full";
        public const string SearchById = "https://api-v2launch.trakt.tv/search?id_type={0}&id={1}";
        
        //Added for epg handler
       // public const string SearchMovieByName = "https://api-v2launch.trakt.tv/search?query={0}&type=movie";
       // public const string SearchShowByName = "https://api-v2launch.trakt.tv/search?query={0}&type=show";
        //public const string SearchByName = "https://api-v2launch.trakt.tv/search?query={0}&type={1}&fields={2}";
        public const string SearchByName = "https://api-v2launch.trakt.tv/search/{0}?query={1}&fields={2}"; //this is final

        public const string NetworkFriends = "https://api-v2launch.trakt.tv/users/{0}/friends?extended=full,images";
        public const string NetworkFollowers = "https://api-v2launch.trakt.tv/users/{0}/followers?extended=full,images";
        public const string NetworkFollowing = "https://api-v2launch.trakt.tv/users/{0}/following?extended=full,images";

        public const string NetworkFollowRequest = "https://api-v2launch.trakt.tv/users/requests/{0}";
        public const string NetworkFollowUser = "https://api-v2launch.trakt.tv/users/{0}/follow";

        public const string ShowSummary = "https://api-v2launch.trakt.tv/shows/{0}?extended=full";
        public const string MovieSummary = "https://api-v2launch.trakt.tv/movies/{0}?extended=full";
        public const string EpisodeSummary = "https://api-v2launch.trakt.tv/shows/{0}/seasons/{1}/episodes/{2}?extended=full";

        public const string ShowSeasons = "https://api-v2launch.trakt.tv/shows/{0}/seasons?extended={1}";
        public const string SeasonEpisodes = "https://api-v2launch.trakt.tv/shows/{0}/seasons/{1}?extended=full";

        public const string CalendarShows = "https://api-v2launch.trakt.tv/calendars/shows/{0}/{1}?extended=full";
        public const string CalendarPremieres = "https://api-v2launch.trakt.tv/calendars/shows/premieres/{0}/{1}?extended=full";
        public const string CalendarNewPremieres = "https://api-v2launch.trakt.tv/calendars/shows/new/{0}/{1}?extended=full";

        public const string ScrobbleStart = "https://api-v2launch.trakt.tv/scrobble/start";
        public const string ScrobblePause = "https://api-v2launch.trakt.tv/scrobble/pause";
        public const string ScrobbleStop = "https://api-v2launch.trakt.tv/scrobble/stop";

        public const string ShowRatings = "https://api-v2launch.trakt.tv/shows/{0}/ratings";
        public const string SeasonRatings = "https://api-v2launch.trakt.tv/shows/{0}/seasons/{1}/ratings";
        public const string EpisodeRatings = "https://api-v2launch.trakt.tv/shows/{0}/seasons/{1}/episodes/{2}/ratings";

        public const string PersonMovieCredits = "https://api-v2launch.trakt.tv/people/{0}/movies?extended=full";
        public const string PersonShowCredits = "https://api-v2launch.trakt.tv/people/{0}/shows?extended=full";
        public const string PersonSummary = "https://api-v2launch.trakt.tv/people/{0}?extended=full";

        public const string MoviePeople = "https://api-v2launch.trakt.tv/movies/{0}/people?extended=full";
        public const string ShowPeople = "https://api-v2launch.trakt.tv/shows/{0}/people?extended=full";

        public const string ShowUpdates = "https://api-v2launch.trakt.tv/shows/updates/{0}?page={1}&limit={2}";
        public const string MovieUpdates = "https://api-v2launch.trakt.tv/movies/updates/{0}?page={1}&limit={2}";

        public const string DismissRecommendedMovie = "https://api-v2launch.trakt.tv/recommendations/movies/{0}";
        public const string DismissRecommendedShow = "https://api-v2launch.trakt.tv/recommendations/shows/{0}";

        public const string DeleteList = "https://api-v2launch.trakt.tv/users/{0}/lists/{1}";
    }
}
