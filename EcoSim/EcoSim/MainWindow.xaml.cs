using GameEngine.Api;
using GameEngine.Mock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            Simple
        }

        private Ruleset _ruleset = Ruleset.Simple;
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
                Application.Current.Shutdown();
            }
            return dlg.FileName;
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
                case Ruleset.Simple:
                    
                    var settings = GameEngine.Simple.Settings.Load(PickFile("Choose settings file"));

                    var speciesList = GameEngine.Simple.AnimalSpecies.Load(PickFile("Choose species list file"));
                    _world = new GameEngine.Simple.World(new GameEngine.Simple.Entity.SimpleEntityFactory(), settings, speciesList);
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

        public void DoFastProcess()
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


        public event PropertyChangedEventHandler PropertyChanged;

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
                progress = Math.Min(progress, 1);
                Canvas.SetLeft(entity.Visual, entity.XPos(progress));
                Canvas.SetTop(entity.Visual, entity.YPos(progress));
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
