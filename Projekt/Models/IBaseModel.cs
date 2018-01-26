using System;
using System.Collections.Generic;
using System.Spatial;
using System.Text;
using NpgsqlTypes;

namespace Models
{
    public interface IBaseModel
    {
        int Id { get; set; }
        string Name { get; set; }
        PostgisGeometry Geometry { get; set; }
        string GeometryAsText { get; set; }

    }
}
