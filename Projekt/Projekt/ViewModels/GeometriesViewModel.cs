using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Models;
using NpgsqlTypes;

namespace Projekt.ViewModels
{
    public class GeometriesViewModel
    {
        public List<IBaseModel> Buildings { get; set; }
        public List<IBaseModel> UserGeometries { get; set; }

        public int SelectedBuildingId { get; set; }
        public string myWkt { get; set; }
        public string road { get; set; }

        public double Distance { get; set; }
        public PostgisLineString ShortestLine_Geometry { get; set; }
        public string ShortestLine_GeometryAsText { get; set; }
        public double Length { get; set; }

        //OVO SU POCETNE I KRAJNJE KOORDINATE DOBIVENE LINIJE (KOJA JE NAJKRACA)
        public double startX { get; set; }
        public double startY { get; set; }
        public double endX { get; set; }
        public double endY { get; set; }

        public string name { get; set; }
        public string enter { get; set; }
    }
}