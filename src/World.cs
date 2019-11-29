﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SimEarth2020
{
    public class World
    {
        public World(MainWindow m)
        {
            Controller = m;
        }

        private UIElementCollection Cells
        {
            get
            {
                return Controller.WorldGrid.Children;
            }
        }
        private double age;
        private int energy;

        internal MainWindow Controller { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get => Width; }

        public double Age
        {
            get => age;
            set
            {
                age = value; Controller.RaisePropertyChanged("TitleString");
            }
        }
        public double Radius { get; set; }
        private string GetAge()
        {
            string[] prefixes = new string[] { "", "k", "M", "G", "T" };
            int index = 0;
            double age = Age;
            while (age >= 1000 && index < prefixes.Length - 1)
            {
                index++;
                age /= 1000;
            }
            return $"{age} {prefixes[index]}";
        }
        public override string ToString()
        {
            return $"{Name}: {GetAge()}yr";
        }
        private long tick = 0;

        public Cell[,] cells;
        internal void Tick()
        {
            var start = DateTime.Now;
            Age += Controller.Speed;
            Energy = Math.Min(MaxEnergy, Energy + GetProducedEnergy());

            foreach (var cell in cells)
            {
                if (cell.Animal == null)
                    continue;

                // 1) Move
                var (x, y) = cell.GetMoveCandidate(tick, rand);
                if (cells[x, y].Animal != null)
                {
                    // already occupied, don't move.
                }
                else
                {
                    if (cells[x, y].Terrain.Kind == TerrainKind.Ocean && !cell.Animal.Stats.CanSwim)
                        continue;

                    if (cells[x, y].Terrain.Kind != TerrainKind.Ocean && !cell.Animal.Stats.CanWalk)
                        continue;

                    Debug.WriteLine($"Moving {cell.Animal.Kind}(LT {cell.Animal.LastTick}) from ({cell.X}, {cell.Y}) to ({x}, {y})");
                    cells[x, y].Animal = cell.Animal;
                    cells[x, y].Animal.LastTick = tick;
                    cell.Animal = null;
                }
                // 2) Eat
                //cell.Animal.Food += cell.Terrain;
                // 3) Reproduce

                // 4) Die
            }
            var duration = DateTime.Now - start;
            Controller.SetStatus($"Tick {tick}: {duration.TotalMilliseconds} ms");
            tick++;
        }

        private Random rand = new Random();

        public void Start()
        {
            ProgressBar progressBar = new ProgressBar() { IsIndeterminate = true };

            Popup popup = new Popup() { Placement = PlacementMode.Center, PlacementTarget = Controller, Width = 200, Height = 80 };
            popup.Child = progressBar;
            popup.IsOpen = true;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            cells = new Cell[Width, Height];
            for (int i = 0; i < Width; i++)
            {
                Controller.WorldGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
            }
            for (int i = 0; i < Height; i++)
            {
                Controller.WorldGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(16) });
            }
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = new Cell(this, x, y);
                    cell.MouseDown += (sender, args) => { (sender as Cell).DoClick(); };
                    cell.MouseEnter += (sender, args) =>
                    {
                        if (args.LeftButton == MouseButtonState.Pressed)
                            (sender as Cell).DoClick();
                    };
                    Grid.SetRow(cell, y);
                    Grid.SetColumn(cell, x);
                    Controller.WorldGrid.Children.Add(cell);
                    cell.Terrain = new Terrain() { Kind = TerrainKind.Rock };
                    cells[x, y] = cell;
                }
            }
            Terraform();
            timer = new Timer(1000);
            timer.Elapsed += (sender, args) => { Controller.Dispatcher.Invoke(() => Tick()); };
            timer.Start();
            watch.Stop();
            popup.IsOpen = false;
            Controller.SetStatus($"Created world in {watch.ElapsedMilliseconds} ms");
        }

        private void Terraform()
        {
            // Oceans
            CoverWithTerrain(Width * 15 / 40, 2 / 3.0, .8, TerrainKind.Ocean, Angle.FromDegrees(90));
            // Lakes
            CoverWithTerrain(3, 2 / 3.0, .4, TerrainKind.Ocean, Angle.FromDegrees(75));
            // Tundra
            MakeLatitudeTerrain(Angle.FromDegrees(90), 2, 15, TerrainKind.Tundra, true);
            // Taiga
            MakeLatitudeTerrain(Angle.FromDegrees(55), 2, 8, TerrainKind.Taiga, false);
            MakeLatitudeTerrain(Angle.FromDegrees(-55), 2, 8, TerrainKind.Taiga, false);
            // Forests
            MakeLatitudeTerrain(Angle.FromDegrees(45), 2, 15, TerrainKind.Forest, false);
            MakeLatitudeTerrain(Angle.FromDegrees(-45), 2, 15, TerrainKind.Forest, false);
            // Jungle
            MakeLatitudeTerrain(Angle.FromDegrees(0), 2, 20, TerrainKind.Jungle, false);
            // Grasslands
            MakeLatitudeTerrain(Angle.FromDegrees(30), 7, 20, TerrainKind.Grass, false);
            MakeLatitudeTerrain(Angle.FromDegrees(-30), 7, 20, TerrainKind.Grass, false);
            // Deserts
            CoverWithTerrain(5, 4, .13, TerrainKind.Desert, Angle.FromDegrees(30));
            // Swamps
            CoverWithTerrain(2, 1.5, .3, TerrainKind.Swamp, Angle.FromDegrees(60));
        }

        private void MakeLatitudeTerrain(Angle latitude, double Sigma, double Coverage, TerrainKind kind, bool onWater)
        { 
            int budget = (int)(Width * Coverage);

            while (budget-- > 0)
            {
                int x = rand.Next(0, Width);
                int y = (int)(GetNormal() * Sigma + LatitudeToY(latitude));
                y = (y + Height) % Height;
                if (onWater || cells[x, y].Terrain.Kind != TerrainKind.Ocean)
                {
                    cells[x, y].Terrain = new Terrain() { Kind = kind };
                }
            }
        }

        private int LatitudeToY(Angle latitude)
        {
            return (int)(Math.Sin(latitude.Radians) * Height / 2 + Height / 2);
        }

        private void MakeLakes()
        {
        }

        private void CoverWithTerrain(int MeanRadius, double Sigma, double Coverage, TerrainKind kind, Angle maxLatitude)
        {
            int Area = Width * Height;
            int budget = (int)(Area * Coverage);
            int ymax = LatitudeToY(maxLatitude);
            int ymin = LatitudeToY(-maxLatitude);
            while (budget > 0)
            {
                int x = rand.Next(0, Width);
                int y = rand.Next(0, Height);
                y = Math.Clamp(y, ymin, ymax);
                var radius = (MeanRadius + (rand.NextDouble() - .5) * 3 * Sigma);
                budget -= SetTerrainAround(x, y, radius, kind);
            }
        }

        double GetNormal()
        {
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();
            return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        }

        private int SetTerrainAround(int x, int y, double radius, TerrainKind kind)
        {
            Debug.WriteLine($"Set {kind} around ({x}, {y}) radius={radius}");
            const double CoverPct = .6;
            int cover = (int)(Math.PI * radius * radius * CoverPct);
            int ret = cover;
            while (cover-- > 0)
            {
                int candX = (int)(x + GetNormal() * radius / 6);
                int candY = (int)(y + GetNormal() * radius / 6);
                candX = (candX + Width) % Width;
                candY = (candY + Height) % Height;
                if (cells[candX, candY].Terrain == null || cells[candX, candY].Terrain.Kind != TerrainKind.Ocean)
                {
                    cells[candX, candY].Terrain = new Terrain() { Kind = kind };
                }
            }
            return ret;
        }

        Timer timer;

        public int GetProducedEnergy()
        {
            // TODO: Calculate produced energy
            return 5;
        }

        public const int MaxEnergy = 5000;
        public int Energy
        {
            get => energy;
            set { energy = value; Controller.RaisePropertyChanged("Energy"); }
        }
    }
}
