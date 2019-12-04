using NUnit.Framework;
using SimEarth2020;
using System;
using System.Diagnostics;
using System.Threading;

namespace SimEarthTests
{
    public class UITests
    {
        private void AppThreadProc()
        {
            mutex = new Mutex(true);

            duration = perf.Profile(() => { app = new App(); app.InitializeComponent(); });
            app.StartupUri = new Uri($"pack://application:,,,/{typeof(App).Assembly.GetName().Name};component/MainWindow.xaml", System.UriKind.Absolute);

            mutex.ReleaseMutex(); // duration is ready
            app.Run();
        }

        PerfUtil perf;
        App app;
        Mutex mutex;
        double duration = 0;

        [OneTimeSetUp]
        public void Setup()
        {
            perf = new PerfUtil();
            var thread = new Thread(AppThreadProc);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "App Thread";
            thread.Start();
            while (mutex == null) Thread.Yield();

            mutex.WaitOne();
            Assert.IsNotNull(app);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            mutex.ReleaseMutex();
            app.Dispatcher.Invoke(() => app.Shutdown());
        }

        [Test, Order(1)]
        public void CreateApp()
        {
            Assert.IsTrue(duration != 0);
            Assert.IsTrue(duration < 4e8);
        }

        [Test, Order(2)]
        public void CreateWorld()
        {
            MainWindow main = null;
            double show = double.MaxValue;
            app.Dispatcher.Invoke(
                () =>
                {
                    main = app.MainWindow as MainWindow;
                    show = perf.Profile(() => main.Show());
                });

            Assert.IsTrue(show < 4e8);
            app.Dispatcher.Invoke(
                () =>
                {
                    duration = perf.Profile(() => main.NewGame(
                        new Environment.World(main)
                        {
                            Radius = 1,
                            Age = 0,
                            Name = "Test World",
                            Width = 100
                        }));
                }
                );
            Assert.IsTrue(duration < 5e9);

            Stopwatch stopwatch = Stopwatch.StartNew();
            app.Dispatcher.Invoke(() =>
            {
                stopwatch.Stop();
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500);
        }
    }
}
