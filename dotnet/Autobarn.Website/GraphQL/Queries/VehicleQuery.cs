// Autobarn.Website/GraphQL/Queries/VehicleQuery.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.GraphQL.GraphTypes;
using GraphQL;
using GraphQL.Types;

namespace Autobarn.Website.GraphQL.Queries {
    public class VehicleQuery : ObjectGraphType {
        private readonly IAutobarnDatabase db;

        public VehicleQuery (IAutobarnDatabase db) {
            this.db = db;

            Field<ListGraphType<VehicleGraphType>> ("Vehicles", "Query to retrieve all Vehicles",
                resolve : GetAllVehicles);

            Field<VehicleGraphType> ("Vehicle", "Query to retrieve a specific Vehicle",
                new QueryArguments (MakeNonNullStringArgument ("registration", "The registration (licence plate) of the Vehicle")),
                resolve : GetVehicle);

            Field<ListGraphType<VehicleGraphType>> ("VehiclesByColor", "Query to retrieve all Vehicles matching the specified color",
                new QueryArguments (MakeNonNullStringArgument ("color", "The name of a color, eg 'blue', 'grey'")),
                resolve : GetVehiclesByColor);

            Field<ListGraphType<VehicleGraphType>> ("VehiclesByYear", "Query to retrieve all Vehicles manufactured in the given year",
                arguments : new QueryArguments (
                    MakeNonNullIntArgument ("year", "The year of manufacture, eg 1999, 2020"),
                    MakeNonNullStringArgument ("searchType", "How to search (older, exact, newer than) given year")
                ),
                resolve : GetVehiclesByYear);
        }

        private QueryArgument MakeNonNullStringArgument (string name, string description) {
            return new QueryArgument<NonNullGraphType<StringGraphType>> {
                Name = name,
                Description = description
            };
        }

        private QueryArgument MakeNonNullIntArgument (string name, string description) {
            return new QueryArgument<NonNullGraphType<IntGraphType>> {
                Name = name,
                Description = description
            };
        }

        private IEnumerable<Vehicle> GetAllVehicles (IResolveFieldContext<object> context) => db.ListVehicles ();

        private IEnumerable<Vehicle> GetVehiclesByColor (IResolveFieldContext<object> context) {
            var color = context.GetArgument<string> ("color");
            return db.ListVehicles ().Where (v => v.Color.Contains (color, StringComparison.InvariantCultureIgnoreCase));
        }

        private IEnumerable<Vehicle> GetVehiclesByYear (IResolveFieldContext<object> context) {
            var year = context.GetArgument<int> ("year");
            var searchType = context.GetArgument<string> ("searchType");
            if (searchType == "exact") {
                return db.ListVehicles ().Where (v => v.Year.Equals (year)).OrderBy (v => v.Year);
            } else if (searchType == "before") {
                return db.ListVehicles ().Where (v => v.Year < year).OrderBy (v => v.Year);
            } else if (searchType == "after") {
                return db.ListVehicles ().Where (v => v.Year > year).OrderBy (v => v.Year);
            } else {
                throw new ExecutionError ("Invalid searchType");
            }
        }

        private Vehicle GetVehicle (IResolveFieldContext<object> context) {
            var registration = context.GetArgument<string> ("registration");
            return db.ListVehicles ().Where (v => v.Registration.Equals (registration, StringComparison.InvariantCultureIgnoreCase)).First ();
        }
    }
}
