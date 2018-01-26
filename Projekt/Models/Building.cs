using System;
using System.Collections.Generic;
using System.Spatial;
using System.Text;
using NpgsqlTypes;


namespace Models
{
    public class Building: IBaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PostgisGeometry Geometry { get; set; }
        public string GeometryAsText { get; set; }
    }
}
