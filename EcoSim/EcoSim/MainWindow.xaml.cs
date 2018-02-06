using GameEngine.Api;
using GameEngine.Mock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EcoSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        enum Ruleset
        {
            Mock,
            Grazing
        }
        private Ruleset _ruleset = Ruleset.Grazing;

        public event PropertyChangedEventHandler PropertyChanged;

        private const int BaseWorldTick = 200;
        private int _actualWorldTick;

        private float _savedTimeMultiplier;

        private float _timeMultiplier = 1;
        public float TimeMultiplier
        {
            get => _timeMultiplier;
            set
            {
                _timeMultiplier = Math.Min(Math.Max(0, value), 10);
                if (_timeMultiplier > 0)
                {
                    OnePlus.IsEnabled = false;
                    _worldTimer.Change(0, (int)(BaseWorldTick / _timeMultiplier));
                }
                else
                {
                    OnePlus.IsEnabled = true;
                }
                _actualWorldTick = (int)(BaseWorldTick / TimeMultiplier) ;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeMultiplier)));
            }
        }

        public int EntityCount => _entitiesDictionary.Count;
        private IWorld _world;
        private Timer _worldTimer;
        private Dictionary<int,EntityWrapper> _entitiesDictionary = new Dictionary<int, EntityWrapper>();
        private Timer _visualTimer;
        private EntityWrapper _currentlySelected;

        private bool _isFastProcessing;
        private Queue<Action> _toDo = new Queue<Action>();

        private string PickFile(string title)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = title;
            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox 

            if (result != true)
            {
                return null;
            }
            return dlg.FileName;
        }

        private GameEngine.Grazing.IBrain PickBrain(GameEngine.Grazing.Settings settings)
        {
            switch(settings.Brain)
            {
                
                case "Mark0":
                    var mk0Brain = new Grazing.QLearningBrainMk0();
                    mk0Brain.CloseRange = settings.InteractionDistance - 1;
                    mk0Brain.StartRandomFactor = settings.BrainRandomFactor;
                    mk0Brain.RandomFactorMultiplier = settings.BrainRandomFactorMultiplier;
                    return mk0Brain;
                case "Mark1":
                default:
                    var mk1Brain = new Grazing.QLearningBrainMk1();
                    mk1Brain.CloseRange = settings.InteractionDistance - 1;
                    mk1Brain.StartRandomFactor = settings.BrainRandomFactor;
                    mk1Brain.RandomFactorMultiplier = settings.BrainRandomFactorMultiplier;
                    return mk1Brain;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            switch(_ruleset)
            {
                case Ruleset.Mock:
                    _world = new MockWorld(new MockEntity.MockEntityFactory());
                    break;
                case Ruleset.Grazing:
                    var grazingSettingsFile = PickFile("Choose settings file, cancel for default");
                    var grazingSettings = grazingSettingsFile!=null
                        ? GameEngine.Grazing.Settings.Load(grazingSettingsFile)
                        : GameEngine.Grazing.Settings.Default;
                    var brain = PickBrain(grazingSettings);
                    brain.Reset();
                    
                    var w = new GameEngine.Grazing.World(new GameEngine.Grazing.Entity.GrazingEntityFactory(),
                        grazingSettings, () => brain);
                    w.Log += str => Dispatcher.Invoke(()=>
                    {
                        LogBox.AppendText("\n" + str);
                        LogBox.ScrollToEnd();
                    });
                    _world = w;
                    ResetBrain.Click += (o, args) =>
                    {
                        brain.Reset();
                    };
                    ResetCounter.Click += (o, args) =>
                    {
                        w.ResetCounter();
                    };
                    Kill.Click += (o, args) =>
                    {
                        w.KillAnimal();
                    };
                    break;
            }
            _world.NewEntity += _world_NewEntity;
            _world.EntityVanished += _world_EntityVanished;
            _world.Initialize();

            _worldTimer = new Timer(state => 
            {
                if (_timeMultiplier > 0 && Monitor.TryEnter(_world))
                {
                    _world.NextFrame();
                    Monitor.Exit(_world);
                }

            }, null, 0, _actualWorldTick);

            _visualTimer = new Timer(state => Dispatcher.Invoke(UpdateVisuals), null, 0, 25);
            WorldCanvas.Height = _world.Height;
            WorldCanvas.Width = _world.Width;
            EntitiesGrid.ItemsSource = _entitiesDictionary.Values;
            TimeMultiplier = 1;

        }



        public void FastProcess(bool toggle)
        {
            if (toggle == _isFastProcessing)
                return;
            if (toggle)
            {
                _isFastProcessing = true;
                Task.Run((Action)DoFastProcess);
            }
            else
            {
                _isFastProcessing = false;
            }
        }

        private void DoFastProcess()
        {
            Dispatcher.Invoke(BeginFastProcess);

            while (_isFastProcessing)
            {
                lock (_world)
                {
                    _world.NextFrame();
                }
            }
            
            Dispatcher.Invoke(EndFastProcess);

        }

        private void BeginFastProcess()
        {
            _savedTimeMultiplier = TimeMultiplier;
            TimeMultiplier = 0;
            Pause.IsEnabled = false;
            Slower.IsEnabled = false;
            Faster.IsEnabled = false;
        }

        private void EndFastProcess()
        {
            TimeMultiplier = _savedTimeMultiplier;
            Pause.IsEnabled = true;
            Slower.IsEnabled = true;
            Faster.IsEnabled = true;
        }

        bool tick = false;

        private void UpdateVisuals()
        {
            TurnCounter.Text = _world.TurnCounter.ToString();
            if (TimeMultiplier <= 0)
                return;
            while (_toDo.Any())
            {
                _toDo.Dequeue()();
            }
            foreach (var kvp in _entitiesDictionary)
            {
                
                var entity = kvp.Value;
                var progress = ((float)(DateTime.Now - entity.LastUpdate).TotalMilliseconds) / _actualWorldTick;
                if (progress <= 1)
                {
                    Canvas.SetLeft(entity.Visual, entity.XPos(progress));
                    Canvas.SetTop(entity.Visual, entity.YPos(progress));
                }
                if (entity.BodyType == "Animal")
                {                                        
                    tick = !tick;
                    //WorldCanvas.Background = tick ? System.Windows.Media.Brushes.AliceBlue : System.Windows.Media.Brushes.Black;
                }
            }
            EntityDetails.Items.Refresh();
        }

        private void UpdateVisualsSnap()
        {
            while(_toDo.Any())
            {
                _toDo.Dequeue()();
            }
            TurnCounter.Text = _world.TurnCounter.ToString();
            foreach (var kvp in _entitiesDictionary)
            {
                var entity = kvp.Value;
                var progress = 1;
                progress = Math.Min(progress, 1);
                Canvas.SetLeft(entity.Visual, entity.XPos(progress));
                Canvas.SetTop(entity.Visual, entity.YPos(progress));
            }
            EntityDetails.Items.Refresh();
        }

        private void _world_NewEntity(IEntity e)
        {
            _toDo.Enqueue(() => AddEntity(e));
        }

        private void _world_EntityVanished(IEntity e)
        {

            _toDo.Enqueue(() => RemoveEntity(e));
        }


        private void AddEntity(IEntity e)
        {
            var wrp = new EntityWrapper(e);
            _entitiesDictionary.Add(wrp.Id, wrp);
            var entity = new Entity(wrp);
            wrp.Visual = entity;
            entity.Height = 20;
            WorldCanvas.Children.Add(entity);
            entity.MouseDown += AnyEntity_Click;
            Canvas.SetLeft(entity, wrp.XPos(1));
            Canvas.SetTop(entity, wrp.YPos(1));
            EntitiesGrid.Items.Refresh();
            EntitiesListRefocus();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EntityCount)));
        }


        private void RemoveEntity(IEntity e)
        {
            var wrp = _entitiesDictionary[e.Id];
            _entitiesDictionary.Remove(wrp.Id);

            if (_currentlySelected == wrp)
                _currentlySelected = null;
            WorldCanvas.Children.Remove(wrp.Visual);         

            EntitiesGrid.Items.Refresh();
            EntitiesListRefocus();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EntityCount)));
        }


        private void AnyEntity_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Entity entity))
                return;
            SelectEntity(entity.Model as EntityWrapper);
        }

        private void SelectEntity(EntityWrapper e)
        {
            if (_currentlySelected != null)
                _currentlySelected.Selected = false;
            _currentlySelected = e;
            if (e != null)
            {
                _currentlySelected.Selected = true;
            }    
            EntityDetails.ItemsSource = e?.Variables;
            EntitiesListRefocus();
        }

        private void EntitiesListRefocus()
        {

            EntitiesGrid.SelectedItem = _currentlySelected;
            if (_currentlySelected == null)
                return;
            EntitiesGrid.ScrollIntoView(_currentlySelected);
        }

        private void TimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement b))
                return;

            switch (b.Uid)
            {
                case "Slower":
                    TimeMultiplier -= 0.2f;
                    break;
                case "Faster":
                    TimeMultiplier += 0.2f;
                    break;
                case "Pause":
                    TimeMultiplier = 0;
                    break;
                case "BackgroundToggle":
                    FastProcess(BackgroundToggle.IsChecked == true);
                    break;
                case "OnePlus":
                    if (!_isFastProcessing && _timeMultiplier == 0 && Monitor.TryEnter(_world))
                    {
                        _world.NextFrame();
                        Monitor.Exit(_world);
                        UpdateVisualsSnap();
                    }
                    break;
            }
        }

        private void Entities_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectEntity(e.AddedCells.FirstOrDefault().Item as EntityWrapper);
        }
    }
}
