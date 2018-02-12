using GameEngine.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    /// Interaction logic for Entity.xaml
    /// </summary>
    public partial class Entity : UserControl
    {
        public EntityWrapper Model { get; }
        public Entity(EntityWrapper e)
        {
            InitializeComponent();
            Model = e;
            DataContext = e;
        }
    }

    public class BodyTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            string bodyTypeStr = item as string;

            if (bodyTypeStr != null)
            {
                if (bodyTypeStr == "Animal")
                {
                    return (container as FrameworkElement).FindResource("AnimalTemplate") as DataTemplate;
                }
                else
                {
                    return (container as FrameworkElement).FindResource("PlantTemplate") as DataTemplate;

                }
            }

            return null;
        }
    }

    public class EntityWrapper : INotifyPropertyChanged
    {
        public string Name => _inner.Name;
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }

        public DateTime LastUpdate { get; private set; }

        private IEntity _inner;

        public EntityWrapper(IEntity entity)
        {
            _inner = entity;
            _inner.UpdatePosition += OnUpdatePosition;
        }

        public string BodyType => _inner.BodyType;

        private float _lastX, _lastY, _currentX, _currentY;

        public event PropertyChangedEventHandler PropertyChanged;

        public float XPos(float progress) => _lastX * (1-progress) + _inner.XPos * progress;

        public float YPos(float progress) => _lastY * (1-progress) + _inner.YPos * progress;

        public int Id => _inner.Id;

        public Entity Visual { get; internal set; }

        public IList<EntityVariable> Variables => _inner.Variables;


        public void OnUpdatePosition()
        {
            _lastX = _currentX;
            _lastY = _currentY;
            _currentX = _inner.XPos;
            _currentY = _inner.YPos;
            LastUpdate = DateTime.Now;

        }        
    }
}
