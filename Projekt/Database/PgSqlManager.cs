using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Spatial;
using System.Text;
using Models;
using Npgsql;
using NpgsqlTypes;

namespace Database
{
    public class PgSqlManager
    {
        protected string Catalog;
        protected NpgsqlCommand Command;
        protected NpgsqlConnection Connection;
        protected readonly StringBuilder CommandText;

        public PgSqlManager()
        {
            Connection = new NpgsqlConnection(PgSqlCredentials.ConnectionString);
            Command = new NpgsqlCommand()
            {
                Connection = Connection
            };

            CommandText = new StringBuilder();
            Catalog = Connection.Database;
        }

        ///Functions to do with database DATA
        #region MAIN FUNCTIONS SPECIFIC TO PROJECT

        public List<IBaseModel> GetData(TableToChoose table)
        {
            string tableParameter = null;

            switch (table)
            {
                case TableToChoose.Buildings:
                    tableParameter = PgSqlCredentials.BuildingsTable;
                    break;
                case TableToChoose.Roads:
                    tableParameter = PgSqlCredentials.RoadsTable;
                    break;
                case TableToChoose.Results:
                    tableParameter = PgSqlCredentials.ResultTable;
                    break;
            }

            if (tableParameter == null)
                throw new Exception($"Something went wrong. NullException");

            CommandText.Append($"SELECT id,geom, name, ST_AsText(geom) FROM" + tableParameter);
            Command.CommandText = CommandText.ToString();

            var modelList = new List<IBaseModel>();
            try
            {
                OpenConnection();
                using (var dataReader = Command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        IBaseModel model = null;
                        switch (table)
                        {
                            case TableToChoose.Buildings:
                                model = new Building();
                                break;
                           
                        }

                        //Order of columns in database = ID, GEOM, NAME
                        model.Id = Convert.ToInt32(dataReader[0]);

                        //BUILDINGS ARE POLYGONS, ROADS ARE LINESTRINGS, RESULT IS LINESTRING
                        if(dataReader.GetValue(1).GetType() == typeof(PostgisLineString))
                            model.Geometry = (PostgisLineString)dataReader[1];
                        if (dataReader.GetValue(1).GetType() == typeof(PostgisPolygon))
                            model.Geometry = (PostgisPolygon)dataReader[1];

                        model.Name = dataReader[2].ToString();
                        model.GeometryAsText = dataReader[3].ToString();

                        modelList.Add(model);
                    }
                }
                CloseConnection();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not execute command. {e}");
            }

            Clear();
            return modelList;
        }

        public int InsertUserChoice(string enter)
        {
            string tableParameter = null;
            tableParameter = PgSqlCredentials.BuildingsTable;
            CommandText.Append($"INSERT INTO " + tableParameter + "(geom, name) VALUES ('"+enter+"','frontpokusaj');");
  
            Command.CommandText = CommandText.ToString();

            var modelList = new List<IBaseModel>();
            try
            {
                OpenConnection();
                using (var dataReader = Command.ExecuteReader())
                {

                }
                CloseConnection();
                
            }
            catch (Exception e)
            {
                CloseConnection();
                Clear();
                return 1;
                //throw new Exception($"Could not execute command. {e}" + Command.CommandText);
                
            }

            Clear();
            return 0;
        }

        //public double CalculateShortestDistance(string lineString, string polygon)
        //{
        //    CommandText.Append($"SELECT ST_Distance(" +
        //                       $"ST_GeomFromText('{lineString}')," +
        //                       $"ST_GeomFromText('{polygon}')" +
        //                       $")");
        //    Command.CommandText = CommandText.ToString();

        //    double result = 0;
        //    try
        //    {
        //        OpenConnection();
        //        using (var dataReader = Command.ExecuteReader())
        //        {
        //            while (dataReader.Read())
        //            {
        //                result = Convert.ToDouble(dataReader[0]);
        //            }
        //        }
        //        CloseConnection();
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception($"Could not execute command. {e}");
        //    }

        //    Clear();

        //    return result;
        //}

        public double CalculateLength(string mywkt)
        {

            string command = "SELECT CAST(ST_LENGTH(geom::geography) as real) as duljinaPrometnice" +
                        " FROM(" +
                           " SELECT id, name, geom, ST_DISTANCESPHERE(ST_GeomFromText('" + mywkt + "'), ST_GeomFromText(ST_AsText(geom))) distance" +
                           " FROM \"Projekt\".\"Roads\"" +
                           " ORDER BY distance asc" +
                           " LIMIT 1 ) sub";

            CommandText.Append(command);
            Command.CommandText = CommandText.ToString();

            double result = 0;
            try
            {
                OpenConnection();
                using (var dataReader = Command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result = Convert.ToDouble(dataReader[0]);
                    }
                }
                CloseConnection();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not execute command. {e}" + Command.CommandText);
            }

            Clear();

            return result;
        }
        public string GetRoad(string myWKT)
        {
            string command = "SELECT g FROM(" +
                "SELECT ST_AsText(Roads.geom) g, ST_DISTANCESPHERE(ST_GeomFromText('" + myWKT + "'), ST_GeomFromText(ST_AsText(geom))) distance"
               + " FROM \"Projekt\".\"Roads\" Roads"
               + " ORDER BY distance desc"
               + " LIMIT 1) shortest;";
            CommandText.Append(command);
            Command.CommandText = CommandText.ToString();

            string result = "";
            try
            {
                OpenConnection();
                using (var dataReader = Command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result = dataReader[0].ToString();
                    }
                }
                CloseConnection();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not execute command. {e}" + Command.CommandText);
            }

            Clear();

            return result;
        }

        public double CalculateShortestDistance( string myWKT)
        {
            string command= "SELECT ST_DISTANCESPHERE(ST_GeomFromText('" + myWKT+"'),ST_GeomFromText(ST_AsText(geom))) distance FROM \"Projekt\".\"Roads\" ORDER BY distance asc LIMIT 1";
            CommandText.Append(command);

            Command.CommandText = CommandText.ToString();

            double result = 0;
            try
            {
                OpenConnection();
                using (var dataReader = Command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result = Convert.ToDouble(dataReader[0]);
                    }
                }
                CloseConnection();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not execute command. {e}" + Command.CommandText);
            }

            Clear();

            return result;
        }

        public Tuple<PostgisLineString, string> GetShortestLine( string mywkt)
        {
            string command = "SELECT ST_ShortestLine(ST_GeomFromText('"+mywkt+"'),ST_GeomFromText( ST_AsText(sub.geom))),"+
                         " ST_AsText(ST_ShortestLine(ST_GeomFromText('" + mywkt + "'), ST_GeomFromText(ST_AsText(sub.geom))))" +
                         " FROM("+
                            " SELECT id, name, geom, ST_DISTANCESPHERE(ST_GeomFromText('" + mywkt + "'), ST_GeomFromText(ST_AsText(geom))) distance" +
                            " FROM \"Projekt\".\"Roads\""+
                            " ORDER BY distance asc"+
                            " LIMIT 1 ) sub";

            CommandText.Append(command);
            Command.CommandText = CommandText.ToString();

            Tuple<PostgisLineString, string> result = null;
            try
            {
                OpenConnection();
                using (var dataReader = Command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        result = Tuple.Create((PostgisLineString)dataReader[0], dataReader[1].ToString());
                    }
                }
                CloseConnection();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not execute command. {e}" + Command.CommandText);
            }

            Clear();

            return result;
        }
        //public Tuple<PostgisLineString, string> GetShortestLine(string lineString, string polygon)
        //{
        //    CommandText.Append($"SELECT ST_ShortestLine(ST_GeomFromText('{lineString}'), ST_GeomFromText('{polygon}')), " +
        //                       $"ST_AsText(ST_ShortestLine(ST_GeomFromText('{lineString}'), ST_GeomFromText('{polygon}')))");
        //    Command.CommandText = CommandText.ToString();

        //    Tuple<PostgisLineString, string> result = null;
        //    try
        //    {
        //        OpenConnection();
        //        using (var dataReader = Command.ExecuteReader())
        //        {
        //            while (dataReader.Read())
        //            {
        //                result = Tuple.Create((PostgisLineString) dataReader[0], dataReader[1].ToString());
        //            }
        //        }
        //        CloseConnection();
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception($"Could not execute command. {e}");
        //    }

        //    Clear();

        //    return result;
        //}

        #endregion



        protected  void OpenConnection()
        {
            try
            {
                Connection.Open();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not open connection. {e}");
            }
        }
        protected void CloseConnection()
            => Connection.Close();
        protected void Clear()
            => CommandText.Clear();
        protected void ExecuteCommandNonQuery(DbCommand command)
        {
            try
            {
                OpenConnection();
                command.Connection = Connection;
                command.ExecuteNonQuery();
                CloseConnection();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not execute. {e}");
            }
        }

    }
}