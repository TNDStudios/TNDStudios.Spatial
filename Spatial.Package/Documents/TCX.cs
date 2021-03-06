﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using TNDStudios.Spatial.Common;
using TNDStudios.Spatial.Helpers;

/// <summary>
/// https://www8.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd
/// </summary>
namespace TNDStudios.Spatial.Documents
{
    /// <summary>
    /// Implementation of https://www8.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd
    /// Folders, workouts, courses element(s) not mapped because we only care about the core activity data we can extract right now
    /// </summary>
    [Serializable]
    [XmlRoot("TrainingCenterDatabase", Namespace = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2")]
    public class TCXFile : XmlBase, IGeoFileConvertable
    {
        // ISO 8601 formatter instead of using roundtrip kind parsing as needed for read and write (get and set)
        public static String DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";

        [XmlElement("Activities")]
        public TCXActivities Activities { get; set; }

        [XmlElement("Author")]
        public TCXAbstractSource Author { get; set; }

        [XmlElement("Extensions")]
        public TCXExtensions Extensions { get; set; }

        public GeoFile ToGeoFile()
        {
            GeoFile result = new GeoFile();

            // Transform the activity to the route information
            result.Routes = this.Activities.Activity.Select(activity => new GeoFileRoute() { Name = activity.Id, Points = activity.ToCoords() }).ToList();

            return result;
        }

        /// <summary>
        /// Convert a GeoFile to the native format
        /// </summary>
        /// <param name="file">The GeoFile format to convert from</param>
        /// <returns>Success Or Failure flag</returns>
        public Boolean FromGeoFile(GeoFile file) => throw new NotImplementedException();
    }

    public class TCXAbstractSource : XmlBase
    {
        [XmlElement("Name")]
        public String Name { get; set; }

        [XmlElement("UnitId")]
        public String UnitID { get; set; }

        [XmlElement("ProductID")]
        public String ProductID { get; set; }

        [XmlElement("Version")]
        public TCXVersion Version { get; set; }

        [XmlElement("Build")]
        public TCXBuild Build { get; set; }

        [XmlElement("PartNumber")]
        public String PartNumber { get; set; }

        [XmlElement("LangID")]
        public String LangID { get; set; }
    }

    /// <summary>
    /// Extensions which is defined purposfully empty so XmlBase will
    /// pick up unmapped members
    /// </summary>
    public class TCXExtensions : XmlBase { }

    public class TCXActivities : XmlBase
    {
        [XmlElement("Activity")]
        public List<TCXActivity> Activity { get; set; }

        [XmlElement("MultiSportSession")]
        public TCXMultiSportSession MultiSportSession { get; set; }
    }

    public class TCXMultiSportSession : XmlBase
    {
        [XmlElement("Id")]
        public String Id { get; set; }

        [XmlElement("FirstSport")]
        public TCXFirstSport FirstSport { get; set; }

        [XmlElement("NextSport")]
        public List<TCXNextSport> NextSport { get; set; }

        [XmlElement("Notes")]
        public String Notes { get; set; }
    }

    public class TCXFirstSport : XmlBase
    {
        [XmlElement("Activity")]
        public TCXActivity Activity { get; set; }
    }

    public class TCXNextSport : XmlBase
    {
        [XmlElement("Transition")]
        public TCXActivityLap Transition { get; set; }

        [XmlElement("Activity")]
        public TCXActivity Activity { get; set; }
    }

    public class TCXActivityLap : XmlBase
    {
        [XmlAttribute("StartTime")]
        public String StartTime { get; set; }

        [XmlElement("TotalTimeSeconds")]
        public Double TotalTimeSeconds { get; set; }

        [XmlElement("DistanceMeters")]
        public Double DistanceMeters { get; set; }

        [XmlElement("MaximumSpeed")]
        public Double MaximumSpeed { get; set; }

        [XmlElement("Calories")]
        public Int32 Calories { get; set; }

        [XmlElement("AverageHeartRateBpm")]
        public TCXHeartRateInBeatsPerMinute AverageHeartRateBpm { get; set; }

        [XmlElement("MaximumHeartRateBpm")]
        public TCXHeartRateInBeatsPerMinute MaximumHeartRateBpm { get; set; }

        [XmlElement("Intensity")]
        public String Intensity { get; set; }

        [XmlElement("Cadence")]
        public Byte Cadence { get; set; }

        [XmlElement("TriggerMethod")]
        public String TriggerMethod { get; set; }

        [XmlElement("Track")]
        public TCXTrack Track { get; set; }

        [XmlElement("Notes")]
        public String Notes { get; set; }

        [XmlElement("Extensions")]
        public TCXExtensions Extensions { get; set; }
    }

    public class TCXTrack : XmlBase
    {
        [XmlElement("Trackpoint")]
        public List<TCXTrackPoint> TrackPoints { get; set; }

        public List<GeoCoordinateExtended> ToCoords()
            => TrackPoints
                .Select(trkpt => trkpt.ToCoord())
                .ToList().InfillPositions();
    }

    public class TCXTrackPoint : XmlBase
    {
        /// <summary>
        /// Creation/modification timestamp for element. Date and time in are in Univeral Coordinated Time (UTC), not local time! Conforms to ISO 8601 specification for date/time representation. Fractional seconds are allowed for millisecond timing in tracklogs.
        /// </summary>
        [XmlIgnore]
        public DateTime CreatedDateTime = DateTime.MinValue;
        [XmlElement("Time")]
        public String Time
        {
            get { return CreatedDateTime.ToString(TCXFile.DateTimeFormat); }
            set { this.CreatedDateTime = DateTime.Parse(value); }
        }

        [XmlElement("Position")]
        public TCXPosition Position { get; set; }

        [XmlElement("AltitudeMeters")]
        public Double AltitudeMeters { get; set; } = 0D;

        [XmlElement("DistanceMeters")]
        public Double DistanceMeters { get; set; } = 0D;

        [XmlElement("HeartRateBpm")]
        public TCXHeartRateInBeatsPerMinute HeartRateBpm { get; set; } = new TCXHeartRateInBeatsPerMinute();

        [XmlElement("Cadence")]
        public Byte Cadence { get; set; } = 0;

        [XmlElement("SensorState")]
        public String SensorState { get; set; } = String.Empty;

        [XmlElement("Extensions")]
        public TCXExtensions Extensions { get; set; } = new TCXExtensions();

        public GeoCoordinateExtended ToCoord()
            => (this.Position == null) ? new GeoCoordinateExtended(0, 0, this.AltitudeMeters, this.CreatedDateTime) { BadCoordinate = true } : new GeoCoordinateExtended(this.Position.LatitudeDegrees, this.Position.LongitudeDegrees, this.AltitudeMeters, this.CreatedDateTime);
    }

    public class TCXPosition : XmlBase
    {
        [XmlElement("LatitudeDegrees")]
        public Double LatitudeDegrees { get; set; }

        [XmlElement("LongitudeDegrees")]
        public Double LongitudeDegrees { get; set; }
    }

    public class TCXHeartRateInBeatsPerMinute : XmlBase
    {
        [XmlElement("Value")]
        public Byte Value { get; set; }
    }

    /// <summary>
    /// Activity type with the Training element dropped as we only care about getting to the movement data
    /// </summary>
    public class TCXActivity : XmlBase
    {
        [XmlElement("Id")]
        public String Id { get; set; }

        [XmlElement("Lap")]
        public List<TCXActivityLap> Laps { get; set; }

        [XmlElement("Notes")]
        public String Notes { get; set; }

        [XmlElement("Creator")]
        public TCXAbstractSource Creator { get; set; }

        [XmlElement("Extensions")]
        public TCXExtensions Extensions { get; set; }

        /// <summary>
        /// Convert the list of points to a list of common coordinates
        /// </summary>
        /// <returns></returns>
        public List<GeoCoordinateExtended> ToCoords()
        {
            List<GeoCoordinateExtended> merged = new List<GeoCoordinateExtended>();
            Laps.ForEach(lap => merged.AddRange(lap.Track.ToCoords()));

            // Infilling is done for each track but there could be bad coordinates left over at the start or end of tracks that
            // might still need dealing with so check first rather than always doing it.
            if (merged.Where(pt => pt.BadCoordinate).Count() > 0)
                return merged.InfillPositions(); // Infill the positions that might still exist in boundaries between tracks before returning
            else
                return merged; // No need for infilling as no bad coordinates
        }
    }

    public class TCXBuild : XmlBase
    {
        [XmlElement("Version")]
        public TCXVersion Version { get; set; }
    }

    public class TCXVersion : XmlBase
    {
        [XmlElement("VersionMajor")]
        public Byte VersionMajor { get; set; }

        [XmlElement("VersionMinor")]
        public Byte VersionMinor { get; set; }

        [XmlElement("BuildMajor")]
        public Byte BuildMajor { get; set; }

        [XmlElement("BuildMinor")]
        public Byte BuildMinor { get; set; }

    }
}