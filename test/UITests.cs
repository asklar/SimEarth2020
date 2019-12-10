using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class UITests
    {
        private void AppThreadProc()
        {
            mutex = new Mutex(true);

            duration = perf.Profile(() =>
            {
                app = new App();
                app.InitializeComponent();
            });
            /// TODO?
            /// app.StartupUri = new Uri($"pack://application:,,,/{typeof(App).Assembly.GetName().Name};component/MainWindow.xaml", System.UriKind.Absolute);

            mutex.ReleaseMutex(); // duration is ready
            /// Application.Start((p) => { return app; });
        }

        PerfUtil perf;
        App app;
        Mutex mutex;
        double duration = 0;

        [TestInitialize]
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

        [TestCleanup]
        public void Teardown()
        {
            mutex.ReleaseMutex();
            app.Exit();
        }

        /*
        public void CreateApp()
        {
            Assert.IsTrue(duration != 0);
            Assert.IsTrue(duration < 4e8);
        }

        public void CreateWorld()
        {
            MainPage main = null;
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
                    duration = perf.Profile(() => main.StartNewGame(
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
            /// TODO
            /*
            app.Dispatcher.Invoke(() =>
            {
                stopwatch.Stop();
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            */
        /*
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500);
    }
    */
    }
}
