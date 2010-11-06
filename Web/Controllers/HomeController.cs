﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Reporting;
using Web.Models;
using Web.Infrastructure.Session;
using TvdbLib.Data;

namespace Web.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        SiteDB _db;
        IUserSession _userSession;

        public HomeController(IUserSession UserSession)
        {
            _db = new SiteDB();
            _userSession = UserSession;
        }

        public ActionResult Index()
        {
            WatchedList model = new WatchedList(_db, _userSession.GetCurrentUserId());

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(string searchSeriesName)
        {
            //search for series.
            WatchedList model = new WatchedList(_db, _userSession.GetCurrentUserId());
            model.SearchResults = TVDBRepository.GetTvdbHandler().SearchSeries(searchSeriesName);
            
            //System.Web.Mvc.Html.FormExtensions.BeginForm(decimal,

            return View(model);
        }

        [HttpPost]
        public ActionResult WatchSeries(string SeriesIds)
        {
            long userId = _userSession.GetCurrentUserId();
            
            //add list of seriesIds to Watch list.
            foreach (string sId in SeriesIds.Split(','))
            {
                int iId;
                if (int.TryParse(sId, out iId))
                {
                    //make sure each Series is in our local db.
                    Series series = SeriesRepository.AddSeries(_db, userId, iId);
                    //add this series to this user's watch list.
                    WatchedRepository.WatchSeries(_db, userId, series.SeriesId);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult WatchEpisodes(long SelectedSeriesId, long[] EpisodeIds)
        {
            long userId = _userSession.GetCurrentUserId();

            WatchedRepository.SetWatched(_db, SelectedSeriesId, EpisodeIds, userId);

            return RedirectToAction("Series", new { Id = SelectedSeriesId });
        }

        /// <summary>
        /// Make watched using Ajax.
        /// </summary>
        /// <param name="EpisodeId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WatchEpisode(long EpisodeId, bool Watched)
        {
            long userId = _userSession.GetCurrentUserId();

            WatchedRepository.SetWatched(_db, EpisodeId, userId, Watched);

            return null;
        }

        /// <summary>
        /// Make watched using Ajax.
        /// </summary>
        /// <param name="EpisodeId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WatchSeason(long SeriesId, int Season, bool Watched)
        {
            long userId = _userSession.GetCurrentUserId();

            WatchedRepository.SetWatched(_db, SeriesId, Season, userId, Watched);

            return null;
        }

        public ActionResult Series(long Id)
        {
            WatchedList model = new WatchedList(_db, _userSession.GetCurrentUserId(), Id);

            return View("Index", model);
        }

        public ActionResult About()
        {
            return View();
        }
    }

    public class WatchedList
    {
        public long UserId { get; set; }
        public long SelectedSeriesId { get; set; }
        public List<WatchedSeries> WatchedSerieses { get; set; }
        public List<WatchedEpisodeStatus> WatchedEpisodes { get; set; }
        public List<TvdbSearchResult> SearchResults { get; set; }

        public WatchedList(SiteDB db, long UserId)
        {
            this.UserId = UserId;
            WatchedSerieses = WatchedRepository.GetAllSeries(db, UserId).ToList();
        }

        public WatchedList(SiteDB db, long UserId, long SeriesId)
        {
            SelectedSeriesId = SeriesId;
            WatchedSerieses = WatchedRepository.GetAllSeries(db, UserId).ToList();
            WatchedEpisodes = WatchedRepository.GetAllEpisodes(db, UserId, SeriesId);
        }
    }
}
