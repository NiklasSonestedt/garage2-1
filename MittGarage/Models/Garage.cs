﻿using System;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MittGarage.Models
{
    [Serializable()]
    class Garage : IEnumerable<Vehicle>
    {
        protected List<Vehicle> vehicles;

        protected const string PathTemplate = @"../..garage-{0}.bin";

        public string Id { get; private set; }

        public uint Capacity { get; protected set; }

        public uint FreeSpace
        {
            get { return Capacity - (uint)vehicles.Count; }
        }

        public IEnumerator<Vehicle> GetEnumerator()
        {
            return vehicles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Vehicle> ListVehicles()
        {
            return GetEnumerator();
        }

        #region --Queries--

        #region Add Vehicle
        public void Add(Vehicle vehicle)
        {
            if (vehicles.Count >= Capacity)
                throw new InvalidOperationException();
            vehicles.Add(vehicle);
        }
        #endregion Add Vehicle

        #region Remove Vehicle
        public void Remove(Vehicle v)
        {
            Remove(v.Id);
        }

        public void Remove(string key)
        {
            int ix;
            try
            {
                ix = vehicles.FindIndex(v => v.Id == key);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
            vehicles.RemoveAt(ix);
        }
        #endregion

        #region Reg Query Summary
        /// <summary>
        /// Find vehicle with given registration number
        /// </summary>
        /// <returns>
        /// Found Vehicle or null if no match.
        /// </returns>
        /// <exception cref='InvalidOperationException'>
        /// Thrown if there are more than one vehicle with given number.
        /// </exception>
        #endregion

        #region Registration Query
        public Vehicle FindByRegNr(string regNr)
        {
            return vehicles
                        .Where(v => v.RegNr == regNr)
                        .SingleOrDefault();
        }
        #endregion

        #region Reg Query
        public Vehicle FindById(string id)
        {
            return vehicles
                        .Where(v => v.Id == id)
                        .Single();
        }
        #endregion Reg Query

        #region CountByType Query
        public IDictionary<VehicleType, int> CountByType()
        {
            return vehicles
                        .GroupBy(v => v.Type)
                        .ToDictionary(g => g.Key, g => g.Count());
        }
        #endregion ´CountByType Query

        #region Search Query Summery
        /// <summary>
        /// Search for vehicles matching given attributes, return possibly 
        /// empty array of matches.
        /// </summary>
        /// <param name='owner'>
        /// Optional: owner to match, or null for don't care.
        /// </param>
        /// <param name='regNr'>
        /// Optional: registration nr to match, or null for don't care.
        /// </param>
        /// <param name='type'>
        /// Optional: vehicle type or null for don't care.
        /// </param>
        /// <param name='color'>
        /// Optional: color to match, or Color.None for any color.
        /// </param>
        #endregion Search Query Summary

        #region Specific Search Queries
        public Vehicle[] Search(string owner = null,
                                string regNr = null,
                                VehicleType type = VehicleType.none,
                                ColorType color = ColorType.none)
        {
            return vehicles
                      .Where(v => owner == null || v.Owner == owner)
                      .Where(v => regNr == null || v.RegNr == regNr)
                      .Where(v => type == VehicleType.none || v.Type == type)
                      .Where(v => color == ColorType.none || v.Color == color)
                      .OrderBy (v => v.checkInDate)
                      .ToArray();
        }

        #endregion Specific Search Queries

        #endregion --Queries--

        #region --Save & Load-- 

        #region Save Garage
        public void store()
        {
            string path = String.Format(PathTemplate, Id);
            BinaryFormatter serializer = new BinaryFormatter();
            Stream f = null;
            try
            {
                f = File.Create(path);
                serializer.Serialize(f, this);
            }
            finally
            {
                if (f != null) f.Close();
            }
        }
        #endregion

        #region Load Garage
        public static Garage load(string id)
        {
            string path = String.Format(PathTemplate, id);
            if (!File.Exists(path))
            {
                throw new InvalidOperationException("No dump stored for " + id);
            }
            BinaryFormatter formatter = new BinaryFormatter();
            Stream s = null;
            try
            {
                s = File.OpenRead(path);
                return (Garage)formatter.Deserialize(s);
            }
            finally
            {
                if (s != null) s.Close();
            }

        }
        #endregion

        #endregion

        #region ClearDump
        public void ClearDump()
        {
            string path = String.Format(PathTemplate, Id);
            if (File.Exists(path)) File.Delete(path);
        }
        #endregion

        #region Garage Capacity, ID and Vehicles
        public Garage(string id, uint capacity)
        {
            this.Capacity = capacity;
            vehicles = new List<Vehicle>();
            this.Id = id;
        }
        #endregion
    }
}