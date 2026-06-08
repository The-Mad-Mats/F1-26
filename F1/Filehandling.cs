using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace F1
{
    partial class ViewModel
    {
        private void SaveSettings()
        {
            var highLight = HighLightPerSeasons.FirstOrDefault(x => x.SeasonName == SeasonName);
            if (highLight != null)
            {
                highLight.Drivers.Clear();
                highLight.Drivers.AddRange(HighLights);
            }
            else
            {
                var hlps = new HighLightPerSeason
                {
                    SeasonName = SeasonName,
                    Drivers = new List<HighLight>()
                };
                hlps.Drivers.AddRange(HighLights);
                HighLightPerSeasons.Add(hlps);
            }
            var filename = "Names\\HighlightedDrivers.txt";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (var hl in HighLightPerSeasons)
                {
                    writer.WriteLine(hl.SeasonName);
                    foreach (var driver in hl.Drivers)
                    {
                        writer.WriteLine(driver.Name + "," + driver.Color + "," + driver.Human);
                    }
                }
            }

            filename = "Names\\Settings.txt";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(PortNumber.ToString() + ";" + SwitchingEnabled.ToString() + ";" + SwitchInterval + ";" + SeasonName);
            }
            var path = "Result\\" + SeasonName;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

        }
        //private void WriteData()
        //{
        //var filename = Track + " " + DateTime.Now.Date.ToLongDateString() + ".txt";
        //using (StreamWriter writer = new StreamWriter(filename))
        //{
        //    foreach (var highLight in HighLights)
        //    {
        //        var user = Users.FirstOrDefault(x => x.Name == highLight.Key);
        //        if (user != null)
        //        {
        //            writer.WriteLine(user.Name);
        //            foreach (var userLap in user.Laps)
        //            {
        //                TimeSpan span = TimeSpan.FromSeconds(userLap.Value.Time);
        //                var time = span.ToString(@"mm\:ss\:fff");
        //                writer.WriteLine(userLap.Key + ": " + time);
        //            }
        //        }
        //    }
        //}
        private void WriteFinalClassification(List<User> sortedList)
        {
            var path = "Result\\" + SeasonName;
            var filename = Track + " " + DateTime.Now.Date.ToShortDateString() + "_" + DateTime.Now.Hour.ToString("00") + "-" + DateTime.Now.Minute.ToString("00") + " " + Session + ".txt";
            //var filename = Track + " " + DateTime.Now.Date.ToShortDateString() + " " + Session + ".txt";
            var fullpath = Path.Combine(path, filename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (StreamWriter writer = new StreamWriter(fullpath))
            {
                if (_session == "5" ||
                    _session == "6" ||
                    _session == "7" ||
                    _session == "8" ||
                    _session == "9" ||
                    _session == "10" ||
                    _session == "11" ||
                    _session == "12" ||
                    _session == "13" ||
                    _session == "14")
                {
                    writer.WriteLine("Position, Driver, Team, Best, Gap");
                }
                if (_session == "15" ||
                    _session == "16" ||
                    _session == "17")
                {
                    writer.WriteLine("Position, Driver, Team, Grid, Stops, Best, Time, Points, Penalties, Penalty Time, Warnings");
                }

                if (_session == "5" ||
                    _session == "6" ||
                    _session == "7" ||
                    _session == "8" ||
                    _session == "9" ||
                    _session == "10" ||
                    _session == "11" ||
                    _session == "12" ||
                    _session == "13" ||
                    _session == "14")
                {
                    var Leader = sortedList[0];
                    foreach (var driver in sortedList)
                    {
                        var Position = driver.m_position;
                        var Driver = driver.Name;
                        var Team = driver.Team;
                        var Best = TimeSpan.FromMilliseconds(driver.m_bestLapTime).ToString(@"mm\:ss\:fff");
                        TimeSpan span = TimeSpan.FromMilliseconds(Leader.m_bestLapTime - driver.m_bestLapTime);
                        var Gap = span.ToString(@"ss\:fff");

                        string[] vars = { Position.ToString(), Driver, Team, Best.ToString(), Gap.ToString() };
                        writer.WriteLine(string.Join(",", vars));
                    }
                    var player = sortedList.FirstOrDefault(x => HighLights.Where(y => y.Human).Select(z => z.Name).Contains(x.Name));
                    if (player != null)
                    {
                        var opponent = sortedList.FirstOrDefault(x => !HighLights.Where(y => y.Human).Select(z => z.Name).Contains(x.Name));
                        TimeSpan diffspan = TimeSpan.FromMilliseconds(player.m_bestLapTime - opponent.m_bestLapTime);
                        var diff = diffspan.ToString(@"mm\:ss\:fff");
                        writer.WriteLine("Gap between players and AI: " + diff);
                    }
                }
                if (_session == "15" ||
                    _session == "16" ||
                    _session == "17")
                {
                    var Leader = sortedList[0];
                    foreach (var driver in sortedList)
                    {
                        var Position = driver.m_position;
                        var Driver = driver.Name;
                        var Team = driver.Team;
                        var Grid = driver.m_gridPosition;
                        var Stops = driver.NumberOfPitstops;
                        var Best = TimeSpan.FromMilliseconds(driver.m_bestLapTime).ToString(@"mm\:ss\:fff");
                        TimeSpan span = TimeSpan.FromSeconds(Leader.m_totalRaceTime - (driver.m_totalRaceTime + driver.m_penaltiesTime));
                        TimeSpan leaderspan = TimeSpan.FromSeconds(driver.m_totalRaceTime);
                        var Gap = Leader == driver ? leaderspan.ToString(@"mm\:ss\:fff") : span.ToString(@"mm\:ss\:fff");
                        var Points = driver.m_points;
                        var Penalties = driver.m_numPenalties;
                        var PenTime = driver.m_penaltiesTime;
                        var Warnings = driver.Warnings;

                        string[] vars = { Position.ToString(), Driver, Team, Grid.ToString(), Stops.ToString(),
                            Best.ToString(), Gap.ToString(), Points.ToString(), Penalties.ToString(), PenTime.ToString(), Warnings.ToString()  };
                        writer.WriteLine(string.Join(",", vars));
                    }
                    var player = sortedList.FirstOrDefault(x => HighLights.Where(y => y.Human).Select(z => z.Name).Contains(x.Name));
                    if (player != null)
                    {
                        var opponent = sortedList.FirstOrDefault(x => !HighLights.Where(y => y.Human).Select(z => z.Name).Contains(x.Name));
                        TimeSpan diffspan = TimeSpan.FromSeconds(player.m_totalRaceTime - opponent.m_totalRaceTime);
                        var diff = diffspan.ToString(@"mm\:ss\:fff");
                        writer.WriteLine("Gap between players and AI: " + diff);
                    }
                }
                //Check best laps
                var humanList = sortedList.Where(x => HighLights.Where(y => y.Human).Select(z => z.Name).Contains(x.Name) && x.m_bestLapTime != 0);
                var bestHuman = humanList.FirstOrDefault(y => y.m_bestLapTime == humanList.Min(x => x.m_bestLapTime));
                var bestLapCollection = ReadBestLaps();
                var bestLap = bestLapCollection.FirstOrDefault(x => x.Track == Track);
                if (_session == "5" ||
                    _session == "6" ||
                    _session == "7" ||
                    _session == "8" ||
                    _session == "9" ||
                    _session == "10" ||
                    _session == "11" ||
                    _session == "12" ||
                    _session == "13" ||
                    _session == "14")
                {
                    var blTimeSpan = TimeSpan.ParseExact(bestLap.QTime, @"m\:ss\:fff", CultureInfo.InvariantCulture);
                    if (bestHuman != null && bestHuman.m_bestLapTime < blTimeSpan.TotalMilliseconds)
                    {
                        bestLap.QTime = TimeSpan.FromMilliseconds(bestHuman.m_bestLapTime).ToString(@"m\:ss\:fff");
                        bestLap.QDriver = bestHuman.Name;
                        bestLap.QSeason = SeasonName;
                        SaveBestLaps(bestLapCollection);
                    }
                }
                if (_session == "15" ||
                    _session == "16" ||
                    _session == "17")
                {
                    var blTimeSpan = TimeSpan.ParseExact(bestLap.RTime, @"m\:ss\:fff", CultureInfo.InvariantCulture);
                    if (bestHuman != null && bestHuman.m_bestLapTime < blTimeSpan.TotalMilliseconds)
                    {
                        bestLap.RTime = TimeSpan.FromMilliseconds(bestHuman.m_bestLapTime).ToString(@"m\:ss\:fff");
                        bestLap.RDriver = bestHuman.Name;
                        bestLap.RSeason = SeasonName;
                        SaveBestLaps(bestLapCollection);
                    }
                }
            }
        }

        private void ReadData()
        {
            FileInfo info = new FileInfo("Names\\Drivers.txt");
            using (StreamReader reader = info.OpenText())
            {
                Drivers.Clear();
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(',');
                    Drivers.Add(Convert.ToInt32(lines[1]), lines[0].ToString());
                }
            }
            info = new FileInfo("Names\\Teams.txt");
            using (StreamReader reader = info.OpenText())
            {
                Teams.Clear();
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(',');
                    Teams.Add(new DataItem { Id = Convert.ToInt32(lines[1]), Name = lines[0].ToString() });
                }
            }
            info = new FileInfo("Names\\Tracks.txt");
            using (StreamReader reader = info.OpenText())
            {
                Tracks.Clear();
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(',');
                    Tracks.Add(Convert.ToInt32(lines[1]), lines[0].ToString());
                }
            }

            info = new FileInfo("Names\\Nationality.txt");
            using (StreamReader reader = info.OpenText())
            {
                Nationality.Clear();
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(',');
                    Nationality.Add(new DataItem { Id = Convert.ToInt32(lines[0]), Name = lines[1].ToString() });
                }
            }

            info = new FileInfo("Names\\Settings.txt");
            string tempSeasonName = string.Empty;
            using (StreamReader reader = info.OpenText())
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(';');
                    PortNumber = Convert.ToInt32(lines[0]);
                    SwitchingEnabled = Convert.ToBoolean(lines[1]);
                    SwitchInterval = Convert.ToInt32(lines[2]);
                    if (lines.Count() > 3)
                    {
                        tempSeasonName = lines[3];
                    }
                }
            }
            Seasons = new ObservableCollection<string>();
            if (Directory.Exists("Result"))
            {
                foreach (string dir in Directory.GetDirectories("Result").Select(Path.GetFileName).ToList())
                {
                    Seasons.Add(dir);
                }
                SelectedSeason = tempSeasonName;
            }
            ReadRaces();
            ReadQualis();

            info = new FileInfo("Names\\HighlightedDrivers.txt");
            HighLightPerSeasons = new ObservableCollection<HighLightPerSeason>();
            using (StreamReader reader = info.OpenText())
            {
                HighLights.Clear();
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(',');
                    if (lines.Length == 1)
                    {
                        var hl = new HighLightPerSeason();
                        hl.SeasonName = lines[0];
                        hl.Drivers = new List<HighLight>();
                        line = reader.ReadLine();
                        lines = line.Split(',');
                        hl.Drivers.Add(new HighLight { Name = lines[0], Color = lines[1].ToString(), Human = Convert.ToBoolean(lines[2]) });
                        line = reader.ReadLine();
                        lines = line.Split(',');
                        hl.Drivers.Add(new HighLight { Name = lines[0], Color = lines[1].ToString(), Human = Convert.ToBoolean(lines[2]) });
                        line = reader.ReadLine();
                        lines = line.Split(',');
                        hl.Drivers.Add(new HighLight { Name = lines[0], Color = lines[1].ToString(), Human = Convert.ToBoolean(lines[2]) });
                        line = reader.ReadLine();
                        lines = line.Split(',');
                        hl.Drivers.Add(new HighLight { Name = lines[0], Color = lines[1].ToString(), Human = Convert.ToBoolean(lines[2]) });
                        HighLightPerSeasons.Add(hl);
                    }
                }
                SeasonsInSettings = new ObservableCollection<string>();
                foreach (var hl in HighLightPerSeasons)
                {
                    SeasonsInSettings.Add(hl.SeasonName);
                    if (hl.SeasonName == tempSeasonName)
                    {
                        foreach (var driver in hl.Drivers)
                            HighLights.Add(driver);
                    }
                }
                SeasonName = tempSeasonName;
            }
        }

        public void ReloadHiglights()
        {
            foreach (var hl in HighLightPerSeasons)
            {
                if (hl.SeasonName == SeasonName)
                {
                    HighLights.Clear();
                    foreach (var driver in hl.Drivers)
                        HighLights.Add(driver);
                }
            }
        }
        private void ReadRaces()
        {
            Races = new ObservableCollection<string>();
            if (Directory.Exists("Result"))
            {
                var di = new DirectoryInfo(Path.Combine("Result", SelectedSeason));
                var files = di.GetFiles("*Race*.txt");
                foreach (var file in files.OrderBy(x => x.Name.Split()[1]))
                {
                    Races.Add(file.Name);
                }
            }

        }
        private void ReadQualis()
        {
            Qualifications = new ObservableCollection<string>();
            if (Directory.Exists("Result"))
            {
                var di = new DirectoryInfo(Path.Combine("Result", SelectedSeason));
                var files = di.GetFiles("*Qualifying*.txt");
                foreach (var file in files.OrderBy(x => x.Name.Split()[1]))
                {
                    Qualifications.Add(file.Name);
                }
            }

        }
        private List<BestLaps> ReadBestLaps()
        {
            var bestLapsCollection = new List<BestLaps>();
            FileInfo info = new FileInfo($"Result\\{SeasonName}\\BestLaps.txt");
            if (!info.Exists)
            {
                File.Copy("BestLaps.txt", $"Result\\{SeasonName}\\BestLaps.txt");

            }
            using (StreamReader reader = info.OpenText())
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(',');
                    var bl = new BestLaps();
                    bl.Track = lines[0];
                    bl.QTime = lines[1];
                    bl.QDriver = lines[2];
                    bl.QSeason = lines[3];
                    bl.RTime = lines[4];
                    bl.RDriver = lines[5];
                    bl.RSeason = lines[6];
                    bestLapsCollection.Add(bl);
                    var blTimeSpan = TimeSpan.ParseExact(bl.QTime, @"m\:ss\:fff", CultureInfo.InvariantCulture);

                }
            }
            var dt = new DataTable();
            dt.Columns.Add("Track");
            dt.Columns.Add("Q Time");
            dt.Columns.Add("Q Driver");
            dt.Columns.Add("Q Season");
            dt.Columns.Add("R Time");
            dt.Columns.Add("R Driver");
            dt.Columns.Add("R Season");
            foreach (var b in bestLapsCollection)
            {
                dt.Rows.Add(b.Track, b.QTime, b.QDriver, b.QSeason, b.RTime, b.RDriver, b.RSeason);
            }
            BestLaps = dt;
            return bestLapsCollection;
        }
        private void SaveBestLaps(List<BestLaps> bestLapsCollection)
        {
            using (StreamWriter writer = new StreamWriter($"Result\\{SeasonName}\\BestLaps.txt"))
            {
                foreach (BestLaps bl in bestLapsCollection)
                {
                    writer.WriteLine($"{bl.Track},{bl.QTime},{bl.QDriver},{bl.QSeason},{bl.RTime},{bl.RDriver},{bl.RSeason}");
                }
            }
        }

        private void ReadStandings(bool reloadPosition)
        {
            var standings = new Standings();
            standings.Driver = new List<DriverStanding> { };
            standings.Header = new HeaderStanding();
            standings.Header.Track = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Driver", "0"),
                new Tuple<string, string>("Total", "1")
            };
            var numberOfRaces = 0;
            var di = new DirectoryInfo(Path.Combine("Result", SelectedSeason));
            if (!Directory.Exists(Path.Combine("Result", SelectedSeason)))
            {
                Directory.CreateDirectory(Path.Combine("Result", SelectedSeason));
            }
            var files = di.GetFiles("*Race*.txt");
            foreach (var file in files.OrderBy(x => x.Name.Split()[1]))
            {
                numberOfRaces++;
                var filePart = file.Name.Split(' ');
                standings.Header.Track.Add(new Tuple<string, string>(filePart[0], filePart[1]));
                using (StreamReader reader = file.OpenText())
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;
                        var lines = line.Split(',');
                        if (lines[0] == "Position" || lines[0].StartsWith("Gap"))
                            continue;
                        var driver = standings.Driver.FirstOrDefault(x => x.Name == lines[1]);
                        if (driver == null)
                        {
                            if (reloadPosition)
                            {
                                var pr = new List<Tuple<string, int>>();
                                for (int index = 1; index < numberOfRaces; index++)
                                {
                                    pr.Add(new Tuple<string, int>(DateTime.MinValue.ToString(), 0));
                                }
                                pr.Add(new Tuple<string, int>(filePart[1], Convert.ToInt32(lines[0])));
                                standings.Driver.Add(new DriverStanding { Name = lines[1], Total = Convert.ToInt32(lines[7]), PerRace = pr });
                            }
                            else
                            {
                                var pr = new List<Tuple<string, int>>();
                                for (int index = 1; index < numberOfRaces; index++)
                                {
                                    pr.Add(new Tuple<string, int>(DateTime.MinValue.ToString(), 0));
                                }
                                pr.Add(new Tuple<string, int>(filePart[1], Convert.ToInt32(lines[7])));
                                standings.Driver.Add(new DriverStanding { Name = lines[1], Total = Convert.ToInt32(lines[7]), PerRace = pr });
                            }
                        }
                        else
                        {
                            if (reloadPosition)
                            {
                                driver.PerRace.Add(new Tuple<string, int>(filePart[1], Convert.ToInt32(lines[0])));
                            }
                            else
                            {
                                driver.PerRace.Add(new Tuple<string, int>(filePart[1], Convert.ToInt32(lines[7])));
                            }
                            driver.Total += Convert.ToInt32(lines[7]);
                        }
                    }
                }

            }
            var dt = new DataTable();
            foreach (string w in standings.Header.Track.OrderBy(x => x.Item2).Select(y => y.Item1.ToString()).ToArray())
            {
                if (dt.Columns.Contains(w))
                {
                    if (dt.Columns.Contains(w + "2"))
                    {
                        dt.Columns.Add(w + "3");
                    }
                    else
                    {
                        dt.Columns.Add(w + "2");
                    }
                }
                else
                {
                    dt.Columns.Add(w);

                }
            }
            foreach (var driver in standings.Driver.OrderByDescending(x => x.Total))
            {
                var l1 = new List<string>
                {
                    driver.Name,
                    driver.Total.ToString()
                };
                var q = driver.PerRace.OrderBy(x => x.Item1).Select(y => y.Item2.ToString()).ToArray();
                l1.AddRange(q);
                dt.Rows.Add(l1.ToArray());
            }
            ChamionshipStandings = dt;
        }

        private void ReadTeamStandings()
        {
            var standings = new TeamStandings();
            standings.Team = new List<TeamStanding> { };
            standings.Header = new HeaderStanding();
            standings.Header.Track = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Team", "0"),
                new Tuple<string, string>("Total", "1")
            };
            var numberOfRaces = 0;
            var di = new DirectoryInfo(Path.Combine("Result", SelectedSeason));
            if (!Directory.Exists(Path.Combine("Result", SelectedSeason)))
            {
                Directory.CreateDirectory(Path.Combine("Result", SelectedSeason));
            }
            var files = di.GetFiles("*Race*.txt");
            foreach (var file in files.OrderBy(x => x.Name.Split()[1]))
            {
                numberOfRaces++;
                var filePart = file.Name.Split(' ');
                standings.Header.Track.Add(new Tuple<string, string>(filePart[0], filePart[1]));
                using (StreamReader reader = file.OpenText())
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;
                        var lines = line.Split(',');
                        if (lines[0] == "Position" || lines[0].StartsWith("Gap"))
                            continue;
                        var team = standings.Team.FirstOrDefault(x => x.Name == lines[2]);
                        if (team == null)
                        {
                            var pr = new List<Tuple<string, string, int>>();
                            for (int index = 1; index < numberOfRaces; index++)
                            {
                                pr.Add(new Tuple<string, string, int>(DateTime.MinValue.ToString(), "", 0));
                            }
                            pr.Add(new Tuple<string, string, int>(filePart[1], filePart[2], Convert.ToInt32(lines[7])));
                            standings.Team.Add(new TeamStanding { Name = lines[2], Total = Convert.ToInt32(lines[7]), PerRace = pr });
                        }
                        else
                        {
                            var perRace = team.PerRace.FirstOrDefault(x => x.Item1 == filePart[1] && x.Item2 == filePart[2]);
                            if (perRace == null)
                            {
                                team.PerRace.Add(new Tuple<string, string, int>(filePart[1], filePart[2], Convert.ToInt32(lines[7])));
                            }
                            else
                            {
                                var newValue = perRace.Item3 + Convert.ToInt32(lines[7]);
                                team.PerRace.Remove(perRace);
                                team.PerRace.Add(new Tuple<string, string, int>(filePart[1], filePart[2], newValue));

                            }
                            team.Total += Convert.ToInt32(lines[7]);
                        }
                    }
                }
            }
            var dt = new DataTable();
            foreach (string w in standings.Header.Track.OrderBy(x => x.Item2).Select(y => y.Item1.ToString()).ToArray())
            {
                if (dt.Columns.Contains(w))
                {
                    if (dt.Columns.Contains(w + "2"))
                    {
                        dt.Columns.Add(w + "3");
                    }
                    else
                    {
                        dt.Columns.Add(w + "2");
                    }
                }
                else
                {
                    dt.Columns.Add(w);

                }
            }
            foreach (var driver in standings.Team.OrderByDescending(x => x.Total))
            {
                var l1 = new List<string>
                {
                    driver.Name,
                    driver.Total.ToString()
                };
                var q = driver.PerRace.OrderBy(x => x.Item1).Select(y => y.Item3.ToString()).ToArray();
                l1.AddRange(q);
                dt.Rows.Add(l1.ToArray());
            }
            TeamChamionshipStandings = dt;
        }

        private void ReadHeadToHead2()
        {
            //Nytt sätt H2H
            var head2heads = new Head2Heads();
            head2heads.Teams = new List<Head2Head>();
            var numberOfRaces = 0;
            var di = new DirectoryInfo(Path.Combine("Result", SelectedSeason));
            if (!Directory.Exists(Path.Combine("Result", SelectedSeason)))
            {
                Directory.CreateDirectory(Path.Combine("Result", SelectedSeason));
            }
            var files = di.GetFiles("*Race*.txt");
            var seasonResults = new List<List<List<string>>>();
            foreach (var file in files.OrderBy(x => x.Name.Split()[1]))
            {
                numberOfRaces++;
                var filePart = file.Name.Split(' ');
                var raceResults = new List<List<string>>();
                using (StreamReader reader = file.OpenText())
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;
                        var lines = line.Split(',');
                        if (lines[0] == "Position" || lines[0].StartsWith("Gap"))
                            continue;
                        raceResults.Add(lines.ToList());
                    }
                }
                seasonResults.Add(raceResults);
            }
            try
            {

                if (seasonResults.Count > 0)
                {
                    foreach (var seasonResult in seasonResults)
                    {
                        foreach (var driver in seasonResult)
                        {
                            var team = driver[2];
                            var d1 = driver[1];
                            var d2 = seasonResult.FirstOrDefault(x => x[2] == team && x[1] != driver[1])[1];
                            var existingTeam = head2heads.Teams.FirstOrDefault(x => x.Team == team && (x.Driver1 == d1 && x.Driver2 == d2) || (x.Driver1 == d2 && x.Driver2 == d1));
                            if (existingTeam == null)
                            {
                                existingTeam = new Head2Head
                                {
                                    Team = team,
                                    Driver1 = d1,
                                    Driver2 = d2,
                                };
                                head2heads.Teams.Add(existingTeam);
                            }
                        }
                        foreach (var team in head2heads.Teams)
                        {
                            var d1s = seasonResult.FirstOrDefault(x => x[1] == team.Driver1 && x[2] == team.Team)?[0];
                            var d2s = seasonResult.FirstOrDefault(x => x[1] == team.Driver2 && x[2] == team.Team)?[0];
                            if (d1s == null || d2s == null)
                                continue;
                            var d1 = Convert.ToInt32(d1s);
                            var d2 = Convert.ToInt32(d2s);

                            if (d1 < d2)
                            {
                                team.D1R++;
                            }
                            else
                            {
                                team.D2R++;
                            }

                        }
                    }


                    var filesq = di.GetFiles("*Quali*.txt");
                    var seasonResultsq = new List<List<List<string>>>();
                    foreach (var file in filesq.OrderBy(x => x.Name.Split()[1]))
                    {
                        numberOfRaces++;
                        var filePart = file.Name.Split(' ');
                        var raceResults = new List<List<string>>();
                        using (StreamReader reader = file.OpenText())
                        {
                            while (true)
                            {
                                string line = reader.ReadLine();
                                if (line == null)
                                    break;
                                var lines = line.Split(',');
                                if (lines[0] == "Position" || lines[0].StartsWith("Gap"))
                                    continue;
                                raceResults.Add(lines.ToList());
                            }
                        }
                        seasonResultsq.Add(raceResults);
                    }


                    foreach (var seasonResult in seasonResultsq)
                    {
                        foreach (var team in head2heads.Teams)
                        {
                            var d1s = seasonResult.FirstOrDefault(x => x[1] == team.Driver1 && x[2] == team.Team)?[0];
                            var d2s = seasonResult.FirstOrDefault(x => x[1] == team.Driver2 && x[2] == team.Team)?[0];
                            if (d1s == null || d2s == null)
                                continue;
                            var d1 = Convert.ToInt32(d1s);
                            var d2 = Convert.ToInt32(d2s);
                            if (d1 < d2)
                            {
                                team.D1Q++;
                            }
                            else
                            {
                                team.D2Q++;
                            }

                        }
                    }

                    var dt = new DataTable();
                    dt.Columns.Add("Team");
                    dt.Columns.Add("Driver 1");
                    dt.Columns.Add("Qualify D1");
                    dt.Columns.Add("Qualify D2");
                    dt.Columns.Add("Driver 2");

                    foreach (var row in head2heads.Teams)
                    {
                        var l = new List<string>();
                        l.Add(row.Team);
                        l.Add(row.Driver1);
                        l.Add(row.D1Q.ToString());
                        l.Add(row.D2Q.ToString());
                        l.Add(row.Driver2);
                        dt.Rows.Add(l.ToArray());
                    }
                    QualifyingH2H = dt;

                    var dtr = new DataTable();
                    dtr.Columns.Add("Team");
                    dtr.Columns.Add("Driver 1");
                    dtr.Columns.Add("Race D1");
                    dtr.Columns.Add("Race D2");
                    dtr.Columns.Add("Driver 2");

                    foreach (var row in head2heads.Teams)
                    {
                        var l = new List<string>();
                        l.Add(row.Team);
                        l.Add(row.Driver1);
                        l.Add(row.D1R.ToString());
                        l.Add(row.D2R.ToString());
                        l.Add(row.Driver2);
                        dtr.Rows.Add(l.ToArray());
                    }
                    RaceH2H = dtr;
                }
            }
            catch (Exception)
            {

            }
        }
        private void ReadRaceResult()
        {
            if (SelectedRace == null)
                return;
            var di = new DirectoryInfo(Path.Combine("Result", SelectedSeason));
            if (!Directory.Exists(Path.Combine("Result", SelectedSeason)))
            {
                Directory.CreateDirectory(Path.Combine("Result", SelectedSeason));
            }
            var file = di.GetFiles(SelectedRace).FirstOrDefault();
            var headers = new List<string>();
            var rows = new List<List<string>>();
            using (StreamReader reader = file.OpenText())
            {

                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(',').ToList();
                    if (lines[0] == "Position")
                    {
                        foreach (string linePart in lines)
                        {
                            headers.Add(linePart);
                        }
                    }
                    else if (lines[0].StartsWith("Gap"))
                    {
                        DiffInRace = lines[0];
                    }

                    else
                    {
                        rows.Add(lines.ToList());
                    }

                }
            }
            var dt = new DataTable();
            dt.Columns.Add("Position");
            dt.Columns.Add("Driver");
            dt.Columns.Add("Team");
            dt.Columns.Add("Grid");
            dt.Columns.Add("Stops");
            dt.Columns.Add("Best");
            dt.Columns.Add("Time");
            dt.Columns.Add("Points");
            dt.Columns.Add("Penalties");
            dt.Columns.Add("Penalty Time");
            if (headers.Count > 10)
                dt.Columns.Add("Warnings");

            foreach (var row in rows)
            {
                dt.Rows.Add(row.ToArray());
            }
            RaceResult = dt;

        }
        private void ReadQualiResult()
        {
            if (SelectedQuali == null)
                return;
            var di = new DirectoryInfo(Path.Combine("Result", SelectedSeason));
            if (!Directory.Exists(Path.Combine("Result", SelectedSeason)))
            {
                Directory.CreateDirectory(Path.Combine("Result", SelectedSeason));
            }
            var file = di.GetFiles(SelectedQuali).FirstOrDefault();
            var headers = new List<string>();
            var rows = new List<List<string>>();
            using (StreamReader reader = file.OpenText())
            {

                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    var lines = line.Split(',').ToList();
                    if (lines[0] == "Position")
                    {
                        foreach (string linePart in lines)
                        {
                            headers.Add(linePart);
                        }
                    }
                    else if (lines[0].StartsWith("Gap"))
                    {
                        DiffInQuali = lines[0];
                    }

                    else
                    {
                        rows.Add(lines.ToList());
                    }

                }
            }
            var dt = new DataTable();
            dt.Columns.Add("Position");
            dt.Columns.Add("Driver");
            dt.Columns.Add("Team");
            dt.Columns.Add("Best");
            dt.Columns.Add("Gap");

            foreach (var row in rows)
            {
                dt.Rows.Add(row.ToArray());
            }
            QualiResult = dt;

        }
        private void ReadNotes()
        {
            TrackNotes = new ObservableCollection<TrackNote>();
            FileInfo info = new FileInfo("Names\\Notes.txt");
            using (StreamReader reader = info.OpenText())
            {
                int index = 1;
                string line = reader.ReadToEnd();
                if (line == null)
                    return;
                var lines = line.Split(';');
                for (int i = 0; i < lines.Length-1; i += 2)
                {
                    TrackNotes.Add(new TrackNote { Id = index++, Track = lines[i].Trim(), Notes = lines[i + 1] });
                }
            }
        }
        private void SaveNotes()
        {
            var filename = "Names\\Notes.txt";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (var line in TrackNotes)
                {
                    writer.WriteLine(line.Track + ";" + line.Notes + ";");
                }
            }

        }
    }
}
