using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database;
using Models;
using Npgsql;
using NpgsqlTypes;
using Projekt.ViewModels;
using JSM;

namespace Projekt.Controllers
{
    public class HomeController : Controller
    {
        private int _limit = 30;

        public ActionResult Index()
        {
            var manager = new PgSqlManager();

            var buildings = manager.GetData(TableToChoose.Buildings).Take(_limit).ToList();
            //var result = manager.GetData(TableToChoose.Results); Ne treba for now?
            
            var viewModel = new GeometriesViewModel()
            {
                Buildings = buildings,
                ShortestLine_Geometry = null,
                ShortestLine_GeometryAsText = null,
                enter = "",
                name= null
            };
            ViewBag.fail = "Unesite WKT";
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(GeometriesViewModel model)
        {
            //potrebno za punjenje modela
            var manager = new PgSqlManager();
            var buildings = manager.GetData(TableToChoose.Buildings).Take(_limit).ToList();

            //DAKLE IZ SELECTANIH STVARI (IZ DROPDOWN LISTA...SelectedRoadId SelectedBuildingId nadjemo sta trebamo i to prosljedjujemo u manager funkcije za racunanje distance i te linije koja je najkraca... liniju dobivamo kao NpgSqlGeometriju i kao string u obliku LINESTRING(long lat, long lat, itd il kako vec)
            var geom2 = buildings.Find(m => m.Id == model.SelectedBuildingId).GeometryAsText;

            var shortestDistance = manager.CalculateShortestDistance(geom2);
            var shortestLine_Geometry = manager.GetShortestLine(geom2).Item1;
            var shortestLine_GeometryAsText = manager.GetShortestLine(geom2).Item2;
            var road = manager.GetRoad(geom2);
            var length = manager.CalculateLength(geom2);

            //Pocetne i krajnje tocke tog najkraceg puta mozda treba na frontu
            model.startX = shortestLine_Geometry[0].X;
            model.startY = shortestLine_Geometry[0].Y;
            model.endX = shortestLine_Geometry[1].X;
            model.endY = shortestLine_Geometry[1].Y;

            //punjenje modela i vracanje u view
            model.Buildings = buildings;
            model.Distance = shortestDistance;
            model.ShortestLine_Geometry = shortestLine_Geometry;
            model.ShortestLine_GeometryAsText = shortestLine_GeometryAsText;
            model.Length = length;
            model.enter = "";
            model.name = null;
            model.myWkt = geom2;
            model.road = road;
      
            ViewBag.fail = "Unesite WKT";

            return View(model);
        }
        //[HttpPost]
        //public ActionResult Index(GeometriesViewModel model)
        //{
        //    //potrebno za punjenje modela
        //    var manager = new PgSqlManager();
        //    var roads = manager.GetData(TableToChoose.Roads).Take(_limit).ToList();
        //    var buildings = manager.GetData(TableToChoose.Buildings).Take(_limit).ToList();

        //    //DAKLE IZ SELECTANIH STVARI (IZ DROPDOWN LISTA...SelectedRoadId SelectedBuildingId nadjemo sta trebamo i to prosljedjujemo u manager funkcije za racunanje distance i te linije koja je najkraca... liniju dobivamo kao NpgSqlGeometriju i kao string u obliku LINESTRING(long lat, long lat, itd il kako vec)
        //    var geom1 = roads.Find(m => m.Id == model.SelectedRoadId).GeometryAsText;
        //    var geom2 = buildings.Find(m => m.Id == model.SelectedBuildingId).GeometryAsText;

        //    var shortestDistance = manager.CalculateShortestDistance(geom1, geom2);
        //    var shortestLine_Geometry = manager.GetShortestLine(geom1, geom2).Item1;
        //    var shortestLine_GeometryAsText = manager.GetShortestLine(geom1, geom2).Item2;

        //    //Pocetne i krajnje tocke tog najkraceg puta mozda treba na frontu
        //    model.startX = shortestLine_Geometry[0].X;
        //    model.startY = shortestLine_Geometry[0].Y;
        //    model.endX = shortestLine_Geometry[1].X;
        //    model.endY = shortestLine_Geometry[1].Y;

        //    //punjenje modela i vracanje u view
        //    model.Buildings = buildings;
        //    model.Roads = roads;
        //    model.Distance = shortestDistance;
        //    model.ShortestLine_Geometry = shortestLine_Geometry;
        //    model.ShortestLine_GeometryAsText = shortestLine_GeometryAsText;
        //    model.enter = "";
        //    model.name = null;
        //    //ViewBag.err = 0;

        //    return View(model);
        //}
        [HttpPost]
        //[HandleError]
        public ActionResult SaveWKT(GeometriesViewModel model)
        {
            //throw new Exception(enter + "is here");
            var manager = new PgSqlManager();
            int error=manager.InsertUserChoice(model.enter);
            if (error == 1) ViewBag.fail = "Nije validan WKT";
            else ViewBag.fail = "Vaš WKT je ispravno unesen, možete ga potražiti u listi WKT-ova";

            var buildings = manager.GetData(TableToChoose.Buildings).Take(_limit).ToList();
            //var result = manager.GetData(TableToChoose.Results); Ne treba for now?

            var viewModel = new GeometriesViewModel()
            {
                Buildings = buildings,
                ShortestLine_Geometry = null,
                ShortestLine_GeometryAsText = null,
                enter = "",
                name = null
            };

            //return View(model);
            return View("Index", viewModel);
            //Do something with formData
            //return RedirectToAction("Index");
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


    }
}