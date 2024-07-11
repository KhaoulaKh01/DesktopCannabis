using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;



namespace projetCannabis
{
    public class Plantule : INotifyPropertyChanged
    {
        // Déclaration de l'événement PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private string _identification;
        public string Identification
        {
            get { return _identification; }
            set
            {
                if (_identification != value)
                {
                    _identification = value;
                    OnPropertyChanged(nameof(Identification));
                }
            }
        }

        private BitmapImage _qrCode;
        public BitmapImage QRCode
        {
            get { return _qrCode; }
            set
            {
                if (_qrCode != value)
                {
                    _qrCode = value;
                    OnPropertyChanged(nameof(QRCode));
                }
            }
        }


        // Autres propriétés et constructeurs comme précédemment

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

        public string EtatSante { get; set; }
        public DateTime? DateEtatSante { get; set; }
        public int? IdQR { get; set; }
        public DateTime? DateReception { get; set; }

        public string Provenance { get; set; }
        public string Description { get; set; }
        public string StadeDeVie { get; set; }
        public DateTime? DateRetrait { get; set; }
        public string Entreposage { get; set; }
        public int? IdEntreposage { get; set; }
        public int? Actif_Inactif { get; set; }
        public int? QteAjoutee { get; set; }
        public int? QteRetiree { get; set; }
        public string ItemRetireInventaire { get; set; }
        public string ResponsableDecontamination { get; set; }
        public string Note { get; set; }
        public int NombreDeModification { get; set; }
        public DateTime DerniereModification { get; set; }

        public bool EstArchive { get; set; }

        public Bitmap CodeQR { get; set; }

        public string QRCodeFilePath { get; set; } // Chemin du fichier du code QR




        public Plantule() { }

        public Plantule(string etatDeSante, DateTime? dateReception, string identification, string provenance, string description, string stadeDeVie, string entreposage, int? qteAjoutee, int? qteRetiree, string itemRetireInventaire, string responsableDecontamination, string note, DateTime? dateEtatSante, int? idQR, DateTime? dateRetrait, int? idEntreposage, int? actif_inactif)
        {
            EtatSante = etatDeSante;
            DateReception = dateReception;
            Identification = identification;
            Provenance = provenance;
            Description = description;
            StadeDeVie = stadeDeVie;
            Entreposage = entreposage;
            QteAjoutee = qteAjoutee;
            QteRetiree = qteRetiree;
            ItemRetireInventaire = itemRetireInventaire;
            ResponsableDecontamination = responsableDecontamination;
            Note = note;
            DateEtatSante = dateEtatSante;
            IdQR = idQR;
            DateRetrait = dateRetrait;
            IdEntreposage = idEntreposage;
            Actif_Inactif = actif_inactif;
        }


        public class PlantuleViewModel
        {
            public ObservableCollection<Plantule> Plantules { get; set; }

            public PlantuleViewModel()
            {
                Plantules = new ObservableCollection<Plantule>
        {
            //new Plantule { Identification = "P001", Description = "Plantule 1", DateReception = DateTime.Now },
             //new Plantule { Identification = "P002", Description = "Plantule 2", DateReception = DateTime.Now }
        };

                foreach (var plantule in Plantules)
                {
                    plantule.QRCode = GenerateQRCode(plantule.Identification);
                }
            }

            public BitmapImage GenerateQRCode(string content)
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                {
                    MemoryStream ms = new MemoryStream();
                    qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
        }


       


    }



    




}



