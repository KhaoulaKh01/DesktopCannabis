using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static projetCannabis.MainWindow;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageGererPlantule.xaml
    /// </summary>
    public partial class PageGererPlantule : Page, INotifyPropertyChanged
    {
        private Plantule _plantule;

        public Plantule Plantule
        {
            get { return _plantule; }
            set
            {
                if (_plantule != value)
                {
                    _plantule = value;
                    OnPropertyChanged(nameof(Plantule));
                }
            }
        }

        public PageGererPlantule()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ReceivePlantule(Plantule plantule)
        {
            this.Plantule = plantule;
        }
    }


   
    public class PageGererPlantuleViewModel
    {
        //public ObservableCollection<EtatSante> Etats { get; set; }

       /* public PageGererPlantuleViewModel()
        {
            Etats = new ObservableCollection<EtatSante>
            {
                new EtatSante { Couleur = "Green", Description = "Bon" },
                new EtatSante { Couleur = "Yellow", Description = "Moyen" },
                new EtatSante { Couleur = "Brown", Description = "Mauvais" },
                new EtatSante { Couleur = "Red", Description = "Critique" }
            };
        }*/
    }
}
